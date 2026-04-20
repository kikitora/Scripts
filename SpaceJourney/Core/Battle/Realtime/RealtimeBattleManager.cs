using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘の統括コンポーネント。
    /// 生存監視、勝敗判定、ログ収集、制限時間管理。
    /// </summary>
    public class RealtimeBattleManager : MonoBehaviour
    {
        [Header("戦闘設定")]
        [Tooltip("制限時間 (秒)")]
        public float battleTimeLimitSec = 20f;

        [Header("実行状態")]
        public bool IsFinished { get; private set; } = false;
        public int WinningSide { get; private set; } = -1;
        public float ElapsedSec { get; private set; } = 0f;

        [Header("ログ")]
        public List<string> BattleLog = new();

        public readonly List<RealtimeBattleUnit> AllUnits = new();

        public void RegisterUnit(RealtimeBattleUnit u)
        {
            if (u != null && !AllUnits.Contains(u)) AllUnits.Add(u);
        }

        public void BeginBattle()
        {
            IsFinished = false;
            WinningSide = -1;
            ElapsedSec = 0f;
            BattleLog.Clear();
            BattleLog.Add("=== リアルタイム戦闘開始 ===");
        }

        private void Update()
        {
            if (IsFinished) return;

            ElapsedSec += Time.deltaTime;

            // 制限時間判定
            if (ElapsedSec >= battleTimeLimitSec)
            {
                EndBattleByTimeout();
                return;
            }

            // 全滅判定
            bool allyAlive = AllUnits.Any(u => u != null && u.ownerSide == 0 && u.IsAlive());
            bool enemyAlive = AllUnits.Any(u => u != null && u.ownerSide == 1 && u.IsAlive());

            if (!allyAlive || !enemyAlive)
            {
                IsFinished = true;
                WinningSide = allyAlive ? 0 : (enemyAlive ? 1 : -1);
                BattleLog.Add($"[{ElapsedSec:F1}s] 決着: {(WinningSide == 0 ? "味方勝利" : WinningSide == 1 ? "敵勝利" : "引き分け")}");
                PlayEndPoses();
                LogSummary();
            }
        }

        private void PlayEndPoses()
        {
            int called = 0;
            foreach (var u in AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                var ja = u.GetComponent<JobAnimator>();
                if (ja == null) { Debug.LogWarning($"[RealtimeBattleManager] {u.name}: no JobAnimator"); continue; }
                if (u.ownerSide == WinningSide) { ja.PlayVictory(); called++; }
                else { ja.PlayDefeat(); called++; }
            }
            Debug.Log($"[RealtimeBattleManager] PlayEndPoses called on {called} units (winner={WinningSide})");
        }

        private void EndBattleByTimeout()
        {
            IsFinished = true;
            int allyHp = AllUnits.Where(u => u != null && u.ownerSide == 0 && u.unit != null).Sum(u => u.unit.CurrentHp);
            int enemyHp = AllUnits.Where(u => u != null && u.ownerSide == 1 && u.unit != null).Sum(u => u.unit.CurrentHp);
            if (allyHp > enemyHp) WinningSide = 0;
            else if (enemyHp > allyHp) WinningSide = 1;
            else WinningSide = -1;
            BattleLog.Add($"[{ElapsedSec:F1}s] 制限時間、HP判定: {(WinningSide == 0 ? "味方" : WinningSide == 1 ? "敵" : "引分")} (味方{allyHp} 敵{enemyHp})");
            PlayEndPoses();
            LogSummary();
        }

        private void LogSummary()
        {
            for (int s = 0; s < 2; s++)
            {
                var side = AllUnits.Where(u => u != null && u.ownerSide == s).ToList();
                int alive = side.Count(u => u.IsAlive());
                int totalHp = side.Where(u => u.unit != null).Sum(u => u.unit.CurrentHp);
                int maxHp = side.Where(u => u.unit != null).Sum(u => u.unit.MaxHp);
                BattleLog.Add($"  {(s == 0 ? "味方" : "敵")}: {alive}/{side.Count} 生存 HP {totalHp}/{maxHp}");
            }

            Debug.Log(string.Join("\n", BattleLog));
        }

        /// <summary>攻撃発生時の外部通知 (ログ用)</summary>
        public void OnAttack(RealtimeBattleUnit attacker, RealtimeBattleUnit target, int dmg, int before, int after)
        {
            string dead = target.IsAlive() ? "" : " ★撃破";
            BattleLog.Add($"[{ElapsedSec:F1}s] {attacker.displayName} → {target.displayName} {dmg}ダメ (HP {before}→{after}){dead}");
        }
    }
}
