// SpaceJourneyUnit.cs
// このクラスで何をするか：
// SpaceJourneyモードにおける「1ユニット（1キャラクター）」の実体データを表します。
// - ソウル（SoulInstance）
// - ボディの実体（BodyInstance：テンプレ＋ランク＋個体差込み）
// を組み合わせて、最終ステータスや現在HPなどを管理します。
// 追加：StatusEffect（additionalEffects）を「戦闘タイム(t)」で管理し、%で最終ステータスに反映します。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    [Serializable]
    public class SpaceJourneyUnit
    {
        [Header("構成要素")]
        [SerializeField] private SoulInstance soul;      // ソウル実体
        [SerializeField] private BodyInstance body;      // ボディ実体（ランク＋個体差込み）

        [Header("戦闘状態")]
        [SerializeField] private int currentHp;          // 現在HP（戦闘中に減る）
        [SerializeField] private bool isDead = false;    // 戦闘不能フラグ

        // ─────────────────────────────
        // 追加：戦闘タイム(t)と状態効果
        // ─────────────────────────────
        [SerializeField] private int battleTime = 0;

        [Serializable]
        private class ActiveStatusEffect
        {
            public StatusEffectType type;
            public int valuePercent; // %ポイント（例：+30 / -20）
            public int expireTime;   // battleTime >= expireTime で失効
        }

        [SerializeField] private List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

        public SpaceJourneyUnit(SoulInstance soul, BodyInstance body)
        {
            this.soul = soul;
            this.body = body;

            currentHp = MaxHp;
            isDead = false;

            battleTime = 0;
            if (activeEffects == null) activeEffects = new List<ActiveStatusEffect>();
        }

        // ─────────────────────────────
        // プロパティ：構成要素
        // ─────────────────────────────
        public SoulInstance Soul => soul;
        public BodyInstance Body => body;

        // ─────────────────────────────
        // 追加：戦闘タイム(t)
        // ─────────────────────────────
        public int BattleTime => battleTime;

        /// <summary>
        /// 戦闘側が「現在の戦闘タイム(t)」を更新するために呼びます。
        /// </summary>
        public void SetBattleTime(int currentBattleTime)
        {
            battleTime = Mathf.Max(0, currentBattleTime);
            PurgeExpiredEffects();
        }

        // ─────────────────────────────
        // 最終ステータス（ボディ適用後 + 状態効果%）
        // ─────────────────────────────

        public int AtFinal
        {
            get
            {
                int baseVal = (soul != null && body != null)
                    ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.AT), StatKind.AT)
                    : 0;
                return ApplyPercentModifier(baseVal, GetTotalPercentModifierForStat(StatKind.AT));
            }
        }

        public int DfFinal
        {
            get
            {
                int baseVal = (soul != null && body != null)
                    ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.DF), StatKind.DF)
                    : 0;
                return ApplyPercentModifierClampMin0(baseVal, GetTotalPercentModifierForStat(StatKind.DF));
            }
        }

        public int AgiFinal
        {
            get
            {
                int baseVal = (soul != null && body != null)
                    ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.AGI), StatKind.AGI)
                    : 0;
                return ApplyPercentModifierClampMin0(baseVal, GetTotalPercentModifierForStat(StatKind.AGI));
            }
        }

        public int MatFinal => (soul != null && body != null)
            ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.MAT), StatKind.MAT)
            : 0;

        public int MdfFinal
        {
            get
            {
                int baseVal = (soul != null && body != null)
                    ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.MDF), StatKind.MDF)
                    : 0;

                // BuffDf/DebuffDf は DF/MDF 両方に効かせる運用（「防御」扱い）
                return ApplyPercentModifierClampMin0(baseVal, GetTotalPercentModifierForStat(StatKind.MDF));
            }
        }

        public int MaxHp => body != null ? body.MaxHp : 0;

        // ─────────────────────────────
        // HP・死亡状態管理
        // ─────────────────────────────
        public int CurrentHp => currentHp;
        public bool IsDead => isDead;

        public void RestoreFullHp()
        {
            currentHp = MaxHp;
            isDead = false;
        }

        public void TakeDamage(int amount)
        {
            if (isDead) return;
            if (amount <= 0) return;

            currentHp -= amount;
            if (currentHp <= 0)
            {
                currentHp = 0;
                isDead = true;
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            currentHp += amount;
            int max = MaxHp;
            if (currentHp > max) currentHp = max;

            if (currentHp > 0) isDead = false;
        }

        // ─────────────────────────────
        // 行動コスト（AGI影響）
        // ─────────────────────────────
        public int CalcActionCost(int baseCost)
        {
            if (soul == null || body == null)
            {
                return baseCost;
            }

            float agi = AgiFinal;
            return SpaceJourneyStatMath.CalcEffectiveActionCost(baseCost, agi);
        }

        // ─────────────────────────────
        // 追加：状態効果（additionalEffects）
        // ─────────────────────────────

        /// <summary>
        /// 追加効果を付与します。duration は「戦闘タイム(t)」。
        /// value は %ポイント（例：30 → +30%）。
        /// Debuff系は value を正で渡してOK（内部で - に正規化）。
        /// </summary>
        public void ApplyStatusEffect(StatusEffectType type, int value, int duration)
        {
            ApplyStatusEffect(type, value, duration, battleTime);
        }

        public void ApplyStatusEffect(StatusEffectType type, int value, int duration, int nowTime)
        {
            if (duration <= 0) return;

            if (activeEffects == null) activeEffects = new List<ActiveStatusEffect>();

            int normalizedValue = NormalizeValuePercentByType(type, value);
            int expire = Mathf.Max(0, nowTime) + duration;

            // 同タイプが既にあれば「強い方 + 長い方」に更新（最小実装）
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type != type) continue;

                activeEffects[i].expireTime = Mathf.Max(activeEffects[i].expireTime, expire);
                activeEffects[i].valuePercent = ChooseStrongerValue(type, activeEffects[i].valuePercent, normalizedValue);
                return;
            }

            activeEffects.Add(new ActiveStatusEffect
            {
                type = type,
                valuePercent = normalizedValue,
                expireTime = expire
            });
        }

        public bool IsActionDisabled
        {
            get
            {
                PurgeExpiredEffects();
                return HasActiveEffect(StatusEffectType.Stun) || HasActiveEffect(StatusEffectType.Freeze);
            }
        }

        private bool HasActiveEffect(StatusEffectType type)
        {
            if (activeEffects == null) return false;
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == type && activeEffects[i].expireTime > battleTime)
                    return true;
            }
            return false;
        }

        private void PurgeExpiredEffects()
        {
            if (activeEffects == null || activeEffects.Count == 0) return;

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].expireTime <= battleTime)
                    activeEffects.RemoveAt(i);
            }
        }

        private int GetTotalPercentModifierForStat(StatKind stat)
        {
            PurgeExpiredEffects();
            if (activeEffects == null || activeEffects.Count == 0) return 0;

            int sum = 0;

            for (int i = 0; i < activeEffects.Count; i++)
            {
                var e = activeEffects[i];
                if (e.expireTime <= battleTime) continue;

                switch (stat)
                {
                    case StatKind.AT:
                        if (e.type == StatusEffectType.BuffAt || e.type == StatusEffectType.DebuffAt)
                            sum += e.valuePercent;
                        break;

                    case StatKind.AGI:
                        if (e.type == StatusEffectType.BuffAgi || e.type == StatusEffectType.DebuffAgi)
                            sum += e.valuePercent;
                        break;

                    case StatKind.DF:
                    case StatKind.MDF:
                        if (e.type == StatusEffectType.BuffDf || e.type == StatusEffectType.DebuffDf)
                            sum += e.valuePercent;
                        break;
                }
            }

            return sum;
        }

        private int NormalizeValuePercentByType(StatusEffectType type, int value)
        {
            int v = Mathf.Abs(value);

            switch (type)
            {
                case StatusEffectType.BuffAt:
                case StatusEffectType.BuffDf:
                case StatusEffectType.BuffAgi:
                    return v;

                case StatusEffectType.DebuffAt:
                case StatusEffectType.DebuffDf:
                case StatusEffectType.DebuffAgi:
                    return -v;

                default:
                    return value;
            }
        }

        private int ChooseStrongerValue(StatusEffectType type, int oldValue, int newValue)
        {
            switch (type)
            {
                case StatusEffectType.BuffAt:
                case StatusEffectType.BuffDf:
                case StatusEffectType.BuffAgi:
                    return Mathf.Max(oldValue, newValue);

                case StatusEffectType.DebuffAt:
                case StatusEffectType.DebuffDf:
                case StatusEffectType.DebuffAgi:
                    return Mathf.Min(oldValue, newValue);

                default:
                    return newValue;
            }
        }

        private int ApplyPercentModifier(int baseValue, int percentAdd)
        {
            float f = baseValue * (1f + percentAdd / 100f);
            return Mathf.RoundToInt(f);
        }

        private int ApplyPercentModifierClampMin0(int baseValue, int percentAdd)
        {
            float f = baseValue * (1f + percentAdd / 100f);
            return Mathf.Max(0, Mathf.RoundToInt(f));
        }
    }

    [System.Serializable]
    public class SkillAndConditions
    {
        [SerializeField] SkillDefinition skill;
        [SerializeField] ConditionDefinition[] conditions;
    }

    public class ConditionDefinition
    {
    }
}
