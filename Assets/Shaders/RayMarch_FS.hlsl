#ifndef RAYMARCH_FS_INCLUDED
#define RAYMARCH_FS_INCLUDED

#include "RayMarchingUtilities.hlsl"
#include "FullScreenTriangle_VS.hlsl"
#include "RayMarch_Properties.hlsl"
#include "SDF_Lighting.hlsl"

// Simple hash based random function for procedural backgrounds
float random2d(float2 st)
{
    return frac(sin(dot(st, float2(12.9898,78.233))) * 43758.5453);
}

float2 rotate2d(float2 p, float a)
{
    float s = sin(a);
    float c = cos(a);
    p -= 0.5;
    float2 r = float2(c * p.x - s * p.y, s * p.x + c * p.y);
    return r + 0.5;
}


float4 fragment_render_fractal(vertex_output IN) : SV_Target
{
    float2 uv01 = IN.uv;
    float2 screen_pos = uv01 * 2.0 - 1.0;

    float3 ray_origin = cam_pos.xyz;
    float3 ray_dir    = normalize(
                            screen_pos.x * cam_right.xyz +
                            screen_pos.y * cam_up.xyz    +
                            cam_forward.xyz );
    
    fractal_output hit = perform_ray_marching(ray_origin, ray_dir);
    
    if (hit.ray_march_distance >= max_ray_distance)
    {
        // Starry background with a moon and clustered stars when no fractal is hit

        // Base vertical gradient
        float3 base_color = lerp(float3(0.01, 0.02, 0.04), float3(0.0, 0.0, 0.1), uv01.y);

        // Milky-way like band
        float2 band_uv = rotate2d(uv01, radians(20.0));
        float  band_mask = pow(saturate(0.5 - abs(band_uv.y - 0.5)), 6.0);

        // Star clustering using a coarse noise pattern
        float  cluster_seed = random2d(floor(uv01 * 10.0));
        float  cluster = step(0.7, cluster_seed);
        float  star_density = lerp(120.0, 300.0, cluster);

        float2 star_cell = floor(uv01 * star_density);
        float  star_seed = random2d(star_cell);
        float  star = step(0.995, star_seed);
        float  star_brightness = random2d(star_cell + 1.0) * (1.0 + 2.0 * band_mask);
        float3 star_color = star * star_brightness * lerp(float3(1.0,1.0,1.0), float3(1.0,0.9,0.8), band_mask);

        // Moon rendered as a soft disc
        float  d = length(uv01 - moon_pos);
        float  moon_mask = smoothstep(moon_radius, moon_radius * 0.5, d);
        float3 moon_col = (1.0 - moon_mask) * moon_color;

        return float4(base_color + star_color + moon_col, 1.0);
    }
    
    float4 final_color = light_effects(ray_origin, ray_dir, hit);
    
    return final_color;
}
#endif // RAYMARCH_FS_INCLUDED