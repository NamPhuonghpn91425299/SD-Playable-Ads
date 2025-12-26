Shader "Horus/Terrain/4 Rotatable Details"
{
    Properties
    {
        [Header(Alpha)][Toggle] _Toggle4 ("Show detail 4", float) = 0
        _Tiling4 ("Scale (xy) and Offset (zw) 4", vector) = (50, 50, 0, 0)
        _Rotate4 ("Rotate 4", range(0, 360)) = 0
        [NoScaleOffset] _Tex4 ("Detail 4", 2D) = "white" {}

        [Header(Blue)][Toggle] _Toggle3 ("Show detail 3", float) = 0
        _Tiling3 ("Scale (xy) and Offset (zw) 3", vector) = (50, 50, 0, 0)
        _Rotate3 ("Rotate 3", range(0, 360)) = 0
        [NoScaleOffset] _Tex3 ("Detail 3", 2D) = "white" {}

        [Header(Green)][Toggle] _Toggle2 ("Show detail 2", float) = 0
        _Tiling2 ("Scale (xy) and Offset (zw) 2", vector) = (50, 50, 0, 0)
        _Rotate2 ("Rotate 2", range(0, 360)) = 0
        [NoScaleOffset] _Tex2 ("Detail 2", 2D) = "white" {}

        [Header(Red)][Toggle] _Toggle1 ("Show detail 1", float) = 0
        _Tiling1 ("Scale (xy) and Offset (zw) 1", vector) = (50, 50, 0, 0)
        _Rotate1 ("Rotate 1", range(0, 360)) = 0
        [NoScaleOffset] _Tex1 ("Detail 1", 2D) = "white" {}

        [NoScaleOffset] _Mask ("Mask", 2D) = "gray" {}

        _Tiling ("Base Scale (xy) and Offset (zw)", vector) = (50, 50, 0, 0)
        _Rotate ("Base Rotate", range(0, 360)) = 0
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

        sampler2D _Base, _Mask, _Tex1, _Tex2, _Tex3, _Tex4;

        inline float2 uv_rotate(float2 uv, float angle)
        {
            const float sin_angle = sin(angle);
            const float cos_angle = cos(angle);

            float2 o;
            o.x = cos_angle * uv.x - sin_angle * uv.y;
            o.y = sin_angle * uv.x + cos_angle * uv.y;

            return o;
        }

        struct Input
        {
            float2 uv_Base;
        };
        
        float4 _Tiling, _Tiling1, _Tiling2, _Tiling3, _Tiling4, _Tiling5, _Tiling6;
        float _Rotate, _Rotate1, _Rotate2, _Rotate3, _Rotate4, _Rotate5, _Rotate6;

        void surf(Input IN, inout SurfaceOutput o)
        {
            float2 uv = uv_rotate(IN.uv_Base * _Tiling.xy + _Tiling.zw, radians(_Rotate));
            fixed4 c = tex2D(_Base, uv);
            fixed4 mask = tex2D(_Mask, IN.uv_Base);
            #if _TOGGLE1_ON
            float2 uv1 = uv_rotate(IN.uv_Base * _Tiling1.xy + _Tiling1.zw, radians(_Rotate1));
            c = lerp(c, tex2D(_Tex1, uv1), mask.r);
            #endif
            #if _TOGGLE2_ON
            float2 uv2 = uv_rotate(IN.uv_Base * _Tiling2.xy + _Tiling2.zw, radians(_Rotate2));
            c = lerp(c, tex2D(_Tex2, uv2), mask.g);
            #endif
            #if _TOGGLE3_ON
            float2 uv3 = uv_rotate(IN.uv_Base * _Tiling3.xy + _Tiling3.zw, radians(_Rotate3));
            c = lerp(c, tex2D(_Tex3, uv3), mask.b);
            #endif
            #if _TOGGLE4_ON
            float2 uv4 = uv_rotate(IN.uv_Base * _Tiling4.xy + _Tiling4.zw, radians(_Rotate4));
            c = lerp(c, tex2D(_Tex4, uv4), mask.a);
            #endif

            o.Albedo = c.rgb;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Mobile/VertexLit"
    // CustomEditor "_Game.THIN.Scripts.Editor.ShaderCustomEditors.Terrain4RotatableDetailsShaderGUI"
}