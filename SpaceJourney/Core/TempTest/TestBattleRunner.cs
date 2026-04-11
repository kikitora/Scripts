using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// テスト用: ランク/レベル指定で味方・敵チームを生成して戦闘を実行する。
    /// テスト完了後にこのファイルごと削除して OK。
    /// </summary>
    public static class TestBattleRunner
    {
        public static BattleManager LastManager { get; private set; }

        // ボディ職定義: 名前, ステ倍率[HP,AT,DF,AGI,MAT,MDF], 初期配置列(x), スキル生成
        private struct BodyJobProfile
        {
            public string name;
            public float hpMul, atMul, dfMul, agiMul, matMul, mdfMul;
            public int startColumn; // 0=前列, 1=中列, 2=後列
        }

        private static readonly BodyJobProfile[] Profiles = new[]
        {
            new BodyJobProfile { name="武士",  hpMul=1.1f, atMul=1.2f, dfMul=1.0f, agiMul=1.1f, matMul=0.8f, mdfMul=0.9f, startColumn=0 },
            new BodyJobProfile { name="騎士",  hpMul=1.3f, atMul=0.8f, dfMul=1.4f, agiMul=0.8f, matMul=0.7f, mdfMul=1.2f, startColumn=0 },
            new BodyJobProfile { name="弓兵",  hpMul=0.9f, atMul=1.1f, dfMul=0.8f, agiMul=1.0f, matMul=0.8f, mdfMul=0.9f, startColumn=2 },
            new BodyJobProfile { name="魔術師", hpMul=0.8f, atMul=0.6f, dfMul=0.7f, agiMul=0.8f, matMul=1.4f, mdfMul=1.1f, startColumn=2 },
            new BodyJobProfile { name="槍兵",  hpMul=1.15f, atMul=1.05f, dfMul=1.1f, agiMul=0.95f, matMul=0.8f, mdfMul=0.95f, startColumn=1 },
        };

        public static List<string> RunTestBattle(
            int allyRank = 3, int allyLevel = 10,
            int enemyRank = 3, int enemyLevel = 10,
            int allyCount = 3, int enemyCount = 3)
        {
            var field = new BattleField();
            var manager = new BattleManager(field);

            var allies = GenerateTeam(allyRank, allyLevel, allyCount);
            PlaceAndRegister(manager, field, allies, 0);

            var enemies = GenerateTeam(enemyRank, enemyLevel, enemyCount);
            PlaceAndRegister(manager, field, enemies, 1);

            manager.RunFullBattle();
            LastManager = manager;
            return manager.Log;
        }

        private static List<(SpaceJourneyUnit unit, List<SkillDefinition> skills, int startCol)> GenerateTeam(
            int rank, int level, int count)
        {
            var team = new List<(SpaceJourneyUnit, List<SkillDefinition>, int)>();
            count = Mathf.Clamp(count, 1, 9);
            for (int i = 0; i < count; i++)
            {
                int idx = i % Profiles.Length;
                var p = Profiles[idx];
                var (unit, skills) = CreateTestUnit(rank, level, p);
                team.Add((unit, skills, p.startColumn));
            }
            return team;
        }

        private static (SpaceJourneyUnit, List<SkillDefinition>) CreateTestUnit(int rank, int level, BodyJobProfile p)
        {
            rank = Mathf.Clamp(rank, 1, 10);
            level = Mathf.Clamp(level, 1, 25);

            float talentFactor = 1.2f;
            float[] muls = { p.atMul, p.dfMul, p.agiMul, p.matMul, p.mdfMul };
            int[] lv1Stats = new int[5];
            int[] soulStats = new int[5];
            for (int i = 0; i < 5; i++)
            {
                float baseStat = SpaceJourneyStatMath.CalcBaseStat(rank, muls[i]);
                float potential = SpaceJourneyStatMath.CalcPotentialStat(baseStat, talentFactor, 1.0f);
                lv1Stats[i] = SpaceJourneyStatMath.CalcLv1Stat(potential);
                soulStats[i] = SpaceJourneyStatMath.CalcSoulStat(lv1Stats[i], level, GrowthType.Normal, 6.25f);
            }

            // HP: ステ平均 × 5 × hpMul (10タイムで決着つく程度に調整)
            int avgStat = (soulStats[0] + soulStats[1] + soulStats[2] + soulStats[3] + soulStats[4]) / 5;
            int bodyHp = Mathf.RoundToInt(avgStat * 5 * p.hpMul) + rank * 15;

            var body = new BodyInstance("human", p.name, "test_weapon",
                maxHp: bodyHp,
                at: Mathf.RoundToInt(soulStats[0] * 0.4f),
                df: Mathf.RoundToInt(soulStats[1] * 0.4f),
                agi: Mathf.RoundToInt(soulStats[2] * 0.4f),
                mat: Mathf.RoundToInt(soulStats[3] * 0.4f),
                mdf: Mathf.RoundToInt(soulStats[4] * 0.4f));

            var soul = SoulInstance.CreateForTest(
                OneReinSoulData.CreateFromArgs(rank, GrowthType.Normal, null, TalentRank.C,
                    "テスト転生", level, lv1Stats,
                    new float[] { 6.25f, 6.25f, 6.25f, 6.25f, 6.25f },
                    null, null, null));

            var unit = new SpaceJourneyUnit(soul, body);
            var skills = CreateSkillsForProfile(p);
            return (unit, skills);
        }

        private static List<SkillDefinition> CreateSkillsForProfile(BodyJobProfile p)
        {
            var skills = new List<SkillDefinition>();

            if (p.name == "武士")
            {
                skills.Add(MakeSkill("斬撃", SkillCategory.ActiveAttack, 2, 2, SkillDamageKind.Physical, 100, prio: 3));
                skills.Add(MakeSkill("薙ぎ払い", SkillCategory.ActiveAttack, 2, 2, SkillDamageKind.Physical, 80, prio: 2,
                    conds: new ActionCondition(ActionConditionKind.EnemyCountInRange, 2)));
            }
            else if (p.name == "騎士")
            {
                skills.Add(MakeSkill("斬撃", SkillCategory.ActiveAttack, 2, 2, SkillDamageKind.Physical, 100, prio: 4));
                skills.Add(MakeSkill("防御", SkillCategory.ActiveSupport, 2, 2, SkillDamageKind.None, 0, prio: 2,
                    targetSide: EffectTargetSide.Self, targetMode: SkillTargetingMode.SelfArea,
                    conds: new ActionCondition(ActionConditionKind.SelfHpBelowRate, 0, 0.7f)));
            }
            else if (p.name == "弓兵")
            {
                skills.Add(MakeSkill("弓射", SkillCategory.ActiveAttack, 3, 3, SkillDamageKind.Physical, 100, prio: 3, tag: SkillTag.Projectile));
                skills.Add(MakeSkill("短剣突き", SkillCategory.ActiveAttack, 2, 2, SkillDamageKind.Physical, 50, prio: 5));
            }
            else if (p.name == "魔術師")
            {
                skills.Add(MakeSkill("バースト", SkillCategory.ActiveAttack, 4, 4, SkillDamageKind.Magical, 70, prio: 2, tag: SkillTag.Projectile));
                skills.Add(MakeSkill("短剣突き", SkillCategory.ActiveAttack, 2, 2, SkillDamageKind.Physical, 40, prio: 6));
            }
            else if (p.name == "槍兵")
            {
                skills.Add(MakeSkill("突撃", SkillCategory.ActiveAttack, 3, 3, SkillDamageKind.Physical, 90, prio: 3));
                skills.Add(MakeSkill("薙ぎ", SkillCategory.ActiveAttack, 3, 3, SkillDamageKind.Physical, 60, prio: 4));
            }

            skills.Add(MakeSkill("前進", SkillCategory.ActiveMove, 1, 1, SkillDamageKind.None, 0, prio: 8,
                conds: new ActionCondition(ActionConditionKind.NoEnemyForward)));

            return skills;
        }

        private static SkillDefinition MakeSkill(string name, SkillCategory cat, int cost, int rc,
            SkillDamageKind dmg, int amount, int prio,
            SkillTag tag = SkillTag.None,
            EffectTargetSide targetSide = EffectTargetSide.Enemy,
            SkillTargetingMode targetMode = SkillTargetingMode.PointArea,
            params ActionCondition[] conds)
        {
            var s = ScriptableObject.CreateInstance<SkillDefinition>();
            var t = typeof(SkillDefinition);
            t.GetField("skillId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(s, name);
            t.GetField("skillName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(s, name);
            s.category = cat;
            s.baseCost = cost;
            s.reuseCycle = rc;
            s.damageKind = dmg;
            s.amount = amount;
            s.hitRate = 1.0f;
            s.effectTargetSide = targetSide;
            s.targetingMode = targetMode;
            s.skillTags = tag;
            s.recommendedPriority = prio;
            s.recommendedConditions = conds.Length > 0
                ? new List<ActionCondition>(conds)
                : new List<ActionCondition> { new ActionCondition(
                    cat == SkillCategory.ActiveAttack ? ActionConditionKind.EnemyInRange :
                    cat == SkillCategory.ActiveMove ? ActionConditionKind.NoEnemyForward :
                    ActionConditionKind.Always) };
            return s;
        }

        private static void PlaceAndRegister(BattleManager manager, BattleField field,
            List<(SpaceJourneyUnit unit, List<SkillDefinition> skills, int startCol)> team, int side)
        {
            // startColumn ごとにグループ分けして y=0,1,2 に配置
            var byCol = new Dictionary<int, List<(SpaceJourneyUnit, List<SkillDefinition>)>>();
            foreach (var (unit, skills, col) in team)
            {
                if (!byCol.ContainsKey(col)) byCol[col] = new();
                byCol[col].Add((unit, skills));
            }

            foreach (var (col, units) in byCol)
            {
                for (int y = 0; y < units.Count && y < 3; y++)
                {
                    var (unit, skills) = units[y];
                    field.PlaceUnit(unit, side, col, y);
                    var actionList = ActionListBuilder.Build(skills, BattleTactic.Balanced);
                    manager.RegisterUnit(unit, actionList);
                }
            }
        }
    }
}
