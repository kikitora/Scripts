#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public class BattleTestWindow : EditorWindow
    {
        [MenuItem("SteraCube/Battle Test")]
        public static void ShowWindow() => GetWindow<BattleTestWindow>("Battle Test");

        private int allyRank = 3, allyLevel = 10, enemyRank = 3, enemyLevel = 10;
        private int allyCount = 3, enemyCount = 3;
        private List<string> battleLog = new();
        private Vector2 logScroll;

        private void OnGUI()
        {
            GUILayout.Label("=== Battle Test ===", EditorStyles.boldLabel);

            // パラメータ
            EditorGUILayout.BeginHorizontal();
            DrawTeamParams("味方", ref allyRank, ref allyLevel, ref allyCount);
            GUILayout.Space(10);
            DrawTeamParams("敵", ref enemyRank, ref enemyLevel, ref enemyCount);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 実行
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⚔ 戦闘実行", GUILayout.Height(35)))
            {
                battleLog = TestBattleRunner.RunTestBattle(allyRank, allyLevel, enemyRank, enemyLevel, allyCount, enemyCount);
                SaveLog();
            }
            if (GUILayout.Button("📋 ログ保存", GUILayout.Height(35), GUILayout.Width(80)))
                SaveLog();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // フィールド
            DrawBattleField();

            EditorGUILayout.Space(5);

            // ログ
            DrawLog();
        }

        private void DrawTeamParams(string label, ref int rank, ref int level, ref int count)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.Label(label, EditorStyles.miniBoldLabel);
            rank = EditorGUILayout.IntSlider("Rank", rank, 1, 10);
            level = EditorGUILayout.IntSlider("Level", level, 1, 25);
            count = EditorGUILayout.IntSlider("人数", count, 1, 9);
            EditorGUILayout.EndVertical();
        }

        private void DrawBattleField()
        {
            var mgr = TestBattleRunner.LastManager;
            if (mgr == null) return;

            var field = mgr.Field;
            string result = mgr.WinningSide == 0 ? "味方勝利" : mgr.WinningSide == 1 ? "敵勝利" : "引き分け";
            GUILayout.Label($"フィールド結果: {result} (t={mgr.CurrentTime})", EditorStyles.miniBoldLabel);

            // セル範囲を算出
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var cell in field.Cells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.y < minY) minY = cell.y;
                if (cell.y > maxY) maxY = cell.y;
            }

            EditorGUILayout.BeginHorizontal();

            // 味方 (右が前列)
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("← 後列   味方   前列 →", EditorStyles.centeredGreyMiniLabel);
            for (int y = minY; y <= maxY; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = maxX; x >= minX; x--)
                    DrawUnitCell(field, mgr, 0, x, y, new Color(0.2f, 0.5f, 0.2f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Label("VS", EditorStyles.boldLabel, GUILayout.Width(25));

            // 敵 (左が前列)
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("← 前列   敵   後列 →", EditorStyles.centeredGreyMiniLabel);
            for (int y = minY; y <= maxY; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = minX; x <= maxX; x++)
                    DrawUnitCell(field, mgr, 1, x, y, new Color(0.5f, 0.2f, 0.2f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawUnitCell(BattleField field, BattleManager mgr, int side, int x, int y, Color baseColor)
        {
            var cell = new Vector2Int(x, y);
            var oldBg = GUI.backgroundColor;
            var style = new GUIStyle(GUI.skin.box) { fontSize = 9, alignment = TextAnchor.MiddleCenter, richText = true };

            if (!field.IsValidCell(cell))
            {
                GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
                GUILayout.Box("", style, GUILayout.Width(65), GUILayout.Height(45));
                GUI.backgroundColor = oldBg;
                return;
            }

            var unit = field.GetUnit(side, cell);

            if (unit == null)
            {
                GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
                GUILayout.Box("", style, GUILayout.Width(65), GUILayout.Height(45));
            }
            else if (unit.IsDead)
            {
                GUI.backgroundColor = new Color(0.3f, 0.15f, 0.15f);
                var state = mgr.GetUnitState(unit);
                string job = unit.Body?.BodyJobId ?? "?";
                GUILayout.Box($"<color=#ff4444><b>DEAD</b></color>\n{job}", style, GUILayout.Width(65), GUILayout.Height(45));
            }
            else
            {
                float hpRate = (float)unit.CurrentHp / Mathf.Max(1, unit.MaxHp);
                GUI.backgroundColor = Color.Lerp(new Color(0.5f, 0.15f, 0.15f), baseColor, hpRate);
                string job = unit.Body?.BodyJobId ?? "?";
                string hp = $"{unit.CurrentHp}/{unit.MaxHp}";
                string bar = MakeBar(hpRate, 6);
                GUILayout.Box($"<b>{job}</b>\n{hp}\n{bar}", style, GUILayout.Width(65), GUILayout.Height(45));
            }
            GUI.backgroundColor = oldBg;
        }

        private static string MakeBar(float rate, int len)
        {
            int filled = Mathf.RoundToInt(rate * len);
            string color = rate > 0.5f ? "#44ff44" : rate > 0.25f ? "#ffcc00" : "#ff4444";
            return $"<color={color}>{"■".PadRight(filled, '■')}</color>{"□".PadRight(len - filled, '□')}";
        }

        private void DrawLog()
        {
            GUILayout.Label("戦闘ログ", EditorStyles.miniBoldLabel);
            logScroll = EditorGUILayout.BeginScrollView(logScroll, GUILayout.Height(350));

            var style = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, fontSize = 12 };
            var bigStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, fontSize = 13, fontStyle = FontStyle.Bold };

            foreach (var line in battleLog)
            {
                string c = line;
                bool useBig = false;

                if (line.Contains("★撃破"))
                {
                    c = $"<color=#ff0000><b>  💀 {line}</b></color>";
                    useBig = true;
                }
                else if (line.Contains("勝者"))
                {
                    c = $"<color=#ffcc00><b>  🏆 {line}</b></color>";
                    useBig = true;
                }
                else if (line.Contains("戦闘終了") || line.Contains("戦闘開始"))
                {
                    c = $"<color=#ffcc00><b>{line}</b></color>";
                    useBig = true;
                }
                else if (line.Contains("--- t="))
                {
                    c = $"\n<color=#00bbbb><b>{'─'.ToString().PadRight(40, '─')}\n{line}\n{'─'.ToString().PadRight(40, '─')}</b></color>";
                    useBig = true;
                }
                else if (line.Contains("「") && line.Contains("[敵"))
                    c = $"<color=#cc2222>  ⚔ {line}</color>";
                else if (line.Contains("「") && line.Contains("[味"))
                    c = $"<color=#2255cc>  ⚔ {line}</color>";
                else if (line.Contains("待機"))
                    c = $"<color=#666666>  ⏳ {line}</color>";
                else if (line.Contains("行動不能"))
                    c = $"<color=#cc8800>  ⛔ {line}</color>";
                else if (line.Contains("回復"))
                    c = $"<color=#22aa22>  💚 {line}</color>";
                else if (line.Contains("HP残量"))
                    c = $"<color=#cccccc><b>{line}</b></color>";

                EditorGUILayout.LabelField(c, useBig ? bigStyle : style);
            }
            EditorGUILayout.EndScrollView();
        }

        private void SaveLog()
        {
            if (battleLog == null || battleLog.Count == 0) return;
            string path = System.IO.Path.Combine(Application.dataPath, "../__battle_log.txt");
            System.IO.File.WriteAllLines(path, battleLog, System.Text.Encoding.UTF8);
            Debug.Log($"[BattleTest] ログ保存: {path}");
        }
    }
}
#endif
