#include <HLSLSupport.cginc>

#ifndef RAYMARCH_PROPS_INCLUDED
#define RAYMARCH_PROPS_INCLUDED
CBUFFER_START(RaymarchProps)
        // Cámara y transformaciones
        float4  cam_pos;
        float4  cam_forward;
        float4  cam_right;
        float4  cam_up;
        float   cam_fov;

        // Parámetros de la “esfera”
        float4  base_color;
        float   max_ray_distance;
        float   surface_epsilon;
        int     max_steps;
        float   normal_delta;
        float   surface_epsilon_far;
        float   normal_delta_far;
        float   lod_near_distance;
        float   lod_far_distance;
        int     bailout;
        float   ao_brightness;

        // mandlebox
        float MB_SCALE       = 2.0;     // Factor de escalado (típicamente entre 2.0 y 3.0)
        float MB_MIN_RADIUS  = 0.5;     // Radio mínimo para el sphere-fold
        float MB_FIXED_RADIUS= 1.0;     // Radio fijo para el sphere-fold
        float MB_BOX_LIMIT   = 1.0;     // Límite de box-fold en cada eje (±1.0)

        // Parámetros del fractal
        int    iterations;
        float  power;
        float  g_Scale;
        float3 fractal_offset;

        // Light
        float3 light_direction;

        // Orbit trap coloring
        float3 orbit_color_0;
        float3 orbit_color_1;
        float3 orbit_color_2;
        float3 orbit_color_3;
        float3 orbit_thresholds; // x,y,z thresholds
        float  orbit_scale;
        int    debug_steps;
    CBUFFER_END

    float compute_lod_factor(float distance)
    {
        float denom = max(lod_far_distance - lod_near_distance, 1e-5);
        return saturate((distance - lod_near_distance) / denom);
    }

    float lod_surface_epsilon(float distance)
    {
        return lerp(surface_epsilon, surface_epsilon_far, compute_lod_factor(distance));
    }

    float lod_normal_delta(float distance)
    {
        return lerp(normal_delta, normal_delta_far, compute_lod_factor(distance));
    }

#endif // RAYMARCH_PROPS_INCLUDED
