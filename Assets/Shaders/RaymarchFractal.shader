Shader "Custom/FullScreenRaymarchFractal"
{
    Properties {
        _MaxSteps("Max Steps", Int) = 128
        _MaxDist("Max Distance", Float) = 100
        _Epsilon("Epsilon", Float) = 0.001
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            Name "FullScreenPass"
            HLSLPROGRAM
            #pragma vertex VS
            #pragma fragment FS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 1) Declaramos CBUFFER para variables de material
            CBUFFER_START(UnityPerMaterial)
                int   _MaxSteps;
                float _MaxDist;
                float _Epsilon;
            CBUFFER_END

            // 2) Estructura de salida del vértice
            struct Varyings {
                float4 position : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            // 3) Generamos triángulo gigante basado en SV_VertexID
            Varyings VS(uint id : SV_VertexID)
            {
                Varyings o;
                // Para id=0,1,2 definimos posiciones que cubren toda la pantalla
                float2 pos = float2(
                    (id == 1) ? 3.0 : -1.0,
                    (id == 2) ? 3.0 : -1.0
                );
                o.position = float4(pos, 0, 1);
                // Mapear UV de 0 a 1
                o.uv = float2(
                    (id == 1) ? 2.0 : 0.0,
                    (id == 2) ? 2.0 : 0.0
                );
                return o;
            }

            // 4) Distance Estimator para Mandelbulb (potencia 8, iteraciones 10)
            float DE(float3 p) {
                float3 z = p;
                float  dr = 1.0;
                float  r  = 0.0;
                const int Power = 8;
                // iterar sobre la fórmula del Mandelbulb
                for (int i = 0; i < 10; i++) {
                    r = length(z);
                    if (r > 2.0) break;
                    float theta = acos(z.z / r);
                    float phi   = atan2(z.y, z.x);
                    dr = pow(r, Power - 1.0) * Power * dr + 1.0;
                    float zr = pow(r, Power);
                    theta *= Power;
                    phi   *= Power;
                    z = zr * float3(
                        sin(theta) * cos(phi),
                        sin(phi) * sin(theta),
                        cos(theta)
                    );
                    z += p;
                }
                return 0.5 * log(r) * r / dr;
            }

            // 5) Cálculo de normales vía gradiente simple
            float3 GetNormal(float3 p) {
                float eps = 0.001;
                float dx = DE(p + float3(eps, 0, 0)) - DE(p - float3(eps, 0, 0));
                float dy = DE(p + float3(0, eps, 0)) - DE(p - float3(0, eps, 0));
                float dz = DE(p + float3(0, 0, eps)) - DE(p - float3(0, 0, eps));
                return normalize(float3(dx, dy, dz));
            }

            // 6) Fragment shader: raymarch desde cámara fija
            half4 FS(Varyings IN) : SV_Target
            {
                // Mapear UV a rango [-1, +1]
                float2 uv = IN.uv * 2 - 1;

                // Definir cámara fija en (0,0,-5) mirando al origen
                float3 ro = float3(0, 0, -5);
                float3 rd = normalize(float3(uv.x, uv.y, 1));

                float t = 0.0;
                // Raymarching principal
                for (int i = 0; i < _MaxSteps; i++)
                {
                    float3 pos = ro + rd * t;
                    float dist = DE(pos);
                    if (dist < _Epsilon)
                    {
                        // Si estamos suficientemente cerca de la superficie:
                        float3 n   = GetNormal(pos);
                        float diff = saturate(dot(n, normalize(float3(1, 1, 1))));
                        // Color basado en iluminación difusa
                        return half4(diff, diff * 0.7, diff * 0.4, 1);
                    }
                    if (t > _MaxDist) break;
                    t += dist;
                }

                // Si no colisionamos, fondo negro
                return half4(0, 0, 0, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
