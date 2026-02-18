using System;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// フィールド上の「1マス」を表すデータクラス。
    /// このクラスの目的：
    /// - 「床(踏める)」「障害物(入れない)」「移動主体(今いる存在)」をスロット分離して保持し、
    ///   “床ギミックを置いたら通れなくなる”問題を避ける。
    /// - CubeInstance本体は保持しない（IDとCubeKindだけ）。本体管理は上位（Board等）が行う。
    /// </summary>
    [Serializable]
    public class FieldCell
    {
        // ─────────────────────────────────────────────────────────────
        // 形状・地形（地形ブロック / 変形マップ）
        // ─────────────────────────────────────────────────────────────

        [SerializeField] private bool isValid = true;          // そのマスが存在する（床がある）か。変形マップで false を使う
        [SerializeField] private bool terrainBlocked = false;  // 壁/穴など、地形として通れない

        // ─────────────────────────────────────────────────────────────
        // スロット：床（踏める／素通りできるキューブ）
        // ─────────────────────────────────────────────────────────────

        [SerializeField] private string floorCubeId = null;
        [SerializeField] private CubeKind floorKind = CubeKind.Event; // floorCubeIdがある時だけ意味を持つ

        // ─────────────────────────────────────────────────────────────
        // スロット：障害物（入れない／近接アクションするキューブ）
        // ─────────────────────────────────────────────────────────────

        [SerializeField] private string blockerCubeId = null;
        [SerializeField] private CubeKind blockerKind = CubeKind.Event; // blockerCubeIdがある時だけ意味を持つ

        // ─────────────────────────────────────────────────────────────
        // スロット：移動主体（プレイヤー等）
        // ─────────────────────────────────────────────────────────────

        [SerializeField] private string actorCubeId = null;
        [SerializeField] private CubeKind actorKind = CubeKind.Player; // actorCubeIdがある時だけ意味を持つ

        // ─────────────────────────────────────────────────────────────
        // 基本プロパティ
        // ─────────────────────────────────────────────────────────────

        public bool IsValid
        {
            get => isValid;
            set => isValid = value;
        }

        public bool TerrainBlocked
        {
            get => terrainBlocked;
            set => terrainBlocked = value;
        }

        /// <summary>
        /// 地形として侵入可能か（床が存在し、地形ブロックではない）
        /// </summary>
        public bool CanEnterTerrainOnly()
        {
            return isValid && !terrainBlocked;
        }

        // ─────────────────────────────────────────────────────────────
        // Floor
        // ─────────────────────────────────────────────────────────────

        public bool HasFloor => !string.IsNullOrEmpty(floorCubeId);
        public string FloorCubeId => floorCubeId;
        public CubeKind FloorKind => floorKind;

        public void SetFloor(string cubeId, CubeKind kind)
        {
            floorCubeId = string.IsNullOrEmpty(cubeId) ? null : cubeId;
            floorKind = kind;
        }

        public void ClearFloor()
        {
            floorCubeId = null;
        }

        // ─────────────────────────────────────────────────────────────
        // Blocker
        // ─────────────────────────────────────────────────────────────

        public bool HasBlocker => !string.IsNullOrEmpty(blockerCubeId);
        public string BlockerCubeId => blockerCubeId;
        public CubeKind BlockerKind => blockerKind;

        public void SetBlocker(string cubeId, CubeKind kind)
        {
            blockerCubeId = string.IsNullOrEmpty(cubeId) ? null : cubeId;
            blockerKind = kind;
        }

        public void ClearBlocker()
        {
            blockerCubeId = null;
        }

        // ─────────────────────────────────────────────────────────────
        // Actor
        // ─────────────────────────────────────────────────────────────

        public bool HasActor => !string.IsNullOrEmpty(actorCubeId);
        public string ActorCubeId => actorCubeId;
        public CubeKind ActorKind => actorKind;

        public void SetActor(string cubeId, CubeKind kind)
        {
            actorCubeId = string.IsNullOrEmpty(cubeId) ? null : cubeId;
            actorKind = kind;
        }

        public void ClearActor()
        {
            actorCubeId = null;
        }

        // ─────────────────────────────────────────────────────────────
        // 移動判定用
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// デフォルトの侵入可否（移動だけを考える）
        /// - 地形がダメなら不可
        /// - Blockerがあれば不可（戦闘/宝箱/街などの“入れない”はここで止める）
        /// - Floorは侵入可否に影響しない（踏める前提）
        /// </summary>
        public bool IsEnterableByDefault()
        {
            if (!CanEnterTerrainOnly()) return false;
            if (HasBlocker) return false;
            return true;
        }

        /// <summary>
        /// デバッグ用初期化（配置全クリア）
        /// </summary>
        public void Reset(bool valid, bool blocked)
        {
            isValid = valid;
            terrainBlocked = blocked;

            floorCubeId = null;
            blockerCubeId = null;
            actorCubeId = null;
        }
    }
}
