Shader "Custom/WaterGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0, 0.5, 1, 0.8)
        _BottomColor ("Bottom Color", Color) = (0, 0.2, 0.8, 1)
        _GradientHeight ("Gradient Height", Range(0, 5)) = 1.0
        _GradientOffset ("Gradient Offset", Range(-2, 2)) = 0.0
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.0
        _WaveStrength ("Wave Strength", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float height : TEXCOORD2;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _GradientHeight;
            float _GradientOffset;
            float _WaveSpeed;
            float _WaveStrength;

            v2f vert (appdata v)
            {
                v2f o;

                float wave = sin(_Time.y * _WaveSpeed + v.vertex.x * 2.0 + v.vertex.z * 3.0) * _WaveStrength;
                v.vertex.y += wave;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                o.height = (v.vertex.y + 0.5) * 2.0;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float gradient = saturate((i.height + _GradientOffset) / _GradientHeight);

                fixed4 color = lerp(_BottomColor, _TopColor, gradient);

                float specular = pow(max(0, dot(normalize(i.worldPos - _WorldSpaceCameraPos), float3(0,1,0))), 32);
                color.rgb += specular * 0.3;
                
                return color;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}