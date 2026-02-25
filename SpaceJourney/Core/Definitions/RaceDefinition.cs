using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// 種族定義。IDで参照し、種族スキルは SkillDefinition を直接参照する。
    /// 追加：種族ごとのステータス倍率（体質差）と、職業別外見（アイコン・Prefab）を保持する。
    /// </summary>
    [CreateAssetMenu(fileName = "RaceDefinition", menuName = "SteraCube/SpaceJourney/Race Definition")]
    public class RaceDefinition : ScriptableObject
    {
        [Header("ID / 名前")]
        [Tooltip("内部参照用のユニークID（例: human, orc, ratfolk など）")]
        public string raceId;

        [Tooltip("ゲーム内で表示する名前")]
        public string displayName;

        [TextArea]
        public string description;

        [Header("種族スキル")]
        [Tooltip("この種族固有のパッシブスキル。SkillDefinition SO を直接登録する。")]
        public SkillDefinition racialSkill;

        [Header("出現ランク")]
        [Tooltip("この種族が候補に入る最低ボディランク。\n-1 = ユニーク（イベントなど特殊入手限定、通常抽選には出現しない）。")]
        public int minRank = 1;

        [Header("種族ステータス倍率（体質差）")]
        [Tooltip("この種族のHP倍率。1.0=等倍")]
        [Range(0.5f, 2.5f)] public float hpMul = 1.0f;

        [Tooltip("この種族のAT倍率。1.0=等倍")]
        [Range(0.5f, 2.5f)] public float atMul = 1.0f;

        [Tooltip("この種族のDF倍率。1.0=等倍")]
        [Range(0.5f, 2.5f)] public float dfMul = 1.0f;

        [Tooltip("この種族のAGI倍率。1.0=等倍")]
        [Range(0.5f, 2.5f)] public float agiMul = 1.0f;

        [Tooltip("この種族のMAT倍率。1.0=等倍")]
        [Range(0.5f, 2.5f)] public float matMul = 1.0f;

        [Tooltip("この種族のMDF倍率。1.0=等倍")]
        [Range(0.5f, 2.5f)] public float mdfMul = 1.0f;

        // =====================================================================
        // 職業別外見
        // =====================================================================

        /// <summary>
        /// この種族が各職業に就いたときのアイコンと3D Prefab のセット。
        /// </summary>
        [System.Serializable]
        public class JobAppearanceEntry
        {
            [Tooltip("対応するボディ職。BodyJobDefinition SO を直接登録する。")]
            public BodyJobDefinition job;

            [Tooltip("この種族×職業のボディアイコン（UI表示用）")]
            public Sprite bodyIcon;

            [Tooltip("この種族×職業の3D表示用 Prefab")]
            public GameObject bodyPrefab;
        }

        [Header("職業別外見（アイコン・3D Prefab）")]
        [Tooltip("BodyJobDefinition ごとにアイコンと3D Prefab を登録する。\n" +
                 "登録されていない職業は null を返す。")]
        public List<JobAppearanceEntry> jobAppearances = new();

        // =====================================================================
        // 取得ヘルパー
        // =====================================================================

        /// <summary>
        /// 指定した職業のボディアイコンを返す。登録がなければ null。
        /// </summary>
        public Sprite GetBodyIcon(BodyJobDefinition job)
        {
            if (job == null || jobAppearances == null) return null;
            foreach (var e in jobAppearances)
                if (e != null && e.job == job) return e.bodyIcon;
            return null;
        }

        /// <summary>
        /// 指定した職業の3D Prefab を返す。登録がなければ null。
        /// </summary>
        public GameObject GetBodyPrefab(BodyJobDefinition job)
        {
            if (job == null || jobAppearances == null) return null;
            foreach (var e in jobAppearances)
                if (e != null && e.job == job) return e.bodyPrefab;
            return null;
        }
    }
}