#ifndef RAYMARCH_FS_INCLUDED
#define RAYMARCH_FS_INCLUDED

#include "RayMarchingUtilities.hlsl"
#include "FullScreenTriangle_VS.hlsl"
#include "RayMarch_Properties.hlsl"
#include "SDF_Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


float4 fragment_render_fractal(vertex_output IN) : SV_Target
{
    float2 uv01 = IN.uv;
    float2 screen_pos = uv01 * 2.0 - 1.0;

        float  aspect = _ScreenParams.x / _ScreenParams.y;
        float  fov_scale = tan(radians(cam_fov) * 0.5);
        screen_pos.x *= aspect;
        screen_pos *= fov_scale;

    float3 ray_origin = cam_pos.xyz;
    float3 ray_dir    = normalize(
                            screen_pos.x * cam_right.xyz +
                            screen_pos.y * cam_up.xyz    +
                            cam_forward.xyz );
    
    fractal_output hit = perform_ray_marching(ray_origin, ray_dir);
    
    if (debug_steps != 0)
    {
        return float4(hit.ray_steps / (float)max_steps, 0, 0, 1);
    }
    if (hit.ray_march_distance >= max_ray_distance)
    {
        return float4(0, 0, 0, 0);
    }
    
    float4 final_color = light_effects(ray_origin, ray_dir, hit);
    
    return final_color;
}
#endif // RAYMARCH_FS_INCLUDED