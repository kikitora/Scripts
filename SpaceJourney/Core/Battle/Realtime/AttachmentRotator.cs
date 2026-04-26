using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// 手装備 (矢等) を「開始ポーズ」→「ターゲット方向へ aim したポーズ」へ時間補間。
    /// ArrowEqOn (SetActive true) で計測開始。duration 経過時点で aim 完了。
    /// これにより発射直前には既に aim が合っている。
    /// </summary>
    public class AttachmentRotator : MonoBehaviour
    {
        [Tooltip("開始ポーズ (親 bone local 回転に適用される euler)。例: Z=90")]
        public Vector3 startLocalEuler;
        [Tooltip("終了ポーズの追加回転 (tip 軸補正等)。aim direction に掛け合わせる")]
        public Vector3 endLocalEulerOffset;
        [Tooltip("Start→aim 到達までの秒数")]
        public float duration;
        [Tooltip("ターゲット取得用の caster 参照")]
        public RealtimeBattleUnit caster;

        private float elapsed;
        private bool snapshotReady;
        private Quaternion snapshotStartWorldRot;
        private Vector3 snapshotAimPos;

        private void OnEnable()
        {
            elapsed = 0f;
            snapshotReady = false;
        }

        private void LateUpdate()
        {
            if (!snapshotReady)
            {
                // 開始 world 回転 (親.rotation × startLocalEuler)
                Quaternion parentRot = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
                snapshotStartWorldRot = parentRot * Quaternion.Euler(startLocalEuler);
                // Aim 位置 (ターゲット有ればその位置、無ければ前方)
                var tgt = caster != null ? caster.CurrentTarget : null;
                if (tgt != null && tgt.IsAlive())
                    snapshotAimPos = tgt.transform.position;
                else
                    snapshotAimPos = transform.position + (caster != null ? caster.transform.forward : transform.forward) * 5f;
                snapshotReady = true;
            }

            elapsed += Time.deltaTime * RealtimeBattleManager.GlobalSpeed;
            float t = duration > 0.001f ? Mathf.Clamp01(elapsed / duration) : 1f;

            Vector3 dir = snapshotAimPos - transform.position; dir.y = 0f;
            Quaternion aimRot = (dir.sqrMagnitude > 0.01f)
                ? Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(endLocalEulerOffset)
                : snapshotStartWorldRot;

            transform.rotation = Quaternion.Slerp(snapshotStartWorldRot, aimRot, t);

            if (t >= 1f) enabled = false;
        }
    }
}
