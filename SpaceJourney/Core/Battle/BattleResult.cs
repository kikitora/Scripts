using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 戦闘終了後の結果データ。
    /// BattleManager.RunFullBattle() 後に生成し、
    /// 士気損失・VP(コアHP)ダメージ・経験値などの計算に使う。
    /// </summary>
    [Serializable]
    public class BattleResult
    {
        /// <summary>勝者 side (0=味方, 1=敵, -1=引き分け)</summary>
        public int winningSide;

        /// <summary>戦闘終了時のタイム</summary>
        public int endTime;

        /// <summary>各サイドの結果</summary>
        public SideResult[] sideResults;

        /// <summary>サイド別の結果</summary>
        [Serializable]
        public class SideResult
        {
            public int side;

            /// <summary>参加ユニット総数</summary>
            public int totalUnits;

            /// <summary>死亡ユニット数</summary>
            public int deadUnits;

            /// <summary>全滅したか</summary>
            public bool wiped;

            /// <summary>ダメージ割合 (0~1): (totalMaxHp - totalEndHp) / totalMaxHp</summary>
            public float dmgRatio;

            /// <summary>死亡ユニットのコスト合計 (現時点では死亡数で代用)</summary>
            public float totalDeadCost;

            /// <summary>各ユニットの最大HP合計</summary>
            public int totalMaxHp;

            /// <summary>各ユニットの残HP合計</summary>
            public int totalEndHp;
        }

        /// <summary>BattleManager の終了状態から BattleResult を生成する</summary>
        public static BattleResult FromManager(BattleManager manager)
        {
            var result = new BattleResult
            {
                winningSide = manager.WinningSide,
                endTime = manager.CurrentTime,
                sideResults = new SideResult[2],
            };

            for (int s = 0; s < 2; s++)
            {
                var all = manager.Field.GetAllUnits(s);
                var alive = manager.Field.GetAllAlive(s);

                int totalMax = all.Sum(u => u.MaxHp);
                int totalEnd = alive.Sum(u => u.CurrentHp);
                int deadCount = all.Count - alive.Count;

                result.sideResults[s] = new SideResult
                {
                    side = s,
                    totalUnits = all.Count,
                    deadUnits = deadCount,
                    wiped = alive.Count == 0,
                    totalMaxHp = totalMax,
                    totalEndHp = totalEnd,
                    dmgRatio = totalMax > 0 ? (float)(totalMax - totalEnd) / totalMax : 0f,
                    // コスト未実装のため、死亡数で代用 (将来: ユニットごとのコスト値を使う)
                    totalDeadCost = deadCount,
                };
            }

            return result;
        }

        /// <summary>
        /// 戦闘終了時の士気損失を計算する。
        /// 仕様: loss = dmgRatio×damageLossMax + baseMoraleLossPerCostOnDeath×totalDeadCost
        ///              + (wiped ? sideWipeMoraleLoss : 0) + participationLoss
        /// </summary>
        public static float CalcMoraleLoss(SideResult sideResult, SpaceJourneyBalanceConfig config)
        {
            float lossDamage = sideResult.dmgRatio * config.damageLossMax;
            float lossDeath = config.baseMoraleLossPerCostOnDeath * sideResult.totalDeadCost;
            float lossWipe = sideResult.wiped ? config.sideWipeMoraleLoss : 0f;
            float lossParticipation = config.participationLoss;

            return Mathf.Clamp(lossDamage + lossDeath + lossWipe + lossParticipation, 0f, 100f);
        }

        /// <summary>
        /// VP(コアHP)ダメージを計算する。
        /// 仕様: damage = baseCoreDamagePerCost×totalDeadCost
        ///              + (wiped ? baseSideWipeCoreDamagePerCost×totalSideCost : 0)
        /// </summary>
        public static int CalcVpDamage(SideResult sideResult, SpaceJourneyBalanceConfig config)
        {
            float dmgDeath = config.baseCoreDamagePerCost * sideResult.totalDeadCost;
            // 全滅ボーナス: サイド総コスト分。コスト未実装のためユニット数で代用
            float dmgWipe = sideResult.wiped
                ? config.baseSideWipeCoreDamagePerCost * sideResult.totalUnits
                : 0f;

            return Mathf.Max(0, Mathf.RoundToInt(dmgDeath + dmgWipe));
        }

        /// <summary>
        /// 戦闘結果を CubeInstance に反映する。
        /// side=0 (味方) の結果を指定された cubeSideNumber の士気・VP に適用。
        /// </summary>
        public static void ApplyToCube(
            CubeInstance cube,
            BattleResult result,
            int battleSide,
            int cubeSideNumber,
            SpaceJourneyBalanceConfig config)
        {
            var sideResult = result.sideResults[battleSide];
            var side = cube.GetSide(cubeSideNumber);

            // 士気損失 (Character面のみ。Tower/Empty面はスキップ)
            if (side != null && side.SideType == CubeSideType.Character)
            {
                float moraleLoss = CalcMoraleLoss(sideResult, config);
                side.Morale = Mathf.Max(0, side.Morale - Mathf.RoundToInt(moraleLoss));
            }

            // VP(コアHP)ダメージ
            int vpDamage = CalcVpDamage(sideResult, config);
            cube.Vp = Mathf.Max(0, cube.Vp - vpDamage);
        }
    }
}
