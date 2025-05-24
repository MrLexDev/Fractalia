// ------------------------------------------------------------
// SdfUtilities.hlsl
// ---------------
// Aquí van todas las funciones relacionadas con las SDFs 
// (Signed Distance Functions) y normales.
// ------------------------------------------------------------

// ------------------------------------------------------------
// WrapPointInCell: plegar un punto 3D dentro del cubo [-cellSize/2,cellSize/2]^3
// ------------------------------------------------------------
/*
float3 WrapPointInCell(float3 _point, float cell_size)
{
    float3 half_cell = float3(cell_size * 0.5, cell_size * 0.5, cell_size * 0.5);
    float3 shifted  = _point + half_cell;
    float3 wrapped  = fmod(shifted, cell_size);
    return wrapped - half_cell;
}*/

// ------------------------------------------------------------
// SphereSignedDistance:
//   Devuelve la distancia firmada desde 'position' a la superficie
//   de una esfera de radio 'radius', con repetición de celdas de tamaño 'cellSize'.
// ------------------------------------------------------------
float SphereSignedDistance(float3 position, float radius, float cell_size)
{
    return length(position) - radius;//
    //float3 rep_pos = WrapPointInCell(position, cell_size);
    //return length(rep_pos) - radius;
}

// ------------------------------------------------------------
// EstimateNormalFromSphereSdf:
//   Aproxima la normal en 'position' usando diferencias finitas sobre SphereSignedDistance.
// ------------------------------------------------------------
float3 EstimateNormalFromSphereSdf(float3 position, float radius, float cell_size)
{
    const float DELTA = 0.0001;
    float3 offset_x = float3( DELTA,   0,      0 );
    float3 offset_y = float3(   0,   DELTA,    0 );
    float3 offset_z = float3(   0,     0,   DELTA);

    float dist_x1 = SphereSignedDistance(position + offset_x, radius, cell_size);
    float dist_x2 = SphereSignedDistance(position - offset_x, radius, cell_size);
    float dist_y1 = SphereSignedDistance(position + offset_y, radius, cell_size);
    float dist_y2 = SphereSignedDistance(position - offset_y, radius, cell_size);
    float dist_z1 = SphereSignedDistance(position + offset_z, radius, cell_size);
    float dist_z2 = SphereSignedDistance(position - offset_z, radius, cell_size);

    float3 gradient = float3(dist_x1 - dist_x2, dist_y1 - dist_y2, dist_z1 - dist_z2);
    return normalize(gradient);
}
