// --------------------------
// RayMarchingUtilities.hlsl
// --------------------------

#include "RayMarch_Properties.hlsl"

float fractal_signed_distance(float3 position);

// ------------------------------------------------------------
// PerformRayMarchingSphere:
//   - Recibe 'rayOrigin' (origen del rayo) y 'rayDir' (dirección normalizada).
//   - 'radius' y 'cellSize' son parámetros para la SDF (llama internamente a SphereSignedDistance).
//   - Devuelve la distancia recorrida hasta el “hit”, o un número muy grande si no hay colisión.
// ------------------------------------------------------------
float perform_ray_marching(float3 ray_origin, float3 ray_dir)
{
    const int   MAX_STEPS        = 64;
    const float SURFACE_EPSILON  = 0.001;

    float distance_traveled = 0.0;

    for (int i = 0; i < MAX_STEPS; i++)
    {
        float3 sample_point  = ray_origin + ray_dir * distance_traveled;
        float distance_to_surface = fractal_signed_distance(sample_point);

        if (distance_to_surface < SURFACE_EPSILON)
        {
            // Hemos llegado lo suficientemente cerca de "la esfera"
            return distance_traveled;
        }

        if (distance_traveled > max_ray_distance)
        {
            // Superamos la distancia máxima; damos por “no colisión”
            break;
        }

        distance_traveled += distance_to_surface;
    }

    return 1e5; // Indicador de “sin colisión válida”
}