#ifndef RAYMARCH_FS_INCLUDED
#define RAYMARCH_FS_INCLUDED

#include "RayMarchingUtilities.hlsl"
#include "FullScreenTriangle_VS.hlsl"
#include "RayMarch_Properties.hlsl"
#include "SDF_Lighting.hlsl"


float4 fragment_render_fractal(vertex_output IN) : SV_Target
{
    float2 uv01 = IN.uv;
    float2 screen_pos = uv01 * 2.0 - 1.0;

    float3 ray_origin = cam_pos.xyz;
    float3 ray_dir    = normalize(
                            screen_pos.x * cam_right.xyz +
                            screen_pos.y * cam_up.xyz    +
                            cam_forward.xyz );
    
    fractal_output hit_distance = perform_ray_marching(ray_origin, ray_dir);
    
    if (hit_distance.ray_march_distance >= max_ray_distance)
    {
        return float4(0, 0, 0, 1);
    }

    float3 hit_point  = ray_origin + ray_dir * hit_distance.ray_march_distance;
    
    float3 normal_hit = estimate_normal_from_fractal_sdf(hit_point);

    // Pequeño offset para evitar self-shadowing/acne
    float surface_offset_epsilon = surface_epsilon * 2.0; // O un valor fijo pequeño
    float3 offset_hit_point = hit_point + normal_hit * surface_offset_epsilon;

    // Iluminación
    float3 light_dir    = normalize(light_direction * 1);   // Luz direccional
    float3 light_color  = float3(1.0, 0.9, 0.8);            // Color de la luz

    // Lambertiano
    float lambert_term = saturate(dot(normal_hit, light_dir));
    
    float ao_term = ambient_occlusion(hit_distance.ray_steps);
    
    float shadow_term = shadow(offset_hit_point, light_dir);

    // Color final
    float3 albedo = base_color.rgb;
    
    float3 diffuse_contrib = albedo * lambert_term;
    
    float3 ambient_light_color = float3(0.1, 0.1, 0.15); // Un azul oscuro o gris claro
    float3 final_color = albedo * ambient_light_color; // Base ambiental
    final_color += diffuse_contrib * light_color * shadow_term;
    final_color *= ao_term;
    
    // Niebla simple
    float fog_factor = exp(-hit_distance.ray_march_distance * 0.1);
    final_color = lerp(float3(0.3, 0.3, 0.3) /* color de la niebla */, final_color, fog_factor);

    return float4(final_color, 1.0);
}


#endif // RAYMARCH_FS_INCLUDED