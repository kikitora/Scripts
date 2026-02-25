// FieldManager.cs
// このクラスで何をするか：
// - FieldBoard（盤面データ）を生成・保持する管理役。
// - プレイヤーの初期位置をボードに登録する。
// - WaveSpawner など他のクラスに FieldBoard を提供する。
// シーン上に1つ置くだけで盤面が初期化される。

using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public class FieldManager : MonoBehaviour
    {
        // =========================================================
        // Inspector
        // =========================================================

        [Header("プレイヤー設定")]
        [Tooltip("プレイヤーキューブの初期X座標（1〜7）")]
        [SerializeField, Range(1, 7)] private int playerStartX = 4;

        [Tooltip("プレイヤーキューブの初期Y座標（1〜7）")]
        [SerializeField, Range(1, 7)] private int playerStartY = 4;

        [Header("連携")]
        [Tooltip("WaveSpawner。FieldBoard を渡すために参照する。")]
        [SerializeField] private WaveSpawner waveSpawner;

        [Header("デバッグ")]
        [SerializeField] private bool logOnInit = true;

        // =========================================================
        // 内部
        // =========================================================

        private FieldBoard _board;

        /// <summary>現在のFieldBoard。他クラスはここから取得する。</summary>
        public FieldBoard Board => _board;

        /// <summary>プレイヤーのアクターID（FieldBoard上の識別子）。</summary>
        public const string PlayerActorId = "PLAYER_ACTOR";

        // =========================================================
        // ライフサイクル
        // =========================================================

        private void Awake()
        {
            InitBoard();
        }

        // =========================================================
        // 初期化
        // =========================================================

        private void InitBoard()
        {
            // 9×9（外周壁込み）の標準フィールドを生成
            _board = FieldBoard.CreateNormal9x9WithBorderWalls();

            // プレイヤーを初期位置に配置
            bool placed = _board.TryPlaceActor(
                PlayerActorId,
                CubeKind.Player,
                playerStartX,
                playerStartY
            );

            if (!placed)
            {
                Debug.LogError($"[FieldManager] プレイヤーの配置に失敗しました。" +
                               $"座標({playerStartX},{playerStartY})を確認してください。");
            }
            else if (logOnInit)
            {
                Debug.Log($"[FieldManager] FieldBoard 初期化完了。" +
                          $"プレイヤー配置: ({playerStartX},{playerStartY})");
            }

            // WaveSpawnerにFieldBoardを渡す
            if (waveSpawner != null)
            {
                waveSpawner.SetFieldBoard(_board);
            }
            else
            {
                Debug.LogWarning("[FieldManager] WaveSpawner が未設定です。Inspectorで設定してください。");
            }
        }

        // =========================================================
        // 公開API
        // =========================================================

        /// <summary>
        /// プレイヤーの現在位置を取得する。
        /// </summary>
        public bool TryGetPlayerPos(out Vector2Int pos)
        {
            return _board.TryGetActorPos(PlayerActorId, out pos);
        }

        /// <summary>
        /// プレイヤーを1マス移動させる。
        /// </summary>
        public FieldBoard.MoveResult MovePlayer(Dir dir)
        {
            return _board.TryMoveActor(PlayerActorId, dir);
        }
    }
}
