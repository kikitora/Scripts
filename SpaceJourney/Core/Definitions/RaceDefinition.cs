using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// 種族定義。IDで参照し、種族スキルは SkillId を1つ持つ。
    /// 追加：種族ごとのステータス倍率（体質差）を保持する。
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

        [Header("種族スキル（SkillId / 1つ）")]
        public string racialSkillId;

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
    }
}
