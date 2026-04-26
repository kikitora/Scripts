using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// ボディ職（= BodyJob）の定義。
    /// ボディ生成時に掛けるステータス倍率（float）と、基礎スキル・候補武器を保持する。
    /// </summary>
    [CreateAssetMenu(fileName = "BodyJobDefinition", menuName = "SteraCube/SpaceJourney/Body Job Definition")]
    public class BodyJobDefinition : ScriptableObject
    {
        [Header("ID / 名前")]
        public string bodyJobId;
        public string displayName;

        [TextArea]
        public string description;

        [Header("ジョブアイコン")]
        [Tooltip("職業を区別するためのアイコン。ボディアイコン（種族×職業）とは別物。")]
        [SerializeField] private Sprite jobIcon;
        /// <summary>ジョブアイコン Sprite。</summary>
        public Sprite Icon => jobIcon;

        [Header("ステータス倍率（1.0 = 等倍）")]
        [Range(0.3f, 3.0f)] public float hpMul = 1.0f;
        [Range(0.3f, 3.0f)] public float atMul = 1.0f;
        [Range(0.3f, 3.0f)] public float dfMul = 1.0f;
        [Range(0.3f, 3.0f)] public float agiMul = 1.0f;
        [Range(0.3f, 3.0f)] public float matMul = 1.0f;
        [Range(0.3f, 3.0f)] public float mdfMul = 1.0f;

        [Header("基礎スキル (旧ターン制)")]
        [Tooltip("旧ターン戦闘用。リアル戦闘では realtimeSkills を参照。")]
        public List<SkillDefinition> baseSkills = new();

        [Header("リアル戦闘用スキル")]
        [Tooltip("リアル戦闘で使うスキル。index 0 が基礎攻撃、以降が追加スキル。ActionEntry.actionSkillIndex はこの index を参照。")]
        public List<SteraCube.SpaceJourney.Realtime.RealtimeSkillDefinition> realtimeSkills = new();

        [Header("候補武器")]
        [Tooltip("この職が持ちうる武器候補。BodyFactory がランダム選択に使用する。\n" +
                 "WeaponDefinition SO を直接登録する。")]
        public List<WeaponDefinition> weaponCandidates = new();
    }
}