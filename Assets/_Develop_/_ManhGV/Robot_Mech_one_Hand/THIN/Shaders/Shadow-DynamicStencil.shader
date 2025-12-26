Shader "Horus/Shadow/Stencil"
{
    Properties
    {
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.5)
        _Stencil ("Stencil Value", Float) = 200
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100


        Pass
        {
            ZWrite Off
            Cull Back
            ColorMask 0
            
            Stencil 
            {
                Ref [_Stencil]
                Comp GEqual
                Pass Replace
            }
        }
        
        Pass
        {
            Cull Front
            ColorMask 0
            
            Stencil 
            {
                Ref [_Stencil]
                Comp Equal
                Pass Zero
            }
        }
        
        Pass 
        {
            Blend SrcAlpha OneMinusSrcAlpha
            
            Stencil 
            {
                Ref [_Stencil]
                Comp Equal
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            fixed4 _ShadowColor;
            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _ShadowColor;
            }
            ENDCG
        }
    }
}
