using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// 盤面移動テストドライバ。
    /// - boardRoot を「各種キューブの親」として登録できる（未設定なら自動生成）
    /// - boardRoot は (0,0,0) に配置し、盤面セル(0,0)のワールド位置＝boardRoot位置とする
    /// - プレイヤー（target）はシーン上のものを使用（Prefab生成しない）
    /// - Blocker / Floor の置物だけ Prefab から生成し boardRoot配下に配置する
    /// - 移動の実処理は CubeUnit 側の合成APIを呼ぶ
    ///
    /// 変更点：
    /// - CubeUnit の向きが FacingIndex(int) から Dir(enum) に変わったため、こちらも Dir で統一
    /// - 移動方向へ回転→移動 では TryRotateToFacingThenMove(nextWorld, movedDir) を呼ぶ
    /// - 周囲ログは target.FacingDir を基準に「前/右/後/左」を判定する
    /// </summary>
    public class CubeUnitBoardMoveAutoTestDriver : MonoBehaviour
    {
        public enum MovementMode
        {
            FacingFixed_MoveOnly = 0,          // 向き固定移動（回転しない）
            RotateToMoveDir_ThenMove = 1,      // 移動方向へ回転→移動
        }

        [Header("Target (scene object)")]
        [SerializeField] private CubeUnit target;

        [Header("Board Root (parent of all cubes)")]
        [Tooltip("各種キューブの親。ここを(0,0,0)に置き、セル(0,0)のワールド位置にする。空なら自動生成。")]
        [SerializeField] private Transform boardRoot;

        [Tooltip("起動時に target を盤面の開始セルへワープさせる（Tweenなし）。")]
        [SerializeField] private bool snapTargetToStartCellOnStart = true;

        [Header("Visual Prefabs (static props)")]
        [SerializeField] private GameObject blockerPrefab;
        [SerializeField] private GameObject floorPrefab;

        [Header("Visual Root")]
        [Tooltip("生成した置物をまとめる親。空なら boardRoot 配下に自動生成。")]
        [SerializeField] private Transform visualRoot;

        [Header("Movement")]
        [SerializeField] private MovementMode movementMode = MovementMode.RotateToMoveDir_ThenMove;

        [Header("Test Settings")]
        [SerializeField] private int steps = 150;
        [SerializeField] private float pauseSec = 0.15f;

        [Header("Logging")]
        [SerializeField] private bool logBlockedAroundPlayer = true;
        [SerializeField] private bool logEveryStepEvenIfNothingBlocked = false;

        [Header("Random Placement")]
        [SerializeField] private bool placeRandomBlockers = true;
        [SerializeField] private int blockerCount = 6;

        [SerializeField] private bool placeRandomFloors = true;
        [SerializeField] private int floorCount = 6;

        private FieldBoard _board;

        private const string ActorId = "PLAYER_ACTOR";

        // 9x9（外周壁）なので中心は(4,4)／実質7x7の中心でもある
        private readonly Vector2Int _startBoard = new Vector2Int(4, 4);

        private float StepDistance => SpaceJourneyConstants.CubeSideGridSize;

        private readonly Dictionary<string, GameObject> _visualById = new Dictionary<string, GameObject>();

        private void Start()
        {
            if (target == null)
            {
                Debug.LogWarning("[CubeUnitBoardMoveAutoTestDriver] target が未設定です。");
                return;
            }

            EnsureBoardRootAtOrigin();
            EnsureVisualRoot();

            _board = FieldBoard.CreateNormal9x9WithBorderWalls();

            if (!_board.TryPlaceActor(ActorId, CubeKind.Player, _startBoard.x, _startBoard.y))
            {
                Debug.LogError("[CubeUnitBoardMoveAutoTestDriver] Actor配置に失敗しました。");
                return;
            }

            // ★起動時にプレイヤーを開始セルへ合わせる（盤面セル(0,0)=boardRoot）
            if (snapTargetToStartCellOnStart)
            {
                Vector3 p = target.transform.position;
                Vector3 startWorld = BoardToWorld(_startBoard, baseY: p.y);
                target.transform.position = startWorld;

                // 起動直後に見た目の向きを確定したいならここで（任意）
                // target.SetFacingDirImmediate(target.FacingDir);
            }

            if (placeRandomBlockers) PlaceRandomBlockers(blockerCount);
            if (placeRandomFloors) PlaceRandomFloors(floorCount);

            StartCoroutine(CoRun());
        }

        private void EnsureBoardRootAtOrigin()
        {
            if (boardRoot == null)
            {
                var go = new GameObject("[BoardRoot]");
                boardRoot = go.transform;
            }

            boardRoot.position = Vector3.zero;
            boardRoot.rotation = Quaternion.identity;
        }

        private void EnsureVisualRoot()
        {
            if (visualRoot != null) return;

            var root = new GameObject("[Board Visual Root]");
            root.transform.SetParent(boardRoot != null ? boardRoot : this.transform, false);
            visualRoot = root.transform;
        }

        private IEnumerator CoRun()
        {
            Debug.Log($"[BoardMoveTest] mode={movementMode} startBoard={_startBoard} boardRootPos={boardRoot.position}");

            for (int i = 0; i < steps; i++)
            {
                if (!_board.TryGetActorPos(ActorId, out var pos))
                    yield break;

                if (logBlockedAroundPlayer)
                    LogBlockedAroundFacing(pos, stepIndex: i);

                var movableDirs = GetMovableDirs(pos);
                if (movableDirs.Count == 0)
                {
                    Debug.Log($"[BoardMoveTest] No movable dirs (stuck). pos={pos} step={i}/{steps}");
                    yield break;
                }

                var dir = movableDirs[Random.Range(0, movableDirs.Count)];
                var result = _board.TryMoveActor(ActorId, dir);

                if (result.moved)
                {
                    float baseY = target.transform.position.y;
                    Vector3 nextWorld = BoardToWorld(result.to, baseY);
                    yield return CoMoveTargetByMode(nextWorld, dir);
                }

                if (pauseSec > 0f)
                    yield return new WaitForSeconds(pauseSec);
            }
        }

        private IEnumerator CoMoveTargetByMode(Vector3 nextWorld, Dir movedDir)
        {
            if (movementMode == MovementMode.FacingFixed_MoveOnly)
            {
                while (!target.TryMoveFacingFixed(nextWorld))
                    yield return null;

                while (target.IsBusy)
                    yield return null;

                yield break;
            }

            // ★ここが修正点：int ではなく Dir を渡す
            while (!target.TryRotateToFacingThenMove(nextWorld, movedDir))
                yield return null;

            while (target.IsBusy)
                yield return null;
        }

        // ─────────────────────────────────────────────────────────────
        // 「プレイヤー目線」通行不可ログ（CubeUnit.FacingDir基準）
        // ─────────────────────────────────────────────────────────────

        private void LogBlockedAroundFacing(Vector2Int playerPos, int stepIndex)
        {
            Dir facing = target != null ? target.FacingDir : Dir.North;

            Dir front = facing;
            Dir right = TurnRight(facing);
            Dir back = TurnRight(right);
            Dir left = TurnLeft(facing);

            var checks = new (Dir dir, string label)[]
            {
                (front, "前"),
                (right, "右"),
                (back,  "後"),
                (left,  "左"),
            };

            bool anyBlocked = false;
            var sb = new StringBuilder();
            sb.Append($"[Around] step={stepIndex} pos={playerPos} facing={facing}  ");

            for (int i = 0; i < checks.Length; i++)
            {
                var (dir, label) = checks[i];
                var info = GetPassabilityInfo(playerPos, dir);

                if (!info.canEnter)
                {
                    anyBlocked = true;
                    sb.Append($"{label}に通行不可: {info.reason}");

                    if (!string.IsNullOrEmpty(info.blockerId))
                        sb.Append($" ({info.blockerKind}:{info.blockerId})");

                    sb.Append(" / ");
                }
            }

            if (anyBlocked || logEveryStepEvenIfNothingBlocked)
            {
                if (!anyBlocked)
                    sb.Append("周囲は通行不可なし");

                Debug.Log(sb.ToString());
            }
        }

        private struct PassabilityInfo
        {
            public bool canEnter;
            public string reason;
            public string blockerId;
            public CubeKind blockerKind;
        }

        private PassabilityInfo GetPassabilityInfo(Vector2Int from, Dir dir)
        {
            Vector2Int to = from + DirToDelta(dir);

            var cell = _board.GetCell(to.x, to.y);
            if (cell == null) return new PassabilityInfo { canEnter = false, reason = "盤外" };
            if (!cell.IsValid) return new PassabilityInfo { canEnter = false, reason = "無効マス" };
            if (cell.TerrainBlocked) return new PassabilityInfo { canEnter = false, reason = "地形ブロック" };
            if (cell.HasActor) return new PassabilityInfo { canEnter = false, reason = "他Actor" };

            if (cell.HasBlocker)
            {
                return new PassabilityInfo
                {
                    canEnter = false,
                    reason = "通行不可キューブ",
                    blockerId = cell.BlockerCubeId,
                    blockerKind = cell.BlockerKind
                };
            }

            return new PassabilityInfo { canEnter = true, reason = "通行可" };
        }

        // ─────────────────────────────────────────────────────────────
        // Board -> World
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 盤面セル(x,y)のワールド座標。
        /// boardRoot をセル(0,0)の基準位置とする。
        /// </summary>
        private Vector3 BoardToWorld(Vector2Int boardPos, float baseY)
        {
            Vector3 origin = boardRoot != null ? boardRoot.position : Vector3.zero;

            return new Vector3(
                origin.x + boardPos.x * StepDistance,
                baseY,
                origin.z + boardPos.y * StepDistance
            );
        }

        // ─────────────────────────────────────────────────────────────
        // Moveable dirs
        // ─────────────────────────────────────────────────────────────

        private List<Dir> GetMovableDirs(Vector2Int pos)
        {
            var list = new List<Dir>(4);
            TryAdd(pos, Dir.North, list);
            TryAdd(pos, Dir.East, list);
            TryAdd(pos, Dir.South, list);
            TryAdd(pos, Dir.West, list);
            return list;
        }

        private void TryAdd(Vector2Int from, Dir dir, List<Dir> list)
        {
            Vector2Int to = from + DirToDelta(dir);
            var cell = _board.GetCell(to.x, to.y);
            if (cell == null) return;

            if (cell.IsEnterableByDefault() && !cell.HasActor)
                list.Add(dir);
        }

        private static Vector2Int DirToDelta(Dir dir)
        {
            return dir switch
            {
                Dir.North => new Vector2Int(0, 1),
                Dir.East => new Vector2Int(1, 0),
                Dir.South => new Vector2Int(0, -1),
                Dir.West => new Vector2Int(-1, 0),
                _ => Vector2Int.zero
            };
        }

        private static Dir TurnRight(Dir dir)
        {
            return dir switch
            {
                Dir.North => Dir.East,
                Dir.East => Dir.South,
                Dir.South => Dir.West,
                Dir.West => Dir.North,
                _ => Dir.North
            };
        }

        private static Dir TurnLeft(Dir dir)
        {
            return dir switch
            {
                Dir.North => Dir.West,
                Dir.West => Dir.South,
                Dir.South => Dir.East,
                Dir.East => Dir.North,
                _ => Dir.North
            };
        }

        // ─────────────────────────────────────────────────────────────
        // Random placement (visual + board)
        // ─────────────────────────────────────────────────────────────

        private void PlaceRandomBlockers(int count)
        {
            int placed = 0;
            int guard = 300;

            while (placed < count && guard-- > 0)
            {
                int x = Random.Range(1, 8);
                int y = Random.Range(1, 8);

                if (x == _startBoard.x && y == _startBoard.y)
                    continue;

                var cell = _board.GetCell(x, y);
                if (cell == null) continue;

                if (!cell.CanEnterTerrainOnly()) continue;
                if (cell.HasBlocker || cell.HasActor) continue;

                string id = $"BLOCKER_{placed}";
                if (_board.TryPlaceBlocker(id, CubeKind.Event, x, y))
                {
                    SpawnPropIfNeeded(id, blockerPrefab, new Vector2Int(x, y));
                    placed++;
                }
            }
        }

        private void PlaceRandomFloors(int count)
        {
            int placed = 0;
            int guard = 300;

            while (placed < count && guard-- > 0)
            {
                int x = Random.Range(1, 8);
                int y = Random.Range(1, 8);

                if (x == _startBoard.x && y == _startBoard.y)
                    continue;

                var cell = _board.GetCell(x, y);
                if (cell == null) continue;

                if (!cell.CanEnterTerrainOnly()) continue;
                if (cell.HasFloor) continue;

                string id = $"FLOOR_{placed}";
                if (_board.TryPlaceFloor(id, CubeKind.Event, x, y))
                {
                    SpawnPropIfNeeded(id, floorPrefab, new Vector2Int(x, y));
                    placed++;
                }
            }
        }

        private void SpawnPropIfNeeded(string id, GameObject prefab, Vector2Int boardPos)
        {
            if (prefab == null) return;
            if (_visualById.ContainsKey(id)) return;

            float baseY = target != null ? target.transform.position.y : 0f;
            Vector3 world = BoardToWorld(boardPos, baseY);

            var go = Instantiate(prefab, world, Quaternion.identity, visualRoot);
            go.name = $"[CellObj] {id}";
            _visualById[id] = go;
        }
    }
}
