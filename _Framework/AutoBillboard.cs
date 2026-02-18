using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[DisallowMultipleComponent]
public class AutoBillboard : MonoBehaviour
{
    public enum BillboardMode { FullFace, YAxisOnly }

    [Header("向き方")]
    public BillboardMode mode = BillboardMode.FullFace;

    [Header("更新間引き（1=毎フレーム, 2=隔フレーム…）")]
    [Range(1, 6)] public int updateInterval = 1;

    [Header("画面外はスキップして軽量化")]
    public bool skipWhenOffscreen = true;

    // 直近で「ゲームビュー」に描画したカメラ（RT出力は除外）
    private static Camera s_LastGameCam;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRenderingSRP;
        Camera.onPreCull += OnPreCullBuiltin;
        FaceOnce(); // 有効化時に一度合わせる
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRenderingSRP;
        Camera.onPreCull -= OnPreCullBuiltin;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) FaceOnce();
    }
#endif

    private void LateUpdate()
    {
        if (updateInterval > 1 && (Time.frameCount % updateInterval) != 0)
            return;

        var cam = GetActiveGameCamera();
        if (!cam) return;

        if (skipWhenOffscreen && !IsVisibleInCamera(cam))
            return;

        FaceTo(cam);
    }

    private void FaceOnce()
    {
        var cam = GetActiveGameCamera();
        if (!cam) return;
        if (skipWhenOffscreen && !IsVisibleInCamera(cam)) return;
        FaceTo(cam);
    }

    private void FaceTo(Camera cam)
    {
        var toCam = cam.transform.position - transform.position;
        if (mode == BillboardMode.YAxisOnly) toCam.y = 0f;
        if (toCam.sqrMagnitude < 1e-8f) return;

        transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
    }

    // ───── カメラ取得（ゲームビューのみ。RenderTexture出力は除外） ─────
    private static bool IsGameViewCamera(Camera cam)
    {
        if (!cam || !cam.enabled) return false;
        if (cam.cameraType != CameraType.Game) return false;
        if (cam.targetTexture != null) return false; // RTは除外
        return true;
    }

    private Camera GetActiveGameCamera()
    {
        if (s_LastGameCam && s_LastGameCam.gameObject.activeInHierarchy) return s_LastGameCam;

        // フォールバック：有効なゲーム用カメラでRTなし
        foreach (var c in Camera.allCameras)
            if (IsGameViewCamera(c)) return c;

        return null;
    }

    private static void OnBeginCameraRenderingSRP(ScriptableRenderContext _, Camera cam)
    {
        if (IsGameViewCamera(cam)) s_LastGameCam = cam;
    }

    private static void OnPreCullBuiltin(Camera cam)
    {
        if (IsGameViewCamera(cam)) s_LastGameCam = cam;
    }

    // ───── 可視判定（画面外スキップ用） ─────
    private bool IsVisibleInCamera(Camera cam)
    {
        // Renderer があればバウンディングで判定（より確実）
        var rend = GetComponentInChildren<Renderer>();
        if (rend)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, rend.bounds);
        }

        // Renderer が無い場合は位置点で簡易チェック
        var vp = cam.WorldToViewportPoint(transform.position);
        return vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
    }
}
