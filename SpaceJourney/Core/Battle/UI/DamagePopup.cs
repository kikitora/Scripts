using TMPro;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// ダメージ数値を被弾キャラ上に表示する1個のポップアップ。
    /// 上昇しながらフェードアウトして自動破棄される。
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        public float riseSpeed = 1.5f;
        public float lifeSec = 1.0f;

        private float elapsed;
        private Color baseColor = Color.white;
        private Camera cam;

        public void Setup(int value, Color color)
        {
            if (text == null) text = GetComponentInChildren<TextMeshPro>();
            if (text != null)
            {
                text.text = value.ToString();
                text.color = color;
            }
            baseColor = color;
            cam = Camera.main;
            elapsed = 0f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (cam != null)
            {
                // カメラと同じ向きに回転 (テキスト面がカメラから見えるように)
                transform.rotation = cam.transform.rotation;
            }
            if (text != null)
            {
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, lifeSec));
                float a = 1f - t;
                text.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            }
            if (elapsed >= lifeSec) Destroy(gameObject);
        }
    }
}
