using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 転生シミュレーションの実行エントリーポイント。
    ///
    /// 役割：
    ///   1. 主魂・守護霊のSoulInstanceからReinSimInputを組み立てる
    ///   2. ReinSimulator.Run() でシミュを走らせる
    ///   3. 結果をOneReinSoulData.CreateFromArgs() でデータ化
    ///   4. 主魂のreinSoulsリストに追加（または上書き）する
    ///
    /// 使い方：
    ///   var reinData = ReinSimRunner.Run(mainSoul, guardians, allEvents);
    ///   // mainSoulのCurrentReinSoulが新しい転生データになっている
    /// </summary>
    public static class ReinSimRunner
    {
        /// <summary>
        /// 転生シミュを実行し、結果を主魂のOneReinSoulDataとして追加する。
        /// </summary>
        /// <param name="mainSoul">転生させる主魂</param>
        /// <param name="guardians">守護霊（最大3体、nullや3体未満でも可）</param>
        /// <param name="allEvents">使用するイベントSOの全リスト</param>
        /// <param name="jobDef">転生後のジョブ定義（nullの場合は主魂の現在ジョブを引き継ぐ）</param>
        /// <param name="replaceIndex">
        ///   -1（デフォルト）：reinSoulsの末尾に追加する。
        ///   0以上：指定インデックスのOneReinSoulDataを上書きする（転生スロット選択UI用）。
        /// </param>
        /// <returns>生成したOneReinSoulData。失敗時はnull。</returns>
        public static OneReinSoulData Run(
            SoulInstance mainSoul,
            SoulInstance[] guardians,
            IReadOnlyList<ReinLifeEventSO> allEvents,
            SoulJobDefinition jobDef = null,
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

            if (allEvents == null || allEvents.Count == 0)
            {
                Debug.LogWarning("[ReinSimRunner] allEvents が空です。イベントなしでシミュを実行します。");
            }

            // --------------------------------------------------
            // 2) ジョブ決定：引数優先、なければ主魂の現在ジョブ
            // --------------------------------------------------
            var finalJobDef = jobDef ?? mainSoul.Job;
            if (finalJobDef == null)
            {
                Debug.LogError("[ReinSimRunner] ジョブが決定できません。jobDef を指定してください。");
                return null;
            }

            // --------------------------------------------------
            // 3) ReinSimInput の組み立て
            // --------------------------------------------------
            var input = ReinSimInput.Build(
                main:      mainSoul,
                guardians: guardians,
                jobDef:    finalJobDef,
                talent:    mainSoul.Talent);

            // --------------------------------------------------
            // 4) シミュ実行
            // --------------------------------------------------
            var result = ReinSimulator.Run(input, allEvents ?? new List<ReinLifeEventSO>());

            Debug.Log($"[ReinSimRunner] シミュ完了: FinalRank={result.FinalRank}, " +
                      $"EventFactors=[{string.Join(", ", result.EventFactors):F2}], " +
                      $"Skills={result.LearnedSkillIds.Count}件, " +
                      $"Events={result.HistoryEvents.Count}件");

            // --------------------------------------------------
            // 5) OneReinSoulData の生成
            // --------------------------------------------------
            var reinData = OneReinSoulData.CreateFromArgs(
                rank:             result.FinalRank,
                growthType:       mainSoul.GrowthType,
                jobDef:           finalJobDef,
                talent:           mainSoul.Talent,
                title:            null,        // jobDefの名前が自動で入る
                level:            1,
                lv1Stats:         null,        // eventFactorsから自動計算
                growthTargets:    null,        // ランダム生成
                permanentBonuses: null,        // 0クリア
                historyEvents:    result.HistoryEvents,
                learnedSkillIds:  result.LearnedSkillIds,
                eventFactors:     result.EventFactors  // ★シミュ結果を反映
            );

            // --------------------------------------------------
            // 6) 主魂の reinSouls に書き込む
            // --------------------------------------------------
            ApplyToSoul(mainSoul, reinData, replaceIndex);

            return reinData;
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
