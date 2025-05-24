// ------------------------------------------------------------
// RayMarchingUtilities.hlsl
// --------------------------
// Aquí va la lógica genérica de ray‐marching para una SDF de esfera repetida.
// ------------------------------------------------------------

// Declaración previa de la SDF para poder usarla aquí.
// Como estás haciendo includes, el compilador HLSL encontrará la definición en SdfUtilities.hlsl
float sphere_signed_distance(float3 position, float radius, float cellSize);
float fractal_signed_distance(float3 position, float overall_cell_size, int iterations, float fractal_scale, float3 fractal_offset);

// ------------------------------------------------------------
// PerformRayMarchingSphere:
//   - Recibe 'rayOrigin' (origen del rayo) y 'rayDir' (dirección normalizada).
//   - 'radius' y 'cellSize' son parámetros para la SDF (llama internamente a SphereSignedDistance).
//   - Devuelve la distancia recorrida hasta el “hit”, o un número muy grande si no hay colisión.
// ------------------------------------------------------------
float perform_ray_marching(float3 ray_origin, float3 ray_dir, float radius, float cell_size, float max_ray_distance)
{
    const int   MAX_STEPS        = 64;
    const float SURFACE_EPSILON  = 0.001;

    float distanceTraveled = 0.0;

    for (int i = 0; i < MAX_STEPS; i++)
    {
        float3 samplePoint  = ray_origin + ray_dir * distanceTraveled;
        float distToSurface = sphere_signed_distance(samplePoint, radius, cell_size);
        //distToSurface = length(samplePoint) - radius;

        if (distToSurface < SURFACE_EPSILON)
        {
            // Hemos llegado lo suficientemente cerca de "la esfera"
            return distanceTraveled;
        }

        if (distanceTraveled > max_ray_distance)
        {
            // Superamos la distancia máxima; damos por “no colisión”
            break;
        }

        distanceTraveled += distToSurface;
    }

    return 1e5; // Indicador de “sin colisión válida”
}