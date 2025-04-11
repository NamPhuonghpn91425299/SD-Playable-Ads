
Shader "Custom/OptimizedBlurDistortMasked" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _BumpAmt("Distortion Amount", Range(0, 1)) = 0.1 // Adjusted range for finer control
        // _MainTex: RGB for Tint, Alpha for Blur Mask
        _MainTex("Tint Color (RGB) & Blur Mask (A)", 2D) = "white" {}
        _BumpMap("Normal Map (RG)", 2D) = "bump" {}
        _Size("Blur Size", Range(0, 10)) = 1 // Blur radius in pixels (approx)
    }

    SubShader {
        GrabPass { "_BackgroundTexture" }

        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent" // Correct RenderType
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord: TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                float4 uvgrab : TEXCOORD0; // xy = screen pos / w, zw = original z & w
                float2 uvbump : TEXCOORD1;
                float2 uvmain : TEXCOORD2;
            };

            sampler2D _BackgroundTexture;
            float4 _BackgroundTexture_TexelSize;
            sampler2D _BumpMap;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BumpMap_ST;

            fixed4 _Color;
            float _BumpAmt;
            float _Size;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvgrab = ComputeGrabScreenPos(o.vertex);
                o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap);
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                // 1. Calculate Distortion Offset
                half2 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rg;
                float2 distortionOffset = bump * _BumpAmt * _BackgroundTexture_TexelSize.xy * i.uvgrab.w; // Perspective correct distortion

                // 2. Calculate the central UV coordinate (distorted)
                // Need to perspective divide here for calculations based on texel size
                float2 centerUV = (i.uvgrab.xy / i.uvgrab.w) + distortionOffset;

                // 3. Sample the ORIGINAL background color at the distorted position
                // We need the original z and w for the projection
                float4 originalUVProj = float4(centerUV, i.uvgrab.z, i.uvgrab.w);
                half4 originalDistortedColor = tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(originalUVProj));

                // 4. Calculate the BLURRED background color centered around the distorted position
                // (Using 9-tap box blur as example)
                half4 sum = half4(0,0,0,0);
                float2 offsets[9] = {
                    float2(-1,-1), float2(0,-1), float2(1,-1),
                    float2(-1, 0), float2(0, 0), float2(1, 0),
                    float2(-1, 1), float2(0, 1), float2(1, 1)
                };

                [unroll] // Small loop, unrolling might help
                for (int j = 0; j < 9; j++) {
                    // Calculate sample UV based on centerUV, offset, size, and texel size
                    float2 sampleOffset = offsets[j] * _BackgroundTexture_TexelSize.xy * _Size;
                    float2 sampleUV = centerUV + sampleOffset;
                    // Reconstruct projection coordinates for tex2Dproj
                    float4 sampleUVProj = float4(sampleUV, i.uvgrab.z, i.uvgrab.w);
                    sum += tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(sampleUVProj));
                }
                half4 blurredColor = sum / 9.0;

                // 5. Get Tint color AND the Blur Mask value from _MainTex's alpha
                half4 mainTexSample = tex2D(_MainTex, i.uvmain);
                half blurMask = mainTexSample.a; // Alpha channel controls blur amount
                half4 tint = mainTexSample * _Color; // Combine texture color with main color for tint

                // 6. Interpolate between original and blurred background based on the mask
                // lerp(a, b, x): result is a when x=0, b when x=1
                half4 finalBgColor = lerp(originalDistortedColor, blurredColor, blurMask);

                // 7. Apply Tint color to the result and use combined alpha for final transparency
                // Modulate the background by the tint RGB, use the final tint alpha.
                return half4(finalBgColor.rgb * tint.rgb, tint.a);
            }
            ENDCG
        }
    }
    Fallback "Transparent/VertexLit"
}

