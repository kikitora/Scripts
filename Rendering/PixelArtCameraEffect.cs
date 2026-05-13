using UnityEngine;

namespace SteraCube.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class PixelArtCameraEffect : MonoBehaviour
    {
        public enum StyleMode
        {
            PixelArt = 0,
            Watercolor = 1,
            Wireframe = 2,
            PixelWatercolor = 3,
            PixelWireframe = 4
        }

        [SerializeField] bool effectEnabled = true;
        [SerializeField] StyleMode styleMode = StyleMode.PixelArt;
        [SerializeField, Range(64, 720)] int pixelHeight = 180;
        [SerializeField, Range(2, 64)] int colorSteps = 24;
        [SerializeField, Range(0f, 1f)] float ditherStrength = 0.12f;
        [SerializeField] bool posterize = true;
        [SerializeField, Range(0, 30)] int animationFps = 12;
        [SerializeField, Range(0f, 1f)] float pixelJitter = 0.2f;
        [SerializeField, Range(0f, 1f)] float animatedDither = 0.35f;
        [SerializeField, Range(0f, 1f)] float watercolorStrength = 0.65f;
        [SerializeField, Range(0f, 1f)] float paperGrain = 0.35f;
        [SerializeField, Range(0f, 1f)] float wireframeStrength = 0.85f;
        [SerializeField, Range(0.05f, 1f)] float wireframeThreshold = 0.2f;
        [SerializeField] Color wireframeColor = new(0.04f, 0.08f, 0.09f, 1f);

        public bool EffectEnabled => effectEnabled && isActiveAndEnabled;
        public StyleMode Mode => styleMode;
        public int PixelHeight => Mathf.Max(1, pixelHeight);
        public int ColorSteps => Mathf.Max(2, colorSteps);
        public float DitherStrength => Mathf.Clamp01(ditherStrength);
        public bool Posterize => posterize;
        public int AnimationFps => Mathf.Max(0, animationFps);
        public float PixelJitter => Mathf.Clamp01(pixelJitter);
        public float AnimatedDither => Mathf.Clamp01(animatedDither);
        public float WatercolorStrength => Mathf.Clamp01(watercolorStrength);
        public float PaperGrain => Mathf.Clamp01(paperGrain);
        public float WireframeStrength => Mathf.Clamp01(wireframeStrength);
        public float WireframeThreshold => Mathf.Clamp(wireframeThreshold, 0.05f, 1f);
        public Color WireframeColor => wireframeColor;
    }
}
