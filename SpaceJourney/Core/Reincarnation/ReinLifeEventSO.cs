using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    [CreateAssetMenu(
        fileName = "ReinLifeEvent_New",
        menuName = "SteraCube/SpaceJourney/Rein Life Event")]
    public class ReinLifeEventSO : ScriptableObject
    {
        // ============================================================
        // グラフエディタ用（ゲームロジックには不要）
        // ============================================================
        [HideInInspector] public Vector2 graphPosition = new Vector2(100, 100);

        // ============================================================
        // 基本情報
        // ============================================================
        [Header("基本情報")]
        [SerializeField] private string eventId;
        [SerializeField] private string displayName;
        [TextArea(1, 3)]
        [SerializeField] private string editorMemo;

        // ============================================================
        // 出現条件（年齢・重み）
        // ============================================================
        [Header("出現条件")]
        [SerializeField, Min(0)] private int startAge = 0;
        [SerializeField, Min(0)] private int endAge = 99;
        [SerializeField, Min(0f)] private float baseWeight = 1f;

        // ============================================================
        // ランク条件（0=制限なし）
        // ============================================================
        [Header("ランク条件（0=制限なし）")]
        [SerializeField, Min(0)] private int requireMinRank = 0;
        [SerializeField, Min(0)] private int requireMaxRank = 0;

        // ============================================================
        // ライフタグ条件
        // ============================================================
        [Header("ライフタグ条件")]
        [Tooltip("いずれか1つ以上持っていれば発生可。空=制限なし")]
        [SerializeField] private List<string> requiresAnyLifeTag = new();

        [Tooltip("いずれか1つでも持っていると発生しない")]
        [SerializeField] private List<string> blockedByLifeTags = new();

        [Tooltip("このイベント発生時に付与するライフタグ")]
        [SerializeField] private List<string> grantsLifeTags = new();

        // ============================================================
        // ステータス前提条件（AND）
        // ============================================================
        [Header("ステータス前提条件（AND、全部満たす必要あり）")]
        [SerializeField] private List<StatPrerequisite> requireStatsAnd = new();

        // ============================================================
        // 職業タグ（傾向フィルタ）
        // ============================================================
        [Header("職業タグ（空=全職業対象）")]
        [Tooltip("設定するとその傾向のジョブのときだけ発生・重みボーナス")]
        [SerializeField] private SoulJobTendency? jobTendency = null;
        [SerializeField] private bool hasTendency = false;
        [SerializeField] private SoulJobTendency tendency = SoulJobTendency.Warrior;
        [SerializeField, Min(0f)] private float jobMatchBonus = 0.5f;

        // ============================================================
        // SO参照による前提・排他条件
        // ============================================================
        [Header("イベント前提・排他（SO参照）")]
        [SerializeField] private List<string> requiresEventIds = new();
        [SerializeField] private List<string> blockedByEventIds = new();

        // ランクアップ用：特定ジョブSOに紐づく
        [Header("紐づくジョブSO（ランクアップイベント用）")]
        [SerializeField] private List<SoulJobDefinition> relatedJobs = new();

        // ============================================================
        // 選択肢
        // ============================================================
        [Header("結果の選択肢")]
        [SerializeField] private List<ReinSentenceOption> options = new();

        // ============================================================
        // プロパティ
        // ============================================================
        public string EventId => eventId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string EditorMemo => editorMemo;

        public int StartAge => startAge;
        public int EndAge => endAge;
        public float BaseWeight => baseWeight;

        public int RequireMinRank => requireMinRank;
        public int RequireMaxRank => requireMaxRank;

        public IReadOnlyList<string> RequiresAnyLifeTag => requiresAnyLifeTag;
        public IReadOnlyList<string> BlockedByLifeTags => blockedByLifeTags;
        public IReadOnlyList<string> GrantsLifeTags => grantsLifeTags;

        public IReadOnlyList<StatPrerequisite> RequireStatsAnd => requireStatsAnd;
        public IReadOnlyList<string> RequiresEventIds => requiresEventIds;
        public IReadOnlyList<string> BlockedByEventIds => blockedByEventIds;
        public IReadOnlyList<SoulJobDefinition> RelatedJobs => relatedJobs;
        public IReadOnlyList<ReinSentenceOption> Options => options;

        public bool HasJobTag => hasTendency;
        public SoulJobTendency JobTendency => tendency;
        public float JobMatchBonus => jobMatchBonus;

        // ============================================================
        // ランタイムチェックメソッド
        // ============================================================

        /// <summary>職業タグフィルタ。タグなし=全職業OK。</summary>
        public bool MatchesJob(SoulJobDefinition job)
        {
            if (!hasTendency) return true;
            if (job == null) return false;
            return job.Tendency == tendency;
        }

        /// <summary>ランク・ステータス・ライフタグの前提条件をまとめてチェック。</summary>
        public bool MeetsPrerequisites(
            int currentRank,
            int[] nowStats,
            HashSet<string> acquiredLifeTags = null)
        {
            // ランク下限
            if (requireMinRank > 0 && currentRank < requireMinRank) return false;
            // ランク上限
            if (requireMaxRank > 0 && currentRank > requireMaxRank) return false;

            // ライフタグ：必須（いずれか）
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

            // ライフタグ：排他
            if (blockedByLifeTags != null && acquiredLifeTags != null)
            {
                foreach (var tag in blockedByLifeTags)
                    if (acquiredLifeTags.Contains(tag)) return false;
            }

            // ステータス前提（AND）
            if (requireStatsAnd != null)
            {
                foreach (var req in requireStatsAnd)
                    if (!req.IsMet(nowStats)) return false;
            }

            return true;
        }

        // ============================================================
        // エディタ用ヘルパー
        // ============================================================
#if UNITY_EDITOR
        public void Editor_SetBasic(string id, string dispName, int sa, int ea, float bw)
        {
            eventId = id; displayName = dispName;
            startAge = sa; endAge = ea; baseWeight = bw;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_SetRankRange(int minR, int maxR)
        {
            requireMinRank = minR; requireMaxRank = maxR;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_SetLifeTags(List<string> reqAny, List<string> blocked, List<string> grants)
        {
            requiresAnyLifeTag = reqAny ?? new List<string>();
            blockedByLifeTags = blocked ?? new List<string>();
            grantsLifeTags = grants ?? new List<string>();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_SetStatPrereqs(List<StatPrerequisite> reqs)
        {
            requireStatsAnd = reqs ?? new List<StatPrerequisite>();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_SetTendency(bool has, SoulJobTendency tend, float bonus = 0.5f)
        {
            hasTendency = has; tendency = tend; jobMatchBonus = bonus;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_SetOptions(List<ReinSentenceOption> opts)
        {
            options = opts ?? new List<ReinSentenceOption>();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_AddRequiresEventId(string id)
        {
            if (string.IsNullOrEmpty(id) || id == eventId || requiresEventIds.Contains(id)) return;
            requiresEventIds.Add(id); UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_RemoveRequiresEventId(string id)
        {
            requiresEventIds.Remove(id); UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_AddBlockedByEventId(string id)
        {
            if (string.IsNullOrEmpty(id) || id == eventId || blockedByEventIds.Contains(id)) return;
            blockedByEventIds.Add(id); UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_RemoveBlockedByEventId(string id)
        {
            blockedByEventIds.Remove(id); UnityEditor.EditorUtility.SetDirty(this);
        }
        public void Editor_SetRelatedJobs(List<SoulJobDefinition> jobs)
        {
            relatedJobs = jobs ?? new List<SoulJobDefinition>();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    // ================================================================
    // ReinSentenceOption：イベント内の結果1件
    // ================================================================
    [Serializable]
    public class ReinSentenceOption
    {
        [Header("テキスト")]
        [TextArea(2, 6)]
        [SerializeField] private string sentence;

        [Header("抽選重み（基本値）")]
        [SerializeField, Min(0f)] private float baseWeight = 1f;

        [Header("ステータス条件（満たすと重みが上乗せされる）")]
        [SerializeField] private List<StatCondition> statConditions = new();

        [Header("EventFactorへの影響（各ステの補正pt）")]
        [Tooltip("小=1 / 中=3 / 大=5 / 特大=10")]
        [SerializeField] private List<StatEffect> statEffects = new();

        [Header("ランクアップ")]
        [SerializeField] private bool isRankUp = false;

        [Header("ランクアップ時に習得するスキル")]
        [SerializeField] private List<SkillDefinition> rankUpSkills = new();

        [Header("スキル習得（ランクアップ以外）")]
        [SerializeField] private List<SkillDefinition> grantSkills = new();

        [Header("付与するライフタグ")]
        [SerializeField] private List<string> grantsLifeTags = new();

        // ============================================================
        // プロパティ
        // ============================================================
        public string Sentence => sentence;
        public float BaseWeight => baseWeight;
        public bool IsRankUp => isRankUp;

        public IReadOnlyList<StatCondition> StatConditions => statConditions;
        public IReadOnlyList<StatEffect> StatEffects => statEffects;
        public IReadOnlyList<SkillDefinition> RankUpSkills => rankUpSkills;
        public IReadOnlyList<SkillDefinition> GrantSkills => grantSkills;
        public IReadOnlyList<string> GrantsLifeTags => grantsLifeTags;

        public float CalcFinalWeight(int[] currentStats)
        {
            float w = baseWeight;
            if (statConditions != null)
                foreach (var c in statConditions)
                    if (c.IsMet(currentStats)) w += c.WeightBonus;
            return Mathf.Max(0f, w);
        }
    }

    // ================================================================
    // StatCondition：重みに加算する stat 閾値条件
    // ================================================================
    [Serializable]
    public class StatCondition
    {
        [SerializeField] private StatKind stat = StatKind.AT;
        [SerializeField] private int threshold = 10;
        [SerializeField] private float weightBonus = 1f;

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
    // StatEffect：EventFactor への影響（int pt方式）
    // ================================================================
    [Serializable]
    public class StatEffect
    {
        [SerializeField] private StatKind stat = StatKind.AT;

        [Tooltip("EventFactorへの加算ポイント\n小=1 / 中=3 / 大=5 / 特大=10")]
        [SerializeField] private int eventFactorPt = 1;

        public StatKind Stat => stat;
        public int EventFactorDelta => eventFactorPt; // 名前は互換性のため維持

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
    // StatPrerequisite：発生前提のステータス条件（AND）
    // ================================================================
    [Serializable]
    public class StatPrerequisite
    {
        [SerializeField] private StatKind stat = StatKind.AT;
        [SerializeField] private int threshold = 10;

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
    // StatPrerequisiteType（旧API互換で残す）
    // ================================================================
    public enum StatPrerequisiteType { AtLeast }
}