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

        /// <summary>
        /// スキルリストと作戦から行動リストを自動生成する。
        /// </summary>
        /// <param name="skills">ユニットが使用可能な全スキル (パッシブ含む)</param>
        /// <param name="tactic">簡単作戦</param>
        /// <returns>最大10エントリの行動リスト (優先度順)</returns>
        public static List<BattleActionEntry> Build(
            IEnumerable<SkillDefinition> skills,
            BattleTactic tactic = BattleTactic.Balanced)
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
                int finalPriority = Mathf.Clamp(skill.recommendedPriority + bonus, 1, 15);

                // 推奨条件を使う。空なら暗黙条件を生成
                var conditions = skill.recommendedConditions != null && skill.recommendedConditions.Count > 0
                    ? new List<ActionCondition>(skill.recommendedConditions)
                    : BuildDefaultConditions(skill);

                entries.Add((finalPriority, new BattleActionEntry(skill, conditions.ToArray())));
            }

            // 優先度でソート (小さい = 高優先)
            entries.Sort((a, b) => a.finalPriority.CompareTo(b.finalPriority));

            // 最大10個に切る
            var result = entries.Take(MaxEntries).Select(e => e.entry).ToList();

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
                    conditions.Add(new ActionCondition(ActionConditionKind.NoEnemyForward));
                    break;

                default:
                    conditions.Add(new ActionCondition(ActionConditionKind.Always));
                    break;
            }

            return conditions;
        }
    }
}
