using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// テスト用 MonoBehaviour: Play モードで MasterDatabase を使った本データ戦闘テスト。
    /// シーン上の GameObject に貼り付けて使う。テスト完了後に削除して OK。
    ///
    /// Inspector から設定:
    /// - allyRank / allyLevel / allyCount
    /// - enemyRank / enemyLevel / enemyCount
    /// - autoStart: true なら Start() で自動実行
    ///
    /// 実行方法:
    /// 1. Inspector で autoStart = true にして Play
    /// 2. または Inspector のコンテキストメニューから「テスト戦闘実行」
    /// </summary>
    public class TestBattleStarter : MonoBehaviour
    {
        [Header("共通設定")]
        public int rank = 3;
        public int level = 10;
        public bool autoStart = true;

        [Header("味方チーム (傾向ごとに1体ずつ生成。5傾向で最大5体)")]
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

        [Header("結果 (実行後に表示)")]
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

            Debug.Log("[TestBattleStarter] 戦闘開始...");

            var field = new BattleField();
            var manager = new BattleManager(field);

            // 味方チーム生成
            var allies = CreateTeam(rank, level, allyMembers);
            PlaceTeam(manager, field, allies, 0, allyTactic);

            // 敵チーム生成
            var enemies = CreateTeam(rank, level, enemyMembers);
            PlaceTeam(manager, field, enemies, 1, enemyTactic);

            // 戦闘実行
            manager.RunFullBattle();

            // ログ出力
            resultLog = string.Join("\n", manager.Log);
            Debug.Log(resultLog);

            // ファイル保存
            string path = System.IO.Path.Combine(Application.dataPath, "../__battle_log.txt");
            System.IO.File.WriteAllLines(path, manager.Log, System.Text.Encoding.UTF8);
            Debug.Log($"[TestBattleStarter] ログ保存: {path}");
        }

        /// <summary>SoulFactory + BodyFactory で正規のユニットを生成</summary>
        private List<(SpaceJourneyUnit unit, List<SkillDefinition> skills)> CreateTeam(
            int rank, int level, List<SoulJobTendency> members)
        {
            var team = new List<(SpaceJourneyUnit, List<SkillDefinition>)>();
            var db = MasterDatabase.Instance;

            foreach (var tendency in members)
            {
                // 傾向に対応するボディ職名
                string bodyJobId = tendency switch
                {
                    SoulJobTendency.Warrior => "Warrior",
                    SoulJobTendency.Knight => "Knight",
                    SoulJobTendency.Archer => "Archer",
                    SoulJobTendency.Mage => "Mage",
                    SoulJobTendency.Lancer => "Spearman",
                    _ => "Warrior",
                };
                // ソウル生成 (SoulFactory 経由)
                var soul = SoulFactory.Create(
                    rank: rank,
                    soulTendency: tendency,
                    level: level,
                    registerToWorld: false
                );

                if (soul == null)
                {
                    Debug.LogError($"[TestBattleStarter] SoulFactory.Create failed for rank={rank}");
                    continue;
                }

                // ボディ生成 (BodyFactory 経由)
                // bodyJobId が見つからない場合はランダム職で生成
                BodyInstance body;
                try
                {
                    body = BodyFactory.CreateRandom(rank: rank, bodyJobId: bodyJobId);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[TestBattleStarter] BodyJob '{bodyJobId}' not found, trying random: {e.Message}");
                    try
                    {
                        body = BodyFactory.CreateRandom(rank: rank);
                    }
                    catch (System.Exception e2)
                    {
                        Debug.LogError($"[TestBattleStarter] BodyFactory failed: {e2.Message}");
                        continue;
                    }
                }

                if (body == null)
                {
                    Debug.LogError($"[TestBattleStarter] BodyFactory returned null for rank={rank}");
                    continue;
                }

                Debug.Log($"[TestBattleStarter] 生成: {tendency} → Body={body.BodyJobId} Race={body.RaceId} Weapon={body.WeaponId}");

                var unit = new SpaceJourneyUnit(soul, body);

                // スキル収集: ボディ職基本スキル
                var skills = new List<SkillDefinition>();
                var bodyJob = db.GetBodyJobById(bodyJobId);
                if (bodyJob != null && bodyJob.baseSkills != null)
                {
                    foreach (var skill in bodyJob.baseSkills)
                    {
                        if (skill != null) skills.Add(skill);
                    }
                }

                // 武器パッシブスキル
                var weapon = db.GetWeaponById(body.WeaponId);
                if (weapon != null && weapon.effectSkill != null)
                    skills.Add(weapon.effectSkill);

                // 種族パッシブスキル
                var race = db.GetRaceById(body.RaceId);
                if (race != null && race.racialSkill != null)
                    skills.Add(race.racialSkill);

                // ソウル生業スキル (転生で覚えたスキル)
                var rein = soul.CurrentReinSoul;
                if (rein != null && rein.LearnedSkillIds != null)
                {
                    foreach (var skillId in rein.LearnedSkillIds)
                    {
                        var skill = db.GetSoulJobSkillById(skillId);
                        if (skill != null) skills.Add(skill);
                    }
                }

                team.Add((unit, skills));
            }

            return team;
        }

        /// <summary>フィールドにランダム配置して BattleManager に登録</summary>
        private void PlaceTeam(BattleManager manager, BattleField field,
            List<(SpaceJourneyUnit unit, List<SkillDefinition> skills)> team, int side,
            BattleTactic tactic)
        {
            // 空きマスをリストアップしてシャッフル
            var emptyCells = new List<Vector2Int>(field.Cells);

            // シャッフル
            for (int i = emptyCells.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (emptyCells[i], emptyCells[j]) = (emptyCells[j], emptyCells[i]);
            }

            int idx = 0;
            foreach (var (unit, skills) in team)
            {
                if (idx >= emptyCells.Count) break;
                var cell = emptyCells[idx];
                field.PlaceUnit(unit, side, cell.x, cell.y);
                manager.RegisterUnit(unit, ActionListBuilder.Build(skills, tactic));
                idx++;
            }
        }
    }
}
