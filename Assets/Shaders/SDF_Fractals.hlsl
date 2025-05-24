// ------------------------------------------------------------
// --- Funciones para el Fractal Iterativo ---
// ------------------------------------------------------------


float fractal_signed_distance(float3 position, float overall_cell_size, int iterations, float fractal_scale, float3 fractal_offset)
{
    float3 z = position;
    float scale_accumulator = 1.0;

    for (int i = 0; i < iterations; ++i)
    {
        z = abs(z);

        z = z - fractal_offset;
        z = z * fractal_scale;  
        scale_accumulator *= fractal_scale;
    }
    return length(z) / scale_accumulator;
}

// ------------------------------------------------------------
// EstimateNormalFromFractalSdf:
//   Aproxima la normal en 'position' usando diferencias finitas sobre FractalSignedDistance.
//   Los parámetros del fractal deben coincidir con los usados en FractalSignedDistance.
// ------------------------------------------------------------
float3 estimate_normal_from_fractal_sdf(float3 position, float overall_cell_size, int iterations, float fractal_scale, float3 fractal_offset)
{
    const float DELTA = 0.0001; // Un delta pequeño para las diferencias finitas.
                                // Puede necesitar ajuste según la escala del fractal.
    float3 offset_x = float3(DELTA, 0, 0);
    float3 offset_y = float3(0, DELTA, 0);
    float3 offset_z = float3(0, 0, DELTA);

    // Calcular la SDF en puntos cercanos para estimar el gradiente.
    float dist_x1 = fractal_signed_distance(position + offset_x, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_x2 = fractal_signed_distance(position - offset_x, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_y1 = fractal_signed_distance(position + offset_y, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_y2 = fractal_signed_distance(position - offset_y, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_z1 = fractal_signed_distance(position + offset_z, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_z2 = fractal_signed_distance(position - offset_z, overall_cell_size, iterations, fractal_scale, fractal_offset);

    float3 gradient = float3(dist_x1 - dist_x2, dist_y1 - dist_y2, dist_z1 - dist_z2);

    return normalize(gradient);
}