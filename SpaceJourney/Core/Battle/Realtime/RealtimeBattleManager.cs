using System.Collections.Generic;
using System.Linq;
using TMPro;
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

        [Tooltip("戦闘開始前のカウントダウン秒数 (0 で無効)")]
        public float countdownSec = 3f;

        /// <summary>カウントダウン中か</summary>
        public bool IsCountdown { get; private set; } = false;
        /// <summary>カウントダウン残り秒数 (Time.deltaTime ベース、倍速影響なし)</summary>
        public float CountdownRemaining { get; private set; } = 0f;

        [Header("戦闘速度倍率")]
        [Tooltip("1=等倍, 3=3倍速。Time.timeScale は触らず battle 系だけ倍速にする。")]
        [Range(0.1f, 10f)]
        public float battleSpeedMul = 1f;

        /// <summary>現在有効な戦闘速度倍率 (static 参照用: Projectile/Attachment 等が参照)。</summary>
        public static float GlobalSpeed { get; private set; } = 1f;

        /// <summary>battleSpeedMul 倍速で進む独自時刻 (秒)。Time.time の代わりに使う。</summary>
        public float BattleTime { get; private set; } = 0f;

        /// <summary>今フレームの戦闘 delta time (= Time.deltaTime * battleSpeedMul)。</summary>
        public float BattleDeltaTime { get; private set; } = 0f;

        [Header("実行状態")]
        public bool IsFinished { get; private set; } = false;
        public int WinningSide { get; private set; } = -1;
        public float ElapsedSec { get; private set; } = 0f;

        [Header("ログ")]
        public List<string> BattleLog = new();

        [Header("UI")]
        [Tooltip("ダメージ数値ポップアップ (未設定ならフォールバック生成)")]
        public DamagePopupSpawner damagePopup;
        [Tooltip("残り秒数を表示する Text (未設定なら何もしない)")]
        public TMP_Text timeText;

        public readonly List<RealtimeBattleUnit> AllUnits = new();

        public void RegisterUnit(RealtimeBattleUnit u)
        {
            if (u != null && !AllUnits.Contains(u))
            {
                AllUnits.Add(u);
                // 登録時点の倍速を Animator に反映 (Starter 経由でスポーン直後)
                var a = u.GetComponentInChildren<Animator>();
                if (a != null) a.speed = battleSpeedMul;
            }
        }

        public void BeginBattle()
        {
            IsFinished = false;
            WinningSide = -1;
            ElapsedSec = 0f;
            BattleTime = 0f;
            BattleDeltaTime = 0f;
            GlobalSpeed = battleSpeedMul;
            BattleLog.Clear();
            BattleLog.Add("=== リアルタイム戦闘開始 ===");
            ApplySpeedToRegisteredUnits();
            // timeText 未割当なら "TimeText" という名前の TMP_Text を自動検索
            if (timeText == null)
            {
                var go = GameObject.Find("TimeText");
                if (go != null) timeText = go.GetComponent<TMP_Text>();
            }
            // カウントダウン開始
            if (countdownSec > 0f)
            {
                IsCountdown = true;
                CountdownRemaining = countdownSec;
            }
            else
            {
                IsCountdown = false;
                CountdownRemaining = 0f;
            }
        }

        /// <summary>ランタイム中に倍速を変更する。Animator/エフェクトに即時反映。</summary>
        public void SetSpeed(float mul)
        {
            battleSpeedMul = Mathf.Max(0.01f, mul);
            GlobalSpeed = battleSpeedMul;
            ApplySpeedToRegisteredUnits();
        }

        private void ApplySpeedToRegisteredUnits()
        {
            foreach (var u in AllUnits)
            {
                if (u == null) continue;
                var a = u.GetComponentInChildren<Animator>();
                if (a != null) a.speed = battleSpeedMul;
            }
        }

        private float _lastAppliedSpeed = float.NaN;

        private void Update()
        {
            if (IsFinished) return;

            // カウントダウン中: unit 停止、timeText に残り秒数 (整数) 表示
            if (IsCountdown)
            {
                CountdownRemaining -= Time.deltaTime;
                if (CountdownRemaining <= 0f)
                {
                    IsCountdown = false;
                    CountdownRemaining = 0f;
                }
                else
                {
                    BattleDeltaTime = 0f; // 戦闘時刻は進めない
                    if (timeText != null)
                    {
                        // "3" → "2" → "1" の整数表示
                        int disp = Mathf.CeilToInt(CountdownRemaining);
                        timeText.text = disp.ToString();
                    }
                    return;
                }
            }

            // 戦闘時刻は battleSpeedMul 倍速で進む
            GlobalSpeed = battleSpeedMul; // Inspector 変更に追随
            // Inspector ランタイム変更時に Animator.speed を再適用
            if (!Mathf.Approximately(_lastAppliedSpeed, battleSpeedMul))
            {
                ApplySpeedToRegisteredUnits();
                _lastAppliedSpeed = battleSpeedMul;
            }
            BattleDeltaTime = Time.deltaTime * battleSpeedMul;
            BattleTime += BattleDeltaTime;
            ElapsedSec = BattleTime; // 戦闘時刻で統一 (倍速時は実時間より速く進む)

            // 残り秒表示
            if (timeText != null)
            {
                float remain = Mathf.Max(0f, battleTimeLimitSec - BattleTime);
                timeText.text = remain.ToString("F1");
            }

            // StatusEffect の expireTime 評価用に battleTime を秒で更新
            int t = Mathf.RoundToInt(BattleTime);
            foreach (var u in AllUnits)
            {
                if (u != null && u.unit != null) u.unit.SetBattleTime(t);
            }

            // 制限時間判定 (戦闘時間ベース: 倍速でも 20秒分の戦闘が進んだら打ち切り)
            if (BattleTime >= battleTimeLimitSec)
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
