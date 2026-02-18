using System;
using UnityEngine;
using System.Collections.Generic;

namespace SteraCube.SpaceJourney
{
    // SoulJobDefinition.cs
    // このクラスで何をするか：
    // SpaceJourneyモードにおける「ソウルジョブ」の設定データ（ScriptableObject）です。
    // AT / DF / AGI / MAT / MDF ごとの倍率（0.6?1.5倍程度）を持ち、
    // ランク別基礎値 rankBaseStat と組み合わせてソウルの基礎ステータスを決めるために使います。
    [CreateAssetMenu(
        fileName = "SoulJob",
        menuName = "SteraCube/SpaceJourney/Soul Job Definition")]
    public class SoulJobDefinition : ScriptableObject
    {
        [Header("傾向（どのボディ職向きか）")]
        [SerializeField] private SoulJobTendency tendency = SoulJobTendency.Warrior;

        [Header("識別情報")]
        [SerializeField] private string jobId;      // 内部ID（英数字）後で参照に使う用
        [SerializeField] private string jobName;    // 表示名
        [TextArea]
        [SerializeField] private string description;

        [Header("ステータス倍率（0.6?1.5倍くらいを想定）")]
        [Range(0.6f, 1.5f)] public float atMultiplier = 1.0f;
        [Range(0.6f, 1.5f)] public float dfMultiplier = 1.0f;
        [Range(0.6f, 1.5f)] public float agiMultiplier = 1.0f;
        [Range(0.6f, 1.5f)] public float matMultiplier = 1.0f;
        [Range(0.6f, 1.5f)] public float mdfMultiplier = 1.0f;

        [Header("ランク・ソウルジョブスキル")]
        [Tooltip("このソウルジョブの就きやすさ（0〜100）。高いほど選ばれやすい。")]
        [SerializeField, Range(0, 100)]
        private int jobEasePercent = 50;

        [Tooltip("このソウルジョブが特定ランクに到達したとき習得するアクティブスキルの一覧（Rank1/4/8 など）")]
        [SerializeField]
        private List<SoulJobSkillSet> skillSets = new List<SoulJobSkillSet>();
        /// <summary>
        /// ソウルジョブがランク到達時に習得するアクティブスキルのセット。
        /// 例：Rank1 / Rank4 / Rank8 で 3 つのスキルを登録するイメージ。
        /// </summary>
        [Serializable]
        public class SoulJobSkillSet
        {
            [Tooltip("このランクに到達したとき習得するソウルジョブスキルのランク（例：1,4,8）")]
            public int unlockRank;

            [Tooltip("このソウルジョブでランク到達時に習得するアクティブスキル")]
            public SkillDefinition skill;   // いつもの SkillDefinition を参照
        }
        /// <summary>
        /// StatKind ごとの倍率を取得するヘルパーです。
        /// SpaceJourneyStatMath.CalcBaseStat(rank, multiplier) に渡して使う想定。
        /// </summary>
        public float GetMultiplier(StatKind kind)
        {
            switch (kind)
            {
                case StatKind.AT: return atMultiplier;
                case StatKind.DF: return dfMultiplier;
                case StatKind.AGI: return agiMultiplier;
                case StatKind.MAT: return matMultiplier;
                case StatKind.MDF: return mdfMultiplier;
                default: return 1.0f;
            }
        }

        public string JobId => jobId;
        public string JobName => jobName;
        public string Description => description;

        public SoulJobTendency Tendency => tendency;
        /// <summary>
        /// このソウルジョブの就きやすさ（0〜100）。高いほど出現しやすい。
        /// 実際の抽選では、この値を重みとして扱う想定。
        /// </summary>
        public int JobEasePercent => jobEasePercent;

        /// <summary>
        /// ランク到達時に習得するソウルジョブスキルの一覧。
        /// 通常は Rank1 / Rank4 / Rank8 の 3 件を登録する。
        /// </summary>
        public IReadOnlyList<SoulJobSkillSet> SkillSets => skillSets;

    }
}
