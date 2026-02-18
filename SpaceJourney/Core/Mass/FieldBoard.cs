using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// フィールド上の「移動だけ」を成立させるための盤面管理クラス。
    /// 
    /// - 盤面は FieldCell の 2D配列で保持（ノーマルは 9×9：外周壁込み）。
    /// - Actor（プレイヤー等）の移動は「地形＋Blockerの有無」だけで判定する。
    /// - Floor（踏める床ギミック）/Blocker（入れない）/Actor（移動主体）の3スロットを持つCellを前提。
    /// - アクション（踏んだ/近接/衝突など）は空のフック関数にしてある（後で実装）。
    /// 
    /// 注意：
    /// - CubeInstance 本体はこのクラスでは保持しない（必要なら外側で辞書管理）。
    /// - まずは「座標更新・占有更新」が正しく動くことを目的に最小構成にしている。
    /// </summary>
    public class FieldBoard
    {
        // ─────────────────────────────────────────────────────────────
        // 内部データ
        // ─────────────────────────────────────────────────────────────

        private readonly int width;
        private readonly int height;
        private readonly FieldCell[,] cells;

        // Actor の現在位置（actorId -> 座標）
        private readonly Dictionary<string, Vector2Int> actorPosById = new Dictionary<string, Vector2Int>();

        // ─────────────────────────────────────────────────────────────
        // 公開：基本情報
        // ─────────────────────────────────────────────────────────────

        public int Width => width;
        public int Height => height;

        public FieldBoard(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException($"Invalid board size: {width}x{height}");

            this.width = width;
            this.height = height;

            cells = new FieldCell[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cells[x, y] = new FieldCell();
        }

        /// <summary>
        /// ノーマル用：9×9（外周壁込み）を作り、外周を地形ブロックにする。
        /// 内側の可動域は (1..7, 1..7)。
        /// </summary>
        public static FieldBoard CreateNormal9x9WithBorderWalls()
        {
            var board = new FieldBoard(9, 9);

            for (int x = 0; x < board.width; x++)
                for (int y = 0; y < board.height; y++)
                {
                    bool isBorder = (x == 0 || y == 0 || x == board.width - 1 || y == board.height - 1);

                    // ノーマルは全マス存在する（isValid=true）
                    board.cells[x, y].IsValid = true;
                    // 外周だけ地形として通行不可
                    board.cells[x, y].TerrainBlocked = isBorder;
                }

            return board;
        }

        // ─────────────────────────────────────────────────────────────
        // セル取得・座標チェック
        // ─────────────────────────────────────────────────────────────

        public bool IsInside(int x, int y)
            => (x >= 0 && y >= 0 && x < width && y < height);

        public FieldCell GetCell(int x, int y)
        {
            if (!IsInside(x, y)) return null;
            return cells[x, y];
        }

        // ─────────────────────────────────────────────────────────────
        // 変形マップ対応：有効/無効を切る（最小）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// そのマスが「存在しない（床がない）」状態にする。
        /// 例：ボス用の変形フィールドで穴あきを作る時など。
        /// </summary>
        public void SetInvalidCell(int x, int y)
        {
            var c = GetCell(x, y);
            if (c == null) return;

            c.IsValid = false;
            c.TerrainBlocked = true; // 念のためブロックもONにしておく
            c.ClearFloor();
            c.ClearBlocker();
            c.ClearActor();
        }

        /// <summary>
        /// そのマスを有効化する（存在する床に戻す）
        /// </summary>
        public void SetValidCell(int x, int y, bool terrainBlocked)
        {
            var c = GetCell(x, y);
            if (c == null) return;

            c.IsValid = true;
            c.TerrainBlocked = terrainBlocked;
        }

        // ─────────────────────────────────────────────────────────────
        // 配置（最小）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Actor を配置する（移動主体）。既に居る場合は失敗。
        /// </summary>
        public bool TryPlaceActor(string actorId, CubeKind actorKind, int x, int y)
        {
            if (string.IsNullOrEmpty(actorId)) return false;

            var cell = GetCell(x, y);
            if (cell == null) return false;
            if (!cell.CanEnterTerrainOnly()) return false;  // 床がない/地形ブロック
            if (cell.HasActor) return false;
            if (cell.HasBlocker) return false;              // “入れない”の上には置かない（暫定）

            cell.SetActor(actorId, actorKind);
            actorPosById[actorId] = new Vector2Int(x, y);
            return true;
        }

        /// <summary>
        /// Blocker を配置する（入れないキューブ）。
        /// </summary>
        public bool TryPlaceBlocker(string cubeId, CubeKind kind, int x, int y)
        {
            if (string.IsNullOrEmpty(cubeId)) return false;

            var cell = GetCell(x, y);
            if (cell == null) return false;
            if (!cell.IsValid) return false;
            if (cell.TerrainBlocked) return false;
            if (cell.HasBlocker) return false;

            // 既にActorがいるなら置かない（暫定）
            if (cell.HasActor) return false;

            cell.SetBlocker(cubeId, kind);
            return true;
        }

        /// <summary>
        /// Floor を配置する（踏める床キューブ）。
        /// </summary>
        public bool TryPlaceFloor(string cubeId, CubeKind kind, int x, int y)
        {
            if (string.IsNullOrEmpty(cubeId)) return false;

            var cell = GetCell(x, y);
            if (cell == null) return false;
            if (!cell.IsValid) return false;
            if (cell.TerrainBlocked) return false;
            if (cell.HasFloor) return false;

            // FloorはActorと共存できる前提（踏める）
            cell.SetFloor(cubeId, kind);
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // 移動（本題）
        // ─────────────────────────────────────────────────────────────

        public enum MoveFailReason
        {
            None = 0,
            ActorNotFound,
            OutOfBoard,
            InvalidCell,
            TerrainBlocked,
            BlockerPresent,
            ActorAlreadyThere
        }

        public readonly struct MoveResult
        {
            public readonly bool moved;
            public readonly Vector2Int from;
            public readonly Vector2Int to;
            public readonly MoveFailReason failReason;

            // 衝突先のBlocker情報（moved=false の場合のみ入ることがある）
            public readonly string collidedBlockerId;
            public readonly CubeKind collidedBlockerKind;

            public MoveResult(bool moved, Vector2Int from, Vector2Int to, MoveFailReason failReason,
                string collidedBlockerId, CubeKind collidedBlockerKind)
            {
                this.moved = moved;
                this.from = from;
                this.to = to;
                this.failReason = failReason;
                this.collidedBlockerId = collidedBlockerId;
                this.collidedBlockerKind = collidedBlockerKind;
            }
        }

        /// <summary>
        /// Actor を 1マス移動させる（Dirベース）。
        /// アクション（踏んだ/衝突/近接）は空フックだけ呼ぶ。
        /// </summary>
        public MoveResult TryMoveActor(string actorId, Dir dir)
        {
            if (string.IsNullOrEmpty(actorId) || !actorPosById.TryGetValue(actorId, out var from))
                return new MoveResult(false, default, default, MoveFailReason.ActorNotFound, null, default);

            var delta = DirToDelta(dir);
            var to = from + delta;

            if (!IsInside(to.x, to.y))
                return new MoveResult(false, from, to, MoveFailReason.OutOfBoard, null, default);

            var fromCell = GetCell(from.x, from.y);
            var toCell = GetCell(to.x, to.y);

            if (toCell == null)
                return new MoveResult(false, from, to, MoveFailReason.OutOfBoard, null, default);

            if (!toCell.IsValid)
                return new MoveResult(false, from, to, MoveFailReason.InvalidCell, null, default);

            if (toCell.TerrainBlocked)
                return new MoveResult(false, from, to, MoveFailReason.TerrainBlocked, null, default);

            // 既に別Actorがいるなら今は不可（後で味方複数などに拡張）
            if (toCell.HasActor)
                return new MoveResult(false, from, to, MoveFailReason.ActorAlreadyThere, null, default);

            // Blockerがあるなら「移動はしない」扱い（衝突イベントはフックへ）
            if (toCell.HasBlocker)
            {
                OnBlockedByBlocker(actorId, from, to, toCell.BlockerCubeId, toCell.BlockerKind); // 空フック
                return new MoveResult(false, from, to, MoveFailReason.BlockerPresent, toCell.BlockerCubeId, toCell.BlockerKind);
            }

            // ここまで来たら移動成立：セル更新（Actorスロット移動）
            if (fromCell == null || !fromCell.HasActor || fromCell.ActorCubeId != actorId)
            {
                // 盤面と辞書がずれている（本来は起きない）。安全側で失敗にする。
                return new MoveResult(false, from, to, MoveFailReason.ActorNotFound, null, default);
            }

            // 移動
            var actorKind = fromCell.ActorKind;
            fromCell.ClearActor();
            toCell.SetActor(actorId, actorKind);
            actorPosById[actorId] = to;

            // 移動後：踏める床があれば「踏んだ」フック（空）
            if (toCell.HasFloor)
            {
                OnEnterFloor(actorId, to, toCell.FloorCubeId, toCell.FloorKind); // 空フック
            }

            // 移動後：近接フック（空）
            OnAfterMove(actorId, from, to);

            return new MoveResult(true, from, to, MoveFailReason.None, null, default);
        }

        // ─────────────────────────────────────────────────────────────
        // Dir → delta
        // ─────────────────────────────────────────────────────────────

        private static Vector2Int DirToDelta(Dir dir)
        {
            // SpaceJourneyCoreTypes.cs の Dir に合わせる想定
            // North/East/South/West か、Up/Right/Down/Left か名称が違う場合はここだけ直せばOK
            return dir switch
            {
                Dir.North => new Vector2Int(0, 1),
                Dir.East => new Vector2Int(1, 0),
                Dir.South => new Vector2Int(0, -1),
                Dir.West => new Vector2Int(-1, 0),
                _ => Vector2Int.zero
            };
        }

        // ─────────────────────────────────────────────────────────────
        // アクション：空フック（後で実装）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Blockerで移動が止められた時のフック（戦闘/宝箱/街/門番などの起点）。
        /// 今は空。
        /// </summary>
        protected virtual void OnBlockedByBlocker(string actorId, Vector2Int from, Vector2Int to, string blockerId, CubeKind blockerKind)
        {
            // TODO: implement later
        }

        /// <summary>
        /// Floorに侵入した時のフック（ワープ床/回復床/罠床などの起点）。今は空。
        /// </summary>
        protected virtual void OnEnterFloor(string actorId, Vector2Int pos, string floorId, CubeKind floorKind)
        {
            // TODO: implement later
        }

        /// <summary>
        /// 移動完了後のフック（近接判定などをここでまとめてやる想定）。今は空。
        /// </summary>
        protected virtual void OnAfterMove(string actorId, Vector2Int from, Vector2Int to)
        {
            // TODO: implement later
        }




        public bool TryGetActorPos(string actorId, out Vector2Int pos)
        {
            return actorPosById.TryGetValue(actorId, out pos);
        }
    }
}
