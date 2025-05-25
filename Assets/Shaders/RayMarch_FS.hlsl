#ifndef RAYMARCH_FS_INCLUDED
#define RAYMARCH_FS_INCLUDED

#include "RayMarchingUtilities.hlsl"
#include "FullScreenTriangle_VS.hlsl"  // Para tener el vertex_output struct
#include "RayMarch_Properties.hlsl"
#include "SDF_Lighting.hlsl"

// Define estas constantes en algún lugar accesible, quizás al inicio de tu shader o en un archivo .hlslinc
#define SHADOW_MAX_STEPS 64       // Número máximo de pasos para el rayo de sombra
#define SHADOW_MAX_DISTANCE 100.0 // Distancia máxima que recorrerá el rayo de sombra (ajusta según la escala de tu fractal)
#define MIN_SHADOW_HIT_DISTANCE 1e-3 // Umbral para considerar un impacto duro (opcional, para sombras más nítidas si se desea)
#define SHADOW_RAY_INITIAL_OFFSET 1e-2 // Pequeño offset inicial para el rayo de sombra


float soft_shadow(float3 ro, float3 rd, float k)
{
    float penumbra = 1.0;
    float t = SHADOW_RAY_INITIAL_OFFSET;

    for (int i = 0; i < SHADOW_MAX_STEPS; i++)
    {
        if (t >= SHADOW_MAX_DISTANCE) break;

        float3 current_pos_on_shadow_ray = ro + rd * t;
        
        float h = fractal_signed_distance(current_pos_on_shadow_ray);

        if (h < MIN_SHADOW_HIT_DISTANCE) {
            return 0.0; 
        }
        penumbra = min(penumbra, k * h / t);
        
        //t += max(h, SHADOW_RAY_INITIAL_OFFSET * 0.5f); 
        // Alternativa más segura pero potentially más lenta si h es consistentemente pequeño:
        t += h;
        if (h < MIN_SHADOW_HIT_DISTANCE * 0.1) t += MIN_SHADOW_HIT_DISTANCE * 0.1; // Ensure some progress
    }

    return saturate(penumbra);
}


float rand(float2 co, float seed) {
    return frac(sin(dot(co.xy, float2(12.9898, 78.233)) + seed) * 43758.5453);
}

float2 rand2(float2 co, float seed) {
    float r1 = rand(co, seed);
    float r2 = rand(co + float2(0.17, 0.83), seed + 10.0);
    return float2(r1, r2);
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
    
    float hit_distance = perform_ray_marching(ray_origin, ray_dir);
    
    if (hit_distance >= max_ray_distance)
    {
        return float4(0, 0, 0, 1);
    }

    float3 hit_point  = ray_origin + ray_dir * hit_distance;
    
    float3 normal_hit = estimate_normal_from_fractal_sdf(hit_point);

    // Pequeño offset para evitar self-shadowing/acne
    float surface_offset_epsilon = surface_epsilon * 2.0; // O un valor fijo pequeño
    float3 offset_hit_point = hit_point + normal_hit * surface_offset_epsilon;

    // Iluminación
    float3 light_dir    = normalize(float3(0.5, 0.7, -1.0)); // Luz direccional
    float3 light_color  = float3(1.0, 0.9, 0.8); // Color de la luz
    float shininess = 128.0; // Brillo especular

    // Lambertiano
    float lambert_term = saturate(dot(normal_hit, light_dir));
    
    // Especular (Blinn-Phong)
    float3 view_dir = normalize(ray_origin - hit_point); // Desde el punto de impacto hacia la cámara
    float3 halfway_dir = normalize(light_dir + view_dir);
    float spec_angle = saturate(dot(normal_hit, halfway_dir));
    float specular_term = pow(spec_angle, shininess);

    float ao_term = ambient_occlusion(offset_hit_point, normal_hit);
    
    float shadow_term = soft_shadow(offset_hit_point, light_dir, 16.0 /* suavidad */);
    //float shadow_term = 1.0; // Sin sombras por ahora para simplificar

    // Color final
    float3 albedo = base_color.rgb;
    
    float3 diffuse_contrib = albedo * lambert_term;
    float3 specular_contrib = light_color * specular_term; // El color de la luz afecta al especular
    
    float3 ambient_light_color = float3(0.1, 0.1, 0.15); // Un azul oscuro o gris claro
    float3 final_color = albedo * ambient_light_color; // Base ambiental
    final_color += (diffuse_contrib + specular_contrib) * light_color * shadow_term * ao_term; // Luz directa + especular
    
    // Niebla simple
    float fog_factor = exp(-hit_distance * 0.1);
    final_color = lerp(float3(0.3, 0.3, 0.3) /* color de la niebla */, final_color, fog_factor);

    return float4(final_color, 1.0);
}


#endif // RAYMARCH_FS_INCLUDED