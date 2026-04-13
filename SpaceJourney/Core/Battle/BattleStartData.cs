using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 戦闘開始に必要な全データ。
    /// BattleManager.StartBattle() に渡す。
    /// </summary>
    [Serializable]
    public class BattleStartData
    {
        public BattleFieldLayout fieldLayout;
        public List<BattleUnitPlacement> allyUnits;
        public List<BattleUnitPlacement> enemyUnits;

        /// <summary>先制側: 0=味方, 1=敵, -1=なし</summary>
        public int initiativeSide = -1;

        /// <summary>味方サイドの士気 (0~100)</summary>
        public float allyMorale = 100f;

        /// <summary>敵サイドの士気 (0~100)</summary>
        public float enemyMorale = 100f;
    }

    /// <summary>
    /// 戦場のレイアウト定義。
    /// cells で可変形のマス配置を表現する。
    /// </summary>
    [Serializable]
    public class BattleFieldLayout
    {
        /// <summary>戦場中心のワールド座標</summary>
        public Vector3 worldPosition;

        /// <summary>戦場の向き (味方がどちら側を向くか)</summary>
        public Quaternion worldRotation = Quaternion.identity;

        /// <summary>
        /// マスの相対座標リスト。
        /// x=前後 (0=前列, 大きいほど後列), y=横方向。
        /// 両サイドとも同じ形状。
        /// </summary>
        public Vector2Int[] cells;

        /// <summary>面数 (通常2: 味方/敵)</summary>
        public int sideCount = 2;

        /// <summary>1マスのワールドサイズ</summary>
        public float cellSize = 1f;

        /// <summary>標準の 3x3 レイアウトを生成</summary>
        public static BattleFieldLayout Default3x3()
        {
            var cells = new Vector2Int[9];
            int i = 0;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    cells[i++] = new Vector2Int(x, y);

            return new BattleFieldLayout
            {
                worldPosition = Vector3.zero,
                worldRotation = Quaternion.identity,
                cells = cells,
                sideCount = 2,
                cellSize = 1f,
            };
        }

        /// <summary>標準の 5x5 レイアウトを生成</summary>
        public static BattleFieldLayout Default5x5()
        {
            var cells = new Vector2Int[25];
            int i = 0;
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    cells[i++] = new Vector2Int(x, y);

            return new BattleFieldLayout
            {
                worldPosition = Vector3.zero,
                worldRotation = Quaternion.identity,
                cells = cells,
                sideCount = 2,
                cellSize = 1f,
            };
        }
    }

    /// <summary>
    /// 戦闘に参加する1ユニットの配置情報。
    /// </summary>
    [Serializable]
    public class BattleUnitPlacement
    {
        public SoulInstance soul;
        public BodyInstance body;

        /// <summary>戦場グリッド上の配置位置 (BattleFieldLayout.cells 内の座標)</summary>
        public Vector2Int battleCell;

        public BattleUnitPlacement() { }

        public BattleUnitPlacement(SoulInstance soul, BodyInstance body, Vector2Int battleCell)
        {
            this.soul = soul;
            this.body = body;
            this.battleCell = battleCell;
        }
    }
}
