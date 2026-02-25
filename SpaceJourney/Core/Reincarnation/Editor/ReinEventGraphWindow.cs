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
    ///
    /// 開き方：メニュー SteraCube > Rein Event Graph
    ///
    /// 操作方法：
    ///   ・ノードをドラッグ      → 位置移動（SOに自動保存）
    ///   ・青ポート同士をドラッグ → requires接続（前提条件）
    ///   ・赤ポート同士をドラッグ → blockedBy接続（排他条件）
    ///   ・Deleteキー            → 選択中のノード/エッジを削除
    ///   ・スクロール            → ズーム
    ///   ・中ボタンドラッグ      → パン
    ///   ・ダブルクリック        → Inspectorでイベント詳細を開く
    ///   ・右クリック           → コンテキストメニュー（新規ノード作成など）
    /// </summary>
    public class ReinEventGraphWindow : EditorWindow
    {
        private ReinGraphView _graphView;
        private string _lastLoadedFolder = "Assets";

        [MenuItem("Tools/SteraCube/Rein Event Graph")]
        public static void Open()
        {
            var window = GetWindow<ReinEventGraphWindow>("Rein Event Graph");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        // ============================================================
        // ライフサイクル
        // ============================================================
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
            // ツールバー
            var toolbar = new IMGUIContainer(DrawToolbar);
            toolbar.style.height = 26;
            rootVisualElement.Add(toolbar);

            // GraphView本体
            _graphView = new ReinGraphView(this);
            _graphView.style.flexGrow = 1;
            rootVisualElement.Add(_graphView);
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("全イベントを読み込む", EditorStyles.toolbarButton, GUILayout.Width(140)))
                LoadAllEventsFromProject();

            if (GUILayout.Button("位置を保存", EditorStyles.toolbarButton, GUILayout.Width(80)))
                SaveAllPositions();

            GUILayout.Space(10);

            if (GUILayout.Button("新規イベントSO作成", EditorStyles.toolbarButton, GUILayout.Width(140)))
                CreateNewEventSO();

            GUILayout.FlexibleSpace();

            // 凡例
            var origColor = GUI.color;
            GUI.color = new Color(0.5f, 0.7f, 1.0f);
            GUILayout.Label("■ 青=前提", EditorStyles.toolbarButton);
            GUI.color = new Color(1.0f, 0.5f, 0.4f);
            GUILayout.Label("■ 赤=排他", EditorStyles.toolbarButton);
            GUI.color = origColor;

            GUILayout.EndHorizontal();
        }

        // ============================================================
        // 全SOの読み込み・グラフ再構築
        // ============================================================
        private void LoadAllEventsFromProject()
        {
            if (_graphView == null) return;
            _graphView.ClearGraph();

            var allEvents = LoadAllEventSOs();
            _graphView.BuildGraph(allEvents);
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
        // 新規SO作成
        // ============================================================
        private void CreateNewEventSO()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "新規 ReinLifeEventSO を作成",
                "ReinLifeEvent_New",
                "asset",
                "保存先を選んでください",
                _lastLoadedFolder);

            if (string.IsNullOrEmpty(path)) return;
            _lastLoadedFolder = System.IO.Path.GetDirectoryName(path);

            var so = ScriptableObject.CreateInstance<ReinLifeEventSO>();
            // グラフの中央あたりに配置
            so.graphPosition = _graphView == null
                ? new Vector2(200, 200)
                : new Vector2(
                    _graphView.contentContainer.layout.width * 0.5f,
                    _graphView.contentContainer.layout.height * 0.5f);

            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();

            // グラフへ追加
            _graphView?.AddNode(so);
        }
    }

    // ================================================================
    // ReinGraphView：GraphView本体
    // ================================================================
    public class ReinGraphView : GraphView
    {
        private readonly ReinEventGraphWindow _window;

        // SO → ノードの対応表
        private readonly Dictionary<ReinLifeEventSO, ReinEventNode> _nodeMap
            = new Dictionary<ReinLifeEventSO, ReinEventNode>();

        public ReinGraphView(ReinEventGraphWindow window)
        {
            _window = window;

            // 基本操作を有効化
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // 背景グリッド
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            // スタイル
            this.StretchToParentSize();

            // 右クリックメニュー
            this.RegisterCallback<ContextualMenuPopulateEvent>(BuildContextMenu);

            // 接続変更コールバック
            graphViewChanged += OnGraphChanged;
        }

        // ============================================================
        // グラフ構築
        // ============================================================
        public void ClearGraph()
        {
            // 全要素削除
            foreach (var elem in graphElements.ToList())
                RemoveElement(elem);
            _nodeMap.Clear();
        }

        public void BuildGraph(List<ReinLifeEventSO> events)
        {
            // 1) ノード生成
            foreach (var ev in events)
            {
                var node = CreateAndAddNode(ev);
                _nodeMap[ev] = node;
            }

            // 2) エッジ生成（ノードが全部できてから）
            foreach (var ev in events)
            {
                if (!_nodeMap.TryGetValue(ev, out var fromNode)) continue;

                // 青：requires
                foreach (var req in ev.RequiresEvents)
                {
                    if (req == null) continue;
                    if (!_nodeMap.TryGetValue(req, out var toNode)) continue;

                    // 「reqが起きた後にevが出られる」
                    // toNode(req) の RequiresOut → fromNode(ev) の RequiresIn
                    var edge = toNode.PortRequiresOut.ConnectTo(fromNode.PortRequiresIn);
                    edge.edgeControl.edgeWidth = 2;
                    AddElement(edge);
                }

                // 赤：blockedBy
                foreach (var blk in ev.BlockedByEvents)
                {
                    if (blk == null) continue;
                    if (!_nodeMap.TryGetValue(blk, out var toNode)) continue;

                    // 「blkが起きたらevは出られない」
                    // toNode(blk) の BlockedOut → fromNode(ev) の BlockedIn
                    var edge = toNode.PortBlockedOut.ConnectTo(fromNode.PortBlockedIn);
                    edge.edgeControl.edgeWidth = 2;
                    AddElement(edge);
                }
            }
        }

        public void AddNode(ReinLifeEventSO so)
        {
            if (_nodeMap.ContainsKey(so)) return;
            var node = CreateAndAddNode(so);
            _nodeMap[so] = node;
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

            // 同じノードのポートには繋げない
            // 青同士・赤同士のみ許可
            foreach (var port in ports.ToList())
            {
                if (port.node == startPort.node) continue;
                if (port.direction == startPort.direction) continue;

                bool startIsRequires = IsRequiresPort(startPort);
                bool portIsRequires = IsRequiresPort(port);

                // 色が同じ（requires同士 or blocked同士）のみ許可
                if (startIsRequires == portIsRequires)
                    result.Add(port);
            }

            return result;
        }

        private bool IsRequiresPort(Port port)
        {
            return port == ((ReinEventNode)port.node).PortRequiresIn
                || port == ((ReinEventNode)port.node).PortRequiresOut;
        }

        // ============================================================
        // 接続変更時にSOを更新
        // ============================================================
        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            // エッジ追加
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                    ApplyEdgeCreation(edge);
            }

            // エッジ/ノード削除
            if (change.elementsToRemove != null)
            {
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is Edge edge)
                        ApplyEdgeRemoval(edge);
                    else if (elem is ReinEventNode node)
                        _nodeMap.Remove(node.EventData);
                }
            }

            // ノード移動 → 位置保存
            if (change.movedElements != null)
            {
                foreach (var elem in change.movedElements)
                {
                    if (elem is ReinEventNode node)
                        node.SavePosition();
                }
            }

            return change;
        }

        private void ApplyEdgeCreation(Edge edge)
        {
            var outputNode = edge.output?.node as ReinEventNode;
            var inputNode = edge.input?.node as ReinEventNode;
            if (outputNode == null || inputNode == null) return;

            if (IsRequiresPort(edge.output))
            {
                // 青：inputNode.requires に outputNode を追加
                inputNode.EventData.Editor_AddRequires(outputNode.EventData);
            }
            else
            {
                // 赤：inputNode.blockedBy に outputNode を追加
                inputNode.EventData.Editor_AddBlockedBy(outputNode.EventData);
            }
        }

        private void ApplyEdgeRemoval(Edge edge)
        {
            var outputNode = edge.output?.node as ReinEventNode;
            var inputNode = edge.input?.node as ReinEventNode;
            if (outputNode == null || inputNode == null) return;

            if (IsRequiresPort(edge.output))
            {
                inputNode.EventData.Editor_RemoveRequires(outputNode.EventData);
            }
            else
            {
                inputNode.EventData.Editor_RemoveBlockedBy(outputNode.EventData);
            }
        }

        // ============================================================
        // 全ノードの位置をSOに書き戻す
        // ============================================================
        public void SaveAllNodePositions()
        {
            foreach (var node in _nodeMap.Values)
                node.SavePosition();
        }

        // ============================================================
        // 右クリックメニュー
        // ============================================================
        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("新規イベントSO作成", _ => _window.SaveAllPositions());
            evt.menu.AppendAction("全イベントを再読み込み", _ =>
            {
                ClearGraph();
                var all = new List<ReinLifeEventSO>();
                var guids = AssetDatabase.FindAssets("t:ReinLifeEventSO");
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
                {
                    if (elem is ReinEventNode node)
                        Selection.activeObject = node.EventData;
                }
            });
        }
    }
}
#endif