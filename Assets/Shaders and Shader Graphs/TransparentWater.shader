Shader "Custom/TransparentWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.2, 0.6, 1, 0.8)
        _MainTex ("Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.1
        _Transparency ("Transparency", Range(0,1)) = 0.8
        _Fresnel ("Fresnel Effect", Range(0,5)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
        };

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        half _Transparency;
        half _Fresnel;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            float fresnel = dot(IN.worldNormal, IN.viewDir);
            fresnel = saturate(1.0 - fresnel);
            fresnel = pow(fresnel, _Fresnel);
            
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a * _Transparency * (1.0 - fresnel * 0.5);
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}