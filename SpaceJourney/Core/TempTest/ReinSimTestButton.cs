using System.Text;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 転生シミュレーションのテスト用 MonoBehaviour。
    /// 適当な GameObject に貼り、UI Buttonの OnClick に TestReincarnation() を登録する。
    ///
    /// 経路:
    ///   useFixedRankPath = true  → C経路（RunFixedRank）固定ランク量産
    ///   useFixedRankPath = false → B経路（Run）プレイヤー転生
    /// </summary>
    public class ReinSimTestButton : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField, Range(1, 10)] private int testRank = 3;
        [SerializeField] private SoulJobTendency testTendency = SoulJobTendency.Warrior;
        [SerializeField] private TalentRank testTalent = TalentRank.C;

        [Header("経路")]
        [Tooltip("true=C経路(固定ランク量産)、false=B経路(プレイヤー転生)")]
        [SerializeField] private bool useFixedRankPath = true;

        /// <summary>ボタンの OnClick から呼ぶ。</summary>
        public void TestReincarnation()
        {
            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[ReinSimTest] MasterDatabase が見つかりません。");
                return;
            }

            var allEvents = db.ReinLifeEvents;
            if (allEvents == null || allEvents.Count == 0)
            {
                Debug.LogError("[ReinSimTest] MasterDatabase.ReinLifeEvents が空です。ReinLifeEventBundle を確認してください。");
                return;
            }

            OneReinSoulData reinData;

            if (useFixedRankPath)
            {
                // ── C経路：固定ランク量産 ──
                reinData = ReinSimRunner.RunFixedRank(
                    fixedRank: testRank,
                    tendency: testTendency,
                    talentRank: testTalent,
                    allEvents: allEvents
                );
            }
            else
            {
                // ── B経路：プレイヤー転生（実在の主魂+守護霊） ──
                var mainSoul = SoulInstance.CreateRandomInitialSoul(testRank, testTendency, registerToWorld: false);

                var allTendencies = (SoulJobTendency[])System.Enum.GetValues(typeof(SoulJobTendency));
                var guardians = new SoulInstance[3];
                for (int i = 0; i < 3; i++)
                {
                    var t = allTendencies[UnityEngine.Random.Range(1, allTendencies.Length)]; // 0=None除外
                    guardians[i] = SoulInstance.CreateRandomInitialSoul(testRank, t, registerToWorld: false);
                }

                reinData = ReinSimRunner.Run(
                    mainSoul: mainSoul,
                    guardians: guardians,
                    allEvents: allEvents,
                    replaceIndex: -1
                );
            }

            if (reinData == null)
            {
                Debug.LogError("[ReinSimTest] シミュ実行に失敗しました。");
                return;
            }

            // ── 結果出力 ──
            DebugPrintReinData(reinData, useFixedRankPath ? "C経路 (RunFixedRank)" : "B経路 (Run)");
        }

        private static void DebugPrintReinData(OneReinSoulData reinData, string pathLabel)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== 転生シミュレーション結果 ===");
            sb.AppendLine($"経路       : {pathLabel}");
            sb.AppendLine($"ジョブ     : {reinData.JobDefinition?.JobName ?? "未確定"}");
            sb.AppendLine($"到達ランク : {reinData.Rank}");
            sb.AppendLine($"成長タイプ : {reinData.GrowthType}");
            sb.AppendLine();

            sb.AppendLine("--- Lv1 ステータス ---");
            string[] names = { "AT ", "DF ", "AGI", "MAT", "MDF" };
            StatKind[] kinds = { StatKind.AT, StatKind.DF, StatKind.AGI, StatKind.MAT, StatKind.MDF };
            for (int i = 0; i < 5; i++)
                sb.AppendLine($"  {names[i]}: {reinData.GetSoulStat(kinds[i]),4}");
            sb.AppendLine();

            if (reinData.LearnedSkillIds != null && reinData.LearnedSkillIds.Count > 0)
            {
                sb.AppendLine("--- 習得スキル ---");
                foreach (var sk in reinData.LearnedSkillIds)
                    sb.AppendLine($"  {sk}");
                sb.AppendLine();
            }

            sb.AppendLine($"--- 転生来歴 ({reinData.HistoryEvents?.Count ?? 0} 件) ---");
            if (reinData.HistoryEvents != null)
            {
                foreach (var ev in reinData.HistoryEvents)
                {
                    string tag = ev.EventType switch
                    {
                        ReinEventType.Birth => "[誕]",
                        ReinEventType.Happy => "[★]",
                        ReinEventType.Sad => "[涙]",
                        ReinEventType.Shock => "[!!]",
                        ReinEventType.RankUp => "[↑]",
                        ReinEventType.JobChange => "[転]",
                        ReinEventType.LifeEnd => "[終]",
                        _ => "   "
                    };
                    string ageStr = ev.HideAge ? "      " : $"{ev.Age,3}歳 ";
                    sb.AppendLine($"  {ageStr}{tag} {ev.Text}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
