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

        [Header("士気")]
        [SerializeField] private float moraleMultiplier = 1f;

        [Header("戦闘状態")]
        [SerializeField] private int currentHp;          // 現在HP（戦闘中に減る）
        [SerializeField] private bool isDead = false;    // 戦闘不能フラグ

        // ─────────────────────────────
        // 追加：戦闘タイム(t)と状態効果
        // ─────────────────────────────
        [SerializeField] private int battleTime = 0;

        /// <summary>ユニットの向き (攻撃パターン回転値 0-3)。0=forward(敵方向), 1=右, 2=後, 3=左。</summary>
        [SerializeField] private int facing = 0;
        public int Facing { get => facing; set => facing = Mathf.Clamp(value, 0, 3) % 4; }

        // ─── パッシブ被ダメ軽減 (社畜魂=魔法 / 与圧スーツ=物理 等で 0.7 などに設定) ───
        [SerializeField] private float passivePhysReduceRate = 1f;
        [SerializeField] private float passiveMagReduceRate = 1f;
        public float PassivePhysReduceRate { get => passivePhysReduceRate; set => passivePhysReduceRate = value; }
        public float PassiveMagReduceRate { get => passiveMagReduceRate; set => passiveMagReduceRate = value; }
        // ─── 踏ん張り (HP閾値到達で1回ずつダメ refund) ───
        [NonSerialized] public bool Shoulder70Used = false;
        [NonSerialized] public bool Shoulder40Used = false;
        // ─── 召喚体マーカー (バリケード = 非貫通1cap, 勝敗集計から除外) ───
        [NonSerialized] public bool IsBarricade = false;
        [NonSerialized] public bool IsSummoned = false;
        // 召喚体専用の固定ステ (Body/Soul を持たない場合に使用)
        [NonSerialized] public bool UseFixedStats = false;
        [NonSerialized] public int FixedMaxHp = 0;
        [NonSerialized] public int FixedAt = 0;
        [NonSerialized] public int FixedDf = 0;
        [NonSerialized] public int FixedAgi = 0;
        [NonSerialized] public int FixedMat = 0;
        [NonSerialized] public int FixedMdf = 0;

        [Serializable]
        private class ActiveStatusEffect
        {
            public StatusEffectType type;
            public int valuePercent; // %ポイント（例：+30 / -20）
            public int expireTime;   // battleTime >= expireTime で失効
            public SpaceJourneyUnit source; // 挑発source等の追跡用
        }

        [SerializeField] private List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

        /// <summary>
        /// 行動妨害系状態異常 (Hardcc + Charm + Silence 等、StatusEffectMeta.IsDisruptive=true) を
        /// 受理した瞬間に発火。引数は duration (秒)。RealtimeBattleUnit が subscribe して
        /// OnDisrupted を呼び、進行中スキルキャンセル + CT払戻 + 硬直 を実装する。
        /// (memory: Disruption spec — DealDamage前=CT払戻+硬直0、後=CT通常+残アニメ秒硬直)
        /// </summary>
        [NonSerialized]
        public Action<float> OnDisruptApplied;

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

        /// <summary>士気によるステータス乗算倍率 (0.1~1.0)</summary>
        public float MoraleMultiplier
        {
            get => moraleMultiplier;
            set => moraleMultiplier = Mathf.Clamp(value, 0.1f, 1f);
        }

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
                int baseVal = UseFixedStats ? FixedAt
                    : (soul != null && body != null)
                        ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.AT), StatKind.AT)
                        : 0;
                int withEffect = ApplyPercentModifier(baseVal, GetTotalPercentModifierForStat(StatKind.AT));
                return Mathf.RoundToInt(withEffect * moraleMultiplier);
            }
        }

        public int DfFinal
        {
            get
            {
                int baseVal = UseFixedStats ? FixedDf
                    : (soul != null && body != null)
                        ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.DF), StatKind.DF)
                        : 0;
                int withEffect = ApplyPercentModifierClampMin0(baseVal, GetTotalPercentModifierForStat(StatKind.DF));
                return Mathf.Max(0, Mathf.RoundToInt(withEffect * moraleMultiplier));
            }
        }

        public int AgiFinal
        {
            get
            {
                int baseVal = UseFixedStats ? FixedAgi
                    : (soul != null && body != null)
                        ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.AGI), StatKind.AGI)
                        : 0;
                int withEffect = ApplyPercentModifierClampMin0(baseVal, GetTotalPercentModifierForStat(StatKind.AGI));
                return Mathf.Max(0, Mathf.RoundToInt(withEffect * moraleMultiplier));
            }
        }

        public int MatFinal
        {
            get
            {
                int baseVal = UseFixedStats ? FixedMat
                    : (soul != null && body != null)
                        ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.MAT), StatKind.MAT)
                        : 0;
                int withEffect = ApplyPercentModifier(baseVal, GetTotalPercentModifierForStat(StatKind.MAT));
                return Mathf.RoundToInt(withEffect * moraleMultiplier);
            }
        }

        public int MdfFinal
        {
            get
            {
                int baseVal = UseFixedStats ? FixedMdf
                    : (soul != null && body != null)
                        ? body.ApplyToSoulStat(soul.GetSoulStat(StatKind.MDF), StatKind.MDF)
                        : 0;

                // BuffDf/DebuffDf は DF/MDF 両方に効かせる運用（「防御」扱い）
                int withEffect = ApplyPercentModifierClampMin0(baseVal, GetTotalPercentModifierForStat(StatKind.MDF));
                return Mathf.Max(0, Mathf.RoundToInt(withEffect * moraleMultiplier));
            }
        }

        public int MaxHp => Mathf.Max(1, Mathf.RoundToInt(
            (UseFixedStats ? FixedMaxHp : (body != null ? body.MaxHp : 0)) * moraleMultiplier));

        /// <summary>召喚体生成: Soul/Body なしで固定ステを持つ Unit を作る (バリケード/守護霊)</summary>
        public static SpaceJourneyUnit CreateSummonedUnit(int hp, int at, int df, int agi, int mat, int mdf, bool isBarricade)
        {
            var u = new SpaceJourneyUnit(null, null);
            u.UseFixedStats = true;
            u.FixedMaxHp = Mathf.Max(1, hp);
            u.FixedAt = at; u.FixedDf = df; u.FixedAgi = agi;
            u.FixedMat = mat; u.FixedMdf = mdf;
            u.currentHp = u.MaxHp;
            u.isDead = false;
            u.IsBarricade = isBarricade;
            u.IsSummoned = true;
            return u;
        }

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
            ApplyStatusEffect(type, value, duration, battleTime, null);
        }

        public void ApplyStatusEffect(StatusEffectType type, int value, int duration, int nowTime)
        {
            ApplyStatusEffect(type, value, duration, nowTime, null);
        }

        public void ApplyStatusEffect(StatusEffectType type, int value, int duration, int nowTime, SpaceJourneyUnit source)
        {
            if (duration <= 0) return;

            if (activeEffects == null) activeEffects = new List<ActiveStatusEffect>();

            int normalizedValue = NormalizeValuePercentByType(type, value);
            int expire = Mathf.Max(0, nowTime) + duration;

            // === MMO 式 slot ベース重複制御 ===
            // 同 slot の既存効果を探し、ランク勝負で決着する。
            //   既存ランク > 新ランク → 拒否
            //   既存ランク < 新ランク → 既存削除 → 新規追加
            //   既存ランク = 新ランク かつ 同 type → 既存を強度+expire 更新 (refresh)
            //   既存ランク = 新ランク かつ 別 type → 既存削除 → 新規追加
            // 別 slot の既存効果には触らない (併存可能)。
            var newSlot = StatusEffectMeta.GetSlot(type);
            int newRank = StatusEffectMeta.GetRank(type);

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var existing = activeEffects[i];
                if (existing.expireTime <= battleTime) continue;          // 期限切れは無視
                if (StatusEffectMeta.GetSlot(existing.type) != newSlot) continue; // 別 slot は触らない

                int existingRank = StatusEffectMeta.GetRank(existing.type);

                if (existingRank > newRank)
                {
                    // 既存の方が強い → 新規拒否
                    return;
                }
                if (existingRank < newRank)
                {
                    // 新規の方が強い → 既存削除して継続走査 (同 slot に複数いる可能性は本来無いが念のため)
                    activeEffects.RemoveAt(i);
                    continue;
                }

                // 同ランク
                if (existing.type == type)
                {
                    // 同 type → 強度+expire を更新 (refresh)
                    existing.expireTime = Mathf.Max(existing.expireTime, expire);
                    existing.valuePercent = ChooseStrongerValue(type, existing.valuePercent, normalizedValue);
                    if (source != null) existing.source = source;
                    // Hardcc は refresh でも硬直イベントを再発火 (Stun→Stun で硬直延長)
                    if (StatusEffectMeta.IsDisruptive(type)) OnDisruptApplied?.Invoke(duration);
                    return;
                }

                // 同ランクで別 type (例: Stun 中に同ランクの別 Hardcc 系)
                // → 既存削除 → 新規追加
                activeEffects.RemoveAt(i);
            }

            activeEffects.Add(new ActiveStatusEffect
            {
                type = type,
                valuePercent = normalizedValue,
                expireTime = expire,
                source = source,
            });

            // 行動妨害系受理 → realtime 側 OnDisrupted を発火させる
            if (StatusEffectMeta.IsDisruptive(type)) OnDisruptApplied?.Invoke(duration);
        }

        /// <summary>指定 type の状態異常の source を取得 (有効期限内のみ)</summary>
        public SpaceJourneyUnit GetActiveEffectSource(StatusEffectType type)
        {
            if (activeEffects == null) return null;
            for (int i = 0; i < activeEffects.Count; i++)
            {
                var e = activeEffects[i];
                if (e.type == type && e.expireTime > battleTime && e.source != null && !e.source.IsDead)
                    return e.source;
            }
            return null;
        }

        public bool IsActionDisabled
        {
            get
            {
                PurgeExpiredEffects();
                return HasActiveEffect(StatusEffectType.Stun)
                    || HasActiveEffect(StatusEffectType.Freeze);
            }
        }

        public bool HasActiveEffect(StatusEffectType type) => HasActiveEffectInternal(type);

        /// <summary>指定StatusEffect の valuePercent を返す。無ければ0。複数あれば最大値。</summary>
        public int GetActiveEffectValue(StatusEffectType type)
        {
            if (activeEffects == null) return 0;
            int max = 0;
            bool found = false;
            for (int i = 0; i < activeEffects.Count; i++)
            {
                var e = activeEffects[i];
                if (e.type == type && e.expireTime > battleTime)
                {
                    if (!found || e.valuePercent > max) { max = e.valuePercent; found = true; }
                }
            }
            return max;
        }

        /// <summary>全てのデバフ/状態異常を解除する (浄化用)。解除数を返す。</summary>
        public int DispelAllDebuffs()
        {
            if (activeEffects == null || activeEffects.Count == 0) return 0;
            int count = 0;
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var e = activeEffects[i];
                if (e.expireTime <= battleTime) continue;
                switch (e.type)
                {
                    case StatusEffectType.DebuffAt:
                    case StatusEffectType.DebuffDf:
                    case StatusEffectType.DebuffAgi:
                    case StatusEffectType.DebuffMat:
                    case StatusEffectType.DebuffMdf:
                    case StatusEffectType.Stun:
                    case StatusEffectType.Freeze:
                    case StatusEffectType.Burn:
                    case StatusEffectType.ChainDamage:
                    case StatusEffectType.Taunt:
                        activeEffects.RemoveAt(i);
                        count++;
                        break;
                }
            }
            return count;
        }

        /// <summary>指定タイプ群のバフを最大 maxCount 個、別ユニットへ移す (奪取)。</summary>
        public int StealBuffsTo(SpaceJourneyUnit thief, StatusEffectType[] stealableTypes, int maxCount, int nowTime)
        {
            if (activeEffects == null || activeEffects.Count == 0) return 0;
            int moved = 0;
            for (int i = activeEffects.Count - 1; i >= 0 && moved < maxCount; i--)
            {
                var e = activeEffects[i];
                if (e.expireTime <= battleTime) continue;
                bool match = false;
                foreach (var ty in stealableTypes) { if (e.type == ty) { match = true; break; } }
                if (!match) continue;
                int remaining = Mathf.Max(1, e.expireTime - battleTime);
                thief.ApplyStatusEffect(e.type, e.valuePercent, remaining, nowTime, null);
                activeEffects.RemoveAt(i);
                moved++;
            }
            return moved;
        }

        /// <summary>指定StatusEffect を強制的に失効させる (消費用)。</summary>
        public void ConsumeActiveEffect(StatusEffectType type)
        {
            if (activeEffects == null) return;
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].type == type && activeEffects[i].expireTime > battleTime)
                {
                    activeEffects.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// 有効な Burn/ChainDamage 効果のダメージ合計を計算し、TakeDamage を呼ぶ。
        /// 1tickごとに呼ぶ想定。value は 1tickあたりのダメージ量 (Burnは固定値)。
        /// 戻り値: (type, damage) のリスト (ログ用)
        /// </summary>
        public List<(StatusEffectType type, int damage)> TickPeriodicDamage()
        {
            var applied = new List<(StatusEffectType, int)>();
            if (activeEffects == null || activeEffects.Count == 0) return applied;

            for (int i = 0; i < activeEffects.Count; i++)
            {
                var e = activeEffects[i];
                if (e.expireTime <= battleTime) continue;

                int dmg = 0;
                switch (e.type)
                {
                    case StatusEffectType.Burn:
                        // valuePercent を1tickの固定ダメージとして扱う
                        dmg = Mathf.Max(1, Mathf.Abs(e.valuePercent));
                        break;
                    case StatusEffectType.ChainDamage:
                        // MaxHP の valuePercent % をダメージ (連鎖ダメージ)
                        dmg = Mathf.Max(1, Mathf.RoundToInt(MaxHp * Mathf.Abs(e.valuePercent) / 100f));
                        break;
                    case StatusEffectType.Regen:
                    {
                        // MaxHP の valuePercent % を回復 (負値でログ化)
                        int heal = Mathf.Max(1, Mathf.RoundToInt(MaxHp * Mathf.Abs(e.valuePercent) / 100f));
                        int before = currentHp;
                        currentHp = Mathf.Min(MaxHp, currentHp + heal);
                        applied.Add((e.type, -(currentHp - before)));
                        continue;
                    }
                    default:
                        continue;
                }

                TakeDamage(dmg);
                applied.Add((e.type, dmg));

                if (IsDead) break;
            }

            return applied;
        }

        private bool HasActiveEffectInternal(StatusEffectType type)
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
                        if (e.type == StatusEffectType.BuffDf || e.type == StatusEffectType.DebuffDf)
                            sum += e.valuePercent;
                        break;

                    case StatKind.MAT:
                        if (e.type == StatusEffectType.BuffMat || e.type == StatusEffectType.DebuffMat)
                            sum += e.valuePercent;
                        break;

                    case StatKind.MDF:
                        // 防御系バフ(BuffDf/DebuffDf)は MDF にも効く運用
                        if (e.type == StatusEffectType.BuffDf || e.type == StatusEffectType.DebuffDf
                            || e.type == StatusEffectType.BuffMdf || e.type == StatusEffectType.DebuffMdf)
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
                case StatusEffectType.BuffMat:
                case StatusEffectType.BuffMdf:
                    return v;

                case StatusEffectType.DebuffAt:
                case StatusEffectType.DebuffDf:
                case StatusEffectType.DebuffAgi:
                case StatusEffectType.DebuffMat:
                case StatusEffectType.DebuffMdf:
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
                case StatusEffectType.BuffMat:
                case StatusEffectType.BuffMdf:
                    return Mathf.Max(oldValue, newValue);

                case StatusEffectType.DebuffAt:
                case StatusEffectType.DebuffDf:
                case StatusEffectType.DebuffAgi:
                case StatusEffectType.DebuffMat:
                case StatusEffectType.DebuffMdf:
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

}
