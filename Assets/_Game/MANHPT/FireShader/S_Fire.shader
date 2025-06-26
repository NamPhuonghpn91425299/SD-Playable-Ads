Shader "Unlit/S_Fire"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _FireMask("Fire Mask", 2D) = "bump"{}
        _Color1("Color 1", Color) = (1,1,1,1)
        [HDR]_Color2("Color 2", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Zwrite off
        LOD 100

        Pass
        {
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float Remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            sampler2D _NoiseTex;
            sampler2D _FireMask;
            float4 _Color1;
            float4 _Color2;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv1 = i.uv * float2(3.5, 0.5) + float2(0, -_Time.y);
                fixed4 tex1 = tex2D(_NoiseTex, uv1);
                fixed4 col1 = tex1 * 0.3;
                float mask1 = Remap(i.uv.y, float2(0,1), float2(1, 0.2)) * 0.5;
                float mask2 = Remap(i.uv.y, float2(0, 0.3), float2(0.7, 0)) * 0.3;
                float lowerMask = mask1 + mask2;
                float2 uv2 = i.uv * float2(3,1) + float2(0, -_Time.y * 0.5);
                fixed4 col2 = tex2D(_NoiseTex, uv2) * 0.5;
                fixed4 col3 = pow(saturate(col1 + lowerMask + col2), 2);
                float fireMask = tex2D(_FireMask, i.uv).a;
                fixed4 col4 = col3 * fireMask;
                float mask4 = (1-i.uv.y) >= 0.01 ? 1 : 0;
                float finalAlpha = mask4 * col4;
                fixed4 finalCol = lerp(_Color1, _Color2, col4) * col4;
                return fixed4(finalCol.xyz, finalAlpha);
            }
            ENDCG
        }
    }
}