Shader "Horus/BakeShadow"
{
    Properties
    {
        [Header(Texture)]
        _MainTex ("Texture", 2D) = "white" {}
        //    _MainColor("Texture Color", Color) = (1,1,1,1)

        [TCP2Separator]
        [Header(Shadow Setting)]
        [Toggle] _EnableShadow ("Enable Shadow", Float) = 1
        [Toggle] _ShadowRota ("Rota Shadow", Float) = 0
        _ShadowColor("Shadow Color", Color) = (0,0,0,0.3)
        _ShadowHeight("Shadow Height", Float) = 0
        _LightDirection("Light Direction", Vector) = (0.15, 0.7, 0.3, 0.3)
    }

    Subshader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        Pass
        {
            Name "Texture"
            
            Stencil
            {
                Ref 0
                Comp Always
                Pass IncrWrap
                ZFail Keep
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                //  fixed4 col = tex2D(_MainTex, i.uv)*_MainColor;
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "Shadow"
            
            Tags
            {
                "Queue" = "Transparent+2" "IgnoreProjector" = "True" "RenderType" = "Transparent"
            }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref 0
                Comp Equal
                Pass IncrWrap
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ENABLESHADOW_ON
            #pragma shader_feature _SHADOWROTA_ON
            #include "UnityCG.cginc"

            struct vsOut
            {
                float4 pos : SV_POSITION;
            };

            fixed _ShadowHeight;
            fixed4 _ShadowColor;
            fixed4 _LightDirection;

            vsOut vert(appdata_base v)
            {
                vsOut o;
                #if _ENABLESHADOW_ON
                #if _SHADOWROTA_ON
                v.vertex.y = 0;
                float4 vPosWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = mul(UNITY_MATRIX_VP, float4(vPosWorld.x, vPosWorld.y + _ShadowHeight, vPosWorld.z, 1));
                #else
                        float objectOriginY = mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0) ).y+_ShadowHeight;
                        float4 vPosWorld = mul(unity_ObjectToWorld, v.vertex);
                        float opposite = vPosWorld.y - objectOriginY;
                        float cosTheta = -_LightDirection.y; 
                        float hypotenuse = opposite / cosTheta;
                        float3 vPos = vPosWorld.xyz + (_LightDirection * hypotenuse);
                        o.pos = mul(UNITY_MATRIX_VP, float4(vPos.x, objectOriginY+_ShadowHeight, vPos.z ,1));
                #endif


                #else
                 o.pos=float4(0, 0, 0 ,1);
                #endif
                return o;
            }

            fixed4 frag(vsOut i) : COLOR
            {
                return _ShadowColor;
            }
            ENDCG
        }

    }

    Fallback "VertexLit"
}