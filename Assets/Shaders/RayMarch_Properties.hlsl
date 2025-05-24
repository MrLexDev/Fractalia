#include <HLSLSupport.cginc>

#ifndef RAYMARCH_PROPS_INCLUDED
#define RAYMARCH_PROPS_INCLUDED
CBUFFER_START(RaymarchProps)
        // Cámara y transformaciones
        float4  cam_pos;
        float4  cam_forward;
        float4  cam_right;
        float4  cam_up;

        // Parámetros de la “esfera”
        float4  base_color;
        float   max_ray_distance;
        float   surface_epsilon;
        int     max_steps;
        float   normal_delta;
        int     bailout;

        // mandlebox
        float MB_SCALE       = 2.0;     // Factor de escalado (típicamente entre 2.0 y 3.0)
        float MB_MIN_RADIUS  = 0.5;     // Radio mínimo para el sphere-fold
        float MB_FIXED_RADIUS= 1.0;     // Radio fijo para el sphere-fold
        float MB_BOX_LIMIT   = 1.0; 

        // Parámetros del fractal
        int    iterations;
        float  power;
    CBUFFER_END

#endif // RAYMARCH_PROPS_INCLUDED
