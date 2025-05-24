using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenTriangleFeature : ScriptableRendererFeature
{
    [SerializeField] private Material material; // Arrastra aquí el material con tu shader
    private FullScreenTriPass _triPass;

    // Se llama al crear o recompilar la feature
    public override void Create()
    {
        // Índice 0 porque asumimos que el primer Pass del shader es el que queremos
        _triPass = new FullScreenTriPass(material, 0);
    }

    // Encolar el pase; aquí NO usamos renderer.cameraColorTarget ni nada obsoleto
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Informamos a URP que vamos a usar este pase (aun cuando todavía no sabemos el RTHandle)
        renderer.EnqueuePass(_triPass);
    }
}