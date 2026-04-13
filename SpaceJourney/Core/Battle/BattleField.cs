using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 可変形バトルフィールド。
    /// cells (Vector2Int[]) で有効マスを定義し、各 side が同じ形状を持つ。
    /// side 0 = 味方側、side 1 = 敵側。
    ///
    /// 座標系:
    ///   x: 0=前列 (敵に近い方), 大きいほど後列
    ///   y: 横方向
    /// </summary>
    public class BattleField
    {
        public int SideCount { get; }

        private readonly HashSet<Vector2Int> validCells;
        private readonly Dictionary<(int side, Vector2Int cell), SpaceJourneyUnit> grid;

        /// <summary>ユニットの所属サイド (物理位置が変わっても所属は変わらない)</summary>
        private readonly Dictionary<SpaceJourneyUnit, int> unitOwnerSide = new();

        /// <summary>有効セル一覧 (読み取り専用コピー)</summary>
        public IReadOnlyCollection<Vector2Int> Cells => validCells;

        /// <summary>可変形フィールドを生成</summary>
        public BattleField(Vector2Int[] cells, int sideCount = 2)
        {
            SideCount = sideCount;
            validCells = new HashSet<Vector2Int>(cells);
            grid = new Dictionary<(int, Vector2Int), SpaceJourneyUnit>();
        }

        /// <summary>デフォルト 5x5 × 2面</summary>
        public BattleField() : this(BuildDefault5x5Cells(), 2) { }

        private static Vector2Int[] BuildDefault5x5Cells()
        {
            var cells = new Vector2Int[25];
            int i = 0;
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    cells[i++] = new Vector2Int(x, y);
            return cells;
        }

        /// <summary>BattleFieldLayout から生成</summary>
        public BattleField(BattleFieldLayout layout)
            : this(layout.cells, layout.sideCount) { }

        public bool IsValidCell(Vector2Int cell) => validCells.Contains(cell);

        public SpaceJourneyUnit GetUnit(int side, Vector2Int cell)
        {
            if (!IsValid(side, cell)) return null;
            grid.TryGetValue((side, cell), out var unit);
            return unit;
        }

        /// <summary>int 引数版 (後方互換)</summary>
        public SpaceJourneyUnit GetUnit(int side, int x, int y)
            => GetUnit(side, new Vector2Int(x, y));

        public bool PlaceUnit(SpaceJourneyUnit unit, int side, Vector2Int cell)
        {
            if (!IsValid(side, cell)) return false;
            var key = (side, cell);
            if (grid.ContainsKey(key) && grid[key] != null) return false;
            grid[key] = unit;
            if (!unitOwnerSide.ContainsKey(unit))
                unitOwnerSide[unit] = side;
            return true;
        }

        /// <summary>int 引数版 (後方互換)</summary>
        public bool PlaceUnit(SpaceJourneyUnit unit, int side, int x, int y)
            => PlaceUnit(unit, side, new Vector2Int(x, y));

        /// <summary>
        /// ユニットの位置を返す。見つからなければ (-1,-1,-1)。
        /// 戻り値: Vector3Int(side, cell.x, cell.y)
        /// </summary>
        public Vector3Int FindUnit(SpaceJourneyUnit unit)
        {
            foreach (var kvp in grid)
            {
                if (kvp.Value == unit)
                    return new Vector3Int(kvp.Key.side, kvp.Key.cell.x, kvp.Key.cell.y);
            }
            return new Vector3Int(-1, -1, -1);
        }

        /// <summary>ユニットの所属サイドを返す (物理位置ではなく元の陣営)</summary>
        public int GetSide(SpaceJourneyUnit unit)
        {
            return unitOwnerSide.TryGetValue(unit, out int s) ? s : -1;
        }

        /// <summary>所属サイドが side のユニット一覧 (物理位置に関係なく)</summary>
        public List<SpaceJourneyUnit> GetAllUnits(int side)
        {
            var list = new List<SpaceJourneyUnit>();
            foreach (var kvp in unitOwnerSide)
            {
                if (kvp.Value == side && kvp.Key != null)
                    list.Add(kvp.Key);
            }
            return list;
        }

        /// <summary>所属サイドが side の生存ユニット一覧</summary>
        public List<SpaceJourneyUnit> GetAllAlive(int side)
        {
            var list = new List<SpaceJourneyUnit>();
            foreach (var kvp in unitOwnerSide)
            {
                if (kvp.Value == side && kvp.Key != null && !kvp.Key.IsDead)
                    list.Add(kvp.Key);
            }
            return list;
        }

        public List<SpaceJourneyUnit> GetAllAliveUnits()
        {
            var list = new List<SpaceJourneyUnit>();
            for (int s = 0; s < SideCount; s++)
                list.AddRange(GetAllAlive(s));
            return list;
        }

        /// <summary>対面のside</summary>
        public static int OppositeSide(int side) => side == 0 ? 1 : 0;

        /// <summary>2ユニット間のマンハッタン距離 (side をまたぐ場合 x は合算)</summary>
        public int Distance(SpaceJourneyUnit a, SpaceJourneyUnit b)
        {
            var pa = FindUnit(a);
            var pb = FindUnit(b);
            if (pa.x < 0 || pb.x < 0) return 999;

            int ax = pa[1], ay = pa[2], aside = pa[0];
            int bx = pb[1], by = pb[2], bside = pb[0];

            if (aside == bside)
                return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);

            // 異なる side: x は「前列(0)同士が最も近い」
            // 距離 = (ax) + 1 + (bx) + |ay - by|
            return ax + 1 + bx + Mathf.Abs(ay - by);
        }

        /// <summary>指定 side の前列 (x 最小) にいる生存ユニットのセル一覧</summary>
        public List<Vector2Int> GetFrontRowCells(int side)
        {
            int minX = int.MaxValue;
            foreach (var cell in validCells)
            {
                if (cell.x < minX) minX = cell.x;
            }

            var result = new List<Vector2Int>();
            foreach (var cell in validCells)
            {
                if (cell.x == minX)
                {
                    var u = GetUnit(side, cell);
                    if (u != null && !u.IsDead)
                        result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>前列の x 値 (最小 x)</summary>
        public int FrontRow
        {
            get
            {
                int min = int.MaxValue;
                foreach (var cell in validCells)
                    if (cell.x < min) min = cell.x;
                return min == int.MaxValue ? 0 : min;
            }
        }

        /// <summary>全有効セルの y 値一覧 (重複なし)</summary>
        public HashSet<int> AllYValues
        {
            get
            {
                var ys = new HashSet<int>();
                foreach (var cell in validCells) ys.Add(cell.y);
                return ys;
            }
        }

        /// <summary>ユニットを同じside内の別セルに移動する。</summary>
        public bool MoveUnit(SpaceJourneyUnit unit, Vector2Int toCell)
        {
            var pos = FindUnit(unit);
            if (pos.x < 0) return false;

            int side = pos[0];
            var fromCell = new Vector2Int(pos[1], pos[2]);

            if (!IsValid(side, toCell)) return false;
            var toKey = (side, toCell);
            if (grid.ContainsKey(toKey) && grid[toKey] != null) return false;

            grid.Remove((side, fromCell));
            grid[toKey] = unit;
            return true;
        }

        /// <summary>ユニットを別のsideに移動する (sideまたぎ)。</summary>
        public bool MoveUnitCrossSide(SpaceJourneyUnit unit, int toSide, Vector2Int toCell)
        {
            var pos = FindUnit(unit);
            if (pos.x < 0) return false;

            int fromSide = pos[0];
            var fromCell = new Vector2Int(pos[1], pos[2]);

            if (!IsValid(toSide, toCell)) return false;
            var toKey = (toSide, toCell);
            if (grid.ContainsKey(toKey) && grid[toKey] != null) return false;

            grid.Remove((fromSide, fromCell));
            grid[toKey] = unit;
            return true;
        }

        /// <summary>2体のユニットの位置を入れ替える (sideまたぎ対応)。</summary>
        public bool Swap(SpaceJourneyUnit a, SpaceJourneyUnit b)
        {
            var pa = FindUnit(a);
            var pb = FindUnit(b);
            if (pa.x < 0 || pb.x < 0) return false;
            int sa = pa[0]; var ca = new Vector2Int(pa[1], pa[2]);
            int sb = pb[0]; var cb = new Vector2Int(pb[1], pb[2]);
            grid[(sa, ca)] = b;
            grid[(sb, cb)] = a;
            return true;
        }

        /// <summary>指定セルが空いているか</summary>
        public bool IsCellEmpty(int side, Vector2Int cell)
        {
            if (!IsValid(side, cell)) return false;
            return !grid.ContainsKey((side, cell)) || grid[(side, cell)] == null;
        }

        private bool IsValid(int side, Vector2Int cell)
            => side >= 0 && side < SideCount && validCells.Contains(cell);

        /// <summary>デバッグ用: フィールドの文字列表現</summary>
        public string ToDebugString()
        {
            // 有効セルの範囲を算出
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var cell in validCells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.y < minY) minY = cell.y;
                if (cell.y > maxY) maxY = cell.y;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== BattleField ===");
            sb.AppendLine("  [味方 side=0]        [敵 side=1]");
            for (int y = minY; y <= maxY; y++)
            {
                for (int s = 0; s < SideCount; s++)
                {
                    if (s > 0) sb.Append("  |  ");
                    for (int x = minX; x <= maxX; x++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (!validCells.Contains(cell))
                        {
                            sb.Append("[----]");
                            continue;
                        }
                        var u = GetUnit(s, cell);
                        if (u == null)
                            sb.Append("[    ]");
                        else if (u.IsDead)
                            sb.Append("[DEAD]");
                        else
                            sb.Append($"[{u.CurrentHp,4}]");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
