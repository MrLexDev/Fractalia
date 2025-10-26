using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenTriPass : ScriptableRenderPass
{
    private readonly Material _material;
    private readonly int _shaderPassIndex;

    private RTHandle _cameraColor;
    private RTHandle _tmpColor; // RT temporal para evitar in-place

    public FullScreenTriPass(Material mat, int passIndex)
    {
        _material = mat;
        _shaderPassIndex = passIndex;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Pide la textura de color de cámara (URP hará el CopyColor si es necesario)
        ConfigureInput(ScriptableRenderPassInput.Color);

        _cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // Descriptor del target de cámara, sin profundidad para el temporal
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        // Crea/Reasigna el RT temporal
        RenderingUtils.ReAllocateIfNeeded(
            ref _tmpColor, desc,
            FilterMode.Bilinear, TextureWrapMode.Clamp,
            name: "_FullScreenTriTmp"
        );
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_material == null) return;

        // Muy importante: RTHandle puede no ser null, pero no tener textura real.
        if (_cameraColor == null || _cameraColor.rt == null) return;
        if (_tmpColor == null || _tmpColor.rt == null) return;

        var cmd = CommandBufferPool.Get("FullScreenTriPass");

        // src -> tmp (aplicando tu material)
        Blit(cmd, _cameraColor, _tmpColor, _material, _shaderPassIndex);
        // tmp -> src
        Blit(cmd, _tmpColor, _cameraColor);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // No liberar _cameraColor: lo gestiona el renderer.
        _tmpColor?.Release();
    }
}
