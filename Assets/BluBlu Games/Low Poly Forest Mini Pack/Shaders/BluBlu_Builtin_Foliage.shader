Shader "BluBlu/Builtin_Foliage"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Material", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Clip Threshold", Range(0,1)) = 0.5
        [Toggle(_SHOWVERTEXCOLOR_ON)] _ShowVertexColor ("Show Vertex Color", Float) = 0
        [HDR] _EmissionColor ("Emissive Color", Color) = (0,0,0,0)
        [Toggle(_USEEMISSIVEMAP_ON)] _useTextureAsEmissive ("Use Texture As Emissive", Float) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0
        _Metallic ("Metallic", Range(0,1)) = 0

        _LeafColourTint ("Leaf Colour Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0,2)) = 0.08
        _WindSpeed ("Wind Speed", Range(0,10)) = 1
        _WindDensity ("Wind Density", Range(0,20)) = 0.1
        _SmallWindStrength ("Small Wind Strength", Range(0,2)) = 0.05
        _SmallWindSpeed ("Small Wind Speed", Range(0,10)) = 1
        _SmallWindDensity ("Small Wind Density", Range(0,20)) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        Cull Back
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow
        #pragma shader_feature_local _SHOWVERTEXCOLOR_ON
        #pragma shader_feature_local _USEEMISSIVEMAP_ON
        #pragma target 3.0

        sampler2D _BaseMap;
        fixed4 _BaseColor;
        fixed4 _LeafColourTint;
        fixed4 _EmissionColor;
        half _Smoothness;
        half _Metallic;

        struct Input
        {
            float2 uv_BaseMap;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap) * _BaseColor;

            #ifdef _SHOWVERTEXCOLOR_ON
            c.rgb *= IN.color.rgb;
            #endif

            c.rgb *= _LeafColourTint.rgb;

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = c.a;

            #ifdef _USEEMISSIVEMAP_ON
            o.Emission = c.rgb * _EmissionColor.rgb;
            #else
            o.Emission = _EmissionColor.rgb;
            #endif
        }
        ENDCG
    }

    FallBack "Transparent/Cutout/Diffuse"
}
