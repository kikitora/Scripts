using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// シンプル投射体。前方 (transform.forward) に一定速度で飛び、
    /// lifeSec 経過 or 指定距離に到達で自滅。
    /// Effect Event で Spawn → 勝手に飛ぶ。
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("飛翔速度 (m/s)")]
        public float speed = 15f;
        [Tooltip("生存秒 (0以下で無制限)")]
        public float lifeSec = 2f;
        [Tooltip("最大飛翔距離 (m)。0以下で無制限")]
        public float maxDistance = 0f;
        [Tooltip("重力影響 (放物線)。0=直進")]
        public float gravity = 0f;

        private float elapsed = 0f;
        private Vector3 startPos;
        private Vector3 velocity;

        private void Start()
        {
            startPos = transform.position;
            velocity = transform.forward * speed;
        }

        private void Update()
        {
            // 倍速対応: 戦闘 delta を適用 (battle speed が変われば投射体もそれに追随)
            float dt = Time.deltaTime * RealtimeBattleManager.GlobalSpeed;

            // 重力加算
            if (gravity > 0f) velocity += Vector3.down * gravity * dt;
            transform.position += velocity * dt;

            // 飛翔方向に向き直し (放物線時に見栄え)
            if (velocity.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            elapsed += dt;
            if (lifeSec > 0f && elapsed >= lifeSec) { Destroy(gameObject); return; }
            if (maxDistance > 0f && Vector3.Distance(startPos, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}
