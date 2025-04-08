Shader "Custom/Behind"
{
    Properties
    {
        _Stencil ("Stencil", range(0, 255)) = 1
        _BehindColor ("Behind Color", Color) = (1, 0, 0, 0.5)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent+7"
        }

        Pass
        {
            Name "Behind"

            ZWrite Off
            ZTest Greater
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref [_Stencil]
                Comp Greater
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _BehindColor;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return _BehindColor;
            }
            ENDCG
        }
    }
}