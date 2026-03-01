using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 転生シミュレーションの実行エントリーポイント。
    ///
    /// 役割：
    ///   1. 転生前に「運命のジョブ」を事前抽選する
    ///   2. 主魂・守護霊のSoulInstanceからReinSimInputを組み立てる
    ///   3. ReinSimulator.Run() でシミュを走らせる
    ///   4. 結果をOneReinSoulData.CreateFromArgs() でデータ化
    ///   5. 主魂のreinSoulsリストに追加（または上書き）する
    ///
    /// 使い方：
    ///   // 通常転生
    ///   var reinData = ReinSimRunner.Run(mainSoul, guardians, allEvents);
    ///   // 特別チケット転生（傾向無視）
    ///   var reinData = ReinSimRunner.Run(mainSoul, guardians, allEvents, useSpecialTicket: true);
    /// </summary>
    public static class ReinSimRunner
    {
        /// <summary>
        /// 転生シミュを実行し、結果を主魂のOneReinSoulDataとして追加する。
        /// </summary>
        /// <param name="mainSoul">転生させる主魂</param>
        /// <param name="guardians">守護霊（最大3体、nullや3体未満でも可）</param>
        /// <param name="allEvents">使用するイベントSOの全リスト</param>
        /// <param name="useSpecialTicket">
        ///   false（デフォルト）：通常転生。soulTendencyの傾向内からjobEasePercentで抽選。
        ///   true：特別チケット転生。傾向を無視して全ジョブからjobEasePercentで抽選。
        /// </param>
        /// <param name="replaceIndex">
        ///   -1（デフォルト）：reinSoulsの末尾に追加する。
        ///   0以上：指定インデックスのOneReinSoulDataを上書きする。
        /// </param>
        /// <returns>生成したOneReinSoulData。失敗時はnull。</returns>
        public static OneReinSoulData Run(
            SoulInstance mainSoul,
            SoulInstance[] guardians,
            IReadOnlyList<ReinLifeEventSO> allEvents,
            bool useSpecialTicket = false,
            int replaceIndex = -1)
        {
            // --------------------------------------------------
            // 1) バリデーション
            // --------------------------------------------------
            if (mainSoul == null)
            {
                Debug.LogError("[ReinSimRunner] mainSoul が null です。");
                return null;
            }

            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[ReinSimRunner] MasterDatabase が見つかりません。");
                return null;
            }

            if (allEvents == null || allEvents.Count == 0)
                Debug.LogWarning("[ReinSimRunner] allEvents が空です。イベントなしでシミュを実行します。");

            // --------------------------------------------------
            // 2) 運命のジョブを事前抽選
            //    通常：soulTendency内からjobEasePercentで抽選
            //    特別：全ジョブからjobEasePercentで抽選（傾向無視）
            // --------------------------------------------------
            SoulJobDefinition destJob;
            if (useSpecialTicket)
            {
                destJob = GetRandomJobFromAll(db);
                Debug.Log($"[ReinSimRunner] 特別チケット転生：全ジョブから抽選 → {destJob?.JobName}");
            }
            else
            {
                destJob = db.GetRandomSoulJobByTendency(mainSoul.SoulTendency);
                Debug.Log($"[ReinSimRunner] 通常転生：{mainSoul.SoulTendency}傾向から抽選 → {destJob?.JobName}");
            }

            if (destJob == null)
            {
                Debug.LogError("[ReinSimRunner] ジョブの抽選に失敗しました。");
                return null;
            }

            // --------------------------------------------------
            // 3) ReinSimInput の組み立て
            // --------------------------------------------------
            var input = ReinSimInput.Build(
                main: mainSoul,
                guardians: guardians,
                jobDef: destJob,
                talent: mainSoul.Talent);

            // --------------------------------------------------
            // 4) シミュ実行
            // --------------------------------------------------
            var result = ReinSimulator.Run(input, allEvents ?? new List<ReinLifeEventSO>());

            Debug.Log($"[ReinSimRunner] シミュ完了: FinalRank={result.FinalRank}, " +
                      $"Skills={result.LearnedSkillIds.Count}件, " +
                      $"Events={result.HistoryEvents.Count}件");

            // --------------------------------------------------
            // 5) OneReinSoulData の生成
            // --------------------------------------------------
            var reinData = OneReinSoulData.CreateFromArgs(
                rank: result.FinalRank,
                growthType: mainSoul.GrowthType,
                jobDef: destJob,
                talent: mainSoul.Talent,
                title: null,
                level: 1,
                lv1Stats: null,
                growthTargets: null,
                permanentBonuses: null,
                historyEvents: result.HistoryEvents,
                learnedSkillIds: result.LearnedSkillIds,
                eventFactors: result.EventFactors
            );

            // --------------------------------------------------
            // 6) デバッグ出力
            // --------------------------------------------------
            DebugPrint(mainSoul, destJob, result, reinData);

            // --------------------------------------------------
            // 7) 主魂の reinSouls に書き込む
            // --------------------------------------------------
            ApplyToSoul(mainSoul, reinData, replaceIndex);

            return reinData;
        }

        // ============================================================
        // 内部：全ジョブからjobEasePercentで重み付き抽選
        // ============================================================
        private static SoulJobDefinition GetRandomJobFromAll(MasterDatabase db)
        {
            var jobs = db.SoulJobDefinitions;
            if (jobs == null || jobs.Length == 0) return null;

            int total = 0;
            foreach (var j in jobs)
            {
                if (j != null) total += Mathf.Max(0, j.JobEasePercent);
            }

            if (total <= 0)
                return jobs[UnityEngine.Random.Range(0, jobs.Length)];

            int r = UnityEngine.Random.Range(0, total);
            foreach (var j in jobs)
            {
                if (j == null) continue;
                int w = Mathf.Max(0, j.JobEasePercent);
                if (r < w) return j;
                r -= w;
            }

            return jobs[jobs.Length - 1];
        }

        // ============================================================
        // 内部：デバッグ出力
        // ============================================================
        private static void DebugPrint(
            SoulInstance soul,
            SoulJobDefinition job,
            ReinSimResult simResult,
            OneReinSoulData reinData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== 転生シミュレーション結果 ===");
            sb.AppendLine($"ジョブ    : {job.JobName}（{soul.SoulTendency}系）");
            sb.AppendLine($"才能      : {soul.Talent}");
            sb.AppendLine($"到達ランク: {simResult.FinalRank}");
            sb.AppendLine();

            sb.AppendLine("--- Lv1 ステータス ---");
            string[] statNames = { "AT ", "DF ", "AGI", "MAT", "MDF" };
            StatKind[] kinds = { StatKind.AT, StatKind.DF, StatKind.AGI, StatKind.MAT, StatKind.MDF };
            for (int i = 0; i < 5; i++)
            {
                float evFactor = simResult.EventFactors[i];
                int ptApprox = Mathf.RoundToInt((evFactor - 1.0f) / 0.8f * 40f);
                int lv1 = reinData.GetSoulStat(kinds[i]);
                sb.AppendLine($"  {statNames[i]}: {lv1,4}  (eventFactor={evFactor:F3}x / ~{ptApprox}pt)");
            }
            sb.AppendLine();

            if (simResult.LearnedSkillIds.Count > 0)
            {
                sb.AppendLine("--- 習得スキル ---");
                foreach (var sk in simResult.LearnedSkillIds)
                    sb.AppendLine($"  {sk}");
                sb.AppendLine();
            }

            sb.AppendLine("--- 転生来歴 ---");
            foreach (var ev in simResult.HistoryEvents)
            {
                string tag = ev.EventType switch
                {
                    ReinEventType.RankUp => "[↑]",
                    ReinEventType.Happy => "[★]",
                    ReinEventType.Sad => "[▼]",
                    ReinEventType.JobChange => "[J]",
                    _ => "   "
                };
                sb.AppendLine($"  {ev.Age,3}歳 {tag} {ev.Text}");
            }

            Debug.Log(sb.ToString());
        }

        // ============================================================
        // 内部：OneReinSoulDataを主魂に適用
        // ============================================================
        private static void ApplyToSoul(
            SoulInstance mainSoul,
            OneReinSoulData reinData,
            int replaceIndex)
        {
            if (replaceIndex >= 0 && replaceIndex < mainSoul.ReinSouls.Count)
            {
                mainSoul.ReplaceReinSoul(replaceIndex, reinData);
                Debug.Log($"[ReinSimRunner] スロット{replaceIndex}を上書きしました。");
            }
            else
            {
                mainSoul.AddReinSoul(reinData);
                Debug.Log($"[ReinSimRunner] スロット{mainSoul.ReinSouls.Count - 1}に追加しました。");
            }
        }
    }
}