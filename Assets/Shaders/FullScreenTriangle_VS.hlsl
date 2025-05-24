// FullScreenTriangle_VS.hlsl

#ifndef FULL_SCREEN_TRIANGLE_VS_INCLUDED
#define FULL_SCREEN_TRIANGLE_VS_INCLUDED

// Estructura de salida del Vertex Shader
struct vertex_output
{
    float4 clip_pos : SV_POSITION;
    float2 uv       : TEXCOORD0;
};

// Vertex Shader para un triángulo a pantalla completa
vertex_output vertex_full_screen_triangle(uint vertex_id : SV_VertexID)
{
    vertex_output output;

    // Coordenadas “fuera” de [-1,1] para cubrir toda la pantalla
    float2 full_screen_pos = float2(
        vertex_id == 1 ? 3.0 : -1.0,
        vertex_id == 2 ? 3.0 : -1.0
    );
    output.clip_pos = float4(full_screen_pos, 0.0, 1.0);

    // UV en [0,2], luego el fragment lo escalará a [0,1]
    output.uv = float2(
        vertex_id == 1 ? 2.0 : 0.0,
        vertex_id == 2 ? 2.0 : 0.0
    );
    return output;
}

#endif // FULL_SCREEN_TRIANGLE_VS_INCLUDED