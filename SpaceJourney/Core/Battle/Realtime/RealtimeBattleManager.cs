using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using SteraCube.SpaceJourney.Realtime.Pathfinding;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘の統括コンポーネント。
    /// 生存監視、勝敗判定、ログ収集、制限時間管理。
    /// 連続移動方式: MovementPhase で全ユニット step + push-out 反復。
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

        /// <summary>動的設置 Barricade (Knight rank 5 スキル) のリスト。
        /// MovementPhase で lifetime / HP チェック + 重なり押し出しを行う。</summary>
        public readonly List<Barricade> barricades = new();

        /// <summary>バリケードマップ (BeginBattle で SimpleGrid 準拠の GridBarricadeMap を構築)。
        /// SimpleGrid 未初期化時のフォールバックとして空マップを保持。</summary>
        public IBarricadeMap barricadeMap = new EmptyBarricadeMap();

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

        /// <summary>連戦バッチ用: 前戦の残骸 (BattleTime / IsFinished 等) をクリアし、
        /// 次戦のユニット Setup が BT=0 を見られるようにする。
        /// 通常の単発戦闘では BeginBattle が同じ役目を果たすが、連戦時は Setup 前に呼ぶ必要がある。</summary>
        public void ResetForNextBattle()
        {
            IsFinished = false;
            WinningSide = -1;
            ElapsedSec = 0f;
            BattleTime = 0f;
            BattleDeltaTime = 0f;
            IsCountdown = false;
            CountdownRemaining = 0f;
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

            // SimpleGrid が Init 済みなら全 mover を attach + barricadeMap 注入
            // (SimpleGrid.Init は RealtimeBattleStarter が先に呼ぶ)
            AttachAllMovers();
        }

        /// <summary>SimpleGrid.Active が存在する場合、全ユニットの mover を attach する。
        /// barricadeMap も SimpleGrid に揃えて GridBarricadeMap (A* 経路探索) を構築。</summary>
        private void AttachAllMovers()
        {
            var grid = SimpleGrid.Active;
            if (grid == null) return;
            // Grid 取得後に GridBarricadeMap 構築 (各 mover に inject)
            barricadeMap = new GridBarricadeMap(this, grid);
            foreach (var u in AllUnits)
            {
                if (u == null) continue;
                var m = u.mover;
                if (m == null) continue;
                m.Detach();
                m.Attach(grid, u.transform.position);
                m.SetBarricadeMap(barricadeMap);
            }
        }

        /// <summary>バリケードの追加/削除後に呼び出す。GridBarricadeMap の blocked セルを再計算し、
        /// 全 mover のキャッシュ path も破棄して次フレで再計画させる。</summary>
        public void RebuildBarricadeMap()
        {
            if (barricadeMap is GridBarricadeMap gb) gb.Rebuild();
            foreach (var u in AllUnits)
            {
                if (u != null && u.mover != null) u.mover.InvalidatePath();
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

            // 移動フェーズ: 全 mover step + push-out 反復 + フィールドクランプ
            MovementPhase(BattleDeltaTime);

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

        // ─────────────────────────────────
        // 連続移動フェーズ (Python _movement_phase 1:1 移植)
        // ─────────────────────────────────

        /// <summary>全ユニット mover.StepBattle → push-out 反復 (20回 max) → フィールドクランプ。
        /// Python battle_manager._movement_phase の 1:1 移植。</summary>
        private void MovementPhase(float dt)
        {
            // 各ユニットの step
            foreach (var u in AllUnits)
            {
                if (u == null || !u.IsAlive() || u.mover == null) continue;
                u.mover.StepBattle(dt, BattleTime);
            }

            // Barricade lifetime チェック (期限 / HP=0 で破棄)
            bool barricadesChanged = false;
            for (int i = barricades.Count - 1; i >= 0; i--)
            {
                var b = barricades[i];
                if (b == null) { barricades.RemoveAt(i); barricadesChanged = true; continue; }
                if (b.IsExpired(BattleTime))
                {
                    BattleLog.Add($"[{ElapsedSec:F1}s] {b.owner?.displayName} Barricade 消滅");
                    b.DestroySelf();
                    barricades.RemoveAt(i);
                    barricadesChanged = true;
                }
            }
            if (barricadesChanged) RebuildBarricadeMap();

            // push-out 反復 (重なり解消) — unit-unit + unit-barricade。
            for (int iter = 0; iter < 20; iter++)
            {
                float maxOverlap = 0f;
                foreach (var (a, b) in UnitPairs())
                {
                    float ov = ResolveOverlap(a.mover, b.mover);
                    if (ov > maxOverlap) maxOverlap = ov;
                }
                // unit-barricade: 全 alive unit を全 active barricade で押し出す
                foreach (var u in AllUnits)
                {
                    if (u == null || !u.IsAlive() || u.mover == null) continue;
                    foreach (var bar in barricades)
                    {
                        if (bar == null || !bar.IsActive) continue;
                        float ov = ResolveUnitBarricadeOverlap(u, bar);
                        if (ov > maxOverlap) maxOverlap = ov;
                    }
                }
                if (maxOverlap < 0.001f) break;
            }

            // フィールドクランプ (SimpleGrid 経由)
            var grid = SimpleGrid.Active;
            if (grid != null)
            {
                foreach (var u in AllUnits)
                {
                    if (u == null || !u.IsAlive() || u.mover == null) continue;
                    // mover が attach されていれば StepBattle 内でもクランプ済みだが念のため
                    if (u.mover.simulateMovement) continue; // StepBattle 済みのものは既にクランプ済
                    // idle 状態でも強制的にフィールド内に収める
                    Vector3 clamped = grid.ClampToField(u.transform.position);
                    clamped.y = u.transform.position.y;
                    if ((clamped - u.transform.position).sqrMagnitude > 0.01f)
                        u.transform.position = clamped;
                }
            }
        }

        /// <summary>alive かつ attached な全ユニットのペアを返す (O(N^2), N<20 想定)。
        /// Python _unit_pairs の 1:1 移植。broadphase で半径和より遠いペアをスキップ。</summary>
        private System.Collections.Generic.IEnumerable<(RealtimeBattleUnit, RealtimeBattleUnit)> UnitPairs()
        {
            // alive かつ mover attached なユニットを収集
            var alive = new List<RealtimeBattleUnit>();
            foreach (var u in AllUnits)
            {
                if (u == null || !u.IsAlive() || u.mover == null) continue;
                alive.Add(u);
            }

            for (int i = 0; i < alive.Count; i++)
            {
                for (int j = i + 1; j < alive.Count; j++)
                {
                    var a = alive[i];
                    var b = alive[j];
                    // broadphase: 半径和より遠ければスキップ
                    float ra = a.mover.radius;
                    float rb = b.mover.radius;
                    float dx = a.transform.position.x - b.transform.position.x;
                    float dz = a.transform.position.z - b.transform.position.z;
                    float distSq = dx * dx + dz * dz;
                    float sumR = ra + rb;
                    if (distSq > sumR * sumR) continue;
                    yield return (a, b);
                }
            }
        }

        /// <summary>2 ユニット間の重なりを push-out で解消する。anchored のユニットは動かない。
        /// 完全一致時は X+ 方向で強制分離。
        /// Python _resolve_overlap の 1:1 移植。戻り値: 解消した overlap 量 (重なってなければ 0)。</summary>
        private float ResolveOverlap(
            Pathfinding.ContinuousMover a,
            Pathfinding.ContinuousMover b)
        {
            if (a == null || b == null) return 0f;

            float dx = a.transform.position.x - b.transform.position.x;
            float dz = a.transform.position.z - b.transform.position.z;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);
            float minDist = a.radius + b.radius;

            if (dist >= minDist) return 0f;

            // 完全一致時は X+ 方向で強制分離
            if (dist < 1e-9f)
            {
                dx = 1f; dz = 0f; dist = 1f;
            }

            float overlap = minDist - dist;
            float nx = dx / dist;
            float nz = dz / dist;

            bool aAnchored = a.IsAnchored;
            bool bAnchored = b.IsAnchored;

            if (aAnchored && bAnchored)
                return 0f;
            else if (aAnchored)
            {
                // a は固定、b を押し出す
                b.transform.position = new Vector3(
                    b.transform.position.x - nx * overlap,
                    b.transform.position.y,
                    b.transform.position.z - nz * overlap);
            }
            else if (bAnchored)
            {
                // b は固定、a を押し出す
                a.transform.position = new Vector3(
                    a.transform.position.x + nx * overlap,
                    a.transform.position.y,
                    a.transform.position.z + nz * overlap);
            }
            else
            {
                // 両者を半分ずつ押し出す
                float half = overlap * 0.5f;
                a.transform.position = new Vector3(
                    a.transform.position.x + nx * half,
                    a.transform.position.y,
                    a.transform.position.z + nz * half);
                b.transform.position = new Vector3(
                    b.transform.position.x - nx * half,
                    b.transform.position.y,
                    b.transform.position.z - nz * half);
            }

            return overlap;
        }

        /// <summary>攻撃発生時の外部通知 (ログ用)</summary>
        public void OnAttack(RealtimeBattleUnit attacker, RealtimeBattleUnit target, int dmg, int before, int after)
        {
            string dead = target.IsAlive() ? "" : " ★撃破";
            BattleLog.Add($"[{ElapsedSec:F1}s] {attacker.displayName} → {target.displayName} {dmg}ダメ (HP {before}→{after}){dead}");
        }

        /// <summary>Barricade に対する攻撃ログ (1ダメ固定)</summary>
        public void OnAttackBarricade(RealtimeBattleUnit attacker, Barricade target, int dmg, int before, int after)
        {
            string dead = target.IsActive ? "" : " ★破壊";
            string ownerName = target.owner != null ? target.owner.displayName : "?";
            BattleLog.Add($"[{ElapsedSec:F1}s] {attacker.displayName} → Barricade({ownerName}) {dmg}ダメ (HP {before}→{after}){dead}");
        }

        /// <summary>unit と Barricade の重なり解消。Barricade は不動 (anchor)、unit のみ押し出し。</summary>
        private float ResolveUnitBarricadeOverlap(RealtimeBattleUnit u, Barricade bar)
        {
            if (u == null || u.mover == null || bar == null || !bar.IsActive) return 0f;
            Vector3 up = u.transform.position;
            float r = u.mover.radius;
            Vector3 nearest = bar.ClosestPointOnWall(up);
            float dx = up.x - nearest.x;
            float dz = up.z - nearest.z;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);
            if (dist >= r) return 0f;
            // 完全一致 (壁の中) → Barricade.facing 方向に押し出す (caster 側 or 反対側、どちらでも壁の外へ)
            // Caster と u が同 side の場合は caster 側 (-facing) に、敵 side なら +facing (壁の外側) に押し出す
            Vector3 pushDir;
            if (dist < 1e-9f)
            {
                bool isAllyOfCaster = bar.owner != null && u.ownerSide == bar.owner.ownerSide;
                pushDir = isAllyOfCaster ? -bar.facing : bar.facing;
                dist = 0f;
            }
            else
            {
                pushDir = new Vector3(dx / dist, 0f, dz / dist);
            }
            float overlap = r - dist;
            u.transform.position = new Vector3(
                up.x + pushDir.x * overlap,
                up.y,
                up.z + pushDir.z * overlap);
            return overlap;
        }
    }
}
