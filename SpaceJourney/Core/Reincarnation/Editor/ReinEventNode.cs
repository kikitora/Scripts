#if UNITY_EDITOR

using System.Collections.Generic;

using UnityEditor;

using UnityEditor.Experimental.GraphView;

using UnityEngine;

using UnityEngine.UIElements;



namespace SteraCube.SpaceJourney.Editor

{

    /// <summary>

    /// ReinEventGraphWindow内で1つのReinLifeEventSOをノードとして表示するクラス。

    ///

    /// ポート設計：

    ///   [RequiresIn]  ← 青ポート（入力）：「このイベントが起きた後に私が出現できる」

    ///   [RequiresOut] → 青ポート（出力）：「私が起きた後にあのイベントが出現できる」

    ///   [BlockedIn]   ← 赤ポート（入力）：「このイベントが起きたら私は出現しない」

    ///   [BlockedOut]  → 赤ポート（出力）：「私が起きたらあのイベントは出現しない」

    /// </summary>

    public class ReinEventNode : Node

    {

        // カラー定義

        private static readonly Color ColRequires = new Color(0.3f, 0.6f, 1.0f); // 青

        private static readonly Color ColBlocked = new Color(1.0f, 0.35f, 0.3f); // 赤

        private static readonly Color ColHeader = new Color(0.18f, 0.18f, 0.18f);



        public ReinLifeEventSO EventData { get; private set; }



        // ポート（接続の口）

        public Port PortRequiresIn { get; private set; } // 青・入力

        public Port PortRequiresOut { get; private set; } // 青・出力

        public Port PortBlockedIn { get; private set; } // 赤・入力

        public Port PortBlockedOut { get; private set; } // 赤・出力



        // ============================================================

        // 生成

        // ============================================================

        public static ReinEventNode Create(ReinLifeEventSO data)

        {

            var node = new ReinEventNode();

            node.Init(data);

            return node;

        }



        private void Init(ReinLifeEventSO data)

        {

            EventData = data;



            // ---- タイトル ----

            title = data.DisplayName;

            tooltip = data.EditorMemo;



            // ---- ヘッダ色 ----

            var headerBg = titleContainer;

            headerBg.style.backgroundColor = new StyleColor(ColHeader);



            // ---- 年齢バッジ ----

            var ageBadge = new Label($"Age {data.StartAge}?{data.EndAge}  w:{data.BaseWeight:F1}");

            ageBadge.style.fontSize = 10;

            ageBadge.style.color = new StyleColor(new Color(0.75f, 0.75f, 0.75f));

            ageBadge.style.unityTextAlign = TextAnchor.MiddleCenter;

            ageBadge.style.paddingBottom = 4;

            titleContainer.Add(ageBadge);



            // ---- 選択肢のプレビュー ----

            var optCount = new Label($"選択肢: {data.Options.Count}件");

            optCount.style.fontSize = 10;

            optCount.style.color = new StyleColor(new Color(0.7f, 0.9f, 0.7f));

            optCount.style.paddingLeft = 6;

            optCount.style.paddingBottom = 4;

            extensionContainer.Add(optCount);



            // ---- ダブルクリックでInspector ----

            this.RegisterCallback<MouseDownEvent>(evt =>

            {

                if (evt.clickCount == 2)

                    Selection.activeObject = EventData;

            });



            // ---- ポート生成 ----

            BuildPorts();



            // ---- ノード位置 ----

            SetPosition(new Rect(data.graphPosition, Vector2.zero));



            RefreshExpandedState();

            RefreshPorts();

        }



        private void BuildPorts()

        {

            // 青ポート（requires）

            PortRequiresIn = CreatePort(

                "前提（入力）", Direction.Input, Port.Capacity.Multi, ColRequires);

            PortRequiresOut = CreatePort(

                "前提（出力）", Direction.Output, Port.Capacity.Multi, ColRequires);



            // 赤ポート（blocked）

            PortBlockedIn = CreatePort(

                "排他（入力）", Direction.Input, Port.Capacity.Multi, ColBlocked);

            PortBlockedOut = CreatePort(

                "排他（出力）", Direction.Output, Port.Capacity.Multi, ColBlocked);



            inputContainer.Add(PortRequiresIn);

            inputContainer.Add(PortBlockedIn);

            outputContainer.Add(PortRequiresOut);

            outputContainer.Add(PortBlockedOut);

        }



        private Port CreatePort(string portName, Direction dir, Port.Capacity capacity, Color color)

        {

            var port = InstantiatePort(Orientation.Horizontal, dir, capacity, typeof(bool));

            port.portName = portName;

            port.portColor = color;

            return port;

        }



        // ============================================================

        // ノード位置の変化をSOに書き戻す

        // ============================================================

        public void SavePosition()

        {

            if (EventData == null) return;

            var pos = GetPosition();

            EventData.graphPosition = pos.position;

            EditorUtility.SetDirty(EventData);

        }



        // ============================================================

        // ノードのタイトルを最新データに更新

        // ============================================================

        public void Refresh()

        {

            if (EventData == null) return;

            title = EventData.DisplayName;

        }

    }

}

#endif