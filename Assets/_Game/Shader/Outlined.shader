Shader "Custom/IsolatedOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.1)) = 0.03
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        LOD 200

        Pass
        {
            Name "OUTLINE"
            Cull Front       // Đảo mặt, giúp hiển thị outline bao quanh
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineThickness;
            fixed4 _OutlineColor;
            
            v2f vertOutline(appdata v)
            {
                v2f o;
                // Mở rộng vertex theo vector pháp tuyến
                float3 displaced = v.vertex.xyz + v.normal * _OutlineThickness;
                o.pos = UnityObjectToClipPos(float4(displaced, 1));
                return o;
            }
            
            fixed4 fragOutline(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    
    Fallback "Diffuse"
}
