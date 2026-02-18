using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// ボディ職（= BodyJob）の定義。
    /// ボディ生成時に掛けるステータス倍率（float）と、基礎スキルIDを保持する。
    /// </summary>
    [CreateAssetMenu(fileName = "BodyJobDefinition", menuName = "SteraCube/SpaceJourney/Body Job Definition")]
    public class BodyJobDefinition : ScriptableObject
    {
        [Header("ID / 名前")]
        public string bodyJobId;
        public string displayName;

        [TextArea]
        public string description;

        [Header("ステータス倍率（1.0 = 等倍）")]
        [Range(0.3f, 3.0f)] public float hpMul = 1.0f;
        [Range(0.3f, 3.0f)] public float atMul = 1.0f;
        [Range(0.3f, 3.0f)] public float dfMul = 1.0f;
        [Range(0.3f, 3.0f)] public float agiMul = 1.0f;
        [Range(0.3f, 3.0f)] public float matMul = 1.0f;
        [Range(0.3f, 3.0f)] public float mdfMul = 1.0f;

        [Header("基礎スキル（SkillId一覧）")]
        public List<string> baseSkillIds = new();
    }
}
