using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 3×3グリッド上の1メンバー（ソウル＋ボディのペア）。
    /// </summary>
    [System.Serializable]
    public class EnemyMemberDefinition
    {
        [Tooltip("3×3グリッド上の位置（x:0-2, y:0-2）。")]
        [SerializeField] private Vector2Int gridPosition;

        [Tooltip("配置するソウルの定義。")]
        [SerializeField] private SoulDefinitionSO soulDefinition;

        [Tooltip("配置するボディの定義。")]
        [SerializeField] private BodyDefinitionSO bodyDefinition;

        public Vector2Int GridPosition => gridPosition;
        public SoulDefinitionSO SoulDefinition => soulDefinition;
        public BodyDefinitionSO BodyDefinition => bodyDefinition;
    }

    /// <summary>
    /// 敵キューブ1体分のキャラ配置定義。
    /// WaveSpawnTableSO や FixedPlacement から参照される。
    /// ランタイムで SoulInstance / BodyInstance を生成して CubeInstance に乗せる。
    /// </summary>
    [CreateAssetMenu(menuName = "SteraCube/SpaceJourney/Enemy/Enemy Group Definition", fileName = "EnemyGroup_")]
    public class EnemyGroupDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string groupId;

        [TextArea]
        [SerializeField] private string description;

        [Header("キューブ種別")]
        [SerializeField] private CubeKind cubeKind = CubeKind.EnemyNormal;

        [Header("敵キューブステータス")]
        [Tooltip("キューブ自体のVP（HP相当）。CubeInstanceで現在値を管理。")]
        [SerializeField] private int maxVp = 10;

        [Header("メンバー（3×3グリッド上の配置）")]
        [SerializeField] private EnemyMemberDefinition[] members;

        public string GroupId => groupId;
        public int MaxVp => maxVp;
        public CubeKind CubeKind => cubeKind;
        public EnemyMemberDefinition[] Members => members;
    }
}