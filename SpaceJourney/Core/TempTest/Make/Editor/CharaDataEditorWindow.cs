#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney.DevTools
{
    public class CharaDataEditorWindow : EditorWindow
    {
        // ── 定数 ─────────────────────────────────────────────────
        private const int GridSize = 3;
        private const int CellSize = 76;
        private const int CellPad = 4;
        private const int LeftW = 240;
        private const int RightW = 300;

        // ── タブ ─────────────────────────────────────────────────
        private enum Tab { Make = 0, Placement = 1 }
        private Tab _tab = Tab.Make;

        // ── グループ設定（配置タブ用） ────────────────────────────
        private DefaultAsset _saveFolder;
        private string _groupId = "EnemyGroup_";
        private string _groupDesc = "";
        private CubeKind _cubeKind = CubeKind.EnemyNormal;
        private int _maxVp = 10;

        // ── Soul / Body リスト ───────────────────────────────────
        private List<SoulDefinitionSO> _souls = new();
        private List<BodyDefinitionSO> _bodies = new();
        private string _searchSoul = "";
        private string _searchBody = "";
        private Vector2 _soulScroll;
        private Vector2 _bodyScroll;

        // ── キャラ制作タブ専用 ───────────────────────────────────
        private SoulDefinitionSO _editingSoul = null;   // 現在編集中のSoul

        // ── 配置タブ専用（グリッド。BodyはSoul.BodyDefinitionを参照）─
        private SoulDefinitionSO[] _gridSouls = new SoulDefinitionSO[GridSize * GridSize];
        private int _selectedCell = -1;

        // ── 右パネル ─────────────────────────────────────────────
        private ScriptableObject _selectedLeftSO = null;
        private Vector2 _rightScroll;
        private ScriptableObject _soulEditorTarget;
        private ScriptableObject _bodyEditorTarget;
        private SerializedObject _soulSO;
        private SerializedObject _bodySO;

        // ── ステータス ───────────────────────────────────────────
        private string _statusMsg = "";
        private Color _statusColor = Color.gray;

        // ── 色定数 ───────────────────────────────────────────────
        private static readonly Color ColAccent = new Color(0.25f, 0.65f, 0.45f);
        private static readonly Color ColCellEmpty = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color ColCellFilled = new Color(0.18f, 0.35f, 0.26f);
        private static readonly Color ColCellSelected = new Color(0.20f, 0.42f, 0.62f);
        private static readonly Color ColCellHalf = new Color(0.25f, 0.35f, 0.30f);

        // ─────────────────────────────────────────────────────────
        [MenuItem("Window/SteraCube/Chara Data Editor")]
        public static void Open()
        {
            var w = GetWindow<CharaDataEditorWindow>("Chara Data Editor");
            w.minSize = new Vector2(900, 540);
            w.RefreshLists();
        }

        private void OnEnable() => RefreshLists();
        private void OnDestroy()
        {
            _soulSO?.Dispose();
            _bodySO?.Dispose();
        }

        // ─────────────────────────────────────────────────────────
        // OnGUI
        // ─────────────────────────────────────────────────────────
        private void OnGUI()
        {
            DrawTopBar();
            EditorGUILayout.Space(2);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftW), GUILayout.ExpandHeight(true)))
                    DrawLeftPanel();

                DrawVerticalSep();

                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    if (_tab == Tab.Make)
                        DrawCenterMakePanel();
                    else
                        DrawCenterPlacementPanel();
                }

                DrawVerticalSep();

                using (new EditorGUILayout.VerticalScope(GUILayout.Width(RightW), GUILayout.ExpandHeight(true)))
                    DrawRightPanel();
            }

            DrawStatusBar();
        }

        // ─────────────────────────────────────────────────────────
        // トップバー
        // ─────────────────────────────────────────────────────────
        private void DrawTopBar()
        {
            // 行1：タブ切替 + 保存先（共通）+ 再読込
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DrawTabButton(Tab.Make, "🎨 キャラ制作");
                DrawTabButton(Tab.Placement, "📐 配置");
                GUILayout.Space(12);

                EditorGUILayout.LabelField("保存先:", GUILayout.Width(46));
                _saveFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                    _saveFolder, typeof(DefaultAsset), false, GUILayout.Width(170));
                GUILayout.Space(6);

                if (GUILayout.Button("↺ 再読込", EditorStyles.toolbarButton, GUILayout.Width(64)))
                {
                    RefreshLists();
                    RefreshIdCache();
                    Status("リスト再読込完了", ColAccent);
                }
                GUILayout.FlexibleSpace();
            }

            // 行2：配置タブのみ（グループ保存設定）
            if (_tab == Tab.Placement)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("GroupID:", GUILayout.Width(56));
                    _groupId = EditorGUILayout.TextField(_groupId, GUILayout.Width(130));
                    GUILayout.Space(4);

                    EditorGUILayout.LabelField("Kind:", GUILayout.Width(34));
                    _cubeKind = (CubeKind)EditorGUILayout.EnumPopup(_cubeKind, GUILayout.Width(100));
                    GUILayout.Space(4);

                    EditorGUILayout.LabelField("MaxVP:", GUILayout.Width(44));
                    _maxVp = EditorGUILayout.IntField(_maxVp, GUILayout.Width(36));
                    GUILayout.Space(4);

                    EditorGUILayout.LabelField("説明:", GUILayout.Width(30));
                    _groupDesc = EditorGUILayout.TextField(_groupDesc, GUILayout.ExpandWidth(true));
                    GUILayout.Space(6);

                    var oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = ColAccent;
                    if (GUILayout.Button("💾 保存", EditorStyles.toolbarButton, GUILayout.Width(80)))
                        SaveBundle();
                    GUI.backgroundColor = oldBg;
                }
            }
        }

        private void DrawTabButton(Tab target, string label)
        {
            bool active = (_tab == target);
            var style = new GUIStyle(EditorStyles.toolbarButton)
            { fontStyle = active ? FontStyle.Bold : FontStyle.Normal };
            var oldBg = GUI.backgroundColor;
            if (active) GUI.backgroundColor = ColAccent;
            if (GUILayout.Button(label, style, GUILayout.Width(100)))
            {
                _tab = target;
                _selectedLeftSO = null;
                _selectedCell = -1;
                _editingSoul = null;
                ClearEditors();
                Repaint();
            }
            GUI.backgroundColor = oldBg;
        }

        // ─────────────────────────────────────────────────────────
        // 左パネル（共通）
        // ─────────────────────────────────────────────────────────
        private void DrawLeftPanel()
        {
            if (_tab == Tab.Make)
                DrawLeftMakePanel();
            else
                DrawLeftPlacementPanel();
        }

        // キャラ制作タブ：Soul / Body 両リスト
        private void DrawLeftMakePanel()
        {
            SectionHeader("▼ Soul Definitions");
            _searchSoul = SearchBar(_searchSoul);

            float halfH = (position.height - 140) * 0.5f - 30;
            using (var sv = new EditorGUILayout.ScrollViewScope(_soulScroll, GUILayout.Height(halfH)))
            {
                _soulScroll = sv.scrollPosition;
                foreach (var s in _souls.Where(s => s != null && Matches(s.DefinitionId, _searchSoul)))
                    DrawSoulRow(s);
            }
            if (GUILayout.Button("+ 新規 SoulDefinitionSO"))
                CreateNew<SoulDefinitionSO>("SoulDef_New");

            EditorGUILayout.Space(4);

            SectionHeader("▼ Body Definitions");
            _searchBody = SearchBar(_searchBody);

            using (var sv = new EditorGUILayout.ScrollViewScope(_bodyScroll, GUILayout.Height(halfH)))
            {
                _bodyScroll = sv.scrollPosition;
                foreach (var b in _bodies.Where(b => b != null && Matches(b.DefinitionId, _searchBody)))
                    DrawBodyRow(b);
            }
            if (GUILayout.Button("+ 新規 BodyDefinitionSO"))
                CreateNew<BodyDefinitionSO>("BodyDef_New");
        }

        // 配置タブ：ペアリング済みSoulのみ「キャラクターリスト」として表示
        private void DrawLeftPlacementPanel()
        {
            SectionHeader("▼ キャラクターリスト");
            _searchSoul = SearchBar(_searchSoul);

            var paired = _souls.Where(s => s != null && s.BodyDefinition != null
                && Matches(s.DefinitionId, _searchSoul)).ToList();

            float listH = position.height - 140;
            using (var sv = new EditorGUILayout.ScrollViewScope(_soulScroll, GUILayout.Height(listH)))
            {
                _soulScroll = sv.scrollPosition;
                if (paired.Count == 0)
                {
                    EditorGUILayout.LabelField(
                        string.IsNullOrEmpty(_searchSoul)
                            ? "ペアリング済みのキャラがいません\n「キャラ制作」タブでペアリングしてください"
                            : "検索に一致するキャラがいません",
                        EditorStyles.wordWrappedMiniLabel);
                }
                else
                {
                    foreach (var s in paired)
                        DrawSoulRow(s);
                }
            }
        }

        private void DrawSoulRow(SoulDefinitionSO soul)
        {
            bool isEditing = (_editingSoul == soul);
            bool hasPair = (soul.BodyDefinition != null);
            string pairIcon = hasPair ? "●" : "○";
            Color pairCol = hasPair ? ColAccent : new Color(0.55f, 0.55f, 0.55f);
            string label = string.IsNullOrEmpty(soul.DefinitionId) ? soul.name : soul.DefinitionId;

            using (new EditorGUILayout.HorizontalScope())
            {
                // ペアリングインジケータ
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = pairCol;
                GUILayout.Box(pairIcon, GUILayout.Width(16), GUILayout.Height(18));
                GUI.backgroundColor = oldBg;

                // Soul バッジ
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.25f, 0.48f, 0.80f);
                GUILayout.Box("S", GUILayout.Width(16), GUILayout.Height(18));
                GUI.backgroundColor = oldBg;

                // ラベルボタン
                var style = new GUIStyle(isEditing ? EditorStyles.boldLabel : EditorStyles.label)
                { alignment = TextAnchor.MiddleLeft };
                if (GUILayout.Button(label, style, GUILayout.ExpandWidth(true), GUILayout.Height(18)))
                {
                    _editingSoul = (_tab == Tab.Make) ? soul : _editingSoul;
                    _selectedLeftSO = soul;
                    _selectedCell = -1;
                    ClearEditors();
                    Repaint();
                }

                // 配置タブ：配置ボタン（セル選択中のみ）
                if (_tab == Tab.Placement && _selectedCell >= 0)
                {
                    if (string.IsNullOrEmpty(soul.DefinitionId))
                        EditorGUILayout.LabelField("ID未入力", EditorStyles.miniLabel, GUILayout.Width(48));
                    else if (GUILayout.Button("配置", GUILayout.Width(36)))
                    {
                        _gridSouls[_selectedCell] = soul;
                        ClearEditors();
                        Status($"セル{CellLabel(_selectedCell)} ← {soul.DefinitionId}", ColAccent);
                        Repaint();
                    }
                }

                // 削除ボタン
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.7f, 0.2f, 0.2f);
                if (GUILayout.Button("🗑", GUILayout.Width(22)))
                    DeleteSO(soul);
                GUI.backgroundColor = oldBg;
            }
        }

        private void DrawBodyRow(BodyDefinitionSO body)
        {
            bool isSelected = (_selectedLeftSO == body);
            string label = string.IsNullOrEmpty(body.DefinitionId) ? body.name : body.DefinitionId;

            using (new EditorGUILayout.HorizontalScope())
            {
                // Body バッジ
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.75f, 0.40f, 0.15f);
                GUILayout.Box("B", GUILayout.Width(16), GUILayout.Height(18));
                GUI.backgroundColor = oldBg;

                // ラベルボタン
                var style = new GUIStyle(isSelected ? EditorStyles.boldLabel : EditorStyles.label)
                { alignment = TextAnchor.MiddleLeft };
                if (GUILayout.Button(label, style, GUILayout.ExpandWidth(true), GUILayout.Height(18)))
                {
                    _selectedLeftSO = body;
                    _selectedCell = -1;
                    ClearEditors();
                    Repaint();
                }

                // 制作タブ：→ペアボタン（Soul編集中のみ）
                if (_tab == Tab.Make && _editingSoul != null)
                {
                    if (string.IsNullOrEmpty(body.DefinitionId))
                        EditorGUILayout.LabelField("ID未入力", EditorStyles.miniLabel, GUILayout.Width(48));
                    else if (GUILayout.Button("→ペア", GUILayout.Width(44)))
                        SetBodyPair(_editingSoul, body);
                }

                // 削除ボタン
                oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.7f, 0.2f, 0.2f);
                if (GUILayout.Button("🗑", GUILayout.Width(22)))
                    DeleteSO(body);
                GUI.backgroundColor = oldBg;
            }
        }

        // ─────────────────────────────────────────────────────────
        // 中央：キャラ制作タブ
        // ─────────────────────────────────────────────────────────
        private void DrawCenterMakePanel()
        {
            EditorGUILayout.LabelField("── キャラ制作（ペアリング） ──", EditorStyles.boldLabel);

            if (_editingSoul == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("← 左パネルの Soul をクリックして選択してください",
                    EditorStyles.wordWrappedMiniLabel);
                GUILayout.FlexibleSpace();
                return;
            }

            EditorGUILayout.Space(8);

            // Soul 情報
            SectionHeader("◆ Soul");
            using (new EditorGUILayout.HorizontalScope())
            {
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.25f, 0.48f, 0.80f);
                GUILayout.Box("S", GUILayout.Width(20), GUILayout.Height(20));
                GUI.backgroundColor = oldBg;

                string sLabel = string.IsNullOrEmpty(_editingSoul.DefinitionId)
                    ? _editingSoul.name : _editingSoul.DefinitionId;
                EditorGUILayout.LabelField(sLabel, EditorStyles.boldLabel);

                if (GUILayout.Button("詳細編集", GUILayout.Width(60)))
                {
                    _selectedLeftSO = _editingSoul;
                    ClearEditors();
                    Repaint();
                }
            }

            EditorGUILayout.Space(12);

            // Body（ペア）情報
            SectionHeader("◆ Body（ペア）");
            var pairedBody = _editingSoul.BodyDefinition;
            using (new EditorGUILayout.HorizontalScope())
            {
                var oldBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.75f, 0.40f, 0.15f);
                GUILayout.Box("B", GUILayout.Width(20), GUILayout.Height(20));
                GUI.backgroundColor = oldBg;

                if (pairedBody != null)
                {
                    string bLabel = string.IsNullOrEmpty(pairedBody.DefinitionId)
                        ? pairedBody.name : pairedBody.DefinitionId;
                    EditorGUILayout.LabelField(bLabel, EditorStyles.boldLabel);

                    if (GUILayout.Button("詳細編集", GUILayout.Width(60)))
                    {
                        _selectedLeftSO = pairedBody;
                        ClearEditors();
                        Repaint();
                    }
                    if (GUILayout.Button("解除", GUILayout.Width(40)))
                        SetBodyPair(_editingSoul, null);
                }
                else
                {
                    EditorGUILayout.LabelField("未設定  ← 左パネルのBodyの [→ペア] でセット",
                        EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space(16);

            // バリデーション表示
            bool soulOk = !string.IsNullOrEmpty(_editingSoul.DefinitionId);
            bool bodyOk = (pairedBody != null);

            var prevCol = GUI.color;
            if (soulOk && bodyOk)
            {
                GUI.color = ColAccent;
                EditorGUILayout.LabelField("✓  ペアリング完了", EditorStyles.boldLabel);
            }
            else
            {
                GUI.color = new Color(1f, 0.75f, 0.3f);
                if (!soulOk) EditorGUILayout.LabelField("⚠  Soul の DefinitionId が未入力です", EditorStyles.miniLabel);
                if (!bodyOk) EditorGUILayout.LabelField("△  Body 未設定（ボディなしで登場）", EditorStyles.miniLabel);
            }
            GUI.color = prevCol;

            GUILayout.FlexibleSpace();
        }

        // ─────────────────────────────────────────────────────────
        // 中央：配置タブ（3×3グリッド）
        // ─────────────────────────────────────────────────────────
        private void DrawCenterPlacementPanel()
        {
            EditorGUILayout.LabelField("── 3×3 配置グリッド ──", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("セルをクリックで選択 → 左パネルの [配置] でSoulをセット",
                EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();

            float totalW = GridSize * (CellSize + CellPad) - CellPad;
            float totalH = GridSize * (CellSize + CellPad) - CellPad;
            var gridRect = GUILayoutUtility.GetRect(totalW, totalH,
                GUILayout.Width(totalW), GUILayout.Height(totalH));
            float areaX = gridRect.x + (gridRect.width - totalW) * 0.5f;

            for (int y = GridSize - 1; y >= 0; y--)
                for (int x = 0; x < GridSize; x++)
                {
                    int idx = y * GridSize + x;
                    float cx = areaX + x * (CellSize + CellPad);
                    float cy = gridRect.y + (GridSize - 1 - y) * (CellSize + CellPad);
                    DrawCell(new Rect(cx, cy, CellSize, CellSize), idx, x, y);
                }

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("グリッドをクリア", GUILayout.Width(120)))
                    ClearGrid();
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(8);
        }

        private void DrawCell(Rect r, int idx, int x, int y)
        {
            bool sel = (idx == _selectedCell);
            var soul = _gridSouls[idx];
            bool hasSoul = (soul != null);
            bool hasBody = hasSoul && (soul.BodyDefinition != null);

            Color bg = sel ? ColCellSelected
                     : (hasSoul && hasBody) ? ColCellFilled
                     : hasSoul ? ColCellHalf
                     : ColCellEmpty;
            EditorGUI.DrawRect(r, bg);
            DrawBorder(r, sel ? Color.white : new Color(0.38f, 0.38f, 0.38f));

            // 座標ラベル
            GUI.Label(new Rect(r.x + 3, r.y + 2, 28, 13), $"{x},{y}",
                new GUIStyle { fontSize = 8, normal = { textColor = new Color(0.7f, 0.7f, 0.7f) } });

            // Soul名
            string sLabel = hasSoul ? "S: " + Clip(soul.DefinitionId ?? soul.name) : "S: ─";
            Color sCol = hasSoul ? new Color(0.55f, 0.78f, 1f) : new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(r.x + 3, r.y + 16, r.width - 6, 16), sLabel,
                new GUIStyle { fontSize = 8, normal = { textColor = sCol } });

            // Body名（Soul.BodyDefinition 参照）
            string bLabel = hasBody
                ? "B: " + Clip(soul.BodyDefinition.DefinitionId ?? soul.BodyDefinition.name)
                : "B: ─";
            Color bCol = hasBody ? new Color(1f, 0.72f, 0.42f) : new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(r.x + 3, r.y + 31, r.width - 6, 16), bLabel,
                new GUIStyle { fontSize = 8, normal = { textColor = bCol } });

            // ✕ クリアボタン
            var clearBtnRect = new Rect(r.xMax - 17, r.y + 2, 15, 15);
            if (hasSoul && GUI.Button(clearBtnRect, "✕",
                new GUIStyle(EditorStyles.miniButton) { fontSize = 7 }))
            {
                _gridSouls[idx] = null;
                if (_selectedCell == idx) _selectedCell = -1;
                Repaint();
                return;
            }

            // クリック → セル選択
            if (Event.current.type == EventType.MouseDown
                && r.Contains(Event.current.mousePosition)
                && !clearBtnRect.Contains(Event.current.mousePosition))
            {
                _selectedCell = sel ? -1 : idx;
                _selectedLeftSO = null;
                ClearEditors();
                GUI.FocusControl(null);
                Event.current.Use();
                Repaint();
            }
        }

        // ─────────────────────────────────────────────────────────
        // 右パネル（共通：SO詳細エディタ）
        // ─────────────────────────────────────────────────────────
        private void DrawRightPanel()
        {
            EditorGUILayout.LabelField("── 詳細エディタ ──", EditorStyles.boldLabel);

            // 左パネルSO選択中
            if (_selectedLeftSO != null)
            {
                EditorGUILayout.LabelField(_selectedLeftSO.name, EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                var so = GetOrBuildSO(_selectedLeftSO);
                if (so != null)
                    using (var sv = new EditorGUILayout.ScrollViewScope(_rightScroll))
                    {
                        _rightScroll = sv.scrollPosition;
                        DrawSOFields(so, _selectedLeftSO);
                    }
                return;
            }

            // 配置タブ：グリッドセル選択中
            if (_tab == Tab.Placement && _selectedCell >= 0)
            {
                int gx = _selectedCell % GridSize;
                int gy = _selectedCell / GridSize;
                EditorGUILayout.LabelField($"セル ({gx}, {gy})", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);

                var soul = _gridSouls[_selectedCell];
                if (soul != null)
                {
                    using (var sv = new EditorGUILayout.ScrollViewScope(_rightScroll))
                    {
                        _rightScroll = sv.scrollPosition;
                        SectionHeader("◆ Soul");
                        if (GUILayout.Button("このセルから Soul を解除", GUILayout.Height(20)))
                        {
                            _gridSouls[_selectedCell] = null;
                            ClearEditors();
                            Repaint();
                        }
                        RebuildIfNeeded(ref _soulEditorTarget, ref _soulSO, soul);
                        if (_soulSO != null) DrawSOFields(_soulSO, soul);
                    }
                }
                else
                    EditorGUILayout.LabelField("未設定 ─ [配置] でSoulをセット", EditorStyles.miniLabel);
                return;
            }

            // 何も選択なし
            EditorGUILayout.Space(8);
            string hint = _tab == Tab.Make
                ? "← Soulをクリックして選択\nBodyの [→ペア] でペアリング"
                : "← SOかグリッドセルをクリック";
            EditorGUILayout.LabelField(hint, EditorStyles.wordWrappedMiniLabel);
        }

        // ─────────────────────────────────────────────────────────
        // ペアリング操作
        // ─────────────────────────────────────────────────────────
        private void SetBodyPair(SoulDefinitionSO soul, BodyDefinitionSO body)
        {
            if (soul == null) return;
            var so = new SerializedObject(soul);
            so.FindProperty("bodyDefinition").objectReferenceValue = body;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(soul);
            AssetDatabase.SaveAssets();

            string msg = body != null
                ? $"ペアリング: {soul.DefinitionId} ⇔ {body.DefinitionId}"
                : $"ペアリング解除: {soul.DefinitionId}";
            Status(msg, ColAccent);
            Repaint();
        }

        // ─────────────────────────────────────────────────────────
        // SO エディタ
        // ─────────────────────────────────────────────────────────
        private SerializedObject GetOrBuildSO(ScriptableObject target)
        {
            if (target is SoulDefinitionSO soul)
            {
                RebuildIfNeeded(ref _soulEditorTarget, ref _soulSO, soul);
                if (_bodyEditorTarget != null) { _bodySO?.Dispose(); _bodySO = null; _bodyEditorTarget = null; }
                return _soulSO;
            }
            if (target is BodyDefinitionSO body)
            {
                RebuildIfNeeded(ref _bodyEditorTarget, ref _bodySO, body);
                if (_soulEditorTarget != null) { _soulSO?.Dispose(); _soulSO = null; _soulEditorTarget = null; }
                return _bodySO;
            }
            return null;
        }

        private void DrawSOFields(SerializedObject so, ScriptableObject dirty)
        {
            so.Update();
            EditorGUI.BeginChangeCheck();

            var prop = so.GetIterator();
            bool enter = true;
            while (prop.NextVisible(enter))
            {
                enter = false;
                if (prop.propertyPath == "m_Script") continue;

                // string 型かつ既知のID フィールド → ドロップダウン
                if (CharaDataIdFields.TryDrawDropdown(prop)) continue;

                // learnedSkillIds (List<string>) → スキルドロップダウンリスト
                // ※ トップレベルにある場合のみ（ネスト内は OneReinSoulConfigDrawer が処理）
                if (prop.isArray
                    && prop.propertyType == SerializedPropertyType.Generic
                    && prop.name == "learnedSkillIds")
                {
                    CharaDataIdFields.DrawSkillIdList(prop);
                    continue;
                }

                EditorGUILayout.PropertyField(prop, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(dirty);
                Repaint();
            }
        }

        /// <summary>IDキャッシュを手動リフレッシュ（再読込ボタン押下時に呼ぶ）。</summary>
        private static void RefreshIdCache() => CharaDataIdFields.Refresh();

        private void RebuildIfNeeded(
            ref ScriptableObject target,
            ref SerializedObject so,
            ScriptableObject current)
        {
            if (target == current && so != null) return;
            so?.Dispose(); so = null;
            target = current;
            if (current != null) so = new SerializedObject(current);
        }

        private void ClearEditors()
        {
            _soulEditorTarget = null; _soulSO?.Dispose(); _soulSO = null;
            _bodyEditorTarget = null; _bodySO?.Dispose(); _bodySO = null;
        }

        // ─────────────────────────────────────────────────────────
        // UI パーツ
        // ─────────────────────────────────────────────────────────
        private void SectionHeader(string label)
        {
            var rc = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rc, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private string SearchBar(string val)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("🔍", GUILayout.Width(18));
                val = EditorGUILayout.TextField(val);
                if (GUILayout.Button("✕", GUILayout.Width(20))) val = "";
            }
            return val;
        }

        private void DrawStatusBar()
        {
            GUI.Label(new Rect(8, position.height - 20, position.width - 16, 18),
                _statusMsg,
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = _statusColor } });
        }

        private static bool Matches(string s, string q)
            => string.IsNullOrEmpty(q) || (s ?? "").ToLower().Contains(q.ToLower());

        private static string Clip(string s)
            => (s != null && s.Length > 9) ? s[..8] + "…" : (s ?? "");

        private static string CellLabel(int i) => $"({i % GridSize},{i / GridSize})";

        private static void DrawVerticalSep()
        {
            var rc = EditorGUILayout.GetControlRect(false, GUILayout.Width(1), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rc, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private static void DrawBorder(Rect r, Color c)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), c);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), c);
        }

        // ─────────────────────────────────────────────────────────
        // アセット操作
        // ─────────────────────────────────────────────────────────
        private void RefreshLists()
        {
            _souls = LoadAll<SoulDefinitionSO>();
            _bodies = LoadAll<BodyDefinitionSO>();
            Repaint();
        }

        private static List<T> LoadAll<T>() where T : ScriptableObject
            => AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

        private void CreateNew<T>(string defaultName) where T : ScriptableObject
        {
            var folder = GetFolder(); if (folder == null) return;
            var path = EditorUtility.SaveFilePanelInProject(
                $"新規 {typeof(T).Name}", defaultName, "asset", "ファイル名を入力", folder);
            if (string.IsNullOrEmpty(path)) return;
            var asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            RefreshLists();
            Selection.activeObject = asset;
            Status($"作成: {path}", ColAccent);
        }

        private void DeleteSO(ScriptableObject so)
        {
            string path = AssetDatabase.GetAssetPath(so);
            if (!EditorUtility.DisplayDialog("削除の確認",
                $"「{so.name}」を削除しますか？\n{path}\n\n元に戻せません。",
                "削除する", "キャンセル")) return;

            for (int i = 0; i < GridSize * GridSize; i++)
                if (_gridSouls[i] == so) _gridSouls[i] = null;

            if (_selectedLeftSO == so) { _selectedLeftSO = null; ClearEditors(); }
            if (_editingSoul == so) _editingSoul = null;

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
            RefreshLists();
            Status($"削除: {so.name}", new Color(1f, 0.5f, 0.3f));
        }

        // ─────────────────────────────────────────────────────────
        // Bundle 保存
        // ─────────────────────────────────────────────────────────
        private void SaveBundle()
        {
            var folder = GetFolder(); if (folder == null) return;
            if (string.IsNullOrEmpty(_groupId)) { Status("GroupID が未入力です", Color.red); return; }

            var assetPath = $"{folder}/EnemyGroup_{_groupId}.asset";
            var groupSO = AssetDatabase.LoadAssetAtPath<EnemyGroupDefinitionSO>(assetPath)
                       ?? CreateInstance<EnemyGroupDefinitionSO>();

            if (!AssetDatabase.Contains(groupSO))
                AssetDatabase.CreateAsset(groupSO, assetPath);

            var so = new SerializedObject(groupSO);
            so.FindProperty("groupId").stringValue = _groupId;
            so.FindProperty("description").stringValue = _groupDesc;
            so.FindProperty("cubeKind").enumValueIndex = (int)_cubeKind;

            var members = so.FindProperty("members");
            int count = _gridSouls.Count(s => s != null);
            members.arraySize = count;

            int mi = 0;
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                if (_gridSouls[i] == null) continue;
                var elem = members.GetArrayElementAtIndex(mi++);
                elem.FindPropertyRelative("gridPosition").vector2IntValue =
                    new Vector2Int(i % GridSize, i / GridSize);
                elem.FindPropertyRelative("soulDefinition").objectReferenceValue = _gridSouls[i];
                // bodyDefinitionはSoulDef.BodyDefinitionで管理するため保存不要
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(groupSO);
            AssetDatabase.SaveAssets();
            Status($"保存完了: {assetPath}", ColAccent);
            EditorGUIUtility.PingObject(groupSO);
        }

        private string GetFolder()
        {
            if (_saveFolder == null) { Status("保存先フォルダを設定してください", Color.yellow); return null; }
            var p = AssetDatabase.GetAssetPath(_saveFolder);
            if (!AssetDatabase.IsValidFolder(p)) { Status("フォルダが無効です", Color.red); return null; }
            return p;
        }

        private void ClearGrid()
        {
            _gridSouls = new SoulDefinitionSO[GridSize * GridSize];
            _selectedCell = -1;
            ClearEditors();
            Status("グリッドをクリアしました", Color.gray);
            Repaint();
        }

        private void Status(string msg, Color col) { _statusMsg = msg; _statusColor = col; Repaint(); }
    }
}
#endif