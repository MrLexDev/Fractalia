
#include "RayMarch_Properties.hlsl"
#include "SDF_Fractals.hlsl"

float smooth_sdf(float3 p)
{
    fractal_output center = fractal_signed_distance(p);

    fractal_output dx1 = fractal_signed_distance(p + float3(normal_delta, 0, 0));
    fractal_output dx2 = fractal_signed_distance(p - float3(normal_delta, 0, 0));

    fractal_output dy1 = fractal_signed_distance(p + float3(0, normal_delta, 0));
    fractal_output dy2 = fractal_signed_distance(p - float3(0, normal_delta, 0));

    fractal_output dz1 = fractal_signed_distance(p + float3(0, 0, normal_delta));
    fractal_output dz2 = fractal_signed_distance(p - float3(0, 0, normal_delta));

    float sum = center.sdf_distance + dx1.sdf_distance + dx2.sdf_distance + dy1.sdf_distance + dy2.sdf_distance + dz1.sdf_distance + dz2.sdf_distance;
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

float ambient_occlusion(int march_steps)
{
        return 1- ((float)march_steps / (float)max_steps);
}


