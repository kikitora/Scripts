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
        public readonly SoulJobDefinition JobDef; // null = 人生イベントから職業を自然決定
        public readonly TalentRank Talent;

        /// <summary>
        /// job_*タグ → SoulJobDefinition のマッピング。
        /// 人生イベントから職業を自然決定するために使う。
        /// </summary>
        public readonly IReadOnlyDictionary<string, SoulJobDefinition> JobTagToJobDef;

        /// <summary>
        /// これまでの転生回数。転生回数ボーナスに使う。
        /// </summary>
        public readonly int ReinCount;

        /// <summary>
        /// SoulInstanceから直接組み立てるファクトリ。
        /// guardians は null や3体未満でも可（不在分は無視）。
        /// jobDef は null で可（人生イベントから職業を自然決定する新方式）。
        /// </summary>
        public static ReinSimInput Build(
            SoulInstance main,
            SoulInstance[] guardians,
            SoulJobDefinition jobDef,
            TalentRank talent,
            IReadOnlyDictionary<string, SoulJobDefinition> jobTagToJobDef = null)
        {
            if (main == null) throw new ArgumentNullException(nameof(main));

            var input = new ReinSimInput(
                GetStats(main),
                BuildGuardianStats(guardians),
                main.Rank,
                main.GrowthType,
                jobDef,
                talent,
                main.ReinSouls.Count,
                jobTagToJobDef);
            return input;
        }

        private ReinSimInput(
            int[] mainStats,
            int[][] halfStats,
            int rank,
            GrowthType growthType,
            SoulJobDefinition jobDef,
            TalentRank talent,
            int reinCount,
            IReadOnlyDictionary<string, SoulJobDefinition> jobTagToJobDef = null)
        {
            MainStats = mainStats;
            HalfStats = halfStats;
            Rank = rank;
            GrowthType = growthType;
            JobDef = jobDef;
            Talent = talent;
            ReinCount = reinCount;
            JobTagToJobDef = jobTagToJobDef;
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
        /// <summary>発生済みイベントSO（requires判定用）key=eventId, value=発火年齢</summary>
        public readonly Dictionary<string, int> OccurredEvents = new();

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
        /// このソウルの運命のジョブ。
        /// 人生イベント方式では arc_*_final が job_*タグを付与した瞬間に確定する。
        /// </summary>
        public SoulJobDefinition DestinyJob { get; private set; }

        /// <summary>job_*タグ → SoulJobDefinitionのマッピング</summary>
        private readonly IReadOnlyDictionary<string, SoulJobDefinition> _jobTagToJobDef;

        /// <summary>
        /// life tagが付与された際にjob_*タグならDestinyJobを確定させる。
        /// </summary>
        public void TrySetJobFromTag(string lifeTag)
        {
            if (DestinyJob != null) return; // 既に確定済み
            if (_jobTagToJobDef == null) return;
            if (_jobTagToJobDef.TryGetValue(lifeTag, out var jobDef))
            {
                DestinyJob = jobDef;
                CurrentJob = jobDef;
                Debug.Log($"[ReinSim] 職業確定: {jobDef.JobName}（タグ: {lifeTag}）");
            }
        }

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
            _jobTagToJobDef = input.JobTagToJobDef;
            // JobDefがあれば事前確定、なければ人生イベントから自然決定
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

        /// <summary>
        /// ポイントをeventFactor倍率に変換。ランクに応じて上限が変化。
        /// rank1 → 0pt=1.0x / 40pt=1.1x
        /// rank10 → 0pt=1.0x / 40pt=1.5x
        /// 間は比例（rank毎に上限+0.0444x）
        /// </summary>
        public static float PtToMultiplier(int pt, int rank = 1)
        {
            // rank1=0.10, rank10=0.50、1ランクごとに +0.0444
            float maxBonus = 0.10f + (Mathf.Clamp(rank, 1, 10) - 1) / 9f * 0.40f;
            return 1.0f + (pt / 40f) * maxBonus;
        }

        // ============================================================
        // 転生内ステータス即時加算（MaxStatsに直接加算 → 以降のNowStat計算に反映）
        // ============================================================
        /// <summary>
        /// grantsStats による転生内ステータス即時加算。
        /// MaxStats を直接増やすことで UpdateNowStats() の結果にも反映される。
        /// </summary>
        public void AddInLifeStat(int statIdx, int value)
        {
            if (statIdx < 0 || statIdx >= 5) return;
            MaxStats[statIdx] = Mathf.Max(0, MaxStats[statIdx] + value);
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

        /// <summary>
        /// 人生イベントから確定したジョブ。
        /// 事前抽選方式では input.JobDef と同じ値。
        /// 人生自然決定方式では arc_*_final 発火時に確定した値。
        /// </summary>
        public readonly SoulJobDefinition DestinyJob;

        /// <summary>シミュ全体で取得したライフタグ一覧</summary>
        public readonly HashSet<string> AcquiredLifeTags;

        public ReinSimResult(ReinSimContext ctx)
        {
            // int pt → float 倍率に変換してから格納（ランク依存上限を適用）
            EventFactors = new float[5];
            int finalRankForFactor = ctx.CurrentRank;
            for (int i = 0; i < 5; i++)
                EventFactors[i] = ReinSimContext.PtToMultiplier(ctx.EventFactors[i], finalRankForFactor);
            FinalRank = ctx.CurrentRank;
            LearnedSkillIds = new List<string>(ctx.LearnedSkillIds);
            HistoryEvents = new List<ReinEvent>(ctx.HistoryEvents);
            DestinyJob = ctx.DestinyJob;
            AcquiredLifeTags = new HashSet<string>(ctx.AcquiredLifeTags);
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
        /// relatedJobs（SO参照）またはrelatedJobIds（ID文字列）のいずれかに
        /// DestinyJobが一致するかチェックする。
        /// </summary>
        private static bool MatchesRelatedJob(ReinLifeEventSO ev, SoulJobDefinition destinyJob)
        {
            if (destinyJob == null) return false;

            // SO参照チェック
            if (ev.RelatedJobs != null)
                foreach (var j in ev.RelatedJobs)
                    if (j == destinyJob) return true;

            // ID文字列チェック
            if (ev.RelatedJobIds != null)
                foreach (var id in ev.RelatedJobIds)
                    if (!string.IsNullOrEmpty(id) && id == destinyJob.JobId) return true;

            return false;
        }

        /// <summary>ランクアップイベントのrequireStatsを動的に計算する。</summary>
        private static bool MeetsRankUpStatRequirements(
            ReinSimContext ctx, int rankIndex)
        {
            var job = ctx.DestinyJob;
            if (job == null) return true;

            float[] muls = job.GetStatMultipliers(); // AT,DF,AGI,MAT,MDF

            // jobTierからランクUP難易度を導出
            // 50以上→Easy / 20〜49→Medium / 19以下→Hard
            int topN;
            int targetLv;
            if (job.JobTier >= 50)
            {
                topN = 2;
                targetLv = NormalLevels[rankIndex];
            }
            else if (job.JobTier >= 20)
            {
                topN = 4;
                targetLv = NormalLevels[rankIndex];
            }
            else
            {
                topN = 4;
                targetLv = NormalLevels[rankIndex] + 2;
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

                // 3) 年初スナップショット：requiresAnyLifeTag の判定に使う
                var tagsAtYearStart = new HashSet<string>(ctx.AcquiredLifeTags);

                // 4) 各イベントSO処理（ランクアップイベントもここで処理）
                foreach (var ev in allEvents)
                    TryOccurEvent(ctx, ev, age, tagsAtYearStart);
            }

            return new ReinSimResult(ctx);
        }

        // ============================================================
        // イベント発生処理
        // ============================================================
        private static void TryOccurEvent(
            ReinSimContext ctx,
            ReinLifeEventSO ev,
            int age,
            HashSet<string> tagsAtYearStart)
        {
            // 年齢レンジ外
            if (age < ev.StartAge || age > ev.EndAge) return;

            // 再発不可
            if (ctx.EndedEvents.Contains(ev)) return;

            // ランク・ステータス前提条件チェック（requiresAnyLifeTag / blockedByLifeTags 含む）
            if (!ev.MeetsPrerequisites(ctx.CurrentRank, ctx.NowStats, tagsAtYearStart)) return;

            // ランクアップイベントの場合：動的閾値チェック
            if (ev.Options != null && ev.Options.Count > 0 && ev.Options[0].IsRankUp)
            {
                int rankIdx = Mathf.Clamp(ctx.CurrentRank - 1, 0, 8);
                if (!MeetsRankUpStatRequirements(ctx, rankIdx)) return;
            }

            // requires：前提イベントが全部発生済みでないとダメ
            foreach (var reqId in ev.RequiresEventIds)
            {
                if (string.IsNullOrEmpty(reqId)) continue;
                if (!ctx.OccurredEvents.ContainsKey(reqId))
                    return; // 未発生
            }

            // requiresPrevYear：前提イベントが今年と同年なら発火禁止
            foreach (var reqId in ev.RequiresPrevYearEventIds)
            {
                if (string.IsNullOrEmpty(reqId)) continue;
                if (!ctx.OccurredEvents.TryGetValue(reqId, out int firedAge))
                    return; // 未発生
                if (firedAge >= age)
                    return; // 同年発火禁止
            }

            // relatedJobIds が設定されている場合、DestinyJobが一致しなければスキップ
            if (ev.RelatedJobIds != null && ev.RelatedJobIds.Count > 0)
                if (!MatchesRelatedJob(ev, ctx.DestinyJob)) return;

            // blockedBy：排他イベントが1つでも発生済みならダメ
            foreach (var blkId in ev.BlockedByEventIds)
            {
                if (!string.IsNullOrEmpty(blkId) && ctx.OccurredEvents.ContainsKey(blkId))
                    return;
            }

            // ライフタグボーナスを加算した出現重みで判定
            float weight = ev.BaseWeight;
            weight += ev.CalcLifeTagBonus(ctx.AcquiredLifeTags);

            if (UnityEngine.Random.value >= weight) return;

            // 選択肢を重み付き抽選
            var option = ChooseOption(ctx, ev);
            if (option == null) return;

            // 結果を適用
            ApplyOption(ctx, ev, option, age);
        }

        // ============================================================
        // イベントIDからReinEventTypeを判定
        // 大半はNone。感情が大きく動く瞬間だけ型をつける。
        // ============================================================
        private static ReinEventType ResolveEventType(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return ReinEventType.None;

            // 生涯の終わり
            if (eventId.StartsWith("life_end_")) return ReinEventType.LifeEnd;

            switch (eventId)
            {
                // 誕生
                case "birth_wealthy":
                case "birth_normal":
                case "birth_poor":
                    return ReinEventType.Birth;

                // Happy：おめでとう系（結婚・子供誕生・合格・夢の達成）
                case "marriage_success":
                case "child_born":
                case "grandchild_born":
                case "elder_grandchild_born2":
                case "exam_middle_pass":
                case "exam_high_pass_top":
                case "exam_high_pass_second":
                case "exam_univ_pass_top":
                case "exam_univ_pass":
                case "exam_univ_pass_ronin":
                case "love_triangle_win":
                case "love_longdistance_survive":
                case "first_love_success":
                case "mid_achievement":
                case "midlife_independent":
                case "talent_overcome":
                case "betrayal_forgive":
                case "parent_recover":
                case "rare_lottery":
                    return ReinEventType.Happy;

                // Sad：じわっと沈む系（死別・喪失・別れ）
                case "parent_death":
                case "old_friend_death":
                case "friend_death":
                case "betrayal_start":
                case "betrayal_distance":
                case "romance_breakup":
                case "marriage_decline":
                case "love_triangle_lose":
                case "love_longdistance_end":
                case "first_love_fail":
                case "midlife_friend_ill":
                case "old_friend_loss":
                case "family_violence":
                case "family_poverty_sudden":
                case "elder_spouse_ill":
                    return ReinEventType.Sad;

                // Shock：青天の霹靂系（事故・災害・突然の発覚）
                case "near_death_accident":
                case "near_death_change":
                case "rare_accident":
                case "rare_disaster":
                case "rare_illness":
                case "family_trouble_start":
                case "family_divorce":
                case "family_escape":
                    return ReinEventType.Shock;

                default:
                    return ReinEventType.None;
            }
        }


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

                ctx.HistoryEvents.Add(new ReinEvent(
                    age,
                    $"ランク{ctx.CurrentRank}に上がった。\n{option.Sentence}",
                    ReinEventType.RankUp));

                foreach (var skill in option.RankUpSkills)
                {
                    if (skill != null)
                    {
                        ctx.LearnSkill(skill);
                        ctx.HistoryEvents.Add(new ReinEvent(
                            age,
                            $"スキル「{skill.SkillName}」を習得した。",
                            ReinEventType.None,
                            hideAge: true));
                    }
                }
            }
            else
            {
                ctx.HistoryEvents.Add(new ReinEvent(
                    age,
                    option.Sentence,
                    ResolveEventType(ev.EventId)));

                foreach (var skill in option.GrantSkills)
                {
                    if (skill != null)
                    {
                        ctx.LearnSkill(skill);
                        ctx.HistoryEvents.Add(new ReinEvent(
                            age,
                            $"スキル「{skill.SkillName}」を習得した。",
                            ReinEventType.None,
                            hideAge: true));
                    }
                }
            }

            // ライフタグ付与（イベントSO側）
            foreach (var tag in ev.GrantsLifeTags)
            {
                ctx.AcquiredLifeTags.Add(tag);
                ctx.TrySetJobFromTag(tag); // job_*タグなら職業を確定
            }

            // ライフタグ付与（選択肢側）
            foreach (var tag in option.GrantsLifeTags)
            {
                ctx.AcquiredLifeTags.Add(tag);
                ctx.TrySetJobFromTag(tag); // job_*タグなら職業を確定
            }

            // 転生内ステータス即時加算（grantsStats）
            if (option.GrantsStats != null)
            {
                foreach (var bonus in option.GrantsStats)
                {
                    ctx.AddInLifeStat(bonus.StatIndex, bonus.Value);
                }
                // 加算後すぐに NowStats を再計算して次のイベント判定に反映
                ctx.UpdateNowStats(age);
            }

            // 発生済みフラグ登録（発火年齢も記録）
            ctx.OccurredEvents[ev.EventId] = age;

            // 再発不可登録
            ctx.EndedEvents.Add(ev);
        }
    }
}