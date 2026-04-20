using System;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘での行動エントリー。優先度リストの1要素。
    /// 条件と行動のペアを表現。
    /// </summary>
    [Serializable]
    public class RealtimeActionEntry
    {
        [Tooltip("条件: この条件を満たしたら action 発動")]
        public RealtimeCondition condition = RealtimeCondition.Always;

        [Tooltip("行動: 条件マッチ時に実行する動き")]
        public RealtimeAction action = RealtimeAction.Wait;

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
        CanBasicAttack,          // 攻撃CT空 AND 基本攻撃の射程内
        TargetWithinClose,       // dist ≤ Close (1マス)
        TargetWithinMid,         // dist ≤ Mid (2マス)
        TargetWithinFar,         // dist ≤ Far (5マス)
        TargetWithinMaxFar,      // dist ≤ MaxFar (8マス)
        TargetOutsideClose,      // dist > Close
        TargetOutsideMid,        // dist > Mid
        TargetOutsideFar,        // dist > Far
        HpBelow50,               // 自HP < 50%
        TargetHpBelow30,         // 対象HP < 30%
    }

    public enum RealtimeAction
    {
        Wait,                    // 停止
        BasicAttack,             // 基本攻撃 (CT/アニメ時間消費)
        MoveToOwnRange,          // 自分の基本攻撃射程内まで接近 (職別: Warrior=1, Lancer=2, Archer/Mage=5)
        MoveToCloseRange,        // Close (1マス) 以内まで接近
        MoveToMidRange,          // Mid (2マス) 以内まで
        MoveToFarRange,          // Far (5マス) 以内まで
        MoveToMaxFarRange,       // MaxFar (8マス) 以内まで
        MoveAwayToClose,         // Close より離れる (dist > 1)
        MoveAwayToMid,           // Mid より離れる (dist > 2)
        MoveAwayToFar,           // Far より離れる (dist > 5)
    }
}
