Shader "Custom/InfiniteSphereField"
{
    Properties
    {
        cam_pos             ("Camera Position",   Vector) = (0,0,-5,0)
        cam_forward         ("Camera Forward",    Vector) = (0,0,1,0)
        cam_right           ("Camera Right",      Vector) = (1,0,0,0)
        cam_up              ("Camera Up",         Vector) = (0,1,0,0)

        sphere_radius       ("Sphere Radius",     Float)  = 1.0
        cell_size           ("Repeat Cell Size",  Float)  = 4.0
        base_color          ("Sphere Base Color", Color)  = (0.2, 0.6, 1.0, 1.0)
        max_ray_distance    ("Max Ray Distance",  Float)  = 100.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            // ======================================================
            // 1) INCLUDES DE ARCHIVOS DE UTILITIES
            // ------------------------------------------------------
            #pragma vertex   Vertex_FullScreenTriangle
            #pragma fragment Fragment_RenderInfiniteSphereField
            #include "SDFUtilities.hlsl"
            #include "RayMarchingUtilities.hlsl"
            #include "SDF_Fractals.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // ======================================================
            
            // —————————————————————————————————————————————
            // 1. CBUFFER para agrupar todas las propiedades
            // —————————————————————————————————————————————
            CBUFFER_START(UnityPerMaterial)
                float4 cam_pos;
                float4 cam_forward;
                float4 cam_right;
                float4 cam_up;

                float  sphere_radius;
                float  cell_size;
                float4 base_color;
                float max_ray_distance;
            
            CBUFFER_END
            // ======================================================
            // 2) OUTPUT DEL VERTEX SHADER
            // ======================================================
            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
                float2 uv      : TEXCOORD0;
            };

            // ======================================================
            // 3) VERTEX SHADER: FULL‐SCREEN TRIANGLE
            // ======================================================
            VertexOutput Vertex_FullScreenTriangle(uint vertexId : SV_VertexID)
            {
                VertexOutput output;

                // Coordenadas “fuera” de [-1,1] para cubrir toda la pantalla
                float2 fullScreenPos = float2(
                    vertexId == 1 ? 3.0 : -1.0,
                    vertexId == 2 ? 3.0 : -1.0
                );
                output.clipPos = float4(fullScreenPos, 0.0, 1.0);

                // UV en [0,2], luego el fragment lo escalará a [0,1]
                output.uv = float2(
                    vertexId == 1 ? 2.0 : 0.0,
                    vertexId == 2 ? 2.0 : 0.0
                );
                return output;
            }

            // ======================================================
            // 4) FRAGMENT SHADER: RECONSTRUCCIÓN DE RAYO + SHADING
            // ======================================================
            float4 Fragment_RenderInfiniteSphereField(VertexOutput IN) : SV_Target
            {                
                // 4.1) Obtener UV en [0,1] y pasar a [-1,1]
                float2 uv01      = IN.uv;
                float2 screenPos = uv01 * 2.0 - 1.0;

                // 4.2) Reconstruir rayo a partir de los parámetros de cámara
                float3 rayOrigin = cam_pos.xyz;
                float3 rayDir    = normalize(
                                        screenPos.x * cam_right.xyz +
                                        screenPos.y * cam_up.xyz    +
                                        cam_forward.xyz );
                

                // 4.3) Ray-marching: llamar a la función importada
                //       (sphereRadius y cellSize vienen de las Properties)
                float hitDistance = PerformRayMarchingSphere(
                                        rayOrigin,
                                        rayDir,
                                        sphere_radius,
                                        cell_size,
                                        max_ray_distance);

    
                // 4.4) Si no colisiona dentro del MAX_RAY_DISTANCE, color fondo
                if (hitDistance > max_ray_distance)
                {
                    return float4(0, 0, 0, 1); // Fondo negro
                }

                // 4.5) Hay impacto: calcular punto y normal
                float3 hitPoint  = rayOrigin + rayDir * hitDistance;
                float3 normalHit = EstimateNormalFromFractalSdf(
                                        hitPoint,
                                        50,1, 1.0, float3(0, 0, 0));

                // 4.6) Sombreado lambertiano
                float3 lightDir    = normalize(float3(0.5, 0.7, -1.0));
                float  lambertTerm = saturate(dot(normalHit, lightDir));
                float3 baseColor   = base_color.rgb;
                float3 shadedColor = baseColor * lambertTerm;

                return float4(shadedColor, 1.0);
            }

            ENDHLSL
        }
    }
    FallBack Off
}
