using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.Pathfinding
{
    /// <summary>spawn 配置と境界クランプのみを担当する軽量グリッド。
    /// 占有テーブル / 経路探索は持たない (連続移動は ContinuousMover が担当)。
    /// 旧 GridState の座標変換ロジックを引き継ぐ。</summary>
    public class SimpleGrid
    {
        public int Width { get; }
        public int DepthPerSide { get; }
        public int Depth => DepthPerSide * 2;
        public float CellSize { get; }

        // クランプの軸計算用 (旧 GridState.field と同様に保持)
        public BattleFieldVisualizer field;

        public static SimpleGrid Active { get; private set; }

        public SimpleGrid(int width, int depthPerSide, float cellSize, BattleFieldVisualizer fieldVis)
        {
            Width = width;
            DepthPerSide = depthPerSide;
            CellSize = cellSize;
            field = fieldVis;
        }

        public static SimpleGrid Init(int width, int depthPerSide, float cellSize, BattleFieldVisualizer fieldVis)
        {
            Active = new SimpleGrid(width, depthPerSide, cellSize, fieldVis);
            return Active;
        }

        public static void Clear()
        {
            Active = null;
        }

        /// <summary>(side, gridX, gridY) → world 座標。
        /// 旧 GridState.SideCellToGlobal + CellToWorld 相当。
        /// BattleFieldVisualizer.GridToWorldPosition がある場合はそれを使う (最も正確)。</summary>
        public Vector3 SideCellToWorld(int side, int gx, int gy)
        {
            if (field != null)
                return field.GridToWorldPosition(side, gx, gy);
            // フォールバック: field なしの場合は簡易計算 (テスト用)
            int globalX = (side == 0) ? (DepthPerSide - 1 - gx) : (DepthPerSide + gx);
            return new Vector3(globalX * CellSize, 0f, gy * CellSize);
        }

        /// <summary>フィールド内に座標をクランプする。
        /// BattleFieldVisualizer がある場合は field の Forward/RightAxis 軸でクランプ。</summary>
        public Vector3 ClampToField(Vector3 world)
        {
            if (field == null)
            {
                // フォールバック: XZ 平面で直接クランプ
                float maxX = Mathf.Max(0f, (Depth - 1) * CellSize);
                float maxZ = Mathf.Max(0f, (Width - 1) * CellSize);
                return new Vector3(
                    Mathf.Clamp(world.x, 0f, maxX),
                    world.y,
                    Mathf.Clamp(world.z, 0f, maxZ));
            }

            // field の軸 (Forward/RightAxis) ベースでクランプ
            // 旧 RealtimeBattleUnit.ClampToField と同等のロジック
            float halfD = field.gridDepth * field.cellSize;      // forward 軸 full range (片側)
            float halfW = field.gridWidth * field.cellSize * 0.5f; // right 軸 半幅

            Vector3 center = (field.playerCube != null && field.enemyCube != null)
                ? (field.playerCube.position + field.enemyCube.position) * 0.5f
                : field.transform.position;
            center.y = 0f;

            Vector3 fwd = field.Forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = field.RightAxis; right.y = 0f; right.Normalize();

            Vector3 rel = world - center;
            rel.y = 0f;

            float fwdAxis = Vector3.Dot(rel, fwd);
            float rightAxis = Vector3.Dot(rel, right);

            fwdAxis = Mathf.Clamp(fwdAxis, -halfD, halfD);
            rightAxis = Mathf.Clamp(rightAxis, -halfW, halfW);

            Vector3 clamped = center + fwd * fwdAxis + right * rightAxis;
            clamped.y = world.y;
            return clamped;
        }
    }
}
