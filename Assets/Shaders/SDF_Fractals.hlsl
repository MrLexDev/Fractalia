// ------------------------------------------------------------
// --- Funciones para el Fractal Iterativo ---
// ------------------------------------------------------------

float3 WrapPointInCell(float3 _point, float cell_size)
{
    float3 half_cell = float3(cell_size * 0.5, cell_size * 0.5, cell_size * 0.5);
    float3 shifted  = _point + half_cell;
    float3 wrapped  = fmod(shifted, cell_size);
    return wrapped - half_cell;
}

float FractalSignedDistance(float3 position, float overall_cell_size, int iterations, float fractal_scale, float3 fractal_offset)
{
    float3 z = WrapPointInCell(position, overall_cell_size);
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
float3 EstimateNormalFromFractalSdf(float3 position, float overall_cell_size, int iterations, float fractal_scale, float3 fractal_offset)
{
    const float DELTA = 0.0001; // Un delta pequeño para las diferencias finitas.
                                // Puede necesitar ajuste según la escala del fractal.
    float3 offset_x = float3(DELTA, 0, 0);
    float3 offset_y = float3(0, DELTA, 0);
    float3 offset_z = float3(0, 0, DELTA);

    // Calcular la SDF en puntos cercanos para estimar el gradiente.
    float dist_x1 = FractalSignedDistance(position + offset_x, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_x2 = FractalSignedDistance(position - offset_x, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_y1 = FractalSignedDistance(position + offset_y, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_y2 = FractalSignedDistance(position - offset_y, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_z1 = FractalSignedDistance(position + offset_z, overall_cell_size, iterations, fractal_scale, fractal_offset);
    float dist_z2 = FractalSignedDistance(position - offset_z, overall_cell_size, iterations, fractal_scale, fractal_offset);

    float3 gradient = float3(dist_x1 - dist_x2, dist_y1 - dist_y2, dist_z1 - dist_z2);
    
    // Normalizar el gradiente para obtener el vector normal.
    // Si el gradiente es (0,0,0) (puede ocurrir en regiones planas o si DELTA es muy grande),
    // normalize puede devolver NaN o un vector nulo. Considera manejar este caso si es necesario.
    return normalize(gradient);
}

// Ejemplo de cómo podrías usar estas funciones en tu Fragment Shader:
/*
float4 main(PixelInputType input) : SV_TARGET
{
    // Parámetros del fractal (puedes hacerlos uniforms o constantes)
    int FRACTAL_ITERATIONS = 5;
    float FRACTAL_SCALE = 3.0;
    // El offset es crucial para la forma. Prueba valores como:
    // float3 FRACTAL_OFFSET = float3(1.0, 1.0, 1.0); // Un offset base
    // Para una celda de tamaño 'cs', y escala 's', un offset de 'cs / (2.0 * s)' o 'cs * (s-1)/(2*s*s)'
    // puede ser un punto de partida para centrar la estructura.
    // Por ejemplo, si overall_cell_size es 4.0 y FRACTAL_SCALE es 3.0:
    // FRACTAL_OFFSET = float3(4.0 / (2.0 * 3.0), 4.0 / (2.0 * 3.0), 4.0 / (2.0 * 3.0)) = float3(0.666, 0.666, 0.666);
    // O un offset fijo que genere una estructura interesante, ej: float3(0.8, 0.8, 0.8) con escala 2 o 3.
    float3 FRACTAL_OFFSET = float3(1.0, 1.0, 1.0); // Necesitarás experimentar con este.
    float OVERALL_CELL_SIZE = 4.0; // Tamaño de la celda para repetir el fractal.

    // 'world_pos' sería la posición del píxel en el espacio del mundo (obtenida del raymarching)
    float3 world_pos = input.worldPos; // Asumiendo que tienes esta variable

    // Calcular la distancia al fractal
    float dist_to_fractal = FractalSignedDistance(world_pos, OVERALL_CELL_SIZE, FRACTAL_ITERATIONS, FRACTAL_SCALE, FRACTAL_OFFSET);

    // Si el rayo está cerca de la superficie del fractal
    if (dist_to_fractal < 0.01) // Umbral pequeño
    {
        // Estimar la normal
        float3 normal = EstimateNormalFromFractalSdf(world_pos, OVERALL_CELL_SIZE, FRACTAL_ITERATIONS, FRACTAL_SCALE, FRACTAL_OFFSET);
        
        // Aplicar iluminación (ejemplo simple de iluminación difusa)
        float3 light_dir = normalize(float3(0.5, 1.0, -0.5));
        float diffuse = saturate(dot(normal, light_dir)) * 0.7 + 0.3; // Ambient + Diffuse
        return float4(diffuse * float3(0.8, 0.7, 0.5), 1.0); // Color del fractal
    }
    else
    {
        return float4(0.1, 0.1, 0.2, 1.0); // Color de fondo
    }
}
*/