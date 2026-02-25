using System.Collections.Generic;
using UnityEngine;
using InspectorToolkit;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 1転生分の生成パラメータ。
    /// ランタイムの OneReinSoulData とは別の「定義用」軽量クラス。
    /// 空・None = 抽選。
    /// </summary>
    [System.Serializable]
    public class OneReinSoulConfig
    {
        [Tooltip("このキャラのランク。")]
        public int rank = 1;

        [Tooltip("None = 抽選。")]
        public GrowthType growthType = GrowthType.None;

        [Tooltip("空なら傾向から抽選。")]
        public string jobId;

        [Tooltip("空なら抽選。")]
        public string title;

        [Tooltip("0 = rank と同値で計算。")]
        public int level = 0;

        [Tooltip("AT,DF,AGI,MAT,MDF の順。空なら自動計算。")]
        public int[] lv1Stats;

        public List<string> learnedSkillIds = new List<string>();
    }

    /// <summary>
    /// ソウルの生成定義。
    /// 転生データは OneReinSoulConfig のリストで最大3件まで定義可能。
    /// ソウル本体のパラメータ（名前・傾向など）は別途オプションで指定。
    /// </summary>
    [CreateAssetMenu(menuName = "SteraCube/SpaceJourney/Chara/Soul Definition", fileName = "SoulDef_")]
    public class SoulDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string definitionId;
        [TextArea]
        [SerializeField] private string description;

        // ─── 転生データ（必須：最低1件） ──────────────────────
        [Header("転生データ（最低1件、最大3件）")]
        [Tooltip("reinConfigs[0] が初期状態、[1][2] と順に転生済みの状態で登場する。")]
        [SerializeField]
        private List<OneReinSoulConfig> reinConfigs = new List<OneReinSoulConfig>
        {
            new OneReinSoulConfig()  // デフォルトで1件
        };

        // ─── ペアリング ───────────────────────────────────────
        [Header("ペアリング（空 = ボディなしで登場）")]
        [Tooltip("このソウルと対になるボディ定義。null なら装備なしで生成される。")]
        [SerializeField] private BodyDefinitionSO bodyDefinition;

        // ─── ソウル本体オプション（空・None = 抽選） ─────────
        [Header("ソウル本体オプション（空・None = 抽選）")]

        [Tooltip("空なら抽選。")]
        [SerializeField] private string soulName;

        [Tooltip("空なら抽選。")]
        [SerializeField] private string iconId;

        [Tooltip("None = 抽選。")]
        [SerializeField] private SoulType soulType = SoulType.None;

        [Tooltip("None = 抽選。")]
        [SerializeField] private TalentRank talentRank = TalentRank.None;

        [Tooltip("None = 抽選。")]
        [SerializeField] private SoulJobTendency jobTendency = SoulJobTendency.None;

        [Tooltip("None = 抽選。")]
        [SerializeField] private FaceSexCategory sex = FaceSexCategory.None;

        // ─── プロパティ ───────────────────────────────────────
        public string DefinitionId => definitionId;
        public BodyDefinitionSO BodyDefinition => bodyDefinition;

        /// <summary>
        /// 定義に従って SoulInstance を生成して返す。
        /// reinConfigs が複数ある場合は複数転生済みの状態で生成する。
        /// </summary>
        public SoulInstance CreateSoulInstance()
        {
            if (reinConfigs == null || reinConfigs.Count == 0)
            {
                Debug.LogError($"[SoulDefinitionSO] {name}: reinConfigs が空です。最低1件必要です。");
                return null;
            }

            var db = MasterDatabase.Instance;

            // reinConfigs が1件 → SoulFactory.Create() に直接引数を渡す
            if (reinConfigs.Count == 1)
            {
                var cfg = reinConfigs[0];
                int level = cfg.level <= 0 ? cfg.rank : cfg.level;

                return SoulFactory.Create(
                    rank: cfg.rank,
                    soulName: string.IsNullOrEmpty(soulName) ? null : soulName,
                    iconId: string.IsNullOrEmpty(iconId) ? null : iconId,
                    soulType: soulType == SoulType.None ? (SoulType?)null : soulType,
                    talentRank: talentRank == TalentRank.None ? (TalentRank?)null : talentRank,
                    soulTendency: jobTendency == SoulJobTendency.None ? (SoulJobTendency?)null : jobTendency,
                    jobId: string.IsNullOrEmpty(cfg.jobId) ? null : cfg.jobId,
                    title: string.IsNullOrEmpty(cfg.title) ? null : cfg.title,
                    growthType: cfg.growthType == GrowthType.None ? (GrowthType?)null : cfg.growthType,
                    sex: sex == FaceSexCategory.None ? (FaceSexCategory?)null : sex,
                    level: level,
                    lv1Stats: (cfg.lv1Stats != null && cfg.lv1Stats.Length == 5) ? cfg.lv1Stats : null,
                    learnedSkillIds: cfg.learnedSkillIds?.Count > 0 ? cfg.learnedSkillIds : null
                );
            }

            // reinConfigs が複数 → 転生済みリストを構築して渡す
            var reinList = new List<OneReinSoulData>();
            foreach (var cfg in reinConfigs)
            {
                if (db == null) break;
                int level = cfg.level <= 0 ? cfg.rank : cfg.level;

                var jobDef = string.IsNullOrEmpty(cfg.jobId)
                    ? null : db.GetSoulJobById(cfg.jobId);

                var finalTalent = talentRank == TalentRank.None ? TalentRank.C : talentRank;
                var finalGrowth = cfg.growthType == GrowthType.None ? GrowthType.Normal : cfg.growthType;

                reinList.Add(OneReinSoulData.CreateFromArgs(
                    rank: cfg.rank,
                    growthType: finalGrowth,
                    jobDef: jobDef,
                    talent: finalTalent,
                    title: string.IsNullOrEmpty(cfg.title) ? null : cfg.title,
                    level: level,
                    lv1Stats: (cfg.lv1Stats != null && cfg.lv1Stats.Length == 5) ? cfg.lv1Stats : null,
                    growthTargets: null,
                    permanentBonuses: null,
                    historyEvents: null,
                    learnedSkillIds: cfg.learnedSkillIds?.Count > 0 ? cfg.learnedSkillIds : null
                ));
            }

            return SoulFactory.Create(
                rank: reinConfigs[0].rank,
                soulName: string.IsNullOrEmpty(soulName) ? null : soulName,
                iconId: string.IsNullOrEmpty(iconId) ? null : iconId,
                soulType: soulType == SoulType.None ? (SoulType?)null : soulType,
                talentRank: talentRank == TalentRank.None ? (TalentRank?)null : talentRank,
                soulTendency: jobTendency == SoulJobTendency.None ? (SoulJobTendency?)null : jobTendency,
                sex: sex == FaceSexCategory.None ? (FaceSexCategory?)null : sex,
                reinSouls: reinList
            );
        }
    }
}