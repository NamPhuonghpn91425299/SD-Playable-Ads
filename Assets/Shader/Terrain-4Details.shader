Shader "Horus/Terrain/4 Details"
{
    Properties
    {
         [Header(Alpha)][Toggle] _Toggle4 ("Show detail 4", float) = 0
        _Tiling4 ("Tiling 4", range(0, 200)) = 50
        [NoScaleOffset] _Tex4 ("Detail 4", 2D) = "white" {}
        
        [Header(Blue)][Toggle] _Toggle3 ("Show detail 3", float) = 0
        _Tiling3 ("Tiling 3", range(0, 200)) = 50
        [NoScaleOffset] _Tex3 ("Detail 3", 2D) = "white" {}
        
        [Header(Green)][Toggle] _Toggle2 ("Show detail 2", float) = 0
        _Tiling2 ("Tiling 2", range(0, 200)) = 50
        [NoScaleOffset] _Tex2 ("Detail 2", 2D) = "white" {}
        
        [Header(Red)][Toggle] _Toggle1 ("Show detail 1", float) = 0
        _Tiling1 ("Tiling 1", range(0, 200)) = 50
        [NoScaleOffset] _Tex1 ("Detail 1", 2D) = "white" {}
        
        [NoScaleOffset] _Mask ("Mask", 2D) = "gray" {}
        
        _Tiling ("Base Tiling", range(0, 200)) = 50
        [NoScaleOffset] _Base ("Base", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd
        #pragma shader_feature _TOGGLE1_ON
        #pragma shader_feature _TOGGLE2_ON
        #pragma shader_feature _TOGGLE3_ON
        #pragma shader_feature _TOGGLE4_ON

        sampler2D _Base, _Mask, _Tex1, _Tex2, _Tex3, _Tex4, _OcclusionMap;

        struct Input
        {
            float2 uv_Base;
        };

        half _Glossiness;
        half _Metallic;
        float _Tiling, _Tiling1, _Tiling2, _Tiling3, _Tiling4, _Tiling5, _Tiling6;

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_Base, IN.uv_Base * _Tiling);
            fixed4 mask = tex2D(_Mask, IN.uv_Base);
            #if _TOGGLE1_ON
            c = lerp(c, tex2D(_Tex1, IN.uv_Base * _Tiling1), mask.r);
            #endif
            #if _TOGGLE2_ON
            c = lerp(c, tex2D(_Tex2, IN.uv_Base * _Tiling2), mask.g);
            #endif
            #if _TOGGLE3_ON
            c = lerp(c, tex2D(_Tex3, IN.uv_Base * _Tiling3), mask.b);
            #endif
            #if _TOGGLE4_ON
            c = lerp(c, tex2D(_Tex4, IN.uv_Base * _Tiling4), mask.a);
            #endif

            o.Albedo = c.rgb;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Mobile/VertexLit"
//    CustomEditor "_Game.THIN.Scripts.Editor.ShaderCustomEditors.Terrain4DetailsShaderGUI"
}