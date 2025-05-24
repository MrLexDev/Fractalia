// ------------------------------------------------------------
// 1) Función SDF del fractal (Mandelbulb 3D)
// ------------------------------------------------------------
float fractal_signed_distance(float3 position)
{
    float3 z = position;
    float radius_derivative = 1.0;
    float radius  = 0.0;

    for (int i = 0; i < iterations; ++i)
    {
        radius = length(z);
        if (radius > 2.0) 
            break;

        // Convertir a coordenadas esféricas
        float theta = acos(z.z / radius);
        float phi   = atan2(z.y, z.x);

        // Derivada para el factor de escape (dr)
        radius_derivative  = pow(radius, power - 1.0) * power * radius_derivative  + 1.0;

        // Elevamos r^POWER y multiplicamos los ángulos por POWER
        float zr = pow(radius, power);
        theta *= power;
        phi   *= power;

        // Reconstruir z en coordenadas cartesianas
        float sinTheta = sin(theta);
        float3 zNew = float3(
            zr * sinTheta * cos(phi),
            zr * sinTheta * sin(phi),
            zr * cos(theta)
        );

        // Iteración de z_{n+1} = zNew + pos (atracción hacia el punto original)
        z = zNew + position;
    }

    // Distancia aproximada según la fórmula del Mandelbulb SDF
    return 0.5 * log(radius) * radius / radius_derivative ;
}


// ------------------------------------------------------------
// 2) Normal estimation basada en la SDF del fractal
// ------------------------------------------------------------
float3 estimate_normal_from_fractal_sdf(float3 p)
{
    const float DELTA = 0.0005;
    float3 dx = float3(DELTA, 0.0, 0.0);
    float3 dy = float3(0.0, DELTA, 0.0);
    float3 dz = float3(0.0, 0.0, DELTA);

    float nx = fractal_signed_distance(p + dx) - fractal_signed_distance(p - dx);
    float ny = fractal_signed_distance(p + dy) - fractal_signed_distance(p - dy);
    float nz = fractal_signed_distance(p + dz) - fractal_signed_distance(p - dz);

    return normalize(float3(nx, ny, nz));
}