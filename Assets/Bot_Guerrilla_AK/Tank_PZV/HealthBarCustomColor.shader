Shader "UI/HealthBarCustomColor" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Fill ("Fill", float) = 0
    }
    SubShader {
        Tags { "Queue"="Overlay" }
        LOD 100

        Pass {
            ZTest Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing


            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // If you need instance data in the fragment shader, uncomment next line
                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _Color;
            
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                // If you need instance data in the fragment shader, uncomment next line

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {

                //UNITY_SETUP_INSTANCE_ID(i);
                // Could access instanced data here too like:
                // UNITY_ACCESS_INSTANCED_PROP(Props, _Foo);
                // But, remember to uncomment lines flagged above

                float4 defaultColor = tex2D(_MainTex, float2(0.75, i.uv.y));
                float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);
                float4 result = lerp(defaultColor, _Color, i.uv.x < fill);
                
                return result;// + _Color;
            }
            ENDCG
        }
    }
}