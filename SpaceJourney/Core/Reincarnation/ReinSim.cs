using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    // ================================================================
    // ReinSimInput：シミュ開始時の入力データ
    // ================================================================
    /// <summary>
    /// 転生シミュレーションへの入力。
    /// 主魂1体＋守護霊最大3体を受け取る。
    /// </summary>
    public class ReinSimInput
    {
        // 主魂・守護霊のステータス（AT=0,DF=1,AGI=2,MAT=3,MDF=4）
        public readonly int[] MainStats   = new int[5]; // 主魂のソウルステ
        public readonly int[][] HalfStats = new int[3][]; // 守護霊[0〜2]のソウルステ（nullの枠は不在）

        // 転生後のベース情報
        public readonly int   Rank;
        public readonly GrowthType GrowthType;
        public readonly SoulJobDefinition JobDef;
        public readonly TalentRank Talent;

        /// <summary>
        /// SoulInstanceから直接組み立てるファクトリ。
        /// guardians は null や3体未満でも可（不在分は無視）。
        /// </summary>
        public static ReinSimInput Build(
            SoulInstance main,
            SoulInstance[] guardians,
            SoulJobDefinition jobDef,
            TalentRank talent)
        {
            if (main == null) throw new ArgumentNullException(nameof(main));

            var input = new ReinSimInput(
                GetStats(main),
                BuildGuardianStats(guardians),
                main.Rank,
                main.GrowthType,
                jobDef,
                talent);
            return input;
        }

        private ReinSimInput(
            int[] mainStats,
            int[][] halfStats,
            int rank,
            GrowthType growthType,
            SoulJobDefinition jobDef,
            TalentRank talent)
        {
            MainStats  = mainStats;
            HalfStats  = halfStats;
            Rank       = rank;
            GrowthType = growthType;
            JobDef     = jobDef;
            Talent     = talent;
        }

        private static int[] GetStats(SoulInstance soul)
        {
            return new int[5]
            {
                soul.GetSoulStat(StatKind.AT),
                soul.GetSoulStat(StatKind.DF),
                soul.GetSoulStat(StatKind.AGI),
                soul.GetSoulStat(StatKind.MAT),
                soul.GetSoulStat(StatKind.MDF),
            };
        }

        private static int[][] BuildGuardianStats(SoulInstance[] guardians)
        {
            var result = new int[3][];
            if (guardians == null) return result;

            for (int i = 0; i < 3 && i < guardians.Length; i++)
            {
                if (guardians[i] != null)
                    result[i] = GetStats(guardians[i]);
            }
            return result;
        }
    }

    // ================================================================
    // ReinSimContext：シミュレーション実行中の状態
    // ================================================================
    /// <summary>
    /// 転生シミュレーション実行中の一時状態。
    /// ゲームロジックからは直接触らず、ReinSimulatorだけが操作する。
    /// </summary>
    public class ReinSimContext
    {
        // --------------------------------------------------------
        // 定数
        // --------------------------------------------------------
        /// <summary>0歳時の転生内ステータス割合（20%スタート）</summary>
        private const float AgeStatusStartRate = 0.20f;
        /// <summary>この年齢でmaxに達する</summary>
        private const int   AgeStatusMaxAge    = 30;

        // --------------------------------------------------------
        // 主40%・守護霊各20%の合成最大値（シミュ全期間で固定）
        // --------------------------------------------------------
        public readonly int[] MaxStats = new int[5];

        // --------------------------------------------------------
        // 現在の転生内ステータス（年齢で変化）
        // AT=0, DF=1, AGI=2, MAT=3, MDF=4
        // --------------------------------------------------------
        public readonly int[] NowStats = new int[5];

        // --------------------------------------------------------
        // eventFactors（最終ソウルステに影響）
        // イベント選択肢のstatEffectsで加算される。初期値1.0f。
        // --------------------------------------------------------
        public readonly float[] EventFactors = new float[5];

        // --------------------------------------------------------
        // フラグ管理（SO参照で管理するため文字列IDは不要）
        // --------------------------------------------------------
        /// <summary>発生済みイベントSO（requires判定用）</summary>
        public readonly HashSet<ReinLifeEventSO> OccurredEvents = new();

        /// <summary>再発不可になったイベントSO（1回起きたら終わり）</summary>
        public readonly HashSet<ReinLifeEventSO> EndedEvents = new();

        // --------------------------------------------------------
        // 転生内のランク・ジョブ
        // --------------------------------------------------------
        public int CurrentRank { get; set; } = 1;
        public SoulJobDefinition CurrentJob { get; private set; }

        // --------------------------------------------------------
        // 結果の蓄積
        // --------------------------------------------------------
        public readonly List<string>   LearnedSkillIds = new();
        public readonly List<ReinEvent> HistoryEvents  = new();

        // --------------------------------------------------------
        // ランクアップ条件（ジョブごとの各ステ閾値 × rank）
        // JrListのRankStatusに相当。jobDefから生成する。
        // --------------------------------------------------------
        private int[] _rankRequirements = new int[5];

        // --------------------------------------------------------
        // EventFactor上限
        // --------------------------------------------------------
        private const float EventFactorMax = 1.5f;
        private const float EventFactorMin = 0.8f;

        // ============================================================
        // 初期化
        // ============================================================
        public ReinSimContext(ReinSimInput input)
        {
            CurrentJob = input.JobDef;

            // --------------------------------------------------
            // MaxStats計算：主×40% + 守護霊×20% ずつ
            // --------------------------------------------------
            for (int i = 0; i < 5; i++)
            {
                float v = input.MainStats[i] * 0.40f;
                for (int g = 0; g < 3; g++)
                {
                    if (input.HalfStats[g] != null)
                        v += input.HalfStats[g][i] * 0.20f;
                }
                MaxStats[i] = Mathf.RoundToInt(v);
            }

            // --------------------------------------------------
            // EventFactors初期値：1.0f
            // --------------------------------------------------
            for (int i = 0; i < 5; i++)
                EventFactors[i] = 1.0f;

            // --------------------------------------------------
            // NowStatsを0歳で初期化
            // --------------------------------------------------
            UpdateNowStats(0);

            // --------------------------------------------------
            // ランクアップ要件をジョブ定義から生成
            // --------------------------------------------------
            BuildRankRequirements();
        }

        // ============================================================
        // 年齢に応じたステータス更新
        // 0歳→20%スタート、30歳→100%到達、以降維持
        // nowStat = (8/3 × age + 20) / 100 × maxStat
        // ============================================================
        public void UpdateNowStats(int age)
        {
            float rate;
            if (age >= AgeStatusMaxAge)
            {
                rate = 1.0f;
            }
            else
            {
                rate = (8f / 3f * age + 20f) / 100f;
            }

            for (int i = 0; i < 5; i++)
                NowStats[i] = Mathf.RoundToInt(MaxStats[i] * rate);
        }

        // ============================================================
        // ランクアップ判定
        // 全ステがjob閾値×(rank+1)を超えていればtrue
        // ============================================================
        public bool CanRankUp()
        {
            if (CurrentRank >= 8) return false; // 旧コードのランク上限

            int nextRank = CurrentRank + 1;
            for (int i = 0; i < 5; i++)
            {
                if (NowStats[i] < _rankRequirements[i] * nextRank)
                    return false;
            }
            return true;
        }

        public void DoRankUp()
        {
            CurrentRank++;
        }

        // ============================================================
        // EventFactorの加算（上下限クランプあり）
        // ============================================================
        public void AddEventFactor(int statIdx, float delta)
        {
            if (statIdx < 0 || statIdx >= 5) return;
            EventFactors[statIdx] = Mathf.Clamp(
                EventFactors[statIdx] + delta,
                EventFactorMin,
                EventFactorMax);
        }

        // ============================================================
        // スキル習得（重複なし）
        // ============================================================
        public void LearnSkill(SkillDefinition skill)
        {
            if (skill == null) return;
            string id = skill.SkillId;
            if (!string.IsNullOrEmpty(id) && !LearnedSkillIds.Contains(id))
                LearnedSkillIds.Add(id);
        }

        // ============================================================
        // 内部：ランクアップ要件テーブル構築
        // jobDefのGetMultiplier × FIRST_STATUSで基本閾値を作る
        // ============================================================
        private const int RANK_BASE = 20; // 旧コードのRankStatusの最小値に相当

        private void BuildRankRequirements()
        {
            // 旧コードではjobごとに int[5]{AT閾値, MAT閾値, DF閾値, MDF閾値, AGI閾値} を持っていた
            // 新設計ではSoulJobDefinition.GetMultiplier(kind)×RANK_BASEで近似する
            if (CurrentJob == null)
            {
                for (int i = 0; i < 5; i++) _rankRequirements[i] = RANK_BASE;
                return;
            }

            StatKind[] order = { StatKind.AT, StatKind.DF, StatKind.AGI, StatKind.MAT, StatKind.MDF };
            for (int i = 0; i < 5; i++)
            {
                float mul = CurrentJob.GetMultiplier(order[i]);
                _rankRequirements[i] = Mathf.RoundToInt(RANK_BASE * mul);
            }
        }
    }

    // ================================================================
    // ReinSimResult：シミュレーション結果
    // ================================================================
    /// <summary>
    /// 転生シミュレーションの出力。
    /// OneReinSoulData.CreateFromArgs に渡す形に整形されている。
    /// </summary>
    public class ReinSimResult
    {
        /// <summary>
        /// 各ステのeventFactor（AT=0,DF=1,AGI=2,MAT=3,MDF=4）。
        /// CalcPotentialStat に渡してLv1ステを決める。
        /// </summary>
        public readonly float[] EventFactors;

        /// <summary>転生内で到達したランク</summary>
        public readonly int FinalRank;

        /// <summary>習得したスキルのIDリスト</summary>
        public readonly List<string> LearnedSkillIds;

        /// <summary>転生の来歴（UI表示・SoulInstanceに保存）</summary>
        public readonly List<ReinEvent> HistoryEvents;

        public ReinSimResult(ReinSimContext ctx)
        {
            EventFactors    = (float[])ctx.EventFactors.Clone();
            FinalRank       = ctx.CurrentRank;
            LearnedSkillIds = new List<string>(ctx.LearnedSkillIds);
            HistoryEvents   = new List<ReinEvent>(ctx.HistoryEvents);
        }
    }

    // ================================================================
    // ReinSimulator：シミュレーション本体
    // ================================================================
    /// <summary>
    /// 転生シミュレーションのロジック本体。
    /// MonoBehaviourに依存しない純粋C#クラス。
    ///
    /// 使い方：
    ///   var input = ReinSimInput.Build(mainSoul, guardians, jobDef, talent);
    ///   var result = ReinSimulator.Run(input, allEvents);
    ///   // result.EventFactors を OneReinSoulData.CreateFromArgs に渡す
    /// </summary>
    public static class ReinSimulator
    {
        private const int MaxAge    = 101; // 0〜101歳
        private const int RankUpMaxAge = 44; // 旧コード：age < 45でのみランクアップ判定

        /// <summary>
        /// シミュレーションを実行し結果を返す。
        /// </summary>
        /// <param name="input">主魂・守護霊のステータス等の入力</param>
        /// <param name="allEvents">使用するイベントSOの全リスト</param>
        public static ReinSimResult Run(
            ReinSimInput input,
            IReadOnlyList<ReinLifeEventSO> allEvents)
        {
            var ctx = new ReinSimContext(input);

            for (int age = 0; age <= MaxAge; age++)
            {
                // 1) 転生内ステータスを年齢に応じて更新
                ctx.UpdateNowStats(age);

                // 2) ランクアップ判定（45歳未満のみ）
                if (age < RankUpMaxAge)
                    TryRankUp(ctx, age);

                // 3) 各イベントSO処理
                foreach (var ev in allEvents)
                    TryOccurEvent(ctx, ev, age);
            }

            return new ReinSimResult(ctx);
        }

        // ============================================================
        // ランクアップ処理
        // ============================================================
        private static void TryRankUp(ReinSimContext ctx, int age)
        {
            if (!ctx.CanRankUp()) return;

            // ランクアップ成否の確率（旧コードのRankUpTest参照）
            float rate;
            // 旧コードでは条件を全部満たしている場合のみrateが設定された
            // ここでは簡略化：条件を満たしていれば一定確率でランクアップ
            rate = Mathf.Clamp01(0.3f + ctx.CurrentRank * 0.05f);

            if (UnityEngine.Random.value > rate) return;

            ctx.DoRankUp();

            // 来歴に記録
            ctx.HistoryEvents.Add(new ReinEvent(
                age,
                $"ランク{ctx.CurrentRank}に到達した。",
                ReinEventType.RankUp));
        }

        // ============================================================
        // イベント発生処理
        // ============================================================
        private static void TryOccurEvent(
            ReinSimContext ctx,
            ReinLifeEventSO ev,
            int age)
        {
            // 年齢レンジ外
            if (age < ev.StartAge || age > ev.EndAge) return;

            // 再発不可
            if (ctx.EndedEvents.Contains(ev)) return;

            // requires：前提イベントが全部発生済みでないとダメ
            foreach (var req in ev.RequiresEvents)
            {
                if (req != null && !ctx.OccurredEvents.Contains(req))
                    return;
            }

            // blockedBy：排他イベントが1つでも発生済みならダメ
            foreach (var blk in ev.BlockedByEvents)
            {
                if (blk != null && ctx.OccurredEvents.Contains(blk))
                    return;
            }

            // 出現確率判定（baseWeightを確率として扱う。0〜1推奨）
            if (UnityEngine.Random.value >= ev.BaseWeight) return;

            // 選択肢を重み付き抽選
            var option = ChooseOption(ctx, ev);
            if (option == null) return;

            // 結果を適用
            ApplyOption(ctx, ev, option, age);
        }

        // ============================================================
        // 選択肢の重み付き抽選
        // ============================================================
        private static ReinSentenceOption ChooseOption(
            ReinSimContext ctx,
            ReinLifeEventSO ev)
        {
            if (ev.Options == null || ev.Options.Count == 0) return null;

            // 各選択肢の最終重みを計算（nowStatsをstatConditionsに渡す）
            float[] weights = new float[ev.Options.Count];
            float total = 0f;

            for (int i = 0; i < ev.Options.Count; i++)
            {
                // statConditionsのチェックにnowStatsを渡す
                float w = ev.Options[i].CalcFinalWeight(ctx.NowStats);
                weights[i] = Mathf.Max(0f, w);
                total += weights[i];
            }

            if (total <= 0f) return null;

            float rnd = UnityEngine.Random.value * total;
            for (int i = 0; i < ev.Options.Count; i++)
            {
                rnd -= weights[i];
                if (rnd <= 0f)
                    return ev.Options[i];
            }

            return ev.Options[ev.Options.Count - 1];
        }

        // ============================================================
        // 選択肢の結果を適用
        // ============================================================
        private static void ApplyOption(
            ReinSimContext ctx,
            ReinLifeEventSO ev,
            ReinSentenceOption option,
            int age)
        {
            // eventFactors への加算
            if (option.StatEffects != null)
            {
                foreach (var effect in option.StatEffects)
                {
                    ctx.AddEventFactor(effect.StatIndex, effect.EventFactorDelta);
                }
            }

            // ランクアップ処理
            if (option.IsRankUp)
            {
                ctx.DoRankUp();
                if (option.RankUpSkill != null)
                    ctx.LearnSkill(option.RankUpSkill);

                ctx.HistoryEvents.Add(new ReinEvent(
                    age,
                    $"{option.Sentence}（ランク{ctx.CurrentRank}）",
                    ReinEventType.RankUp));
            }
            else
            {
                ctx.HistoryEvents.Add(new ReinEvent(
                    age,
                    option.Sentence,
                    ReinEventType.Happy)); // TODO：DecorationTypeからReinEventTypeへの変換
            }

            // スキル習得（ランクアップ以外）
            if (option.GrantSkill != null)
                ctx.LearnSkill(option.GrantSkill);

            // 発生済みフラグ登録
            ctx.OccurredEvents.Add(ev);

            // 再発不可登録
            ctx.EndedEvents.Add(ev);
        }
    }
}
