using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// パッシブの発動契機。
    /// 武器スキル/種族スキルを考えていく過程で、必要な種別が出たら追加する方針。
    /// </summary>
    public enum PassiveTrigger
    {
        BattleStart,                 // 戦闘開始時 1 回 (戦闘中常時バフのキック用にも)
        OnNthBasicAttackHit,         // 戦闘中 N 回目の基本攻撃命中時 (triggerIntParam=N)
        OnConsecutiveHitSameTarget,  // 同一対象への連続命中 N 回目 (triggerIntParam=N)
        OnDamaged,                   // 被弾時
        OnDamagedThenNextHit,        // 被弾後、次に自分が当てた攻撃命中時
        OnDealDamage,                // 攻撃命中時 (毎回)
        OnKill,                      // 敵撃破時
        OnSelfHpBelowPercent,        // 自 HP が triggerFloatParam% 以下に落ちた瞬間
        AlwaysWhileActive,           // 戦闘中常時 (状態異常/バフを戦闘中維持)
    }

    /// <summary>
    /// パッシブの効果種別。必要なら追加していく。
    /// </summary>
    public enum PassiveEffect
    {
        ExtraDamageThisHit,       // 発動契機のヒットに追加ダメ (effectAmount% = 50/100/150 等)
        NextHitDamageReduction,   // 次に受ける被ダメを effectAmount% 軽減
        ExtraHitOnSameTarget,     // 同じ攻撃をもう 1 回当てる (effectAmount は倍率)
        ApplyStatusEffectSelf,    // 自分に StatusEffect 付与 (statusEffectType/Value/durationSec)
        ApplyStatusEffectTarget,  // 発動契機の対象に StatusEffect 付与
        HealSelf,                 // 自 HP を effectAmount ぶん回復
        // --- Phase A で追加 (ロジックは Phase B 以降で実装) ---
        GuardChanceBoost,         // 自分のガード確率を effectAmount% UP (騎士 / 盾)
        StatusEffectOnHitTarget,  // 攻撃命中時に対象に状態異常 (炎上 / 凍結 / スタン等)
        CooldownResetEnemyBasic,  // 敵の通常攻撃 CT を初期化 (ウエイトカッター)
        RangeMultiplier,          // 射程倍率 (彼方 / 魔王の杖)
        MoveSpeedMultiplier,      // 移動速度倍率 (軽弓 AGI UP 等)
        AttackSpeedMultiplier,    // 攻撃 CT 倍率 (速く / 遅く)
        DamageKindOverride,       // 通常攻撃の属性を上書き (魔力轟槌 = Magical)
        HealMultiplier,           // 自分が発する回復効果の倍率 (ヒーラーロッド)
        PullEnemies,              // 重力源発生、敵引き寄せ (重奏大剣 / グラビティ)
        KnockbackAoE,             // 自中心 AoE ノックバック (ストームロングエッジ)
        MultiShot,                // 多方向発射 (トリデントアーチャー)
        BasicAttackMul,           // 基本攻撃のダメ倍率 (白銀の大剣)
        SkillMul,                 // スキル攻撃のダメ倍率 (白銀の大剣 / アルカナ)
        CooldownMultiplierSelf,   // 自 CT 倍率 (玉砕覚悟 / 達人の弓)
        RandomBuffSelf,           // ランダムステ UP (打出の大槌)
        FocusMark,                // シャドウマーク: 近場味方のターゲット誘導
        GoldDropChance,           // 金ドロップ確率 (黄金の輝き)
        DisableBasicAttack,       // ノーマル攻撃を使えなくする (アルカナマキシマス)
        AttackShapeOverride,      // 範囲変更 (収束する運命)
        ExtraAoeHit,              // 対象中心に AoE 追加ヒット (三星剣)
        AutoReviveOnce,           // 死亡時 1 度だけ復活 (十字架)
    }

    /// <summary>
    /// リアルタイム戦闘向けのパッシブ定義。
    /// 武器スキル (RealtimeWeaponDefinition.weaponPassives) と
    /// 種族スキル (RaceDefinition.realtimePassives) の両方で使う。
    /// </summary>
    [CreateAssetMenu(fileName = "RealtimePassive", menuName = "SteraCube/Realtime/Passive")]
    public class RealtimePassiveDefinition : ScriptableObject
    {
        [Header("基本")]
        public string passiveId;
        public string displayName;
        [TextArea] public string description;

        [Header("発動契機")]
        public PassiveTrigger trigger;
        [Tooltip("N 回目、連続N回 等の整数パラメータ")]
        public int triggerIntParam = 0;
        [Tooltip("HP %、割合 等の浮動小数パラメータ")]
        public float triggerFloatParam = 0f;

        [Header("効果")]
        public PassiveEffect effect;
        [Tooltip("追撃 %、軽減 %、回復量 などの主要数値")]
        public float effectAmount = 0f;
        [Tooltip("StatusEffect 系の効果持続秒")]
        public int effectDurationSec = 5;

        [Header("StatusEffect (effect が ApplyStatusEffect* のとき)")]
        public StatusEffectType statusEffectType = StatusEffectType.None;
        public int statusEffectValue = 0;

        [Header("発動確率 / 制限")]
        [Tooltip("発動確率 (0-1)。1.0=確定発動、0.2=20%で発動。RT 戦闘では確率トリガの表現に使う")]
        [Range(0f, 1f)] public float procChance = 1.0f;
        [Tooltip("1 戦闘中の最大発動回数。-1 で無制限")]
        public int maxTriggersPerBattle = -1;
        [Tooltip("次の発動まで必要な秒数 (連発防止)")]
        public float cooldownSec = 0f;
    }
}
