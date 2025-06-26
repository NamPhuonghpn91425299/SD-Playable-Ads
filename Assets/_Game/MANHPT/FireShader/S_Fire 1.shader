Shader "Unlit/S_Fire_Ver2"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _FireMask("Fire Mask", 2D) = "bump"{}
        _Color1("Color 1", Color) = (1,1,1,1)
        [HDR]_Color2("Color 2", Color) = (1,1,1,1)
        _Strength("Strength", float) = 0.5
        _FireSpeed1("FireSpeed1", float) = 0.1
        _FireSpeed2("FireSpeed2", float) = 0.15
        _UVSpeed1("UVSpeed1", float) = 1
        _UVSpeed2("UVSpeed2", float) = 0.5
        _DetailScale1("_DetailScale1", float) = 3.5
        _DetailScale2("_DetailScale2", float) = 3
        _FlameScale1("_FlameScale1", float) = 0.55
        _FlameScale2("_FlameScale2", float) = 0.35

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
            float _Strength;
            float _FireSpeed1;
            float _FireSpeed2;
            float _UVSpeed1;
            float _UVSpeed2;
            float _DetailScale1;
            float _DetailScale2;
            float _FlameScale1;
            float _FlameScale2;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv1 = i.uv * float2(_DetailScale1, 0.5) + float2(0, -_Time.y * _UVSpeed1);
                fixed4 tex1 = tex2D(_NoiseTex, uv1);
                fixed4 col1 = tex1 * 0.3;
                float mask1 = Remap(i.uv.y, float2(0,1), float2(1, 0.2)) * 0.5;
                float mask2 = Remap(i.uv.y, float2(0, 0.3), float2(0.7, 0)) * 0.5;
                float lowerMask = (mask1 + mask2) * _Strength;
                float2 uv2 = i.uv * float2(_DetailScale2,1) + float2(0, -_Time.y * _UVSpeed2);
                fixed4 col2 = tex2D(_NoiseTex, uv2) * 0.5;
                fixed4 col3 = pow(saturate(col1 + lowerMask + col2), 2);
                float fireMask1 = tex2D(_FireMask, i.uv * float2(_FlameScale1,1) + float2(_Time.y * _FireSpeed1, 0)).a;
                float fireMask2 = tex2D(_FireMask, i.uv * float2(_FlameScale2,1) + float2(-_Time.y * _FireSpeed2, 0)).a;
                float fireMask = saturate((fireMask1 + fireMask2) / 2);
                float mask4 = saturate(Remap(1 - abs(i.uv.x - 0.5), float2(0.5,0.7), float2(0,1)));
                float finalAlpha = saturate(col3) * fireMask * mask4;
                fixed3 finalCol = lerp(_Color1, _Color2, finalAlpha) * finalAlpha;
                return fixed4(finalCol, finalAlpha);
            }
            ENDCG
        }
    }
}