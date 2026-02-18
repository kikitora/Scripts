using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UISlotKind { CommentWindow, ImageWindow }

public class CommentUI : MonoBehaviour
{
    private enum State { Hidden, Typing, Waiting, Choosing }

    // 早送り
    private const float FAST_FORWARD_MULTIPLIER = 8f;
    private const bool AUTO_ADVANCE_WHILE_FF = true;
    private const float AUTO_ADVANCE_DELAY_FF = 0f;
    private const bool STOP_FF_CONSUMES_TAP = true;

    [Header("Roots & Buttons")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button tapAnywhereButton;
    [SerializeField] private List<Button> skipButtons = new();

    [SerializeField] private List<UISlot> slots = new();

    [Header("Choices")]
    [SerializeField] private GameObject choiceRootPanel;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Same Window Close→Open")]
    [SerializeField] private bool closeOpenOnSameCommentWindow = true;
    [SerializeField] private float closeOpenGapSec = 0.02f;

    [Serializable]
    public class NamedSequence { public string key; public CommentSequence sequence; }

    [SerializeField] private List<NamedSequence> namedSequences = new();

    [Serializable]
    public class UISlot
    {
        public string name;
        public UISlotKind kind = UISlotKind.CommentWindow;

        // 共通
        public GameObject parent;

        // CommentWindow
        public TMP_Text headerText;
        public TMP_Text bodyText;

        // ImageWindow
        public Image image;
    }

    // ====== 既定値（SetCommentDefaults で更新／Comment で利用） ======
    private struct CommentDefaults
    {
        public string slot, header, linkedSlot;
        public Sprite linkedSprite;
        public void Clear() { slot = header = linkedSlot = ""; linkedSprite = null; }
    }
    private CommentDefaults _defaults;

    // ====== イベント ======
    public event Action OnOpened;
    public event Action OnClosed;
    public event Action<int, StepType> OnStep;

    // ====== 内部状態 ======
    private int _index;
    private State _state = State.Hidden;
    private Coroutine _typeCo;
    private bool _isFast;

    private bool _autoMode;
    private float _autoDelay;
    private float _perStepAutoCloseSec; // <0: 無効
    private float _typingCps;
    private Action<int> _onChoiceSelected;

    private int _ephemeralImageSlot = -1; // コメントに紐づく一時画像
    private Coroutine[] _slotAutoClose;

    private readonly List<Button> _spawnedChoiceButtons = new();
    private readonly Stack<Button> _choicePool = new();

    private CommentSequence _currentSeq;
    private Dictionary<string, CommentSequence> _seqMap;

    private int _lastCommentSlot = -1;

    public bool IsOpen => panelRoot && panelRoot.activeSelf;
    public int LastChoiceValue { get; private set; }

    // ====== Public API ======
    public IEnumerator Play(CommentSequence seq = null, bool auto = false, float autoDelay = 0.4f,
                            float perStepAutoCloseSec = -1f, float typingCps = 30f, Action<int> onChoiceSelected = null)
    {
        if (IsOpen) Hide();
        Show(seq, auto, autoDelay, perStepAutoCloseSec, typingCps, onChoiceSelected);
        yield return new WaitUntil(() => !IsOpen);
    }

    public void Show(CommentSequence seq = null, bool auto = false, float autoDelay = 0.4f,
                     float perStepAutoCloseSec = -1f, float typingCps = 30f, Action<int> onChoiceSelected = null)
    {
        _currentSeq = seq;
        if (_currentSeq == null || _currentSeq.steps == null || _currentSeq.steps.Count == 0) return;

        _index = 0;
        _isFast = false;
        _autoMode = auto;
        _autoDelay = Mathf.Max(0f, autoDelay);
        _perStepAutoCloseSec = perStepAutoCloseSec;
        _typingCps = Mathf.Max(1f, typingCps);
        _onChoiceSelected = onChoiceSelected;
        _lastCommentSlot = -1;
        _defaults.Clear();

        EnsureSlotArrays();

        if (panelRoot) panelRoot.SetActive(true);
        OnOpened?.Invoke();

        if (tapAnywhereButton)
        {
            tapAnywhereButton.onClick.RemoveAllListeners();
            tapAnywhereButton.onClick.AddListener(OnTapAnywhere);
        }

        if (skipButtons != null)
        {
            foreach (var b in skipButtons)
            {
                if (!b) continue;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(ToggleFastForward);
                b.interactable = true;
                var cg = b.GetComponentInParent<CanvasGroup>();
                if (cg) cg.blocksRaycasts = true;
            }
        }

        StopAllCoroutines();
        StartCoroutine(MainLoop());
    }

    public IEnumerator PlayByKey(string key, bool auto = false, float autoDelay = 0.4f,
                                 float perStepAutoCloseSec = -1f, float typingCps = 30f, Action<int> onChoiceSelected = null)
    {
        if (!TryGetSequenceByKey(key, out var seq))
        {
            Debug.LogWarning($"[CommentUI] PlayByKey: key '{key}' not found.");
            yield break;
        }
        yield return Play(seq, auto, autoDelay, perStepAutoCloseSec, typingCps, onChoiceSelected);
    }

    public void ShowByKey(string key, bool auto = false, float autoDelay = 0.4f,
                          float perStepAutoCloseSec = -1f, float typingCps = 30f, Action<int> onChoiceSelected = null)
    {
        if (!TryGetSequenceByKey(key, out var seq))
        {
            Debug.LogWarning($"[CommentUI] ShowByKey: key '{key}' not found.");
            return;
        }
        Show(seq, auto, autoDelay, perStepAutoCloseSec, typingCps, onChoiceSelected);
    }

    [Obsolete("Use PlayByKey or Play(CommentSequence) instead.")]
    public IEnumerator PlayByIndex(int index, bool auto = false, float autoDelay = 0.4f,
                                   float perStepAutoCloseSec = -1f, float typingCps = 30f, Action<int> onChoiceSelected = null)
    {
        if (namedSequences == null || index < 0 || index >= namedSequences.Count || namedSequences[index]?.sequence == null)
        {
            Debug.LogWarning($"[CommentUI] PlayByIndex: invalid index {index}");
            yield break;
        }
        yield return Play(namedSequences[index].sequence, auto, autoDelay, perStepAutoCloseSec, typingCps, onChoiceSelected);
    }

    public void Hide()
    {
        StopAllCoroutines();
        ClearTyping();
        ClearChoices();
        DeactivateAllCommentWindows();

        if (_ephemeralImageSlot >= 0) ForceHideImageSlot(_ephemeralImageSlot);
        _ephemeralImageSlot = -1;

        if (panelRoot) panelRoot.SetActive(false);
        _state = State.Hidden;
        OnClosed?.Invoke();
    }

    // ====== 入力 ======
    private void OnTapAnywhere()
    {
        if (_state == State.Typing) { _isFast = true; return; }
        if (_state == State.Waiting) { _state = State.Hidden; return; }
    }

    private void ToggleFastForward()
    {
        var wasFast = _isFast;
        _isFast = !wasFast;

        if (_state == State.Waiting && _isFast && AUTO_ADVANCE_WHILE_FF)
            _state = State.Hidden;

        if (wasFast && !_isFast && STOP_FF_CONSUMES_TAP)
        {
            // 必要ならここで1フレーム入力を吸収
        }
    }

    // ====== 本流 ======
    private IEnumerator MainLoop()
    {
        _state = State.Hidden;

        while (InRangeStep())
        {
            var step = _currentSeq.steps[_index];
            OnStep?.Invoke(_index, step.type);

            switch (step.type)
            {
                case StepType.SetCommentDefaults:
                    // 入力値をそのまま既定値に（空ならクリア）
                    _defaults.slot = step.targetSlotName;
                    _defaults.header = step.header;
                    _defaults.linkedSlot = step.linkedImageSlotName;
                    _defaults.linkedSprite = step.linkedImageSprite;
                    _index++;
                    break;

                case StepType.Comment:
                    {
                        if (_ephemeralImageSlot >= 0) { ForceHideImageSlot(_ephemeralImageSlot); _ephemeralImageSlot = -1; }

                        // 未指定は既定値を使用
                        string slotName = !string.IsNullOrEmpty(step.targetSlotName) ? step.targetSlotName : _defaults.slot;
                        string headerEff = !string.IsNullOrEmpty(step.header) ? step.header : _defaults.header;
                        string linkSlot = !string.IsNullOrEmpty(step.linkedImageSlotName) ? step.linkedImageSlotName : _defaults.linkedSlot;
                        Sprite linkSprite = step.linkedImageSprite ? step.linkedImageSprite : _defaults.linkedSprite;

                        int cslot = FindSlotIndex(slotName, UISlotKind.CommentWindow);
                        if (cslot < 0)
                        {
                            Debug.LogWarning("[CommentUI] Comment step skipped: target comment slot is not set.");
                            _index++;
                            break;
                        }

                        if (closeOpenOnSameCommentWindow && _lastCommentSlot == cslot)
                            yield return CloseOpenCommentWindow(cslot, closeOpenGapSec);

                        ShowComment(cslot, headerEff, step.text);

                        if (!string.IsNullOrEmpty(linkSlot) && linkSprite)
                        {
                            int islot = FindSlotIndex(linkSlot, UISlotKind.ImageWindow);
                            if (islot >= 0) { ShowImageOnSlot(islot, linkSprite); _ephemeralImageSlot = islot; }
                        }

                        yield return TypeThenWait(cslot);
                        _lastCommentSlot = cslot;
                        _index++;
                    }
                    break;

                case StepType.SetAndComment:
                    {
                        if (_ephemeralImageSlot >= 0) { ForceHideImageSlot(_ephemeralImageSlot); _ephemeralImageSlot = -1; }

                        // 既定値は使わない。空は空のまま適用。
                        int cslot = FindSlotIndex(step.targetSlotName, UISlotKind.CommentWindow);
                        if (cslot < 0)
                        {
                            Debug.LogWarning("[CommentUI] SetAndComment step skipped: target comment slot is not set.");
                            _index++;
                            break;
                        }

                        if (closeOpenOnSameCommentWindow && _lastCommentSlot == cslot)
                            yield return CloseOpenCommentWindow(cslot, closeOpenGapSec);

                        ShowComment(cslot, step.header, step.text);

                        if (!string.IsNullOrEmpty(step.linkedImageSlotName) && step.linkedImageSprite)
                        {
                            int islot = FindSlotIndex(step.linkedImageSlotName, UISlotKind.ImageWindow);
                            if (islot >= 0) { ShowImageOnSlot(islot, step.linkedImageSprite); _ephemeralImageSlot = islot; }
                        }

                        yield return TypeThenWait(cslot);
                        _lastCommentSlot = cslot;
                        _index++;
                    }
                    break;

                case StepType.ShowImage:
                    {
                        int islot = FindSlotIndex(step.targetSlotName, UISlotKind.ImageWindow);
                        if (islot >= 0) ShowImageOnSlot(islot, step.image);
                        _index++;
                    }
                    break;

                case StepType.HideImage:
                    {
                        int islot = FindSlotIndex(step.targetSlotName, UISlotKind.ImageWindow);
                        if (islot >= 0) ForceHideImageSlot(islot);
                        _index++;
                    }
                    break;

                case StepType.ShowChoices:
                    {
                        int cslot = FindSlotIndex(step.targetSlotName, UISlotKind.CommentWindow);
                        if (cslot >= 0)
                        {
                            yield return ShowChoicesAndWait(step.choices);
                            _lastCommentSlot = cslot;
                        }
                        _index++;
                    }
                    break;
            }
        }

        Hide();
    }

    // ====== 補助 ======
    private IEnumerator CloseOpenCommentWindow(int slot, float gapSec)
    {
        var s = slots[slot];
        if (s != null && s.parent)
        {
            s.parent.SetActive(false);
            if (gapSec > 0f) yield return new WaitForSeconds(gapSec);
            s.parent.SetActive(true);
        }
        else yield return null;
    }

    private IEnumerator TypeThenWait(int cslot)
    {
        _state = State.Typing;

        var s = slots[cslot];
        if (s.bodyText)
        {
            string full = s.bodyText.text;
            s.bodyText.maxVisibleCharacters = 0;

            float cpsBase = Mathf.Max(1f, _typingCps);
            ClearTyping();
            _typeCo = StartCoroutine(TypeRoutine(s.bodyText, full, cpsBase));
            yield return _typeCo;
        }

        _state = State.Waiting;

        float wait = _isFast && AUTO_ADVANCE_WHILE_FF ? AUTO_ADVANCE_DELAY_FF : _autoDelay;
        if (_autoMode || (_isFast && AUTO_ADVANCE_WHILE_FF))
        {
            if (wait > 0f) yield return new WaitForSeconds(wait);
            _index++;
        }
        else
        {
            while (_state == State.Waiting) yield return null;
        }

        StartSlotAutoClose(cslot, _perStepAutoCloseSec);
    }

    private IEnumerator TypeRoutine(TMP_Text text, string full, float cpsBase)
    {
        float t = 0f;
        int total = full.Length;
        while (t < total)
        {
            float cpsNow = Mathf.Max(1f, cpsBase * (_isFast ? FAST_FORWARD_MULTIPLIER : 1f));
            text.maxVisibleCharacters = Mathf.Clamp(Mathf.FloorToInt(t), 0, total);
            t += cpsNow * Time.deltaTime;
            yield return null;
        }
        text.maxVisibleCharacters = total;
    }

    private IEnumerator ShowChoicesAndWait(List<CommentSequence.ChoiceItem> choices)
    {
        _state = State.Choosing;

        ClearChoices();
        choiceRootPanel?.SetActive(true);

        if (choices != null)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                var item = choices[i];
                var btn = SpawnChoiceButton();
                var t = btn.GetComponentInChildren<TMP_Text>();
                if (t) t.text = item.label;
                int val = item.value;
                btn.onClick.AddListener(() =>
                {
                    LastChoiceValue = val;
                    _onChoiceSelected?.Invoke(val);
                    ClearChoices();
                    _state = State.Waiting;
                });
            }
        }

        while (_state == State.Choosing) yield return null;
    }

    private void ShowComment(int slot, string header, string body)
    {
        var s = slots[slot];
        if (s.parent) s.parent.SetActive(true);
        if (s.headerText) s.headerText.text = header ?? "";
        if (s.bodyText) s.bodyText.text = body ?? "";
    }

    private void ShowImageOnSlot(int slot, Sprite sprite)
    {
        var s = slots[slot];
        if (s.image) s.image.sprite = sprite;
        if (s.parent) s.parent.SetActive(true);
    }

    private void ForceHideImageSlot(int slot)
    {
        var s = slots[slot];
        if (s.parent) s.parent.SetActive(false);
        if (s.image) s.image.sprite = null;
    }

    private void DeactivateAllCommentWindows()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].kind == UISlotKind.CommentWindow && slots[i].parent)
                slots[i].parent.SetActive(false);
        }
    }

    private void ClearTyping()
    {
        if (_typeCo != null) StopCoroutine(_typeCo);
        _typeCo = null;
    }

    private void ClearChoices()
    {
        if (choiceRootPanel) choiceRootPanel.SetActive(false);

        foreach (var b in _spawnedChoiceButtons)
        {
            if (!b) continue;
            b.onClick.RemoveAllListeners();
            b.gameObject.SetActive(false);
            _choicePool.Push(b);
        }
        _spawnedChoiceButtons.Clear();
    }

    private Button SpawnChoiceButton()
    {
        Button btn = null;
        if (_choicePool.Count > 0)
        {
            btn = _choicePool.Pop();
            btn.gameObject.SetActive(true);
        }
        else
        {
            btn = Instantiate(choiceButtonPrefab, choiceRootPanel ? choiceRootPanel.transform : transform);
        }
        _spawnedChoiceButtons.Add(btn);
        return btn;
    }

    private void EnsureSlotArrays()
    {
        if (_slotAutoClose == null || _slotAutoClose.Length != slots.Count)
            _slotAutoClose = new Coroutine[slots.Count];

        for (int i = 0; i < slots.Count; i++)
        {
            if (_slotAutoClose[i] != null) StopCoroutine(_slotAutoClose[i]);
            _slotAutoClose[i] = null;
        }
    }

    private IEnumerator DelayHideComment(int slot, float sec)
    {
        yield return new WaitForSeconds(sec);
        var s = (slot >= 0 && slot < slots.Count) ? slots[slot] : null;
        if (s != null && s.kind == UISlotKind.CommentWindow && s.parent)
            s.parent.SetActive(false);
        _slotAutoClose[slot] = null;
    }

    private void StartSlotAutoClose(int slot, float sec)
    {
        var s = (slot >= 0 && slot < slots.Count) ? slots[slot] : null;
        if (sec < 0f || s == null || s.kind != UISlotKind.CommentWindow) return;
        if (_slotAutoClose[slot] != null) StopCoroutine(_slotAutoClose[slot]);
        _slotAutoClose[slot] = StartCoroutine(DelayHideComment(slot, sec));
    }

    private int FindSlotIndex(string slotName, UISlotKind kind)
    {
        if (string.IsNullOrEmpty(slotName)) return -1;
        for (int i = 0; i < slots.Count; i++)
            if (slots[i].kind == kind && string.Equals(slots[i].name, slotName, StringComparison.Ordinal))
                return i;
        return -1;
    }

    private bool InRangeStep()
        => _currentSeq != null && _index >= 0 && _index < _currentSeq.steps.Count;

    private void RebuildSeqMap()
    {
        if (_seqMap == null) _seqMap = new Dictionary<string, CommentSequence>();
        _seqMap.Clear();
        if (namedSequences == null) return;

        foreach (var n in namedSequences)
        {
            if (n == null || string.IsNullOrEmpty(n.key) || n.sequence == null) continue;
            _seqMap[n.key] = n.sequence; // 後勝ち
        }
    }

    private bool TryGetSequenceByKey(string key, out CommentSequence seq)
    {
        seq = null;
        if (string.IsNullOrEmpty(key)) return false;
        RebuildSeqMap();
        return _seqMap.TryGetValue(key, out seq);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CommentSlotCatalog.AutoSyncFrom(this);
    }
#endif
}
