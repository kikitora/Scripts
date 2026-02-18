// GridRangePattern.cs
// このクラスで何をするか：
// バトルやフィールドで共通して使う「マス範囲」を表現するデータクラスです。
// - グリッドの幅・高さ
// - 中心(0,0)からの相対座標 List<Vector2Int>
// を保持します。
// 座標仕様：画面上方向（前方）が +Y。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    [Serializable]
    public class GridRangePattern
    {
        /// <summary>
        /// 座標の表現バージョン
        /// 0: 旧（前方が -Y だった時代）
        /// 1: 新（前方が +Y）
        /// </summary>
        [SerializeField] private int coordinateVersion = CurrentCoordinateVersion;

        public const int CurrentCoordinateVersion = 1;

        // ★デフォルトは「3×6」に戻す（君の想定通り）
        [SerializeField] private int gridWidth = 3;
        [SerializeField] private int gridHeight = 6;

        [SerializeField] private List<Vector2Int> offsets = new List<Vector2Int>();

        public int CoordinateVersion => coordinateVersion;
        public int GridWidth => Mathf.Max(1, gridWidth);
        public int GridHeight => Mathf.Max(1, gridHeight);
        public IReadOnlyList<Vector2Int> Offsets => offsets;
        public List<Vector2Int> RawOffsets => offsets;

        public bool Contains(Vector2Int offset) => offsets.Contains(offset);

        /// <summary>
        /// 旧仕様(0)のデータを、新仕様(1)に移行する。
        /// - 旧：前方=-Y
        /// - 新：前方=+Y
        /// なので、y を反転する。
        /// </summary>
        public void EnsureCurrentCoordinateVersion()
        {
            if (coordinateVersion >= CurrentCoordinateVersion) return;

            for (int i = 0; i < offsets.Count; i++)
            {
                var o = offsets[i];
                offsets[i] = new Vector2Int(o.x, -o.y);
            }

            coordinateVersion = CurrentCoordinateVersion;
        }

        /// <summary>
        /// 未使用（隠れてる）領域を「デフォルト」に戻す用。
        /// </summary>
        public void ResetToDefault()
        {
            coordinateVersion = CurrentCoordinateVersion;
            gridWidth = 3;
            gridHeight = 6;
            offsets.Clear();
        }
    }
}
