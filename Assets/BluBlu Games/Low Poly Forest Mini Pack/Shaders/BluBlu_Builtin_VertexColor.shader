Shader "BluBlu/Builtin_VertexColor"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Material", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Color ("Color", Color) = (1,1,1,1)
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
        [Toggle(_VERTEXCOLOR_ON)] _UseVertexColor ("Use Vertex Color", Float) = 1
        _Cutoff ("Alpha Clip Threshold", Range(0,1)) = 0.5
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Alpha Clip", Float) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0
        _Metallic ("Metallic", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma shader_feature_local _VERTEXCOLOR_ON
        #pragma shader_feature_local _ALPHATEST_ON
        #pragma target 3.0

        sampler2D _BaseMap;
        fixed4 _BaseColor;
        fixed4 _Color;
        fixed4 _ColorTint;
        half _Smoothness;
        half _Metallic;
        half _Cutoff;

        struct Input
        {
            float2 uv_BaseMap;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_BaseMap, IN.uv_BaseMap);
            c.rgb *= _BaseColor.rgb;
            c.rgb *= _Color.rgb;
            c.rgb *= _ColorTint.rgb;

            #ifdef _VERTEXCOLOR_ON
            c.rgb *= IN.color.rgb;
            #endif

            #ifdef _ALPHATEST_ON
            o.Alpha = c.a;
            clip(o.Alpha - _Cutoff);
            #endif

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }

    FallBack "Standard"
}
