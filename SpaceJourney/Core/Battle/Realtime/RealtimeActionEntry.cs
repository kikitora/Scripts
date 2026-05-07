using System;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘での行動エントリー。優先度リストの1要素。
    /// 条件マッチで action 実行。skillIndex は CastSkill/Skill系条件 で参照。
    /// </summary>
    [Serializable]
    public class RealtimeActionEntry
    {
        [Tooltip("条件: この条件を満たしたら action 発動")]
        public RealtimeCondition condition = RealtimeCondition.Always;

        [Tooltip("条件用スキル index (SkillCooldownReady/TargetInSkillRange/EnemiesHitCountGe 用)")]
        public int conditionSkillIndex = 0;

        [Tooltip("条件用の数値パラメータ (HP%、ヒット数 n 等)")]
        public float conditionParam = 0f;

        [Tooltip("行動: 条件マッチ時に実行する動き")]
        public RealtimeAction action = RealtimeAction.Wait;

        [Tooltip("行動用スキル index (CastSkill 用)")]
        public int actionSkillIndex = 0;

        [Tooltip("備考 (デバッグ用)")]
        public string label = "";

        public RealtimeActionEntry() { }
        public RealtimeActionEntry(RealtimeCondition cond, RealtimeAction act, string note = "")
        {
            condition = cond; action = act; label = note;
        }
    }

    public enum RealtimeCondition
    {
        Always,                  // 常に真
        CanBasicAttack,          // 基本攻撃CT空 AND 射程内 (内蔵攻撃用、後方互換)
        TargetWithinClose,       // dist ≤ Close (1m)
        TargetWithinMid,         // dist ≤ Mid (2m)
        TargetWithinFar,         // dist ≤ Far (5m)
        TargetWithinMaxFar,      // dist ≤ MaxFar (8m)
        TargetOutsideClose,
        TargetOutsideMid,
        TargetOutsideFar,
        HpBelow50,               // 自HP < 50%
        TargetHpBelow30,         // 対象HP < 30%

        // ─── スキル関連 (新) ───
        SkillCooldownReady,      // skills[conditionSkillIndex] の CT 空
        TargetInSkillRange,      // 現在ターゲットが skills[i] の射程内
        CanCastSkill,            // CT空 AND 射程内 (合成、一番よく使う条件)
        EnemiesHitCountGe,       // skills[i] を今撃てば conditionParam 体以上ヒット
        AnyEnemyInSkillRange,    // currentTarget に縛られず、誰か敵が skills[i] 射程内に居る (currentTarget が射程外でも別の敵を撃ちに行ける)
        CanCastSkillAny,         // CT空 AND skills[i] 射程内に敵 1 体以上 (= AnyEnemyInSkillRange + IsSkillReady)
        TargetOutsideSkillRange, // currentTarget が skills[i] の射程外 (= TargetInSkillRange の否定)
        SelfHpBelowPercent,      // 自HP < conditionParam%
        AllyHpBelowPercent,      // HP < conditionParam% の味方が存在
        CurrentTargetIsAlly,     // currentTarget が味方陣営
        CurrentTargetIsEnemy,    // currentTarget が敵陣営
        FragileAllyTargetedByEnemy, // 弓/魔/槍 味方が敵に狙われてる (Knight用)
        WasAttackedRecently,     // 直近3秒以内に被弾 (lastAttacker が有効)
        AttackedAndNoEnemyInBasicRange, // 被弾直近 AND preferredRange 内に敵なし
        AttackerCloserThanCurrentTarget, // 被弾直近 AND attacker が現ターゲットより近い AND currentTarget が skills[0] 射程外
        NoLongRangeAttack,       // 3m+ 射程で使える攻撃スキルを 1 つも所持していない (basic 無効化考慮)
        EnemyInContact,          // 自分中心 EnemyContactRadius (default 1.5m) 内に敵が 1 体以上いる (ルート上 path obstacle 検出より広範囲。接敵時のターゲット切替を path 迂回より先行させる用)
    }

    public enum RealtimeAction
    {
        Wait,
        BasicAttack,             // 内蔵基本攻撃 (後方互換)
        MoveToOwnRange,         // 接近のみ (自分の preferredRange まで近づく、中で止まる、後退なし)
        MoveToOwnRangeKeep,     // 距離維持 (preferredRange ぴったりキープ、近すぎれば後退)
        MoveToCloseRange,
        MoveToMidRange,
        MoveToFarRange,
        MoveToMaxFarRange,
        MoveAwayToClose,
        MoveAwayToMid,
        MoveAwayToFar,

        // ─── 新 ───
        CastSkill,               // skills[actionSkillIndex] を発動
        MoveToTarget,            // 現在ターゲットに接近 (距離カテゴリ依存なし)
        MoveToLowestHpAlly,      // HP最少の味方に接近 (ヒール系動線)
    }
}
