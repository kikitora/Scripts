using System;
using UnityEngine;
using UnityEngine.Serialization;
using SCConditionalFields;

namespace SteraCube.SpaceJourney
{
    // このクラスはスキル定義（SO）です。
    // カテゴリごとに必要な項目だけをInspectorに出して、迷いを減らします。
    // ターゲット方式は 3 種（SelfArea / PointArea / MultiSingle）に統一します。
    [CreateAssetMenu(
        fileName = "Skill",
        menuName = "SteraCube/SpaceJourney/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        // =====================================================================
        // 識別
        // =====================================================================
        [Header("識別情報")]
        [SerializeField] private string skillId;
        [SerializeField] private string skillName;
        [TextArea]
        [SerializeField] private string description;

        // =====================================================================
        // 分類
        // =====================================================================
        [Header("分類")]
        public SkillCategory category = SkillCategory.ActiveAttack;
        public SkillTag skillTags = SkillTag.None;

        // =====================================================================
        // 発動条件（共通：最低条件は別ロジックで判定する想定）
        // =====================================================================
        [Header("発動条件（共通）")]
        [Tooltip("スキル使用/発動の追加条件（AND）。\n" +
                 "※『範囲内に対象がいる』等の最低条件は、別の共通ロジックで判定する想定。")]
        public SkillOccasionCondition[] activationConditions = Array.Empty<SkillOccasionCondition>();

        // =====================================================================
        // Passive（timings + conditions）
        // =====================================================================
        [Header("パッシブ発動（Passiveのみ）")]
        [SCShowIf(nameof(category), SkillCategory.Passive)]
        [Tooltip("どのイベント系統で条件チェックするか（複数登録可）")]
        public SkillTriggerTiming[] passiveTimings = Array.Empty<SkillTriggerTiming>();

        [SCShowIf(nameof(category), SkillCategory.Passive)]
        [Tooltip("パッシブ発動条件（AND）。空なら常にOK。")]
        public SkillOccasionCondition[] passiveConditions = Array.Empty<SkillOccasionCondition>();

        // =====================================================================
        // ターゲット（攻撃/補助のみ）
        // =====================================================================
        [Header("ターゲット（攻撃/補助のみ）")]
        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        public SkillTargetingMode targetingMode = SkillTargetingMode.PointArea;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [SCShowIf(nameof(targetingMode), SkillTargetingMode.PointArea)]
        public SkillPointTargetKind pointTargetKind = SkillPointTargetKind.Unit;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [InspectorName("効果対象陣営")]
        public EffectTargetSide effectTargetSide = EffectTargetSide.Enemy;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        public TargetSelectFlags targetSelect = TargetSelectFlags.None;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [SCShowIf(nameof(targetingMode), SkillTargetingMode.MultiSingle)]
        public MultiSinglePickMode multiSinglePickMode = MultiSinglePickMode.MaxTargets;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [SCShowIf(nameof(targetingMode), SkillTargetingMode.MultiSingle)]
        [SCShowIf(nameof(multiSinglePickMode), MultiSinglePickMode.MaxTargets)]
        [Range(1, 5)]
        public int maxTargets = 1;

        // 互換用（Inspectorには出さない）
        // 旧実装が skill.requiresTarget / skill.selectAllInRange を見ても壊れないように残す
        [HideInInspector] public bool requiresTarget = true;
        [HideInInspector] public bool selectAllInRange = false;

        // =====================================================================
        // 行動パラメータ（全カテゴリ共通）
        // =====================================================================
        [Header("行動パラメータ")]
        [Range(1, 9)]
        public int baseCost = 2;

        [InspectorName("開幕待機 (OC)")]
        [Range(0, 10)]
        public int openingCoolTime = 0;

        // ここはあなたが reuseCycle に変えた前提で残す（CTという名前は使わない）
        [InspectorName("再使用周期 (RC)")]
        [Range(0, 10)]
        public int reuseCycle = 0;

        [Tooltip("baseCost のうち何番目で発動するか（0=即時、1=1タイム目…）")]
        [Range(0, 8)]
        public int activationIndexInCost = 0;

        // =====================================================================
        // 命中・ダメージ（攻撃/補助のみ）
        // =====================================================================
        [Header("命中・ダメージ/回復（攻撃/補助のみ）")]
        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [Range(0f, 1f)]
        public float hitRate = 1f;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        public SkillDamageKind damageKind = SkillDamageKind.Physical;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [Tooltip("Physical/Magical系は%（100=等倍）。Fixed/MaxHpRateは値/％。")]
        public int amount = 100;

        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [SCShowIfAny(nameof(damageKind), SkillDamageKind.PenetratePhysical, SkillDamageKind.PenetrateMagical)]
        [Range(0, 100)]
        public int defenseIgnorePercent = 0;

        // =====================================================================
        // 範囲（攻撃/補助/移動で出し分け）
        // =====================================================================
        [Header("範囲（バトル用）")]

        // PointArea / MultiSingle の「候補を選ぶ範囲」
        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [SCShowIfAny(nameof(targetingMode), SkillTargetingMode.PointArea, SkillTargetingMode.MultiSingle)]
        public GridRangePattern targetRange = new GridRangePattern();

        // SelfArea / PointArea の「効果範囲」(AoEはここだけで表現)
        [SCShowIfAny(nameof(category), SkillCategory.ActiveAttack, SkillCategory.ActiveSupport)]
        [SCShowIfAny(nameof(targetingMode), SkillTargetingMode.SelfArea, SkillTargetingMode.PointArea)]
        public GridRangePattern effectRange = new GridRangePattern();

        // =====================================================================
        // 移動（ActiveMoveのみ）
        // =====================================================================
        [Header("移動（ActiveMoveのみ）")]
        [SCShowIf(nameof(category), SkillCategory.ActiveMove)]
        [Tooltip("移動可能マス。例：前方1マスなら(0,+1)のみ。")]
        public GridRangePattern moveRange = new GridRangePattern();

        // 他カテゴリで「移動付き攻撃」等を作りたい場合のトグル（ActiveMove時は固定でtrue扱い）
        [Header("アクション後の最終マス設定（ActiveMove以外用）")]
        [SCHideIf(nameof(category), SkillCategory.ActiveMove)]
        public bool moveAfterSkill = false;

        [SCHideIf(nameof(category), SkillCategory.ActiveMove)]
        [SCShowIf(nameof(moveAfterSkill), true)]
        public GridRangePattern moveAfterRange = new GridRangePattern();

        // =====================================================================
        // 追加効果（全カテゴリ）
        // =====================================================================
        [Header("追加効果（オプション）")]
        [SerializeField] private StatusEffectSpec[] additionalEffects;

        [Serializable]
        public class StatusEffectSpec
        {
            public StatusEffectType effectType = StatusEffectType.None;
            public int value = 0;
            public int duration = 0;
            [Range(0f, 1f)] public float probability = 1f;

            [Header("汎用パラメータ（特殊効果用）")]
            public int intParam1 = 0;
            public int intParam2 = 0;
            public float floatParam1 = 0f;
            public float floatParam2 = 0f;
            public bool boolParam1 = false;
        }

        // =====================================================================
        // 演出
        // =====================================================================
        [Header("アニメーション・演出")]
        [SerializeField] private string animatorTriggerKey;

        // 外部参照用
        public string SkillId => skillId;
        public string SkillName => skillName;
        public string Description => description;
        public StatusEffectSpec[] AdditionalEffects => additionalEffects;
        public string AnimatorTriggerKey => animatorTriggerKey;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // まずは共通クランプ
            maxTargets = Mathf.Clamp(maxTargets, 1, 5);
            defenseIgnorePercent = Mathf.Clamp(defenseIgnorePercent, 0, 100);
            hitRate = Mathf.Clamp01(hitRate);
            activationIndexInCost = Mathf.Clamp(activationIndexInCost, 0, 8);
            baseCost = Mathf.Clamp(baseCost, 1, 9);
            openingCoolTime = Mathf.Clamp(openingCoolTime, 0, 10);
            reuseCycle = Mathf.Clamp(reuseCycle, 0, 10);

            // null保険
            if (activationConditions == null) activationConditions = Array.Empty<SkillOccasionCondition>();
            if (passiveTimings == null) passiveTimings = Array.Empty<SkillTriggerTiming>();
            if (passiveConditions == null) passiveConditions = Array.Empty<SkillOccasionCondition>();

            // Passive以外では、Passive専用欄はデフォルトへ（隠れる部分は触らない運用でも事故防止）
            if (category != SkillCategory.Passive)
            {
                passiveTimings = Array.Empty<SkillTriggerTiming>();
                passiveConditions = Array.Empty<SkillOccasionCondition>();
            }

            // ActiveMove は「移動スキル」：バトルターゲット系を全部デフォルトへ
            if (category == SkillCategory.ActiveMove)
            {
                // 互換フラグ
                requiresTarget = false;
                selectAllInRange = false;

                // バトル用ターゲット系（非表示なのでデフォルトに戻す）
                targetingMode = SkillTargetingMode.PointArea;
                pointTargetKind = SkillPointTargetKind.Unit;
                effectTargetSide = EffectTargetSide.Enemy;
                targetSelect = TargetSelectFlags.None;
                multiSinglePickMode = MultiSinglePickMode.MaxTargets;
                maxTargets = 1;

                // バトル用数値（非表示なのでデフォルト）
                hitRate = 1f;
                damageKind = SkillDamageKind.Physical;
                amount = 100;
                defenseIgnorePercent = 0;

                // バトル用範囲（非表示なのでデフォルト）
                targetRange = new GridRangePattern();
                effectRange = new GridRangePattern();

                // ActiveMove では「移動後マス」は使わないのでデフォルト
                moveAfterSkill = false;
                moveAfterRange = new GridRangePattern();
                return;
            }

            bool isBattleSkill = (category == SkillCategory.ActiveAttack || category == SkillCategory.ActiveSupport);

            // バトルスキルじゃないカテゴリ（例：Passive等）は、戦闘系を全部デフォルトへ
            if (!isBattleSkill)
            {
                requiresTarget = false;
                selectAllInRange = false;

                targetingMode = SkillTargetingMode.PointArea;
                pointTargetKind = SkillPointTargetKind.Unit;
                effectTargetSide = EffectTargetSide.Enemy;
                targetSelect = TargetSelectFlags.None;
                multiSinglePickMode = MultiSinglePickMode.MaxTargets;
                maxTargets = 1;

                hitRate = 1f;
                damageKind = SkillDamageKind.Physical;
                amount = 100;
                defenseIgnorePercent = 0;

                targetRange = new GridRangePattern();
                effectRange = new GridRangePattern();

                // moveAfterSkill が false なら隠れてる moveAfterRange はデフォルトへ
                if (!moveAfterSkill) moveAfterRange = new GridRangePattern();
                return;
            }

            // --- ここから ActiveAttack / ActiveSupport 用 ---

            // 互換フラグ同期
            requiresTarget = (targetingMode != SkillTargetingMode.SelfArea);

            if (targetingMode == SkillTargetingMode.MultiSingle)
                selectAllInRange = (multiSinglePickMode == MultiSinglePickMode.AllInRange);
            else
                selectAllInRange = false;

            // targetingModeごとに「隠れている項目」をデフォルトに戻す
            switch (targetingMode)
            {
                case SkillTargetingMode.SelfArea:
                    // SelfArea では pointTarget / multiSingle / targetRange は不要なのでデフォルト化
                    pointTargetKind = SkillPointTargetKind.Unit;
                    multiSinglePickMode = MultiSinglePickMode.MaxTargets;
                    maxTargets = 1;
                    targetRange = new GridRangePattern();
                    break;

                case SkillTargetingMode.PointArea:
                    // PointArea では multiSingle は不要
                    multiSinglePickMode = MultiSinglePickMode.MaxTargets;
                    maxTargets = 1;
                    break;

                case SkillTargetingMode.MultiSingle:
                    // MultiSingle では pointTarget / effectRange は不要
                    pointTargetKind = SkillPointTargetKind.Unit;
                    effectRange = new GridRangePattern();

                    if (multiSinglePickMode == MultiSinglePickMode.AllInRange)
                        maxTargets = 1;
                    else
                        maxTargets = Mathf.Clamp(maxTargets, 1, 5);
                    break;
            }

            // moveAfterSkill が false なら隠れてる moveAfterRange はデフォルトへ
            if (!moveAfterSkill) moveAfterRange = new GridRangePattern();
        }
#endif
    }
}
