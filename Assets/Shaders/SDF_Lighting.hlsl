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
        return 0.1 / ((float)march_steps / (float)max_steps);
}

#define SHADOW_MAX_STEPS 64       // Número máximo de pasos para el rayo de sombra
#define SHADOW_MAX_DISTANCE 100.0 // Distancia máxima que recorrerá el rayo de sombra (ajusta según la escala de tu fractal)
#define MIN_SHADOW_HIT_DISTANCE 1e-3 // Umbral para considerar un impacto duro (opcional, para sombras más nítidas si se desea)
#define SHADOW_RAY_INITIAL_OFFSET 1e-2 // Pequeño offset inicial para el rayo de sombra

float shadow(float3 ro, float3 rd)
{
    float t = SHADOW_RAY_INITIAL_OFFSET;

    for (int i = 0; i < SHADOW_MAX_STEPS; i++)
    {
        if (t >= SHADOW_MAX_DISTANCE) break;

        float3 current_pos_on_shadow_ray = ro + rd * t;
        fractal_output h = fractal_signed_distance(current_pos_on_shadow_ray);

        if (h.sdf_distance < MIN_SHADOW_HIT_DISTANCE) {
            return 0.0; 
        }

        t += h.sdf_distance;
    }

    return 1.0;
}


