#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SteraCube.SpaceJourney.Editor
{
    /// <summary>
    /// 転生ライフイベントグラフエディタ。
    /// 開き方：メニュー Tools > SteraCube > Rein Event Graph
    ///
    /// 操作方法：
    ///   ・ノードをドラッグ       → 位置移動（SOに自動保存）
    ///   ・青ポート同士をドラッグ  → requires接続（前提条件）
    ///   ・赤ポート同士をドラッグ  → blockedBy接続（排他条件）
    ///   ・Deleteキー             → 選択中のノード/エッジを削除
    ///   ・スクロール             → ズーム
    ///   ・中ボタンドラッグ       → パン
    ///   ・ノードをダブルクリック  → Inspectorでイベント詳細を開く
    ///   ・右クリック             → コンテキストメニュー
    /// </summary>
    public class ReinEventGraphWindow : EditorWindow
    {
        private const string PrefsKeySaveFolder = "SteraCube_ReinGraph_SaveFolder";

        private ReinGraphView _graphView;

        /// <summary>新規SO保存先フォルダ（EditorPrefsで永続化）</summary>
        private string SaveFolder
        {
            get => EditorPrefs.GetString(PrefsKeySaveFolder, "Assets");
            set => EditorPrefs.SetString(PrefsKeySaveFolder, value);
        }

        [MenuItem("Tools/SteraCube/Rein Event Graph")]
        public static void Open()
        {
            var window = GetWindow<ReinEventGraphWindow>("Rein Event Graph");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        private void OnEnable()
        {
            BuildUI();
            LoadAllEventsFromProject();
        }

        private void OnDisable()
        {
            SaveAllPositions();
        }

        // ============================================================
        // UI構築
        // ============================================================
        private void BuildUI()
        {
            // 1行目：操作ボタン
            var toolbar1 = new IMGUIContainer(DrawToolbarButtons);
            toolbar1.style.height = 26;
            rootVisualElement.Add(toolbar1);

            // 2行目：保存先フォルダ
            var toolbar2 = new IMGUIContainer(DrawToolbarFolder);
            toolbar2.style.height = 22;
            rootVisualElement.Add(toolbar2);

            // GraphView本体
            _graphView = new ReinGraphView(this);
            _graphView.style.flexGrow = 1;
            rootVisualElement.Add(_graphView);
        }

        private void DrawToolbarButtons()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("全イベントを読み込む", EditorStyles.toolbarButton, GUILayout.Width(140)))
                LoadAllEventsFromProject();

            if (GUILayout.Button("位置を保存", EditorStyles.toolbarButton, GUILayout.Width(80)))
                SaveAllPositions();

            GUILayout.Space(10);

            if (GUILayout.Button("＋ 新規イベントSO作成", EditorStyles.toolbarButton, GUILayout.Width(150)))
                CreateNewEventSO();

            GUILayout.FlexibleSpace();

            var origColor = GUI.color;
            GUI.color = new Color(0.5f, 0.7f, 1.0f);
            GUILayout.Label("■ 青=前提条件", EditorStyles.toolbarButton);
            GUI.color = new Color(1.0f, 0.5f, 0.4f);
            GUILayout.Label("■ 赤=排他条件", EditorStyles.toolbarButton);
            GUI.color = origColor;

            GUILayout.EndHorizontal();
        }

        private void DrawToolbarFolder()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("新規SO保存先:", EditorStyles.miniLabel, GUILayout.Width(80));

            // パスを表示（読み取り専用）
            var pathStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.7f, 0.9f, 1.0f) }
            };
            GUILayout.Label(SaveFolder, pathStyle, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("変更...", EditorStyles.toolbarButton, GUILayout.Width(55)))
                ChangeSaveFolder();

            if (GUILayout.Button("Assets直下に戻す", EditorStyles.toolbarButton, GUILayout.Width(110)))
                SaveFolder = "Assets";

            GUILayout.EndHorizontal();
        }

        // ============================================================
        // 保存先フォルダ変更
        // ============================================================
        private void ChangeSaveFolder()
        {
            var path = EditorUtility.OpenFolderPanel(
                "新規イベントSOの保存先フォルダを選択",
                SaveFolder, "");

            if (string.IsNullOrEmpty(path)) return;

            if (path.StartsWith(Application.dataPath))
            {
                SaveFolder = "Assets" + path.Substring(Application.dataPath.Length);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "フォルダ選択エラー",
                    "Assetsフォルダ以下のフォルダを選択してください。",
                    "OK");
            }
        }

        // ============================================================
        // 全SOの読み込み・グラフ再構築
        // ============================================================
        private void LoadAllEventsFromProject()
        {
            if (_graphView == null) return;
            _graphView.ClearGraph();
            _graphView.BuildGraph(LoadAllEventSOs());
        }

        private List<ReinLifeEventSO> LoadAllEventSOs()
        {
            var guids = AssetDatabase.FindAssets("t:ReinLifeEventSO");
            var result = new List<ReinLifeEventSO>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ReinLifeEventSO>(path);
                if (so != null) result.Add(so);
            }
            return result;
        }

        // ============================================================
        // 位置保存
        // ============================================================
        public void SaveAllPositions()
        {
            if (_graphView == null) return;
            _graphView.SaveAllNodePositions();
            AssetDatabase.SaveAssets();
        }

        // ============================================================
        // 新規SO作成（public：GraphViewの右クリックからも呼ぶ）
        // ============================================================
        public void CreateNewEventSO()
        {
            // 保存先フォルダの存在確認
            if (!AssetDatabase.IsValidFolder(SaveFolder))
            {
                EditorUtility.DisplayDialog(
                    "保存先フォルダが見つかりません",
                    $"「{SaveFolder}」は存在しません。\nツールバーの「変更...」で有効なフォルダを設定してください。",
                    "OK");
                return;
            }

            var path = EditorUtility.SaveFilePanelInProject(
                "新規 ReinLifeEventSO を作成",
                "ReinLifeEvent_New",
                "asset",
                "ファイル名と保存先を確認してください",
                SaveFolder);

            if (string.IsNullOrEmpty(path)) return;

            // 次回のデフォルトフォルダを記憶
            SaveFolder = System.IO.Path.GetDirectoryName(path);

            var so = ScriptableObject.CreateInstance<ReinLifeEventSO>();
            so.graphPosition = new Vector2(200, 200);

            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _graphView?.AddNode(so);

            // 作成直後にInspectorで開く
            Selection.activeObject = so;

            Debug.Log($"[ReinEventGraph] 新規イベントSOを作成しました: {path}");
        }
    }

    // ================================================================
    // ReinGraphView：GraphView本体
    // ================================================================
    public class ReinGraphView : GraphView
    {
        private readonly ReinEventGraphWindow _window;
        private readonly Dictionary<ReinLifeEventSO, ReinEventNode> _nodeMap
            = new Dictionary<ReinLifeEventSO, ReinEventNode>();

        public ReinGraphView(ReinEventGraphWindow window)
        {
            _window = window;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            this.StretchToParentSize();

            this.RegisterCallback<ContextualMenuPopulateEvent>(BuildContextMenu);
            graphViewChanged += OnGraphChanged;
        }

        // ============================================================
        // グラフ構築
        // ============================================================
        public void ClearGraph()
        {
            foreach (var elem in graphElements.ToList())
                RemoveElement(elem);
            _nodeMap.Clear();
        }

        public void BuildGraph(List<ReinLifeEventSO> events)
        {
            foreach (var ev in events)
                _nodeMap[ev] = CreateAndAddNode(ev);

            foreach (var ev in events)
            {
                if (!_nodeMap.TryGetValue(ev, out var fromNode)) continue;

                foreach (var req in ev.RequiresEvents)
                {
                    if (req == null || !_nodeMap.TryGetValue(req, out var reqNode)) continue;
                    var edge = reqNode.PortRequiresOut.ConnectTo(fromNode.PortRequiresIn);
                    edge.edgeControl.edgeWidth = 2;
                    AddElement(edge);
                }

                foreach (var blk in ev.BlockedByEvents)
                {
                    if (blk == null || !_nodeMap.TryGetValue(blk, out var blkNode)) continue;
                    var edge = blkNode.PortBlockedOut.ConnectTo(fromNode.PortBlockedIn);
                    edge.edgeControl.edgeWidth = 2;
                    AddElement(edge);
                }
            }
        }

        public void AddNode(ReinLifeEventSO so)
        {
            if (_nodeMap.ContainsKey(so)) return;
            _nodeMap[so] = CreateAndAddNode(so);
        }

        private ReinEventNode CreateAndAddNode(ReinLifeEventSO so)
        {
            var node = ReinEventNode.Create(so);
            AddElement(node);
            return node;
        }

        // ============================================================
        // 接続可否ルール
        // ============================================================
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var result = new List<Port>();
            bool startIsRequires = IsRequiresPort(startPort);

            foreach (var port in ports.ToList())
            {
                if (port.node == startPort.node) continue;
                if (port.direction == startPort.direction) continue;
                if (IsRequiresPort(port) != startIsRequires) continue;
                result.Add(port);
            }
            return result;
        }

        private bool IsRequiresPort(Port port)
        {
            if (port.node is not ReinEventNode node) return false;
            return port == node.PortRequiresIn || port == node.PortRequiresOut;
        }

        // ============================================================
        // 接続変更時にSOを更新
        // ============================================================
        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
                foreach (var edge in change.edgesToCreate)
                    ApplyEdgeCreation(edge);

            if (change.elementsToRemove != null)
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is Edge edge) ApplyEdgeRemoval(edge);
                    if (elem is ReinEventNode n) _nodeMap.Remove(n.EventData);
                }

            if (change.movedElements != null)
                foreach (var elem in change.movedElements)
                    if (elem is ReinEventNode node)
                        node.SavePosition();

            return change;
        }

        private void ApplyEdgeCreation(Edge edge)
        {
            var outputNode = edge.output?.node as ReinEventNode;
            var inputNode = edge.input?.node as ReinEventNode;
            if (outputNode == null || inputNode == null) return;

            if (IsRequiresPort(edge.output))
                inputNode.EventData.Editor_AddRequires(outputNode.EventData);
            else
                inputNode.EventData.Editor_AddBlockedBy(outputNode.EventData);
        }

        private void ApplyEdgeRemoval(Edge edge)
        {
            var outputNode = edge.output?.node as ReinEventNode;
            var inputNode = edge.input?.node as ReinEventNode;
            if (outputNode == null || inputNode == null) return;

            if (IsRequiresPort(edge.output))
                inputNode.EventData.Editor_RemoveRequires(outputNode.EventData);
            else
                inputNode.EventData.Editor_RemoveBlockedBy(outputNode.EventData);
        }

        public void SaveAllNodePositions()
        {
            foreach (var node in _nodeMap.Values)
                node.SavePosition();
        }

        // ============================================================
        // 右クリックメニュー（バグ修正済み）
        // ============================================================
        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            // ★修正：SaveAllPositions() ではなく CreateNewEventSO() を呼ぶ
            evt.menu.AppendAction("＋ 新規イベントSO作成", _ => _window.CreateNewEventSO());

            evt.menu.AppendAction("全イベントを再読み込み", _ =>
            {
                ClearGraph();
                var guids = AssetDatabase.FindAssets("t:ReinLifeEventSO");
                var all = new List<ReinLifeEventSO>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var so = AssetDatabase.LoadAssetAtPath<ReinLifeEventSO>(path);
                    if (so != null) all.Add(so);
                }
                BuildGraph(all);
            });

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("選択ノードをInspectorで開く", _ =>
            {
                foreach (var elem in selection)
                    if (elem is ReinEventNode node)
                        Selection.activeObject = node.EventData;
            });

            evt.menu.AppendAction("現在の位置を保存", _ => _window.SaveAllPositions());
        }
    }
}
#endif