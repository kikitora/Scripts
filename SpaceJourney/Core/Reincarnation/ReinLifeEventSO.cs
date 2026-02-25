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
        [Header("前提・排他条件")]
        [Tooltip("青矢印：これらのイベントが「過去に発生済み」でないと出現しない")]
        [SerializeField] private List<ReinLifeEventSO> requiresEvents = new();

        [Tooltip("赤矢印：これらのイベントが「発生済み」なら出現しない")]
        [SerializeField] private List<ReinLifeEventSO> blockedByEvents = new();

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
        public string EditorMemo => editorMemo;

        public int StartAge => startAge;
        public int EndAge => endAge;
        public float BaseWeight => baseWeight;

        public IReadOnlyList<ReinLifeEventSO> RequiresEvents => requiresEvents;
        public IReadOnlyList<ReinLifeEventSO> BlockedByEvents => blockedByEvents;
        public IReadOnlyList<ReinSentenceOption> Options => options;

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

        [Tooltip("ランクアップ時に習得するスキル（SoulJobDefinitionから移行）")]
        [SerializeField] private SkillDefinition rankUpSkill;

        [Header("スキル習得（ランクアップ以外）")]
        [SerializeField] private SkillDefinition grantSkill;

        // ============================================================
        // プロパティ
        // ============================================================
        public string Sentence => sentence;
        public float BaseWeight => baseWeight;
        public IReadOnlyList<StatCondition> StatConditions => statConditions;
        public IReadOnlyList<StatEffect> StatEffects => statEffects;
        public bool IsRankUp => isRankUp;
        public SkillDefinition RankUpSkill => rankUpSkill;
        public SkillDefinition GrantSkill => grantSkill;

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
            {
                if (cond.IsMet(currentStats))
                    w += cond.WeightBonus;
            }
            return Mathf.Max(0f, w);
        }
    }

    // ================================================================
    // StatCondition：ステータス条件
    // ================================================================
    [Serializable]
    public class StatCondition
    {
        [Tooltip("チェックするステータス（HPを除くAT?MDF）")]
        [SerializeField] private StatKind stat = StatKind.AT;

        [Tooltip("この値「以上」なら条件を満たす")]
        [SerializeField] private int threshold = 10;

        [Tooltip("条件を満たした場合に加算する重み")]
        [SerializeField] private float weightBonus = 1f;

        public StatKind Stat => stat;
        public int Threshold => threshold;
        public float WeightBonus => weightBonus;

        /// <summary>
        /// currentStats: AT=0, DF=1, AGI=2, MAT=3, MDF=4 の配列
        /// </summary>
        public bool IsMet(int[] currentStats)
        {
            int idx = StatKindToIndex(stat);
            if (idx < 0 || currentStats == null || currentStats.Length <= idx) return false;
            return currentStats[idx] >= threshold;
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

        [Tooltip("EventFactorへの加算量（例：0.05 → +5%）")]
        [SerializeField] private float eventFactorDelta = 0.05f;

        public StatKind Stat => stat;
        public float EventFactorDelta => eventFactorDelta;

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
}