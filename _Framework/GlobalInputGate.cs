using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

[DisallowMultipleComponent]
public class GlobalInputGate : SingletonPersistent<GlobalInputGate>
{
#if ENABLE_INPUT_SYSTEM
    [Header("（任意）新InputSystemの全体アクションアセット")]
    [SerializeField] private InputActionAsset globalActions;
#endif
    [Header("ブロッカーCanvasのSortingOrder（より大きいほど前面）")]
    [SerializeField] private int blockerSortingOrder = 32760;

    // 内部生成物（インスペクター不要）
    private Canvas _overlayCanvas;          // 専用オーバーレイ（自動生成・最前面）
    private GameObject _uiBlocker;          // 全画面透明Image（UIレイキャスト吸収）
    private EventSystem _eventSystem;       // current優先。無ければ自前生成
    private StandaloneInputModule _legacyUIModule;
#if ENABLE_INPUT_SYSTEM
    private InputSystemUIInputModule _newUIModule;
#endif
    private int _lockCount = 0;

    protected override void Awake()
    {
        base.Awake(); // SingletonPersistent: 重複自壊＆DontDestroyOnLoad

        // --- EventSystem（current優先。無ければ自前生成） ---
        _eventSystem = EventSystem.current;
        if (_eventSystem == null)
        {
            var esGO = new GameObject("EventSystem (GlobalInputGate)");
            _eventSystem = esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            _newUIModule = esGO.AddComponent<InputSystemUIInputModule>();
#else
            _legacyUIModule = esGO.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(esGO);
        }
        else
        {
            _legacyUIModule = _eventSystem.GetComponent<StandaloneInputModule>();
#if ENABLE_INPUT_SYSTEM
            _newUIModule = _eventSystem.GetComponent<InputSystemUIInputModule>();
#endif
        }

        // --- 専用オーバーレイCanvasを自動生成 ---
        var canvasGO = new GameObject("GlobalInputGate_OverlayCanvas");
        _overlayCanvas = canvasGO.AddComponent<Canvas>();
        _overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _overlayCanvas.sortingOrder = blockerSortingOrder - 1;   // ブロッカー本体はさらに上に出す
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);
    }

    /// <summary>すべての操作を完全停止（UIレイキャスト含む）</summary>
    public void StopAllInput()
    {
        _lockCount++;
        if (_lockCount > 1) return;

        EnsureUiBlocker();          // ブロッカー生成（初回のみ）
        _uiBlocker.SetActive(true); // UIのクリック/ドラッグ/長押し等を全吸収

#if ENABLE_INPUT_SYSTEM
        try { globalActions?.Disable(); } catch { }
#endif
        if (_eventSystem != null) _eventSystem.sendNavigationEvents = false;
        if (_legacyUIModule != null) _legacyUIModule.enabled = false;
#if ENABLE_INPUT_SYSTEM
        if (_newUIModule != null) _newUIModule.enabled = false;
#endif
    }

    /// <summary>停止していた操作を再開</summary>
    public void RestartAllInput()
    {
        if (_lockCount <= 0) { _lockCount = 0; return; }
        _lockCount--;
        if (_lockCount > 0) return;

        if (_uiBlocker != null) _uiBlocker.SetActive(false);

#if ENABLE_INPUT_SYSTEM
        try { globalActions?.Enable(); } catch { }
#endif
        if (_eventSystem != null) _eventSystem.sendNavigationEvents = true;
        if (_legacyUIModule != null) _legacyUIModule.enabled = true;
#if ENABLE_INPUT_SYSTEM
        if (_newUIModule != null) _newUIModule.enabled = true;
#endif
    }

    // ---- 内部：全画面ブロッカー生成（専用オーバーレイ配下に作る） ----
    private void EnsureUiBlocker()
    {
        if (_uiBlocker != null) return;

        var go = new GameObject("UIInputBlocker");
        go.transform.SetParent(_overlayCanvas.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0f);   // 完全透明
        img.raycastTarget = true;            // UIレイキャストを全吸収

        var canvas = go.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = blockerSortingOrder; // 最前面

        go.AddComponent<GraphicRaycaster>();

        _uiBlocker = go;
        _uiBlocker.SetActive(false);
        //DontDestroyOnLoad(go);
    }

    public void SceneChangeMeshod()//シーンチェンジ時に呼ぶ
    {
        AddressableMediaHub.Instance.ReleaseAll();
    }
}
