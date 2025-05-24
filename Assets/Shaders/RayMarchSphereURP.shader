Shader "Custom/ExampleWithCameraProps"
{
    Properties
    {
        _CamPos     ("Camera Position",    Vector) = (0,0, -5, 0)
        _CamForward ("Camera Forward",     Vector) = (0, 0, 1, 0)
        _CamRight   ("Camera Right",       Vector) = (1, 0, 0, 0)
        _CamUp      ("Camera Up",          Vector) = (0, 1, 0, 0)

        _SphereRadius("Sphere Radius",     Float ) = 1.0
        _CellSize    ("Repeat Cell Size",  Float ) = 4.0
        _BaseColor   ("Sphere Base Color", Color ) = (0.2, 0.6, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex   VertexFullScreenTriangle
            #pragma fragment FragmentWithCameraProps
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // —————————————————————————————————————————————
            // 1. CBUFFER para agrupar todas las propiedades
            // —————————————————————————————————————————————
            CBUFFER_START(UnityPerMaterial)
                float4 _CamPos;
                float4 _CamForward;
                float4 _CamRight;
                float4 _CamUp;

                float  _SphereRadius;
                float  _CellSize;
                float4 _BaseColor;
            CBUFFER_END

            // ------------------------------------------------------------
            // Aquí vendrían las funciones auxiliares (RepeatPointInCell, etc.)
            // ------------------------------------------------------------
            float3 RepeatPointInCell(float3 _point, float cellSize)
            {
                float3 halfCell = float3(cellSize * 0.5, cellSize * 0.5, cellSize * 0.5);
                float3 wrapped  = fmod(_point + halfCell, cellSize);
                return wrapped - halfCell;
            }

            
            float ComputeSphereSdf(float3 positionSample, float sphereRadius, float cellSize)
            {
                float3 repeatedPosition = RepeatPointInCell(positionSample, cellSize);
                return length(repeatedPosition) - sphereRadius;
                /*
                float3 halfExtents = float3(.5, .5, .5);
                float3 q = abs(repeatedPosition) - halfExtents;
                return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                */
            }

            float3 EstimateNormalFromSdf(float3 surfacePoint, float sphereRadius, float cellSize)
            {
                const float delta = 0.0001;
                float3 dx = float3(delta, 0, 0);
                float3 dy = float3(0, delta, 0);
                float3 dz = float3(0, 0, delta);

                float distX1 = ComputeSphereSdf(surfacePoint + dx, sphereRadius, cellSize);
                float distX2 = ComputeSphereSdf(surfacePoint - dx, sphereRadius, cellSize);
                float distY1 = ComputeSphereSdf(surfacePoint + dy, sphereRadius, cellSize);
                float distY2 = ComputeSphereSdf(surfacePoint - dy, sphereRadius, cellSize);
                float distZ1 = ComputeSphereSdf(surfacePoint + dz, sphereRadius, cellSize);
                float distZ2 = ComputeSphereSdf(surfacePoint - dz, sphereRadius, cellSize);

                float3 gradient = float3(distX1 - distX2, distY1 - distY2, distZ1 - distZ2);
                return normalize(gradient);
            }

            float PerformRayMarching(float3 rayOrigin, float3 rayDirection, float sphereRadius, float cellSize)
            {
                const int   MAX_STEPS        = 64;
                const float SURFACE_EPSILON  = 0.001;
                const float MAX_RAY_DISTANCE = 100.0;
                float traveledDistance = 0;

                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 samplePoint       = rayOrigin + rayDirection * traveledDistance;
                    float  distanceToSurface = ComputeSphereSdf(samplePoint, sphereRadius, cellSize);
                    // Parámetros que puedes exponer como constantes o Properties:
                    int   mandelMaxIter = 12;    // Número máximo de iteraciones (12 suele dar resultado decente)
                    float mandelPower   = 8.0;   // “Grado” del Mandelbulb (8 por defecto)
                    float mandelBailout = 2.0;   // Bailout radius (suele usarse 2.0)

                    //float distanceToSurface = ComputeMandelbulbSdf(samplePoint, mandelMaxIter, mandelPower, mandelBailout);


                    if (distanceToSurface < SURFACE_EPSILON)
                        return traveledDistance;
                    if (traveledDistance > MAX_RAY_DISTANCE)
                        break;

                    traveledDistance += distanceToSurface;
                }
                return 1e5;
            }

            // ------------------------------------------------------------
            // 2. Vertex Shader (sin cambios de cámara fija)
            // ------------------------------------------------------------
            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
                float2 uv      : TEXCOORD0;
            };

            VertexOutput VertexFullScreenTriangle(uint vertexId : SV_VertexID)
            {
                VertexOutput output;
                float2 fullScreenPositions = float2(
                    vertexId == 1 ? 3.0 : -1.0,
                    vertexId == 2 ? 3.0 : -1.0
                );
                output.clipPos = float4(fullScreenPositions, 0, 1);
                output.uv      = float2(
                    vertexId == 1 ? 2.0 : 0.0,
                    vertexId == 2 ? 2.0 : 0.0
                );
                return output;
            }

            // ------------------------------------------------------------
            // 3. Fragment Shader (reconstruye rayo usando variables de CBUFFER)
            // ------------------------------------------------------------
            float4 FragmentWithCameraProps(VertexOutput input) : SV_Target
            {
                float2 uvNormalized = input.uv * 0.5;      // [0,2] → [0,1]
                float2 screenPos    = uvNormalized * 2 - 1; // [0,1] → [-1,+1]

                // 3.1. Reconstrucción del rayo usando los vectores de la cámara
                float3 cameraPosition = _CamPos.xyz;
                float3 rayDir = normalize(
                    screenPos.x * _CamRight.xyz +
                    screenPos.y * _CamUp.xyz +
                                  _CamForward.xyz
                );

                // 3.2. Parámetros de la esfera y repetición
                float sphereRadius = _SphereRadius;
                float cellSize     = _CellSize;

                // 3.3. Ray-marching en espacio repetido
                float hitDist = PerformRayMarching(cameraPosition, rayDir, sphereRadius, cellSize);
                if (hitDist > 100.0)
                    return float4(0, 0, 0, 1); // fondo negro

                

                // 3.4. Cálculo de color con iluminación lambertiana
                float3 hitPoint     = cameraPosition + rayDir * hitDist;
                float3 normalAtHit  = EstimateNormalFromSdf(hitPoint, sphereRadius, cellSize);
                //float3 normalAtHit = EstimateNormalMandelbulb(hitPoint, 30, 10, 2);
                float3 lightDir     = normalize(float3(0.5, 0.7, -1.0));
                float  lambertTerm  = saturate(dot(normalAtHit, lightDir));
                float3 baseColor    = _BaseColor.rgb;
                float3 shadedColor  = baseColor * lambertTerm;

                return float4(shadedColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
