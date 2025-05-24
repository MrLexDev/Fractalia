// ------------------------------------------------------------
// 1) Función SDF del fractal (Mandelbulb 3D)
// ------------------------------------------------------------
/*
float fractal_signed_distance(float3 position)
{
    float3 z = position;
    float radius_derivative = 1.0;
    float radius  = 0.0;

    for (int i = 0; i < iterations; ++i)
    {
        radius = length(z);
        if (radius > bailout) 
            break;

        // Convertir a coordenadas esféricas
        float theta = acos(z.z / radius);
        float phi   = atan2(z.y, z.x);

        // Derivada para el factor de escape (radius_derivative)
        float radius_power = pow(max(radius, 1e-6), power - 1.0);
        radius_derivative  = radius_power * power * radius_derivative + 1.0;

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

float smooth_sdf(float3 p)
{
    // Valor central
    float center = fractal_signed_distance(p);

    // Valores en +DELTA y -DELTA en X
    float dx1 = fractal_signed_distance(p + float3(normal_delta, 0, 0));
    float dx2 = fractal_signed_distance(p - float3(normal_delta, 0, 0));

    // Valores en +DELTA y -DELTA en Y
    float dy1 = fractal_signed_distance(p + float3(0, normal_delta, 0));
    float dy2 = fractal_signed_distance(p - float3(0, normal_delta, 0));

    // Valores en +DELTA y -DELTA en Z
    float dz1 = fractal_signed_distance(p + float3(0, 0, normal_delta));
    float dz2 = fractal_signed_distance(p - float3(0, 0, normal_delta));

    // Promediamos: centro + seis vecinos (vecindario 3×3×3 muy básico)
    float sum = center + dx1 + dx2 + dy1 + dy2 + dz1 + dz2;
    return sum / 7.0;
}


// ------------------------------------------------------------
// 2) Normal estimation basada en la SDF del fractal
// ------------------------------------------------------------
float3 estimate_normal_from_fractal_sdf(float3 p)
{
    float3 dx = float3(normal_delta, 0.0,          0.0);
    float3 dy = float3(0.0,          normal_delta, 0.0);
    float3 dz = float3(0.0,          0.0,          normal_delta);

    // En lugar de fractal_signed_distance, usamos smooth_sdf:
    float nx = smooth_sdf(p + dx) - smooth_sdf(p - dx);
    float ny = smooth_sdf(p + dy) - smooth_sdf(p - dy);
    float nz = smooth_sdf(p + dz) - smooth_sdf(p - dz);

    return normalize(float3(nx, ny, nz));
}

float ambient_occlusion(float3 p, float3 normal)
{
    float ao = 0.0;
    float sca = 1.0;
    for (int i = 1; i <= 5; i++)
    {
        float dist = fractal_signed_distance(p + normal * i * 0.1);
        ao += (i * 0.1 - dist) * sca;
        sca *= 0.5;
    }
    return saturate(1.0 - ao);
}
*/

// Parámetros generales (ajústalos según te convenga)
static const int   MB_ITERATIONS  = 15;      // Número máximo de iteraciones
/*
static const float MB_SCALE       = 2.0;     // Factor de escalado (típicamente entre 2.0 y 3.0)
static const float MB_MIN_RADIUS  = 0.5;     // Radio mínimo para el sphere-fold
static const float MB_FIXED_RADIUS= 1.0;     // Radio fijo para el sphere-fold
static const float MB_BOX_LIMIT   = 1.0;     // Límite de box-fold en cada eje (±1.0)
*/

// Delta para estimar normales (idéntico al ejemplo que diste)
//static const float normal_delta = 0.0005;


// -----------------------------------------------------------------------
// 1) Distance Estimator (SDF) para Mandelbox
// -----------------------------------------------------------------------
float fractal_signed_distance(float3 position)
{
    // Inicializamos z = p y derivada dr = 1
    float3 z = position;
    float dr = 1.0;

    for (int i = 0; i < MB_ITERATIONS; i++)
    {
        // -----------------------
        // 1) Box-fold (fold en cada eje)
        // -----------------------
        // Si algún componente de z está fuera de [-MB_BOX_LIMIT, +MB_BOX_LIMIT],
        // lo plegamos reflejándolo (“box-fold”). La forma compacta es:
        z = clamp(z, -MB_BOX_LIMIT, MB_BOX_LIMIT) * 2.0 - z;
        // (equivale a: if(z.x > +1) z.x = 2 - z.x; if(z.x < -1) z.x = -2 - z.x; y igual para y,z)

        // -----------------------
        // 2) Sphere-fold (fold esférico)
        // -----------------------
        float r2 = dot(z, z);
        if (r2 < MB_MIN_RADIUS * MB_MIN_RADIUS)
        {
            // Si estamos dentro del círculo de radio mínimo, escalamos hasta MB_FIXED_RADIUS
            float factor = (MB_FIXED_RADIUS * MB_FIXED_RADIUS) / (MB_MIN_RADIUS * MB_MIN_RADIUS);
            z *= factor;
            dr *= factor;
        }
        else if (r2 < MB_FIXED_RADIUS * MB_FIXED_RADIUS)
        {
            // Si estamos entre minRadius y fixedRadius, escalamos hasta fixedRadius^2 / r2
            float factor = (MB_FIXED_RADIUS * MB_FIXED_RADIUS) / r2;
            z *= factor;
            dr *= factor;
        }
        // Si r2 >= fixedRadius^2, no hacemos sphere-fold.

        // -----------------------
        // 3) Escalado y translação (scale + translate)
        // -----------------------
        z = z * MB_SCALE + position;
        // Actualizamos derivada de la iteración: dr = dr * |scale| + 1
        dr = dr * abs(MB_SCALE) + 1.0;
    }

    // Finalmente, devolvemos la distancia aproximada: |z| / |dr|
    return length(z) / abs(dr);
}


// -----------------------------------------------------------------------
// 2) Smooth SDF (para suavizar ruido al estimar normales)
// -----------------------------------------------------------------------
float smooth_sdf(float3 p)
{
    float center = fractal_signed_distance(p);

    float dx1 = fractal_signed_distance(p + float3(normal_delta, 0, 0));
    float dx2 = fractal_signed_distance(p - float3(normal_delta, 0, 0));

    float dy1 = fractal_signed_distance(p + float3(0, normal_delta, 0));
    float dy2 = fractal_signed_distance(p - float3(0, normal_delta, 0));

    float dz1 = fractal_signed_distance(p + float3(0, 0, normal_delta));
    float dz2 = fractal_signed_distance(p - float3(0, 0, normal_delta));

    float sum = center + dx1 + dx2 + dy1 + dy2 + dz1 + dz2;
    return sum / 7.0;
}


// -----------------------------------------------------------------------
// 3) Estimación de normales (basada en la SDF suavizada)
// -----------------------------------------------------------------------
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


// -----------------------------------------------------------------------
// 4) Ambient Occlusion aproximada
// -----------------------------------------------------------------------
float ambient_occlusion(float3 p, float3 normal)
{
    float ao = 0.0;
    float sca = 1.0;

    // Probamos 5 pasos a lo largo de la normal: usan la SDF directa (no suavizada)
    for (int i = 1; i <= 5; i++)
    {
        float dist = fractal_signed_distance(p + normal * (i * 0.1));
        ao += (i * 0.1 - dist) * sca;
        sca *= 0.5;
    }

    return saturate(1.0 - ao);
}
