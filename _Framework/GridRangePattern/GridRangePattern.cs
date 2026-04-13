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
        /// オフセットを90度単位で回転させたリストを返す。
        /// rotation: 0=前(+Y), 1=右(+X), 2=後(-Y), 3=左(-X)
        /// </summary>
        public List<Vector2Int> GetRotatedOffsets(int rotation)
        {
            rotation = ((rotation % 4) + 4) % 4;
            if (rotation == 0) return new List<Vector2Int>(offsets);

            var result = new List<Vector2Int>(offsets.Count);
            foreach (var o in offsets)
            {
                result.Add(RotateOffset(o, rotation));
            }
            return result;
        }

        /// <summary>
        /// 指定オフセットが、いずれかの回転方向で含まれるかチェック。
        /// 含まれる場合はその回転値(0~3)を返す。含まれなければ -1。
        /// </summary>
        public int ContainsAnyRotation(Vector2Int offset)
        {
            for (int r = 0; r < 4; r++)
            {
                var inv = RotateOffset(offset, (4 - r) % 4);
                if (offsets.Contains(inv)) return r;
            }
            return -1;
        }

        /// <summary>
        /// いずれかの回転で offset が含まれるか。
        /// </summary>
        public bool ContainsRotated(Vector2Int offset)
            => ContainsAnyRotation(offset) >= 0;

        private static Vector2Int RotateOffset(Vector2Int o, int rotation)
        {
            // 0=そのまま, 1=90°右(x,y)→(y,-x), 2=180°→(-x,-y), 3=90°左→(-y,x)
            return rotation switch
            {
                1 => new Vector2Int(o.y, -o.x),
                2 => new Vector2Int(-o.x, -o.y),
                3 => new Vector2Int(-o.y, o.x),
                _ => o,
            };
        }

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
