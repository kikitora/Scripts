using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>
    /// SimpleGrid のフィールド (Forward/Right 軸) に整列した 2D セル化グリッドを
    /// Barricade で動的に塞ぎ、A* で経路を計算する IBarricadeMap 実装。
    ///
    /// ・cellSize 0.4m (Barricade 厚さ 0.4m と一致させて壁を確実にブロック扱い)
    /// ・8 方向移動 + 斜め移動は √2 コスト
    /// ・斜め移動時に両隣セルが両方ブロックなら不可 (壁の隙間すり抜け防止)
    /// ・フィールド外セルもブロック扱い
    /// ・Barricade の存在判定は OverlapsCircle(unitRadius + 余裕) で各セル中心を試験
    /// ・経路は line-of-sight で平滑化
    ///
    /// バリケード追加/削除時は Rebuild() を呼ぶこと。
    /// </summary>
    public class GridBarricadeMap : IBarricadeMap
    {
        // ============= 設定 =============

        private const float CELL_SIZE = 0.4f;          // セル一辺 (Barricade 厚さ = 0.4m)
        private const float UNIT_RADIUS = 0.36f;       // ユニット衝突半径 (push-out と一致)
        private const float CLEARANCE = 0.05f;         // 余裕 (壁ギリギリは通れない扱い)

        // ============= フィールド軸 =============

        private readonly SimpleGrid _simple;
        private readonly Vector3 _center;
        private readonly Vector3 _fwd;
        private readonly Vector3 _right;
        private readonly float _halfD;        // forward 軸 半幅 (= halfDepth)
        private readonly float _halfW;        // right 軸 半幅 (= halfWidth)

        // ============= グリッド =============

        private readonly int _cellsForward;
        private readonly int _cellsRight;
        private bool[] _blocked;              // [_cellsForward * _cellsRight]

        private readonly RealtimeBattleManager _mgr;

        public bool IsPresent => _mgr != null && _mgr.barricades != null && _mgr.barricades.Count > 0;

        // ============= 構築 =============

        public GridBarricadeMap(RealtimeBattleManager mgr, SimpleGrid grid)
        {
            _mgr = mgr;
            _simple = grid;

            if (grid != null && grid.field != null)
            {
                var f = grid.field;
                _halfD = f.gridDepth * f.cellSize;
                _halfW = f.gridWidth * f.cellSize * 0.5f;

                _center = (f.playerCube != null && f.enemyCube != null)
                    ? (f.playerCube.position + f.enemyCube.position) * 0.5f
                    : f.transform.position;
                _center.y = 0f;

                Vector3 fwd = f.Forward; fwd.y = 0f; fwd.Normalize();
                Vector3 right = f.RightAxis; right.y = 0f; right.Normalize();
                _fwd = fwd;
                _right = right;
            }
            else
            {
                // フォールバック (テスト用): world XZ 軸に整列
                _halfD = 5f;
                _halfW = 5f;
                _center = Vector3.zero;
                _fwd = Vector3.forward;
                _right = Vector3.right;
            }

            _cellsForward = Mathf.Max(1, Mathf.CeilToInt((_halfD * 2f) / CELL_SIZE));
            _cellsRight = Mathf.Max(1, Mathf.CeilToInt((_halfW * 2f) / CELL_SIZE));
            _blocked = new bool[_cellsForward * _cellsRight];

            Rebuild();
        }

        /// <summary>バリケードの追加/削除後に呼ぶ。全セル走査して再計算。</summary>
        public void Rebuild()
        {
            for (int i = 0; i < _blocked.Length; i++) _blocked[i] = false;

            if (_mgr == null || _mgr.barricades == null) return;

            float blockRadius = UNIT_RADIUS + CLEARANCE;

            foreach (var b in _mgr.barricades)
            {
                if (b == null || !b.IsActive) continue;
                MarkBarricadeCells(b, blockRadius);
            }
        }

        private void MarkBarricadeCells(Barricade b, float blockRadius)
        {
            // バリケードの AABB (拡張) 範囲のセルだけを試験 (全セル舐めるよりずっと速い)
            float halfLen = Barricade.LENGTH * 0.5f + blockRadius;
            float halfThk = Barricade.THICKNESS * 0.5f + blockRadius;

            // バリケードを field 軸 (forward/right) に投影した bbox
            Vector3 along = new Vector3(-b.facing.z, 0f, b.facing.x);
            Vector3[] corners = {
                b.center + along * halfLen + b.facing * halfThk,
                b.center + along * halfLen - b.facing * halfThk,
                b.center - along * halfLen + b.facing * halfThk,
                b.center - along * halfLen - b.facing * halfThk,
            };

            float minF = float.PositiveInfinity, maxF = float.NegativeInfinity;
            float minR = float.PositiveInfinity, maxR = float.NegativeInfinity;
            foreach (var c in corners)
            {
                Vector3 rel = c - _center;
                float f = Vector3.Dot(rel, _fwd);
                float r = Vector3.Dot(rel, _right);
                if (f < minF) minF = f;
                if (f > maxF) maxF = f;
                if (r < minR) minR = r;
                if (r > maxR) maxR = r;
            }

            int cf0 = AxisToCell(minF, _halfD);
            int cf1 = AxisToCell(maxF, _halfD);
            int cr0 = AxisToCell(minR, _halfW);
            int cr1 = AxisToCell(maxR, _halfW);

            cf0 = Mathf.Clamp(cf0, 0, _cellsForward - 1);
            cf1 = Mathf.Clamp(cf1, 0, _cellsForward - 1);
            cr0 = Mathf.Clamp(cr0, 0, _cellsRight - 1);
            cr1 = Mathf.Clamp(cr1, 0, _cellsRight - 1);

            for (int cf = cf0; cf <= cf1; cf++)
            {
                for (int cr = cr0; cr <= cr1; cr++)
                {
                    Vector3 wp = CellCenterToWorld(cf, cr);
                    if (b.OverlapsCircle(wp, blockRadius))
                        _blocked[cf * _cellsRight + cr] = true;
                }
            }
        }

        // ============= 座標変換 =============

        private int AxisToCell(float axisVal, float halfRange)
        {
            return Mathf.FloorToInt((axisVal + halfRange) / CELL_SIZE);
        }

        private void WorldToCell(Vector3 world, out int cf, out int cr)
        {
            Vector3 rel = world - _center;
            float f = Vector3.Dot(rel, _fwd);
            float r = Vector3.Dot(rel, _right);
            cf = AxisToCell(f, _halfD);
            cr = AxisToCell(r, _halfW);
        }

        private Vector3 CellCenterToWorld(int cf, int cr)
        {
            float f = (cf + 0.5f) * CELL_SIZE - _halfD;
            float r = (cr + 0.5f) * CELL_SIZE - _halfW;
            return _center + _fwd * f + _right * r;
        }

        private bool InBounds(int cf, int cr)
            => cf >= 0 && cf < _cellsForward && cr >= 0 && cr < _cellsRight;

        private bool IsCellBlocked(int cf, int cr)
        {
            if (!InBounds(cf, cr)) return true; // 外周はブロック扱い
            return _blocked[cf * _cellsRight + cr];
        }

        // ============= IBarricadeMap =============

        public bool IsBlocked(Vector3 world)
        {
            WorldToCell(world, out int cf, out int cr);
            return IsCellBlocked(cf, cr);
        }

        public bool LineOfSight(Vector3 a, Vector3 b, float step = 0.25f)
        {
            Vector3 d = b - a; d.y = 0f;
            float dist = d.magnitude;
            if (dist < 1e-6f) return true;
            int n = Mathf.Max(1, Mathf.CeilToInt(dist / step));
            Vector3 inv = d / n;
            Vector3 p = a;
            for (int i = 1; i <= n; i++)
            {
                p += inv;
                if (IsBlocked(p)) return false;
            }
            return true;
        }

        // ============= A* =============

        // ノード struct を避け、配列ベースで保持 (パス計算が頻繁なので GC 抑制)
        private float[] _gScore;
        private int[] _cameFrom;
        private int[] _openHeap;            // バイナリヒープ (cell index)
        private float[] _heapKey;           // f-score key
        private int _heapCount;

        private void EnsureBuffers()
        {
            int n = _cellsForward * _cellsRight;
            if (_gScore == null || _gScore.Length != n)
            {
                _gScore = new float[n];
                _cameFrom = new int[n];
                _openHeap = new int[n];
                _heapKey = new float[n];
            }
        }

        public List<Vector3> FindPath(Vector3 start, Vector3 goal, float resolution = 0.5f, int maxIter = 600)
        {
            EnsureBuffers();
            int n = _cellsForward * _cellsRight;
            for (int i = 0; i < n; i++) { _gScore[i] = float.PositiveInfinity; _cameFrom[i] = -1; }
            _heapCount = 0;

            WorldToCell(start, out int sf, out int sr);
            WorldToCell(goal, out int gf, out int gr);
            sf = Mathf.Clamp(sf, 0, _cellsForward - 1);
            sr = Mathf.Clamp(sr, 0, _cellsRight - 1);
            gf = Mathf.Clamp(gf, 0, _cellsForward - 1);
            gr = Mathf.Clamp(gr, 0, _cellsRight - 1);

            // start/goal が壁内なら最寄りの空セルにスナップ (search radius 4 cell)
            if (IsCellBlocked(sf, sr)) SnapToFree(ref sf, ref sr, 4);
            if (IsCellBlocked(gf, gr)) SnapToFree(ref gf, ref gr, 4);

            int startIdx = sf * _cellsRight + sr;
            int goalIdx = gf * _cellsRight + gr;

            if (startIdx == goalIdx)
                return new List<Vector3> { goal };

            _gScore[startIdx] = 0f;
            HeapPush(startIdx, Heuristic(sf, sr, gf, gr));

            int iter = 0;
            int found = -1;

            while (_heapCount > 0 && iter < maxIter)
            {
                iter++;
                int cur = HeapPop();
                if (cur == goalIdx) { found = cur; break; }

                int cf = cur / _cellsRight;
                int cr = cur % _cellsRight;

                // 8 方向
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nf = cf + dx;
                        int nr = cr + dy;
                        if (!InBounds(nf, nr)) continue;
                        if (IsCellBlocked(nf, nr)) continue;

                        // 斜めは両側セルが空である必要 (隙間貫通防止)
                        if (dx != 0 && dy != 0)
                        {
                            if (IsCellBlocked(cf + dx, cr) || IsCellBlocked(cf, cr + dy))
                                continue;
                        }

                        float step = (dx != 0 && dy != 0) ? 1.41421356f : 1f;
                        int nIdx = nf * _cellsRight + nr;
                        float tentative = _gScore[cur] + step;
                        if (tentative < _gScore[nIdx])
                        {
                            _gScore[nIdx] = tentative;
                            _cameFrom[nIdx] = cur;
                            float fScore = tentative + Heuristic(nf, nr, gf, gr);
                            HeapPush(nIdx, fScore);
                        }
                    }
                }
            }

            if (found < 0)
            {
                // 経路無し: goal へ直行 (mover 側の clamp で壁にブロックされる)
                return new List<Vector3> { goal };
            }

            // 経路復元 (start → goal 順)
            var cells = new List<int>();
            int c = found;
            while (c >= 0)
            {
                cells.Add(c);
                c = _cameFrom[c];
            }
            cells.Reverse();

            // セル → world 座標 + 平滑化
            var raw = new List<Vector3>(cells.Count);
            foreach (var idx in cells)
            {
                int xf = idx / _cellsRight;
                int xr = idx % _cellsRight;
                raw.Add(CellCenterToWorld(xf, xr));
            }
            // 末尾は実 goal に置換
            if (raw.Count > 0) raw[raw.Count - 1] = goal;

            return SmoothPath(raw);
        }

        /// <summary>line-of-sight で中間 waypoint を間引く。</summary>
        private List<Vector3> SmoothPath(List<Vector3> raw)
        {
            if (raw.Count <= 2) return raw;
            var result = new List<Vector3>();
            int i = 0;
            while (i < raw.Count - 1)
            {
                int j = raw.Count - 1;
                // 一番遠い見通せる点まで一気に飛ばす
                while (j > i + 1 && !LineOfSight(raw[i], raw[j])) j--;
                result.Add(raw[j]);
                i = j;
            }
            return result;
        }

        private float Heuristic(int af, int ar, int bf, int br)
        {
            int dx = Mathf.Abs(af - bf);
            int dy = Mathf.Abs(ar - br);
            // 8 方向用 octile heuristic
            int dmin = Mathf.Min(dx, dy);
            int dmax = Mathf.Max(dx, dy);
            return dmin * 1.41421356f + (dmax - dmin);
        }

        private void SnapToFree(ref int cf, ref int cr, int searchRadius)
        {
            for (int r = 1; r <= searchRadius; r++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue; // 境界のみ
                        int nf = cf + dx;
                        int nr = cr + dy;
                        if (InBounds(nf, nr) && !_blocked[nf * _cellsRight + nr])
                        {
                            cf = nf; cr = nr; return;
                        }
                    }
                }
            }
            // 見つからない場合は元のまま (search 失敗 → 直行 fallback に任せる)
        }

        // ============= バイナリヒープ (min-heap) =============

        private void HeapPush(int cellIdx, float key)
        {
            int i = _heapCount;
            _openHeap[i] = cellIdx;
            _heapKey[i] = key;
            _heapCount++;
            // sift up
            while (i > 0)
            {
                int parent = (i - 1) >> 1;
                if (_heapKey[parent] <= _heapKey[i]) break;
                Swap(parent, i);
                i = parent;
            }
        }

        private int HeapPop()
        {
            int root = _openHeap[0];
            _heapCount--;
            if (_heapCount == 0) return root;
            _openHeap[0] = _openHeap[_heapCount];
            _heapKey[0] = _heapKey[_heapCount];
            // sift down
            int i = 0;
            while (true)
            {
                int l = i * 2 + 1;
                int r = i * 2 + 2;
                int smallest = i;
                if (l < _heapCount && _heapKey[l] < _heapKey[smallest]) smallest = l;
                if (r < _heapCount && _heapKey[r] < _heapKey[smallest]) smallest = r;
                if (smallest == i) break;
                Swap(smallest, i);
                i = smallest;
            }
            return root;
        }

        private void Swap(int a, int b)
        {
            int ti = _openHeap[a]; _openHeap[a] = _openHeap[b]; _openHeap[b] = ti;
            float tk = _heapKey[a]; _heapKey[a] = _heapKey[b]; _heapKey[b] = tk;
        }
    }
}
