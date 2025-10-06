using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if RENDER_GRAPH_ENABLED
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

public class FullScreenTriPass : ScriptableRenderPass
{
    private Material material;
    private int shaderPassIndex;
    private RTHandle colorTargetHandle;

    public FullScreenTriPass(Material mat, int passIndex)
    {
        material = mat;
        shaderPassIndex = passIndex;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // This method is unused, dont know why is here
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
#if !RENDER_GRAPH_ENABLED
        ConfigureInput(ScriptableRenderPassInput.Color);
        colorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
#endif
    }
    
#if RENDER_GRAPH_ENABLED
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();
        
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Configurar descriptor con dimensiones de la cámara
        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        // Crear textura en RenderGraph
        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "FullScreenTriPassTarget", false);

        TextureHandle src = resourceData.activeColorTexture;
        if (!src.IsValid() || !dst.IsValid())
            return;

        // Registrar pase de blit completo
        RenderGraphUtils.BlitMaterialParameters blitParams = new RenderGraphUtils.BlitMaterialParameters(dst, src, material, shaderPassIndex);
        renderGraph.AddBlitPass(blitParams, "FullScreenTriPassBlit");
    }
#endif

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
#if !RENDER_GRAPH_ENABLED
        if (material == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get("FullScreenTriPass");
        var src = colorTargetHandle;
        if (src == null)
        {
            CommandBufferPool.Release(cmd);
            return;
        }
        Blit(cmd, src, src, material, shaderPassIndex);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
#endif
    }
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }
}
