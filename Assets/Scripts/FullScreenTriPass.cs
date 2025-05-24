using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

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
    public void Setup(RTHandle cameraColorHandle)
    {
        colorTargetHandle = cameraColorHandle;
    }
    
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

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }
}
