using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>
    /// 戦闘グリッド用 4方向 A*。50ノード規模なので毎回フル計算で十分。
    /// path 維持の責務 (再計算するか) は GridUnitMover 側、ここは純粋な探索器。
    /// </summary>
    public static class GridPathfinder
    {
        private static readonly Vector2Int[] _dirs4 = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };

        /// <summary>
        /// start から isGoal を満たすセルへの最短パス (start 含まない隣接以降のセル列) を返す。
        /// 経路無しなら null。start が既に goal 条件を満たすなら空 List を返す。
        /// </summary>
        /// <param name="start">出発セル</param>
        /// <param name="isGoal">そのセルに着けば終了でいい場合 true。target に隣接などを含められる。</param>
        /// <param name="isWalkable">そのセルに入れるかどうか。動的占有判定込み。</param>
        /// <param name="heuristicTarget">A* ヒューリスティック計算用の "概念的なゴール座標"。null なら 0。</param>
        /// <param name="maxIterations">安全停止 (50ノード grid なら 200 で十分)</param>
        public static List<Vector2Int> FindPath(
            Vector2Int start,
            Predicate<Vector2Int> isGoal,
            Predicate<Vector2Int> isWalkable,
            Vector2Int? heuristicTarget = null,
            int maxIterations = 1000)
        {
            if (isGoal(start)) return new List<Vector2Int>();

            // tiebreak: start から cell までの 90度方向転換回数 (turn count) を優先キーに使う。
            // 同コストパスが複数ある時、転換が少ない L字型パスを優先 →
            // velocity-based facing で「1歩→右→1歩→右」となる stair-step を抑制。
            var open = new SortedSet<(int f, int turn, int order, Vector2Int cell)>(new TupleComparer());
            var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var parentDir = new Dictionary<Vector2Int, Vector2Int> { [start] = Vector2Int.zero };
            var turnCount = new Dictionary<Vector2Int, int> { [start] = 0 };
            int orderCounter = 0;
            open.Add((Heuristic(start, heuristicTarget), 0, orderCounter++, start));

            int iter = 0;
            while (open.Count > 0 && iter++ < maxIterations)
            {
                var current = open.Min;
                open.Remove(current);
                var cell = current.cell;
                if (isGoal(cell))
                    return Reconstruct(cameFrom, cell);

                int currentG = gScore[cell];
                Vector2Int myInDir = parentDir[cell];
                int myTurn = turnCount[cell];

                foreach (var d in _dirs4)
                {
                    var next = cell + d;
                    if (!isWalkable(next)) continue;

                    int tentativeG = currentG + 1;
                    int newTurn = myTurn + ((myInDir != Vector2Int.zero && myInDir != d) ? 1 : 0);

                    bool hasExisting = gScore.TryGetValue(next, out int existingG);
                    int existingTurn = hasExisting ? turnCount[next] : int.MaxValue;
                    // (g, turn) 辞書順で既存より良くなければスキップ
                    if (hasExisting && (existingG < tentativeG ||
                        (existingG == tentativeG && existingTurn <= newTurn)))
                        continue;

                    gScore[next] = tentativeG;
                    cameFrom[next] = cell;
                    parentDir[next] = d;
                    turnCount[next] = newTurn;
                    int f = tentativeG + Heuristic(next, heuristicTarget);
                    open.Add((f, newTurn, orderCounter++, next));
                }
            }

            return null;
        }

        private static int Heuristic(Vector2Int cell, Vector2Int? target)
        {
            if (!target.HasValue) return 0;
            var t = target.Value;
            return Mathf.Abs(cell.x - t.x) + Mathf.Abs(cell.y - t.y);
        }

        private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
        {
            var path = new List<Vector2Int> { end };
            while (cameFrom.TryGetValue(end, out var prev))
            {
                end = prev;
                path.Add(end);
            }
            path.Reverse();
            // 出発点 [0] は捨て、進むべきセル列だけ返す
            if (path.Count > 0) path.RemoveAt(0);
            return path;
        }

        private class TupleComparer : IComparer<(int f, int turn, int order, Vector2Int cell)>
        {
            public int Compare((int f, int turn, int order, Vector2Int cell) a, (int f, int turn, int order, Vector2Int cell) b)
            {
                int c = a.f.CompareTo(b.f);
                if (c != 0) return c;
                int t = a.turn.CompareTo(b.turn);
                if (t != 0) return t;
                return a.order.CompareTo(b.order);
            }
        }

        // ============== ヘルパー: 直進可能判定 ==============

        /// <summary>start から goal まで、4方向移動の単純線で walkable のみを通れるか。</summary>
        public static bool CanGoStraight(Vector2Int start, Vector2Int goal, Predicate<Vector2Int> isWalkable)
        {
            // L字: x 方向先 → y 方向、または y 方向先 → x 方向、の片方が通ればOK。
            if (CanGoStraightAxis(start, goal, isWalkable, xFirst: true)) return true;
            if (CanGoStraightAxis(start, goal, isWalkable, xFirst: false)) return true;
            return false;
        }

        private static bool CanGoStraightAxis(Vector2Int start, Vector2Int goal, Predicate<Vector2Int> isWalkable, bool xFirst)
        {
            int sx = start.x, sy = start.y;
            int gx = goal.x, gy = goal.y;
            int x = sx, y = sy;
            if (xFirst)
            {
                int dx = gx > sx ? 1 : -1;
                while (x != gx)
                {
                    x += dx;
                    if (!isWalkable(new Vector2Int(x, y))) return false;
                }
                int dy = gy > sy ? 1 : -1;
                while (y != gy)
                {
                    y += dy;
                    if (!isWalkable(new Vector2Int(x, y))) return false;
                }
            }
            else
            {
                int dy = gy > sy ? 1 : -1;
                while (y != gy)
                {
                    y += dy;
                    if (!isWalkable(new Vector2Int(x, y))) return false;
                }
                int dx = gx > sx ? 1 : -1;
                while (x != gx)
                {
                    x += dx;
                    if (!isWalkable(new Vector2Int(x, y))) return false;
                }
            }
            return true;
        }
    }
}
