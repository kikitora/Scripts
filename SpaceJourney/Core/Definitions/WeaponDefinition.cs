using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 武器定義。効果スキルは SkillDefinition を直接参照する。
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "SteraCube/SpaceJourney/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        public string weaponId;
        public string displayName;

        [TextArea]
        public string description;

        [Header("効果スキル")]
        [Tooltip("この武器が付与するスキル。SkillDefinition SO を直接登録する。")]
        public SkillDefinition effectSkill;

        [Header("外見3D ID")]
        public string weaponVisual3dId;

        [Header("出現ランク")]
        [Tooltip("この武器が候補に入る最低ボディランク。ボディのランクがこれ未満の場合は選ばれない。")]
        [Min(1)]
        public int minRank = 1;
    }
}