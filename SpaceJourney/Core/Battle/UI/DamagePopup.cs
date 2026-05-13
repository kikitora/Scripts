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
        public float riseSpeed = 0.85f;
        public float lifeSec = 0.85f;

        private float elapsed;
        private Color baseColor = Color.white;
        private Camera cam;

        public void Setup(int value, Color color)
        {
            Setup(value.ToString(), color);
        }

        public void Setup(string value, Color color)
        {
            if (text == null) text = GetComponentInChildren<TextMeshPro>();
            if (text != null)
            {
                text.text = value;
                text.color = color;
            }
            baseColor = color;
            cam = Camera.main;
            elapsed = 0f;
        }

        public void ApplyMotion(float newRiseSpeed, float newLifeSec)
        {
            riseSpeed = Mathf.Max(0f, newRiseSpeed);
            lifeSec = Mathf.Max(0.05f, newLifeSec);
        }

        public void ApplyFont(TMP_FontAsset font)
        {
            if (font == null) return;
            if (text == null) text = GetComponentInChildren<TextMeshPro>();
            if (text != null) text.font = font;
        }

        public void SetRenderOnTop(bool enabled)
        {
            if (text == null) text = GetComponentInChildren<TextMeshPro>();
            if (text == null) return;

            var renderer = text.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = enabled ? 500 : 0;
            }

            if (!enabled) return;

            var mat = text.fontMaterial;
            if (mat == null) return;

            if (mat.HasProperty("_ZTest"))
                mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
            mat.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);
            if (mat.HasProperty("_ZWrite"))
                mat.SetFloat("_ZWrite", 0f);

            mat.renderQueue = 4000;
            text.fontMaterial = mat;
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
