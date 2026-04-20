using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 戦闘ログを UI 上に流すコンポーネント。
    /// BattleManager.Log を監視し、差分を Text に追記する。
    /// </summary>
    public class BattleLogUI : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("ログを表示する TMP_Text (ScrollRect 内推奨)")]
        public TMP_Text logText;

        [Tooltip("スクロール追従用 (任意)")]
        public ScrollRect scrollRect;

        [Header("設定")]
        [Tooltip("表示する最大ログ行数 (古いものから削除)")]
        [Range(20, 500)]
        public int maxLines = 100;

        private readonly List<string> displayedLines = new();
        private int lastLogCount = 0;

        public void Clear()
        {
            displayedLines.Clear();
            lastLogCount = 0;
            if (logText != null) logText.text = "";
        }

        /// <summary>
        /// BattleManager.Log を読み、差分を追記する。
        /// ティックごとに TestBattleStarter から呼ぶ。
        /// </summary>
        public void SyncFromManager(BattleManager manager)
        {
            if (manager == null || logText == null) return;
            var log = manager.Log;
            if (log == null) return;

            for (int i = lastLogCount; i < log.Count; i++)
            {
                displayedLines.Add(log[i]);
            }
            lastLogCount = log.Count;

            // 古いログを切り詰め
            while (displayedLines.Count > maxLines)
                displayedLines.RemoveAt(0);

            logText.text = string.Join("\n", displayedLines);

            // 下へスクロール
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
