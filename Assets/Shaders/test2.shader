Shader "Test2"
{
    Properties
    {
        _CamPos       ("Camera Position",   Vector) = (0,0,-5,0)
        _CamForward   ("Camera Forward",    Vector) = (0,0,1,0)
        _CamRight     ("Camera Right",      Vector) = (1,0,0,0)
        _CamUp        ("Camera Up",         Vector) = (0,1,0,0)

        _SphereRadius ("Sphere Radius",     Float)  = 1.0
        _CellSize     ("Repeat Cell Size",  Float)  = 4.0
        _BaseColor    ("Sphere Base Color", Color)  = (0.2, 0.6, 1.0, 1.0)
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
            
            // ======================================================
            
            // —————————————————————————————————————————————
            // 1. CBUFFER para agrupar todas las propiedades
            // —————————————————————————————————————————————
               float4 cam_pos     = float4(0.0f, 0.0f, -5.0f, 1.0f);
                float4 cam_forward = float4(0.0f, 0.0f,  1.0f, 0.0f);
                float4 cam_right   = float4(1.0f, 0.0f,  0.0f, 0.0f);
                float4 cam_up      = float4(0.0f, 1.0f,  0.0f, 0.0f);
                float  sphere_radius = 1.0f;
                float  cell_size     = 1.0f;
                float4 base_color    = float4(1.0f, 1.0f, 1.0f, 1.0f);

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
                float2 uv01      = IN.uv * 0.5;           // [0,2] → [0,1]
                float2 screenPos = uv01 * 2.0 - 1.0;      // [0,1] → [-1,1]

                // 4.2) Reconstruir rayo a partir de los parámetros de cámara
                float3 rayOrigin = cam_pos.xyz;
                float3 rayDir    = normalize(
                                        screenPos.x * cam_right.xyz +
                                        screenPos.y * cam_up.xyz    +
                                        cam_forward.xyz );

                // 4.3) Ray-marching: llamar a la función importada
                //       (sphereRadius y cellSize vienen de las Properties)
                float hitDistance = 50

                // 4.4) Si no colisiona dentro del MAX_RAY_DISTANCE, color fondo
                if (hitDistance > 100.0)
                {
                    return float4(0, 0, 0, 1); // Fondo negro
                }

                // 4.5) Hay impacto: calcular punto y normal
                float3 hitPoint  = rayOrigin + rayDir * hitDistance;
                float3 normalHit = float3(0,0,0);

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
