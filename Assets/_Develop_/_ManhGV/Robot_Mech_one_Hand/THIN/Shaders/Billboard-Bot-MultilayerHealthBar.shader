Shader "Horus/Bot/Multilayer Health Bar"
{
    Properties
    {
        _Fill ("Fill", range(0,1)) = 1
        _Range1 ("Layer 1 Range", float) = 1
        _Range2 ("Layer 2 Range", float) = 0
        _Range3 ("Layer 3 Range", float) = 0
        _Range4 ("Layer 4 Range", float) = 0
        _Range5 ("Layer 5 Range", float) = 0
        _BgColor ("Background Color", color) = (0, 0, 0, 1)
        _Color1 ("Layer 1 Color", color) = (1, 0, 0, 1)
        _Color2 ("Layer 2 Color", color) = (1, 1, 0, 1)
        _Color3 ("Layer 3 Color", color) = (0, 1, 0, 1)
        _Color4 ("Layer 4 Color", color) = (0, 1, 1, 1)
        _Color5 ("Layer 5 Color", color) = (0, 0, 1, 1)
        [Toggle] _ZTest ("Coverable", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }
        LOD 100

        Pass
        {
            ZTest [_ZTest]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Assets/_Develop_/_ThanhNT/Shader/MyLib.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 bg_color : TEXCOORD1;
                fixed4 color : TEXCOORD2;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
            UNITY_DEFINE_INSTANCED_PROP(float, _Range1)
            UNITY_DEFINE_INSTANCED_PROP(float, _Range2)
            UNITY_DEFINE_INSTANCED_PROP(float, _Range3)
            UNITY_DEFINE_INSTANCED_PROP(float, _Range4)
            UNITY_DEFINE_INSTANCED_PROP(float, _Range5)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color1)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color2)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color3)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color4)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color5)
            UNITY_INSTANCING_BUFFER_END(Props)
            
            fixed4 _BgColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);

                float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);
                const float range1 = UNITY_ACCESS_INSTANCED_PROP(Props, _Range1);
                const float range2 = UNITY_ACCESS_INSTANCED_PROP(Props, _Range2);
                const float range3 = UNITY_ACCESS_INSTANCED_PROP(Props, _Range3);
                const float range4 = UNITY_ACCESS_INSTANCED_PROP(Props, _Range4);
                const float range5 = UNITY_ACCESS_INSTANCED_PROP(Props, _Range5);

                const fixed4 color1 = UNITY_ACCESS_INSTANCED_PROP(Props, _Color1);
                const fixed4 color2 = UNITY_ACCESS_INSTANCED_PROP(Props, _Color2);
                const fixed4 color3 = UNITY_ACCESS_INSTANCED_PROP(Props, _Color3);
                const fixed4 color4 = UNITY_ACCESS_INSTANCED_PROP(Props, _Color4);
                const fixed4 color5 = UNITY_ACCESS_INSTANCED_PROP(Props, _Color5);
                
                const float sum2 = range1 + range2;
                const float sum3 = sum2 + range3;
                const float sum4 = sum3 + range4;
                fill *= (sum4 + range5);
                
                o.bg_color = _BgColor;
                o.color = color1;
                o.uv = v.uv;

                if (fill > sum4)
                {
                    o.bg_color = color4;
                    o.color = color5;
                    o.uv.x -= (fill - sum4) / range5;
                }
                else if (fill > sum3)
                {
                    o.bg_color = color3;
                    o.color = color4;
                    o.uv.x -= (fill - sum3) / range4;
                }
                else if (fill > sum2)
                {
                    o.bg_color = color2;
                    o.color = color3;
                    o.uv.x -= (fill - sum2) / range3;
                }
                else if (fill > range1)
                {
                    o.bg_color = color1;
                    o.color = color2;
                    o.uv.x -= (fill - range1) / range2;
                }
                else if (range1 > 0)
                {
                    o.uv.x -= fill / range1;
                }

                o.vertex = billboard(v.vertex, SCALE_X, SCALE_Y);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return lerp(i.bg_color, i.color, i.uv.x < 0);
            }
            ENDCG
        }
    }
}
