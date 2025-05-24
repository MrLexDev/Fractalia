#include <HLSLSupport.cginc>

#ifndef RAYMARCH_PROPS_INCLUDED
#define RAYMARCH_PROPS_INCLUDED
CBUFFER_START(RaymarchProps)
        // Cámara y transformaciones
        float4 cam_pos;
        float4 cam_forward;
        float4 cam_right;
        float4 cam_up;

        // Parámetros de la “esfera” (o lo que fuera antes)
        float  sphere_radius;
        float  cell_size;
        float4 base_color;
        float  max_ray_distance;

        // Parámetros del fractal
        int    iterations;
        float  power;
    CBUFFER_END

#endif // RAYMARCH_PROPS_INCLUDED
