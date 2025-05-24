// ------------------------------------------------------------
// SdfUtilities.hlsl
// ---------------
// Aquí van todas las funciones relacionadas con las SDFs 
// (Signed Distance Functions) y normales.
// ------------------------------------------------------------

// ------------------------------------------------------------
// WrapPointInCell: plegar un punto 3D dentro del cubo [-cellSize/2,cellSize/2]^3
// ------------------------------------------------------------

float3 wrap_point_in_cell(float3 _point, float cell_size)
{
    return _point - cell_size * round(_point / cell_size);
}

float sphere_signed_distance(float3 position, float radius, float cell_size)
{
    //return length(position) - radius;
    float3 rep_pos = wrap_point_in_cell(position, cell_size);
    return length(rep_pos) - radius;
}

float3 estimate_normal_from_sphere_sdf(float3 position, float radius, float cell_size)
{
    const float DELTA = 0.0001;
    float3 offset_x = float3( DELTA,   0,      0 );
    float3 offset_y = float3(   0,   DELTA,    0 );
    float3 offset_z = float3(   0,     0,   DELTA);

    float dist_x1 = sphere_signed_distance(position + offset_x, radius, cell_size);
    float dist_x2 = sphere_signed_distance(position - offset_x, radius, cell_size);
    float dist_y1 = sphere_signed_distance(position + offset_y, radius, cell_size);
    float dist_y2 = sphere_signed_distance(position - offset_y, radius, cell_size);
    float dist_z1 = sphere_signed_distance(position + offset_z, radius, cell_size);
    float dist_z2 = sphere_signed_distance(position - offset_z, radius, cell_size);

    float3 gradient = float3(dist_x1 - dist_x2, dist_y1 - dist_y2, dist_z1 - dist_z2);
    return normalize(gradient);
}
