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
        [Tooltip("チームサイズ (1戦あたりのユニット数)")]
        [Range(3, 10)]
        public int teamSize = 5;

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

        [Header("結果 (実行���に表示)")]
        [TextArea(5, 30)]
        public string resultLog = "";

        private static readonly SoulJobTendency[] ALL_TENDENCIES = new[]
        {
            SoulJobTendency.Warrior, SoulJobTendency.Knight, SoulJobTendency.Lancer,
            SoulJobTendency.Archer, SoulJobTendency.Mage,
        };

        private void Start()
        {
            if (!autoStart) return;
            if (batchCount > 1) RunBatch();
            else RunTestBattle();
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
            }

            float avgT = totalEndT / Mathf.Max(1, batchCount);
            string summary = $"\n--- サマリー ---\n味方勝利: {allyWins} ({100f * allyWins / batchCount:F1}%)\n敵勝利: {enemyWins} ({100f * enemyWins / batchCount:F1}%)\n引き分け: {draws} ({100f * draws / batchCount:F1}%)\n平均終了タイム: {avgT:F2}";
            allLogs.Add(summary);

            resultLog = string.Join("\n", allLogs);
            Debug.Log(resultLog);

            // ファイル保存
            string path = System.IO.Path.Combine(Application.dataPath, "../__battle_batch_log.txt");
            System.IO.File.WriteAllLines(path, allLogs, System.Text.Encoding.UTF8);
            Debug.Log($"[TestBattleStarter] バッチログ保存: {path}");
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

            // 5x5 での職業配置:
            // x=0: 前列 (Knight, Warrior), x=1: 中衛 (Lancer), x=3: 後衛 (Archer, Mage), x=4: 予備
            var rowSlots = new Dictionary<int, List<int>>
            {
                [0] = new List<int> { 0, 1, 2, 3, 4 },
                [1] = new List<int> { 0, 1, 2, 3, 4 },
                [2] = new List<int> { 0, 1, 2, 3, 4 },
                [3] = new List<int> { 0, 1, 2, 3, 4 },
                [4] = new List<int> { 0, 1, 2, 3, 4 },
            };

            int memberIdx = 0;
            foreach (var tendency in members)
            {
                int row = tendency switch
                {
                    SoulJobTendency.Knight => 0,
                    SoulJobTendency.Warrior => 0,
                    SoulJobTendency.Lancer => 1,
                    SoulJobTendency.Archer => 3,
                    SoulJobTendency.Mage => 3,
                    _ => 2,
                };
                // 指定列が満杯なら前後を順に探す
                if (rowSlots[row].Count == 0)
                {
                    for (int r = 0; r < 5; r++)
                        if (rowSlots[r].Count > 0) { row = r; break; }
                }
                if (rowSlots[row].Count == 0) break;

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

                // 行動リストがなければ自動生成してソ��ルに保存
                if (soul.GetActionList(bodyJobId) == null || soul.GetActionList(bodyJobId).Count == 0)
                {
                    var skills = CollectAllSkills(soul, body, db);
                    var actionList = ActionListBuilder.Build(skills, tactic, bodyJobId);
                    soul.SetActionList(bodyJobId, actionList);
                }

                Debug.Log($"[TestBattleStarter] 生��: {tendency} → Body={body.BodyJobId} Race={body.RaceId} Weapon={body.WeaponId}");

                placements.Add(new BattleUnitPlacement(soul, body, cell));
                memberIdx++;
            }

            return placements;
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
