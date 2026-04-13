using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// スキルリスト + 作戦 から行動リスト (BattleActionEntry × 最大10個) を自動生成する。
    /// プレイヤーの「おすすめ」ボタンや、敵AI のプリセット生成に使う。
    /// </summary>
    public static class ActionListBuilder
    {
        public const int MaxEntries = 10;

        // ── 作戦別ボーナステーブル ──
        // 最終優先度 = recommendedPriority + tacticBonus[tactic][category]
        // 値が小さいほど優先度が高い
        private static readonly Dictionary<BattleTactic, Dictionary<SkillTacticCategory, int>> TacticBonus = new()
        {
            [BattleTactic.Balanced] = new()
            {
                [SkillTacticCategory.Attack] = 0,
                [SkillTacticCategory.Heal] = 0,
                [SkillTacticCategory.Buff] = 0,
                [SkillTacticCategory.Debuff] = 0,
                [SkillTacticCategory.Move] = 0,
                [SkillTacticCategory.Wait] = 0,
            },
            [BattleTactic.Offensive] = new()
            {
                [SkillTacticCategory.Attack] = -3,
                [SkillTacticCategory.Heal] = +3,
                [SkillTacticCategory.Buff] = +2,
                [SkillTacticCategory.Debuff] = -1,
                [SkillTacticCategory.Move] = 0,
                [SkillTacticCategory.Wait] = 0,
            },
            [BattleTactic.Supportive] = new()
            {
                [SkillTacticCategory.Attack] = +2,
                [SkillTacticCategory.Heal] = -3,
                [SkillTacticCategory.Buff] = 0,
                [SkillTacticCategory.Debuff] = +1,
                [SkillTacticCategory.Move] = 0,
                [SkillTacticCategory.Wait] = 0,
            },
            [BattleTactic.Defensive] = new()
            {
                [SkillTacticCategory.Attack] = +1,
                [SkillTacticCategory.Heal] = -1,
                [SkillTacticCategory.Buff] = -3,
                [SkillTacticCategory.Debuff] = 0,
                [SkillTacticCategory.Move] = 0,
                [SkillTacticCategory.Wait] = 0,
            },
        };

        // 職特性別の退避HP閾値 (AoE詠唱予告範囲内で退避発動)
        private static readonly Dictionary<string, float> EvadeBaseHp = new()
        {
            ["Warrior"] = 0.30f,
            ["Lancer"] = 0.40f,
            ["Archer"] = 0.60f,
            ["Mage"] = 0.70f,
            // Knight は退避しない (含めない)
        };

        // 作戦別の退避HP調整
        private static readonly Dictionary<BattleTactic, float> TacticEvadeAdjust = new()
        {
            [BattleTactic.Offensive] = -0.20f,
            [BattleTactic.Balanced] = 0.0f,
            [BattleTactic.Defensive] = +0.15f,
            [BattleTactic.Supportive] = +0.30f,
        };

        /// <summary>
        /// スキルリストと作戦から行動リストを自動生成する。
        /// </summary>
        /// <param name="skills">ユニットが使用可能な全スキル (パッシブ含む)</param>
        /// <param name="tactic">簡単作戦</param>
        /// <param name="bodyJobId">職特性の退避条件に使用 (空ならデフォルト挙動)</param>
        /// <returns>最大10エントリの行動リスト (優先度順)</returns>
        public static List<BattleActionEntry> Build(
            IEnumerable<SkillDefinition> skills,
            BattleTactic tactic = BattleTactic.Balanced,
            string bodyJobId = "")
        {
            var entries = new List<(int finalPriority, BattleActionEntry entry)>();
            var bonusTable = TacticBonus[tactic];

            foreach (var skill in skills)
            {
                if (skill == null) continue;

                // パッシブは行動リストに入れない (自動発動)
                if (skill.category == SkillCategory.Passive) continue;

                var tacticCat = skill.GetTacticCategory();
                int bonus = bonusTable.TryGetValue(tacticCat, out int b) ? b : 0;

                // 新形式: recommendedActionSets があれば各セットを独立エントリとして展開 (OR)
                // 位置が下のセットほど自動で優先度が下がる (index分加算 → 数値大=低優先)
                if (skill.recommendedActionSets != null && skill.recommendedActionSets.Count > 0)
                {
                    int setsToUse = Mathf.Min(skill.recommendedActionSets.Count, 3);
                    for (int i = 0; i < setsToUse; i++)
                    {
                        var set = skill.recommendedActionSets[i];
                        if (set == null) continue;
                        int fPriority = Mathf.Clamp(set.priority + bonus, 1, 15);
                        var conds = (set.conditions != null && set.conditions.Count > 0)
                            ? new List<ActionCondition>(set.conditions)
                            : BuildDefaultConditions(skill);
                        var e = new BattleActionEntry(skill, conds.ToArray());
                        if (skill.category == SkillCategory.ActiveMove)
                        {
                            // conditions 内の MoveTo_* があれば moveTarget として抽出、なければ set.moveTarget を使用
                            var mt = ExtractMoveTargetFromConditions(conds);
                            e.moveTarget = mt ?? set.moveTarget;
                        }
                        entries.Add((fPriority, e));
                    }
                    continue;
                }

                // 旧形式: recommendedPriority + recommendedConditions で1エントリ
                int finalPriority = Mathf.Clamp(skill.recommendedPriority + bonus, 1, 15);

                var conditions = skill.recommendedConditions != null && skill.recommendedConditions.Count > 0
                    ? new List<ActionCondition>(skill.recommendedConditions)
                    : BuildDefaultConditions(skill);

                var entry = new BattleActionEntry(skill, conditions.ToArray());

                if (skill.category == SkillCategory.ActiveMove)
                {
                    entry.moveTarget = DefaultMoveTarget(tactic);
                }

                entries.Add((finalPriority, entry));
            }

            // 優先度でソート (小さい = 高優先)
            entries.Sort((a, b) => a.finalPriority.CompareTo(b.finalPriority));

            // 最大10個に切る
            var result = entries.Take(MaxEntries).Select(e => e.entry).ToList();

            // 退避エントリを挿入: 移動スキル直前に「AoE予告範囲内 + HP低い時」の退避を入れる (職別)
            if (EvadeBaseHp.TryGetValue(bodyJobId, out float baseHp))
            {
                float adj = TacticEvadeAdjust.TryGetValue(tactic, out float a) ? a : 0f;
                float evadeHp = Mathf.Clamp(baseHp + adj, 0f, 1f);
                int insertAt = result.FindIndex(e => e.skill != null && e.skill.category == SkillCategory.ActiveMove);
                if (insertAt >= 0 && result.Count < MaxEntries)
                {
                    var moveSkill = result[insertAt].skill;
                    var evadeEntry = new BattleActionEntry(moveSkill,
                        new ActionCondition(ActionConditionKind.SelfInCastTargetArea),
                        new ActionCondition(ActionConditionKind.SelfHpBelowRate, 0, evadeHp));
                    evadeEntry.moveTarget = MoveTargetKind.Retreat;
                    result.Insert(insertAt, evadeEntry);
                }
            }

            // 最後に待機を追加 (枠があれば)
            if (result.Count < MaxEntries)
            {
                result.Add(new BattleActionEntry(null, new ActionCondition(ActionConditionKind.Always)));
            }

            return result;
        }

        /// <summary>推奨条件が未設定の場合の暗黙条件を生成</summary>
        private static List<ActionCondition> BuildDefaultConditions(SkillDefinition skill)
        {
            var conditions = new List<ActionCondition>();
            var cat = skill.GetTacticCategory();

            switch (cat)
            {
                case SkillTacticCategory.Attack:
                case SkillTacticCategory.Debuff:
                    conditions.Add(new ActionCondition(ActionConditionKind.EnemyInRange));
                    break;

                case SkillTacticCategory.Heal:
                    conditions.Add(new ActionCondition(ActionConditionKind.AllyInRange));
                    conditions.Add(new ActionCondition(ActionConditionKind.AllyHpBelowRate, 0, 0.5f));
                    break;

                case SkillTacticCategory.Buff:
                    conditions.Add(new ActionCondition(ActionConditionKind.Always));
                    break;

                case SkillTacticCategory.Move:
                    // 条件なし (常に移動可能、上位エントリが優先されなかったら移動)
                    conditions.Add(new ActionCondition(ActionConditionKind.Always));
                    break;

                default:
                    conditions.Add(new ActionCondition(ActionConditionKind.Always));
                    break;
            }

            return conditions;
        }

        /// <summary>conditions 内の MoveTo_* を moveTarget に変換して返す。なければ null。</summary>
        private static MoveTargetKind? ExtractMoveTargetFromConditions(List<ActionCondition> conds)
        {
            if (conds == null) return null;
            foreach (var c in conds)
            {
                switch (c.kind)
                {
                    case ActionConditionKind.MoveTo_NearestEnemy: return MoveTargetKind.NearestEnemy;
                    case ActionConditionKind.MoveTo_NearestJobEnemy: return MoveTargetKind.NearestJobEnemy;
                    case ActionConditionKind.MoveTo_IntruderEnemy: return MoveTargetKind.IntruderEnemy;
                    case ActionConditionKind.MoveTo_FarthestEnemy: return MoveTargetKind.FarthestEnemy;
                    case ActionConditionKind.MoveTo_EnemyTerritory: return MoveTargetKind.EnemyTerritory;
                    case ActionConditionKind.MoveTo_Retreat: return MoveTargetKind.Retreat;
                }
            }
            return null;
        }

        /// <summary>作戦に応じたデフォルトの移動先方針</summary>
        private static MoveTargetKind DefaultMoveTarget(BattleTactic tactic)
        {
            return tactic switch
            {
                BattleTactic.Offensive => MoveTargetKind.NearestEnemy,
                BattleTactic.Defensive => MoveTargetKind.IntruderEnemy,
                BattleTactic.Supportive => MoveTargetKind.Retreat,
                _ => MoveTargetKind.NearestEnemy,
            };
        }
    }
}
