using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kino.Aqua.Universal
{

    sealed class AquaEffectPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            Camera camera = cameraData.camera;
            AquaEffect fx = camera.GetComponent<AquaEffect>();

            if (fx == null || !fx.enabled)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);

            destinationDesc.name = "AquaEffectTexture";
            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            var blitParams = new RenderGraphUtils.BlitMaterialParameters(
                source,
                destination,
                fx.BlitMaterial,
                0
            );

            renderGraph.AddBlitPass(
                blitParams,
                "AquaEffect"
            );

            resourceData.cameraColor = destination;
        }

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
