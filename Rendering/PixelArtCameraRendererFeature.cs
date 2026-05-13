using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SteraCube.Rendering
{
    public sealed class PixelArtCameraRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] Shader shader;
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        Material material;
        PixelArtCameraPass pass;

        public override void Create()
        {
            if (shader == null)
            {
                shader = Shader.Find("Hidden/Cube33/PixelArtCameraEffect");
            }

            CoreUtils.Destroy(material);
            material = shader != null ? CoreUtils.CreateEngineMaterial(shader) : null;

            pass = new PixelArtCameraPass
            {
                renderPassEvent = renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null || pass == null || material.shader == null || !material.shader.isSupported)
            {
                return;
            }

            var effect = GetEffect(renderingData.cameraData.camera);
            if (effect == null)
            {
                return;
            }

            pass.Setup(material, effect);
            renderer.EnqueuePass(pass);
        }

        static PixelArtCameraEffect GetEffect(Camera camera)
        {
            if (camera == null)
            {
                return null;
            }

            var effect = camera.GetComponent<PixelArtCameraEffect>();
            return effect != null && effect.EffectEnabled ? effect : null;
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
        }

        sealed class PixelArtCameraPass : ScriptableRenderPass
        {
            static readonly int PixelHeightId = Shader.PropertyToID("_PixelHeight");
            static readonly int ColorStepsId = Shader.PropertyToID("_ColorSteps");
            static readonly int DitherStrengthId = Shader.PropertyToID("_DitherStrength");
            static readonly int PosterizeId = Shader.PropertyToID("_Posterize");
            static readonly int PixelFrameId = Shader.PropertyToID("_PixelFrame");
            static readonly int PixelJitterId = Shader.PropertyToID("_PixelJitter");
            static readonly int AnimatedDitherId = Shader.PropertyToID("_AnimatedDither");
            static readonly int StyleModeId = Shader.PropertyToID("_StyleMode");
            static readonly int WatercolorStrengthId = Shader.PropertyToID("_WatercolorStrength");
            static readonly int PaperGrainId = Shader.PropertyToID("_PaperGrain");
            static readonly int WireframeStrengthId = Shader.PropertyToID("_WireframeStrength");
            static readonly int WireframeThresholdId = Shader.PropertyToID("_WireframeThreshold");
            static readonly int WireframeColorId = Shader.PropertyToID("_WireframeColor");

            Material material;
            PixelArtCameraEffect effect;

            public void Setup(Material material, PixelArtCameraEffect effect)
            {
                this.material = material;
                this.effect = effect;
                requiresIntermediateTexture = true;
                ConfigureInput(ScriptableRenderPassInput.Color);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null || effect == null)
                {
                    return;
                }

                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                {
                    return;
                }

                ApplyMaterialSettings();

                var source = resourceData.activeColorTexture;
                var destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = "_Cube33PixelArtCameraColor";
                destinationDesc.clearBuffer = false;

                var destination = renderGraph.CreateTexture(destinationDesc);
                var parameters = new RenderGraphUtils.BlitMaterialParameters(source, destination, material, 0);
                renderGraph.AddBlitPass(parameters, "Pixel Art Camera Effect");

                resourceData.cameraColor = destination;
            }

            void ApplyMaterialSettings()
            {
                material.SetFloat(PixelHeightId, effect.PixelHeight);
                material.SetFloat(ColorStepsId, effect.ColorSteps);
                material.SetFloat(DitherStrengthId, effect.DitherStrength);
                material.SetFloat(PosterizeId, effect.Posterize ? 1f : 0f);
                material.SetFloat(PixelFrameId, GetPixelFrame(effect.AnimationFps));
                material.SetFloat(PixelJitterId, effect.PixelJitter);
                material.SetFloat(AnimatedDitherId, effect.AnimatedDither);
                material.SetFloat(StyleModeId, (float)effect.Mode);
                material.SetFloat(WatercolorStrengthId, effect.WatercolorStrength);
                material.SetFloat(PaperGrainId, effect.PaperGrain);
                material.SetFloat(WireframeStrengthId, effect.WireframeStrength);
                material.SetFloat(WireframeThresholdId, effect.WireframeThreshold);
                material.SetColor(WireframeColorId, effect.WireframeColor);
            }

            static float GetPixelFrame(int animationFps)
            {
                if (animationFps <= 0)
                {
                    return 0f;
                }

                return Mathf.Floor(Time.unscaledTime * animationFps);
            }
        }
    }
}
