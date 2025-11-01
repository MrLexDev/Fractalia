
#ifndef SDF_FRACTALS_INCLUDED
#define SDF_FRACTALS_INCLUDED

struct fractal_output {
    float sdf_distance;
    int ray_steps;
    float ray_march_distance;
    float orbit_min_dist;
};

static const int   MENGER_ITERATIONS    = 10;
//static const float3 JULIA_CONSTANT      = float3(-0.745, 0.113, 0.274);
static const float  JULIA_MIN_RADIUS    = 1e-6;

// ------------------------------------------------------------
// 1) Función SDF del fractal (Mandelbulb 3D)
// ------------------------------------------------------------
fractal_output sdf_mandelbulb(float3 position)
{
    fractal_output result = {0.0, 0.0, 0.0, 1e20};
    float3 z = position;
    float radius_derivative = 1.0;
    float radius = 0;
    float orbit_min = 1e20;

    for (int i = 0; i < iterations; ++i)
    {
        radius = length(z);
        orbit_min = min(orbit_min, radius);
        if (radius > bailout)
            break;

        // Convertir a coordenadas esféricas evitando divisiones por cero
        float invRadius = radius > 1e-6 ? 1.0 / radius : 0.0;
        float theta = radius > 1e-6 ? acos(z.z * invRadius) : 0.0;
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

    result.sdf_distance = 0.5 * log(radius) * radius / radius_derivative;
    result.orbit_min_dist = orbit_min;
    return result;
}

float sd_cross(float3 p, float b)
{
    float3 q = abs(p);
    return min(min(max(q.x, q.y), max(q.y, q.z)), max(q.z, q.x)) - b;
}

fractal_output sdf_menger_sponge(float3 position)
{
    fractal_output result = {0.0, 0.0, 0.0, 1e20};

    float distance = sd_cross(position, 1.0);
    float scale = 1.0;
    float orbit_min = length(position);

    for (int i = 0; i < MENGER_ITERATIONS; ++i)
    {
        float3 scaled = position * scale;
        float3 cell = frac(scaled * 0.5 + 0.5) * 2.0 - 1.0;
        scale *= 3.0;

        float3 r = abs(1.0 - 3.0 * abs(cell));
        float c = (min(min(r.x, r.y), r.z) - 1.0) / scale;
        distance = max(distance, c);

        orbit_min = min(orbit_min, length(cell));
    }

    result.sdf_distance = distance;
    result.orbit_min_dist = orbit_min;
    return result;
}

fractal_output sdf_quaternion_julia(float3 position)
{
    fractal_output result = {0.0, 0.0, 0.0, 1e20};
    float3 z = position;
    float radius = length(z);
    float derivative = 1.0;
    float orbit_min = radius;

    for (int i = 0; i < iterations; ++i)
    {
        radius = length(z);
        orbit_min = min(orbit_min, radius);
        if (radius > bailout)
        {
            break;
        }

        float safe_radius = max(radius, JULIA_MIN_RADIUS);
        derivative = 2.0 * derivative * safe_radius;

        float x = z.x;
        float y = z.y;
        float zVal = z.z;

        float x2 = x * x;
        float y2 = y * y;
        float z2 = zVal * zVal;

        float newX = x2 - y2 - z2 + julia_constant.x;
        float newY = 2.0 * x * y + julia_constant.y;
        float newZ = 2.0 * x * zVal + julia_constant.z;

        z = float3(newX, newY, newZ);
    }

    float safe_radius_final = max(radius, JULIA_MIN_RADIUS);
    float safe_derivative = max(derivative, JULIA_MIN_RADIUS);
    result.sdf_distance = 0.5 * log(safe_radius_final) * safe_radius_final / safe_derivative;
    result.orbit_min_dist = orbit_min;
    return result;
}

// -----------------------------------------------------------------------
// 1) Distance Estimator (SDF) para Mandelbox
// -----------------------------------------------------------------------
fractal_output sdf_mandelbox(float3 position)
{
    fractal_output result = {0.0, 0.0, 0.0, 1e20};
    // Inicializamos z = p y derivada dr = 1
    float3 z = position;
    float dr = 1.0;
    float orbit_min = 1e20;

    for (int i = 0; i < iterations; i++)
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
        orbit_min = min(orbit_min, sqrt(r2));
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
    result.sdf_distance = length(z) / abs(dr);
    result.orbit_min_dist = orbit_min;
    return result;
}



// -----------------------------------------------------------------------
// SDF para una Pirámide de Sierpinski (Tetraedro de Sierpinski)
// -----------------------------------------------------------------------
static const int   SIERPINSKI_ITERATIONS = 15;
static const float SIERPINSKI_SCALE      = 2.0;
static const float3 SIERPINSKI_OFFSET_VEC = float3(1.0, 1.0, 1.0);

static const float SIERPINSKI_STRUT_THICKNESS = 1.2; // Ajusta el "grosor" de las barras del fractal.
                                                     // Corresponde al radio 'C' en la fórmula (length(p) - C) / s.
                                                     // Un valor entre 1.0 y 1.5 suele funcionar bien con un OFFSET_VEC de magnitud sqrt(3).

fractal_output sdf_sierpinski(float3 p)
{
    fractal_output result = {0.0, 0.0, 0.0, 1e20};
    float accumulated_total_scale = 1.0;
    float orbit_min = 1e20;

    for (int i = 0; i < SIERPINSKI_ITERATIONS; ++i)
    {
        // Si p.x + p.y < 0, refleja p a través del plano x = -y
        if (p.x + p.y < 0.0) { p.xy = float2(-p.y, -p.x); }
        // Si p.x + p.z < 0, refleja p a través del plano x = -z
        if (p.x + p.z < 0.0) { p.xz = float2(-p.z, -p.x); }
        // Si p.y + p.z < 0, refleja p a través del plano y = -z
        if (p.y + p.z < 0.0) { p.yz = float2(-p.z, -p.y); }
        
        p = p * SIERPINSKI_SCALE - SIERPINSKI_OFFSET_VEC * (SIERPINSKI_SCALE - 1.0);
        
        orbit_min = min(orbit_min, length(p));
        accumulated_total_scale *= SIERPINSKI_SCALE;
    }
    result.sdf_distance = (length(p) - SIERPINSKI_STRUT_THICKNESS) / accumulated_total_scale;
    result.orbit_min_dist = orbit_min;
    return result;
}

fractal_output fractal_signed_distance(float3 position)
{
    if (fractal_type == 1)
    {
        return sdf_mandelbox(position);
    }
    else if (fractal_type == 2)
    {
        return sdf_sierpinski(position);
    }
    else if (fractal_type == 3)
    {
        return sdf_menger_sponge(position);
    }
    else if (fractal_type == 4)
    {
        return sdf_quaternion_julia(position);
    }

    return sdf_mandelbulb(position);
}


#endif // SDF_FRACTALS_INCLUDED
