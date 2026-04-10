using System;
using System.Collections.Generic;

namespace SteraCube.SpaceJourney
{
    // ================================================================
    // ReinLifeEvent：転生ライフイベント（純C#クラス・JSON駆動）
    // ================================================================
    // 旧 ReinLifeEventSO（ScriptableObject）からの完全置換。
    //
    // ・データソースは Resources/ReinLifeEvents.json（→ ReinLifeEventLoader）
    // ・grantSkills / isRankUp / rankUpSkills は廃止済み
    // ・スキル習得の正規ルートは SoulJobDefinition.SkillSets（rank到達時の自動付与）
    // ・MasterDatabase は実行時に Loader を呼んで List<ReinLifeEvent> を保持する
    //
    // JSON とのフィールド名はすべてキャメルケースで固定。
    // 互換のため内部 class 名（StatCondition / StatEffect / StatPrerequisite /
    // StatBonus / StatWeightConfig / MinYearsAfterEntry / LifeTagWeightBonus /
    // ReinSentenceOption）は旧SO定義と同じにしてある。
    // ================================================================
    [Serializable]
    public class ReinLifeEvent
    {
        // ────────────────────────────────────────
        // 基本情報
        // ────────────────────────────────────────
        public string eventId;
        public string displayName;
        public string editorMemo;

        // ────────────────────────────────────────
        // イベント本文
        // ────────────────────────────────────────
        public string sentence;

        // ────────────────────────────────────────
        // 出現条件
        // ────────────────────────────────────────
        public int startAge = 0;
        public int endAge = 99;
        public float baseWeight = 1f;
        public bool isRepeatable = false;

        // ────────────────────────────────────────
        // ランク条件（0=制限なし）
        // ────────────────────────────────────────
        public int requireMinRank = 0;
        public int requireMaxRank = 0;

        // ────────────────────────────────────────
        // ライフタグ条件
        // ────────────────────────────────────────
        public List<string> requiresAnyLifeTag = new();
        public List<string> blockedByLifeTags = new();
        public List<string> grantsLifeTags = new();

        // ────────────────────────────────────────
        // ステータス前提条件（AND）
        // ────────────────────────────────────────
        public List<StatPrerequisite> requireStatsAnd = new();

        // ────────────────────────────────────────
        // 経過年数条件
        // ────────────────────────────────────────
        public List<MinYearsAfterEntry> minYearsAfterEvents = new();

        // ────────────────────────────────────────
        // 職業タグ
        // ────────────────────────────────────────
        public bool hasTendency = false;
        public SoulJobTendency tendency = SoulJobTendency.Warrior;
        public float jobMatchBonus = 0.5f;

        // ────────────────────────────────────────
        // ライフタグ重みボーナス
        // ────────────────────────────────────────
        public List<LifeTagWeightBonus> lifeTagWeightBonuses = new();

        // ────────────────────────────────────────
        // StatWeightConfig
        // ────────────────────────────────────────
        public bool hasStatWeightConfig = false;
        public StatWeightConfig statWeightConfig = null;

        // ────────────────────────────────────────
        // イベント前提・排他（ID参照）
        // ────────────────────────────────────────
        public List<string> requiresEventIds = new();
        public List<string> requiresPrevYearEventIds = new();
        public List<string> blockedByEventIds = new();

        // ────────────────────────────────────────
        // 紐づくジョブ（ID参照のみ。SO参照は廃止）
        // ────────────────────────────────────────
        // relatedJobIds: このリストに DestinyJob が含まれる場合のみ発火 (whitelist)
        // excludedJobIds: このリストに DestinyJob が含まれる場合は発火しない (blacklist)
        // 両方空の場合はジョブ条件なし。両方指定の場合は relatedJobIds 優先 → 含まれていれば発火、
        // 次に excludedJobIds → 含まれていれば発火しない。
        public List<string> relatedJobIds = new();
        public List<string> excludedJobIds = new();

        // ────────────────────────────────────────
        // 結果の選択肢
        // ────────────────────────────────────────
        public List<ReinSentenceOption> options = new();

        // ────────────────────────────────────────
        // 新仕様: 5段階×ランク発火確率
        // ────────────────────────────────────────
        // eventStage:
        //   0 = 旧仕様（baseWeight + statWeightConfig）で発火判定
        //   +1〜+5 = 新仕様（5段階×ランクテーブル参照、stat値は startAge での値で判定）
        //     +1=易, +2=普通, +3=中堅, +4=難, +5=伝説
        //   -1〜-2 = 不幸補正（baseWeight × ランク別乗算率）
        //     高ランクほど発火率が下がる
        //
        // statCompareCount:
        //   1〜5。比較する stat の数。ジョブの statMul 上位 N 個を取る。
        //
        // statCompareMode:
        //   "min" = 上位 N stat の最低ランクで判定（ボトルネック方式）
        //   "avg" = 平均
        //   "max" = 最高
        //
        // 「段階0=変更なし」なので既存イベントに何も付けなくても動く。
        // 段階を付けたい場合のみ初期化子で指定する。
        public int eventStage = 0;
        public int statCompareCount = 3;
        public string statCompareMode = "min";

        // ────────────────────────────────────────
        // 生業確定からN年経過必須(死亡など人生後半向け)
        // ────────────────────────────────────────
        // 例: requireYearsAfterJob = 30 → 生業確定+30年（=ランクUPスケジュール完了後）以降のみ発火
        // 0 = 制限なし
        public int requireYearsAfterJob = 0;

        // ────────────────────────────────────────
        // 旧SOプロパティ互換（参照側コードを書き換えなくてよいように）
        // ────────────────────────────────────────
        public string EventId => eventId;
        public string Sentence => sentence;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? eventId : displayName;
        public string EditorMemo => editorMemo;

        public int StartAge => startAge;
        public int EndAge => endAge;
        public float BaseWeight => baseWeight;
        public bool IsRepeatable => isRepeatable;

        public int RequireMinRank => requireMinRank;
        public int RequireMaxRank => requireMaxRank;

        public IReadOnlyList<string> RequiresAnyLifeTag => requiresAnyLifeTag;
        public IReadOnlyList<string> BlockedByLifeTags => blockedByLifeTags;
        public IReadOnlyList<string> GrantsLifeTags => grantsLifeTags;

        public IReadOnlyList<StatPrerequisite> RequireStatsAnd => requireStatsAnd;
        public IReadOnlyList<MinYearsAfterEntry> MinYearsAfterEvents => minYearsAfterEvents;
        public IReadOnlyList<string> RequiresEventIds => requiresEventIds;
        public IReadOnlyList<string> RequiresPrevYearEventIds => requiresPrevYearEventIds;
        public IReadOnlyList<string> BlockedByEventIds => blockedByEventIds;
        public IReadOnlyList<string> RelatedJobIds => relatedJobIds;
        public IReadOnlyList<string> ExcludedJobIds => excludedJobIds;
        public IReadOnlyList<ReinSentenceOption> Options => options;

        public bool HasJobTag => hasTendency;
        public SoulJobTendency JobTendency => tendency;
        public float JobMatchBonus => jobMatchBonus;
        public IReadOnlyList<LifeTagWeightBonus> LifeTagWeightBonuses => lifeTagWeightBonuses;
        public StatWeightConfig StatWeightConfig => hasStatWeightConfig ? statWeightConfig : null;

        // ────────────────────────────────────────
        // ランタイムチェック
        // ────────────────────────────────────────

        /// <summary>職業タグフィルタ。タグなし=全職業OK。</summary>
        public bool MatchesJob(SoulJobDefinition job)
        {
            if (!hasTendency) return true;
            if (job == null) return false;
            return job.Tendency == tendency;
        }

        /// <summary>relatedJobIdsに DestinyJob が含まれるか。</summary>
        public bool MatchesRelatedJob(SoulJobDefinition destinyJob)
        {
            if (destinyJob == null) return false;
            if (relatedJobIds == null) return false;
            foreach (var id in relatedJobIds)
                if (!string.IsNullOrEmpty(id) && id == destinyJob.JobId) return true;
            return false;
        }

        /// <summary>excludedJobIds に DestinyJob が含まれていれば true (ブロック対象)</summary>
        public bool MatchesExcludedJob(SoulJobDefinition destinyJob)
        {
            if (destinyJob == null) return false;
            if (excludedJobIds == null || excludedJobIds.Count == 0) return false;
            foreach (var id in excludedJobIds)
                if (!string.IsNullOrEmpty(id) && id == destinyJob.JobId) return true;
            return false;
        }

        /// <summary>保有LifeTagに対応するボーナス重みの合計を返す。</summary>
        public float CalcLifeTagBonus(HashSet<string> acquiredLifeTags)
        {
            if (lifeTagWeightBonuses == null || lifeTagWeightBonuses.Count == 0) return 0f;
            if (acquiredLifeTags == null) return 0f;
            float bonus = 0f;
            foreach (var ltb in lifeTagWeightBonuses)
                if (!string.IsNullOrEmpty(ltb.lifeTag) && acquiredLifeTags.Contains(ltb.lifeTag))
                    bonus += ltb.weightBonus;
            return bonus;
        }

        /// <summary>ランク・ステータス・ライフタグの前提条件をまとめてチェック。</summary>
        public bool MeetsPrerequisites(
            int currentRank,
            int[] nowStats,
            HashSet<string> acquiredLifeTags = null)
        {
            if (requireMinRank > 0 && currentRank < requireMinRank) return false;
            if (requireMaxRank > 0 && currentRank > requireMaxRank) return false;

            if (requiresAnyLifeTag != null && requiresAnyLifeTag.Count > 0)
            {
                bool any = false;
                foreach (var tag in requiresAnyLifeTag)
                {
                    if (acquiredLifeTags != null && acquiredLifeTags.Contains(tag))
                    { any = true; break; }
                }
                if (!any) return false;
            }

            if (blockedByLifeTags != null && acquiredLifeTags != null)
            {
                foreach (var tag in blockedByLifeTags)
                    if (acquiredLifeTags.Contains(tag)) return false;
            }

            if (requireStatsAnd != null)
            {
                foreach (var req in requireStatsAnd)
                    if (!req.IsMet(nowStats)) return false;
            }

            return true;
        }
    }

    // ================================================================
    // ReinSentenceOption：イベント内の結果1件
    // ================================================================
    [Serializable]
    public class ReinSentenceOption
    {
        public string sentence;
        public float baseWeight = 1f;
        public List<StatCondition> statConditions = new();
        public List<StatEffect> statEffects = new();
        public List<string> grantsLifeTags = new();
        public List<StatBonus> grantsStats = new();

        // 旧API互換
        public string Sentence => sentence;
        public float BaseWeight => baseWeight;
        public IReadOnlyList<StatCondition> StatConditions => statConditions;
        public IReadOnlyList<StatEffect> StatEffects => statEffects;
        public IReadOnlyList<string> GrantsLifeTags => grantsLifeTags;
        public IReadOnlyList<StatBonus> GrantsStats => grantsStats;

        public float CalcFinalWeight(int[] currentStats)
        {
            float w = baseWeight;
            if (statConditions != null)
                foreach (var c in statConditions)
                    if (c.IsMet(currentStats)) w += c.weightBonus;
            return w < 0f ? 0f : w;
        }
    }

    // ================================================================
    // StatCondition：重み加算条件
    // ================================================================
    [Serializable]
    public class StatCondition
    {
        public StatKind stat = StatKind.AT;
        public int threshold = 10;
        public float weightBonus = 1f;

        public StatKind Stat => stat;
        public int Threshold => threshold;
        public float WeightBonus => weightBonus;

        public bool IsMet(int[] currentStats)
        {
            int idx = StatKindToIdx(stat);
            if (idx < 0 || currentStats == null || currentStats.Length <= idx) return false;
            return currentStats[idx] >= threshold;
        }
        private static int StatKindToIdx(StatKind k) => k switch
        {
            StatKind.AT => 0,
            StatKind.DF => 1,
            StatKind.AGI => 2,
            StatKind.MAT => 3,
            StatKind.MDF => 4,
            _ => -1
        };
    }

    // ================================================================
    // StatEffect：EventFactor pt 加算
    // ================================================================
    [Serializable]
    public class StatEffect
    {
        public StatKind stat = StatKind.AT;
        public int eventFactorPt = 1;

        public StatKind Stat => stat;
        public int EventFactorDelta => eventFactorPt;

        public int StatIndex => stat switch
        {
            StatKind.AT => 0,
            StatKind.DF => 1,
            StatKind.AGI => 2,
            StatKind.MAT => 3,
            StatKind.MDF => 4,
            _ => -1
        };
    }

    // ================================================================
    // StatPrerequisite：発生前提のステ条件（AND）
    // ================================================================
    [Serializable]
    public class StatPrerequisite
    {
        public StatKind stat = StatKind.AT;
        public int threshold = 10;

        public StatKind Stat => stat;
        public int Threshold => threshold;

        public bool IsMet(int[] nowStats)
        {
            int idx = stat switch
            {
                StatKind.AT => 0,
                StatKind.DF => 1,
                StatKind.AGI => 2,
                StatKind.MAT => 3,
                StatKind.MDF => 4,
                _ => -1
            };
            if (idx < 0 || nowStats == null || nowStats.Length <= idx) return false;
            return nowStats[idx] >= threshold;
        }
    }

    // ================================================================
    // StatBonus：転生内ステータス即時加算（MaxStats直接加算）
    // ================================================================
    [Serializable]
    public class StatBonus
    {
        public StatKind stat = StatKind.AT;
        public int value = 0;

        public StatKind Stat => stat;
        public int Value => value;

        public int StatIndex => stat switch
        {
            StatKind.AT => 0,
            StatKind.DF => 1,
            StatKind.AGI => 2,
            StatKind.MAT => 3,
            StatKind.MDF => 4,
            _ => -1
        };
    }

    // ================================================================
    // StatPrerequisiteType（旧API互換で残す）
    // ================================================================
    public enum StatPrerequisiteType { AtLeast }

    // ================================================================
    // StatWeightConfig：statに応じた出現重み変化
    // ================================================================
    [Serializable]
    public class StatWeightConfig
    {
        public StatKind stat = StatKind.AT;
        public string sign = "+"; // "+" or "-"

        public StatKind Stat => stat;
        public string Sign => sign;

        public int StatIndex => stat switch
        {
            StatKind.AT => 0,
            StatKind.DF => 1,
            StatKind.AGI => 2,
            StatKind.MAT => 3,
            StatKind.MDF => 4,
            _ => -1
        };
    }

    // ================================================================
    // MinYearsAfterEntry：経過年数条件
    // ================================================================
    [Serializable]
    public class MinYearsAfterEntry
    {
        public string eventId;
        public int minYears = 1;

        public string EventId => eventId;
        public int MinYears => minYears;
    }

    // ================================================================
    // LifeTagWeightBonus：LifeTag保有時の出現重みボーナス
    // ================================================================
    [Serializable]
    public class LifeTagWeightBonus
    {
        public string lifeTag;
        public float weightBonus = 0.1f;

        public string LifeTag => lifeTag;
        public float WeightBonus => weightBonus;
    }
}
