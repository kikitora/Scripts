using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 転生シミュレーション上の「1つのライフイベント」を表すScriptableObject。
    /// ReinEventGraphEditorでノードとして表示・接続管理される。
    /// </summary>
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
        [Tooltip("内部ID。他のイベントからの参照はSO参照で行うのでユニーク性必須ではないが\n識別しやすい名前にすること")]
        [SerializeField] private string eventId;

        [Tooltip("グラフ・UIで表示する名前")]
        [SerializeField] private string displayName;

        [Tooltip("イベント発生時に画面に表示するテキスト\n例：「近所で火事を目撃する\n衝撃を受けた」")]
        [TextArea(2, 8)]
        [SerializeField] private string eventText;

        [TextArea(1, 3)]
        [SerializeField] private string editorMemo;

        // ============================================================
        // 出現条件（年齢・確率）
        // ============================================================
        [Header("出現条件")]
        [Tooltip("このイベントが出現しうる最小年齢（0=誕生時から）")]
        [SerializeField, Min(0)] private int startAge = 0;

        [Tooltip("このイベントが出現しうる最大年齢")]
        [SerializeField, Min(0)] private int endAge = 99;

        [Tooltip("出現重み。高いほど選ばれやすい（他イベントとの相対値）")]
        [SerializeField, Min(0f)] private float baseWeight = 1f;

        // ============================================================
        // 前提・排他条件（SO参照でグラフの矢印と同期）
        // ============================================================
        // ============================================================
        // 職業タグ
        // ============================================================
        [Header("職業タグ（空=全員共通イベント）")]
        [Tooltip("設定したジョブのいずれかが運命のジョブと一致する場合のみ発生する。\n" +
                 "空リストの場合は全ジョブ共通イベントとして発生しうる。")]
        [SerializeField] private List<SoulJobDefinition> relatedJobs = new();

        [Tooltip("ジョブが一致した場合に baseWeight に加算するボーナス重み")]
        [SerializeField, Min(0f)] private float jobMatchBonus = 1f;

        [Header("前提・排他条件")]
        [Tooltip("青矢印：これらのイベントが「過去に発生済み」でないと出現しない")]
        [SerializeField] private List<ReinLifeEventSO> requiresEvents = new();

        [Tooltip("赤矢印：これらのイベントが「発生済み」なら出現しない")]
        [SerializeField] private List<ReinLifeEventSO> blockedByEvents = new();

        [Header("ライフイベントタグ")]
        [Tooltip("このイベントが発生したときに付与するタグ\n例：\"婚活経験あり\" \"師匠あり\"")]
        [SerializeField] private List<string> grantsLifeTags = new();

        [Tooltip("このリストのタグを1つでも持っていれば出現可能（OR条件）\n空リストなら制限なし")]
        [SerializeField] private List<string> requiresAnyLifeTag = new();

        [Tooltip("このリストのタグをすべて持っていると出現しない（ブロック）\n例：\"結婚済み\"")]
        [SerializeField] private List<string> blockedByLifeTags = new();

        [Tooltip("このランク以上の場合のみ出現（0=制限なし）")]
        [SerializeField, Min(0)] private int requireMinRank = 0;

        [Tooltip("このランク以下の場合のみ出現（0=制限なし）")]
        [SerializeField, Min(0)] private int requireMaxRank = 0;

        [Tooltip("ステータス前提条件（AND：すべて満たす必要あり）")]
        [SerializeField] private List<StatPrerequisite> requireStatsAnd = new();

        [Tooltip("ステータス前提条件（OR：1つでも満たせば出現）")]
        [SerializeField] private List<StatPrerequisite> requireStatsOr = new();

        // ============================================================
        // イベント結果の選択肢
        // ============================================================
        [Header("結果の選択肢（複数登録→重み付き抽選）")]
        [SerializeField] private List<ReinSentenceOption> options = new();

        // ============================================================
        // プロパティ
        // ============================================================
        public string EventId => eventId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string EventText => eventText;
        public string EditorMemo => editorMemo;

        public int StartAge => startAge;
        public int EndAge => endAge;
        public float BaseWeight => baseWeight;

        /// <summary>職業タグが1つ以上設定されているか（falseなら全ジョブ共通）</summary>
        public bool HasJobTag => relatedJobs != null && relatedJobs.Count > 0;
        public IReadOnlyList<SoulJobDefinition> RelatedJobs => relatedJobs;
        public float JobMatchBonus => jobMatchBonus;

        /// <summary>
        /// 指定したジョブがこのイベントのタグと一致するか。
        /// タグなし（全共通）の場合は常にtrue。
        /// タグあり＋不一致の場合はfalse（発生しない）。
        /// </summary>
        public bool MatchesJob(SoulJobDefinition destinyJob)
        {
            if (!HasJobTag) return true;
            if (destinyJob == null) return false;
            return relatedJobs.Contains(destinyJob);
        }

        public IReadOnlyList<ReinLifeEventSO> RequiresEvents => requiresEvents;
        public IReadOnlyList<ReinLifeEventSO> BlockedByEvents => blockedByEvents;
        public IReadOnlyList<string> GrantsLifeTags => grantsLifeTags;
        public IReadOnlyList<string> RequiresAnyLifeTag => requiresAnyLifeTag;
        public IReadOnlyList<string> BlockedByLifeTags => blockedByLifeTags;
        public int RequireMinRank => requireMinRank;
        public int RequireMaxRank => requireMaxRank;
        public IReadOnlyList<ReinSentenceOption> Options => options;

        /// <summary>
        /// ランク・ステータス前提条件をすべてチェックする。
        /// </summary>
        /// <summary>タグ条件チェック（SimContext側から呼ぶ）</summary>
        public bool MeetsLifeTagConditions(HashSet<string> acquiredTags)
        {
            // requiresAnyLifeTag：1つでも持っていればOK（空なら無条件OK）
            if (requiresAnyLifeTag != null && requiresAnyLifeTag.Count > 0)
            {
                bool anyMet = false;
                foreach (var tag in requiresAnyLifeTag)
                    if (acquiredTags.Contains(tag)) { anyMet = true; break; }
                if (!anyMet) return false;
            }

            // blockedByLifeTags：1つでも持っていたらNG
            if (blockedByLifeTags != null)
                foreach (var tag in blockedByLifeTags)
                    if (acquiredTags.Contains(tag)) return false;

            return true;
        }

        public bool MeetsPrerequisites(int currentRank, int[] nowStats)
        {
            // ランク下限
            if (requireMinRank > 0 && currentRank < requireMinRank) return false;
            // ランク上限
            if (requireMaxRank > 0 && currentRank > requireMaxRank) return false;

            // AND条件：1つでも満たさなければNG
            if (requireStatsAnd != null)
            {
                foreach (var req in requireStatsAnd)
                    if (!req.IsMet(nowStats)) return false;
            }

            // OR条件：1つでも満たせばOK（リストが空なら無条件OK）
            if (requireStatsOr != null && requireStatsOr.Count > 0)
            {
                bool anyMet = false;
                foreach (var req in requireStatsOr)
                {
                    if (req.IsMet(nowStats)) { anyMet = true; break; }
                }
                if (!anyMet) return false;
            }

            return true;
        }

        // ============================================================
        // エディタ用ヘルパー（グラフウィンドウから呼ばれる）
        // ============================================================
#if UNITY_EDITOR
        /// <summary>requires矢印を追加（重複チェックあり）</summary>
        public void Editor_AddRequires(ReinLifeEventSO target)
        {
            if (target == null || target == this) return;
            if (!requiresEvents.Contains(target))
                requiresEvents.Add(target);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>requires矢印を削除</summary>
        public void Editor_RemoveRequires(ReinLifeEventSO target)
        {
            requiresEvents.Remove(target);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>blockedBy矢印を追加（重複チェックあり）</summary>
        public void Editor_AddBlockedBy(ReinLifeEventSO target)
        {
            if (target == null || target == this) return;
            if (!blockedByEvents.Contains(target))
                blockedByEvents.Add(target);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>blockedBy矢印を削除</summary>
        public void Editor_RemoveBlockedBy(ReinLifeEventSO target)
        {
            blockedByEvents.Remove(target);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// ノーマルイベントに職業固有スキルが登録されていたら警告。
        /// Inspector変更時に自動チェック。
        /// </summary>
        private void OnValidate()
        {
            // ランクアップ選択肢かどうかは選択肢側で判断
            // ここではgrantSkillsの中にユニバーサルでないスキルが混入していないかチェック
            if (options == null) return;
            foreach (var option in options)
            {
                if (option == null || option.IsRankUp) continue;
                if (option.GrantSkills == null) continue;
                foreach (var skill in option.GrantSkills)
                {
                    if (skill != null && !skill.IsUniversal)
                    {
                        UnityEngine.Debug.LogWarning(
                            $"[ReinLifeEventSO] {name} の選択肢にユニバーサルでないスキル {skill.SkillName} が登録されています。ノーマルイベントにはallowedBodyJobs空のスキルのみ登録可能です。",
                            this);
                    }
                }
            }
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

        [Header("EventFactorへの影響（各ステの補正値）")]
        [Tooltip("例：ATのeventFactorを+0.05する → AT delta = 0.05")]
        [SerializeField] private List<StatEffect> statEffects = new();

        [Header("ランクアップ")]
        [SerializeField] private bool isRankUp = false;

        [Tooltip("ランクアップ時に習得するスキル。職業固有スキルもここに登録可。")]
        [SerializeField] private List<SkillDefinition> rankUpSkills = new();

        [Header("スキル習得（ランクアップ以外）")]
        [Tooltip("ユニバーサルスキル（allowedBodyJobs空）のみ登録可。\n" +
                 "職業固有スキルはランクアップ選択肢のrankUpSkillに登録すること。")]
        [SerializeField] private List<SkillDefinition> grantSkills = new();

        [Header("ライフタグ付与（この選択肢が選ばれたとき）")]
        [Tooltip("この選択肢が選ばれたときに付与するライフタグ\n例：\"婚活経験あり\" \"結婚済み\"")]
        [SerializeField] private List<string> grantsLifeTags = new();

        // ============================================================
        // プロパティ
        // ============================================================
        public string Sentence => sentence;
        public float BaseWeight => baseWeight;
        public IReadOnlyList<StatCondition> StatConditions => statConditions;
        public IReadOnlyList<StatEffect> StatEffects => statEffects;
        public bool IsRankUp => isRankUp;
        public IReadOnlyList<SkillDefinition> RankUpSkills => rankUpSkills;
        public IReadOnlyList<SkillDefinition> GrantSkills => grantSkills;
        public IReadOnlyList<string> GrantsLifeTags => grantsLifeTags;

        /// <summary>
        /// シミュレーター側から最終的な重みを計算するために呼ぶ。
        /// currentEventFactors は AT=0,DF=1,AGI=2,MAT=3,MDF=4 の順。
        /// ここではeventFactorsではなく生ステ（lv1Stats換算など）を渡す想定。
        /// </summary>
        public float CalcFinalWeight(int[] currentStats)
        {
            float w = baseWeight;
            if (statConditions == null) return w;

            foreach (var cond in statConditions)
                w += cond.CalcWeightBonus(currentStats);

            return Mathf.Max(0f, w);
        }
    }

    // ================================================================
    // StatConditionType：条件の判定方式
    // ================================================================
    public enum StatConditionType
    {
        /// <summary>stat >= threshold なら weightBonus を加算</summary>
        AtLeast,
        /// <summary>stat × scalePerStat をそのまま加算（上限なし）</summary>
        Scales,
    }

    // ================================================================
    // StatCondition：ステータス条件
    // ================================================================
    [Serializable]
    public class StatCondition
    {
        [Tooltip("チェックするステータス（HPを除くAT?MDF）")]
        [SerializeField] private StatKind stat = StatKind.AT;

        [Tooltip("判定方式")]
        [SerializeField] private StatConditionType conditionType = StatConditionType.AtLeast;

        [Tooltip("[AtLeast] この値以上なら条件を満たす")]
        [SerializeField] private int threshold = 10;

        [Tooltip("[AtLeast] 条件を満たした場合に加算する重み")]
        [SerializeField] private float weightBonus = 1f;

        [Tooltip("[Scales] stat × scalePerStat が重みに加算される\n例：0.05 → AT=20なら+1.0、AT=40なら+2.0")]
        [SerializeField] private float scalePerStat = 0.05f;

        public StatKind Stat => stat;
        public StatConditionType ConditionType => conditionType;
        public int Threshold => threshold;
        public float WeightBonus => weightBonus;
        public float ScalePerStat => scalePerStat;

        /// <summary>
        /// currentStats（AT=0,DF=1,AGI=2,MAT=3,MDF=4）を受け取り、
        /// この条件が加算する重みを返す。条件を満たさない場合は0。
        /// </summary>
        public float CalcWeightBonus(int[] currentStats)
        {
            int idx = StatKindToIndex(stat);
            if (idx < 0 || currentStats == null || currentStats.Length <= idx) return 0f;

            int statVal = currentStats[idx];

            switch (conditionType)
            {
                case StatConditionType.AtLeast:
                    return statVal >= threshold ? weightBonus : 0f;

                case StatConditionType.Scales:
                    return statVal * scalePerStat;

                default:
                    return 0f;
            }
        }

        private static int StatKindToIndex(StatKind k) => k switch
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
    // StatEffect：EventFactorへの影響
    // ================================================================
    [Serializable]
    public class StatEffect
    {
        [Tooltip("影響するステータス（HPを除くAT?MDF）")]
        [SerializeField] private StatKind stat = StatKind.AT;

        [Tooltip("EventFactorへの加算ポイント\n小=1 / 中=3 / 大=5 / 特大=10")]
        [SerializeField] private int eventFactorPt = 1;

        public StatKind Stat => stat;
        public int EventFactorDelta => eventFactorPt;  // 名前は互換性のため維持

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
    // StatPrerequisiteType：ステータス前提条件の判定方式
    // ================================================================
    public enum StatPrerequisiteType
    {
        AtLeast, // stat >= threshold
        AtMost,  // stat <= threshold
    }

    // ================================================================
    // StatPrerequisite：イベント出現のステータス前提条件（AND/OR用）
    // ================================================================
    [Serializable]
    public class StatPrerequisite
    {
        [Tooltip("チェックするステータス")]
        [SerializeField] private StatKind stat = StatKind.AT;

        [Tooltip("AtLeast：この値以上 / AtMost：この値以下")]
        [SerializeField] private StatPrerequisiteType type = StatPrerequisiteType.AtLeast;

        [Tooltip("閾値")]
        [SerializeField] private int threshold = 10;

        public StatKind Stat => stat;
        public StatPrerequisiteType Type => type;
        public int Threshold => threshold;

        public bool IsMet(int[] nowStats)
        {
            if (nowStats == null) return false;
            int idx = stat switch
            {
                StatKind.AT => 0,
                StatKind.DF => 1,
                StatKind.AGI => 2,
                StatKind.MAT => 3,
                StatKind.MDF => 4,
                _ => -1
            };
            if (idx < 0 || idx >= nowStats.Length) return false;

            return type == StatPrerequisiteType.AtLeast
                ? nowStats[idx] >= threshold
                : nowStats[idx] <= threshold;
        }
    }
} // end namespace