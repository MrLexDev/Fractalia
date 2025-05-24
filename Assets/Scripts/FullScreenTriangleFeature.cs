using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenTriangleFeature : ScriptableRendererFeature
{
    [SerializeField] private Material material;
    private FullScreenTriPass _triPass;

    // Se llama al crear o recompilar la feature
    public override void Create()
    {
        // Índice 0 porque asumimos que el primer Pass del shader es el que queremos
        _triPass = new FullScreenTriPass(material, 0);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_triPass);
    }
}