using System.Collections.Generic;
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
        [Tooltip("ON の場合だけ、戦闘開始時にフィールド中心からカメラ位置を自動計算する")]
        public bool autoArrangeCamera = false;
        [Tooltip("ON の場合、戦闘開始時に現在のカメラ位置・回転・FOVを維持する")]
        public bool preserveCurrentCameraPose = true;
        [Tooltip("ON の場合、戦闘開始時の自動計算カメラではなく fixedCamera* の値をそのまま使う")]
        public bool useFixedCameraPose = false;
        public Vector3 fixedCameraPosition = new Vector3(22.5f, 11.8f, 14.2f);
        public Vector3 fixedCameraEulerAngles = new Vector3(64.25f, 0f, 0f);
        [Range(1f, 179f)]
        public float fixedCameraFieldOfView = 60f;
        [Tooltip("ON の場合、現在のGameView縦横比に合わせて10x5戦場全体が画面内に入る距離を自動計算する")]
        public bool fitCameraToBattleField = true;
        [Tooltip("自動フレーミング時に戦場外へ足す余白 (mass)")]
        public float cameraFitPadding = 1.2f;
        [Tooltip("コメントUIを避けるため、注視点を味方側へずらして盤面を画面上側へ寄せる量 (mass)")]
        public float cameraUiSafeForwardOffset = -1.8f;

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

            if (!autoArrangeCamera)
                return;

            if (preserveCurrentCameraPose)
                return;

            if (useFixedCameraPose)
            {
                cam.transform.SetPositionAndRotation(
                    fixedCameraPosition,
                    Quaternion.Euler(fixedCameraEulerAngles));
                cam.fieldOfView = fixedCameraFieldOfView;
                return;
            }

            ArrangeCameraForCurrentField(cam);
        }

        public void ArrangeCameraForCurrentField(Camera cam = null)
        {
            cam = cam != null ? cam : (battleCamera != null ? battleCamera : Camera.main);
            if (cam == null) return;

            Vector3 lookTarget = battleCenter;
            lookTarget += forward * cameraUiSafeForwardOffset;
            lookTarget.y = surfaceY;
            float halfDepth = gridDepth * cellSize;
            float halfWidth = gridWidth * cellSize * 0.5f;

            ArrangeCameraAt(cam, lookTarget, halfDepth, halfWidth);
        }

        public void ArrangeCameraForWorldPoints(IEnumerable<Vector3> worldPoints, Camera cam = null)
        {
            cam = cam != null ? cam : (battleCamera != null ? battleCamera : Camera.main);
            if (cam == null) return;

            float halfFieldDepth = gridDepth * cellSize;
            float halfFieldWidth = gridWidth * cellSize * 0.5f;
            float minF = -halfFieldDepth;
            float maxF = halfFieldDepth;
            float minR = -halfFieldWidth;
            float maxR = halfFieldWidth;
            const float unitMargin = 0.75f;

            if (worldPoints != null)
            {
                foreach (var p in worldPoints)
                {
                    Vector3 local = p - battleCenter;
                    float f = Vector3.Dot(local, forward);
                    float r = Vector3.Dot(local, right);
                    minF = Mathf.Min(minF, f - unitMargin);
                    maxF = Mathf.Max(maxF, f + unitMargin);
                    minR = Mathf.Min(minR, r - unitMargin);
                    maxR = Mathf.Max(maxR, r + unitMargin);
                }
            }

            Vector3 lookTarget = battleCenter
                                 + forward * ((minF + maxF) * 0.5f)
                                 + right * ((minR + maxR) * 0.5f);
            lookTarget += forward * cameraUiSafeForwardOffset;
            lookTarget.y = surfaceY;

            float halfDepth = (maxF - minF) * 0.5f;
            float halfWidth = (maxR - minR) * 0.5f;
            ArrangeCameraAt(cam, lookTarget, halfDepth, halfWidth);
        }

        private void ArrangeCameraAt(Camera cam, Vector3 lookTarget, float halfDepth, float halfWidth)
        {
            // カメラを Player 側後方の斜め上に配置 (フィールドを俯瞰)。
            // forward を画面の縦方向に流すことで、キューブ配置が X/Z どちら向きでも縦画面に収める。
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float distance = fitCameraToBattleField ? CalculateFitCameraDistance(cam, angleRad, halfDepth, halfWidth) : cameraDistance;
            float height = Mathf.Sin(angleRad) * distance;
            float horizontalDist = Mathf.Cos(angleRad) * distance;

            Vector3 camPos = lookTarget - forward * horizontalDist + Vector3.up * height;
            cam.transform.position = camPos;
            cam.transform.LookAt(lookTarget);
        }

        private float CalculateFitCameraDistance(Camera cam, float angleRad)
        {
            float halfTotalDepth = gridDepth * cellSize;
            float halfWidth = gridWidth * cellSize * 0.5f;
            return CalculateFitCameraDistance(cam, angleRad, halfTotalDepth, halfWidth);
        }

        private float CalculateFitCameraDistance(Camera cam, float angleRad, float halfTotalDepth, float halfWidth)
        {
            float aspect = Mathf.Max(0.01f, cam.aspect);
            float padding = Mathf.Max(0f, cameraFitPadding);

            float verticalHalf = halfTotalDepth * Mathf.Cos(angleRad) + padding;
            float horizontalHalf = halfWidth + padding;
            float tanHalfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            if (tanHalfFov <= 0.001f) return cameraDistance;

            float byVertical = verticalHalf / tanHalfFov;
            float byHorizontal = horizontalHalf / (tanHalfFov * aspect);
            float fitDistance = Mathf.Max(byVertical, byHorizontal);

            return Mathf.Max(cameraDistance, fitDistance);
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
