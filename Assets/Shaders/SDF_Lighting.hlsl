
#include "RayMarch_Properties.hlsl"
#include "SDF_Fractals.hlsl"

float smooth_sdf(float3 p)
{
    float center = fractal_signed_distance(p);

    float dx1 = fractal_signed_distance(p + float3(normal_delta, 0, 0));
    float dx2 = fractal_signed_distance(p - float3(normal_delta, 0, 0));

    float dy1 = fractal_signed_distance(p + float3(0, normal_delta, 0));
    float dy2 = fractal_signed_distance(p - float3(0, normal_delta, 0));

    float dz1 = fractal_signed_distance(p + float3(0, 0, normal_delta));
    float dz2 = fractal_signed_distance(p - float3(0, 0, normal_delta));

    float sum = center + dx1 + dx2 + dy1 + dy2 + dz1 + dz2;
    return sum / 7.0;
}

float3 estimate_normal_from_fractal_sdf(float3 p)
{
    float3 dx = float3(normal_delta, 0.0,          0.0);
    float3 dy = float3(0.0,          normal_delta, 0.0);
    float3 dz = float3(0.0,          0.0,          normal_delta);

    float nx = smooth_sdf(p + dx) - smooth_sdf(p - dx);
    float ny = smooth_sdf(p + dy) - smooth_sdf(p - dy);
    float nz = smooth_sdf(p + dz) - smooth_sdf(p - dz);

    return normalize(float3(nx, ny, nz));
}

float ambient_occlusion(float3 p, float3 normal)
{
    float ao = 0.0;
    float sca = 1.0;

    for (int i = 1; i <= 5; i++)
    {
        float dist = fractal_signed_distance(p + normal * (i * 0.1));
        ao += (i * 0.1 - dist) * sca;
        sca *= 0.5;
    }

    return saturate(1.0 - ao);
}


