using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    // このクラス群は「条件（Condition）」の表現と評価を担当します。
    // 「いつチェックするか（Timing）」は別レイヤー（BattleEventBus + SkillDefinition.passiveTimings）で管理し、
    // ここは “条件だけ” に集中させます。

    /// <summary>
    /// 条件1つ分。ANDで複数組み合わせる。
    /// </summary>
    [Serializable]
    public struct SkillOccasionCondition
    {
        public SkillConditionKind kind;

        [Tooltip("整数パラメータ（例：EnemyCountAtLeastのN）。")]
        public int intParam;

        [Tooltip("割合パラメータ（0〜1）。例：HP%の閾値。")]
        [Range(0f, 1f)]
        public float rateParam;

        [Tooltip("ビットフラグ/タグ用。UsedSkillHasTag などで使用。")]
        public SkillTag tagParam;

        [Tooltip("必要なら使う汎用bool。")]
        public bool boolParam;
    }

    /// <summary>
    /// 戦闘イベントの文脈（BattleEventBusが流すもの）
    /// 条件判定に必要な “値” を詰めるだけ。ゲーム実装の具体クラスに依存しない方向。
    /// </summary>
    public struct SkillTriggerContext
    {
        public SkillTriggerTiming timing;

        // Action系の詳細
        public ActionEventPhase actionPhase;

        // HP系の詳細
        public HpDeltaSourceType hpSource;
        public int hpDelta; // damage=+ / heal=- （君の既存流儀に合わせるならdamage+でOK）

        // 状態付与試行の詳細（StatusAttemptEvent用）
        public StatusEffectType statusEffectType;
        public int statusValue;
        public int statusDuration;

        // 関係者（必要なときだけ入れる）
        public SpaceJourneyUnit self;
        public SpaceJourneyUnit other;           // 例：攻撃者/被攻撃者、付与者/被付与者など
        public SkillDefinition usedSkill;        // 原因になったスキル（無ければnull）

        // よく使う評価用の値（無ければデフォルト0/falseでOK）
        [Range(0f, 1f)] public float selfHpRate;
        [Range(0f, 1f)] public float otherHpRate;
        public bool selfMovedThisTime;

        public bool otherHasAnyStatus;
        public bool otherHasDebuff;

        public int enemyCount;
        public int allyCount;

        public bool usedSkillIsBasic;
        public SkillTag usedSkillTags;
        public bool usedSkillIsBodySkill;
        public bool usedSkillIsWeaponSkill;
    }

    /// <summary>
    /// 条件判定の共通ロジック（AND）
    /// </summary>
    public static class SkillOccasionEvaluator
    {
        public static bool AreAllTrue(SkillOccasionCondition[] conditions, in SkillTriggerContext ctx)
        {
            if (conditions == null || conditions.Length == 0) return true;

            for (int i = 0; i < conditions.Length; i++)
            {
                if (!IsConditionTrue(conditions[i], ctx))
                    return false;
            }
            return true;
        }

        private static bool IsConditionTrue(in SkillOccasionCondition c, in SkillTriggerContext ctx)
        {
            switch (c.kind)
            {
                case SkillConditionKind.None:
                    return true;

                // 自分状態
                case SkillConditionKind.SelfHpBelowPercent:
                    return ctx.selfHpRate < c.rateParam;

                case SkillConditionKind.SelfHpAbovePercent:
                    return ctx.selfHpRate > c.rateParam;

                case SkillConditionKind.SelfMovedThisTime:
                    return ctx.selfMovedThisTime;

                case SkillConditionKind.SelfNotMovedThisTime:
                    return !ctx.selfMovedThisTime;

                // 対象状態（other）
                case SkillConditionKind.TargetHasAnyStatus:
                    return ctx.otherHasAnyStatus;

                case SkillConditionKind.TargetHasDebuff:
                    return ctx.otherHasDebuff;

                case SkillConditionKind.TargetHpBelowPercent:
                    return ctx.otherHpRate < c.rateParam;

                // 戦況
                case SkillConditionKind.EnemyCountAtLeast:
                    return ctx.enemyCount >= c.intParam;

                case SkillConditionKind.AllyCountAtLeast:
                    return ctx.allyCount >= c.intParam;

                // スキル側性質
                case SkillConditionKind.UsedSkillIsBasic:
                    return ctx.usedSkillIsBasic;

                case SkillConditionKind.UsedSkillHasTag:
                    return (ctx.usedSkillTags & c.tagParam) != 0;

                case SkillConditionKind.UsedSkillIsBodySkill:
                    return ctx.usedSkillIsBodySkill;

                case SkillConditionKind.UsedSkillIsWeaponSkill:
                    return ctx.usedSkillIsWeaponSkill;

                default:
                    // 未実装種別は不成立（暴発防止）
                    return false;
            }
        }
    }
}
