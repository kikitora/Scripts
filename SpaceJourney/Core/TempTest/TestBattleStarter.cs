using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// テスト用 MonoBehaviour: Play モードで MasterDatabase を使った本データ戦闘テスト。
    /// BattleStartData → BattleManager.StartBattle() の新フローで実行する。
    ///
    /// Inspector から設��:
    /// - rank / level
    /// - allyMembers / enemyMembers (傾向リスト)
    /// - allyTactic / enemyTactic
    /// - allyMorale / enemyMorale
    /// - initiativeSide
    /// - autoStart: true なら Start() で自動実行
    /// </summary>
    public class TestBattleStarter : MonoBehaviour
    {
        [Header("共通設定")]
        public int rank = 3;
        public int level = 10;
        public bool autoStart = true;

        [Header("転生由来ソウル")]
        [Tooltip("true にすると ReinSimRunner.RunFixedRank で転生経由ソウルを生成。" +
                 "共通レアスキル等の習得が戦闘に反映される。")]
        public bool useReincarnatedSouls = true;
        [Tooltip("転生ソウル生成時の才能ランク")]
        public TalentRank talentRank = TalentRank.C;

        [Header("バッチ実行")]
        [Tooltip("複数戦闘を連続実行して勝率統計を取る。1=通常の1戦")]
        [Range(1, 100)]
        public int batchCount = 1;
        [Tooltip("バッチ実行時、各戦闘でチーム構成をランダムにする")]
        public bool randomizeTeams = true;
        [Tooltip("配置を完全ランダム化する (PickPreferredRow を無視、5x5 のどのセルにも配置)")]
        public bool randomizePlacement = true;
        [Tooltip("チームサイズ (1戦あたりのユニット数)")]
        [Range(3, 10)]
        public int teamSize = 5;
        [Tooltip("Realtime バッチ実行時、戦闘間の待機秒数 (勝利モーション確認用)")]
        [Range(0f, 5f)]
        public float batchInterBattlePauseSec = 2f;
        [Tooltip("Realtime バッチ実行時、1 戦の最大実時間 (タイムリミット)。これを超えたら強制次戦へ")]
        [Range(10f, 180f)]
        public float batchMaxRealSecPerBattle = 60f;

        [Header("味方チーム")]
        public BattleTactic allyTactic = BattleTactic.Balanced;
        public List<SoulJobTendency> allyMembers = new()
        {
            SoulJobTendency.Warrior,
            SoulJobTendency.Knight,
            SoulJobTendency.Archer,
        };

        [Header("敵チーム")]
        public BattleTactic enemyTactic = BattleTactic.Balanced;
        public List<SoulJobTendency> enemyMembers = new()
        {
            SoulJobTendency.Warrior,
            SoulJobTendency.Mage,
            SoulJobTendency.Lancer,
        };

        [Header("戦闘条件")]
        [Tooltip("先制側: 0=味方, 1=敵, -1=なし")]
        public int initiativeSide = -1;
        [Range(0, 100)] public float allyMorale = 100f;
        [Range(0, 100)] public float enemyMorale = 100f;

        [Header("共通レア強制注入 (テスト用)")]
        [Tooltip("各ユニットに共通レアスキルをN個ランダムに強制学習させる (Python --common-rare 相当)。0=無効")]
        [Range(0, 6)]
        public int forceCommonRareCount = 0;

        // 共通レアスキル全18件 (Assets/.../SoulJobSkill/CommonRare/)
        private static readonly string[] COMMON_RARE_SKILL_IDS = new[]
        {
            "skill_mentor_wisdom", "skill_mentor_inspire", "skill_mentor_insight",
            "skill_train_focus", "skill_train_versatile", "skill_train_breakthrough",
            "skill_nature_storm", "skill_nature_starlight", "skill_nature_bond",
            "skill_save_heal", "skill_save_guard", "skill_save_protect",
            "skill_nd_tenacity", "skill_nd_recovery", "skill_nd_survival",
            "skill_betray_counter", "skill_betray_vigilance", "skill_betray_endure",
        };

        [Header("結果 (実行���に表示)")]
        [TextArea(5, 30)]
        public string resultLog = "";

        private static readonly SoulJobTendency[] ALL_TENDENCIES = new[]
        {
            SoulJobTendency.Warrior, SoulJobTendency.Knight, SoulJobTendency.Lancer,
            SoulJobTendency.Archer, SoulJobTendency.Mage,
        };

        [Header("3Dビジュアル")]
        [Tooltip("設定すると戦闘開始時に3Dモデルをグリッド上に配置")]
        public BattleUnitSpawner unitSpawner;

        [Tooltip("true: 戦闘を1tickずつ進行させてモデルの移動を可視化 (batchCount=1時のみ)")]
        public bool visualMode = true;

        [Tooltip("true: リアルタイム戦闘 (連続時間、30秒制限)。false: 従来のタイムポイント制")]
        public bool useRealtimeBattle = true;

        [Tooltip("リアルタイム戦闘の制限時間 (秒)")]
        [Range(10f, 120f)]
        public float realtimeMaxSec = 30f;

        [Header("Action RPG 型プロトタイプ (Realtime)")]
        [Tooltip("true: 新 Realtime (自由移動+円形射程) プロト使用。false: 既存ロジック。")]
        public bool useActionRpgProto = false;

        [Tooltip("Realtime プロトスターター")]
        public SteraCube.SpaceJourney.Realtime.RealtimeBattleStarter realtimeStarter;

        [Tooltip("攻撃アクションの待機秒数 (visualMode時、攻撃マス点滅表示の見せ場)")]
        [Range(0.1f, 3f)]
        public float tickIntervalSec = 1.2f;

        [Tooltip("被弾のみ (攻撃者でなく被害者側) ユニットの追加待機秒数")]
        [Range(0f, 2f)]
        public float damageOnlyExtraSec = 0f;

        [Tooltip("tick境界の追加待機秒数 (visualMode時)")]
        [Range(0f, 2f)]
        public float tickBoundaryPauseSec = 0.3f;

        [Tooltip("全体再生速度倍率 (2=2倍速, 0.5=半速)")]
        [Range(0.25f, 4f)]
        public float playbackSpeedMultiplier = 1f;

        [Tooltip("UIログ表示 (任意)")]
        public BattleLogUI logUI;

        [Tooltip("セル点滅演出 (任意)")]
        public CellHighlighter cellHighlighter;

        [Header("機能発火検証")]
        [Tooltip("バッチ実行時、各ログ内のマーカー出現数を集計して表示")]
        public bool countFeatureFires = false;

        // ログ内検出マーカー → 機能名 (Python run_batch と同一)
        private static readonly (string key, string marker)[] FEATURE_MARKERS = new[]
        {
            ("plow_thrust",         "マス突進"),
            ("team_teleport",       "瞬間移動"),
            ("randomize_all",       "位置をシャッフル"),
            ("randomized_effect",   "ランダム効果"),
            ("shoulder",            "踏ん張り発動"),
            ("next_attack_stun",    "(未来を見通す眼)"),
            ("cover_ally",          "身代わり!"),
            ("counter",             "反撃!"),
            ("survive_lethal",      "不屈!"),
            ("swap_ally",           "位置交換"),
            ("pull_enemy",          "を引き寄せ"),
            ("dispel",              "個解除"),
            ("steal_buffs",         "個奪取"),
            ("double_action",       "連撃準備完了"),
            ("random_move_enemies", "体をランダム移動"),
            ("experiment_fail",     "実験失敗"),
            ("summon_barricade",    "バリケード3体配置"),
            ("barricade_fail",      "配置失敗"),
            ("summon_ally",         "守護霊召喚 ("),
            ("zanshin",             "残心発動"),
        };

        private void Start()
        {
            if (!autoStart) return;
            // Realtime バッチ: useActionRpgProto && batchCount > 1
            if (useActionRpgProto && batchCount > 1)
            {
                StartCoroutine(RunRealtimeBatchCoroutine());
                return;
            }
            if (batchCount > 1) RunBatch();
            else RunTestBattle();
        }

        [ContextMenu("Realtime バッチ戦闘実行")]
        public void RunRealtimeBatch()
        {
            StartCoroutine(RunRealtimeBatchCoroutine());
        }

        /// <summary>Realtime 戦闘を batchCount 回連続実行、ログを 1 ファイルに集約。
        /// チーム/配置ランダム、戦闘間に勝利モーション確認の小休止。</summary>
        private System.Collections.IEnumerator RunRealtimeBatchCoroutine()
        {
            if (realtimeStarter == null)
            {
                Debug.LogError("[TestBattleStarter] realtimeStarter 未設定 (Inspector)");
                yield break;
            }
            var startWallClock = System.DateTime.Now;
            var sessionLogs = new List<string>();
            int allyWins = 0, enemyWins = 0, draws = 0;
            float totalBattleTime = 0f;

            sessionLogs.Add($"=== Realtime バッチ開始 {batchCount} 戦 ({startWallClock:yyyy-MM-dd HH:mm:ss}) ===");
            sessionLogs.Add($"teamSize={teamSize} randomizeTeams={randomizeTeams} randomizePlacement={randomizePlacement} rank={rank}");

            for (int i = 0; i < batchCount; i++)
            {
                var allyTends = randomizeTeams ? PickRandomTeam(teamSize) : allyMembers;
                var enemyTends = randomizeTeams ? PickRandomTeam(teamSize) : enemyMembers;

                var startData = new BattleStartData
                {
                    fieldLayout = BattleFieldLayout.Default5x5(),
                    allyUnits = CreatePlacements(rank, level, allyTends, allyTactic),
                    enemyUnits = CreatePlacements(rank, level, enemyTends, enemyTactic),
                    initiativeSide = initiativeSide,
                    allyMorale = allyMorale,
                    enemyMorale = enemyMorale,
                };

                sessionLogs.Add($"\n========== 戦 {i + 1}/{batchCount} ==========");
                sessionLogs.Add($"味方: {string.Join(",", allyTends)}");
                sessionLogs.Add($"敵  : {string.Join(",", enemyTends)}");

                // 前戦の残骸クリーンアップ (初回は no-op)
                CleanupRealtimeBattle();
                yield return null; // Destroy 反映を待つ 1 フレーム

                realtimeStarter.StartRealtimeBattle(startData);
                var manager = realtimeStarter.manager;
                if (manager == null)
                {
                    sessionLogs.Add("  [ERROR] manager null、スキップ");
                    continue;
                }

                // IsFinished 待機 (実時間タイムアウトで強制終了)
                float t0 = Time.realtimeSinceStartup;
                while (!manager.IsFinished)
                {
                    yield return null;
                    if (Time.realtimeSinceStartup - t0 > batchMaxRealSecPerBattle)
                    {
                        sessionLogs.Add($"  [WARN] 実時間 {batchMaxRealSecPerBattle}s 超過、強制中断");
                        break;
                    }
                }

                // ログ収集 + 集計
                sessionLogs.AddRange(manager.BattleLog);
                int ws = manager.WinningSide;
                if (ws == 0) allyWins++;
                else if (ws == 1) enemyWins++;
                else draws++;
                totalBattleTime += manager.ElapsedSec;

                sessionLogs.Add($"--- 戦{i + 1} 結果: {(ws == 0 ? "味方勝" : ws == 1 ? "敵勝" : "引分")} (戦闘時間 {manager.ElapsedSec:F1}s) ---");

                if (batchInterBattlePauseSec > 0f)
                    yield return new WaitForSeconds(batchInterBattlePauseSec);
            }

            // 最終クリーンアップ
            CleanupRealtimeBattle();

            // サマリ
            sessionLogs.Add($"\n========== バッチ完了 ==========");
            sessionLogs.Add($"味方勝利: {allyWins} ({100f * allyWins / batchCount:F1}%)");
            sessionLogs.Add($"敵勝利  : {enemyWins} ({100f * enemyWins / batchCount:F1}%)");
            sessionLogs.Add($"引き分け: {draws} ({100f * draws / batchCount:F1}%)");
            sessionLogs.Add($"平均戦闘時間: {totalBattleTime / Mathf.Max(1, batchCount):F2}s");
            sessionLogs.Add($"実行時間: {(System.DateTime.Now - startWallClock).TotalSeconds:F1}s (実時刻)");

            resultLog = string.Join("\n", sessionLogs);
            Debug.Log(resultLog);

            string ts = startWallClock.ToString("yyyyMMdd_HHmmss");
            string path = System.IO.Path.Combine(Application.dataPath, $"../__realtime_batch_{ts}.txt");
            try
            {
                System.IO.File.WriteAllLines(path, sessionLogs, System.Text.Encoding.UTF8);
                Debug.Log($"[TestBattleStarter] Realtime バッチログ保存: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TestBattleStarter] ログ保存失敗: {e.Message}");
            }
        }

        /// <summary>Realtime 戦闘の残骸 (ユニット/バリケード) を全削除し、manager 状態を次戦用にリセット。
        /// 次戦の SpawnSide → Setup() で BT=0 が見えるよう ResetForNextBattle() を呼ぶ。</summary>
        private void CleanupRealtimeBattle()
        {
            if (realtimeStarter == null) return;
            var mgr = realtimeStarter.manager;
            if (mgr != null)
            {
                foreach (var u in mgr.AllUnits)
                {
                    if (u != null && u.gameObject != null) Destroy(u.gameObject);
                }
                mgr.AllUnits.Clear();
                if (mgr.barricades != null)
                {
                    foreach (var b in mgr.barricades)
                    {
                        if (b != null && b.gameObject != null) Destroy(b.gameObject);
                    }
                    mgr.barricades.Clear();
                }
                mgr.BattleLog.Clear();
                // 前戦の BattleTime をリセット (これをやらないと次戦の Setup 内 skillNextReadyTime が
                // 前戦の終了時刻を起点に計算され、CT が永遠に明かないバグが発生する)
                mgr.ResetForNextBattle();
            }
        }

        [ContextMenu("テスト戦闘実行 (1戦)")]
        public void RunTestBattle()
        {
            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[TestBattleStarter] MasterDatabase.Instance が null。シーンに MasterDatabase が必要。");
                return;
            }

            Debug.Log("[TestBattleStarter] BattleStartData 経由で戦闘開始...");

            // BattleStartData を組み立て
            var startData = new BattleStartData
            {
                fieldLayout = BattleFieldLayout.Default5x5(),
                allyUnits = CreatePlacements(rank, level, allyMembers, allyTactic),
                enemyUnits = CreatePlacements(rank, level, enemyMembers, enemyTactic),
                initiativeSide = initiativeSide,
                allyMorale = allyMorale,
                enemyMorale = enemyMorale,
            };

            // Action RPG プロト (自由移動+円形射程)
            if (useActionRpgProto && realtimeStarter != null)
            {
                realtimeStarter.StartRealtimeBattle(startData);
                return;
            }

            // visualMode: コルーチンで段階実行
            if (visualMode && unitSpawner != null)
            {
                StartCoroutine(RunVisualBattle(startData));
                return;
            }

            // 通常実行: 3Dモデル静止配置 + 戦闘即時完走
            if (unitSpawner != null)
                unitSpawner.SpawnAll(startData);

            // StartBattle で一連の流れを実行
            var manager = BattleManager.StartBattle(startData);

            // ログ出力
            resultLog = string.Join("\n", manager.Log);
            Debug.Log(resultLog);

            // 戦闘結果
            var result = BattleResult.FromManager(manager);
            Debug.Log($"[TestBattleStarter] 勝者: {(result.winningSide == 0 ? "味方" : result.winningSide == 1 ? "敵" : "引分")}");

            for (int s = 0; s < 2; s++)
            {
                var sr = result.sideResults[s];
                string label = s == 0 ? "味方" : "敵";
                float moraleLoss = BattleResult.CalcMoraleLoss(sr, null);
                int vpDamage = BattleResult.CalcVpDamage(sr, null);
                Debug.Log($"  [{label}] 死亡{sr.deadUnits}/{sr.totalUnits} dmgRatio={sr.dmgRatio:P0} 全滅={sr.wiped}");
            }

            // ファイル保存
            string path = System.IO.Path.Combine(Application.dataPath, "../__battle_log.txt");
            System.IO.File.WriteAllLines(path, manager.Log, System.Text.Encoding.UTF8);
            Debug.Log($"[TestBattleStarter] ログ保存: {path}");
        }

        /// <summary>
        /// visualMode 用: 戦闘を 1 tick ずつ進行させて 3D モデル位置を更新するコルーチン。
        /// </summary>
        private System.Collections.IEnumerator RunVisualBattle(BattleStartData startData)
        {
            Debug.Log("[TestBattleStarter] visualMode 戦闘開始...");

            // リアルタイム戦闘の制限時間をセット
            BattleManager.RealtimeMaxSec = realtimeMaxSec;

            var manager = BattleManager.InitBattle(startData);
            if (manager == null) yield break;

            // UI/演出の初期化
            if (logUI != null) logUI.Clear();

            // モデル配置
            unitSpawner.SpawnAll(manager, startData);
            if (logUI != null) logUI.SyncFromManager(manager);
            yield return null;

            float speedInv = 1f / Mathf.Max(0.01f, playbackSpeedMultiplier);

            if (useRealtimeBattle)
            {
                // リアルタイム: 各アクションは実クロック進行と連動
                float prevClock = 0f;
                foreach (var clockValue in manager.RunRealtimeStepByStep())
                {
                    unitSpawner.UpdatePositions(manager);
                    if (logUI != null) logUI.SyncFromManager(manager);
                    if (cellHighlighter != null) cellHighlighter.HighlightTick(manager);

                    // ゲーム内時間の経過分を実秒 (速度倍率調整) で待つ
                    float elapsed = Mathf.Max(0f, clockValue - prevClock);
                    prevClock = clockValue;
                    float wait = Mathf.Max(0.1f, elapsed) * speedInv;
                    yield return new WaitForSeconds(wait);
                }
            }
            else
            {
                // 従来のタイムポイント制
                int prevTime = manager.CurrentTime;
                foreach (var tickValue in manager.RunStepByStep())
                {
                    unitSpawner.UpdatePositions(manager);
                    if (logUI != null) logUI.SyncFromManager(manager);
                    if (cellHighlighter != null) cellHighlighter.HighlightTick(manager);

                    bool isTickBoundary = tickValue != prevTime;
                    prevTime = tickValue;

                    if (isTickBoundary)
                    {
                        if (tickBoundaryPauseSec > 0f)
                            yield return new WaitForSeconds(tickBoundaryPauseSec * speedInv);
                    }
                    else
                    {
                        yield return new WaitForSeconds(tickIntervalSec * speedInv);
                    }
                }
            }

            // 最終状態を1回反映
            unitSpawner.UpdatePositions(manager);
            if (logUI != null) logUI.SyncFromManager(manager);

            resultLog = string.Join("\n", manager.Log);
            Debug.Log(resultLog);

            var result = BattleResult.FromManager(manager);
            Debug.Log($"[TestBattleStarter] 勝者: {(result.winningSide == 0 ? "味方" : result.winningSide == 1 ? "敵" : "引分")}");

            string path = System.IO.Path.Combine(Application.dataPath, "../__battle_log.txt");
            System.IO.File.WriteAllLines(path, manager.Log, System.Text.Encoding.UTF8);
            Debug.Log($"[TestBattleStarter] ログ保存: {path}");
        }

        [ContextMenu("バッチ戦闘実行 (batchCount戦)")]
        public void RunBatch()
        {
            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[TestBattleStarter] MasterDatabase.Instance が null。");
                return;
            }

            int allyWins = 0, enemyWins = 0, draws = 0;
            float totalEndT = 0;
            var allLogs = new List<string>();
            var detailedLogs = new List<string>();
            var featureCounts = new Dictionary<string, int>();
            foreach (var (key, _) in FEATURE_MARKERS) featureCounts[key] = 0;
            allLogs.Add($"=== バッチ実行 {batchCount}戦 / ランダム={randomizeTeams} / チームサイズ={teamSize} ===");

            for (int i = 0; i < batchCount; i++)
            {
                var allyTends = randomizeTeams ? PickRandomTeam(teamSize) : allyMembers;
                var enemyTends = randomizeTeams ? PickRandomTeam(teamSize) : enemyMembers;

                var startData = new BattleStartData
                {
                    fieldLayout = BattleFieldLayout.Default5x5(),
                    allyUnits = CreatePlacements(rank, level, allyTends, allyTactic),
                    enemyUnits = CreatePlacements(rank, level, enemyTends, enemyTactic),
                    initiativeSide = initiativeSide,
                    allyMorale = allyMorale,
                    enemyMorale = enemyMorale,
                };

                var manager = BattleManager.StartBattle(startData);
                int ws = manager.WinningSide;
                if (ws == 0) allyWins++;
                else if (ws == 1) enemyWins++;
                else draws++;
                totalEndT += manager.CurrentTime;

                string allyStr = string.Join(",", allyTends);
                string enemyStr = string.Join(",", enemyTends);
                string winStr = ws == 0 ? "味方勝" : ws == 1 ? "敵勝" : "引分";
                allLogs.Add($"戦{i + 1}: {winStr} (t={manager.CurrentTime}) 味方[{allyStr}] vs 敵[{enemyStr}]");

                // 機能発火カウント集計 (各戦闘の全ログを走査)
                if (countFeatureFires)
                {
                    foreach (var line in manager.Log)
                    {
                        foreach (var (key, marker) in FEATURE_MARKERS)
                        {
                            if (line.Contains(marker)) featureCounts[key]++;
                        }
                    }
                }

                // 詳細ログ蓄積 (1戦ごとに区切り)
                if (countFeatureFires)
                {
                    detailedLogs.Add($"\n===== 戦{i + 1} =====");
                    detailedLogs.AddRange(manager.Log);
                }
            }

            float avgT = totalEndT / Mathf.Max(1, batchCount);
            string summary = $"\n--- サマリー ---\n味方勝利: {allyWins} ({100f * allyWins / batchCount:F1}%)\n敵勝利: {enemyWins} ({100f * enemyWins / batchCount:F1}%)\n引き分け: {draws} ({100f * draws / batchCount:F1}%)\n平均終了タイム: {avgT:F2}";
            allLogs.Add(summary);

            // 機能発火サマリー
            if (countFeatureFires)
            {
                allLogs.Add("\n=== 新機能発火カウント (全戦合計) ===");
                foreach (var (key, _) in FEATURE_MARKERS)
                {
                    int n = featureCounts[key];
                    float per = (float)n / Mathf.Max(1, batchCount);
                    allLogs.Add($"  {key,-22}: {n,5}  ({per:F2}/戦)");
                }
            }

            resultLog = string.Join("\n", allLogs);
            Debug.Log(resultLog);

            // ファイル保存
            string path = System.IO.Path.Combine(Application.dataPath, "../__battle_batch_log.txt");
            System.IO.File.WriteAllLines(path, allLogs, System.Text.Encoding.UTF8);
            Debug.Log($"[TestBattleStarter] バッチログ保存: {path}");

            // countFeatureFires=true のときは詳細全ログも保存 (マーカー検証用)
            if (countFeatureFires)
            {
                string dpath = System.IO.Path.Combine(Application.dataPath, "../__battle_batch_detail_log.txt");
                System.IO.File.WriteAllLines(dpath, detailedLogs, System.Text.Encoding.UTF8);
                Debug.Log($"[TestBattleStarter] 詳細ログ保存: {dpath}");
            }
        }

        private static List<SoulJobTendency> PickRandomTeam(int size)
        {
            var list = new List<SoulJobTendency>(size);
            for (int i = 0; i < size; i++)
                list.Add(ALL_TENDENCIES[Random.Range(0, ALL_TENDENCIES.Length)]);
            return list;
        }

        /// <summary>傾向リストから BattleUnitPlacement を生成</summary>
        private List<BattleUnitPlacement> CreatePlacements(
            int rank, int level, List<SoulJobTendency> members, BattleTactic tactic)
        {
            var placements = new List<BattleUnitPlacement>();
            var db = MasterDatabase.Instance;

            // 5x5 での職業配置: x=0(前列) 〜 x=4(後列)
            var rowSlots = new Dictionary<int, List<int>>
            {
                [0] = new List<int> { 0, 1, 2, 3, 4 },
                [1] = new List<int> { 0, 1, 2, 3, 4 },
                [2] = new List<int> { 0, 1, 2, 3, 4 },
                [3] = new List<int> { 0, 1, 2, 3, 4 },
                [4] = new List<int> { 0, 1, 2, 3, 4 },
            };
            const int maxRow = 5;

            int memberIdx = 0;
            foreach (var tendency in members)
            {
                // 配置ロジック: randomizePlacement なら preferred row 無視、
                //               偏らず空きセルからランダムで pick
                int row;
                if (randomizePlacement)
                {
                    // 空き row のうちランダム
                    var openRows = new List<int>();
                    for (int r = 0; r < maxRow; r++)
                        if (rowSlots[r].Count > 0) openRows.Add(r);
                    if (openRows.Count == 0) break;
                    row = openRows[Random.Range(0, openRows.Count)];
                }
                else
                {
                    row = PickPreferredRow(tendency);
                    if (row >= maxRow) row = maxRow - 1;
                    // 指定列が満杯なら前後を順に探す
                    if (rowSlots[row].Count == 0)
                    {
                        for (int r = 0; r < maxRow; r++)
                            if (rowSlots[r].Count > 0) { row = r; break; }
                    }
                    if (rowSlots[row].Count == 0) break;
                }

                int yIdx = Random.Range(0, rowSlots[row].Count);
                int y = rowSlots[row][yIdx];
                rowSlots[row].RemoveAt(yIdx);
                var cell = new Vector2Int(row, y);

                string bodyJobId = OneReinSoulData.GetDefaultBodyJobId(tendency);

                // ソウル生成
                SoulInstance soul;
                if (useReincarnatedSouls)
                {
                    // 転生経由: RunFixedRank で OneReinSoulData を生成し SoulInstance に追加
                    soul = SoulInstance.CreateRandomInitialSoul(rank, tendency, registerToWorld: false);
                    var reinData = ReinSimRunner.RunFixedRank(
                        fixedRank: rank,
                        tendency: tendency,
                        talentRank: talentRank,
                        allEvents: db.ReinLifeEvents);
                    if (reinData != null) soul.AddReinSoul(reinData);
                    // 共通レアスキル数をログ
                    var rareCount = 0;
                    if (reinData?.LearnedSkillIds != null)
                    {
                        foreach (var sid in reinData.LearnedSkillIds)
                            if (sid != null && sid.StartsWith("skill_")) rareCount++;
                    }
                    if (rareCount > 0)
                        Debug.Log($"[TestBattleStarter] 転生ソウル: {tendency} 共通レア{rareCount}個習得");
                }
                else
                {
                    soul = SoulFactory.Create(
                        rank: rank,
                        soulTendency: tendency,
                        level: level,
                        registerToWorld: false
                    );
                }
                if (soul == null)
                {
                    Debug.LogError($"[TestBattleStarter] soul creation failed for {tendency}");
                    continue;
                }

                // ボディ生成
                BodyInstance body;
                try
                {
                    body = BodyFactory.CreateRandom(rank: rank, bodyJobId: bodyJobId);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[TestBattleStarter] BodyJob '{bodyJobId}' not found, trying random: {e.Message}");
                    try { body = BodyFactory.CreateRandom(rank: rank); }
                    catch (System.Exception e2)
                    {
                        Debug.LogError($"[TestBattleStarter] BodyFactory failed: {e2.Message}");
                        continue;
                    }
                }
                if (body == null) continue;

                // 行動リストがなければ自動生成してソ��ルに保存。
                // 共通レア強制注入モード時は既存 action_list を無視して再ビルド。
                bool needRebuild = soul.GetActionList(bodyJobId) == null
                                 || soul.GetActionList(bodyJobId).Count == 0
                                 || forceCommonRareCount > 0;
                if (needRebuild)
                {
                    var skills = CollectAllSkills(soul, body, db);
                    // テスト用: 共通レアスキル強制注入 (Python --common-rare 相当)
                    if (forceCommonRareCount > 0)
                    {
                        var pool = new List<string>(COMMON_RARE_SKILL_IDS);
                        int pickN = Mathf.Min(forceCommonRareCount, pool.Count);
                        for (int k = 0; k < pickN; k++)
                        {
                            int idx = Random.Range(0, pool.Count);
                            var rareId = pool[idx]; pool.RemoveAt(idx);
                            var rareSkill = db.GetSoulJobSkillById(rareId);
                            if (rareSkill != null && !skills.Contains(rareSkill))
                                skills.Add(rareSkill);
                        }
                    }
                    var actionList = ActionListBuilder.Build(skills, tactic, bodyJobId);
                    soul.SetActionList(bodyJobId, actionList);
                }

                Debug.Log($"[TestBattleStarter] 生��: {tendency} → Body={body.BodyJobId} Race={body.RaceId} Weapon={body.WeaponId}");

                placements.Add(new BattleUnitPlacement(soul, body, cell));
                memberIdx++;
            }

            return placements;
        }

        /// <summary>
        /// 職業傾向ごとに配置行を重み付きランダム選択する (5x5 用)。
        /// row 0=前列 〜 row 4=後列。
        /// </summary>
        private static int PickPreferredRow(SoulJobTendency tendency)
        {
            int[] weights = tendency switch
            {
                SoulJobTendency.Knight  => new[] { 80, 15,  5,  0,  0 },
                SoulJobTendency.Warrior => new[] { 45, 35, 15,  5,  0 },
                SoulJobTendency.Lancer  => new[] { 10, 35, 35, 15,  5 },
                SoulJobTendency.Archer  => new[] {  0, 10, 25, 35, 30 },
                SoulJobTendency.Mage    => new[] {  0,  5, 15, 35, 45 },
                _                        => new[] { 20, 20, 20, 20, 20 },
            };

            int total = 0;
            foreach (var w in weights) total += w;
            int r = Random.Range(0, total);
            for (int i = 0; i < weights.Length; i++)
            {
                if (r < weights[i]) return i;
                r -= weights[i];
            }
            return 0;
        }

        /// <summary>ソウル+ボディから全スキルを収集</summary>
        private static List<SkillDefinition> CollectAllSkills(SoulInstance soul, BodyInstance body, MasterDatabase db)
        {
            var skills = new List<SkillDefinition>();

            // ボディ職基本スキル
            var bodyJob = db.GetBodyJobById(body.BodyJobId);
            if (bodyJob?.baseSkills != null)
            {
                foreach (var skill in bodyJob.baseSkills)
                    if (skill != null) skills.Add(skill);
            }

            // 武器パッシブ
            var weapon = db.GetWeaponById(body.WeaponId);
            if (weapon?.effectSkill != null) skills.Add(weapon.effectSkill);

            // 種族パッシブ
            var race = db.GetRaceById(body.RaceId);
            if (race?.racialSkill != null) skills.Add(race.racialSkill);

            // ソウル生業スキル
            var rein = soul.CurrentReinSoul;
            if (rein?.LearnedSkillIds != null)
            {
                foreach (var skillId in rein.LearnedSkillIds)
                {
                    var skill = db.GetSoulJobSkillById(skillId);
                    if (skill != null) skills.Add(skill);
                }
            }

            return skills;
        }
    }
}
