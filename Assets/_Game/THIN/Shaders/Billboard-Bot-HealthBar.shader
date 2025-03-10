Shader "Horus/Bot/Health Bar"
{
    Properties
    {
        _Fill ("Fill", range(0,1)) = 1
        _Color ("Health Color", Color) = (0, 1, 0, 1)
        _BgColor ("Background Color", Color) = (0, 0, 0, 1)
        [Toggle] _ZTest ("Coverable", float) = 0
        [Toggle] _AutoSize ("Size base on Game Object's scale", float) = 1
        _Size ("Fixed Size", vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
        }
        LOD 100

        Pass
        {
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // #pragma multi_compile_instancing
            #pragma shader_feature _AUTOSIZE_ON
            #include "MyLib.cginc"

            struct mesh_data
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // If you need instance data in the fragment shader, uncomment next line
                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            fixed4 _Color;
            fixed4 _BgColor;
            float _Fill;
            #if _AUTOSIZE_ON
            #else
            float4 _Size;
            #endif

            Interpolators vert(mesh_data v)
            {
                Interpolators o;

                #if _AUTOSIZE_ON
                o.vertex = billboard(v.vertex, SCALE_X, SCALE_Y);
                #else
                o.vertex = billboard(v.vertex, _Size.x, _Size.y);
                #endif
                o.uv = v.uv;
                o.uv.x -= _Fill;
                return o;
            }

            fixed4 frag(Interpolators i) : SV_Target
            {
                return lerp(_BgColor, _Color, i.uv.x < 0);
            }
            ENDCG
        }
    }
}