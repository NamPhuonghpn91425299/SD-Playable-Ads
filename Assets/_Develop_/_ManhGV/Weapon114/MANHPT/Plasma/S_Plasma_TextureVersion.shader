Shader "Unlit/S_Plasma_TextureVersion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector]_ScaleUV("_ScaleUV", float) = 1
        _MultiplyMask("Multiply Mask", 2D) = "white"{}
        _MultiplyMaskPower("Multiply Power", Range(1,10)) = 1
        _Clip("Clip", Range(0,1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite off
        LOD 100

        Pass{
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma editor_sync_compilation
            #include "UnityCG.cginc"

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

            sampler2D _MultiplyMask;
            float _MultiplyMaskPower;
            float _Clip;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = pow(tex2D(_MultiplyMask, i.uv), _MultiplyMaskPower);
                clip(col.a - _Clip);
                return col;
            }

            ENDCG
        }

        Pass
        {
            blend SrcAlpha one
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScaleUV;
            float _Clip;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  
                o.uv.x = (o.uv.x - 0.5) * _ScaleUV * 2 + 0.5;
                o.uv.x -= _Time.y * 2 * (1 + _ScaleUV);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _Clip);
                return col;
            }
            ENDCG
        }
    }
}
