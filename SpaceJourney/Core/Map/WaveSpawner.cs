// WaveSpawner.cs
// このクラスで何をするか：
// WAVEごとにフィールドへキューブをポップさせる司令塔。
// - WaveSpawnTableSO を読んで「何を何個出すか」を抽選
// - FieldBoard を参照して「プレイヤー隣接マスは除外」しながら空きマスを選ぶ
// - 実際に CubeInstance を生成して WorldState / FieldBoard に登録する
// - WAVE切り替え時は門番以外の既存キューブを消してから再抽選

using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    public class WaveSpawner : MonoBehaviour
    {
        // =========================================================
        // Inspector
        // =========================================================

        [Header("参照")]
        [Tooltip("WAVEポップ定義SO。難易度別に差し替える。")]
        [SerializeField] private WaveSpawnTableSO spawnTable;

        [Tooltip("WorldStateRuntime。生成したCubeInstanceを登録するために使う。")]
        [SerializeField] private WorldStateRuntime worldStateRuntime;

        // FieldBoardはFieldManagerから渡してもらう（Inspectorでは設定しない）
        private FieldBoard fieldBoard;

        [Header("キューブ生成設定")]
        [Tooltip("生成したキューブのGameObjectをまとめる親Transform。")]
        [SerializeField] private Transform cubeRoot;

        [Header("デバッグ")]
        [SerializeField] private bool logSpawnResults = true;

        // =========================================================
        // 内部状態
        // =========================================================

        // プレイヤーのアクターID（FieldBoard上での識別子）
        private const string PlayerActorId = "PLAYER_ACTOR";

        // 現在スポーン済みのキューブID → GameObjectのマップ
        private readonly Dictionary<string, GameObject> _spawnedCubeObjects
            = new Dictionary<string, GameObject>();

        // =========================================================
        // 公開API
        // =========================================================

        /// <summary>
        /// FieldManagerから FieldBoard を受け取る。
        /// FieldManager.Awake() から呼ばれる。
        /// </summary>
        public void SetFieldBoard(FieldBoard board)
        {
            fieldBoard = board;
        }

        /// <summary>
        /// WAVE切り替えを実行する。
        /// 門番以外のキューブを消して、新しいキューブを抽選＆配置する。
        /// </summary>
        /// <param name="mapRank">現在のエリアランク</param>
        public void OnWaveChanged(int mapRank)
        {
            ClearNonGatekeeperCubes();
            SpawnCubes(mapRank);
        }

        /// <summary>
        /// エリアに初めて入ったとき（WAVE途中）の初回生成。
        /// </summary>
        /// <param name="mapRank">現在のエリアランク</param>
        public void OnFirstEnterArea(int mapRank)
        {
            SpawnCubes(mapRank);
        }

        // =========================================================
        // ポップ処理
        // =========================================================

        private void SpawnCubes(int mapRank)
        {
            if (spawnTable == null)
            {
                Debug.LogWarning("[WaveSpawner] spawnTable が未設定です。");
                return;
            }

            var rankEntry = spawnTable.GetEntryForRank(mapRank);
            if (rankEntry == null)
            {
                Debug.LogWarning($"[WaveSpawner] mapRank={mapRank} に対応する RankEntry が見つかりません。");
                return;
            }

            // プレイヤー位置を取得（隣接禁止判定用）
            fieldBoard.TryGetActorPos(PlayerActorId, out var playerPos);

            // 空きマス一覧を取得（プレイヤー隣接を除く）
            var availableCells = GetAvailableCells(playerPos);

            if (availableCells.Count == 0)
            {
                Debug.LogWarning("[WaveSpawner] 配置可能なマスがありません。");
                return;
            }

            // 各エントリーを処理
            foreach (var entry in rankEntry.Entries)
            {
                ProcessEntry(entry, availableCells, rankEntry);

                if (availableCells.Count == 0) break; // 空きが尽きたら終了
            }
        }

        private void ProcessEntry(SpawnEntry entry, List<Vector2Int> availableCells, RankEntry rankEntry)
        {
            // minCount分は確定でスポーン
            int spawnCount = entry.MinCount;

            // spawnRateで参加するか判定し、参加したらmaxCountまで追加抽選
            if (Random.value <= entry.SpawnRate)
            {
                // minCount〜maxCountの範囲でランダムに決定
                spawnCount = Random.Range(entry.MinCount, entry.MaxCount + 1);
            }

            if (spawnCount <= 0) return;

            for (int i = 0; i < spawnCount; i++)
            {
                if (availableCells.Count == 0) return;

                // ランダムなマスを選ぶ
                int cellIndex = Random.Range(0, availableCells.Count);
                var cell = availableCells[cellIndex];
                availableCells.RemoveAt(cellIndex); // 使ったマスは除外

                // バリアントが指定されていれば抽選、なければcubeKindのみで決定
                var variant = entry.PickVariant();
                string enemyGroupId = variant?.EnemyGroupId;

                SpawnCubeAt(cell, entry.CubeKind, enemyGroupId, rankEntry);
            }

            if (logSpawnResults)
            {
                Debug.Log($"[WaveSpawner] {entry.CubeKind}: {spawnCount}個スポーン");
            }
        }

        private void SpawnCubeAt(Vector2Int boardPos, CubeKind kind, string enemyGroupId, RankEntry rankEntry)
        {
            var world = worldStateRuntime?.CurrentWorld;
            if (world == null)
            {
                Debug.LogWarning("[WaveSpawner] CurrentWorld が null です。");
                return;
            }

            var db = MasterDatabase.Instance;

            // エリアの地面IDをRankEntryから抽選
            string groundEntryId = rankEntry.PickGroundEntryId();
            if (string.IsNullOrEmpty(groundEntryId))
            {
                Debug.LogWarning("[WaveSpawner] groundVariants が未設定です。RankEntry を確認してください。");
                return;
            }

            // 敵キューブかどうかで建物の有無を分岐
            bool isEnemy = kind == CubeKind.EnemyNormal
                        || kind == CubeKind.EnemyElite
                        || kind == CubeKind.EnemyBoss
                        || kind == CubeKind.GateKeeper
                        || kind == CubeKind.RareBoss;

            // ─── CubeInstanceを生成 ─────────────────────────
            var cubeInstance = new CubeInstance();
            cubeInstance.CubeId = world.GenerateUniqueInstanceId();
            cubeInstance.CubeDefId = kind;
            cubeInstance.FieldX = boardPos.x;
            cubeInstance.FieldY = boardPos.y;
            cubeInstance.GroundId = groundEntryId;

            // 非敵キューブは建物IDをCubeDefinitionから取得
            string cubeEntryId = null;
            if (!isEnemy)
            {
                var def = db?.GetCubeDefinitionById(kind.ToString());
                if (def == null)
                {
                    Debug.LogWarning($"[WaveSpawner] CubeDefinition が見つかりません: {kind}");
                    return;
                }
                cubeEntryId = def.CubeEntryId;
                cubeInstance.CubePrefabId = cubeEntryId;
            }

            world.RegisterCubeInstance(cubeInstance);

            // FieldBoardにブロッカーとして登録
            fieldBoard.TryPlaceBlocker(cubeInstance.CubeId, kind, boardPos.x, boardPos.y);

            // ─── GameObjectを生成 ───────────────────────────
            Vector3 worldPos = BoardToWorld(boardPos);
            GameObject buildingGO = null;

            if (!isEnemy)
            {
                // 非敵：建物を生成
                if (string.IsNullOrEmpty(cubeEntryId))
                {
                    Debug.LogWarning($"[WaveSpawner] cubeEntryId が未設定です: {kind}");
                    return;
                }

                var buildingPrefab = db.GetCubeById(cubeEntryId);
                if (buildingPrefab == null)
                {
                    Debug.LogWarning($"[WaveSpawner] cubeEntry が見つかりません: {cubeEntryId}");
                    return;
                }

                buildingGO = Instantiate(buildingPrefab, worldPos, Quaternion.identity, cubeRoot);
                buildingGO.name = $"[Cube] {kind}_{cubeInstance.CubeId}";

                var cubeUnit = buildingGO.GetComponent<CubeUnit>();
                if (cubeUnit != null)
                    cubeInstance.SetRuntimeCubeUnit(cubeUnit);
            }
            else
            {
                // 敵：建物なし。空のGameObjectをルートとして使う
                buildingGO = new GameObject($"[Cube] {kind}_{cubeInstance.CubeId}");
                buildingGO.transform.SetParent(cubeRoot);
                buildingGO.transform.position = worldPos;
            }

            // ─── 地面を配置 ─────────────────────────────────
            var groundPrefab = db.GetCubeGroundById(groundEntryId);
            if (groundPrefab != null)
            {
                var groundParent = FindGroundParent(buildingGO.transform) ?? buildingGO.transform;
                var groundGO = Instantiate(groundPrefab, groundParent);
                groundGO.name = $"[CubeGround] {kind}_{cubeInstance.CubeId}";
                groundGO.transform.localPosition = Vector3.zero;
                groundGO.transform.localRotation = Quaternion.identity;
                cubeInstance.SetRuntimeGroundGO(groundGO);
            }
            else
            {
                Debug.LogWarning($"[WaveSpawner] cubeGroundEntry が見つかりません: {groundEntryId}");
            }

            // スポーン済み管理
            _spawnedCubeObjects[cubeInstance.CubeId] = buildingGO;

            if (logSpawnResults)
            {
                string groupInfo = isEnemy ? $"group={enemyGroupId}" : kind.ToString();
                Debug.Log($"[WaveSpawner] Spawned {kind}({groupInfo}) at {boardPos}  id={cubeInstance.CubeId}");
            }
        }

        // =========================================================
        // クリア処理
        // =========================================================

        /// <summary>
        /// 門番キューブ以外をすべて消す。
        /// </summary>
        private void ClearNonGatekeeperCubes()
        {
            var world = worldStateRuntime?.CurrentWorld;
            if (world == null) return;

            var toRemove = new List<CubeInstance>();

            foreach (var cube in world.ExCubes)
            {
                if (cube == null) continue;
                if (cube.CubeDefId == CubeKind.GateKeeper) continue; // 門番は残す

                toRemove.Add(cube);
            }

            foreach (var cube in toRemove)
            {
                // WorldStateから削除
                world.ExCubes.Remove(cube);

                // FieldBoardから削除
                fieldBoard.RemoveBlocker(cube.CubeId);

                // GameObjectを削除
                if (_spawnedCubeObjects.TryGetValue(cube.CubeId, out var go))
                {
                    Destroy(go);
                    _spawnedCubeObjects.Remove(cube.CubeId);
                }
            }

            if (logSpawnResults)
            {
                Debug.Log($"[WaveSpawner] {toRemove.Count}個のキューブをクリアしました。");
            }
        }

        // =========================================================
        // 空きマス取得
        // =========================================================

        /// <summary>
        /// 配置可能なマス一覧を返す。
        /// - フィールド内（1〜7）
        /// - 空きマス（HasBlocker / HasActor でない）
        /// - プレイヤーと隣接していない
        /// </summary>
        private List<Vector2Int> GetAvailableCells(Vector2Int playerPos)
        {
            var result = new List<Vector2Int>();

            // 7×7の内側（外周壁を除く 1〜7）
            for (int x = 1; x <= 7; x++)
            {
                for (int y = 1; y <= 7; y++)
                {
                    var cell = fieldBoard.GetCell(x, y);
                    if (cell == null) continue;
                    if (!cell.IsValid) continue;
                    if (cell.TerrainBlocked) continue;
                    if (cell.HasActor) continue;
                    if (cell.HasBlocker) continue;

                    // プレイヤー隣接（上下左右）は除外
                    var pos = new Vector2Int(x, y);
                    if (IsAdjacentToPlayer(pos, playerPos)) continue;

                    result.Add(pos);
                }
            }

            return result;
        }

        private bool IsAdjacentToPlayer(Vector2Int pos, Vector2Int playerPos)
        {
            int dx = Mathf.Abs(pos.x - playerPos.x);
            int dy = Mathf.Abs(pos.y - playerPos.y);
            // 上下左右の隣接（斜めは含まない）
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        // =========================================================
        // ヘルパー
        // =========================================================

        /// <summary>
        /// Transform以下を再帰的に探して "GroundParent" という名前のTransformを返す。
        /// 見つからない場合は null。
        /// </summary>
        private Transform FindGroundParent(Transform root)
        {
            if (root.name == "GroundParent") return root;

            for (int i = 0; i < root.childCount; i++)
            {
                var result = FindGroundParent(root.GetChild(i));
                if (result != null) return result;
            }
            return null;
        }

        // =========================================================
        // 座標変換
        // =========================================================

        private Vector3 BoardToWorld(Vector2Int boardPos)
        {
            float gridSize = SpaceJourneyConstants.CubeSideGridSize;
            Vector3 origin = cubeRoot != null ? cubeRoot.position : Vector3.zero;
            return new Vector3(
                origin.x + boardPos.x * gridSize,
                origin.y,
                origin.z + boardPos.y * gridSize
            );
        }
    }
}