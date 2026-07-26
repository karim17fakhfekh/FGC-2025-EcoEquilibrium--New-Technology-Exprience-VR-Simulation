Shader "Custom/BranchDissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normal);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                float dissolve = (IN.worldPos.x + IN.worldPos.y + IN.worldPos.z) * 0.1;
                dissolve = frac(dissolve);
                
                float3 worldCenter = float3(0, 0, 0);
                float distanceToCenter = distance(IN.worldPos, worldCenter);
                float dissolvePattern = dissolve * (1.0 - distanceToCenter * 0.1);

                if (dissolvePattern < _DissolveAmount)
                    discard;

                float edge = smoothstep(_DissolveAmount, _DissolveAmount + _EdgeWidth, dissolvePattern);
                if (edge > 0.1)
                {
                    col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge);
                    col.rgb *= _EdgeColor.a * 2.0;
                }
                
                return col;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}