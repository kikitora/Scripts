using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    // このクラス群は「条件（Condition）」の表現と評価を担当します。
    // 「いつチェックするか（Timing）」は別レイヤー（BattleEventBus + SkillDefinition.passiveTimings）で管理し、
    // ここは "条件だけ" に集中させます。

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
    /// 戦闘イベントの文脈（BattleEventBusが流すもの）。
    /// 条件判定に必要な "値" を詰めるだけ。ゲーム実装の具体クラスに依存しない方向。
    ///
    /// ■ 値のセット責務
    ///   - 既存フィールド（selfHpRate 等）: BattleEventBus がイベントを流す直前にセット。
    ///   - 戦闘中カウント（selfAttackCount 等）: BattleContext（または SpaceJourneyUnit）が
    ///     攻撃命中のたびにインクリメントし、ctx 生成時にコピーする。
    ///   - 直前アクション系フラグ（selfWasHitSinceLastAction 等）: 行動機会の開始時に
    ///     前の行動機会が終わってから今回までの出来事をフラグに転写し、行動終了時にリセット。
    /// </summary>
    public struct SkillTriggerContext
    {
        public SkillTriggerTiming timing;

        // Action系の詳細
        public ActionEventPhase actionPhase;

        // HP系の詳細
        public HpDeltaSourceType hpSource;
        public int hpDelta; // damage=+ / heal=-

        // 状態付与試行の詳細（StatusAttemptEvent用）
        public StatusEffectType statusEffectType;
        public int statusValue;
        public int statusDuration;

        // 関係者（必要なときだけ入れる）
        public SpaceJourneyUnit self;
        public SpaceJourneyUnit other;       // 攻撃者/被攻撃者、付与者/被付与者など
        public SkillDefinition usedSkill;    // 原因になったスキル（無ければ null）

        // ─── 既存：よく使う評価用の値（無ければデフォルト 0/false でOK）────
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

        // ─── 追加：戦闘中カウント（80番台 SkillConditionKind 対応）───────────
        // BattleContext（または SpaceJourneyUnit）で管理し、ctx 生成時にコピーする。

        /// <summary>
        /// この戦闘中にこのユニットが攻撃スキルで命中させた累計回数。
        /// AttackCountReached（80）の判定に使う。
        /// </summary>
        public int selfAttackCount;

        /// <summary>
        /// この戦闘中にこのユニットが魔法スキルで命中させた累計回数。
        /// MagicCountReached（88）の判定に使う。
        /// </summary>
        public int selfMagicCount;

        // ─── 追加：直前アクション系フラグ（行動機会ごとにリセット）──────────
        // 「前回の行動機会が終わってから今回の行動機会が始まるまでの間」に
        // 発生した出来事をフラグとして保持する。
        // 行動機会の開始時にセット → 行動機会の終了時にリセット。

        /// <summary>
        /// 前回の行動機会終了後〜今回の行動機会開始前の間に被弾した。
        /// SelfWasHitSinceLastAction（81）の判定に使う。
        /// </summary>
        public bool selfWasHitSinceLastAction;

        /// <summary>
        /// 直前の行動機会でこのユニットが Defend スキルを使用した。
        /// SelfUsedDefendLastAction（82）の判定に使う。
        /// </summary>
        public bool selfUsedDefendLastAction;

        /// <summary>
        /// 直前の攻撃でこのユニットが敵を撃破した。
        /// SelfKilledEnemyLastAction（83）の判定に使う。
        /// </summary>
        public bool selfKilledEnemyLastAction;

        // ─── 追加：対象・スキル系──────────────────────────────────────────────

        /// <summary>
        /// 現在の攻撃が同一対象への連続命中の何回目か（1始まり）。
        /// 対象が変わったらリセット。
        /// ConsecutiveHitSameTarget（84）の判定に使う。
        /// </summary>
        public int consecutiveHitCount;

        /// <summary>
        /// このタイムに、敵が自分のマスに新たに隣接してきた。
        /// EnemyBecameAdjacentThisTime（85）の判定に使う。
        /// </summary>
        public bool enemyBecameAdjacentThisTime;

        /// <summary>
        /// 使用スキルが「重スキル」（reuseCycle > baseCost）かどうか。
        /// UsedSkillIsHeavy（86）の判定に使う。
        /// </summary>
        public bool usedSkillIsHeavy;

        /// <summary>
        /// 使用スキルが単体対象スキル（PointArea + Unit）かどうか。
        /// TargetingIsSingle（87）の判定に使う。
        /// </summary>
        public bool targetingIsSingle;

        /// <summary>
        /// 現在の攻撃対象が被ダメ増マーク（DamageMarkApply）を持っているかどうか。
        /// TargetHasDamageMark（89）の判定に使う。
        /// </summary>
        public bool targetHasDamageMark;
    }

    /// <summary>
    /// 条件判定の共通ロジック（AND）。
    /// PassiveDispatcher から呼び出す。
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

                // ─── 自分の状態（10番台）────────────────────────────────────
                case SkillConditionKind.SelfHpBelowPercent:
                    return ctx.selfHpRate < c.rateParam;

                case SkillConditionKind.SelfHpAbovePercent:
                    return ctx.selfHpRate > c.rateParam;

                case SkillConditionKind.SelfMovedThisTime:
                    return ctx.selfMovedThisTime;

                case SkillConditionKind.SelfNotMovedThisTime:
                    return !ctx.selfMovedThisTime;

                // ─── 対象（other）の状態（30番台）──────────────────────────
                case SkillConditionKind.TargetHasAnyStatus:
                    return ctx.otherHasAnyStatus;

                case SkillConditionKind.TargetHasDebuff:
                    return ctx.otherHasDebuff;

                case SkillConditionKind.TargetHpBelowPercent:
                    return ctx.otherHpRate < c.rateParam;

                // ─── 戦況（50番台）─────────────────────────────────────────
                case SkillConditionKind.EnemyCountAtLeast:
                    return ctx.enemyCount >= c.intParam;

                case SkillConditionKind.AllyCountAtLeast:
                    return ctx.allyCount >= c.intParam;

                // ─── 使用スキルの性質（70番台）─────────────────────────────
                case SkillConditionKind.UsedSkillIsBasic:
                    return ctx.usedSkillIsBasic;

                case SkillConditionKind.UsedSkillHasTag:
                    return (ctx.usedSkillTags & c.tagParam) != 0;

                case SkillConditionKind.UsedSkillIsBodySkill:
                    return ctx.usedSkillIsBodySkill;

                case SkillConditionKind.UsedSkillIsWeaponSkill:
                    return ctx.usedSkillIsWeaponSkill;

                // ─── 戦闘中カウント・直前アクション系（80番台）─────────────

                // 80: 戦闘中N回目の攻撃に到達
                // intParam=N で「N回目以上」を判定する。
                // 「N回目の1回だけ」にしたい場合は useCountLimit=1 と組み合わせる。
                case SkillConditionKind.AttackCountReached:
                    return ctx.selfAttackCount >= c.intParam;

                // 81: 前回行動後〜今回行動前の間に被弾した
                case SkillConditionKind.SelfWasHitSinceLastAction:
                    return ctx.selfWasHitSinceLastAction;

                // 82: 直前にDefendスキルを使った
                case SkillConditionKind.SelfUsedDefendLastAction:
                    return ctx.selfUsedDefendLastAction;

                // 83: 直前の攻撃で敵を撃破した
                case SkillConditionKind.SelfKilledEnemyLastAction:
                    return ctx.selfKilledEnemyLastAction;

                // 84: 同一対象への連続命中がN回目以上（intParam=N）
                case SkillConditionKind.ConsecutiveHitSameTarget:
                    return ctx.consecutiveHitCount >= c.intParam;

                // 85: このタイムに敵が新たに隣接してきた
                case SkillConditionKind.EnemyBecameAdjacentThisTime:
                    return ctx.enemyBecameAdjacentThisTime;

                // 86: 使用スキルが重スキル（reuseCycle > baseCost）
                case SkillConditionKind.UsedSkillIsHeavy:
                    return ctx.usedSkillIsHeavy;

                // 87: 使用スキルが単体対象スキル
                case SkillConditionKind.TargetingIsSingle:
                    return ctx.targetingIsSingle;

                // 88: 戦闘中N回目の魔法に到達（intParam=N）
                case SkillConditionKind.MagicCountReached:
                    return ctx.selfMagicCount >= c.intParam;

                // 89: 攻撃対象が被ダメ増マークを持っている
                case SkillConditionKind.TargetHasDamageMark:
                    return ctx.targetHasDamageMark;

                default:
                    // 未実装種別は不成立（暴発防止）
                    return false;
            }
        }
    }
}