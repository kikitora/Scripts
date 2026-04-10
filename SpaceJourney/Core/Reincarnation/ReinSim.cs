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
        /// B経路：SoulInstanceから直接組み立てるファクトリ（プレイヤー転生用）。
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

        /// <summary>
        /// C経路：シードステータスから直接組み立てるファクトリ（固定ランク量産用）。
        /// 守護霊は持たない（HalfStats=null）。MaxStats計算は seedStats × 100% になる。
        /// </summary>
        public static ReinSimInput BuildFromSeed(
            int[] seedStats,
            int rank,
            GrowthType growthType,
            SoulJobDefinition jobDef,
            TalentRank talent,
            IReadOnlyDictionary<string, SoulJobDefinition> jobTagToJobDef = null)
        {
            if (seedStats == null || seedStats.Length < 5)
                throw new ArgumentException("seedStats must be length 5", nameof(seedStats));

            var input = new ReinSimInput(
                seedStats,
                new int[3][], // 守護霊なし（全部null）
                rank,
                growthType,
                jobDef,
                talent,
                0,
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
        public readonly HashSet<ReinLifeEvent> EndedEvents = new();

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
        /// 事前抽選方式では ReinSimContext 初期化時に input.JobDef がそのまま入る。
        /// 「ジョブが決まっている」状態であって、「実際に生業についた」状態とは別。
        /// 実際に生業についたかは JobConfirmedAge >= 0 で判定する。
        /// </summary>
        public SoulJobDefinition DestinyJob { get; private set; }

        /// <summary>
        /// 実際に生業についた年齢（job_*タグが発火した年）。
        /// -1 = まだ生業についていない。
        /// ランクUP判定や「就業後フレーバー」判定はこの値を見る。
        /// </summary>
        public int JobConfirmedAge { get; private set; } = -1;

        /// <summary>job_*タグ → SoulJobDefinitionのマッピング</summary>
        private readonly IReadOnlyDictionary<string, SoulJobDefinition> _jobTagToJobDef;

        /// <summary>
        /// life tagが付与された際にjob_*タグなら、生業就任を確定させる。
        /// 事前抽選方式では DestinyJob は既に設定済みなので、ここでは JobConfirmedAge だけを記録する。
        /// </summary>
        public void TrySetJobFromTag(string lifeTag, int age)
        {
            if (JobConfirmedAge >= 0) return; // 既に就業確定済み
            if (_jobTagToJobDef == null) return;
            if (!_jobTagToJobDef.TryGetValue(lifeTag, out var jobDef)) return;

            // 事前抽選と人生で確定したジョブが食い違う場合は、後者を優先する
            if (DestinyJob != jobDef)
            {
                DestinyJob = jobDef;
                CurrentJob = jobDef;
            }
            JobConfirmedAge = age;
            Debug.Log($"[ReinSim] 生業確定: {jobDef.JobName}（タグ: {lifeTag}, 年齢: {age}）");
        }

        /// <summary>転生回数（SpaceJourneyCoreTypesの定数でボーナス計算に使う）</summary>
        public int ReinCount { get; private set; }

        // --------------------------------------------------------
        // 結果の蓄積
        // --------------------------------------------------------
        public readonly List<string> LearnedSkillIds = new();
        public readonly List<ReinEvent> HistoryEvents = new();

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

            // 新仕様：シミュ開始時は常に rank=1 スタート（生業に就く前）。
            // ランクUPは ReinSimulator.Run のループ内で
            //   - B経路（fixedRank=null）：各歳でステ閾値+確率判定で進行
            //   - C経路（fixedRank!=null）：事前スケジュールに沿って強制進行
            // のいずれかで処理される。
            CurrentRank = 1;
            GuaranteedRankUpTo = 1;

            // --------------------------------------------------
            // MaxStats計算
            // 守護霊あり（B経路：プレイヤー転生）
            //   → 主×40% + 守護霊×20%×3
            // 守護霊なし（C経路：固定ランク量産。シードソウルのみ）
            //   → 主×100%
            // --------------------------------------------------
            bool hasGuardians = false;
            if (input.HalfStats != null)
            {
                for (int g = 0; g < input.HalfStats.Length; g++)
                {
                    if (input.HalfStats[g] != null)
                    {
                        hasGuardians = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                float v;
                if (hasGuardians)
                {
                    v = input.MainStats[i] * 0.40f;
                    for (int g = 0; g < input.HalfStats.Length; g++)
                    {
                        if (input.HalfStats[g] != null)
                            v += input.HalfStats[g][i] * 0.20f;
                    }
                }
                else
                {
                    // 守護霊なし：シードソウル100%
                    v = input.MainStats[i] * 1.00f;
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

        // ランクアップ閾値計算に使うバランス定数
        private const float GrowthNormal = 6.25f;
        private const float GrowthPower = 1.0f;
        private const int StatMaxLevel = 25;
        private static readonly int[] NormalLevels = { 4, 6, 7, 9, 11, 13, 15, 17, 19, 21 };
        private static readonly float[] RankBaseStats =
            { 45,55,65,75,85,95,105,115,125,135 };
        private static readonly float TalentMidMul = (1.10f + 1.24f) / 2f; // C人材中間値

        /// <summary>
        /// relatedJobIds（ID文字列）に DestinyJob が含まれるかチェックする。
        /// ※ 旧SO参照版（relatedJobs）は廃止。ReinLifeEvent.MatchesRelatedJob と等価。
        /// </summary>
        private static bool MatchesRelatedJob(ReinLifeEvent ev, SoulJobDefinition destinyJob)
        {
            return ev != null && ev.MatchesRelatedJob(destinyJob);
        }

        /// <summary>
        /// requiresAnyLifeTag に "job_" で始まるタグが含まれているか。
        /// 含まれる = 「就業後に発火するフレーバーイベント」 → ルート扱いしない。
        /// 含まれない = 「就業前の連鎖イベント」 → ルート扱い対象。
        /// </summary>
        private static bool RequiresJobLifeTag(ReinLifeEvent ev)
        {
            if (ev?.requiresAnyLifeTag == null) return false;
            foreach (var tag in ev.requiresAnyLifeTag)
            {
                if (!string.IsNullOrEmpty(tag) && tag.StartsWith("job_"))
                    return true;
            }
            return false;
        }

        // ランクUPの「就職年齢起点で何年以内」に収まるか（C経路スケジュール用）
        private const int RankUpWindowYears = 30;

        // ランクUP年齢の揺らぎ（C経路スケジュール用、±この値の整数）
        private const int RankUpAgeJitter = 3;

        // ----------------------------------------------------------------
        // ReinSimulator.Run（B/C両対応）
        // ----------------------------------------------------------------
        // fixedRank == null  → B経路：プレイヤー転生。各歳でステ閾値+確率判定でランクUPする
        // fixedRank != null → C経路：固定ランク量産。事前スケジュールで決まった年齢に強制ランクUP
        // ----------------------------------------------------------------
        public static ReinSimResult Run(
            ReinSimInput input,
            IReadOnlyList<ReinLifeEvent> allEvents,
            int? fixedRank = null)
        {
            var ctx = new ReinSimContext(input);

            // C経路用：生業確定後に1度だけ作るランクUPスケジュール
            HashSet<int> rankUpSchedule = null;
            // C経路用：スケジュール年齢 → 上昇後ランクのマッピング（来歴テキスト用）
            Dictionary<int, int> rankUpScheduleAgeToRank = null;

            for (int age = 0; age <= MaxAge; age++)
            {
                // 1) 転生内ステータスを年齢に応じて更新
                ctx.UpdateNowStats(age);

                // 2) イベント発火（先に処理 = この年の発火結果がランクUP判定に反映される）
                //    マルチパスイベント処理（最大6パス・1年あたり最大2イベントの「枠」）
                //    強制発火（保証 or ルート最終年）は枠を消費しないので、
                //    生業ルートは詰まらず必ず通る。
                //    passごとにAcquiredLifeTagsの最新状態を使うことで、
                //    同年内にタグが付与された後のイベント（宇宙中フレーバー等）も正しく発火する。
                var firedThisAge = new HashSet<string>();
                int slotConsumed = 0;
                for (int pass = 0; pass < 6; pass++)
                {
                    // passごとに最新のタグスナップショットを取得
                    var tagsThisPass = new HashSet<string>(ctx.AcquiredLifeTags);
                    bool firedAny = false;
                    foreach (var ev in allEvents)
                    {
                        if (firedThisAge.Contains(ev.EventId)) continue;
                        var (fired, consumes) = TryOccurEvent(ctx, ev, age, tagsThisPass, firedThisAge);
                        if (fired)
                        {
                            firedAny = true;
                            if (consumes) slotConsumed++;
                            if (slotConsumed >= 2) break;
                        }
                    }
                    if (!firedAny || slotConsumed >= 2) break;
                }

                // 3) ランクUP判定（生業確定後・30年枠内のみ）
                //    JobConfirmedAge は TrySetJobFromTag で job_*タグが付与された年に設定される。
                int jobStartAge = ctx.JobConfirmedAge;
                if (jobStartAge >= 0)
                {
                    int yearsSinceJob = age - jobStartAge;
                    if (yearsSinceJob >= 0 && yearsSinceJob <= RankUpWindowYears)
                    {
                        if (fixedRank.HasValue)
                        {
                            // ── C経路：固定ランクスケジュール ──
                            if (rankUpSchedule == null)
                            {
                                rankUpSchedule = new HashSet<int>();
                                rankUpScheduleAgeToRank = new Dictionary<int, int>();
                                BuildFixedRankSchedule(
                                    jobStartAge,
                                    fixedRank.Value,
                                    rankUpSchedule,
                                    rankUpScheduleAgeToRank);
                            }
                            if (rankUpSchedule.Contains(age) &&
                                ctx.CurrentRank < SpaceJourneyConstants.MaxSoulJobRank)
                            {
                                ctx.DoRankUp();
                                int newRank = rankUpScheduleAgeToRank[age];
                                ctx.HistoryEvents.Add(new ReinEvent(
                                    age,
                                    $"ランク{newRank}に上がった。",
                                    ReinEventType.RankUp));
                            }
                        }
                        else
                        {
                            // ── B経路：因果ループ。各歳でステ閾値+確率判定 ──
                            if (ctx.CurrentRank < SpaceJourneyConstants.MaxSoulJobRank)
                            {
                                int rankIdx = ctx.CurrentRank - 1;
                                if (rankIdx >= 0 && rankIdx < NormalLevels.Length &&
                                    RollRankUpProb(ctx, rankIdx))
                                {
                                    ctx.DoRankUp();
                                    int newRank = ctx.CurrentRank;
                                    ctx.HistoryEvents.Add(new ReinEvent(
                                        age,
                                        $"ランク{newRank}に上がった。",
                                        ReinEventType.RankUp));
                                }
                            }
                        }
                    }
                }
            }

            return new ReinSimResult(ctx);
        }

        // ----------------------------------------------------------------
        // B経路：年単位ランクUP確率判定
        // ----------------------------------------------------------------
        // ジョブ上位N stat の最弱（ボトルネック）を ratio = NowStats/threshold で見て、
        // prob = clamp((ratio - 1.0)^1.5 × 0.35, 0, 0.90) で確率判定する。
        // ratio < 1.0 なら 0%（閾値未達）。
        // ratio が 2.0 で 35%、3.0 で 90%（キャップ）。
        // ----------------------------------------------------------------
        private static bool RollRankUpProb(ReinSimContext ctx, int rankIdx)
        {
            var job = ctx.DestinyJob;
            if (job == null) return false;

            float[] muls = job.GetStatMultipliers();
            int tier = job.JobTier;

            int topN;
            int targetLv;
            if (tier >= 50) { topN = 2; targetLv = NormalLevels[rankIdx]; }
            else if (tier >= 20) { topN = 4; targetLv = NormalLevels[rankIdx]; }
            else { topN = 4; targetLv = NormalLevels[rankIdx] + 2; }

            int[] order = { 0, 1, 2, 3, 4 };
            System.Array.Sort(order, (a, b) => muls[b].CompareTo(muls[a]));

            float s = (targetLv - 1f) / (StatMaxLevel - 1f);
            float gf = 1f + (GrowthNormal - 1f) * Mathf.Pow(s, GrowthPower);

            float minRatio = float.MaxValue;
            for (int i = 0; i < topN; i++)
            {
                int si = order[i];
                float lv1Stat = RankBaseStats[rankIdx] * muls[si] * TalentMidMul * 0.1f;
                int threshold = Mathf.RoundToInt(lv1Stat * gf);
                if (threshold <= 0) continue;
                float ratio = (float)ctx.NowStats[si] / threshold;
                if (ratio < minRatio) minRatio = ratio;
            }

            if (minRatio < 1.0f) return false;

            float prob = Mathf.Pow(minRatio - 1.0f, 1.5f) * 0.35f;
            prob = Mathf.Clamp(prob, 0f, 0.90f);

            return UnityEngine.Random.value < prob;
        }

        // ----------------------------------------------------------------
        // C経路：ランクUPスケジュール作成
        // ----------------------------------------------------------------
        // 就職年齢を起点に30年枠を均等割り。上のランクほど間隔が長くなるよう
        // 重み weights[i] = i + 1 で配分し、各点に ±RankUpAgeJitter の揺らぎを加える。
        // 揺らぎで順序が逆転しないよう昇順ソートで保証する。
        // ----------------------------------------------------------------
        private static void BuildFixedRankSchedule(
            int jobStartAge,
            int finalRank,
            HashSet<int> outAges,
            Dictionary<int, int> outAgeToRank)
        {
            int rankUpCount = finalRank - 1;
            if (rankUpCount <= 0) return;

            // 重み（後ろのランクほど長い間隔）
            float[] weights = new float[rankUpCount];
            float weightSum = 0f;
            for (int i = 0; i < rankUpCount; i++)
            {
                weights[i] = i + 1;
                weightSum += weights[i];
            }

            int[] rankUpAges = new int[rankUpCount];
            float accumulated = 0f;
            for (int i = 0; i < rankUpCount; i++)
            {
                accumulated += weights[i];
                float baseAge = jobStartAge + (accumulated / weightSum) * RankUpWindowYears;
                // ±RankUpAgeJitter の整数揺らぎ
                int jitter = UnityEngine.Random.Range(-RankUpAgeJitter, RankUpAgeJitter + 1);
                int candidate = Mathf.RoundToInt(baseAge) + jitter;
                // 下限：就職翌年以降
                rankUpAges[i] = Mathf.Max(jobStartAge + 1, candidate);
                // 上限：30年枠内
                rankUpAges[i] = Mathf.Min(jobStartAge + RankUpWindowYears, rankUpAges[i]);
            }

            // 順序保証
            System.Array.Sort(rankUpAges);

            // 重複が起きた場合、後続を1歳ずつずらして解消（30年枠は越えない）
            for (int i = 1; i < rankUpAges.Length; i++)
            {
                if (rankUpAges[i] <= rankUpAges[i - 1])
                    rankUpAges[i] = Mathf.Min(jobStartAge + RankUpWindowYears, rankUpAges[i - 1] + 1);
            }

            for (int i = 0; i < rankUpAges.Length; i++)
            {
                int age = rankUpAges[i];
                int newRank = i + 2; // rank2/3/4...
                outAges.Add(age);
                outAgeToRank[age] = newRank;
            }
        }

        // ============================================================
        // イベント発生処理
        // ============================================================
        /// <summary>
        /// イベント発火試行。返り値は「発火したか」と「枠消費するか(=通常発火)」のタプル。
        /// 通常発火 = 1年あたり2件上限の枠を消費する。
        /// 強制発火（baseWeight≥1.0 or ルート最終年） = 枠を消費しない。
        /// </summary>
        private static (bool fired, bool consumesSlot) TryOccurEvent(
            ReinSimContext ctx,
            ReinLifeEvent ev,
            int age,
            HashSet<string> tagsThisPass,
            HashSet<string> firedThisAge)
        {
            // 年齢レンジ外
            if (age < ev.StartAge || age > ev.EndAge) return (false, false);

            // 再発不可
            if (ctx.EndedEvents.Contains(ev)) return (false, false);

            // ランク・ステータス前提条件チェック（requiresAnyLifeTag / blockedByLifeTags 含む）
            if (!ev.MeetsPrerequisites(ctx.CurrentRank, ctx.NowStats, tagsThisPass)) return (false, false);

            // requires：前提イベントが全部発生済みでないとダメ
            foreach (var reqId in ev.RequiresEventIds)
            {
                if (string.IsNullOrEmpty(reqId)) continue;
                if (!ctx.OccurredEvents.ContainsKey(reqId))
                    return (false, false); // 未発生
            }

            // requiresPrevYear：前提イベントが今年と同年なら発火禁止
            foreach (var reqId in ev.RequiresPrevYearEventIds)
            {
                if (string.IsNullOrEmpty(reqId)) continue;
                if (!ctx.OccurredEvents.TryGetValue(reqId, out int firedAge))
                    return (false, false); // 未発生
                if (firedAge >= age)
                    return (false, false); // 同年発火禁止
            }

            // minYearsAfter：前提イベントからN年以上経過していないと発火禁止
            foreach (var entry in ev.MinYearsAfterEvents)
            {
                if (string.IsNullOrEmpty(entry.EventId)) continue;
                if (!ctx.OccurredEvents.TryGetValue(entry.EventId, out int firedAge))
                    return (false, false); // 前提イベント未発生
                if (age - firedAge < entry.MinYears)
                    return (false, false); // 経過年数が足りない
            }

            // relatedJobIds が設定されている場合、DestinyJobが一致しなければスキップ
            if (ev.RelatedJobIds != null && ev.RelatedJobIds.Count > 0)
                if (!MatchesRelatedJob(ev, ctx.DestinyJob)) return (false, false);

            // blockedBy：排他イベントが1つでも発生済みならダメ
            foreach (var blkId in ev.BlockedByEventIds)
            {
                if (!string.IsNullOrEmpty(blkId) && ctx.OccurredEvents.ContainsKey(blkId))
                    return (false, false);
            }

            // ── 強制発火判定 ──
            // 1) baseWeight >= 1.0 の保証イベント（ev_*g など）
            // 2) 「ルートイベント」かつ年齢範囲の最終年に到達
            //    ルート = relatedJobIds が DestinyJob と一致 かつ 就業前イベント
            //    （requiresAnyLifeTag に job_* を含まない = 生業就任前の連鎖イベント）
            //    → 「年齢的ずれはあっても必ず起きる」を実装
            bool isGuaranteed = ev.BaseWeight >= 1.0f;
            bool isRouteForcedFire =
                !isGuaranteed &&
                age >= ev.EndAge &&
                ctx.JobConfirmedAge < 0 &&
                MatchesRelatedJob(ev, ctx.DestinyJob) &&
                !RequiresJobLifeTag(ev);

            if (!isGuaranteed && !isRouteForcedFire)
            {
                // StatWeightConfigがある場合は年齢範囲で割りstatで重みを変化させる
                float weight;
                var swc = ev.StatWeightConfig;
                if (swc != null && swc.StatIndex >= 0)
                {
                    int si = swc.StatIndex;
                    float norm = Mathf.Min(ctx.NowStats[si] / 100f, 1f);
                    float period = Mathf.Max(ev.EndAge - ev.StartAge + 1, 1);
                    weight = (ev.BaseWeight / period) * (swc.Sign == "+" ? norm * 3f : (1f - norm) * 3f);
                    weight = Mathf.Clamp(weight, 0.001f, 1f);
                }
                else
                {
                    weight = ev.BaseWeight;
                }
                weight += ev.CalcLifeTagBonus(ctx.AcquiredLifeTags);

                if (UnityEngine.Random.value >= weight) return (false, false);
            }

            // 選択肢を重み付き抽選
            var option = ChooseOption(ctx, ev);
            if (option == null) return (false, false);

            // 結果を適用
            ApplyOption(ctx, ev, option, age);

            // 同年発火済みに登録
            firedThisAge.Add(ev.EventId);

            // 強制発火（保証 or ルート最終年）は枠を消費しない
            bool consumes = !isGuaranteed && !isRouteForcedFire;
            return (true, consumes);
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
            ReinLifeEvent ev)
        {
            if (ev.Options == null || ev.Options.Count == 0) return null;

            // 各選択肢の最終重みを計算（nowStatsをstatConditionsに渡す）
            float[] weights = new float[ev.Options.Count];
            float total = 0f;

            for (int i = 0; i < ev.Options.Count; i++)
            {
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
            ReinLifeEvent ev,
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

            // イベント本文を記録（空でなければ）
            if (!string.IsNullOrEmpty(ev.Sentence))
            {
                ctx.HistoryEvents.Add(new ReinEvent(
                    age,
                    ev.Sentence,
                    ResolveEventType(ev.EventId)));
            }

            // オプション文を記録（空でなければ）
            if (!string.IsNullOrEmpty(option.Sentence))
            {
                ctx.HistoryEvents.Add(new ReinEvent(
                    age,
                    option.Sentence,
                    ReinEventType.None,
                    hideAge: true));
            }

            // ライフタグ付与（イベントSO側）
            foreach (var tag in ev.GrantsLifeTags)
            {
                ctx.AcquiredLifeTags.Add(tag);
                ctx.TrySetJobFromTag(tag, age); // job_*タグなら生業就任を確定
                // minYearsAfterEventsがライフタグ名で参照できるようOccurredEventsにも記録
                if (!ctx.OccurredEvents.ContainsKey(tag))
                    ctx.OccurredEvents[tag] = age;
            }

            // ライフタグ付与（選択肢側）
            foreach (var tag in option.GrantsLifeTags)
            {
                ctx.AcquiredLifeTags.Add(tag);
                ctx.TrySetJobFromTag(tag, age); // job_*タグなら生業就任を確定
                if (!ctx.OccurredEvents.ContainsKey(tag))
                    ctx.OccurredEvents[tag] = age;
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

            // 再発不可登録（Repeatableでないイベントのみ）
            if (!ev.IsRepeatable)
                ctx.EndedEvents.Add(ev);
        }
    }
}