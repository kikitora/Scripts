// FieldStageDefinitionSO.cs
// このファイルで何をするか：
// ノーマル／ボス／レアボスの各ステージをデータとして定義するScriptableObject。
// - Normal   : 7×7固定グリッド。WaveSpawnTableSO でランダムスポーン。GateKeeper撃破でクリア。
// - Boss     : 可変サイズ。キューブはすべてfixedPlacementsで手動配置。turnLimitを持つ。
// - RareBoss : Normalと同構造だが、RareBossキューブをfixedPlacementsで配置。

using UnityEngine;
using InspectorToolkit;

namespace SteraCube.SpaceJourney
{
    public enum StageType
    {
        Normal,
        Boss,
        RareBoss,
    }

    // =========================================================
    // 固定配置キューブ（Boss / RareBoss ステージで使用）
    // =========================================================

    [System.Serializable]
    public class FixedCubePlacement
    {
        [Tooltip("フィールド上のグリッド座標（0,0 = 左下）")]
        [SerializeField] private Vector2Int gridPosition;

        [Tooltip("配置するキューブの種別")]
        [SerializeField] private CubeKind cubeKind = CubeKind.EnemyBoss;

        [Tooltip("敵キューブの場合に参照するEnemyGroupDefinition。Rock等は不要。")]
        [SerializeField] private EnemyGroupDefinitionSO enemyGroupDefinition;

        [Tooltip("地面の見た目ID（MasterDatabase.cubeGroundEntries のID）")]
        [SerializeField] private string groundEntryId;

        public Vector2Int GridPosition => gridPosition;
        public CubeKind CubeKind => cubeKind;
        public EnemyGroupDefinitionSO EnemyGroupDefinition => enemyGroupDefinition;
        public string GroundEntryId => groundEntryId;
    }

    // =========================================================
    // ステージ定義本体
    // =========================================================

    [CreateAssetMenu(
        menuName = "SteraCube/SpaceJourney/Stage/Field Stage Definition",
        fileName = "StageDef_")]
    public class FieldStageDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string stageId;

        [TextArea]
        [SerializeField] private string description;

        // ─── ステージ種別 ──────────────────────────────────────
        [Header("ステージ種別")]
        [SerializeField] private StageType stageType = StageType.Normal;

        // ─── グリッドサイズ ────────────────────────────────────
        [Header("グリッドサイズ")]
        [Tooltip("Normal は 7×7 固定。Boss / RareBoss はここで指定。")]
        [SCShowIf(nameof(stageType), StageType.Normal, true)]
        [SerializeField] private int fieldWidth = 7;

        [SCShowIf(nameof(stageType), StageType.Normal, true)]
        [SerializeField] private int fieldHeight = 7;

        // ─── Normal 専用 ───────────────────────────────────────
        [Header("Normal 専用：スポーンテーブル")]
        [Tooltip("ランダムスポーンに使うWaveSpawnTable。NormalとRareBossで使用。")]
        [SCShowIfAny(nameof(stageType), StageType.Normal, StageType.RareBoss)]
        [SerializeField] private WaveSpawnTableSO waveSpawnTable;

        // ─── Boss 専用 ─────────────────────────────────────────
        [Header("Boss 専用：ターン制限")]
        [Tooltip("招集WAVEのターン制限。0は制限なし。")]
        [SCShowIf(nameof(stageType), StageType.Boss)]
        [SerializeField] private int turnLimit = 0;

        // ─── 固定配置（Boss / RareBoss） ──────────────────────
        [Header("固定配置キューブ（Boss / RareBoss）")]
        [Tooltip("Rockによるフィールド形状の定義、ボス本体、固定イベント等をここで配置。")]
        [SCShowIfAny(nameof(stageType), StageType.Boss, StageType.RareBoss)]
        [SerializeField] private FixedCubePlacement[] fixedPlacements;

        // ─── プロパティ ───────────────────────────────────────
        public string StageId => stageId;
        public StageType StageType => stageType;

        public int FieldWidth  => stageType == StageType.Normal ? 7 : fieldWidth;
        public int FieldHeight => stageType == StageType.Normal ? 7 : fieldHeight;

        public WaveSpawnTableSO WaveSpawnTable => waveSpawnTable;

        public int TurnLimit => turnLimit;

        public FixedCubePlacement[] FixedPlacements => fixedPlacements;

        /// <summary>
        /// クリア条件のキューブ種別を返す。
        /// Normal / RareBoss → GateKeeper
        /// Boss              → EnemyBoss
        /// </summary>
        public CubeKind ClearTargetKind
        {
            get
            {
                switch (stageType)
                {
                    case StageType.Boss:     return CubeKind.EnemyBoss;
                    case StageType.RareBoss: return CubeKind.RareBoss;
                    default:                 return CubeKind.GateKeeper;
                }
            }
        }
    }
}
