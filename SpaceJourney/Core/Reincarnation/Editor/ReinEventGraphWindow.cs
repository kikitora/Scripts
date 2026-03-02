#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney.Editor
{
    /// <summary>
    /// ReinLifeEventSO のイベント接続をノードグラフで表示・編集するウィンドウ。
    ///
    /// 使い方：Window > SteraCube > ReinEvent Graph
    ///
    /// 操作：
    ///   ・中ボタンドラッグ            ：キャンバス移動
    ///   ・ノード左クリック            ：選択（Inspector連動）
    ///   ・ノード右クリック            ：コンテキストメニュー
    ///   ・ポート（●）ドラッグ→ノード  ：接続追加（緑=requires / 赤=blocked）
    ///   ・接続ラベル右クリック        ：接続削除
    /// </summary>
    public class ReinEventGraphWindow : EditorWindow
    {
        // 定数
        private const float NODE_W = 170f;
        private const float NODE_H = 54f;
        private const float PORT_R = 7f;
        private const float TOOLBAR_H = 28f;

        private static readonly Color BG_COLOR = new Color(0.18f, 0.18f, 0.18f);
        private static readonly Color NODE_NORMAL = new Color(0.28f, 0.28f, 0.32f);
        private static readonly Color NODE_SEL = new Color(0.22f, 0.44f, 0.70f);
        private static readonly Color COL_REQ = new Color(0.25f, 0.85f, 0.45f);
        private static readonly Color COL_BLK = new Color(0.90f, 0.28f, 0.22f);

        // 状態
        private List<NodeData> _nodes = new();
        private Dictionary<string, NodeData> _byId = new();
        private NodeData _selected;
        private NodeData _portDragFrom;
        private bool _portDragIsReq;
        private Vector2 _portDragPos;
        private Vector2 _dragStart;
        private bool _panDragging;
        private string _search = "";

        // ================================================================
        [MenuItem("Window/SteraCube/ReinEvent Graph")]
        public static void Open()
        {
            var w = GetWindow<ReinEventGraphWindow>("ReinEvent Graph");
            w.minSize = new Vector2(700, 480);
            w.LoadAll();
        }

        private void OnEnable() => LoadAll();

        private void LoadAll()
        {
            _nodes.Clear();
            _byId.Clear();
            var guids = AssetDatabase.FindAssets("t:ReinLifeEventSO");
            float x = 40, y = 40;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ReinLifeEventSO>(path);
                if (so == null) continue;
                var nd = new NodeData
                {
                    So = so,
                    Id = so.EventId,
                    Label = string.IsNullOrEmpty(so.DisplayName) ? so.name : so.DisplayName,
                    Rect = new Rect(x, y, NODE_W, NODE_H),
                };
                _nodes.Add(nd);
                if (!string.IsNullOrEmpty(so.EventId)) _byId[so.EventId] = nd;
                x += NODE_W + 24;
                if (x > 1400) { x = 40; y += NODE_H + 32; }
            }
            Repaint();
        }

        // ================================================================
        private void OnGUI()
        {
            DrawToolbar();
            var cRect = new Rect(0, TOOLBAR_H, position.width, position.height - TOOLBAR_H);
            GUI.BeginClip(cRect);
            EditorGUI.DrawRect(new Rect(0, 0, cRect.width, cRect.height), BG_COLOR);

            DrawEdges();

            // ドラッグ中の仮線
            if (_portDragFrom != null)
            {
                DrawBezier(_portDragPos, Event.current.mousePosition,
                    _portDragIsReq ? COL_REQ : COL_BLK);
                Repaint();
            }

            BeginWindows();
            for (int i = 0; i < _nodes.Count; i++)
            {
                var nd = _nodes[i];
                if (!Matches(nd)) continue;
                GUI.color = nd == _selected ? NODE_SEL : NODE_NORMAL;
                nd.Rect = GUI.Window(i, nd.Rect, DrawNode, "");
                GUI.color = Color.white;
            }
            EndWindows();

            HandleInput();
            GUI.EndClip();
        }

        // ================================================================
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("再読込", EditorStyles.toolbarButton, GUILayout.Width(56)))
                    LoadAll();
                GUILayout.Label("検索:", EditorStyles.miniLabel, GUILayout.Width(34));
                _search = EditorGUILayout.TextField(_search,
                    EditorStyles.toolbarSearchField, GUILayout.Width(180));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"総件数: {_nodes.Count}   " +
                    "  ●緑=requires  ●赤=blocked",
                    EditorStyles.miniLabel);
            }
        }

        // ================================================================
        private void DrawNode(int id)
        {
            if (id >= _nodes.Count) return;
            var nd = _nodes[id];

            // ラベル
            EditorGUILayout.LabelField(nd.Label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(nd.Id, EditorStyles.miniLabel);

            // ポート描画
            DrawPortGUI(nd, true);
            DrawPortGUI(nd, false);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                _selected = nd;
                Selection.activeObject = nd.So;
                GUI.changed = true;
            }
            if (e.type == EventType.ContextClick)
            {
                ShowNodeMenu(nd); e.Use();
            }
            GUI.DragWindow();
        }

        private void DrawPortGUI(NodeData nd, bool isReq)
        {
            float px = isReq ? NODE_W * 0.28f : NODE_W * 0.72f;
            float py = NODE_H - PORT_R * 2 - 4;
            var r = new Rect(px - PORT_R, py, PORT_R * 2, PORT_R * 2);

            var prev = GUI.color;
            GUI.color = isReq ? COL_REQ : COL_BLK;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;

            var e = Event.current;
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
            {
                _portDragFrom = nd;
                _portDragIsReq = isReq;
                _portDragPos = new Vector2(nd.Rect.x + px, nd.Rect.y + py + PORT_R);
                e.Use();
            }
        }

        // ================================================================
        private void DrawEdges()
        {
            foreach (var nd in _nodes)
            {
                if (nd.So == null) continue;
                foreach (var id in nd.So.RequiresEventIds)
                    if (_byId.TryGetValue(id, out var src))
                        DrawEdge(src, nd, COL_REQ, true);
                foreach (var id in nd.So.BlockedByEventIds)
                    if (_byId.TryGetValue(id, out var src))
                        DrawEdge(src, nd, COL_BLK, false);
            }
        }

        private void DrawEdge(NodeData from, NodeData to, Color col, bool isReq)
        {
            var s = new Vector2(from.Rect.xMax, from.Rect.center.y);
            var e = new Vector2(to.Rect.xMin, to.Rect.center.y);
            DrawBezier(s, e, col);

            var mid = (s + e) * 0.5f;
            var lRect = new Rect(mid.x - 10, mid.y - 9, 20, 18);
            GUI.Label(lRect, isReq ? "R" : "B");

            if (Event.current.type == EventType.ContextClick &&
                new Rect(mid.x - 14, mid.y - 12, 28, 24).Contains(Event.current.mousePosition))
            {
                string fromId = from.So.EventId;
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent($"削除: [{(isReq ? "R" : "B")}] {from.Label}→{to.Label}"),
                    false, () =>
                    {
                        Undo.RecordObject(to.So, "Remove Connection");
                        if (isReq) to.So.Editor_RemoveRequiresEventId(fromId);
                        else to.So.Editor_RemoveBlockedByEventId(fromId);
                        EditorUtility.SetDirty(to.So);
                    });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private static void DrawBezier(Vector2 a, Vector2 b, Color col)
        {
            float t = Mathf.Abs(b.x - a.x) * 0.5f + 30f;
            Handles.DrawBezier(a, b, a + Vector2.right * t, b - Vector2.right * t, col, null, 2f);
        }

        // ================================================================
        private void HandleInput()
        {
            var e = Event.current;

            // ポートドロップ→接続作成
            if (_portDragFrom != null && e.type == EventType.MouseUp)
            {
                foreach (var nd in _nodes)
                {
                    if (nd == _portDragFrom || !nd.Rect.Contains(e.mousePosition)) continue;
                    Undo.RecordObject(nd.So, "Add Connection");
                    if (_portDragIsReq) nd.So.Editor_AddRequiresEventId(_portDragFrom.So.EventId);
                    else nd.So.Editor_AddBlockedByEventId(_portDragFrom.So.EventId);
                    EditorUtility.SetDirty(nd.So);
                    break;
                }
                _portDragFrom = null;
                e.Use();
                Repaint();
            }

            // 中ボタンパン
            if (e.type == EventType.MouseDown && e.button == 2)
            { _panDragging = true; _dragStart = e.mousePosition; }
            if (_panDragging && e.type == EventType.MouseDrag)
            {
                var d = e.mousePosition - _dragStart;
                _dragStart = e.mousePosition;
                foreach (var nd in _nodes) nd.Rect.position += d;
                Repaint();
            }
            if (e.type == EventType.MouseUp && e.button == 2)
                _panDragging = false;
        }

        private void ShowNodeMenu(NodeData nd)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Inspectorで開く"), false, () =>
            {
                Selection.activeObject = nd.So;
                EditorGUIUtility.PingObject(nd.So);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("requires接続を全削除"), false, () =>
            {
                Undo.RecordObject(nd.So, "Clear Requires");
                foreach (var id in nd.So.RequiresEventIds.ToList())
                    nd.So.Editor_RemoveRequiresEventId(id);
                EditorUtility.SetDirty(nd.So);
            });
            menu.AddItem(new GUIContent("blocked接続を全削除"), false, () =>
            {
                Undo.RecordObject(nd.So, "Clear Blocked");
                foreach (var id in nd.So.BlockedByEventIds.ToList())
                    nd.So.Editor_RemoveBlockedByEventId(id);
                EditorUtility.SetDirty(nd.So);
            });
            menu.ShowAsContext();
        }

        private bool Matches(NodeData nd) =>
            string.IsNullOrEmpty(_search) ||
            nd.Label.Contains(_search) || nd.Id.Contains(_search);

        // ================================================================
        private class NodeData
        {
            public ReinLifeEventSO So;
            public string Id;
            public string Label;
            public Rect Rect;
        }
    }
}
#endif