Shader "Unlit/GlowEye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0, 1, 0, 1)
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
//        _DissolveNoiseTex("_Dissolve Noise Tex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 dissolveUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            // sampler2D _DissolveNoiseTex;
            // float4 _DissolveNoiseTex_ST;
            float _DissolveAmount;
            

            float Remap1(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.dissolveUV = TRANSFORM_TEX(v.uv, _DissolveNoiseTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half glow = tex2D(_MainTex, i.uv).r;
                //float dissolve = tex2D(_DissolveNoiseTex, i.dissolveUV).r;
                //dissolve = dissolve + 0.1 - dissolve;
                float finalAlpha = lerp(glow, 0, _DissolveAmount);
                return half4(_Color.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}
