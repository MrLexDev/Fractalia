Shader "Custom/FullScreenTriangle"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VS
            #pragma fragment FS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings {
                float4 position : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            Varyings VS(uint id : SV_VertexID)
            {
                Varyings o;
                float2 pos = float2(
                    id == 1 ? 3.0 : -1.0,
                    id == 2 ? 3.0 : -1.0
                );
                o.position = float4(pos, 0, 1);
                o.uv = float2(
                    id == 1 ? 2.0 : 0.0,
                    id == 2 ? 2.0 : 0.0
                );
                return o;
            }

            float4 FS(Varyings IN) : SV_Target
            {
                // Por ahora, visualizamos las UV:
                return float4(IN.uv, 0, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
