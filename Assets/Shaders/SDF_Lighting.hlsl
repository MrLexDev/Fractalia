
#include "SDF_Fractals.hlsl"

float smooth_sdf(float3 p, float delta)
{
    fractal_output center = fractal_signed_distance(p);

    fractal_output dx1 = fractal_signed_distance(p + float3(delta, 0, 0));
    fractal_output dx2 = fractal_signed_distance(p - float3(delta, 0, 0));

    fractal_output dy1 = fractal_signed_distance(p + float3(0, delta, 0));
    fractal_output dy2 = fractal_signed_distance(p - float3(0, delta, 0));

    fractal_output dz1 = fractal_signed_distance(p + float3(0, 0, delta));
    fractal_output dz2 = fractal_signed_distance(p - float3(0, 0, delta));

    float sum = center.sdf_distance + dx1.sdf_distance + dx2.sdf_distance + dy1.sdf_distance + dy2.sdf_distance + dz1.sdf_distance + dz2.sdf_distance;
    return sum / 7.0;
}

float3 estimate_normal_from_fractal_sdf(float3 p, float delta)
{
    float3 dx = float3(delta, 0.0,          0.0);
    float3 dy = float3(0.0,          delta, 0.0);
    float3 dz = float3(0.0,          0.0,          delta);

    float nx = smooth_sdf(p + dx, delta) - smooth_sdf(p - dx, delta);
    float ny = smooth_sdf(p + dy, delta) - smooth_sdf(p - dy, delta);
    float nz = smooth_sdf(p + dz, delta) - smooth_sdf(p - dz, delta);

    return normalize(float3(nx, ny, nz));
}

float ambient_occlusion(int march_steps)
{
    return ao_brightness / ((float)march_steps / (float)max_steps);
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

float3 compute_hit_point(float3 ray_origin, float3 ray_dir, float distance)
{
    return ray_origin + ray_dir * distance;
}

float3 offset_surface_point(float3 hit_point, float3 normal_hit, float epsilon)
{
    float surface_offset_epsilon = epsilon * 2.0;
    return hit_point + normal_hit * surface_offset_epsilon;
}

float lambert_lighting(float3 normal_hit, float3 light_dir)
{
    return saturate(dot(normal_hit, light_dir));
}

float3 ambient_light(float3 albedo)
{
    float3 ambient_light_color = float3(0.1, 0.1, 0.15);
    return albedo * ambient_light_color;
}

float3 lighting_color(float3 albedo, float lambert_term, float ao_term, float shadow_term, float3 light_color)
{
    float3 diffuse_contrib = albedo * lambert_term;
    float3 final_color = ambient_light(albedo);
    final_color += diffuse_contrib * light_color * shadow_term;
    final_color *= ao_term;
    return final_color;
}

float3 apply_fog(float3 color, float distance)
{
    float fog_factor = exp(-distance * 0.1);
    return lerp(float3(0.3, 0.3, 0.3), color, fog_factor);
}


float4 light_effects(float3 ray_origin, float3 ray_dir, fractal_output hit)
{
    float3 hit_point  = compute_hit_point(ray_origin, ray_dir, hit.ray_march_distance);
    float delta = lod_normal_delta(hit.ray_march_distance);
    float epsilon = lod_surface_epsilon(hit.ray_march_distance);
    float3 normal_hit = estimate_normal_from_fractal_sdf(hit_point, delta);
    float3 offset_hit_point = offset_surface_point(hit_point, normal_hit, epsilon);

    float3 light_dir   = normalize(light_direction);
    float3 light_color = float3(1.0, 0.9, 0.8);

    float lambert_term = lambert_lighting(normal_hit, light_dir);
    float ao_term      = ambient_occlusion(hit.ray_steps);
    float shadow_term  = shadow(offset_hit_point, light_dir);

    float orbit_value = hit.orbit_min_dist * orbit_scale;
    float3 orbit_color;

    if (orbit_value < orbit_thresholds.x)
    {
        float t = saturate(orbit_value / max(orbit_thresholds.x, 1e-5));
        orbit_color = lerp(orbit_color_0, orbit_color_1, t);
    }
    else if (orbit_value < orbit_thresholds.y)
    {
        float t = saturate((orbit_value - orbit_thresholds.x) / max(orbit_thresholds.y - orbit_thresholds.x, 1e-5));
        orbit_color = lerp(orbit_color_1, orbit_color_2, t);
    }
    else if (orbit_value < orbit_thresholds.z)
    {
        float t = saturate((orbit_value - orbit_thresholds.y) / max(orbit_thresholds.z - orbit_thresholds.y, 1e-5));
        orbit_color = lerp(orbit_color_2, orbit_color_3, t);
    }
    else
    {
        orbit_color = orbit_color_3;
    }

    float3 albedo = orbit_color;

    float3 final_color = lighting_color(albedo, lambert_term, ao_term, shadow_term, light_color);
    //final_color = apply_fog(final_color, hit.ray_march_distance);
    return float4(final_color, 1.0);
}