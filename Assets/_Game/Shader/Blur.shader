// // Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// // Upgrade NOTE: Combined passes for efficiency, corrected blur logic approach.
// // Upgrade NOTE: Added alpha masking for blur effect based on _MainTex alpha.

// Shader "Custom/OptimizedBlurDistortMasked" {
//     Properties{
//         _Color("Main Color", Color) = (1,1,1,1)
//         _BumpAmt("Distortion Amount", Range(0, 1)) = 0.1 // Adjusted range for finer control
//         // _MainTex: RGB for Tint, Alpha for Blur Mask
//         _MainTex("Tint Color (RGB) & Blur Mask (A)", 2D) = "white" {}
//         _BumpMap("Normal Map (RG)", 2D) = "bump" {}
//         _Size("Blur Size", Range(0, 10)) = 1 // Blur radius in pixels (approx)
//     }

//     SubShader {
//         GrabPass { "_BackgroundTexture" }

//         Tags {
//             "Queue" = "Transparent"
//             "RenderType" = "Transparent" // Correct RenderType
//             "IgnoreProjector" = "True"
//         }
//         LOD 100

//         Pass {
//             Blend SrcAlpha OneMinusSrcAlpha
//             ZWrite Off

//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma target 3.0
//             #include "UnityCG.cginc"

//             struct appdata_t {
//                 float4 vertex : POSITION;
//                 float2 texcoord: TEXCOORD0;
//             };

//             struct v2f {
//                 float4 vertex : POSITION;
//                 float4 uvgrab : TEXCOORD0; // xy = screen pos / w, zw = original z & w
//                 float2 uvbump : TEXCOORD1;
//                 float2 uvmain : TEXCOORD2;
//             };

//             sampler2D _BackgroundTexture;
//             float4 _BackgroundTexture_TexelSize;
//             sampler2D _BumpMap;
//             sampler2D _MainTex;
//             float4 _MainTex_ST;
//             float4 _BumpMap_ST;

//             fixed4 _Color;
//             float _BumpAmt;
//             float _Size;

//             v2f vert(appdata_t v) {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.uvgrab = ComputeGrabScreenPos(o.vertex);
//                 o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
//                 o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap);
//                 return o;
//             }

//             half4 frag(v2f i) : SV_Target {
//                 // 1. Calculate Distortion Offset
//                 half2 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rg;
//                 float2 distortionOffset = bump * _BumpAmt * _BackgroundTexture_TexelSize.xy * i.uvgrab.w; // Perspective correct distortion

//                 // 2. Calculate the central UV coordinate (distorted)
//                 // Need to perspective divide here for calculations based on texel size
//                 float2 centerUV = (i.uvgrab.xy / i.uvgrab.w) + distortionOffset;

//                 // 3. Sample the ORIGINAL background color at the distorted position
//                 // We need the original z and w for the projection
//                 float4 originalUVProj = float4(centerUV, i.uvgrab.z, i.uvgrab.w);
//                 half4 originalDistortedColor = tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(originalUVProj));

//                 // 4. Calculate the BLURRED background color centered around the distorted position
//                 // (Using 9-tap box blur as example)
//                 half4 sum = half4(0,0,0,0);
//                 float2 offsets[9] = {
//                     float2(-1,-1), float2(0,-1), float2(1,-1),
//                     float2(-1, 0), float2(0, 0), float2(1, 0),
//                     float2(-1, 1), float2(0, 1), float2(1, 1)
//                 };

//                 [unroll] // Small loop, unrolling might help
//                 for (int j = 0; j < 9; j++) {
//                     // Calculate sample UV based on centerUV, offset, size, and texel size
//                     float2 sampleOffset = offsets[j] * _BackgroundTexture_TexelSize.xy * _Size;
//                     float2 sampleUV = centerUV + sampleOffset;
//                     // Reconstruct projection coordinates for tex2Dproj
//                     float4 sampleUVProj = float4(sampleUV, i.uvgrab.z, i.uvgrab.w);
//                     sum += tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(sampleUVProj));
//                 }
//                 half4 blurredColor = sum / 9.0;

//                 // 5. Get Tint color AND the Blur Mask value from _MainTex's alpha
//                 half4 mainTexSample = tex2D(_MainTex, i.uvmain);
//                 half blurMask = mainTexSample.a; // Alpha channel controls blur amount
//                 half4 tint = mainTexSample * _Color; // Combine texture color with main color for tint

//                 // 6. Interpolate between original and blurred background based on the mask
//                 // lerp(a, b, x): result is a when x=0, b when x=1
//                 half4 finalBgColor = lerp(originalDistortedColor, blurredColor, blurMask);

//                 // 7. Apply Tint color to the result and use combined alpha for final transparency
//                 // Modulate the background by the tint RGB, use the final tint alpha.
//                 return half4(finalBgColor.rgb * tint.rgb, tint.a);
//             }
//             ENDCG
//         }
//     }
//     Fallback "Transparent/VertexLit"
// }
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: Combined passes, alpha mask for blur, corrected final alpha.
// Upgrade NOTE: Added clear central circle based on UV distance.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: Combined passes, alpha mask for blur, corrected final alpha.
// Upgrade NOTE: Added clear central circle based on UV distance.
// Upgrade NOTE: Fixed aspect ratio distortion for the clear circle.

// Shader sử dụng Alpha của _MainTex để điều khiển cả Blur và Cutout/Overlay.
// Phù hợp cho hiệu ứng Scope nơi hình dạng mask được định nghĩa bởi texture.
Shader "Custom/ScopeBlurMaskedByTextureOverlay" {
    Properties{
        _Color("Overlay Tint / Overall Alpha", Color) = (1,1,1,1) // Tint cho phần overlay (Alpha=1), Alpha tổng thể
        _BumpAmt("Distortion Amount (Background)", Range(0, 1)) = 0.05 // Làm méo hậu cảnh
        // _MainTex: RGB là hình ảnh scope (viền, crosshair), Alpha là mask (0=trong suốt/không blur, 1=đục/blur)
        _MainTex("Scope Visual (RGB) & Mask (A)", 2D) = "white" {}
        _BumpMap("Distortion Map (Background)", 2D) = "bump" {} // Optional normal map for distortion
        _Size("Blur Size (Behind Overlay)", Range(0, 20)) = 5 // Độ mạnh blur phía sau phần đục (Alpha=1)
    }

    SubShader {
        GrabPass { "_BackgroundTexture" } // Chụp ảnh hậu cảnh một lần

        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending
            ZWrite Off // Không ghi vào depth buffer

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
                float4 vertex : SV_POSITION; // Clip space position
                float4 uvgrab : TEXCOORD0; // Screen UVs for grab pass
                float2 uvmain : TEXCOORD1; // UVs for _MainTex (scope texture)
                float2 uvbump : TEXCOORD2; // UVs for _BumpMap
            };

            // Textures and Samplers
            sampler2D _BackgroundTexture;
            float4 _BackgroundTexture_TexelSize; // Kích thước pixel của grab texture
            sampler2D _BumpMap;
            sampler2D _MainTex;
            float4 _MainTex_ST; // Tiling and offset for _MainTex
            float4 _BumpMap_ST; // Tiling and offset for _BumpMap

            // Properties
            fixed4 _Color;
            float _BumpAmt;
            float _Size;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvgrab = ComputeGrabScreenPos(o.vertex); // Tính UV cho grab pass
                o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex); // Transform UV cho scope texture
                o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap); // Transform UV cho bump map (giả sử dùng chung)
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                // 1. Tính toán Distortion Offset (nếu có) và UV trung tâm cho background sampling
                half2 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rg;
                float2 distortionOffset = bump * _BumpAmt * _BackgroundTexture_TexelSize.xy * i.uvgrab.w; // Perspective correct distortion
                float2 centerScreenUV = (i.uvgrab.xy / i.uvgrab.w) + distortionOffset; // UV màn hình đã bị distort

                // 2. Lấy mẫu màu Background GỐC tại vị trí đã distort
                float4 originalUVProj = float4(centerScreenUV, i.uvgrab.z, i.uvgrab.w);
                half4 originalBgColor = tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(originalUVProj));

                // 3. Tính toán màu Background ĐÃ BLUR tại vị trí đã distort
                // (Sử dụng box blur 9-tap làm ví dụ)
                half4 blurredBgColor;
                half4 sum = half4(0,0,0,0);
                float2 offsets[9] = {
                    float2(-1,-1), float2(0,-1), float2(1,-1),
                    float2(-1, 0), float2(0, 0), float2(1, 0),
                    float2(-1, 1), float2(0, 1), float2(1, 1)
                };
                [unroll] // Có thể giúp với vòng lặp nhỏ
                for (int j = 0; j < 9; j++) {
                    float2 sampleOffset = offsets[j] * _BackgroundTexture_TexelSize.xy * _Size; // Offset theo pixel * size
                    float4 sampleUVProj = float4(centerScreenUV + sampleOffset, i.uvgrab.z, i.uvgrab.w); // Tọa độ lấy mẫu blur
                    sum += tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(sampleUVProj)); // Cộng dồn màu
                }
                blurredBgColor = sum / 9.0h; // Tính trung bình

                // 4. Lấy mẫu Texture Scope (RGB là màu sắc, A là mask)
                half4 scopeSample = tex2D(_MainTex, i.uvmain);
                half maskValue = scopeSample.a; // Giá trị Alpha (0=trong suốt/không blur, 1=đục/blur)

                // 5. Chọn màu Background dựa trên Mask Alpha
                // lerp(a, b, t): chọn a khi t=0, chọn b khi t=1
                // Chọn original khi mask=0, chọn blurred khi mask=1
                half4 chosenBgColor = lerp(originalBgColor, blurredBgColor, maskValue);

                // 6. Tính toán màu RGB và Alpha cuối cùng (Kiểu Overlay)
                // Màu RGB của phần overlay (phần đục của scope) đã được tint
                half3 tintedOverlayRGB = scopeSample.rgb * _Color.rgb;

                // Hòa trộn giữa background đã chọn (bước 5) và màu overlay (bước 6) dựa trên mask alpha
                // Khi mask=0, kết quả là chosenBgColor (là originalBgColor) -> nhìn xuyên qua
                // Khi mask=1, kết quả là tintedOverlayRGB -> thấy viền scope
                half3 finalRGB = lerp(chosenBgColor.rgb, tintedOverlayRGB, maskValue);

                // Alpha cuối cùng được điều khiển bởi mask alpha và alpha tổng thể của _Color
                // Khi mask=0, alpha=0 -> trong suốt
                // Khi mask=1, alpha=_Color.a -> đục (theo _Color.a)
                half finalAlpha = maskValue * _Color.a;

                // Trả về màu và alpha cuối cùng
                return half4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
    Fallback "Transparent/VertexLit" // Fallback cho phần cứng cũ
}