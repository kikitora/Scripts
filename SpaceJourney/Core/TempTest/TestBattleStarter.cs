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

        private void Start()
        {
            if (autoStart) RunTestBattle();
        }

        [ContextMenu("テスト戦闘実行")]
        public void RunTestBattle()
        {
            var db = MasterDatabase.Instance;
            if (db == null)
            {
                Debug.LogError("[TestBattleStarter] MasterDatabase.Instance が null。シーンに MasterDatabase が必要。");
                return;
            }

            Debug.Log("[TestBattleStarter] BattleStartData 経由で戦闘開��...");

            // BattleStartData を組み立て
            var startData = new BattleStartData
            {
                fieldLayout = BattleFieldLayout.Default3x3(),
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

        /// <summary>傾向リストから BattleUnitPlacement を生成</summary>
        private List<BattleUnitPlacement> CreatePlacements(
            int rank, int level, List<SoulJobTendency> members, BattleTactic tactic)
        {
            var placements = new List<BattleUnitPlacement>();
            var db = MasterDatabase.Instance;

            // 職業に応じた配置列を決定
            // x=0: 前列 (Knight, Warrior), x=1: 中列 (Lancer), x=2: 後列 (Archer, Mage)
            var rowSlots = new Dictionary<int, List<int>>
            {
                [0] = new List<int> { 0, 1, 2 },
                [1] = new List<int> { 0, 1, 2 },
                [2] = new List<int> { 0, 1, 2 },
            };

            int memberIdx = 0;
            foreach (var tendency in members)
            {
                int row = tendency switch
                {
                    SoulJobTendency.Knight => 0,
                    SoulJobTendency.Warrior => 0,
                    SoulJobTendency.Lancer => 1,
                    SoulJobTendency.Archer => 2,
                    SoulJobTendency.Mage => 2,
                    _ => 1,
                };
                // 指定列が満杯なら隣の列を探す
                if (rowSlots[row].Count == 0)
                {
                    row = rowSlots[0].Count > 0 ? 0 : rowSlots[1].Count > 0 ? 1 : 2;
                }
                if (rowSlots[row].Count == 0) break;

                int yIdx = Random.Range(0, rowSlots[row].Count);
                int y = rowSlots[row][yIdx];
                rowSlots[row].RemoveAt(yIdx);
                var cell = new Vector2Int(row, y);

                string bodyJobId = OneReinSoulData.GetDefaultBodyJobId(tendency);

                // ソウル生成
                var soul = SoulFactory.Create(
                    rank: rank,
                    soulTendency: tendency,
                    level: level,
                    registerToWorld: false
                );
                if (soul == null)
                {
                    Debug.LogError($"[TestBattleStarter] SoulFactory.Create failed for {tendency}");
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
                    var actionList = ActionListBuilder.Build(skills, tactic);
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
