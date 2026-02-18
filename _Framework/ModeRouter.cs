// ModeRouter.cs
// - GlobalInputGate.StopAllInput()〜RestartAllInput() で切替中は完全ロック
// - Exit→Enter：Inspector UnityEvent（void）→ Inspector登録Coroutine（全て待つ）→
//                 コード登録Action（void）→ コード登録Coroutine（全て待つ）
// - 「待たない」オプションは廃止。コルーチンを登録したら必ず完了まで待機
// - 連打安全：最新遷移に乗り換え。ロックは最初の1回のみ取得し、最後にだけ解除
// - OnModeChanged は Enter 側の全完了後に (prev, next) を発火

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public enum GameMode { Exploration, Combat, Map, Formation, Ev }

[Serializable]
public class InspectorCoroutineCall
{
    [Tooltip("引数なし IEnumerator メソッドを持つ対象コンポーネント")]
    public MonoBehaviour target;

    [Tooltip("メソッド名（例: PlayFadeIn）※引数なし／IEnumerator を返すこと")]
    public string methodName;
}

[Serializable]
public class ModeHook
{
    public GameMode mode;

    [Header("UnityEvent（voidメソッド）")]
    [Tooltip("Enter直後に呼ばれる（待機なし）")]
    public UnityEvent onEnter;

    [Tooltip("Exit直前に呼ばれる（待機なし）")]
    public UnityEvent onExit;

    [Header("Inspector 登録コルーチン（すべて完了まで待機）")]
    public List<InspectorCoroutineCall> enterCoroutines = new();
    public List<InspectorCoroutineCall> exitCoroutines = new();
}

[DisallowMultipleComponent]
public class ModeRouter : SceneSingleton<ModeRouter>
{
    [Header("起動時のモード")]
    [SerializeField] private GameMode initialMode = GameMode.Exploration;

    [Header("モードごとの実行（Inspector割り当て）")]
    [SerializeField] private List<ModeHook> hooks = new();

    /// <summary>現在のモード</summary>
    public GameMode CurrentMode { get; private set; }

    /// <summary>Enter側の全処理完了後に (prev, next) を通知</summary>
    public event Action<GameMode, GameMode> OnModeChanged;

    // --- コード登録（Action / Coroutine はすべて待つ） ---
    private readonly Dictionary<GameMode, List<Action>> _enterActions = new();
    private readonly Dictionary<GameMode, List<Action>> _exitActions = new();
    private readonly Dictionary<GameMode, List<Func<IEnumerator>>> _enterCoroutines = new();
    private readonly Dictionary<GameMode, List<Func<IEnumerator>>> _exitCoroutines = new();

    public void RegisterEnterAction(GameMode mode, Action action)
    {
        if (action == null) return;
        if (!_enterActions.TryGetValue(mode, out var list)) { list = new List<Action>(); _enterActions[mode] = list; }
        list.Add(action);
    }
    public void RegisterExitAction(GameMode mode, Action action)
    {
        if (action == null) return;
        if (!_exitActions.TryGetValue(mode, out var list)) { list = new List<Action>(); _exitActions[mode] = list; }
        list.Add(action);
    }
    public void RegisterEnterCoroutine(GameMode mode, Func<IEnumerator> factory)
    {
        if (factory == null) return;
        if (!_enterCoroutines.TryGetValue(mode, out var list)) { list = new List<Func<IEnumerator>>(); _enterCoroutines[mode] = list; }
        list.Add(factory);
    }
    public void RegisterExitCoroutine(GameMode mode, Func<IEnumerator> factory)
    {
        if (factory == null) return;
        if (!_exitCoroutines.TryGetValue(mode, out var list)) { list = new List<Func<IEnumerator>>(); _exitCoroutines[mode] = list; }
        list.Add(factory);
    }
    public void ClearRegistrations(GameMode mode)
    {
        _enterActions.Remove(mode);
        _exitActions.Remove(mode);
        _enterCoroutines.Remove(mode);
        _exitCoroutines.Remove(mode);
    }

    // --- 内部状態 ---
    private Coroutine _transitionCo;
    private int _serial = 0;
    private bool _hasInputLocked = false;

    private void Start()
    {
        SwitchMode(initialMode, force: true);
    }

    /// <summary>モード切替（全処理待機）</summary>
    public void SwitchMode(GameMode next, bool force = false)
    {
        if (!force && next == CurrentMode) return;

        if (_transitionCo != null)
        {
            StopCoroutine(_transitionCo); // 旧遷移は破棄（ロックは保持）
            _transitionCo = null;
        }
        _transitionCo = StartCoroutine(RunTransition(next));
    }

    private IEnumerator RunTransition(GameMode next)
    {
        int my = ++_serial;
        var prev = CurrentMode;

        // --- 切替中は全入力ロック（多重ロック防止フラグ） ---
        if (!_hasInputLocked)
        {
            GlobalInputGate.Instance?.StopAllInput();
            _hasInputLocked = true;
        }

        // ----- Exit(prev) -----
        if (prev != next)
        {
            // 1) UnityEvent (void)
            InvokeInspectorExit(prev);

            // 2) Inspector コルーチン（すべて待つ）
            yield return RunInspectorCoroutines(GetHook(prev)?.exitCoroutines);

            // 3) コード登録 Action（void）
            InvokeActionList(_exitActions, prev);

            // 4) コード登録 Coroutine（すべて待つ）
            yield return RunCoroutineList(_exitCoroutines, prev);
        }

        if (my != _serial) yield break; // 乗り換え

        // モード確定
        CurrentMode = next;

        // ----- Enter(next) -----
        // 1) UnityEvent (void)
        InvokeInspectorEnter(next);

        // 2) Inspector コルーチン（すべて待つ）
        yield return RunInspectorCoroutines(GetHook(next)?.enterCoroutines);

        // 3) コード登録 Action（void）
        InvokeActionList(_enterActions, next);

        // 4) コード登録 Coroutine（すべて待つ）
        yield return RunCoroutineList(_enterCoroutines, next);

        if (my != _serial) yield break; // 乗り換え

        // --- 全て完了 → 入力再開 & 通知 ---
        if (_hasInputLocked)
        {
            GlobalInputGate.Instance?.RestartAllInput();
            _hasInputLocked = false;
        }
        OnModeChanged?.Invoke(prev, next);

        if (_transitionCo != null && my == _serial) _transitionCo = null;
    }

    // ===== 実行ヘルパ =====

    private ModeHook GetHook(GameMode mode)
    {
        for (int i = 0; i < hooks.Count; i++)
            if (hooks[i].mode.Equals(mode)) return hooks[i];
        return null;
    }

    private void InvokeInspectorEnter(GameMode mode)
    {
        var hook = GetHook(mode);
        if (hook?.onEnter == null) return;
        try { hook.onEnter.Invoke(); } catch (Exception e) { Debug.LogException(e); }
    }

    private void InvokeInspectorExit(GameMode mode)
    {
        var hook = GetHook(mode);
        if (hook?.onExit == null) return;
        try { hook.onExit.Invoke(); } catch (Exception e) { Debug.LogException(e); }
    }

    private void InvokeActionList(Dictionary<GameMode, List<Action>> dict, GameMode mode)
    {
        if (!dict.TryGetValue(mode, out var list) || list == null || list.Count == 0) return;
        for (int i = 0; i < list.Count; i++)
        {
            try { list[i]?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
        }
    }

    private IEnumerator RunCoroutineList(Dictionary<GameMode, List<Func<IEnumerator>>> dict, GameMode mode)
    {
        if (!dict.TryGetValue(mode, out var list) || list == null || list.Count == 0) yield break;

        for (int i = 0; i < list.Count; i++)
        {
            IEnumerator ie = null;
            try { ie = list[i]?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
            if (ie != null) yield return StartCoroutine(ie); // 完了まで待機
        }
    }

    private IEnumerator RunInspectorCoroutines(List<InspectorCoroutineCall> list)
    {
        if (list == null || list.Count == 0) yield break;

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item == null || item.target == null || string.IsNullOrEmpty(item.methodName))
                continue;

            IEnumerator ie = null;
            try
            {
                var t = item.target.GetType();
                var mi = t.GetMethod(item.methodName,
                                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi == null)
                {
                    Debug.LogWarning($"[ModeRouter] Method not found: {t.Name}.{item.methodName}()");
                    continue;
                }

                var ret = mi.Invoke(item.target, null);
                ie = ret as IEnumerator;
                if (ie == null)
                {
                    Debug.LogWarning($"[ModeRouter] {t.Name}.{item.methodName}() did not return IEnumerator.");
                    continue;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (ie != null)
            {
                // すべて待つ（仕様）
                yield return StartCoroutine(ie);
            }
        }
    }
}
