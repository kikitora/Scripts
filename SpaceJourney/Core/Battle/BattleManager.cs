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

        private const int MaxTime = 15;
        private const int InitiativeDelay = 5;
        private const float AgiCap = 600f;
        private const float MaxCostReductionRate = 0.35f;

        // 詠唱情報
        public class CastingInfo
        {
            public SkillDefinition Skill;
            public SpaceJourneyUnit PrimaryTarget;
            public int FireTime;
            public List<(int side, Vector2Int cell)> TargetCells = new();
        }

        // ユニットごとの戦闘状態
        public class UnitBattleState
        {
            public SpaceJourneyUnit Unit;
            public float NextActTime;
            public int NormalTurnCount;
            public int AttackCount; // 戦闘中の攻撃回数 (パッシブ条件用)
            public List<BattleActionEntry> ActionList;

            // 詠唱中 (activationIndexInCost > 0 のスキル使用中)
            public CastingInfo Casting;

            // スキルごとの再使用カウントダウン: skillId → 次に使える t
            public Dictionary<string, float> SkillCooldowns = new();

            // このユニットのパッシブスキル一覧
            public List<SkillDefinition> PassiveSkills = new();

            // ─── 種族パッシブ用コンテキスト ─────────────────
            // Rat: 残り移動コスト-1使用回数
            public int RatScurryRemaining = 2;
            // Robot: 残り状態異常無効化回数
            public int RobotImmuneRemaining = 3;
            // Human: 重スキル CT-1 をまだ使ってないか
            public bool HumanFocusAvailable = true;
            // Orc: 次攻撃のダメージブースト% (0なら無効)。HP閾値到達で 5/10/15 に上書き
            public int OrcNextAttackBoostPercent = 0;
            // Orc: どの閾値を越えたか記録 (0=75%超, 1=75%到達, 2=50%, 3=25%)
            public int OrcLastHpTier = 0;
            // Reaper: 現タイム内で既に発動済みか
            public int ReaperLastFiredTime = -1;
            // Reaper: 戦闘中の総発動回数 (最大3回)
            public int ReaperFiredTotal = 0;
            // 連撃: 次スキル発動直後に同tでもう1度行動可能
            public bool DoubleActionReady = false;

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

        // ─── 種族パッシブ SkillId 定数 ─────────────────
        private const string RACE_HUMAN = "Race_Human_Focus";
        private const string RACE_RAT = "Race_Rat_Scurry";
        private const string RACE_GOBLIN = "Race_Goblin_Ambush";
        private const string RACE_ORC = "Race_Orc_RevengeBlow";
        private const string RACE_SKELETON = "Race_Skeleton_MagicResist";
        private const string RACE_SUCCUBUS = "Race_Succubus_AllureAura";
        private const string RACE_REAPER = "Race_Reaper_SoulHarvest";
        private const string RACE_ROBOT = "Race_Robot_Synthetic";

        /// <summary>指定の種族パッシブSkillIdを持っているか</summary>
        private bool HasRacePassive(UnitBattleState state, string skillId)
        {
            if (state?.PassiveSkills == null) return false;
            foreach (var p in state.PassiveSkills)
                if (p != null && p.SkillId == skillId) return true;
            return false;
        }

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

            // 状態異常ティック処理 (Burn/ChainDamage)
            ApplyStatusTickDamage(t);
            CheckVictory();
            if (IsFinished) return;

            // 詠唱発動チェック (fire_time 到達で実際スキル発動)
            FirePendingCasts(t);
            CheckVictory();
            if (IsFinished) return;

            // Succubus の隣接敵AT-10% オーラ適用 (毎タイム)
            ApplySuccubusAura(t);

            // Reaper のタイム単位クールダウンリセット
            ResetReaperPerTurn(t);

            foreach (var state in actors)
            {
                if (state.Unit.IsDead || IsFinished) continue;

                // 詠唱中ユニットは他行動不可
                if (state.Casting != null)
                {
                    Log.Add($"  [{GetLabel(state.Unit)}] 詠唱中「{state.Casting.Skill.SkillName}」(t={state.Casting.FireTime}発動)");
                    continue;
                }

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

        /// <summary>詠唱中ユニットの FireTime が t に達したら実発動</summary>
        private void FirePendingCasts(int t)
        {
            foreach (var state in unitStates.Values)
            {
                if (state.Unit.IsDead || state.Casting == null) continue;
                if (state.Casting.FireTime != t) continue;
                var skill = state.Casting.Skill;
                var primary = state.Casting.PrimaryTarget;
                if (primary == null || primary.IsDead)
                {
                    Log.Add($"  [{GetLabel(state.Unit)}] 詠唱発動「{skill.SkillName}」→ 対象不在で不発");
                    state.Casting = null;
                    continue;
                }
                Log.Add($"  [{GetLabel(state.Unit)}] 詠唱発動「{skill.SkillName}」!");
                if (skill.category == SkillCategory.ActiveAttack)
                    ExecuteAttackSkill(state, skill, primary, t, 0);
                else if (skill.category == SkillCategory.ActiveSupport)
                    ExecuteSupportSkill(state, skill, primary, t, 0);
                state.Casting = null;
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
                ExecuteSkill(state, skill, target, t, entry);
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

        /// <summary>全生存ユニットに Burn/ChainDamage の1tick分を適用</summary>
        private void ApplyStatusTickDamage(int t)
        {
            foreach (var s in unitStates.Values)
            {
                var unit = s.Unit;
                if (unit.IsDead) continue;

                var applied = unit.TickPeriodicDamage();
                foreach (var (type, dmg) in applied)
                {
                    string effName = type == StatusEffectType.Burn ? "燃焼" : "連鎖ダメージ";
                    string deadMark = unit.IsDead ? " ★撃破!" : "";
                    Log.Add($"    [{GetLabel(unit)}] {effName} {dmg}ダメ (HP {unit.CurrentHp + dmg}→{unit.CurrentHp}){deadMark}");
                }
            }
        }

        /// <summary>Succubus の「誘惑の気配」: 隣接中の敵に AT-10% を毎タイム更新</summary>
        private void ApplySuccubusAura(int t)
        {
            foreach (var s in unitStates.Values)
            {
                var unit = s.Unit;
                if (unit.IsDead) continue;
                if (!HasRacePassive(s, RACE_SUCCUBUS)) continue;

                var pos = Field.FindUnit(unit);
                if (pos.x < 0) continue;
                int physSide = pos[0];
                int ownerSide = Field.GetSide(unit);
                int enemySide = BattleField.OppositeSide(ownerSide);
                var cell = new Vector2Int(pos[1], pos[2]);

                // 隣接4方向の敵を取得
                foreach (var enemy in Field.GetAllAlive(enemySide))
                {
                    var bfOff = CalcBFOffset(cell, physSide, enemy);
                    if (!bfOff.HasValue) continue;
                    int dist = Mathf.Abs(bfOff.Value.x) + Mathf.Abs(bfOff.Value.y);
                    if (dist != 1) continue; // 隣接のみ

                    // DebuffAt -10% duration=1 (次タイムで切れる、毎タイム再付与)
                    enemy.ApplyStatusEffect(StatusEffectType.DebuffAt, 10, 1, t);
                }
            }
        }

        /// <summary>Reaper の「1ターンに1回」制限を、新タイムでリセット</summary>
        private void ResetReaperPerTurn(int t)
        {
            // ReaperLastFiredTime はタイム単位で判定するため、毎タイム開始時に
            // 前タイムと異なれば発動可能になる (コード上は自動反映されるため実質何もしない)。
            // この関数はセマンティクスを明示するために残す。
        }

        private void ExecuteSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t, BattleActionEntry currentEntry = null)
        {
            var actor = state.Unit;
            int baseCost = skill.baseCost;
            int effectiveCost = CalcEffectiveCost(baseCost, actor.AgiFinal);

            // Rat: 純移動スキルなら最初の2回までコスト-1 (最低1)
            if (skill.category == SkillCategory.ActiveMove
                && HasRacePassive(state, RACE_RAT)
                && state.RatScurryRemaining > 0)
            {
                int reduced = Mathf.Max(1, effectiveCost - 1);
                if (reduced < effectiveCost)
                {
                    Log.Add($"    [種族] {GetLabel(actor)} 「疾走本能」コスト{effectiveCost}→{reduced} (残り{state.RatScurryRemaining - 1}/2)");
                    effectiveCost = reduced;
                    state.RatScurryRemaining--;
                }
            }

            state.NextActTime = t + effectiveCost;

            // 連撃: DoubleActionReady ならコスト無視で同t再行動可能に
            // ただし連撃スキル (SpecialKind.DoubleActionCharge) 自身の使用ではフラグを消費しない
            if (state.DoubleActionReady && skill.SpecialKind != SpecialSkillKind.DoubleActionCharge)
            {
                state.NextActTime = t;
                state.DoubleActionReady = false;
                Log.Add($"    [連撃] {GetLabel(actor)} 「{skill.SkillName}」同タイムで追加行動可");
            }

            // CT (再使用まで) 設定。Human: 「最初に使う 再使用>コスト のスキル」は戦闘中1回、CT-1
            if (skill.reuseCycle > 0)
            {
                int appliedCycle = skill.reuseCycle;
                if (state.HumanFocusAvailable
                    && HasRacePassive(state, RACE_HUMAN)
                    && skill.reuseCycle > skill.baseCost)
                {
                    appliedCycle = Mathf.Max(1, skill.reuseCycle - 1);
                    Log.Add($"    [種族] {GetLabel(actor)} 「集中」「{skill.SkillName}」CT {skill.reuseCycle}→{appliedCycle} (戦闘中1回)");
                    state.HumanFocusAvailable = false;
                }
                // CycleDelay: 所持してれば +value 加算
                int cycleDelay = actor.GetActiveEffectValue(StatusEffectType.CycleDelay);
                if (cycleDelay > 0)
                {
                    appliedCycle += cycleDelay;
                    Log.Add($"    [牽制] {GetLabel(actor)} 「{skill.SkillName}」CT +{cycleDelay}");
                }
                state.SkillCooldowns[skill.SkillId] = t + appliedCycle;
            }

            var actorPos = Field.FindUnit(actor);

            // ─── 詠唱スキル: activationIndexInCost > 0 なら発動を遅延 ───
            if (skill.activationIndexInCost > 0
                && (skill.category == SkillCategory.ActiveAttack || skill.category == SkillCategory.ActiveSupport))
            {
                int fireT = t + skill.activationIndexInCost;
                var castCells = PredictCastCells(state, skill, target);
                state.Casting = new CastingInfo
                {
                    Skill = skill,
                    PrimaryTarget = target,
                    FireTime = fireT,
                    TargetCells = castCells,
                };
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」詠唱開始 → t={fireT}発動 [cost={effectiveCost}]");
                if (skill.category == SkillCategory.ActiveAttack) state.AttackCount++;
                return;
            }

            // ─── 特殊実行スキル (共通レアスキル等) を先にハンドリング ───
            if (skill.SpecialKind != SpecialSkillKind.None)
            {
                ExecuteSpecialSkill(state, skill, target, t, effectiveCost);
                return;
            }

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
                    ExecuteMoveSkill(state, skill, currentEntry, t, effectiveCost);
                    break;

                default:
                    Log.Add($"  [{GetLabel(actor)}] {skill.SkillName} (未実装カテゴリ) [cost={effectiveCost}]");
                    break;
            }
        }

        // ================================================================
        // 特殊実行スキル (共通レアスキル: 入れ替え / 引き寄せ / 浄化 / 嵐)
        // ================================================================
        private void ExecuteSpecialSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            var actor = state.Unit;
            switch (skill.SpecialKind)
            {
                case SpecialSkillKind.SwapAlly:
                    ExecuteSwapAlly(actor, skill, target, t, cost);
                    break;
                case SpecialSkillKind.PullEnemy:
                    ExecutePullEnemy(actor, skill, target, t, cost);
                    break;
                case SpecialSkillKind.DispelAlly:
                    ExecuteDispelAlly(actor, skill, target, t, cost);
                    break;
                case SpecialSkillKind.RandomMoveEnemies:
                    ExecuteRandomMoveEnemies(state, skill, t, cost);
                    break;
                case SpecialSkillKind.CancelEnemyAction:
                    ExecuteCancelEnemyAction(actor, skill, target, t, cost);
                    break;
                case SpecialSkillKind.DoubleActionCharge:
                    state.DoubleActionReady = true;
                    Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 連撃準備完了 [cost={cost}]");
                    break;
                case SpecialSkillKind.StealBuffs:
                    ExecuteStealBuffs(actor, skill, target, t, cost);
                    break;
            }
            ApplyAdditionalEffects(skill, actor, actor, t);
        }

        private void ExecuteCancelEnemyAction(SpaceJourneyUnit actor, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            if (target == null || target.IsDead)
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 対象なし [cost={cost}]");
                return;
            }
            var tgtState = GetUnitState(target);
            if (tgtState == null) return;
            bool wasCasting = tgtState.Casting != null;
            tgtState.Casting = null;
            tgtState.NextActTime += 3;
            string detail = wasCasting ? "詠唱キャンセル" : "次行動+3遅延";
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(target)}] {detail} [cost={cost}]");
        }

        private static readonly StatusEffectType[] StealableBuffs = new[]
        {
            StatusEffectType.BuffAt, StatusEffectType.BuffDf, StatusEffectType.BuffAgi,
            StatusEffectType.BuffMat, StatusEffectType.BuffMdf,
        };

        private void ExecuteStealBuffs(SpaceJourneyUnit actor, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            if (target == null || target.IsDead)
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 対象なし [cost={cost}]");
                return;
            }
            int stolen = target.StealBuffsTo(actor, StealableBuffs, 3, t);
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(target)}] バフ{stolen}個奪取 [cost={cost}]");
        }

        private void ExecuteSwapAlly(SpaceJourneyUnit actor, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            if (target == null || target.IsDead || target == actor)
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 対象なし [cost={cost}]");
                return;
            }
            var aPos = Field.FindUnit(actor);
            var tPos = Field.FindUnit(target);
            if (aPos.x < 0 || tPos.x < 0) return;
            Field.Swap(actor, target);
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」⇄ [{GetLabel(target)}] と位置交換 [cost={cost}]");
        }

        private void ExecutePullEnemy(SpaceJourneyUnit actor, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            if (target == null || target.IsDead)
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 対象なし [cost={cost}]");
                return;
            }
            var aPos = Field.FindUnit(actor);
            if (aPos.x < 0) return;
            int aSide = aPos[0];
            var aCell = new Vector2Int(aPos[1], aPos[2]);

            // 自分の隣接 (同物理side 内) マス4方向のうち空いてるところへ配置
            var candidates = new List<Vector2Int>
            {
                new Vector2Int(aCell.x - 1, aCell.y),
                new Vector2Int(aCell.x + 1, aCell.y),
                new Vector2Int(aCell.x, aCell.y - 1),
                new Vector2Int(aCell.x, aCell.y + 1),
            };
            foreach (var c in candidates)
            {
                if (c.x < 0 || c.x >= 5 || c.y < 0 || c.y >= 5) continue;
                if (Field.GetUnit(aSide, c) != null) continue;
                Field.MoveUnitCrossSide(target, aSide, c);
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(target)}] を引き寄せ ({c.x},{c.y}) [cost={cost}]");
                return;
            }
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 引き寄せ先なし [cost={cost}]");
        }

        private void ExecuteDispelAlly(SpaceJourneyUnit actor, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            var tgt = target ?? actor;
            if (tgt.IsDead) return;
            int removed = tgt.DispelAllDebuffs();
            // HP20%回復 (amount=0 でも固定回復)
            int heal = Mathf.RoundToInt(tgt.MaxHp * 0.20f);
            tgt.Heal(heal);
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(tgt)}] デバフ{removed}個解除 +{heal}回復 [cost={cost}]");
        }

        private void ExecuteRandomMoveEnemies(UnitBattleState state, SkillDefinition skill, int t, int cost)
        {
            var actor = state.Unit;
            var enemies = FindAllTargetsInRange(state, skill);
            if (enemies == null || enemies.Count == 0)
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ 対象なし [cost={cost}]");
                return;
            }
            int movedCount = 0;
            var dirs = new[]
            {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(0, 1),
            };
            foreach (var e in enemies)
            {
                if (e.IsDead) continue;
                var ePos = Field.FindUnit(e);
                if (ePos.x < 0) continue;
                int eSide = ePos[0];
                var eCell = new Vector2Int(ePos[1], ePos[2]);
                // ランダム順に試行
                var shuffled = dirs.OrderBy(_ => UnityEngine.Random.value).ToArray();
                foreach (var d in shuffled)
                {
                    var newCell = eCell + d;
                    if (newCell.x < 0 || newCell.x >= 5 || newCell.y < 0 || newCell.y >= 5) continue;
                    if (Field.GetUnit(eSide, newCell) != null) continue;
                    Field.MoveUnit(e, newCell);
                    movedCount++;
                    break;
                }
            }
            Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ {movedCount}体をランダム移動 [cost={cost}]");
        }

        /// <summary>詠唱予告マス一覧を計算</summary>
        private List<(int side, Vector2Int cell)> PredictCastCells(
            UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit primary)
        {
            var cells = new List<(int, Vector2Int)>();
            bool isAoE = skill.selectAllInRange
                || (skill.targetingMode == SkillTargetingMode.MultiSingle
                    && skill.multiSinglePickMode == MultiSinglePickMode.AllInRange);
            if (isAoE)
            {
                foreach (var c in FindAllTargetsInRange(state, skill))
                {
                    var p = Field.FindUnit(c);
                    if (p.x >= 0) cells.Add((p[0], new Vector2Int(p[1], p[2])));
                }
            }
            else
            {
                var p = Field.FindUnit(primary);
                if (p.x >= 0) cells.Add((p[0], new Vector2Int(p[1], p[2])));
            }
            return cells;
        }

        private void ExecuteAttackSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            var actor = state.Unit;

            // AoE判定: selectAllInRange フラグ or targetingMode+multiSinglePickMode の組み合わせ
            bool isAoE = skill.selectAllInRange
                || (skill.targetingMode == SkillTargetingMode.MultiSingle
                    && skill.multiSinglePickMode == MultiSinglePickMode.AllInRange);

            var targets = isAoE
                ? FindAllTargetsInRange(state, skill)
                : (target != null ? new List<SpaceJourneyUnit> { target } : new List<SpaceJourneyUnit>());

            if (targets.Count == 0) return;

            // AoE吸収: AoEのtargetsにAoeAbsorb持ちがいれば、その1体に集約
            if (isAoE && targets.Count > 1)
            {
                var absorber = targets.FirstOrDefault(u => u.HasActiveEffect(StatusEffectType.AoeAbsorb));
                if (absorber != null)
                {
                    Log.Add($"    [{GetLabel(absorber)}] AoE吸収! 他{targets.Count - 1}体への被害を引き受け");
                    // AoeAbsorb状態を消費
                    absorber.ApplyStatusEffect(StatusEffectType.AoeAbsorb, 0, 1, t - 10); // 即expire
                    targets = new List<SpaceJourneyUnit> { absorber };
                }
            }

            foreach (var origTgt in targets)
            {
                if (origTgt.IsDead) continue;

                // CoverAlly: 隣接味方が身代わり状態なら、その味方が被弾
                var tgt = TryRedirectToCoverer(origTgt);
                if (tgt != origTgt)
                {
                    Log.Add($"    [{GetLabel(tgt)}] 🛡身代わり! [{GetLabel(origTgt)}] への攻撃を受ける");
                    tgt.ConsumeActiveEffect(StatusEffectType.CoverAlly);
                    if (tgt.IsDead) continue;
                }

                int damage = CalculateSkillDamage(skill, actor, tgt);

                // Invincible: 全ダメ0
                if (tgt.HasActiveEffect(StatusEffectType.Invincible))
                {
                    damage = 0;
                }

                // SurviveLethal: 致死→1HP残し、効果消費
                bool survived = false;
                if (damage >= tgt.CurrentHp && tgt.HasActiveEffect(StatusEffectType.SurviveLethal))
                {
                    damage = Mathf.Max(0, tgt.CurrentHp - 1);
                    tgt.ConsumeActiveEffect(StatusEffectType.SurviveLethal);
                    survived = true;
                }

                int hpBefore = tgt.CurrentHp;
                tgt.TakeDamage(damage);
                state.AttackCount++;

                string deadMark = tgt.IsDead ? " ★撃破!" : (survived ? " ☆不屈!" : "");
                Log.Add($"  [{GetLabel(actor)}] →「{skill.SkillName}」→ [{GetLabel(tgt)}] " +
                        $"{damage}ダメ (HP {hpBefore}→{tgt.CurrentHp}){deadMark} [cost={cost}]");

                // 追加効果
                ApplyAdditionalEffects(skill, actor, tgt, t);

                // 被ダメ後の種族処理 (Orc: HP閾値到達で次攻撃ブースト)
                UpdateOrcRevengeOnDamaged(tgt);

                // パッシブ発火: 攻撃命中
                FirePassivesForUnit(state, 1, actor, tgt, skill);

                // 被弾パッシブ
                var defenderState = GetUnitState(tgt);
                if (defenderState != null)
                    FirePassivesForUnit(defenderState, 3, tgt, actor, skill);

                // Counter: 被害者生存 && 攻撃者生存 && Counter持ち → 即時反撃割り込み
                if (!tgt.IsDead && !actor.IsDead && tgt.HasActiveEffect(StatusEffectType.Counter))
                {
                    int rate = Mathf.Max(1, tgt.GetActiveEffectValue(StatusEffectType.Counter));
                    float atkPower = tgt.AtFinal * (rate / 100f);
                    int counterDmg = Mathf.Max(1, CalcDamageWithThroughRate(atkPower, actor.DfFinal));
                    int counterHpBefore = actor.CurrentHp;
                    actor.TakeDamage(counterDmg);
                    Log.Add($"    [{GetLabel(tgt)}] ⚔反撃! [{GetLabel(actor)}] に{counterDmg}ダメ (HP {counterHpBefore}→{actor.CurrentHp})");
                    if (actor.IsDead)
                        FireReaperOnAllyDeath(actor, t);
                }

                // 撃破したら Reaper イベントを発火
                if (tgt.IsDead)
                    FireReaperOnAllyDeath(tgt, t);
            }

            // スキル使用後の追加移動 (moveAfterSkill=true)
            if (skill.moveAfterSkill && !actor.IsDead)
            {
                ExecuteMoveAfterSkill(actor, skill);
            }
        }

        /// <summary>moveAfterSkill=true のスキルで、moveAfterRange の offset分actor を動かす</summary>
        private void ExecuteMoveAfterSkill(SpaceJourneyUnit actor, SkillDefinition skill)
        {
            if (skill.moveAfterRange == null || skill.moveAfterRange.Offsets.Count == 0) return;
            var pos = Field.FindUnit(actor);
            if (pos.x < 0) return;
            int curSide = pos[0];
            var curCell = new Vector2Int(pos[1], pos[2]);
            foreach (var off in skill.moveAfterRange.Offsets)
            {
                var dest = curCell + off;
                if (Field.IsValidCell(dest) && Field.IsCellEmpty(curSide, dest))
                {
                    if (Field.MoveUnit(actor, dest))
                    {
                        Log.Add($"    [{GetLabel(actor)}] 攻撃後移動 ({curCell.x},{curCell.y})→({dest.x},{dest.y})");
                    }
                    break;
                }
            }
        }

        /// <summary>Orc の「逆境の一撃」: HP が 75/50/25% の閾値を跨いだら、次攻撃のダメージブーストを設定</summary>
        private void UpdateOrcRevengeOnDamaged(SpaceJourneyUnit victim)
        {
            var vs = GetUnitState(victim);
            if (vs == null) return;
            if (!HasRacePassive(vs, RACE_ORC)) return;
            if (victim.MaxHp <= 0) return;

            float rate = (float)victim.CurrentHp / victim.MaxHp;
            int tier = 0;
            if (rate < 0.25f) tier = 3;
            else if (rate < 0.50f) tier = 2;
            else if (rate < 0.75f) tier = 1;

            if (tier > vs.OrcLastHpTier)
            {
                // 新しい閾値到達 → ブースト上書き (5/10/15%)
                int boost = tier == 1 ? 5 : tier == 2 ? 10 : 15;
                vs.OrcNextAttackBoostPercent = boost;
                vs.OrcLastHpTier = tier;
                Log.Add($"    [種族] {GetLabel(victim)} 「逆境の一撃」HP<{(int)((1 - tier * 0.25f) * 100)}% 次攻撃+{boost}%");
            }
        }

        /// <summary>Reaper の「魂刈り」: 味方が倒れた時、CT中スキルの残り時間を-1 (1タイム1回/戦闘中最大-3)</summary>
        private void FireReaperOnAllyDeath(SpaceJourneyUnit deadUnit, int t)
        {
            int deadSide = Field.GetSide(deadUnit);
            foreach (var s in unitStates.Values)
            {
                if (s.Unit.IsDead) continue;
                if (s.Unit == deadUnit) continue;
                if (Field.GetSide(s.Unit) != deadSide) continue; // 同side (味方) のみ
                if (!HasRacePassive(s, RACE_REAPER)) continue;
                if (s.ReaperLastFiredTime == t) continue; // 同タイム2回目は不可
                if (s.ReaperFiredTotal >= 3) continue; // 戦闘中最大3回

                // CT中のスキルから1つ選んで残り時間を-1
                string targetSkillId = null;
                float latestReady = -1;
                foreach (var kv in s.SkillCooldowns)
                {
                    if (kv.Value > t && kv.Value > latestReady)
                    {
                        latestReady = kv.Value;
                        targetSkillId = kv.Key;
                    }
                }
                if (targetSkillId == null) continue;

                s.SkillCooldowns[targetSkillId] = latestReady - 1;
                s.ReaperLastFiredTime = t;
                s.ReaperFiredTotal++;
                Log.Add($"    [種族] {GetLabel(s.Unit)} 「魂刈り」→ 「{targetSkillId}」のCT-1 (累計{s.ReaperFiredTotal}/3)");
            }
        }

        /// <summary>射程内の全対象を返す (maxTargetsで上限制限)</summary>
        private List<SpaceJourneyUnit> FindAllTargetsInRange(UnitBattleState state, SkillDefinition skill)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);
            int physicalSide = actorPos[0];
            int ownerSide = Field.GetSide(actor);
            var actorCell = new Vector2Int(actorPos[1], actorPos[2]);

            List<SpaceJourneyUnit> candidates;
            if (skill.effectTargetSide == EffectTargetSide.Self)
                candidates = Field.GetAllAlive(ownerSide).Where(u => u != actor).ToList();
            else
                candidates = Field.GetAllAlive(BattleField.OppositeSide(ownerSide));

            if (skill.targetRange != null && skill.targetRange.Offsets.Count > 0)
            {
                candidates = candidates.Where(c => IsInRange(actorCell, physicalSide, c, skill.targetRange, actor)).ToList();
            }

            // AllInRange の場合は maxTargets を無視 (SO設計上 maxTargets=1 強制だが全員ヒットが正しい)
            bool allInRange = skill.multiSinglePickMode == MultiSinglePickMode.AllInRange;
            if (!allInRange && skill.maxTargets > 0 && candidates.Count > skill.maxTargets)
            {
                candidates = candidates.OrderBy(c => Field.Distance(actor, c)).Take(skill.maxTargets).ToList();
            }

            return candidates;
        }

        private void ExecuteSupportSkill(UnitBattleState state, SkillDefinition skill, SpaceJourneyUnit target, int t, int cost)
        {
            var actor = state.Unit;

            // AoE判定: selectAllInRange or MultiSingle+AllInRange
            bool isAoE = skill.selectAllInRange
                || (skill.targetingMode == SkillTargetingMode.MultiSingle
                    && skill.multiSinglePickMode == MultiSinglePickMode.AllInRange);

            var healTargets = new List<SpaceJourneyUnit>();
            if (isAoE && skill.effectRange != null && skill.effectRange.Offsets.Count > 0)
            {
                healTargets = FindAllAlliesInEffectRange(state, skill);
            }
            else if (skill.targetingMode == SkillTargetingMode.SelfArea
                     && skill.effectTargetSide == EffectTargetSide.Self)
            {
                // 自己バフ/状態変化スキル (Provoke/Defend等) は actor 自身に効果適用
                healTargets.Add(actor);
            }
            else
            {
                healTargets.Add(target ?? actor);
            }

            if (healTargets.Count == 0) healTargets.Add(actor);

            foreach (var ht in healTargets)
            {
                if (ht.IsDead) continue;

                if (skill.damageKind == SkillDamageKind.Fixed && skill.amount < 0)
                {
                    int heal = -skill.amount;
                    ht.Heal(heal);
                    Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(ht)}] {heal}回復 " +
                            $"(HP:{ht.CurrentHp - heal}→{ht.CurrentHp}) [cost={cost}]");
                }
                else if (skill.damageKind == SkillDamageKind.MaxHpRate && skill.amount < 0)
                {
                    int heal = Mathf.RoundToInt(ht.MaxHp * (-skill.amount / 100f));
                    ht.Heal(heal);
                    Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」→ [{GetLabel(ht)}] {heal}回復 " +
                            $"(HP:{ht.CurrentHp - heal}→{ht.CurrentHp}) [cost={cost}]");
                }
                else if (ht == healTargets[0])
                {
                    Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」[cost={cost}]");
                }

                ApplyAdditionalEffects(skill, actor, ht, t);
            }
        }

        /// <summary>effectRange 内にいる味方 (actor含む) を返す</summary>
        private List<SpaceJourneyUnit> FindAllAlliesInEffectRange(UnitBattleState state, SkillDefinition skill)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);
            int physicalSide = actorPos[0];
            int ownerSide = Field.GetSide(actor);
            var actorCell = new Vector2Int(actorPos[1], actorPos[2]);

            var result = new List<SpaceJourneyUnit>();
            var allies = Field.GetAllAlive(ownerSide);
            foreach (var ally in allies)
            {
                if (IsInRange(actorCell, physicalSide, ally, skill.effectRange) || ally == actor)
                    result.Add(ally);
            }

            if (skill.maxTargets > 0 && result.Count > skill.maxTargets)
            {
                result = result.OrderBy(c => Field.Distance(actor, c)).Take(skill.maxTargets).ToList();
            }

            return result;
        }

        private void ExecuteMoveSkill(UnitBattleState state, SkillDefinition skill, BattleActionEntry entry, int t, int cost)
        {
            var actor = state.Unit;
            var actorPos = Field.FindUnit(actor);
            int side = actorPos[0];
            var currentCell = new Vector2Int(actorPos[1], actorPos[2]);
            int ownerSide = Field.GetSide(actor);
            int enemySide = BattleField.OppositeSide(ownerSide);

            var moveTarget = entry?.moveTarget ?? MoveTargetKind.NearestEnemy;
            string moveParam = entry?.moveTargetParam ?? "";

            int steps = Mathf.Max(1, skill.moveSteps);
            var startCell = currentCell;
            int startSide = side;
            int totalMoved = 0;

            for (int step = 0; step < steps; step++)
            {
                var pos = Field.FindUnit(actor);
                int curSide = pos[0];
                var curCell = new Vector2Int(pos[1], pos[2]);

                // 目標地点を決める
                var goalInfo = CalcMoveGoal(actor, curSide, curCell, ownerSide, enemySide, moveTarget, moveParam);
                if (!goalInfo.HasValue) break;

                var (goalSide, goalCell) = goalInfo.Value;

                // moveRange のオフセットから最適な1歩を選ぶ
                // 現在位置の距離を初期 bestDist にすることで、遠くなる移動や同距離移動(=往復)を防ぐ
                int currentDist = CalcDistToGoal(curCell, curSide, goalCell, goalSide);
                Vector2Int? bestCell = null;
                int bestSideTo = curSide;
                int bestDist = currentDist;
                bool bestCross = false;

                if (skill.moveRange != null && skill.moveRange.Offsets.Count > 0)
                {
                    foreach (var offset in skill.moveRange.Offsets)
                    {
                        var candidate = curCell + offset;

                        // 同サイド内の移動
                        if (Field.IsCellEmpty(curSide, candidate))
                        {
                            int dist = CalcDistToGoal(candidate, curSide, goalCell, goalSide);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestCell = candidate;
                                bestSideTo = curSide;
                                bestCross = false;
                            }
                        }

                        // 前列(x=FrontRow)にいて前進しようとした場合 → sideまたぎで敵陣の前列(x=FrontRow)に入る
                        // moveRange は Grid座標のため BF 前進は offset.y > 0 (Grid y+ = BF x-)
                        // ただし既に enemySide に侵入中の場合はスキップ (自陣へ戻らない)
                        if (curSide == ownerSide && curCell.x == Field.FrontRow && offset.y > 0)
                        {
                            var crossCell = new Vector2Int(Field.FrontRow, curCell.y);
                            int otherSide = BattleField.OppositeSide(curSide);
                            if (Field.IsCellEmpty(otherSide, crossCell))
                            {
                                int dist = CalcDistToGoal(crossCell, otherSide, goalCell, goalSide);
                                if (dist < bestDist)
                                {
                                    bestDist = dist;
                                    bestCell = crossCell;
                                    bestSideTo = otherSide;
                                    bestCross = true;
                                }
                            }
                        }
                    }
                }

                if (!bestCell.HasValue) break;

                bool moved;
                if (bestCross)
                    moved = Field.MoveUnitCrossSide(actor, bestSideTo, bestCell.Value);
                else
                    moved = Field.MoveUnit(actor, bestCell.Value);

                if (!moved) break;
                totalMoved++;
            }

            // ログ出力
            var finalPos = Field.FindUnit(actor);
            var finalCell = new Vector2Int(finalPos[1], finalPos[2]);
            if (totalMoved > 0)
            {
                string crossLabel = finalPos[0] != startSide ? " [敵陣侵入]" : "";
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」移動 ({startCell.x},{startCell.y})→({finalCell.x},{finalCell.y}) {totalMoved}歩{crossLabel} [cost={cost}]");
            }
            else
            {
                Log.Add($"  [{GetLabel(actor)}] 「{skill.SkillName}」移動先なし → 待機 [cost={cost}]");
            }
        }

        /// <summary>MoveTargetKind に応じた目標地点を返す。(side, cell) のタプル。</summary>
        private (int side, Vector2Int cell)? CalcMoveGoal(
            SpaceJourneyUnit actor, int curSide, Vector2Int curCell,
            int ownerSide, int enemySide, MoveTargetKind moveTarget, string moveParam)
        {
            // 挑発(Taunt)優先: actor 自身が Taunt状態なら source (挑発者) に向かう
            var provoker = actor.GetActiveEffectSource(StatusEffectType.Taunt);
            if (provoker != null && !provoker.IsDead)
            {
                var pp = Field.FindUnit(provoker);
                if (pp[0] >= 0)
                    return (pp[0], new Vector2Int(pp[1], pp[2]));
            }

            switch (moveTarget)
            {
                case MoveTargetKind.NearestEnemy:
                {
                    var target = FindNearestAliveEnemy(actor, enemySide);
                    if (target == null) return null;
                    var tp = Field.FindUnit(target);
                    return (tp[0], new Vector2Int(tp[1], tp[2]));
                }

                case MoveTargetKind.NearestJobEnemy:
                {
                    var target = FindNearestAliveEnemyByJob(actor, enemySide, moveParam);
                    if (target == null) return FindNearestAliveEnemyGoal(actor, enemySide); // fallback
                    var tp = Field.FindUnit(target);
                    return (tp[0], new Vector2Int(tp[1], tp[2]));
                }

                case MoveTargetKind.IntruderEnemy:
                {
                    // 自陣(ownerSide)に物理的にいる敵ユニットを探す
                    var intruders = new List<SpaceJourneyUnit>();
                    foreach (var enemy in Field.GetAllAlive(enemySide))
                    {
                        var ep = Field.FindUnit(enemy);
                        if (ep[0] == ownerSide) intruders.Add(enemy);
                    }
                    if (intruders.Count == 0) return FindNearestAliveEnemyGoal(actor, enemySide); // fallback
                    var nearest = intruders.OrderBy(e => Field.Distance(actor, e)).First();
                    var np = Field.FindUnit(nearest);
                    return (np[0], new Vector2Int(np[1], np[2]));
                }

                case MoveTargetKind.FarthestEnemy:
                {
                    var enemies = Field.GetAllAlive(enemySide);
                    if (enemies.Count == 0) return null;
                    var farthest = enemies.OrderByDescending(e => Field.Distance(actor, e)).First();
                    var fp = Field.FindUnit(farthest);
                    return (fp[0], new Vector2Int(fp[1], fp[2]));
                }

                case MoveTargetKind.EnemyTerritory:
                {
                    // 敵陣の前列(x=0)中央を目指す
                    int midY = 0;
                    int count = 0;
                    foreach (var c in Field.Cells) { midY += c.y; count++; }
                    midY = count > 0 ? midY / count : 0;
                    return (enemySide, new Vector2Int(0, midY));
                }

                case MoveTargetKind.Retreat:
                {
                    // 自陣の後列(x=最大)を目指す
                    int maxX = 0;
                    foreach (var c in Field.Cells)
                        if (c.x > maxX) maxX = c.x;
                    return (ownerSide, new Vector2Int(maxX, curCell.y));
                }

                default:
                    return FindNearestAliveEnemyGoal(actor, enemySide);
            }
        }

        private (int side, Vector2Int cell)? FindNearestAliveEnemyGoal(SpaceJourneyUnit actor, int enemySide)
        {
            var target = FindNearestAliveEnemy(actor, enemySide);
            if (target == null) return null;
            var tp = Field.FindUnit(target);
            return (tp[0], new Vector2Int(tp[1], tp[2]));
        }

        private SpaceJourneyUnit FindNearestAliveEnemy(SpaceJourneyUnit actor, int enemySide)
        {
            var enemies = Field.GetAllAlive(enemySide);
            if (enemies.Count == 0) return null;
            return enemies.OrderBy(e => Field.Distance(actor, e)).First();
        }

        private SpaceJourneyUnit FindNearestAliveEnemyByJob(SpaceJourneyUnit actor, int enemySide, string bodyJobId)
        {
            var enemies = Field.GetAllAlive(enemySide)
                .Where(e => e.Body?.BodyJobId == bodyJobId)
                .ToList();
            if (enemies.Count == 0) return null;
            return enemies.OrderBy(e => Field.Distance(actor, e)).First();
        }

        /// <summary>現在位置から目標地点までの距離 (sideまたぎ対応)</summary>
        private int CalcDistToGoal(Vector2Int cell, int cellSide, Vector2Int goalCell, int goalSide)
        {
            if (cellSide == goalSide)
                return Mathf.Abs(cell.x - goalCell.x) + Mathf.Abs(cell.y - goalCell.y);

            // sideまたぎ
            return cell.x + 1 + goalCell.x + Mathf.Abs(cell.y - goalCell.y);
        }

        // ================================================================
        // ダメージ計算
        // ================================================================

        /// <summary>victim の隣接味方 (Manhattan 1) に CoverAlly 状態持ちがいれば、その味方を返す。</summary>
        private SpaceJourneyUnit TryRedirectToCoverer(SpaceJourneyUnit victim)
        {
            int vSide = Field.GetSide(victim);
            var vPos = Field.FindUnit(victim);
            if (vPos.x < 0) return victim;
            foreach (var ally in Field.GetAllAlive(vSide))
            {
                if (ally == victim) continue;
                if (!ally.HasActiveEffect(StatusEffectType.CoverAlly)) continue;
                var aPos = Field.FindUnit(ally);
                if (aPos.x < 0) continue;
                if (aPos.x != vPos.x) continue; // 同じ物理side
                int d = Mathf.Abs(aPos.y - vPos.y) + Mathf.Abs(aPos.z - vPos.z);
                if (d == 1) return ally;
            }
            return victim;
        }

        private int CalculateSkillDamage(SkillDefinition skill, SpaceJourneyUnit attacker, SpaceJourneyUnit defender)
        {
            float amount = skill.amount;
            bool isMagical = skill.damageKind == SkillDamageKind.Magical
                           || skill.damageKind == SkillDamageKind.PenetrateMagical;

            int raw;
            switch (skill.damageKind)
            {
                case SkillDamageKind.None:
                    return 0;

                case SkillDamageKind.Physical:
                {
                    float atkPower = attacker.AtFinal * (amount / 100f);
                    raw = CalcDamageWithThroughRate(atkPower, defender.DfFinal);
                    break;
                }

                case SkillDamageKind.Magical:
                {
                    float atkPower = attacker.MatFinal * (amount / 100f);
                    raw = CalcDamageWithThroughRate(atkPower, defender.MdfFinal);
                    break;
                }

                case SkillDamageKind.PenetratePhysical:
                {
                    float atkPower = attacker.AtFinal * (amount / 100f);
                    float effDef = defender.DfFinal * (1f - skill.defenseIgnorePercent / 100f);
                    raw = CalcDamageWithThroughRate(atkPower, effDef);
                    break;
                }

                case SkillDamageKind.PenetrateMagical:
                {
                    float atkPower = attacker.MatFinal * (amount / 100f);
                    float effDef = defender.MdfFinal * (1f - skill.defenseIgnorePercent / 100f);
                    raw = CalcDamageWithThroughRate(atkPower, effDef);
                    break;
                }

                case SkillDamageKind.Fixed:
                    raw = Mathf.Max(1, (int)amount);
                    break;

                case SkillDamageKind.MaxHpRate:
                    raw = Mathf.Max(1, Mathf.RoundToInt(defender.MaxHp * (amount / 100f)));
                    break;

                default:
                    return 0;
            }

            // ─── 種族パッシブ補正 ─────────────
            float mul = 1f;

            // Skeleton: 魔法ダメージ被 -20%
            if (isMagical && HasRacePassive(GetUnitState(defender), RACE_SKELETON))
                mul *= 0.8f;

            // Goblin: 移動を伴うスキル (ActiveMove or moveAfterSkill) ダメ+10%
            bool moveAssociated = skill.category == SkillCategory.ActiveMove || skill.moveAfterSkill;
            if (moveAssociated && HasRacePassive(GetUnitState(attacker), RACE_GOBLIN))
                mul *= 1.10f;

            // Orc: 次攻撃ダメージブースト (1回だけ)
            var atkState = GetUnitState(attacker);
            if (atkState != null && atkState.OrcNextAttackBoostPercent > 0)
            {
                mul *= (1f + atkState.OrcNextAttackBoostPercent / 100f);
                atkState.OrcNextAttackBoostPercent = 0; // 消費
            }

            return Mathf.Max(1, Mathf.RoundToInt(raw * mul));
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
            int physicalSide = actorPos[0];
            var actorCell = new Vector2Int(actorPos[1], actorPos[2]);
            // 所属ベース (敵陣侵入中でも味方は味方、敵は敵)
            int mySide = Field.GetSide(actor);
            int enemySide = BattleField.OppositeSide(mySide);

            // スキルの射程フィルタ (targetRangeが空なら全域扱い)
            // targetRange 空 + SelfArea の場合は effectRange を射程判定に使う (SelfArea AoE対応)
            GridRangePattern EffectiveRange()
            {
                if (skill == null) return null;
                if (skill.targetRange != null && skill.targetRange.Offsets.Count > 0) return skill.targetRange;
                if (skill.targetingMode == SkillTargetingMode.SelfArea
                    && skill.effectRange != null && skill.effectRange.Offsets.Count > 0)
                    return skill.effectRange;
                return null;
            }
            bool HasRange() => EffectiveRange() != null;
            bool InSkillRange(SpaceJourneyUnit u) => !HasRange() || IsInRange(actorCell, physicalSide, u, EffectiveRange());

            switch (cond.kind)
            {
                // ── 移動方向指定 (設定値。条件としては常に true) ──
                case ActionConditionKind.MoveTo_NearestEnemy:
                case ActionConditionKind.MoveTo_NearestJobEnemy:
                case ActionConditionKind.MoveTo_IntruderEnemy:
                case ActionConditionKind.MoveTo_FarthestEnemy:
                case ActionConditionKind.MoveTo_EnemyTerritory:
                case ActionConditionKind.MoveTo_Retreat:
                    return true;

                case ActionConditionKind.Always:
                    return true;

                case ActionConditionKind.EnemyInRange:
                    return Field.GetAllAlive(enemySide).Any(InSkillRange);

                case ActionConditionKind.EnemyCountInRange:
                    return Field.GetAllAlive(enemySide).Count(InSkillRange) >= cond.intParam;

                case ActionConditionKind.EnemyHpBelowRate:
                    return Field.GetAllAlive(enemySide)
                        .Where(InSkillRange)
                        .Any(e => (float)e.CurrentHp / Mathf.Max(1, e.MaxHp) <= cond.rateParam);

                case ActionConditionKind.NoEnemyForward:
                    // 前列に敵がいない
                    return Field.GetFrontRowCells(enemySide).Count == 0;

                case ActionConditionKind.EnemyHasEmptyBehind:
                    // 射程内の敵のうち、ノックバック先が空いているものが1体以上
                    return Field.GetAllAlive(enemySide)
                        .Where(InSkillRange)
                        .Any(e => CalcKnockbackDest(actor, e, out _, out _));

                case ActionConditionKind.SelfInCastTargetArea:
                {
                    // 自分が敵の詠唱AoE予告マス内にいるか
                    foreach (var s in unitStates.Values)
                    {
                        if (s.Unit.IsDead || s.Casting == null) continue;
                        if (Field.GetSide(s.Unit) == mySide) continue; // 味方の詠唱は対象外
                        foreach (var (cellSide, cell) in s.Casting.TargetCells)
                        {
                            if (cellSide == actorPos[0] && cell.x == actorCell.x && cell.y == actorCell.y)
                                return true;
                        }
                    }
                    return false;
                }

                case ActionConditionKind.EnemyAdjacentCount:
                {
                    // 自分の隣接マス (マンハッタン1) にいる敵数が intParam 体以上か
                    int need = Mathf.Max(1, cond.intParam);
                    int found = 0;
                    foreach (var enemy in Field.GetAllAlive(enemySide))
                    {
                        var ePos = Field.FindUnit(enemy);
                        if (ePos.x < 0) continue;
                        if (ePos[0] != actorPos[0]) continue; // 同じ物理 side のみ隣接判定
                        int d = Mathf.Abs(ePos[1] - actorCell.x) + Mathf.Abs(ePos[2] - actorCell.y);
                        if (d == 1) { found++; if (found >= need) return true; }
                    }
                    return false;
                }

                case ActionConditionKind.AllyInRange:
                    return Field.GetAllAlive(mySide).Where(a => a != actor).Any(InSkillRange);

                case ActionConditionKind.AllyHpBelowRate:
                    return Field.GetAllAlive(mySide)
                        .Where(a => a != actor)
                        .Where(InSkillRange)
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
            int physicalSide = actorPos[0];
            int ownerSide = Field.GetSide(actor);
            var actorCell = new Vector2Int(actorPos[1], actorPos[2]);

            EffectTargetSide targetSide = skill.effectTargetSide;
            List<SpaceJourneyUnit> candidates;

            // 所属ベースで味方/敵を判定 (敵陣侵入中でも正しく動くように)
            if (targetSide == EffectTargetSide.Self)
                candidates = Field.GetAllAlive(ownerSide).Where(u => u != actor).ToList();
            else
                candidates = Field.GetAllAlive(BattleField.OppositeSide(ownerSide));

            if (candidates.Count == 0) return null;

            // GridRangePattern による射程フィルタ (4方向回転対応)
            // 射程判定は物理位置ベース (actor の現在いる物理side から見た距離)
            if (skill.targetRange != null && skill.targetRange.Offsets.Count > 0)
            {
                candidates = candidates.Where(c => IsInRange(actorCell, physicalSide, c, skill.targetRange, actor)).ToList();
                if (candidates.Count == 0) return null;
            }

            // 挑発(Taunt)優先: actor 自身が Taunt状態なら source (挑発者) を狙う
            if (skill.category == SkillCategory.ActiveAttack && targetSide != EffectTargetSide.Self)
            {
                var provoker = actor.GetActiveEffectSource(StatusEffectType.Taunt);
                if (provoker != null && candidates.Contains(provoker) && !provoker.IsDead)
                    return provoker;

                // 従来のマーキング: Taunt持ちが射程内にいれば優先
                var taunters = candidates.Where(c => c.HasActiveEffect(StatusEffectType.Taunt)).ToList();
                if (taunters.Count > 0)
                    return taunters.OrderBy(c => Field.Distance(actor, c)).First();
            }

            // 最も近い候補を選択
            return candidates.OrderBy(c => Field.Distance(actor, c)).First();
        }

        /// <summary>
        /// BattleField座標のオフセットを GridRangePattern座標に変換。
        /// BF: x=前後(0=前列), y=横方向
        /// Grid: x=横, y=前方(+Y)
        /// </summary>
        private static Vector2Int BFToGridOffset(Vector2Int bfOffset)
            => new Vector2Int(bfOffset.y, -bfOffset.x);

        /// <summary>
        /// GridRangePattern座標のオフセットを BattleField座標に変換。
        /// </summary>
        private static Vector2Int GridToBFOffset(Vector2Int gridOffset)
            => new Vector2Int(-gridOffset.y, gridOffset.x);

        /// <summary>対象が射程パターン内にいるか判定 (4方向回転対応)</summary>
        private bool IsInRange(Vector2Int actorCell, int actorSide, SpaceJourneyUnit target, GridRangePattern range)
            => IsInRange(actorCell, actorSide, target, range, null);

        private bool IsInRange(Vector2Int actorCell, int actorSide, SpaceJourneyUnit target, GridRangePattern range, SpaceJourneyUnit actor)
        {
            var bfOffset = CalcBFOffset(actorCell, actorSide, target);
            if (!bfOffset.HasValue) return false;
            var gridOffset = BFToGridOffset(bfOffset.Value);
            if (range.ContainsRotated(gridOffset)) return true;

            // RangeBoost: actor が保持していればパターン外周から Manhattan(boost) 以内を許容
            if (actor != null)
            {
                int boost = actor.GetActiveEffectValue(StatusEffectType.RangeBoost);
                if (boost > 0)
                {
                    for (int rot = 0; rot < 4; rot++)
                    {
                        foreach (var po in range.GetRotatedOffsets(rot))
                        {
                            int d = Mathf.Abs(po.x - gridOffset.x) + Mathf.Abs(po.y - gridOffset.y);
                            if (d <= boost) return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>対象への方向(回転値0~3)を返す。射程外なら-1。</summary>
        private int GetAttackRotation(Vector2Int actorCell, int actorSide, SpaceJourneyUnit target, GridRangePattern range)
        {
            var bfOffset = CalcBFOffset(actorCell, actorSide, target);
            if (!bfOffset.HasValue) return -1;
            var gridOffset = BFToGridOffset(bfOffset.Value);
            return range.ContainsAnyRotation(gridOffset);
        }

        /// <summary>actor から target への BattleField 座標オフセットを計算</summary>
        private Vector2Int? CalcBFOffset(Vector2Int actorCell, int actorSide, SpaceJourneyUnit target)
        {
            var targetPos = Field.FindUnit(target);
            if (targetPos.x < 0) return null;

            int targetSide = targetPos[0];
            var targetCell = new Vector2Int(targetPos[1], targetPos[2]);

            if (actorSide == targetSide)
                return targetCell - actorCell;

            // 異なる side: 自陣の前列(x=0) と 敵陣の前列(x=0) が隣接
            return new Vector2Int(-(targetCell.x + 1), targetCell.y - actorCell.y);
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
                if (eff.effectType == StatusEffectType.Custom)
                {
                    // Custom は customIntValue/rateParam/weaponEffect で個別処理が必要
                    Log.Add($"    [Custom効果] {skill.SkillName} (customIntValue={eff.customIntValue}) ※未実装");
                    continue;
                }

                if (UnityEngine.Random.value > eff.probability) continue;

                // ノックバックは即時処理 (持続なし)
                if (eff.effectType == StatusEffectType.Knockback)
                {
                    TryKnockback(actor, target);
                    continue;
                }

                // 士気回復は戦闘後にまとめて計算されるため戦闘中は何もしない
                if (eff.effectType == StatusEffectType.HealMorale)
                {
                    Log.Add($"    [士気効果] {skill.SkillName} 戦闘後計算のためスキップ");
                    continue;
                }

                // Robot: 状態異常付与を3回まで無効化 (戦闘ごと)
                if (IsAbnormalStatusEffect(eff.effectType))
                {
                    var ts = GetUnitState(target);
                    if (ts != null && HasRacePassive(ts, RACE_ROBOT) && ts.RobotImmuneRemaining > 0)
                    {
                        ts.RobotImmuneRemaining--;
                        Log.Add($"    [種族] {GetLabel(target)} 「機械体」状態異常({eff.effectType})を無効化 (残り{ts.RobotImmuneRemaining}/3)");
                        continue;
                    }
                }

                int duration = eff.duration > 0 ? eff.duration : CalcEffectiveCost(skill.baseCost, actor.AgiFinal);
                // Taunt は source (挑発者) 情報を保持
                SpaceJourneyUnit src = eff.effectType == StatusEffectType.Taunt ? actor : null;
                target.ApplyStatusEffect(eff.effectType, eff.value, duration, t, src);
            }
        }

        /// <summary>「状態異常」に該当するタイプか (Robot の無効化対象)</summary>
        private static bool IsAbnormalStatusEffect(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                case StatusEffectType.Freeze:
                case StatusEffectType.Burn:
                case StatusEffectType.DebuffAt:
                case StatusEffectType.DebuffDf:
                case StatusEffectType.DebuffAgi:
                case StatusEffectType.DebuffMat:
                case StatusEffectType.DebuffMdf:
                case StatusEffectType.ChainDamage:
                case StatusEffectType.Taunt:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 攻撃者 → 対象 方向に target を1マス進める。
        /// 押し出し先がマス外/占有済み/方向算出不能のいずれかなら何もしない。
        /// (呼び出し側で条件判定していなくても、ここで保証される。ダメージ処理はこの関数より前で完了している前提)
        /// </summary>
        private void TryKnockback(SpaceJourneyUnit attacker, SpaceJourneyUnit target)
        {
            if (!CalcKnockbackDest(attacker, target, out var destSide, out var destCell)) return;

            var tgtPos = Field.FindUnit(target);
            var tgtCell = new Vector2Int(tgtPos[1], tgtPos[2]);

            if (Field.MoveUnit(target, destCell))
                Log.Add($"    [{GetLabel(target)}] ノックバック ({tgtCell.x},{tgtCell.y})→({destCell.x},{destCell.y})");
        }

        /// <summary>
        /// ノックバックの押し出し先を計算。攻撃者 → 対象 の方向に1マス進める。
        /// 以下のいずれかに該当する場合 false (= ノックバック不発):
        ///   - 攻撃者または対象が見つからない
        ///   - 方向ベクトルがゼロ (攻撃者と対象が同じマス)
        ///   - 押し出し先がフィールド外 (マスなし)
        ///   - 押し出し先に別ユニットがいる
        /// </summary>
        private bool CalcKnockbackDest(SpaceJourneyUnit attacker, SpaceJourneyUnit target,
            out int destSide, out Vector2Int destCell)
        {
            destSide = -1;
            destCell = default;

            var atkPos = Field.FindUnit(attacker);
            var tgtPos = Field.FindUnit(target);
            if (atkPos.x < 0 || tgtPos.x < 0) return false;

            int atkSide = atkPos[0];
            var atkCell = new Vector2Int(atkPos[1], atkPos[2]);
            int tgtSide = tgtPos[0];
            var tgtCell = new Vector2Int(tgtPos[1], tgtPos[2]);

            Vector2Int dir;
            if (atkSide == tgtSide)
            {
                // 同side: 攻撃者 → 対象 の方向に押す
                var bfOffset = tgtCell - atkCell;
                dir = new Vector2Int(
                    bfOffset.x == 0 ? 0 : (Mathf.Abs(bfOffset.x) >= Mathf.Abs(bfOffset.y) ? (int)Mathf.Sign(bfOffset.x) : 0),
                    bfOffset.y == 0 ? 0 : (Mathf.Abs(bfOffset.y) > Mathf.Abs(bfOffset.x) ? (int)Mathf.Sign(bfOffset.y) : 0));
            }
            else
            {
                // cross-side: 対象は自sideの後退方向 (x+1) に押す
                dir = new Vector2Int(1, 0);
            }
            if (dir == Vector2Int.zero) return false;

            destSide = tgtSide;
            destCell = tgtCell + dir;

            // マス外・占有は不発
            if (!Field.IsValidCell(destCell)) return false;
            if (!Field.IsCellEmpty(destSide, destCell)) return false;

            return true;
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
                ApplyPassiveEffect(state, passive, otherUnit);
            }
        }

        private SkillTriggerContext BuildTriggerContext(UnitBattleState state, int timing,
            SpaceJourneyUnit triggerUnit, SpaceJourneyUnit otherUnit, SkillDefinition triggerSkill)
        {
            var unit = state.Unit;
            // 所属ベース (敵陣侵入中でも正しく判定)
            int mySide = Field.GetSide(unit);

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

        private void ApplyPassiveEffect(UnitBattleState state, SkillDefinition passive, SpaceJourneyUnit otherUnit)
        {
            var unit = state.Unit;

            // パッシブの追加効果を適用先に応じて適用
            if (passive.AdditionalEffects != null)
            {
                foreach (var eff in passive.AdditionalEffects)
                {
                    if (eff.effectType == StatusEffectType.None) continue;
                    if (eff.effectType == StatusEffectType.Custom) continue; // 個別処理が必要
                    if (UnityEngine.Random.value > eff.probability) continue;

                    // ターゲット決定 (Self=自分, Enemy=トリガー相手, Both=両方, None=自分)
                    var targets = new List<SpaceJourneyUnit>();
                    switch (passive.effectTargetSide)
                    {
                        case EffectTargetSide.Self:
                        case EffectTargetSide.None:
                            targets.Add(unit);
                            break;
                        case EffectTargetSide.Enemy:
                            if (otherUnit != null && !otherUnit.IsDead) targets.Add(otherUnit);
                            break;
                        case EffectTargetSide.Both:
                            targets.Add(unit);
                            if (otherUnit != null && !otherUnit.IsDead) targets.Add(otherUnit);
                            break;
                    }

                    foreach (var tgt in targets)
                    {
                        // ノックバックは即時処理
                        if (eff.effectType == StatusEffectType.Knockback)
                        {
                            if (otherUnit != null) TryKnockback(unit, tgt);
                            continue;
                        }

                        int duration = eff.duration > 0 ? eff.duration : 2;
                        // Taunt は source (発動者=unit) 情報を保持 (武器パッシブ等)
                        SpaceJourneyUnit pSrc = eff.effectType == StatusEffectType.Taunt ? unit : null;
                        tgt.ApplyStatusEffect(eff.effectType, eff.value, duration, CurrentTime, pSrc);
                    }
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
            int ownerSide = Field.GetSide(unit);
            string side = ownerSide == 0 ? "味" : "敵";
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
