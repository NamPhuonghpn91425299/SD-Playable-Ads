Shader "Horus/Billboard/Texture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", float) = 1
        [Toggle] _AutoSize ("Size base on Game Object's scale", float) = 0
        _Size ("Fixed Size", vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        ZTest [_ZTest]
        ZWrite off

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _AUTOSIZE_ON

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
            #if _AUTOSIZE_ON
            #else
            float4 _Size;
            #endif

            v2f vert(appdata v)
            {
                v2f o;
                #if _AUTOSIZE_ON
                o.vertex = billboard(v.vertex, SCALE_X, SCALE_Y);
                #else
                o.vertex = billboard(v.vertex, _Size.x, _Size.y);
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}