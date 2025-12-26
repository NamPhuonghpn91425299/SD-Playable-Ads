Shader "Horus/Test/Terrain 8 Details"
{
    Properties
    {
        _Tiling6 ("Tiling 6", range(0, 200)) = 50
        [NoScaleOffset] _Tex6 ("Detail 4", 2D) = "white" {}
        _Tiling5 ("Tiling 5", range(0, 200)) = 50 
        [NoScaleOffset] _Tex5 ("Detail 4", 2D) = "white" {}
        _Tiling4 ("Tiling 4", range(0, 200)) = 50
        [NoScaleOffset] _Tex4 ("Detail 4", 2D) = "white" {}
        _Tiling3 ("Tiling 3", range(0, 200)) = 50
        [NoScaleOffset] _Tex3 ("Detail 3", 2D) = "white" {}
        _Tiling2 ("Tiling 2", range(0, 200)) = 50
        [NoScaleOffset] _Tex2 ("Detail 2", 2D) = "white" {}
        _Tiling1 ("Tiling 1", range(0, 200)) = 50
        [NoScaleOffset] _Tex1 ("Detail 1", 2D) = "white" {}
        _Tiling ("Base Tiling", range(0, 200)) = 50
        [NoScaleOffset] _Base ("Base", 2D) = "white" {}
        [NoScaleOffset] _Mask1 ("Mask 1 - 2 - 3", 2D) = "black" {}
        [NoScaleOffset] _Mask2 ("Mask 4 - 5 - 6", 2D) = "black" {}
        [Header(Other)]
        _OcclusionStrength ("Occlusion Strength", range(0, 1)) = 0.0
        [NoScaleOffset] _OcclusionMap ("Occlusion", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _Mask1, _Mask2, _Tex1, _Tex2, _Tex3, _Tex4, _Tex5, _Tex6, _OcclusionMap;

        struct Input
        {
            float2 uv_Base;
        };

        half _Glossiness;
        half _Metallic;
        float _Tiling, _Tiling1, _Tiling2, _Tiling3, _Tiling4, _Tiling5, _Tiling6, _OcclusionStrength;

        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_Base * _Tiling);
            fixed4 mask = tex2D(_Mask1, IN.uv_Base);
            c = lerp(c, tex2D(_Tex1, IN.uv_Base * _Tiling1), mask.r);
            c = lerp(c, tex2D(_Tex2, IN.uv_Base * _Tiling2), mask.g);
            c = lerp(c, tex2D(_Tex3, IN.uv_Base * _Tiling3), mask.b);
            
            c = lerp(c, tex2D(_Tex4, IN.uv_Base * _Tiling4), mask.r);
            c = lerp(c, tex2D(_Tex5, IN.uv_Base * _Tiling5), mask.g);
            c = lerp(c, tex2D(_Tex6, IN.uv_Base * _Tiling6), mask.b);

            c = lerp(c, tex2D(_OcclusionMap, IN.uv_Base).r, _OcclusionStrength);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}