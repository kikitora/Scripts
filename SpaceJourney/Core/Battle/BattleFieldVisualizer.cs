using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 2つのキューブ (PlayerCube / EnemyCube) を隣接配置し、
    /// バトルグリッド座標 (side, gridX, gridY) ↔ ワールド座標の変換を提供する。
    ///
    /// グリッド座標系 (BattleField と同一):
    ///   side 0 = 味方 (PlayerCube), side 1 = 敵 (EnemyCube)
    ///   gridX: 0=前列(敵に近い側), 4=後列
    ///   gridY: 0〜4 横方向
    ///
    /// 物理配置 (10×5):
    ///   Player後列(x=4) ... Player前列(x=0) | Enemy前列(x=0) ... Enemy後列(x=4)
    ///
    /// 起動時にランダムな向き (4方向) でキューブを配置する。
    /// </summary>
    public class BattleFieldVisualizer : MonoBehaviour
    {
        [Header("キューブ参照")]
        [Tooltip("味方側キューブ (side 0)")]
        public Transform playerCube;
        [Tooltip("敵側キューブ (side 1)")]
        public Transform enemyCube;

        [Header("グリッド設定")]
        [Tooltip("1セルのサイズ (キューブ上の1マス)")]
        public float cellSize = 1f;
        [Tooltip("グリッド幅 (y方向セル数)")]
        public int gridWidth = 5;
        [Tooltip("グリッド奥行き (x方向セル数、1サイドあたり)")]
        public int gridDepth = 5;

        [Header("カメラ")]
        [Tooltip("自動配置するカメラ (null なら Main Camera を使用)")]
        public Camera battleCamera;
        [Tooltip("カメラの俯瞰角度 (度)")]
        [Range(30, 90)]
        public float cameraAngle = 55f;
        [Tooltip("カメラとフィールド中心の距離")]
        public float cameraDistance = 12f;

        [Header("デバッグ")]
        public bool drawGizmos = true;

        // 戦場の中心
        private Vector3 battleCenter;
        // forward: Player前列→Enemy前列 方向, right: y軸+ 方向
        private Vector3 forward;
        private Vector3 right;
        // キューブ表面の高さ (Y)
        private float surfaceY;

        /// <summary>外部からアクセス用: Player→Enemy 方向のワールドベクトル</summary>
        public Vector3 Forward => forward;
        /// <summary>外部からアクセス用: y軸+ 方向のワールドベクトル</summary>
        public Vector3 RightAxis => right;
        /// <summary>外部からアクセス用: 戦場表面の Y 座標</summary>
        public float SurfaceY => surfaceY;

        private void Awake()
        {
            SetupRandomOrientation();
        }

        /// <summary>ランダム方向 (4方向) でキューブを配置</summary>
        public void SetupRandomOrientation()
        {
            int orientation = Random.Range(0, 4);
            Setup(orientation);
        }

        /// <summary>指定方向でキューブを配置 (0=+Z, 1=+X, 2=-Z, 3=-X)</summary>
        public void Setup(int orientation)
        {
            switch (orientation)
            {
                case 0: forward = Vector3.forward; right = Vector3.right; break;
                case 1: forward = Vector3.right;   right = Vector3.back;  break;
                case 2: forward = Vector3.back;     right = Vector3.left;  break;
                case 3: forward = Vector3.left;     right = Vector3.forward; break;
                default: forward = Vector3.forward; right = Vector3.right; break;
            }

            // 2つのキューブの中間点を戦場中心とする (XZ平面)
            battleCenter = (playerCube.position + enemyCube.position) * 0.5f;
            battleCenter.y = 0;
            float halfDepth = gridDepth * cellSize * 0.5f;

            // PlayerCube: 中心ラインから forward の反対側に配置
            playerCube.position = battleCenter - forward * halfDepth;
            // EnemyCube: 中心ラインから forward 側に配置
            enemyCube.position = battleCenter + forward * halfDepth;

            // キューブの回転: 視覚的に同じ向きにする
            playerCube.rotation = Quaternion.identity;
            enemyCube.rotation = Quaternion.identity;

            // 表面の高さを取得
            surfaceY = GetSurfaceY();

            Debug.Log($"[BattleFieldVisualizer] orientation={orientation} " +
                      $"forward={forward} center={battleCenter} surfaceY={surfaceY:F2}");

            SetupCamera();
        }

        /// <summary>カメラを戦場全体が見える位置に配置</summary>
        private void SetupCamera()
        {
            var cam = battleCamera != null ? battleCamera : Camera.main;
            if (cam == null) return;

            // フィールド中心の表面位置
            Vector3 lookTarget = battleCenter;
            lookTarget.y = surfaceY;

            // カメラを Player 側後方の斜め上に配置 (フィールドを俯瞰)
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float height = Mathf.Sin(angleRad) * cameraDistance;
            float horizontalDist = Mathf.Cos(angleRad) * cameraDistance;

            // Player 後方 (-forward 方向) からフィールドを見下ろす
            Vector3 camPos = lookTarget - forward * horizontalDist + Vector3.up * height;

            cam.transform.position = camPos;
            cam.transform.LookAt(lookTarget);
        }

        private float GetSurfaceY()
        {
            // PlayerCube の Renderer から上面を取得
            var renderer = playerCube.GetComponentInChildren<Renderer>();
            if (renderer != null)
                return renderer.bounds.max.y;
            // fallback: position.y + 想定高さ
            return playerCube.position.y + 0.5f;
        }

        /// <summary>
        /// バトルグリッド座標 → ワールド座標 (セル中心、表面上)
        /// </summary>
        public Vector3 GridToWorldPosition(int side, int gridX, int gridY)
        {
            // セル中心オフセット (0.5 を加えてセル中央に)
            float cellOffset = 0.5f * cellSize;

            // 前列方向 (forward 軸) のオフセット
            // side=0 (Player): x=0 → 中心ラインの手前 0.5, x=4 → 手前 4.5
            // side=1 (Enemy):  x=0 → 中心ラインの奥 0.5,  x=4 → 奥 4.5
            float forwardOffset;
            if (side == 0)
                forwardOffset = -(gridX * cellSize + cellOffset);
            else
                forwardOffset = gridX * cellSize + cellOffset;

            // 横方向 (right 軸) のオフセット: 中央揃え
            float halfWidth = gridWidth * cellSize * 0.5f;
            float rightOffset = gridY * cellSize + cellOffset - halfWidth;

            // XZ は battleCenter 基準、Y は表面高さ (絶対値)
            Vector3 pos = battleCenter
                          + forward * forwardOffset
                          + right * rightOffset;
            pos.y = surfaceY;
            return pos;
        }

        /// <summary>
        /// ワールド座標 → 最も近いグリッドセル (side, gridX, gridY)。
        /// 戻り値: Vector3Int(side, gridX, gridY)。範囲外なら (-1,-1,-1)。
        /// </summary>
        public Vector3Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 local = worldPos - battleCenter;
            float fwd = Vector3.Dot(local, forward);
            float rgt = Vector3.Dot(local, right);

            int side = fwd >= 0 ? 1 : 0;
            float absFwd = Mathf.Abs(fwd);

            float halfWidth = gridWidth * cellSize * 0.5f;

            int gridX = Mathf.FloorToInt(absFwd / cellSize);
            int gridY = Mathf.FloorToInt((rgt + halfWidth) / cellSize);

            if (gridX < 0 || gridX >= gridDepth || gridY < 0 || gridY >= gridWidth)
                return new Vector3Int(-1, -1, -1);

            return new Vector3Int(side, gridX, gridY);
        }

        /// <summary>キューブ間の境界線のワールド座標 (デバッグ用)</summary>
        public Vector3 GetCenterLinePosition(int gridY)
        {
            float halfWidth = gridWidth * cellSize * 0.5f;
            float rightOffset = gridY * cellSize + 0.5f * cellSize - halfWidth;
            Vector3 pos = battleCenter + right * rightOffset;
            pos.y = surfaceY;
            return pos;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (playerCube == null || enemyCube == null) return;

            // 実行中はセットアップ済みの値を使う、エディタ中は仮計算
            Vector3 fwd = Application.isPlaying ? forward : Vector3.forward;
            Vector3 rgt = Application.isPlaying ? right : Vector3.right;
            Vector3 center = Application.isPlaying
                ? battleCenter
                : (playerCube.position + enemyCube.position) * 0.5f;

            float sy;
            if (Application.isPlaying)
            {
                sy = surfaceY;
            }
            else
            {
                var rend = playerCube.GetComponentInChildren<Renderer>();
                sy = rend != null ? rend.bounds.max.y : playerCube.position.y + 2.5f;
            }

            float halfW = gridWidth * cellSize * 0.5f;

            for (int side = 0; side < 2; side++)
            {
                for (int x = 0; x < gridDepth; x++)
                {
                    for (int y = 0; y < gridWidth; y++)
                    {
                        float cellOffset = 0.5f * cellSize;
                        float forwardOff = side == 0
                            ? -(x * cellSize + cellOffset)
                            : (x * cellSize + cellOffset);
                        float rightOff = y * cellSize + cellOffset - halfW;

                        Vector3 pos = center
                                      + fwd * forwardOff
                                      + rgt * rightOff;
                        pos.y = sy;

                        // 色: 味方=青, 敵=赤, 前列ほど濃い
                        float alpha = 1f - x * 0.15f;
                        Gizmos.color = side == 0
                            ? new Color(0.2f, 0.4f, 1f, alpha)
                            : new Color(1f, 0.3f, 0.2f, alpha);

                        Gizmos.DrawWireCube(pos, new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f));

                        // 前列マーカー
                        if (x == 0)
                        {
                            Gizmos.color = side == 0
                                ? new Color(0, 1, 0, 0.5f)
                                : new Color(1, 1, 0, 0.5f);
                            Gizmos.DrawSphere(pos, 0.15f);
                        }
                    }
                }
            }

            // 中心ライン
            Gizmos.color = Color.white;
            Vector3 lineStart = center + rgt * (-halfW);
            lineStart.y = sy;
            Vector3 lineEnd = center + rgt * halfW;
            lineEnd.y = sy;
            Gizmos.DrawLine(lineStart, lineEnd);
        }
#endif
    }
}
