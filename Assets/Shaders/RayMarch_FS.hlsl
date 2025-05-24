#ifndef RAYMARCH_FS_INCLUDED
#define RAYMARCH_FS_INCLUDED

#include "RayMarchingUtilities.hlsl"
#include "SDF_Fractals.hlsl"
#include "FullScreenTriangle_VS.hlsl"  // Para tener el vertex_output struct
#include "RayMarch_Properties.hlsl"

float4 fragment_render_infinite_sphere_field(vertex_output IN) : SV_Target
{
    // 4.1) Obtener UV en [0,1] y pasar a [-1,1]
    float2 uv01       = IN.uv;
    float2 screen_pos = uv01 * 2.0 - 1.0;

    // 4.2) Reconstruir rayo a partir de los parámetros de cámara
    float3 ray_origin = cam_pos.xyz;
    float3 ray_dir    = normalize(
                            screen_pos.x * cam_right.xyz +
                            screen_pos.y * cam_up.xyz    +
                            cam_forward.xyz );
    
    // 4.3) Ray-marching
    float hit_distance = perform_ray_marching(ray_origin, ray_dir);
    
    // 4.4) Si no colisiona
    if (hit_distance > max_ray_distance)
    {
        return float4(0, 0, 0, 1); // Fondo negro
    }

    // 4.5) Hay impacto
    float3 hit_point  = ray_origin + ray_dir * hit_distance;
    float3 normal_hit = estimate_normal_from_fractal_sdf(hit_point);

    // 4.6) Sombreado lambertiano
    float3 light_dir    = normalize(float3(0.5, 0.7, -1.0));
    float  lambert_term = saturate(dot(normal_hit, light_dir));
    float3 shaded_color = base_color.rgb * lambert_term * ambient_occlusion(hit_point, normal_hit);

    return float4(shaded_color, 1.0);
}

#endif // RAYMARCH_FS_INCLUDED