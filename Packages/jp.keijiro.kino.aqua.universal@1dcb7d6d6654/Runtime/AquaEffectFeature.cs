using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kino.Aqua.Universal
{

    sealed class AquaEffectPass : ScriptableRenderPass
    {
        /// <summary>
        /// Unity 6 / URP 17 uses Render Graph instead of the old Execute() API
        /// This replaces KinoAqua's original Execute() method
        /// </summary>
        /// <param name="renderGraph">Render Graph used to describe rendering operations, used to create te4xtures and schedule the Aqua blit</param>
        /// <param name="frameData">Contains data and resources for the current frame</param>
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Get the camera and the textures currently being used by URP
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Skip the effect when rendering directly to the screen
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            // Get the AquaEffect component from the current camera
            Camera camera = cameraData.camera;
            AquaEffect fx = camera.GetComponent<AquaEffect>();

            if (fx == null || !fx.enabled)
            {
                return;
            }

            // Get the current camera image
            TextureHandle source = resourceData.activeColorTexture;

            // Render Graph requires a separate texture for the result
            // We copy the source texture's settings so the new texture matches the camera image
            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "AquaEffectTexture";
            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            // Apply KinoAqua's material to the camera image
            // This replaces the old Blit() call from KinoAqua
            var blitParams = new RenderGraphUtils.BlitMaterialParameters(source, destination, fx.BlitMaterial, 0);

            renderGraph.AddBlitPass(blitParams, "AquaEffect");

            // Tell URP to use the processed texture as the camera's color texture from this point onward
            resourceData.cameraColor = destination;
        }

        //OLD CODE:
        //public override void Execute
        //  (ScriptableRenderContext context, ref RenderingData data)
        //{
        //    var fx = data.cameraData.camera.GetComponent<AquaEffect>();
        //    if (fx == null || !fx.enabled) return;

        //    var cmd = CommandBufferPool.Get("AquaEffect");
        //    Blit(cmd, ref data, fx.BlitMaterial, 0);
        //    context.ExecuteCommandBuffer(cmd);
        //    CommandBufferPool.Release(cmd);
        //}
    }

    public sealed class AquaEffectFeature : ScriptableRendererFeature
    {
        AquaEffectPass _pass;

        public override void Create()
          => _pass = new AquaEffectPass
          { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };

        public override void AddRenderPasses
          (ScriptableRenderer renderer, ref RenderingData data)
          => renderer.EnqueuePass(_pass);
    }
} // namespace Kino.Aqua.Universal
