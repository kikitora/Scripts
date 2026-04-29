using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>
    /// 戦闘グリッドの状態 (静的歩行可否 + 動的占有ユニット)。
    /// 戦闘開始時に GridState.Init() で初期化、戦闘中は単一インスタンス Active から参照する。
    /// 座標系：side=0 (Ally) と side=1 (Enemy) を flat global cell に統合。
    ///   globalX 0..(depthPerSide*2-1) (forward 軸、0 が ally 最奥、最大が enemy 最奥)
    ///   globalY 0..(width-1)          (right 軸)
    /// </summary>
    public class GridState
    {
        public static GridState Active;

        public readonly int width;       // grid Y 方向 (right axis)
        public readonly int depth;       // 両陣合算の forward 軸長 = depthPerSide * 2
        public readonly int depthPerSide;
        public readonly BattleFieldVisualizer field;

        // 静的歩行可否 (壁・自軍ギミック・回避対象敵ギミック = false)
        // 戦闘開始時に焼き、ギミック破壊時のみ更新。
        private readonly bool[,] _staticWalkable;

        // 動的占有 (移動中含む現在位置)。null = 空き。
        private readonly RealtimeBattleUnit[,] _occupant;

        private GridState(BattleFieldVisualizer fv)
        {
            field = fv;
            width = fv.gridWidth;
            depthPerSide = fv.gridDepth;
            depth = depthPerSide * 2;
            _staticWalkable = new bool[depth, width];
            _occupant = new RealtimeBattleUnit[depth, width];
            for (int x = 0; x < depth; x++)
                for (int y = 0; y < width; y++)
                    _staticWalkable[x, y] = true;
        }

        /// <summary>戦闘開始時にコール。Active を入れ替える。</summary>
        public static GridState Init(BattleFieldVisualizer fv)
        {
            Active = new GridState(fv);
            return Active;
        }

        public static void Clear()
        {
            Active = null;
        }

        // ============== 座標変換 ==============

        /// <summary>BattleFieldVisualizer の (side, gridX, gridY) → global cell。</summary>
        public Vector2Int SideCellToGlobal(int side, int gridX, int gridY)
        {
            int gx = (side == 0) ? (depthPerSide - 1 - gridX) : (depthPerSide + gridX);
            return new Vector2Int(gx, gridY);
        }

        /// <summary>global cell → (side, gridX, gridY)。</summary>
        public void GlobalToSideCell(Vector2Int cell, out int side, out int gridX, out int gridY)
        {
            if (cell.x < depthPerSide)
            {
                side = 0;
                gridX = depthPerSide - 1 - cell.x;
            }
            else
            {
                side = 1;
                gridX = cell.x - depthPerSide;
            }
            gridY = cell.y;
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            GlobalToSideCell(cell, out int side, out int gx, out int gy);
            return field.GridToWorldPosition(side, gx, gy);
        }

        /// <summary>world 位置 → global cell。範囲外なら (-1,-1)。</summary>
        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            var v = field.WorldToGrid(worldPos);
            if (v.x < 0) return new Vector2Int(-1, -1);
            return SideCellToGlobal(v.x, v.y, v.z);
        }

        // ============== 状態クエリ ==============

        public bool InBounds(Vector2Int cell)
            => cell.x >= 0 && cell.x < depth && cell.y >= 0 && cell.y < width;

        public bool IsStaticWalkable(Vector2Int cell)
            => InBounds(cell) && _staticWalkable[cell.x, cell.y];

        public RealtimeBattleUnit GetOccupant(Vector2Int cell)
            => InBounds(cell) ? _occupant[cell.x, cell.y] : null;

        /// <summary>requestor が cell に入れるか。
        /// 静的歩行可 && (空 or 自分自身が居る)。target 指定時はその cell の占有者は無視 (隣接で攻撃する用)。</summary>
        public bool IsWalkableForUnit(Vector2Int cell, RealtimeBattleUnit requestor, RealtimeBattleUnit ignore = null)
        {
            if (!IsStaticWalkable(cell)) return false;
            var occ = _occupant[cell.x, cell.y];
            return occ == null || occ == requestor || occ == ignore;
        }

        // ============== 状態更新 ==============

        public void SetStaticWalkable(Vector2Int cell, bool walkable)
        {
            if (InBounds(cell)) _staticWalkable[cell.x, cell.y] = walkable;
        }

        public void SetOccupant(Vector2Int cell, RealtimeBattleUnit unit)
        {
            if (InBounds(cell)) _occupant[cell.x, cell.y] = unit;
        }

        public void ClearOccupant(Vector2Int cell)
        {
            if (InBounds(cell)) _occupant[cell.x, cell.y] = null;
        }

        /// <summary>同一 unit の cell 移動を反映 (旧 cell から消し、新 cell に書く)。</summary>
        public void MoveOccupant(RealtimeBattleUnit unit, Vector2Int fromCell, Vector2Int toCell)
        {
            if (InBounds(fromCell) && _occupant[fromCell.x, fromCell.y] == unit)
                _occupant[fromCell.x, fromCell.y] = null;
            if (InBounds(toCell))
                _occupant[toCell.x, toCell.y] = unit;
        }
    }
}
