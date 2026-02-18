using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// 武器定義。効果スキルは SkillId を1つだけ持つ。
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "SteraCube/SpaceJourney/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        public string weaponId;
        public string displayName;

        [TextArea]
        public string description;

        [Header("効果スキル（SkillId / 1つ）")]
        public string effectSkillId;

        [Header("外見3D ID")]
        public string weaponVisual3dId;
    }
}
