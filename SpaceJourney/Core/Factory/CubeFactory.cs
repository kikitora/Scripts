using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// フィールド上に配置される「CubeInstance（セーブ可能なキューブ状態）」を生成するFactory。
    /// - CubeId（GUID）を発行し、座標や種別、見た目参照ID（cubePrefabId / groundId）を埋める
    /// - ランタイム実体（CubeUnit / GroundGO）はここでは生成しない（別フェーズ）
    /// </summary>
    public static class CubeFactory
    {
        /// <summary>
        /// 最小限の情報で CubeInstance を生成する。
        /// 見た目ID（cubePrefabId/groundId）は後回し可能（空でも可：ただし表示生成はできない）。
        /// </summary>
        public static CubeInstance Create(
            CubeKind kind,
            int fieldX,
            int fieldY,
            UpperSideNum floorIndex = UpperSideNum.up,
            Dir direction = Dir.South,
            string cubePrefabId = "",
            string groundId = "",
            string visualVariantId = "",
            IEnumerable<string> initialSoulIds = null,
            int vp = 0,
            int ep = 0,
            string cubeId = null)
        {
            var cube = new CubeInstance();

            // Identity
            cube.CubeId = string.IsNullOrEmpty(cubeId) ? Guid.NewGuid().ToString("N") : cubeId;

            // Kind / Position / Facing
            cube.CubeDefId = kind;
            cube.FieldX = fieldX;
            cube.FieldY = fieldY;
            cube.FloorIndex = floorIndex;
            cube.Direction = direction;

            // Visual references (optional for now)
            cube.CubePrefabId = cubePrefabId ?? "";
            cube.GroundId = groundId ?? "";
            cube.VisualVariantId = visualVariantId ?? "";

            // Souls (IDs only)
            cube.SoulIds = initialSoulIds != null ? new List<string>(initialSoulIds) : new List<string>();

            // Resources
            cube.Vp = vp;
            cube.Ep = ep;

            return cube;
        }

        /// <summary>
        /// 便利：フィールド座標だけ指定して、まず置けるキューブを作る（見た目IDは空）。
        /// 初期配置やセーブ/ロード検証用。
        /// </summary>
        public static CubeInstance CreateEmptyVisual(
            CubeKind kind,
            Vector2Int boardPos,
            UpperSideNum floorIndex = UpperSideNum.up,
            Dir direction = Dir.South,
            IEnumerable<string> initialSoulIds = null,
            int vp = 0,
            int ep = 0,
            string cubeId = null)
        {
            return Create(
                kind,
                boardPos.x,
                boardPos.y,
                floorIndex,
                direction,
                cubePrefabId: "",
                groundId: "",
                visualVariantId: "",
                initialSoulIds: initialSoulIds,
                vp: vp,
                ep: ep,
                cubeId: cubeId);
        }
    }
}
