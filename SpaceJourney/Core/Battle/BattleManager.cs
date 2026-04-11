using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// タイムポイント制バトルのコアロジック。
    ///
    /// 仕様:
    /// - t = 0 ~ 9 の 10 タイムユニットで進行
    /// - 各ユニットは nextActTime を持ち、t >= nextActTime で行動可能になる
    /// - 行動リスト (BattleActionEntry × 最大10) を上から順に条件チェック
    /// - 最初に条件を満たした + CT 解けてる + 射程内ターゲットがいる スキルを実行
    /// - ダメージは通り率 (tanh カーブ) で計算
    /// - パッシブはイベントドリブンで自動発動
    /// </summary>
    public class BattleManager
    {
        public BattleField Field { get; private set; }
        public int CurrentTime { get; private set; }
        public bool IsFinished { get; private set; }
        public int WinningSide { get; private set; } = -1;

        public List<string> Log { get; } = new List<string>();

        private const int MaxTime = 10;
        private const float AgiCap = 600f;
        private const float MaxCostReductionRate = 0.35f;

        // ユニットごとの戦闘状態
        public class UnitBattleState
        {
            public SpaceJourneyUnit Unit;
            public float NextActTime;
            public int NormalTurnCount;
            public int AttackCount; // 戦闘中の攻撃回数 (パッシブ条件用)
            public List<BattleActionEntry> ActionList;

            // スキルごとの再使用カウントダウン: skillId → 次に使える t
            public Dictionary<string, float> SkillCooldowns = new();

            public UnitBattleState(SpaceJourneyUnit unit, List<BattleActionEntry> actionList)
            {
                Unit = unit;
                NextActTime = 0;
                NormalTurnCount = 0;
                AttackCount = 0;
                ActionList = actionList ?? new List<BattleActionEntry>();
            }
        }

        private readonly Dictionary<SpaceJourneyUnit, UnitBattleState> unitStates = new();

        /// <summary>外部からユニット状態を参照する用</summary>
        public UnitBattleState GetUnitState(SpaceJourneyUnit unit)
            => unitStates.TryGetValue(unit, out var s) ? s : null;

        public BattleManager(BattleField field)
        {
            Field = field;
            CurrentTime = 0;
            IsFinished = false;
        }

        /// <summary>
        /// BattleStartData から戦闘を初期化して実行する。
        /// </summary>
        public static BattleManager StartBattle(BattleStartData data)
        {
            var field = new BattleField(data.fieldLayout);
            var manager = new BattleManager(field);

            // 士気倍率: 0.1 + 0.9 × (morale/100)^1.3
            float allyMoraleMul = CalcMoraleMultiplier(data.allyMorale);
            float enemyMoraleMul = CalcMoraleMultiplier(data.enemyMorale);

            // 味方配置
            foreach (var p in data.allyUnits)
            {
                var unit = new SpaceJourneyUnit(p.soul, p.body);
                unit.MoraleMultiplier = allyMoraleMul;
                string bodyJobId = p.body?.BodyJobId;
                var actionList = p.soul?.GetActionList(bodyJobId) ?? new List<BattleActionEntry>();
                field.PlaceUnit(unit, 0, p.battleCell);
                manager.RegisterUnit(unit, actionList);

                var state = manager.GetUnitState(unit);
                state.NextActTime = data.initiativeSide == 0 ? 0 : 1;
            }

            // 敵配置
            foreach (var p in data.enemyUnits)
            {
                var unit = new SpaceJourneyUnit(p.soul, p.body);
                unit.MoraleMultiplier = enemyMoraleMul;
                string bodyJobId = p.body?.BodyJobId;
                var actionList = p.soul?.GetActionList(bodyJobId) ?? new List<BattleActionEntry>();
                field.PlaceUnit(unit, 1, p.battleCell);
                manager.RegisterUnit(unit, actionList);

                var state = manager.GetUnitState(unit);
                state.NextActTime = data.initiativeSide == 1 ? 0 : 1;
            }

            // 先制なし (-1) の場合は全員 nextActTime=0
            if (data.initiativeSide < 0)
            {
                foreach (var s in manager.unitStates.Values)
                    s.NextActTime = 0;
            }

            manager.RunFullBattle();
            return manager;
        }

        /// <summary>士気 → ステータス乗算倍率</summary>
        public static float CalcMoraleMultiplier(float morale)
        {
            float clamped = Mathf.Clamp(morale, 0f, 100f);
            return 0.1f + 0.9f * Mathf.Pow(clamped / 100f, 1.3f);
        }

        /// <summary>ユニットの戦闘状態を登録する (戦闘開始前に呼ぶ)</summary>
        public void RegisterUnit(SpaceJourneyUnit unit, List<BattleActionEntry> actionList)
        {
            unitStates[unit] = new UnitBattleState(unit, actionList);
        }

        /// <summary>戦闘を最後まで実行</summary>
        public void RunFullBattle()
        {
            Log.Clear();
            Log.Add("=== 戦闘開始 ===");
            LogAllUnitsStatus();

            // 開幕パッシブ (timing=2)
            FirePassives(2);

            while (!IsFinished && CurrentTime < MaxTime)
            {
                ProcessTimeUnit(CurrentTime);
                CurrentTime++;
            }

            if (!IsFinished)
            {
                // 時間切れ → HP 残量比で勝敗判定
                IsFinished = true;
                float side0Rate = CalcSideHpRate(0);
                float side1Rate = CalcSideHpRate(1);
                if (side0Rate > side1Rate)
                    WinningSide = 0;
                else if (side1Rate > side0Rate)
                    WinningSide = 1;
                else
                    WinningSide = -1;

                Log.Add($"\n[t={CurrentTime}] 制限時間到達。HP残量で判定。");
            }

            Log.Add($"\n=== 戦闘終了 (t={CurrentTime}) ===");

            for (int s = 0; s < 2; s++)
            {
                string sLabel = s == 0 ? "味方" : "敵";
                var alive = Field.GetAllAlive(s);
                var all = Field.GetAllUnits(s);
                int totalHp = alive.Sum(u => u.CurrentHp);
                int totalMax = all.Sum(u => u.MaxHp);
                float rate = totalMax > 0 ? (float)totalHp / totalMax * 100f : 0f;
                Log.Add($"  {sLabel}: {alive.Count}/{all.Count}名生存, HP {totalHp}/{totalMax} ({rate:F0}%)");
            }

            if (WinningSide >= 0)
                Log.Add($"\n勝者: {(WinningSide == 0 ? "味方" : "敵")}");
            else
                Log.Add("\n結果: 引き分け");
        }

        private void ProcessTimeUnit(int t)
        {
            var actors = unitStates.Values
                .Where(s => !s.Unit.IsDead && s.NextActTime <= t)
                .OrderByDescending(s => s.Unit.AgiFinal + UnityEngine.Random.Range(-2f, 2f))
                .ToList();

            if (actors.Count == 0) return;
            Log.Add($"\n--- t={t} ({actors.Count}体行動) ---");

            foreach (var s in unitStates.Values)
                s.Unit.SetBattleTime(t);

            foreach (var state in actors)
            {
                if (state.Unit.IsDead || IsFinished) continue;

                if (state.Unit.IsActionDisabled)
                {
                    Log.Add($"  [{GetLabel(state.Unit)}] 行動不能 → 待機 [cost=1]");
                    state.NextActTime = t + 1;
                    state.NormalTurnCount++;
                    continue;
                }

                ExecuteFromActionList(state, t);
                state.NormalTurnCount++;
                CheckVictory();
            }
        }

        /// <summary>行動リストを上から評価して最初に条件を満たしたスキルを実行</summary>
        private void ExecuteFromActionList(UnitBattleState state, int t)
        {
            var actor = state.Unit;

            foreach (var entry in state.ActionList)
            {
                // スキルなし = 待機
                if (entry.skill == null)
                {
                    DoWait(state, t);
                    return;
                }

                var skill = entry.skill;

                // パッシブは行動リストに入ってるべきではないが念のためスキップ
                if (skill.category == SkillCategory.Passive) continue;

                // 開幕CT チェック
                if (skill.openingCoolTime > 0 && state.NormalTurnCount < skill.openingCoolTime)
                    continue;

                // 再使用サイクル チェック
                if (state.SkillCooldowns.TryGetValue(skill.SkillId, out float readyAt) && t < readyAt)
                    continue;

                // 条件チェック
                if (!EvaluateConditions(entry.conditions, state, skill))
                    continue;

                // 射程内にターゲットがいるかチェック (攻撃系)
                var target = FindTarget(state, skill);
                if (skill.category == SkillCategory.ActiveAttack && target == null)
                    continue;

                // ─── 実行 ───
                ExecuteSkill(state, skill, target, t);
                return;
            }

            // どれも使えなかった → 待機
            DoWait(state, t);
        }

        private void DoWait(UnitBattleState state, int t)
        {
            state.NextActTime = t + 1;
            Log.Add($"  [{GetLabel(state.Unit)}] 待機 [cost=1]");
        }

        private void ExecuteSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t)
        {
            var actor = state.Unit;
            int baseCost = skill.baseCost;
            int effectiveCost = CalcEffectiveCost(baseCost, actor.AgiFinal);
            state.NextActTime = t + effectiveCost;

            // クールダウン設定
            if (skill.reuseCycle > 0)
                state.SkillCooldowns[skill.SkillId] = t + skill.reuseCycle;

            var actorPos = Field.FindUnit(actor);

            // ─── スキルカテゴリ別処理 ───
            switch (skill.category)
            {
                case SkillCategory.ActiveAttack:
                    ExecuteAttackSkill(state, skill, target, t, effectiveCost);
                    break;

                case SkillCategory.ActiveSupport:
                    ExecuteSupportSkill(state, skill, target, t, effectiveCost);
                    break;

                case SkillCategory.ActiveMove:
                    ExecuteMoveSkill(state, skill, t, effectiveCost);
                    break;

                default:
                    Log.Add($"  [{GetLabel(actor)}] {skill.SkillName} (未実装カテゴリ) [cost={effectiveCost}]");
                    break;
            }
        }

        private void ExecuteAttackSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            var actor = state.Unit;

            int damage = CalculateSkillDamage(skill, actor, target);
            int hpBefore = target.CurrentHp;
            target.TakeDamage(damage);
            state.AttackCount++;

            string deadMark = target.IsDead ? " ★撃破!" : "";
            Log.Add($"  [{GetLabel(actor)}] →「{skill.SkillName}」→ [{GetLabel(target)}] " +
                    $"{damage}ダメ (HP {hpBefore}→{target.CurrentHp}){deadMark} [cost={cost}]");

            // 追加効果
            ApplyAdditionalEffects(skill, actor, target, t);

            // パッシブ発火: 攻撃命中 (timing=1)
            FirePassivesForUnit(state, 1);

            // 被弾パッシブ (timing=3)
            var defenderState = GetUnitState(target);
            if (defenderState != null)
                FirePassivesForUnit(defenderState, 3);
        }

        private void ExecuteSupportSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);

            // 回復/バフ処理
            if (skill.damageKind == SkillDamageKind.Fixed && skill.amount < 0)
            {
                // 固定値回復
                int heal = -skill.amount;
                var healTarget = target ?? actor;
                healTarget.Heal(heal);
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(healTarget)}] {heal}回復 " +
                        $"(HP:{healTarget.CurrentHp - heal}→{healTarget.CurrentHp}) [cost={cost}]");
            }
            else if (skill.damageKind == SkillDamageKind.MaxHpRate && skill.amount < 0)
            {
                // 最大HP割合回復
                var healTarget = target ?? actor;
                int heal = Mathf.RoundToInt(healTarget.MaxHp * (-skill.amount / 100f));
                healTarget.Heal(heal);
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(healTarget)}] {heal}回復 " +
                        $"(HP:{healTarget.CurrentHp - heal}→{healTarget.CurrentHp}) [cost={cost}]");
            }
            else
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」[cost={cost}]");
            }

            // 追加効果
            ApplyAdditionalEffects(skill, actor, target ?? actor, t);
        }

        private void ExecuteMoveSkill(UnitBattleState state, SkillDefinition skill, int t, int cost)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);

            // TODO: 実際のマス移動処理 (GridRangePattern.moveRange に基づく)
            // 現時点ではログだけ
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」移動 [cost={cost}]");
        }

        // ================================================================
        // ダメージ計算
        // ================================================================

        private int CalculateSkillDamage(SkillDefinition skill, SpaceJourneyUnit attacker, SpaceJourneyUnit defender)
        {
            float amount = skill.amount;

            switch (skill.damageKind)
            {
                case SkillDamageKind.None:
                    return 0;

                case SkillDamageKind.Physical:
                {
                    float atkPower = attacker.AtFinal * (amount / 100f);
                    return CalcDamageWithThroughRate(atkPower, defender.DfFinal);
                }

                case SkillDamageKind.Magical:
                {
                    float atkPower = attacker.MatFinal * (amount / 100f);
                    return CalcDamageWithThroughRate(atkPower, defender.MdfFinal);
                }

                case SkillDamageKind.PenetratePhysical:
                {
                    float atkPower = attacker.AtFinal * (amount / 100f);
                    float effDef = defender.DfFinal * (1f - skill.defenseIgnorePercent / 100f);
                    return CalcDamageWithThroughRate(atkPower, effDef);
                }

                case SkillDamageKind.PenetrateMagical:
                {
                    float atkPower = attacker.MatFinal * (amount / 100f);
                    float effDef = defender.MdfFinal * (1f - skill.defenseIgnorePercent / 100f);
                    return CalcDamageWithThroughRate(atkPower, effDef);
                }

                case SkillDamageKind.Fixed:
                    return Mathf.Max(1, (int)amount);

                case SkillDamageKind.MaxHpRate:
                    return Mathf.Max(1, Mathf.RoundToInt(defender.MaxHp * (amount / 100f)));

                default:
                    return 0;
            }
        }

        public static int CalcDamageWithThroughRate(float attackPower, float defenderDf)
        {
            const float throughEqual = 0.53f;
            const float throughMin = 0.10f;
            const float throughMax = 1.10f;
            const float deltaScale = 60f;

            float delta = attackPower - defenderDf;
            float throughRate = throughEqual +
                (float)Math.Tanh(delta / deltaScale) * (throughMax - throughEqual);
            throughRate = Mathf.Clamp(throughRate, throughMin, throughMax);

            float damage = attackPower * throughRate;

            // ±10% 揺らぎ
            float variance = UnityEngine.Random.Range(0.90f, 1.10f);
            return Mathf.Max(1, Mathf.RoundToInt(damage * variance));
        }

        public static int CalcEffectiveCost(int baseCost, float agiFinal)
        {
            float agiUsed = agiFinal * UnityEngine.Random.Range(0.95f, 1.05f);
            float agiRatio = Mathf.Clamp01(agiUsed / AgiCap);
            float reductionRate = MaxCostReductionRate * agiRatio;
            float rawCost = baseCost * (1f - reductionRate);
            float costGain = baseCost - rawCost;
            if (costGain < 1f) return baseCost;
            return Mathf.Max(1, Mathf.RoundToInt(rawCost));
        }

        // ================================================================
        // 条件評価
        // ================================================================

        private bool EvaluateConditions(List<ActionCondition> conditions, UnitBattleState state, SkillDefinition skill)
        {
            if (conditions == null || conditions.Count == 0) return true;

            foreach (var cond in conditions)
            {
                if (!EvaluateOneCondition(cond, state, skill)) return false;
            }
            return true;
        }

        private bool EvaluateOneCondition(ActionCondition cond, UnitBattleState state, SkillDefinition skill)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);
            int mySide = actorPos[0];
            int enemySide = BattleField.OppositeSide(mySide);

            switch (cond.kind)
            {
                case ActionConditionKind.Always:
                    return true;

                case ActionConditionKind.EnemyInRange:
                    return Field.GetAllAlive(enemySide).Count > 0;

                case ActionConditionKind.EnemyCountInRange:
                    return Field.GetAllAlive(enemySide).Count >= cond.intParam;

                case ActionConditionKind.EnemyHpBelowRate:
                    return Field.GetAllAlive(enemySide)
                        .Any(e => (float)e.CurrentHp / Mathf.Max(1, e.MaxHp) <= cond.rateParam);

                case ActionConditionKind.NoEnemyForward:
                    // 前列に敵がいない
                    return Field.GetFrontRowCells(enemySide).Count == 0;

                case ActionConditionKind.AllyInRange:
                    return Field.GetAllAlive(mySide).Count > 1; // 自分以外

                case ActionConditionKind.AllyHpBelowRate:
                    return Field.GetAllAlive(mySide)
                        .Where(a => a != actor)
                        .Any(a => (float)a.CurrentHp / Mathf.Max(1, a.MaxHp) <= cond.rateParam);

                case ActionConditionKind.NoAllyForward:
                {
                    var frontCells = Field.GetFrontRowCells(mySide);
                    foreach (var cell in frontCells)
                    {
                        var u = Field.GetUnit(mySide, cell);
                        if (u != null && !u.IsDead && u != actor) return false;
                    }
                    return true;
                }

                case ActionConditionKind.SelfHpBelowRate:
                    return (float)actor.CurrentHp / Mathf.Max(1, actor.MaxHp) <= cond.rateParam;

                case ActionConditionKind.SelfHpAboveRate:
                    return (float)actor.CurrentHp / Mathf.Max(1, actor.MaxHp) >= cond.rateParam;

                default:
                    return true;
            }
        }

        // ================================================================
        // ターゲット選択
        // ================================================================

        private SpaceJourneyUnit FindTarget(UnitBattleState state, SkillDefinition skill)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);
            int mySide = actorPos[0];

            EffectTargetSide targetSide = skill.effectTargetSide;
            List<SpaceJourneyUnit> candidates;

            if (targetSide == EffectTargetSide.Self)
                candidates = Field.GetAllAlive(mySide).Where(u => u != actor).ToList();
            else
                candidates = Field.GetAllAlive(BattleField.OppositeSide(mySide));

            if (candidates.Count == 0) return null;

            // TODO: GridRangePattern による射程フィルタ
            // 現時点では距離ベースで最も近い敵/味方を選択

            return candidates.OrderBy(c => Field.Distance(actor, c)).First();
        }

        // ================================================================
        // 追加効果
        // ================================================================

        private void ApplyAdditionalEffects(SkillDefinition skill, SpaceJourneyUnit actor, SpaceJourneyUnit target, int t)
        {
            if (skill.AdditionalEffects == null) return;

            foreach (var eff in skill.AdditionalEffects)
            {
                if (eff.effectType == StatusEffectType.None) continue;
                if (eff.effectType == StatusEffectType.Custom) continue; // Custom は個別処理が必要

                if (UnityEngine.Random.value > eff.probability) continue;

                int duration = eff.duration > 0 ? eff.duration : CalcEffectiveCost(skill.baseCost, actor.AgiFinal);
                target.ApplyStatusEffect(eff.effectType, eff.value, duration, t);
            }
        }

        // ================================================================
        // パッシブ発動
        // ================================================================

        private void FirePassives(int timing)
        {
            foreach (var state in unitStates.Values)
            {
                if (state.Unit.IsDead) continue;
                FirePassivesForUnit(state, timing);
            }
        }

        private void FirePassivesForUnit(UnitBattleState state, int timing)
        {
            // TODO: ユニットのパッシブスキルリストをチェックして条件を評価
            // 現時点ではスタブ
        }

        // ================================================================
        // 勝敗チェック
        // ================================================================

        private float CalcSideHpRate(int side)
        {
            var all = Field.GetAllUnits(side);
            if (all.Count == 0) return 0f;
            int totalHp = all.Sum(u => u.IsDead ? 0 : u.CurrentHp);
            int totalMax = all.Sum(u => u.MaxHp);
            return totalMax > 0 ? (float)totalHp / totalMax : 0f;
        }

        private void CheckVictory()
        {
            bool side0Alive = Field.GetAllAlive(0).Count > 0;
            bool side1Alive = Field.GetAllAlive(1).Count > 0;

            if (!side0Alive && !side1Alive)
            {
                IsFinished = true;
                WinningSide = -1;
            }
            else if (!side0Alive)
            {
                IsFinished = true;
                WinningSide = 1;
            }
            else if (!side1Alive)
            {
                IsFinished = true;
                WinningSide = 0;
            }
        }

        // ================================================================
        // ヘルパー
        // ================================================================

        private string GetLabel(SpaceJourneyUnit unit)
        {
            var pos = Field.FindUnit(unit);
            if (pos[0] < 0) return "??";
            string side = pos[0] == 0 ? "味" : "敵";
            string job = unit.Body?.BodyJobId ?? "";
            if (job.Length > 3) job = job.Substring(0, 3);
            return $"{side}{job}({pos[1]},{pos[2]})";
        }

        private void LogAllUnitsStatus()
        {
            Log.Add("\n[初期ステータス]");
            for (int s = 0; s < 2; s++)
            {
                string sideLabel = s == 0 ? "味方" : "敵";
                foreach (var unit in Field.GetAllUnits(s))
                {
                    var pos = Field.FindUnit(unit);
                    var state = GetUnitState(unit);
                    int skillCount = state?.ActionList?.Count(e => e.skill != null) ?? 0;
                    Log.Add($"  [{sideLabel}({pos[1]},{pos[2]})] " +
                           $"HP={unit.MaxHp} AT={unit.AtFinal} DF={unit.DfFinal} " +
                           $"AGI={unit.AgiFinal} MAT={unit.MatFinal} MDF={unit.MdfFinal} " +
                           $"スキル={skillCount}");
                }
            }
        }
    }
}
