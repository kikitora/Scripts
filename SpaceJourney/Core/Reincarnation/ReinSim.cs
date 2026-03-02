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
        public readonly int[] MainStats = new int[5]; // 主魂のソウルステ
        public readonly int[][] HalfStats = new int[3][]; // 守護霊[0〜2]のソウルステ（nullの枠は不在）

        // 転生後のベース情報
        public readonly int Rank;
        public readonly GrowthType GrowthType;
        public readonly SoulJobDefinition JobDef;
        public readonly TalentRank Talent;

        /// <summary>
        /// これまでの転生回数。転生回数ボーナスに使う。
        /// </summary>
        public readonly int ReinCount;

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
                talent,
                main.ReinSouls.Count);
            return input;
        }

        private ReinSimInput(
            int[] mainStats,
            int[][] halfStats,
            int rank,
            GrowthType growthType,
            SoulJobDefinition jobDef,
            TalentRank talent,
            int reinCount)
        {
            MainStats = mainStats;
            HalfStats = halfStats;
            Rank = rank;
            GrowthType = growthType;
            JobDef = jobDef;
            Talent = talent;
            ReinCount = reinCount;
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
        private const int AgeStatusMaxAge = 30;

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
        // eventFactors：ポイント積み上げ式（int）
        // 変換：1.0 + points / 40.0 * 0.8 → 上限40pt=1.8x
        public readonly int[] EventFactors = new int[5];

        // --------------------------------------------------------
        // フラグ管理（SO参照で管理するため文字列IDは不要）
        // --------------------------------------------------------
        /// <summary>発生済みイベントSO（requires判定用）</summary>
        public readonly HashSet<string> OccurredEvents = new(); // eventId で管理

        /// <summary>再発不可になったイベントSO（1回起きたら終わり）</summary>
        public readonly HashSet<ReinLifeEventSO> EndedEvents = new();

        /// <summary>取得済みライフタグ（婚活経験あり・結婚済みなど）</summary>
        public readonly HashSet<string> AcquiredLifeTags = new();

        // --------------------------------------------------------
        // 転生内のランク・ジョブ
        // --------------------------------------------------------
        /// <summary>
        /// 転生内の現在ランク。
        /// 初期値：max(1, input.Rank - 1)（前の転生ランクより1下からスタート）
        /// </summary>
        public int CurrentRank { get; set; }

        /// <summary>
        /// この値未満のランクには毎年自動でステップアップする（保証ライン）。
        /// = input.Rank - 1（主の前転生ランクと同じランクまでは保証）
        /// </summary>
        public int GuaranteedRankUpTo { get; private set; }

        public SoulJobDefinition CurrentJob { get; private set; }

        /// <summary>
        /// このソウルの運命のジョブ（転生開始時に確定、シミュ全期間で固定）。
        /// イベントの職業タグチェックに使う。
        /// </summary>
        public SoulJobDefinition DestinyJob { get; private set; }

        /// <summary>転生回数（SpaceJourneyCoreTypesの定数でボーナス計算に使う）</summary>
        public int ReinCount { get; private set; }

        // --------------------------------------------------------
        // 結果の蓄積
        // --------------------------------------------------------
        public readonly List<string> LearnedSkillIds = new();
        public readonly List<ReinEvent> HistoryEvents = new();

        // --------------------------------------------------------
        // ランクアップ条件（ジョブごとの各ステ閾値 × rank）
        // JrListのRankStatusに相当。jobDefから生成する。
        // --------------------------------------------------------
        private int[] _rankRequirements = new int[5];

        // --------------------------------------------------------
        // EventFactor上限
        // --------------------------------------------------------
        private const int EventFactorPtMax = 40; // 40pt = 1.8x

        // ============================================================
        // 初期化
        // ============================================================
        public ReinSimContext(ReinSimInput input)
        {
            CurrentJob = input.JobDef;
            DestinyJob = input.JobDef;
            ReinCount = input.ReinCount;

            // workingRankは前の転生ランク-1からスタート（最低1）
            CurrentRank = Mathf.Max(1, input.Rank - 1);
            // 主のランク-1まで保証ステップアップ（同ランクまでは確実に上がる）
            GuaranteedRankUpTo = Mathf.Max(1, input.Rank - 1);

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
                EventFactors[i] = 0; // 0pt = 1.0x（変換はReinSimResultで行う）

            // --------------------------------------------------
            // NowStatsを0歳で初期化
            // --------------------------------------------------
            UpdateNowStats(0);
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
        // ランクアップ
        // ============================================================
        public void DoRankUp()
        {
            CurrentRank = Mathf.Min(CurrentRank + 1, SpaceJourneyConstants.MaxSoulJobRank);
        }

        // ============================================================
        // EventFactorの加算（上下限クランプあり）
        // ============================================================
        /// <summary>イベントポイントを加算（0〜EventFactorPtMax でクランプ）</summary>
        public void AddEventFactor(int statIdx, int points)
        {
            if (statIdx < 0 || statIdx >= 5) return;
            EventFactors[statIdx] = Mathf.Clamp(
                EventFactors[statIdx] + points,
                0,
                EventFactorPtMax);
        }

        /// <summary>ポイントを倍率に変換（外部参照用）。0pt=1.0x、40pt=1.8x</summary>
        public static float PtToMultiplier(int pt)
            => 1.0f + pt / 40f * 0.8f;

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
    } // end ReinSimContext

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
        /// <summary>変換済みEventFactor倍率（1.0〜1.8x）。CalcPotentialStatに渡す。</summary>
        public readonly float[] EventFactors;

        /// <summary>転生内で到達したランク</summary>
        public readonly int FinalRank;

        /// <summary>習得したスキルのIDリスト</summary>
        public readonly List<string> LearnedSkillIds;

        /// <summary>転生の来歴（UI表示・SoulInstanceに保存）</summary>
        public readonly List<ReinEvent> HistoryEvents;

        public ReinSimResult(ReinSimContext ctx)
        {
            // int pt → float 倍率に変換してから格納
            EventFactors = new float[5];
            for (int i = 0; i < 5; i++)
                EventFactors[i] = ReinSimContext.PtToMultiplier(ctx.EventFactors[i]);
            FinalRank = ctx.CurrentRank;
            LearnedSkillIds = new List<string>(ctx.LearnedSkillIds);
            HistoryEvents = new List<ReinEvent>(ctx.HistoryEvents);
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
        private const int MaxAge = 101; // 0〜101歳

        // ランクアップ閾値計算に使うバランス定数（RankUpEvents.json生成時と同値）
        private const float RankBaseStatPerRank = 0.4f;  // SoulJobRankExpPerRank相当
        private const float GrowthNormal = 6.25f;
        private const float GrowthPower = 1.0f;
        private const int StatMaxLevel = 25;
        private static readonly int[] NormalLevels = { 4, 6, 7, 9, 11, 13, 15, 17, 19, 21 };
        private static readonly float[] RankBaseStats =
            { 45,55,65,75,85,95,105,115,125,135 };
        private static readonly float TalentMidMul = (1.10f + 1.24f) / 2f; // C人材中間値

        /// <summary>
        /// ランクアップイベントのrequireStatsを動的に計算する。
        /// SOには持たせず、ジョブ定義から毎回算出する。
        /// </summary>
        private static bool MeetsRankUpStatRequirements(
            ReinSimContext ctx, int rankIndex)
        {
            var job = ctx.DestinyJob;
            if (job == null) return true;

            float[] muls = job.GetStatMultipliers(); // AT,DF,AGI,MAT,MDF

            // 難易度設定
            int topN;
            int targetLv;
            switch (job.RankUpDifficulty)
            {
                case RankUpDifficulty.Easy:
                    topN = 2;
                    targetLv = NormalLevels[rankIndex];
                    break;
                case RankUpDifficulty.Hard:
                    topN = 4;
                    targetLv = NormalLevels[rankIndex] + 2;
                    break;
                default: // Medium
                    topN = 4;
                    targetLv = NormalLevels[rankIndex];
                    break;
            }

            // stat倍率の高い上位N個を対象に
            int[] order = { 0, 1, 2, 3, 4 };
            System.Array.Sort(order, (a, b) => muls[b].CompareTo(muls[a]));

            // 成長係数
            float s = (targetLv - 1f) / (StatMaxLevel - 1f);
            float growthFactor = 1f + (GrowthNormal - 1f) * Mathf.Pow(s, GrowthPower);

            for (int i = 0; i < topN; i++)
            {
                int si = order[i];
                float lv1Stat = RankBaseStats[rankIndex] * muls[si] * TalentMidMul * 0.1f;
                int threshold = Mathf.RoundToInt(lv1Stat * growthFactor);
                if (ctx.NowStats[si] < threshold) return false;
            }
            return true;
        }

        public static ReinSimResult Run(
            ReinSimInput input,
            IReadOnlyList<ReinLifeEventSO> allEvents)
        {
            var ctx = new ReinSimContext(input);

            for (int age = 0; age <= MaxAge; age++)
            {
                // 1) 転生内ステータスを年齢に応じて更新
                ctx.UpdateNowStats(age);

                // 2) 保証ステップアップ：currentRank < guaranteedRankUpTo なら毎年+1
                if (ctx.CurrentRank < ctx.GuaranteedRankUpTo)
                {
                    ctx.DoRankUp();
                    ctx.HistoryEvents.Add(new ReinEvent(
                        age,
                        $"ランク{ctx.CurrentRank}に到達した。（保証）",
                        ReinEventType.RankUp));
                }

                // 3) 各イベントSO処理（ランクアップイベントもここで処理）
                foreach (var ev in allEvents)
                    TryOccurEvent(ctx, ev, age);
            }

            return new ReinSimResult(ctx);
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

            // 職業タグチェック：タグあり＋不一致なら発生しない
            if (!ev.MatchesJob(ctx.DestinyJob)) return;

            // ランク・ステータス前提条件チェック
            if (!ev.MeetsPrerequisites(ctx.CurrentRank, ctx.NowStats, ctx.AcquiredLifeTags)) return;

            // ランクアップイベントの場合：動的閾値チェック
            // SOにrequireStatsAndを持たせず、ジョブ定義から毎回計算
            if (ev.Options != null && ev.Options.Count > 0 && ev.Options[0].IsRankUp)
            {
                // ランクアップイベントはrelatedJobが一致している必要がある
                if (ev.RelatedJobs != null && ev.RelatedJobs.Count > 0)
                {
                    bool jobMatch = false;
                    foreach (var j in ev.RelatedJobs)
                        if (j == ctx.DestinyJob) { jobMatch = true; break; }
                    if (!jobMatch) return;
                }

                // 現在ランク（0始まり）で閾値計算
                int rankIdx = Mathf.Clamp(ctx.CurrentRank - 1, 0, 8);
                if (!MeetsRankUpStatRequirements(ctx, rankIdx)) return;
            }

            // requires：前提イベントが全部発生済みでないとダメ
            foreach (var reqId in ev.RequiresEventIds)
            {
                if (!string.IsNullOrEmpty(reqId) && !ctx.OccurredEvents.Contains(reqId))
                    return;
            }

            // blockedBy：排他イベントが1つでも発生済みならダメ
            foreach (var blkId in ev.BlockedByEventIds)
            {
                if (!string.IsNullOrEmpty(blkId) && ctx.OccurredEvents.Contains(blkId))
                    return;
            }

            // 職業一致ボーナスを加算した出現重みで判定
            float weight = ev.BaseWeight;
            if (ev.HasJobTag)
                weight += ev.JobMatchBonus;

            if (UnityEngine.Random.value >= weight) return;

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
                foreach (var skill in option.RankUpSkills)
                    if (skill != null) ctx.LearnSkill(skill);

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
            foreach (var skill in option.GrantSkills)
                if (skill != null) ctx.LearnSkill(skill);

            // ライフタグ付与（イベントSO側）
            foreach (var tag in ev.GrantsLifeTags)
                ctx.AcquiredLifeTags.Add(tag);

            // ライフタグ付与（選択肢側）
            foreach (var tag in option.GrantsLifeTags)
                ctx.AcquiredLifeTags.Add(tag);

            // 発生済みフラグ登録
            ctx.OccurredEvents.Add(ev.EventId);

            // 再発不可登録
            ctx.EndedEvents.Add(ev);
        }
    }
}