using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// キューブ定義（ScriptableObject）
    /// - ルール情報（盤面占有・反応トリガー等）
    /// - Visual情報（どのcubeGroundEntry / cubeEntryを使うか）
    /// - 状態（現在VP・疲労度など）は CubeInstance 側で保持する前提
    /// MasterDatabase の cubeDefinitions に登録して使う。
    /// </summary>
    [CreateAssetMenu(menuName = "SteraCube/SpaceJourney/Cube Definition", fileName = "CubeDef_")]
    public class CubeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string cubeDefId = "cube_def_id";
        [SerializeField] private CubeKind cubeKind = CubeKind.Event;

        [Header("説明（UI表示用）")]
        [TextArea]
        [SerializeField] private string description;

        [Header("Visual")]
        [Tooltip("MasterDatabase.cubeEntries に登録されているID。キューブ上の建物。空なら建物なし。")]
        [SerializeField] private string cubeEntryId;

        [Header("Board Rule")]
        [SerializeField] private CubeOccupancyType occupancyType = CubeOccupancyType.Blocks;

        [Header("Reaction")]
        [Tooltip("None=反応なし / AdjacentButton=隣接ボタン / StepOn=踏んだとき（将来拡張）")]
        [SerializeField] private CubeReactionType reactionType = CubeReactionType.None;

        [Tooltip("反応したときの処理（TreasureActionSO / HealActionSO 等を差し替え）")]
        [SerializeField] private CubeActionSO action;


        // ---- public ----
        public string CubeDefId => cubeDefId;
        public CubeKind CubeKind => cubeKind;
        public string Description => description;
        public string CubeEntryId => cubeEntryId;

        public CubeOccupancyType OccupancyType => occupancyType;

        public CubeReactionType ReactionType => reactionType;
        public CubeActionSO Action => action;


        // ---- convenience ----
        public bool IsBlocking => occupancyType == CubeOccupancyType.Blocks;
        public bool IsPassableOnTop => occupancyType == CubeOccupancyType.PassableOnTop;
        public bool IsEnemy => cubeKind == CubeKind.EnemyNormal
                                    || cubeKind == CubeKind.EnemyElite
                                    || cubeKind == CubeKind.EnemyBoss
                                    || cubeKind == CubeKind.GateKeeper
                                    || cubeKind == CubeKind.RareBoss;
    }
}