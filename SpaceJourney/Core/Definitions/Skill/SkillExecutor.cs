// SkillExecutor.cs
// このクラスで何をするか：
// SkillDefinition と SpaceJourneyUnit を使って、スキルのHP増減を実行し、追加効果(StatusEffect)も付与します。
// 追加：duration=0 を「コスト連動（effectiveCost）」として扱う。

using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public static class SkillExecutor
    {
        /// <summary>
        /// 単体へのHP変化（ダメージ/回復）を実行します。
        /// 戻り値：HP変化量（ダメージ=+ / 回復=- / ミス=0）
        /// </summary>
        public static int ExecuteSingleTargetHpDelta(
            SkillDefinition skill,
            SpaceJourneyUnit attacker,
            SpaceJourneyUnit defender)
        {
            if (skill == null || attacker == null || defender == null) return 0;
            if (defender.IsDead) return 0;

            // ── 命中判定 ──
            if (skill.hitRate < 1f && Random.value > skill.hitRate)
            {
                return 0;
            }

            // ── HP増減計算 ──
            int hpDelta = CalcHpDelta(skill, attacker, defender);

            // ── 適用 ──
            if (hpDelta > 0) defender.TakeDamage(hpDelta);
            else if (hpDelta < 0) defender.Heal(-hpDelta);

            // ── 追加効果 ──
            ApplyAdditionalEffects(skill, attacker, defender);

            return hpDelta;
        }

        // 互換用：旧メソッド名が呼ばれてる箇所がある場合に備えたラッパー
        public static int ExecuteSingleTargetDamage(
            SkillDefinition skill,
            SpaceJourneyUnit attacker,
            SpaceJourneyUnit defender)
        {
            int delta = ExecuteSingleTargetHpDelta(skill, attacker, defender);
            return Mathf.Max(0, delta);
        }

        private static int CalcHpDelta(SkillDefinition skill, SpaceJourneyUnit attacker, SpaceJourneyUnit defender)
        {
            int amount = skill.amount; // % or 固定値（Fixed/MaxHpRateの時）
            int ignore = Mathf.Clamp(skill.defenseIgnorePercent, 0, 100);

            switch (skill.damageKind)
            {
                case SkillDamageKind.None:
                    return 0;

                case SkillDamageKind.Fixed:
                    // amount がそのまま（-なら回復）
                    return amount;

                case SkillDamageKind.MaxHpRate:
                    {
                        float v = defender.MaxHp * (amount / 100f);
                        return Mathf.RoundToInt(v);
                    }

                case SkillDamageKind.Physical:
                    {
                        float baseAtk = attacker.AtFinal * (amount / 100f);
                        if (baseAtk <= 0f) return 0;
                        return SpaceJourneyStatMath.CalcPhysicalDamage(baseAtk, defender.DfFinal);
                    }

                case SkillDamageKind.Magical:
                    {
                        float baseMat = attacker.MatFinal * (amount / 100f);
                        if (baseMat <= 0f) return 0;
                        return SpaceJourneyStatMath.CalcMagicalDamage(baseMat, defender.MdfFinal);
                    }

                case SkillDamageKind.PenetratePhysical:
                    {
                        float df2 = defender.DfFinal * (1f - ignore / 100f);
                        df2 = Mathf.Max(0f, df2);

                        float baseAtk = attacker.AtFinal * (amount / 100f);
                        if (baseAtk <= 0f) return 0;
                        return SpaceJourneyStatMath.CalcPhysicalDamage(baseAtk, df2);
                    }

                case SkillDamageKind.PenetrateMagical:
                    {
                        float mdf2 = defender.MdfFinal * (1f - ignore / 100f);
                        mdf2 = Mathf.Max(0f, mdf2);

                        float baseMat = attacker.MatFinal * (amount / 100f);
                        if (baseMat <= 0f) return 0;
                        return SpaceJourneyStatMath.CalcMagicalDamage(baseMat, mdf2);
                    }

                default:
                    return 0;
            }
        }

        private static void ApplyAdditionalEffects(SkillDefinition skill, SpaceJourneyUnit attacker, SpaceJourneyUnit defender)
        {
            var effects = skill.AdditionalEffects;
            if (effects == null || effects.Length == 0) return;

            int now = attacker.BattleTime;

            for (int i = 0; i < effects.Length; i++)
            {
                var e = effects[i];
                if (e == null) continue;
                if (e.effectType == StatusEffectType.None) continue;

                // 確率
                float p = Mathf.Clamp01(e.probability);
                if (p < 1f && Random.value > p) continue;

                // duration=0 は「コスト連動」
                int duration = e.duration;
                if (duration == 0)
                {
                    duration = attacker.CalcActionCost(skill.baseCost);
                }

                if (duration <= 0) continue;

                // Buff系は自分、Debuff/状態異常系は相手（防御スキルが自己バフなので重要）
                SpaceJourneyUnit target = IsBuffType(e.effectType) ? attacker : defender;

                target.ApplyStatusEffect(e.effectType, e.value, duration, now);
            }
        }

        private static bool IsBuffType(StatusEffectType type)
        {
            return type == StatusEffectType.BuffAt
                || type == StatusEffectType.BuffDf
                || type == StatusEffectType.BuffAgi;
        }
    }
}
