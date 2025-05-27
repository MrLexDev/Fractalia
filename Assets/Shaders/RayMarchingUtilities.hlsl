// --------------------------
// RayMarchingUtilities.hlsl
// --------------------------

#include "RayMarch_Properties.hlsl"
#include "SDF_Fractals.hlsl"

// ------------------------------------------------------------
// PerformRayMarchingSphere:
//   - Recibe 'rayOrigin' (origen del rayo) y 'rayDir' (dirección normalizada).
//   - 'radius' y 'cellSize' son parámetros para la SDF (llama internamente a SphereSignedDistance).
//   - Devuelve la distancia recorrida hasta el “hit”, o un número muy grande si no hay colisión.
// ------------------------------------------------------------
fractal_output perform_ray_marching(float3 ray_origin, float3 ray_dir)
{
    fractal_output result = {0, 1e5, 1e5};
    float distance_traveled = 0.0;
    int ray_steps = 0;

    for (int i = 0; i < max_steps; i++)
    {
        ray_steps++;
        float3 sample_point  = ray_origin + ray_dir * distance_traveled;
        fractal_output distance_to_surface = fractal_signed_distance(sample_point);

        if (distance_to_surface.sdf_distance < surface_epsilon)
        {
            result.ray_steps = ray_steps;
            result.ray_march_distance = distance_traveled;
            return result;
        }

        if (distance_traveled > max_ray_distance)
        {
            break;
        }

        distance_traveled += distance_to_surface.sdf_distance;
    }

    return result; // Indicador de “sin colisión válida”
}