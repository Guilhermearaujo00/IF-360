Shader "BluBlu/Builtin_Water"
{
    Properties
    {
        [MainColor] _WaterColor ("Water Color", Color) = (0.33,0.70,0.96,1)
        _AbsorptionColor ("Absorption Color", Color) = (0.27,0.03,0,1)
        _FoamColor ("Foam Color", Color) = (0.81,0.93,1,0.45)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0
        _NormalScale ("Normal Scale", Range(0,2)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        fixed4 _WaterColor;
        fixed4 _AbsorptionColor;
        fixed4 _FoamColor;
        half _Smoothness;
        half _Metallic;
        half _NormalScale;

        struct Input
        {
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _WaterColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _WaterColor.rgb * 0.05;
            o.Alpha = _WaterColor.a;
        }
        ENDCG
    }

    FallBack "Standard"
}
