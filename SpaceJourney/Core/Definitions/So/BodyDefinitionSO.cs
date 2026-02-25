using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// ボディの生成定義。
    /// rank 以外はすべてオプション。未入力（null / 空）の場合は生成関数内で抽選される。
    /// </summary>
    [CreateAssetMenu(menuName = "SteraCube/SpaceJourney/Chara/Body Definition", fileName = "BodyDef_")]
    public class BodyDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string definitionId;
        [TextArea]
        [SerializeField] private string description;

        [Header("ランク（必須）")]
        [Tooltip("生成ランクの範囲。同値なら固定ランク。")]
        [SerializeField] private int minRank = 1;
        [SerializeField] private int maxRank = 1;

        [Header("オプション（空 = 抽選）")]
        [Tooltip("空なら MasterDatabase からランダム選択。")]
        [SerializeField] private string raceId;

        [Tooltip("空なら MasterDatabase からランダム選択。")]
        [SerializeField] private string bodyJobId;

        [Tooltip("空なら BodyJobDefinition.weaponCandidateIds から minRank を考慮して抽選。")]
        [SerializeField] private string weaponId;

        public string DefinitionId => definitionId;

        public BodyInstance CreateBodyInstance()
        {
            int rank = Random.Range(minRank, maxRank + 1);

            return BodyFactory.CreateRandom(
                rank: rank,
                raceId: string.IsNullOrEmpty(raceId) ? null : raceId,
                bodyJobId: string.IsNullOrEmpty(bodyJobId) ? null : bodyJobId,
                weaponId: string.IsNullOrEmpty(weaponId) ? null : weaponId
            );
        }
    }
}