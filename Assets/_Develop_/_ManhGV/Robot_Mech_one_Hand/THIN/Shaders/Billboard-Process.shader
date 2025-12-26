Shader "Horus/Billboard/Circle Process"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0, 0, 1)
        _Fill ("Fill", Range(0, 1)) = 1
        _StartAngle("Start Angle", Range(0, 360)) = 0
        [Toggle] _Clockwise("Clockwise", Float) = 0
        _Cutoff ("Cutoff", Float) = 0.001
        [Toggle] _ZTest ("Coverable", float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            ZTest [_ZTest]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _CLOCKWISE_ON

            #include "Assets/_Develop_/_ThanhNT/Shader/MyLib.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _Cutoff;
            float _Fill;
            float _StartAngle;
            float _Direction;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = billboard(v.vertex, SCALE_X, SCALE_Y);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - _Cutoff);
                
                float x = i.uv.x * 2 - 1;
                float y = i.uv.y * 2 - 1;
                float angle = degrees(atan2(y, x));
                angle = wrap(angle - _StartAngle + 180, -180, 180); 
                float fill = remap(_Fill, 0, 1, -180, 180);
                #if _CLOCKWISE_ON
                clip(fill - angle);
                #else
                clip(fill + angle);
                #endif
                return col;
            }
            ENDCG
        }
    }
}
