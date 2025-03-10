Shader "Horus/Unlit/Texture Detail"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [NoScaleOffset] _Mask ("Mask", 2d) = "black" {}
        _Tex1 ("Detail 1", 2d) = "white" {}
        _Tex2 ("Detail 2", 2d) = "black" {}
        _Mul ("Mask Multiple", range(0, 5)) = 1
        _Pow ("Mask Power", range(0.1, 3)) = 1
        [Toggle] _Test ("Test", float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        // Non-lightmapped
        Pass
        {
            Tags
            {
                "LightMode" = "Vertex"
            }
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _TEST_ON
            #pragma target 2.0
            #include "UnityCG.cginc"
            #pragma multi_compile_fog

            sampler2D _MainTex, _Mask, _Tex1, _Tex2;
            float4 _MainTex_ST, _Tex1_ST, _Tex2_ST;
            float _Mul, _Pow;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
                float3 uv_mask : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv_mask : TEXCOORD2;
                float2 uv_detail : TEXCOORD3;
                float2 uv_detail2 : TEXCOORD4;
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_mask = v.uv_mask;
                o.uv_detail = TRANSFORM_TEX(v.uv, _Tex1);
                o.uv_detail2 = TRANSFORM_TEX(v.uv, _Tex2);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_Mask, i.uv);
                mask = pow(mask * _Mul, _Pow);
                mask.gb = 0;
                fixed4 detail = tex2D(_Tex1, i.uv_detail);
                fixed4 detail2 = tex2D(_Tex2, i.uv_detail2);
                col.rgb = lerp(col.rgb, detail.rgb, mask.r);
                col.rgb = lerp(col.rgb, detail2.rgb, mask.g);
#if _TEST_ON
                col.rgb = mask.rgb;
#endif
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Lightmapped
        Pass
        {
            Tags
            {
                "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #pragma multi_compile_fog
            #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
            #pragma shader_feature _TEST_ON

            // uniforms
            float4 _MainTex_ST, _Tex1_ST, _Tex2_ST;

            // vertex shader input data
            struct appdata
            {
                float3 pos : POSITION;
                float3 uv0 : TEXCOORD0;
                float3 uv1 : TEXCOORD1;
                float3 uv_mask : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // vertex-to-fragment interpolators
            struct v2f
            {
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                #if USING_FOG
                fixed fog : TEXCOORD2;
                #endif
                float2 uv_mask : TEXCOORD3;
                float2 uv_detail : TEXCOORD4;
                float2 uv_detail2 : TEXCOORD5;
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // vertex shader
            v2f vert(appdata IN)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // compute texture coordinates
                o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                o.uv1 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.uv_mask = IN.uv_mask;
                o.uv_detail = TRANSFORM_TEX(IN.uv0, _Tex1);
                o.uv_detail2 = TRANSFORM_TEX(IN.uv0, _Tex2);
                // fog
                #if USING_FOG
                float3 eyePos = UnityObjectToViewPos(float4(IN.pos, 1));
                float fogCoord = length(eyePos.xyz);  // radial fog distance
                UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
                o.fog = saturate(unityFogFactor);
                #endif

                // transform position
                o.pos = UnityObjectToClipPos(IN.pos);
                return o;
            }

            // textures
            sampler2D _MainTex, _Mask, _Tex1, _Tex2;
            float _Mul, _Pow;

            // fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv1.xy);
                fixed4 mask = tex2D(_Mask, i.uv_mask);
                mask = pow(mask * _Mul, _Pow);
                mask.gb = 0;
                fixed4 detail = tex2D(_Tex1, i.uv_detail);
                fixed4 detail2 = tex2D(_Tex2, i.uv_detail2);
                col.rgb = lerp(col.rgb, detail.rgb, mask.r);
                col.rgb = lerp(col.rgb, detail2.rgb, mask.g);
                // Fetch lightmap
                half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv0.xy);
                col.rgb *= DecodeLightmap(bakedColorTex);
                col.a = 1;

#if _TEST_ON
                col.rgb = mask.rgb;
#endif

                // fog
                #if USING_FOG
                col.rgb = lerp(unity_FogColor.rgb, col.rgb, i.fog);
                #endif
                return col;
            }
            ENDCG
        }
    }
}