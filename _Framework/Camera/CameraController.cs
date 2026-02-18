using DG.Tweening;
using System.Collections;
using UnityEngine;
using static Constants;
public class CameraController : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    // 直接値指定で動かす版
    public IEnumerator MoveCameraCoroutine(
        Camera cam,
        Vector3 localPos,
        Vector3 eulerAngles,
        float fov,
        float duration = 0.5f,
        Ease ease = Ease.OutQuad)
    {
        if (!cam) yield break;

        cam.transform.DOKill();
        cam.DOKill();

        var seq = DOTween.Sequence()
            .SetLink(cam.gameObject);

        seq.Join(cam.transform.DOLocalMove(localPos, duration).SetEase(ease));
        seq.Join(cam.transform.DOLocalRotate(eulerAngles, duration).SetEase(ease));
        seq.Join(cam.DOFieldOfView(fov, duration).SetEase(ease));

        yield return seq.WaitForCompletion();
    }

    // プリセットキーから動かす版
    public IEnumerator MoveCameraCoroutine(
        Camera cam,
        CameraKey key,
        float duration = 0.5f,
        Ease ease = Ease.OutQuad)
    {
        var def = GetCamera(key);
        if (def == null) yield break;

        yield return MoveCameraCoroutine(
            cam,
            def.LocalPos,
            def.EulerAngles,
            def.FieldOfView,
            duration,
            ease
        );
    }

    // ============================================================================
    //  RUNTIME API ── ここから実際に動かす関数群
    // ============================================================================

    public IEnumerator ChangeModeToPlayerFollow()
    {
        yield return StartCoroutine(MoveCameraCoroutine(
            playerCamera,
            CameraKey.PlayerFollow
        ));
    }

    public IEnumerator ChangeModeToFormation()
    {
        yield return StartCoroutine(MoveCameraCoroutine(
            playerCamera,
            CameraKey.Formation
        ));
    }
}
