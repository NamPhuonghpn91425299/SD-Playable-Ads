Shader "Custom/ScopeEffectShader"
{
    Properties
    {
        _MainTex ("Texture (TGA)", 2D) = "white" {} // Texture đầu vào (ảnh TGA của bạn)
        _ScopeCenter ("Scope Center UV (X, Y)", Vector) = (0.5, 0.5, 0, 0) // Tâm scope (0.5, 0.5 là giữa)
        _ScopeRadius ("Scope Radius (UV units)", Range(0, 0.71)) = 0.4 // Bán kính scope (tối đa khoảng 0.707 để chạm góc)
        _EdgeSoftness ("Edge Softness", Range(0.0, 0.1)) = 0.01 // Độ mềm của viền scope
        _OutsideColor ("Outside Color", Color) = (0, 0, 0, 1) // Màu bên ngoài (mặc định là đen)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" } // Cần thiết cho blending/alpha
        LOD 100
        ZWrite Off // Thường tắt ZWrite cho UI/Sprite hoặc hiệu ứng trong suốt
        Blend SrcAlpha OneMinusSrcAlpha // Chế độ blend alpha thông thường

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog // Không cần fog cho UI/2D thường

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR; // Thêm color để hoạt động tốt với UI Image/Sprite Tint
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1) // Không cần fog
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR; // Truyền color qua vertex shader
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; // Hỗ trợ Tiling/Offset

            float2 _ScopeCenter;
            float _ScopeRadius;
            float _EdgeSoftness;
            fixed4 _OutsideColor; // Sử dụng màu có thể tùy chỉnh

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // Áp dụng Tiling/Offset
                // UNITY_TRANSFER_FOG(o,o.vertex); // Không cần fog
                o.color = v.color; // Truyền color gốc
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lấy màu gốc từ texture
                fixed4 originalColor = tex2D(_MainTex, i.uv) * i.color; // Nhân với vertex color (quan trọng cho UI/Sprite Tint)

                // Tính khoảng cách từ pixel hiện tại tới tâm scope (trong không gian UV)
                // Sử dụng khoảng cách bình phương để tránh tính sqrt đắt đỏ
                float distSq = dot(i.uv - _ScopeCenter.xy, i.uv - _ScopeCenter.xy);
                float radiusSq = _ScopeRadius * _ScopeRadius;

                // Tính toán alpha mask dựa trên khoảng cách và độ mềm viền
                // smoothstep(a, b, x): trả về 0 nếu x < a, 1 nếu x > b, và chuyển mượt giữa 0 và 1 khi a <= x <= b
                float edgeInner = radiusSq;
                float edgeOuter = radiusSq + _EdgeSoftness * _ScopeRadius; // Điều chỉnh độ mềm tương đối với bán kính

                // alphaInside = 1 khi ở trong hoặc trên viền mềm, = 0 khi ở xa bên ngoài
                float alphaInside = 1.0 - smoothstep(edgeInner, edgeOuter, distSq);

                // Kết hợp màu gốc và màu bên ngoài dựa trên alpha
                // Nếu alphaInside = 1 (bên trong), kết quả là originalColor
                // Nếu alphaInside = 0 (bên ngoài), kết quả là _OutsideColor (với alpha của màu gốc)
                // Sử dụng alpha của màu gốc cho màu bên ngoài để giữ độ trong suốt gốc nếu có
                fixed4 finalColor = lerp(fixed4(_OutsideColor.rgb, originalColor.a * _OutsideColor.a), originalColor, alphaInside);

                // Nếu muốn bên ngoài hoàn toàn trong suốt thay vì màu đen, bạn có thể làm:
                // finalColor.a = originalColor.a * alphaInside;

                // Đảm bảo kết quả cuối cùng không vượt quá alpha của màu gốc (quan trọng khi dùng Blend)
                finalColor.a *= i.color.a; // Áp dụng alpha từ tint color

                return finalColor;
            }
            ENDCG
        }
    }
}
