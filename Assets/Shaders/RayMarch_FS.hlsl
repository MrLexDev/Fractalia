// RayMarch_FS.hlsl

#ifndef RAYMARCH_FS_INCLUDED
#define RAYMARCH_FS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "SDFUtilities.hlsl"           // Asegúrate que la ruta sea correcta
#include "RayMarchingUtilities.hlsl"   // Asegúrate que la ruta sea correcta
#include "SDF_Fractals.hlsl"           // Asegúrate que la ruta sea correcta

// Definición de la estructura de entrada (debe coincidir con VertexOutput)
// Si InfiniteSphereField_VS.hlsl está en una ruta de include reconocida, 
// podrías incluirlo aquí en vez de redefinir, pero por claridad la ponemos.
struct vertex_output_from_vs
{
    float4 clip_pos : SV_POSITION;
    float2 uv      : TEXCOORD0;
};

CBUFFER_START(UnityPerMaterial)
    float4 cam_pos;
    float4 cam_forward;
    float4 cam_right;
    float4 cam_up;

    float  sphere_radius;
    float  cell_size;
    float4 base_color;
    float  max_ray_distance;
CBUFFER_END

float4 fragment_render_infinite_sphere_field(vertex_output_from_vs IN) : SV_Target
{
    // 4.1) Obtener UV en [0,1] y pasar a [-1,1]
    float2 uv01      = IN.uv;
    float2 screen_pos = uv01 * 2.0 - 1.0;

    // 4.2) Reconstruir rayo a partir de los parámetros de cámara
    float3 ray_origin = cam_pos.xyz;
    float3 ray_dir    = normalize(
                            screen_pos.x * cam_right.xyz +
                            screen_pos.y * cam_up.xyz    +
                            cam_forward.xyz );
    
    // 4.3) Ray-marching
    float hit_distance = perform_ray_marching(
                            ray_origin,
                            ray_dir,
                            sphere_radius,
                            cell_size,
                            max_ray_distance);
    
    // 4.4) Si no colisiona
    if (hit_distance > max_ray_distance)
    {
        return float4(0, 0, 0, 0); // Fondo negro
    }

    // 4.5) Hay impacto
    float3 hit_point  = ray_origin + ray_dir * hit_distance;
    float3 normal_hit = estimate_normal_from_sphere_sdf(hit_point, sphere_radius, cell_size);

    // 4.6) Sombreado lambertiano
    float3 light_dir    = normalize(float3(0.5, 0.7, -1.0));
    float  lambert_term = saturate(dot(normal_hit, light_dir));
    float3 shaded_color = base_color.rgb * lambert_term;

    return float4(shaded_color, 1.0);
}

#endif // RAYMARCH_FS_INCLUDED