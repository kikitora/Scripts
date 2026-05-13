// StatusEffectPreviewController.cs
// 状態異常プレビュー用シーンの中核。
// ダミー unit に Aura prefab を装着 / Hit VFX を再生 / Animator.speed を制御してビジュアル確認する。
// MasterDatabase 経由で StatusEffectMetaDatabase を取得し、全 Entry 分のボタンを Canvas に動的生成。

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SteraCube.SpaceJourney
{
    [ExecuteAlways]
    public class StatusEffectPreviewController : MonoBehaviour
    {
        [Header("対象")]
        [Tooltip("aura prefab の装着先。中心 (0,0,0) に置いた DummyUnit 等を指定")]
        public Transform dummyUnit;

        [Tooltip("Animator (pauseMode テスト用)。Humanoid キャラの Animator を指定")]
        public Animator dummyAnimator;

        [Header("UI")]
        [Tooltip("ボタンを並べる ScrollView の Content (VerticalLayoutGroup 推奨)")]
        public RectTransform buttonContainer;

        [Tooltip("ボタンの Prefab (TMP_Text or 普通の Text 子を持つ Button)")]
        public Button buttonPrefab;

        [Header("カテゴリ別ヘッダー (任意)")]
        public bool showCategoryHeaders = true;
        public Color actionDisruptColor = new Color(1f, 0.5f, 0.5f);
        public Color debuffColor        = new Color(1f, 0.8f, 0.5f);
        public Color ailmentColor       = new Color(1f, 1f, 0.5f);
        public Color buffColor          = new Color(0.5f, 1f, 0.7f);

        [Header("アイコン表示")]
        [Tooltip("一覧ボタンの左側に StatusEffectMetaDatabase.Entry.icon を表示する")]
        public bool showIconsInButtons = true;

        [Header("プレビューモード")]
        [Tooltip("ON: slot/rank/exclusiveWith を実戦同様に適用。OFF: ルールを無視して押したエフェクトを全て重ねる")]
        public bool applyBattleRules = true;

        [Tooltip("プレビュー中の状態アイコンを dummyUnit の頭上に 2 段表示する")]
        public bool showSelectedIconAboveDummy = true;

        [Tooltip("頭上プレビューアイコンのワールドサイズ。戦闘画面の StatusEffectIconBar と同じ初期値")]
        public float selectedIconWorldSize = 0.216f;

        [Tooltip("頭上プレビューアイコン同士の横間隔")]
        public float selectedIconSpacing = 0.18f;

        [Tooltip("バフ段とデバフ/状態異常段の縦間隔")]
        public float selectedIconRowSpacing = 0.216f;

        [Tooltip("dummyUnit の Renderer 上端からの高さ")]
        public float selectedIconVerticalPadding = 0.25f;

        [Header("状態")]
        [SerializeField] private StatusEffectType _currentType = StatusEffectType.None;

        private readonly List<StatusEffectType> _activeTypes = new();
        private readonly List<StatusEffectType> _buffTypes = new();
        private readonly List<StatusEffectType> _badTypes = new();
        private readonly Dictionary<StatusEffectType, GameObject> _auras = new();
        private Transform _iconRoot;
        private Camera _cam;
        private Vector3 _cachedDummyIconPosition;
        private bool _hasCachedDummyIconPosition;

        private void OnEnable()
        {
            InitializePreview();
        }

        private void Start()
        {
            InitializePreview();
        }

        private void InitializePreview()
        {
            if (dummyAnimator == null && dummyUnit != null)
                dummyAnimator = dummyUnit.GetComponentInChildren<Animator>();
            _cam = Camera.main;
            CacheDummyIconPosition();
            BuildButtons();
        }

        private void LateUpdate()
        {
            if (_iconRoot == null) return;
            if (_cam == null) _cam = Camera.main;
            _iconRoot.position = GetDummyIconPosition();
            if (_cam != null)
                _iconRoot.rotation = _cam.transform.rotation;
        }

        private void BuildButtons()
        {
            if (buttonContainer == null || buttonPrefab == null) return;
            var db = GetStatusEffectMetaDatabase();
            if (db == null)
            {
                Debug.LogError("[StatusEffectPreview] MasterDatabase.StatusEffectMetaDatabase が未登録");
                return;
            }

            // カテゴリ順に並べる
            var groups = new (StatusEffectCategoryGroup cat, string label, Color color)[]
            {
                (StatusEffectCategoryGroup.ActionDisrupt, "━━ 行動妨害系 ━━", actionDisruptColor),
                (StatusEffectCategoryGroup.Debuff,        "━━ デバフ系 ━━",   debuffColor),
                (StatusEffectCategoryGroup.Ailment,       "━━ 状態異常系 ━━", ailmentColor),
                (StatusEffectCategoryGroup.Buff,          "━━ バフ・自己効果 ━━", buffColor),
            };

            // 既存の子をクリア (重複生成防止)
            for (int i = buttonContainer.childCount - 1; i >= 0; i--)
                DestroyPreviewObject(buttonContainer.GetChild(i).gameObject);

            // Stop ボタン (上部)
            CreateButton("◯ STOP / クリア", null, Color.white, () => ClearCurrent());
            CreateButton(GetRuleModeButtonLabel(), null, new Color(0.85f, 0.85f, 1f), ToggleRuleMode);

            foreach (var (cat, label, color) in groups)
            {
                if (showCategoryHeaders)
                    CreateHeader(label, color);
                foreach (var entry in db.GetListForCategory(cat))
                {
                    if (entry == null) continue;
                    var captured = entry;
                    string text = $"{captured.displayName}  [{captured.type}]";
                    CreateButton(text, captured.icon, color, () => Apply(captured.type));
                    if (captured.resolvedVfxPrefab != null || captured.resolvedTimelineSkill != null)
                    {
                        string resolvedText = $"  発動演出: {captured.displayName}  [{captured.type}]";
                        CreateButton(resolvedText, captured.icon, new Color(color.r * 0.85f, color.g * 0.85f, color.b * 0.85f, 1f),
                            () => PlayResolvedVfx(captured.type));
                    }
                }
            }
        }

        private void CreateHeader(string text, Color color)
        {
            var go = new GameObject($"Header_{text}", typeof(RectTransform));
            go.transform.SetParent(buttonContainer, false);
            var rt = (RectTransform)go.transform;
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
            var img = go.AddComponent<Image>();
            img.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 1f);
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var t = labelGO.AddComponent<Text>();
            t.text = text;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.fontSize = 16;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void CreateButton(string text, Sprite icon, Color color, System.Action onClick)
        {
            var btn = Instantiate(buttonPrefab, buttonContainer);
            btn.gameObject.SetActive(true);
            btn.name = $"Btn_{text}";
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = color;

            if (showIconsInButtons && icon != null)
                AttachButtonIcon(btn, icon);

            var label = btn.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = text;
                var rt = label.rectTransform;
                rt.offsetMin = new Vector2(Mathf.Max(rt.offsetMin.x, showIconsInButtons && icon != null ? 36f : rt.offsetMin.x), rt.offsetMin.y);
            }
            else
            {
                var tmp = btn.GetComponentInChildren<TMPro.TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = text;
                    var rt = tmp.rectTransform;
                    rt.offsetMin = new Vector2(Mathf.Max(rt.offsetMin.x, showIconsInButtons && icon != null ? 36f : rt.offsetMin.x), rt.offsetMin.y);
                }
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClick());
        }

        private void AttachButtonIcon(Button btn, Sprite icon)
        {
            var iconGO = new GameObject("StatusIcon", typeof(RectTransform));
            iconGO.transform.SetParent(btn.transform, false);
            var rt = (RectTransform)iconGO.transform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(18f, 0f);
            rt.sizeDelta = new Vector2(28f, 28f);

            var image = iconGO.AddComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        public void Apply(StatusEffectType type)
        {
            var db = GetStatusEffectMetaDatabase();
            var entry = db != null ? db.Get(type) : null;
            if (entry == null)
            {
                Debug.LogWarning($"[StatusEffectPreview] No entry for {type}");
                return;
            }
            if (!TryAddPreviewStatus(db, entry))
                return;

            // aura 装着
            if (entry.auraPrefab != null && dummyUnit != null && !_auras.ContainsKey(type))
            {
                var aura = Instantiate(entry.auraPrefab, dummyUnit);
                aura.transform.localPosition = Vector3.zero;
                aura.transform.localRotation = Quaternion.identity;
                aura.name = $"_Aura_{type}";
                _auras[type] = aura;
            }

            // hit VFX (ワンショット、5秒で破棄)
            if (entry.hitVfxPrefab != null && dummyUnit != null)
            {
                var hit = Instantiate(entry.hitVfxPrefab, dummyUnit.position, Quaternion.identity);
                Destroy(hit, 5f);
            }

            RebuildHeadIcons(db);
            UpdateAnimatorSpeed(db);

            Debug.Log($"[StatusEffectPreview] Applied {type} ({entry.displayName}): aura={(entry.auraPrefab != null)}, hitVfx={(entry.hitVfxPrefab != null)}, pause={entry.pauseMode}");
        }

        public void PlayResolvedVfx(StatusEffectType type)
        {
            var db = GetStatusEffectMetaDatabase();
            var entry = db != null ? db.Get(type) : null;
            if (entry == null)
            {
                Debug.LogWarning($"[StatusEffectPreview] No entry for {type}");
                return;
            }
            if (entry.resolvedVfxPrefab == null && entry.resolvedTimelineSkill == null)
            {
                Debug.LogWarning($"[StatusEffectPreview] {type} has no resolved visual");
                return;
            }
            if (dummyUnit == null) return;

            if (entry.resolvedVfxPrefab != null)
            {
                var vfx = Instantiate(entry.resolvedVfxPrefab, dummyUnit.position, Quaternion.identity);
                vfx.name = $"_ResolvedVfx_{type}";
                if (Application.isPlaying)
                    Destroy(vfx, 5f);
            }

            PlayResolvedTimelinePreview(entry.resolvedTimelineSkill);

            Debug.Log($"[StatusEffectPreview] Played resolved visual {type} ({entry.displayName})");
        }

        private void PlayResolvedTimelinePreview(Realtime.RealtimeSkillDefinition skill)
        {
            if (skill == null || dummyAnimator == null) return;

            string stateName = skill.animStateName;
            if (!string.IsNullOrEmpty(stateName) && dummyAnimator.HasState(0, Animator.StringToHash(stateName)))
            {
                dummyAnimator.CrossFadeInFixedTime(stateName, 0.08f);
                return;
            }

            string triggerName = skill.animTriggerName;
            if (!string.IsNullOrEmpty(triggerName) && HasAnimatorParameter(dummyAnimator, triggerName))
            {
                dummyAnimator.SetTrigger(triggerName);
            }
        }

        private static bool HasAnimatorParameter(Animator animator, string parameterName)
        {
            if (animator == null || string.IsNullOrEmpty(parameterName)) return false;
            foreach (var p in animator.parameters)
            {
                if (p.name == parameterName) return true;
            }
            return false;
        }

        private bool TryAddPreviewStatus(StatusEffectMetaDatabase db, StatusEffectMetaDatabase.Entry entry)
        {
            StatusEffectType type = entry.type;
            _currentType = type;

            if (!applyBattleRules)
            {
                if (!_activeTypes.Contains(type))
                    _activeTypes.Add(type);
                return true;
            }

            if (entry.exclusiveWith != null && entry.exclusiveWith.Count > 0)
            {
                for (int i = _activeTypes.Count - 1; i >= 0; i--)
                {
                    if (entry.exclusiveWith.Contains(_activeTypes[i]))
                        RemovePreviewStatus(_activeTypes[i]);
                }
            }

            var newSlot = entry.slot;
            int newRank = entry.rank;

            for (int i = _activeTypes.Count - 1; i >= 0; i--)
            {
                var existingType = _activeTypes[i];
                var existing = db.Get(existingType);
                if (existing == null || existing.slot != newSlot) continue;

                if (existing.rank > newRank)
                {
                    Debug.Log($"[StatusEffectPreview] Rejected {type}: weaker than active {existingType}");
                    return false;
                }

                if (existingType == type)
                    return true;

                RemovePreviewStatus(existingType);
            }

            if (!_activeTypes.Contains(type))
                _activeTypes.Add(type);
            return true;
        }

        private string GetRuleModeButtonLabel()
        {
            return applyBattleRules
                ? "MODE: 実戦ルール適用"
                : "MODE: ルール無視で全表示";
        }

        private void ToggleRuleMode()
        {
            applyBattleRules = !applyBattleRules;
            ClearCurrent();
            BuildButtons();
        }

        private void RemovePreviewStatus(StatusEffectType type)
        {
            _activeTypes.Remove(type);
            if (_auras.TryGetValue(type, out var aura) && aura != null)
                DestroyPreviewObject(aura);
            _auras.Remove(type);
        }

        private void RebuildHeadIcons(StatusEffectMetaDatabase db)
        {
            if (!showSelectedIconAboveDummy || dummyUnit == null) return;

            EnsureIconRoot();
            ClearIconChildren();
            _buffTypes.Clear();
            _badTypes.Clear();

            for (int i = 0; i < _activeTypes.Count; i++)
            {
                var type = _activeTypes[i];
                var entry = db.Get(type);
                if (entry == null || entry.icon == null) continue;
                if (StatusEffectMetaDatabase.CategorizeByType(type) == StatusEffectCategoryGroup.Buff)
                    _buffTypes.Add(type);
                else
                    _badTypes.Add(type);
            }

            AddIconRow(db, _buffTypes, selectedIconRowSpacing * 0.5f);
            AddIconRow(db, _badTypes, -selectedIconRowSpacing * 0.5f);
            _iconRoot.gameObject.SetActive(_buffTypes.Count + _badTypes.Count > 0);
        }

        private void AddIconRow(StatusEffectMetaDatabase db, List<StatusEffectType> row, float localY)
        {
            if (row.Count == 0) return;
            float startX = -((row.Count - 1) * selectedIconSpacing) * 0.5f;

            for (int i = 0; i < row.Count; i++)
            {
                var type = row[i];
                var entry = db.Get(type);
                if (entry == null || entry.icon == null) continue;

                var go = new GameObject($"StatusIcon_{type}");
                go.transform.SetParent(_iconRoot, false);
                go.transform.localPosition = new Vector3(startX + i * selectedIconSpacing, localY, 0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = entry.icon;
                sr.sortingOrder = 250;

                Vector2 spriteSize = entry.icon.bounds.size;
                float baseSize = Mathf.Max(0.01f, Mathf.Max(spriteSize.x, spriteSize.y));
                go.transform.localScale = Vector3.one * (selectedIconWorldSize / baseSize);
            }
        }

        private void EnsureIconRoot()
        {
            if (_iconRoot != null) return;
            var go = new GameObject("_StatusIconBarPreview");
            _iconRoot = go.transform;
            _iconRoot.position = GetDummyIconPosition();
            if (_cam == null) _cam = Camera.main;
            if (_cam != null)
                _iconRoot.rotation = _cam.transform.rotation;
        }

        private void ClearIconChildren()
        {
            if (_iconRoot == null) return;
            for (int i = _iconRoot.childCount - 1; i >= 0; i--)
                DestroyPreviewObject(_iconRoot.GetChild(i).gameObject);
        }

        private void UpdateAnimatorSpeed(StatusEffectMetaDatabase db)
        {
            if (dummyAnimator == null) return;

            AnimatorPauseMode strongest = AnimatorPauseMode.None;
            for (int i = 0; i < _activeTypes.Count; i++)
            {
                var entry = db.Get(_activeTypes[i]);
                if (entry != null && (int)entry.pauseMode > (int)strongest)
                    strongest = entry.pauseMode;
            }

            dummyAnimator.speed = strongest switch
            {
                AnimatorPauseMode.FullStop => 0f,
                AnimatorPauseMode.SlowMotion => 0.3f,
                _ => 1f,
            };
        }

        private Vector3 GetDummyIconPosition()
        {
            if (_hasCachedDummyIconPosition)
                return _cachedDummyIconPosition;

            if (dummyUnit == null)
                return Vector3.up * selectedIconVerticalPadding;

            Bounds bounds = default;
            bool hasBounds = false;
            var renderers = dummyUnit.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || r is SpriteRenderer) continue;
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            if (hasBounds)
                return new Vector3(bounds.center.x, bounds.max.y + selectedIconVerticalPadding, bounds.center.z);

            return dummyUnit.position + Vector3.up * (1.8f + selectedIconVerticalPadding);
        }

        private void CacheDummyIconPosition()
        {
            _hasCachedDummyIconPosition = false;
            if (dummyUnit == null) return;

            Bounds bounds = default;
            bool hasBounds = false;
            var renderers = dummyUnit.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || r is SpriteRenderer) continue;
                foreach (var kv in _auras)
                {
                    if (kv.Value != null && r.transform.IsChildOf(kv.Value.transform))
                        goto SkipRenderer;
                }
                if (_iconRoot != null && r.transform.IsChildOf(_iconRoot)) continue;
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
                SkipRenderer: ;
            }

            _cachedDummyIconPosition = hasBounds
                ? new Vector3(bounds.center.x, bounds.max.y + selectedIconVerticalPadding, bounds.center.z)
                : dummyUnit.position + Vector3.up * (1.8f + selectedIconVerticalPadding);
            _hasCachedDummyIconPosition = true;
        }

        public void ClearCurrent()
        {
            foreach (var kv in _auras)
            {
                if (kv.Value != null) DestroyPreviewObject(kv.Value);
            }
            _auras.Clear();
            _activeTypes.Clear();
            ClearIconChildren();
            if (_iconRoot != null) _iconRoot.gameObject.SetActive(false);
            _currentType = StatusEffectType.None;
            if (dummyAnimator != null) dummyAnimator.speed = 1f;
        }

        private StatusEffectMetaDatabase GetStatusEffectMetaDatabase()
        {
            if (MasterDatabase.Instance != null && MasterDatabase.Instance.StatusEffectMetaDatabase != null)
                return MasterDatabase.Instance.StatusEffectMetaDatabase;

            var dbInScene = FindFirstObjectByType<MasterDatabase>();
            if (dbInScene != null)
                return dbInScene.StatusEffectMetaDatabase;

            return StatusEffectMeta.CurrentDatabase;
        }

        private static void DestroyPreviewObject(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying)
                Destroy(go);
            else
                DestroyImmediate(go);
        }
    }
}
