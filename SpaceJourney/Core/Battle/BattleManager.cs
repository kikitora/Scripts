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

        private const int MaxTime = 20;
        private const int InitiativeDelay = 5;
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

            // このユニットのパッシブスキル一覧
            public List<SkillDefinition> PassiveSkills = new();

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
                var passives = CollectPassiveSkills(p.soul, p.body);
                field.PlaceUnit(unit, 0, p.battleCell);
                manager.RegisterUnit(unit, actionList, passives);

                var state = manager.GetUnitState(unit);
                state.NextActTime = data.initiativeSide == 0 ? 0 : InitiativeDelay;
            }

            // 敵配置
            foreach (var p in data.enemyUnits)
            {
                var unit = new SpaceJourneyUnit(p.soul, p.body);
                unit.MoraleMultiplier = enemyMoraleMul;
                string bodyJobId = p.body?.BodyJobId;
                var actionList = p.soul?.GetActionList(bodyJobId) ?? new List<BattleActionEntry>();
                var passives = CollectPassiveSkills(p.soul, p.body);
                field.PlaceUnit(unit, 1, p.battleCell);
                manager.RegisterUnit(unit, actionList, passives);

                var state = manager.GetUnitState(unit);
                state.NextActTime = data.initiativeSide == 1 ? 0 : InitiativeDelay;
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

        /// <summary>ソウル���ボディからパッシブスキルを収集���る</summary>
        private static List<SkillDefinition> CollectPassiveSkills(SoulInstance soul, BodyInstance body)
        {
            var passives = new List<SkillDefinition>();
            var db = MasterDatabase.Instance;
            if (db == null) return passives;

            // 武器パッシブ
            if (body != null)
            {
                var weapon = db.GetWeaponById(body.WeaponId);
                if (weapon?.effectSkill != null && weapon.effectSkill.category == SkillCategory.Passive)
                    passives.Add(weapon.effectSkill);
            }

            // 種族パッシブ
            if (body != null)
            {
                var race = db.GetRaceById(body.RaceId);
                if (race?.racialSkill != null && race.racialSkill.category == SkillCategory.Passive)
                    passives.Add(race.racialSkill);
            }

            // ソウル生業スキルのうちパッシブ
            if (soul?.CurrentReinSoul?.LearnedSkillIds != null)
            {
                foreach (var skillId in soul.CurrentReinSoul.LearnedSkillIds)
                {
                    var skill = db.GetSoulJobSkillById(skillId);
                    if (skill != null && skill.category == SkillCategory.Passive)
                        passives.Add(skill);
                }
            }

            return passives;
        }

        /// <summary>ユニットの戦闘状態を登録する (戦闘開始前に呼ぶ)</summary>
        public void RegisterUnit(SpaceJourneyUnit unit, List<BattleActionEntry> actionList)
        {
            unitStates[unit] = new UnitBattleState(unit, actionList);
        }

        /// <summary>ユニットの戦闘状態を登録する (パッシブスキルリスト付き)</summary>
        public void RegisterUnit(SpaceJourneyUnit unit, List<BattleActionEntry> actionList, List<SkillDefinition> passiveSkills)
        {
            var state = new UnitBattleState(unit, actionList);
            if (passiveSkills != null)
                state.PassiveSkills = passiveSkills;
            unitStates[unit] = state;
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

            // パッシブ発火: 攻撃命中 (Action=1)
            FirePassivesForUnit(state, 1, actor, target, skill);

            // 被弾パッシブ (Hp=3)
            var defenderState = GetUnitState(target);
            if (defenderState != null)
                FirePassivesForUnit(defenderState, 3, target, actor, skill);
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
            int side = actorPos[0];
            var currentCell = new Vector2Int(actorPos[1], actorPos[2]);
            int enemySide = BattleField.OppositeSide(side);

            // 1. 自陣内で moveRange に基づく移動先を探す
            Vector2Int? bestCell = null;
            int bestDist = int.MaxValue;
            bool crossSide = false;

            if (skill.moveRange != null && skill.moveRange.Offsets.Count > 0)
            {
                foreach (var offset in skill.moveRange.Offsets)
                {
                    var candidate = currentCell + offset;
                    if (!Field.IsCellEmpty(side, candidate)) continue;

                    int dist = CalcDistToNearestEnemy(candidate, side, enemySide);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestCell = candidate;
                    }
                }
            }

            // 2. 自陣で移動先がない + 前列(x=0)にいる → 敵陣に攻め込む
            if (bestCell == null && currentCell.x == Field.FrontRow)
            {
                // 敵陣の後列(x=最大)の同じy、またはその周辺に空きがあれば移動
                var enemyCells = new List<Vector2Int>();
                int maxX = 0;
                foreach (var cell in Field.Cells)
                    if (cell.x > maxX) maxX = cell.x;

                // 後列 → 中列 → 前列の順で空きを探す
                for (int x = maxX; x >= 0; x--)
                {
                    foreach (var cell in Field.Cells)
                    {
                        if (cell.x != x) continue;
                        if (Field.IsCellEmpty(enemySide, cell))
                            enemyCells.Add(cell);
                    }
                    if (enemyCells.Count > 0) break;
                }

                if (enemyCells.Count > 0)
                {
                    // 敵に最も近いセルを選ぶ
                    foreach (var candidate in enemyCells)
                    {
                        int dist = CalcDistToNearestEnemySameSide(candidate, enemySide);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestCell = candidate;
                            crossSide = true;
                        }
                    }
                }
            }

            // 実行
            if (bestCell.HasValue)
            {
                bool moved;
                if (crossSide)
                    moved = Field.MoveUnitCrossSide(actor, enemySide, bestCell.Value);
                else
                    moved = Field.MoveUnit(actor, bestCell.Value);

                if (moved)
                {
                    string sideLabel = crossSide ? " [敵陣侵入]" : "";
                    Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」移動 ({currentCell.x},{currentCell.y})→({bestCell.Value.x},{bestCell.Value.y}){sideLabel} [cost={cost}]");
                    return;
                }
            }

            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」移動先なし → 待機 [cost={cost}]");
        }

        private int CalcDistToNearestEnemy(Vector2Int cell, int mySide, int enemySide)
        {
            var enemies = Field.GetAllAlive(enemySide);
            if (enemies.Count == 0) return int.MaxValue;

            int minDist = int.MaxValue;
            foreach (var enemy in enemies)
            {
                var ep = Field.FindUnit(enemy);
                // sideまたぎ距離
                int dist = cell.x + 1 + ep[1] + Mathf.Abs(cell.y - ep[2]);
                if (dist < minDist) minDist = dist;
            }
            return minDist;
        }

        private int CalcDistToNearestEnemySameSide(Vector2Int cell, int side)
        {
            var enemies = Field.GetAllAlive(side);
            if (enemies.Count == 0) return int.MaxValue;

            int minDist = int.MaxValue;
            foreach (var enemy in enemies)
            {
                var ep = Field.FindUnit(enemy);
                int dist = Mathf.Abs(cell.x - ep[1]) + Mathf.Abs(cell.y - ep[2]);
                if (dist < minDist) minDist = dist;
            }
            return minDist;
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
            var actorCell = new Vector2Int(actorPos[1], actorPos[2]);

            EffectTargetSide targetSide = skill.effectTargetSide;
            List<SpaceJourneyUnit> candidates;

            if (targetSide == EffectTargetSide.Self)
                candidates = Field.GetAllAlive(mySide).Where(u => u != actor).ToList();
            else
                candidates = Field.GetAllAlive(BattleField.OppositeSide(mySide));

            if (candidates.Count == 0) return null;

            // GridRangePattern による射程フィルタ
            if (skill.targetRange != null && skill.targetRange.Offsets.Count > 0)
            {
                candidates = candidates.Where(c => IsInRange(actorCell, mySide, c, skill.targetRange)).ToList();
                if (candidates.Count == 0) return null;
            }

            // 最も近い候補を選択
            return candidates.OrderBy(c => Field.Distance(actor, c)).First();
        }

        /// <summary>対象が射程パターン内にいるか判定</summary>
        private bool IsInRange(Vector2Int actorCell, int actorSide, SpaceJourneyUnit target, GridRangePattern range)
        {
            var targetPos = Field.FindUnit(target);
            if (targetPos.x < 0) return false;

            int targetSide = targetPos[0];
            var targetCell = new Vector2Int(targetPos[1], targetPos[2]);

            // 同じ side の場合: 単純なオフセット比較
            if (actorSide == targetSide)
            {
                var offset = targetCell - actorCell;
                return range.Contains(offset);
            }

            // 異なる side の場合: 自陣から見た相対座標に変換
            // 自陣の前列(x=0) と 敵陣の前列(x=0) が隣接
            // 敵陣の座標を自陣基準に変換: x → -(targetCell.x + 1), y はそのまま
            var crossOffset = new Vector2Int(-(targetCell.x + 1), targetCell.y - actorCell.y);
            return range.Contains(crossOffset);
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
            FirePassives(timing, null, null, null);
        }

        private void FirePassives(int timing, SpaceJourneyUnit triggerUnit, SpaceJourneyUnit otherUnit, SkillDefinition triggerSkill)
        {
            foreach (var state in unitStates.Values)
            {
                if (state.Unit.IsDead) continue;
                FirePassivesForUnit(state, timing, triggerUnit, otherUnit, triggerSkill);
            }
        }

        private void FirePassivesForUnit(UnitBattleState state, int timing)
        {
            FirePassivesForUnit(state, timing, null, null, null);
        }

        private void FirePassivesForUnit(UnitBattleState state, int timing,
            SpaceJourneyUnit triggerUnit, SpaceJourneyUnit otherUnit, SkillDefinition triggerSkill)
        {
            if (state.PassiveSkills == null || state.PassiveSkills.Count == 0) return;

            var unit = state.Unit;
            var unitPos = Field.FindUnit(unit);
            int mySide = unitPos[0];

            foreach (var passive in state.PassiveSkills)
            {
                if (passive == null) continue;
                if (passive.category != SkillCategory.Passive) continue;

                // タイミングチェック: passiveTimings のいずれかが timing に一致するか
                bool timingMatch = false;
                if (passive.passiveTimings != null)
                {
                    foreach (var t in passive.passiveTimings)
                    {
                        if ((int)t == timing) { timingMatch = true; break; }
                    }
                }
                if (!timingMatch) continue;

                // コンテキスト構築
                var ctx = BuildTriggerContext(state, timing, triggerUnit, otherUnit, triggerSkill);

                // 条件チェック
                if (!SkillOccasionEvaluator.AreAllTrue(passive.passiveConditions, ctx))
                    continue;

                // パッシブ発動: 追加効果を適用
                ApplyPassiveEffect(state, passive);
            }
        }

        private SkillTriggerContext BuildTriggerContext(UnitBattleState state, int timing,
            SpaceJourneyUnit triggerUnit, SpaceJourneyUnit otherUnit, SkillDefinition triggerSkill)
        {
            var unit = state.Unit;
            var unitPos = Field.FindUnit(unit);
            int mySide = unitPos[0];

            return new SkillTriggerContext
            {
                timing = (SkillTriggerTiming)timing,
                self = unit,
                other = otherUnit,
                usedSkill = triggerSkill,
                selfHpRate = unit.MaxHp > 0 ? (float)unit.CurrentHp / unit.MaxHp : 1f,
                otherHpRate = otherUnit != null && otherUnit.MaxHp > 0
                    ? (float)otherUnit.CurrentHp / otherUnit.MaxHp : 1f,
                enemyCount = Field.GetAllAlive(BattleField.OppositeSide(mySide)).Count,
                allyCount = Field.GetAllAlive(mySide).Count,
                selfAttackCount = state.AttackCount,
                usedSkillTags = triggerSkill?.skillTags ?? SkillTag.None,
            };
        }

        private void ApplyPassiveEffect(UnitBattleState state, SkillDefinition passive)
        {
            var unit = state.Unit;

            // パッシブの追加効果を自分 or 対象に適用
            if (passive.AdditionalEffects != null)
            {
                foreach (var eff in passive.AdditionalEffects)
                {
                    if (eff.effectType == StatusEffectType.None) continue;
                    if (UnityEngine.Random.value > eff.probability) continue;

                    int duration = eff.duration > 0 ? eff.duration : 2;
                    var target = passive.effectTargetSide == EffectTargetSide.Self ? unit : unit;
                    target.ApplyStatusEffect(eff.effectType, eff.value, duration, CurrentTime);
                }
            }

            Log.Add($"    [パッシブ] {GetLabel(unit)} 「{passive.SkillName}」発動");
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
