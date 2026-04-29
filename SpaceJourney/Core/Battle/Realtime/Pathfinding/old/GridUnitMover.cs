using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>
    /// グリッド A* + ストリクトなセル単位移動。1セル1ユニット保証。
    /// セル予約方式：移動を始める時に next cell を「予約」(SetOccupant)。
    /// 同フレーム内で他ユニットがその cell を取れない → 重なり防止。
    /// </summary>
    public class GridUnitMover : MonoBehaviour
    {
        // ============== 公開 API ==============
        public Vector3 destination;
        public float endReachedDistance = 1.0f;
        public float maxSpeed = 1.0f;
        public bool simulateMovement = false;

        public Vector3 velocity { get; private set; }
        public bool reachedDestination { get; private set; }

        /// <summary>セル間移動中 (transit 中) なら true。攻撃を抑止したい時に参照。</summary>
        public bool IsInTransit => _hasNextCell;

        /// <summary>現在所有してるセル (transit 中は出発側)。距離計算等に。</summary>
        public Vector2Int CurrentCell => _currentCell;

        // 攻撃時の対象ユニットを path 障害物から除外する
        public RealtimeBattleUnit ignoreOccupant;

        // ============== 内部状態 ==============
        private RealtimeBattleUnit _self;
        private Vector2Int _currentCell = new(-1, -1);   // 現在所有してる cell
        private bool _hasNextCell;                        // transit 中フラグ
        private Vector2Int _nextCell;                     // 予約済 next cell

        private List<Vector2Int> _path;
        private int _pathIdx;
        private Vector2Int _lastDestCell = new(-1, -1);
        private float _lastReplanTime;
        private float _blockedSince = -1f;
        private bool _detourMode;       // true: 味方を障害物扱い (迂回 path)。false: 静的障害のみ (直進寄り path)

        private const float CellArriveDistance = 0.05f;   // セル中心からこの距離内なら到着扱い

        // 詰まり時の replan 待機時間。ユニットごとにランダム化して同時 replan を避ける
        // (全員同時に道を変えると同じ判断→また衝突→無限ゆらゆらの thundering herd 防止)
        private float _waitBeforeReplan;

        // ============== ライフサイクル ==============

        private void Awake()
        {
            // ユニットごとに詰まり時 replan 待機時間をランダム化 (0.3〜0.8s)。
            // 同時詰まり→同時 replan を避ける。
            _waitBeforeReplan = 0.3f + Random.Range(0f, 0.5f);
        }

        private void EnsureSelf()
        {
            if (_self == null) _self = GetComponent<RealtimeBattleUnit>();
        }

        private void OnEnable()
        {
            EnsureSelf();
            var grid = GridState.Active;
            if (grid != null && _self != null)
            {
                var cell = grid.WorldToCell(transform.position);
                if (cell.x >= 0)
                {
                    // 占有チェック (上書き防止)
                    var occ = grid.GetOccupant(cell);
                    if (occ != null && occ != _self)
                    {
                        var alt = FindNearestFreeCell(grid, cell);
                        if (alt.x >= 0) cell = alt;
                    }
                    _currentCell = cell;
                    grid.SetOccupant(_currentCell, _self);
                    Vector3 c = grid.CellToWorld(_currentCell);
                    c.y = transform.position.y;
                    transform.position = c;
                }
            }
        }

        private void OnDisable()
        {
            ReleaseAllReservations();
            _hasNextCell = false;
        }

        private void ReleaseAllReservations()
        {
            var grid = GridState.Active;
            if (grid == null) return;
            if (_currentCell.x >= 0 && grid.GetOccupant(_currentCell) == _self)
                grid.ClearOccupant(_currentCell);
            if (_hasNextCell && grid.GetOccupant(_nextCell) == _self)
                grid.ClearOccupant(_nextCell);
        }

        public void Teleport(Vector3 pos, bool clearVelocity = true)
        {
            ReleaseAllReservations();
            _hasNextCell = false;
            _path = null;
            _pathIdx = 0;
            _blockedSince = -1f;

            var grid = GridState.Active;
            if (grid != null)
            {
                EnsureSelf();
                var newCell = grid.WorldToCell(pos);
                if (newCell.x >= 0)
                {
                    // 占有チェック: 別ユニットが居たら近隣の空きセルへ寄せる
                    var occ = grid.GetOccupant(newCell);
                    if (occ != null && occ != _self)
                    {
                        var alt = FindNearestFreeCell(grid, newCell);
                        if (alt.x >= 0) newCell = alt;
                    }
                    _currentCell = newCell;
                    if (_self != null) grid.SetOccupant(_currentCell, _self);
                    Vector3 c = grid.CellToWorld(_currentCell);
                    c.y = pos.y;
                    transform.position = c;
                }
                else
                {
                    transform.position = pos;
                }
            }
            else
            {
                transform.position = pos;
            }

            if (clearVelocity) velocity = Vector3.zero;
        }

        /// <summary>from セルから BFS で最寄りの空き walkable セルを返す。見つからなければ (-1,-1)。</summary>
        private static Vector2Int FindNearestFreeCell(GridState grid, Vector2Int from)
        {
            var visited = new HashSet<Vector2Int> { from };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(from);
            var dirs = new[] {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1),
            };
            int maxIter = 100;
            while (queue.Count > 0 && maxIter-- > 0)
            {
                var cell = queue.Dequeue();
                if (cell != from && grid.IsStaticWalkable(cell) && grid.GetOccupant(cell) == null)
                    return cell;
                foreach (var d in dirs)
                {
                    var n = cell + d;
                    if (visited.Contains(n)) continue;
                    visited.Add(n);
                    if (grid.InBounds(n)) queue.Enqueue(n);
                }
            }
            return new Vector2Int(-1, -1);
        }

        public void StopMovement()
        {
            simulateMovement = false;
            velocity = Vector3.zero;
            // _hasNextCell はそのまま：transit 中なら現セルまで進んでから止まる
        }

        // ============== Update ==============

        private void Update()
        {
            var grid = GridState.Active;
            if (grid == null) { velocity = Vector3.zero; return; }

            EnsureSelf();
            if (_self == null) return;

            InitCellIfNeeded(grid);
            if (_currentCell.x < 0) return;

            // transit 中ならまず現在の transit を継続 (途中放棄はしない、cell 重複防止)
            if (_hasNextCell)
            {
                ContinueTransit(grid);
                return;
            }

            // 到達判定 (世界座標距離)
            Vector3 toDest = destination - transform.position; toDest.y = 0;
            reachedDestination = toDest.magnitude <= endReachedDistance;

            if (!simulateMovement || reachedDestination)
            {
                velocity = Vector3.zero;
                SnapToCurrentCell();
                return;
            }

            // 目標セル算出
            Vector2Int destCell = grid.WorldToCell(destination);
            if (!grid.InBounds(destCell))
            {
                velocity = Vector3.zero;
                return;
            }

            // path 計画 (必要時)
            if (NeedReplan(grid, destCell)) ReplanPath(grid, destCell);

            if (_path == null || _pathIdx >= _path.Count)
            {
                velocity = Vector3.zero;
                return;
            }

            // 次セル予約を試みる
            Vector2Int nextC = _path[_pathIdx];
            int adj = Mathf.Abs(nextC.x - _currentCell.x) + Mathf.Abs(nextC.y - _currentCell.y);
            if (adj != 1)
            {
                // path inconsistent (ワープ等で離れた) → 再計画
                _path = null;
                return;
            }

            // 予約検査 (ignoreOccupant 渡さない → target セルにも入らない)
            if (!grid.IsWalkableForUnit(nextC, _self, null))
            {
                if (_blockedSince < 0f) _blockedSince = Time.time;
                velocity = Vector3.zero;
                return;
            }

            // 予約 → transit 開始 (この同フレームで即動かしてビジュアル遅延ゼロに)
            grid.SetOccupant(nextC, _self);
            _nextCell = nextC;
            _hasNextCell = true;
            _blockedSince = -1f;

            // velocity を即セット (RealtimeBattleUnit の facing logic が同フレーム取得できるように)
            Vector3 nextWorld = grid.CellToWorld(_nextCell); nextWorld.y = transform.position.y;
            Vector3 dirNow = nextWorld - transform.position; dirNow.y = 0;
            if (dirNow.sqrMagnitude > 0.0001f)
                velocity = dirNow.normalized * maxSpeed;

            // 同フレームで実際に1ステップ進める
            ContinueTransit(grid);
        }

        // ============== セル初期化 ==============

        private void InitCellIfNeeded(GridState grid)
        {
            if (_currentCell.x >= 0) return;
            EnsureSelf();
            if (_self == null) return;
            var cell = grid.WorldToCell(transform.position);
            if (cell.x < 0)
            {
                Debug.LogWarning($"[GridUnitMover] {name}: spawn pos {transform.position} is out of grid");
                return;
            }
            // 既に他ユニットが居たら近隣の空きセルへずらす (上書き防止)
            var occ = grid.GetOccupant(cell);
            if (occ != null && occ != _self)
            {
                Debug.LogWarning($"[GridUnitMover] {name}: spawn cell {cell} already occupied by {occ.name}, finding alt");
                var alt = FindNearestFreeCell(grid, cell);
                if (alt.x >= 0) cell = alt;
                else Debug.LogError($"[GridUnitMover] {name}: no free cell near {cell}!");
            }
            _currentCell = cell;
            grid.SetOccupant(_currentCell, _self);
            Vector3 c = grid.CellToWorld(_currentCell);
            c.y = transform.position.y;
            transform.position = c;
            Debug.Log($"[GridUnitMover] {name}: registered at cell {_currentCell} (world {c:F2})");
        }

        // ============== Transit ==============

        private void ContinueTransit(GridState grid)
        {
            Vector3 nextWorld = grid.CellToWorld(_nextCell);
            nextWorld.y = transform.position.y;
            Vector3 cur = transform.position;
            Vector3 to = nextWorld - cur; to.y = 0;
            float d = to.magnitude;

            if (d < CellArriveDistance)
            {
                // 到着 → currentCell の占有を解除し、nextCell を新 currentCell に
                if (grid.GetOccupant(_currentCell) == _self)
                    grid.ClearOccupant(_currentCell);
                _currentCell = _nextCell;
                _hasNextCell = false;
                transform.position = nextWorld;
                _pathIdx++;
                // path は維持 → 既存 path を最後まで辿る (毎セル replan による
                // 方向ふらつき/振り向きを抑制)。path が exhausted (idx >= len) の時
                // のみ Update 内 NeedReplan で replan。
                // velocity は維持 (次セル即時 claim でアニメ flicker 防止)
                return;
            }

            float step = Mathf.Min(d, maxSpeed * Time.deltaTime);
            Vector3 dir = to / d;
            transform.position = cur + dir * step;
            velocity = dir * maxSpeed;
        }

        // ============== Snap (アイドル時 cell 中央へ) ==============

        private void SnapToCurrentCell()
        {
            var grid = GridState.Active;
            if (grid == null || _currentCell.x < 0) return;
            Vector3 c = grid.CellToWorld(_currentCell);
            Vector3 cur = transform.position;
            c.y = cur.y;
            Vector3 diff = c - cur;
            if (diff.sqrMagnitude < 0.0001f) return;
            // 速い lerp で cell 中央に寄せる (0.2秒目安)
            float t = Mathf.Min(1f, Time.deltaTime / 0.2f);
            transform.position = Vector3.Lerp(cur, c, t);
        }

        // ============== Path 維持 ==============

        private bool NeedReplan(GridState grid, Vector2Int destCell)
        {
            // 既存 path が有効なら再計算しない (path stickiness)。
            // 動的占有の変化で振動するのを防ぐため、明確な無効化条件のみで replan する。
            if (_path == null) return true;
            if (destCell != _lastDestCell) return true;          // 目的地セル変更
            if (_pathIdx >= _path.Count) return true;            // path 末尾到達
            if (_blockedSince > 0f && Time.time - _blockedSince > _waitBeforeReplan) return true; // 詰まり (個体毎にランダム閾値)
            return false;
        }

        private void ReplanPath(GridState grid, Vector2Int destCell)
        {
            // destination が変わったら detour モードはリセット (まず直進を試す)
            if (destCell != _lastDestCell) _detourMode = false;
            // 詰まり由来の replan なら detour ON、それ以外は直進維持
            if (_blockedSince > 0f && Time.time - _blockedSince > _waitBeforeReplan)
                _detourMode = true;

            _lastReplanTime = Time.time;
            _blockedSince = -1f;
            _lastDestCell = destCell;

            float cellSize = grid.field != null ? Mathf.Max(0.01f, grid.field.cellSize) : 1f;
            int cellRadius = Mathf.Max(0, Mathf.FloorToInt(endReachedDistance / cellSize));

            // 直進モード: 静的歩行可否のみ見る (味方は無視) → 一直線に近い path
            // 迂回モード: 味方も障害物 → 動的に避ける path
            // どちらでも target は障害物 (隣接で攻撃するので入る必要なし)
            System.Predicate<Vector2Int> isWalkable = _detourMode
                ? (System.Predicate<Vector2Int>)(cell => grid.IsWalkableForUnit(cell, _self, null))
                : (System.Predicate<Vector2Int>)(cell => grid.IsStaticWalkable(cell));

            System.Predicate<Vector2Int> isGoal = cell =>
            {
                int md = Mathf.Abs(cell.x - destCell.x) + Mathf.Abs(cell.y - destCell.y);
                if (md > cellRadius) return false;
                // 範囲 >= 1 なら destCell 自身は除外 (target に隣接して止まる)
                if (cellRadius >= 1 && cell == destCell) return false;
                return isWalkable(cell);
            };

            _path = GridPathfinder.FindPath(_currentCell, isGoal, isWalkable, destCell);
            _pathIdx = 0;
        }
    }
}
