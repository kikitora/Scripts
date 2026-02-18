using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// キューブ定義（ルール専用・ミニマム）
    /// - 盤面占有は常に1マス（特殊形状は扱わない）
    /// - 見た目差分（サンド/ストーン等）は CubeInstance 側で管理する
    /// - 反応トリガーは2種（隣接ボタン / 踏み）で、反応内容は1つだけ
    /// - 状態（使用済み/現在耐久など）は CubeInstance 側で保持する前提
    /// </summary>
    [CreateAssetMenu(menuName = "SteraCube/SpaceJourney/Cube Definition", fileName = "CubeDef_")]
    public class CubeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string cubeDefId = "cube_def_id";
        [SerializeField] private CubeKind cubeKind = CubeKind.Event;

        [Header("Board Rule")]
        [SerializeField] private CubeOccupancyType occupancyType = CubeOccupancyType.Blocks;

        [Tooltip("停止可能か（通過はできるが停止不可、など将来用）。")]
        [SerializeField] private bool canStopOn = true;

        [Header("Durability")]
        [Tooltip("最大耐久。0なら耐久なし/破壊不可扱い。")]
        [SerializeField] private int maxDurability = 0;

        [Header("Lifecycle")]
        [SerializeField] private CubeReusePolicy reusePolicy = CubeReusePolicy.Repeatable;

        [Tooltip("WAVEを跨いでも残る（門番/街など）。")]
        [SerializeField] private bool persistAcrossWaves = true;

        [Header("Reaction")]
        [SerializeField] private CubeReactionType reactionType = CubeReactionType.None;

        [Tooltip("隣接ボタン反応時：対象方向を向いている必要があるか")]
        [SerializeField] private bool requiresFacingForAdjacent = true;

        [Tooltip("反応内容（1つだけ）")]
        [SerializeField] private CubeActionSO action;

        // ---- public ----
        public string CubeDefId => cubeDefId;
        public CubeKind CubeKind => cubeKind;

        public CubeOccupancyType OccupancyType => occupancyType;
        public bool CanStopOn => canStopOn;

        public int MaxDurability => Mathf.Max(0, maxDurability);

        public CubeReusePolicy ReusePolicy => reusePolicy;
        public bool PersistAcrossWaves => persistAcrossWaves;

        public CubeReactionType ReactionType => reactionType;
        public bool RequiresFacingForAdjacent => requiresFacingForAdjacent;
        public CubeActionSO Action => action;

        // ---- convenience ----
        public bool IsBlocking => occupancyType == CubeOccupancyType.Blocks;
        public bool IsPassableOnTop => occupancyType == CubeOccupancyType.PassableOnTop;
        public bool IsOneShot => reusePolicy == CubeReusePolicy.OneShot;

        public bool IsEnterableByDefault() => occupancyType == CubeOccupancyType.PassableOnTop;
    }
}
