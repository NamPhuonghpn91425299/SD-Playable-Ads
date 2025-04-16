Shader "Horus/Decoration"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Normal("Normal Map", 2D) = "bump" {}
        _DecorGlossiness("Decor Smoothness", Range(0, 1)) = 0.5
        _DecorMetallic("Decor Metallic", Range(0,1)) = 0.0
        _Decor("Decoration", 2D) = "white" {}
        _Mask("Decoration Mask", 2D) = "black" {}
        _MulStrength("Multiply Strength", Range(-10, 10)) = 1.0
        [Enum(Multiply, 1, Blend, 0)] _MulOrBlend("Color Mix Mode", Float) = 1
        _Opacity("Opacity", Range(0,1)) = 1.0

        // Outline properties
        _BlendMode("Blend Mode", Int) = 1
        _MainColor("Main Color", Color)=(1,0,0,1)
        _OutlineColor("Outline Color", Color)=(1,1,0,1)
        _OutlineSize("Outline Size", float) = 0.01
        _ChangeColorSpeed("Change Color Speed", float) = 5
        _MinAlpha("Min Alpha", Range(0,1)) = 0
        _MaxAlpha("Max Alpha", Range(0,1)) = .3
        _TurnOnOutline("Turn On Outline", Range(0,1)) = 1
        _TurnOnSilhouette("Turn On Silhouette", Range(0,1)) = 1
        _Stencil ("Stencil", float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normal;
        sampler2D _Decor;
        sampler2D _Mask;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float2 uv_Decor;
            float2 uv_Mask;
        };

        half4 _Color;
        half _Glossiness;
        half _Metallic;
        half _DecorGlossiness;
        half _DecorMetallic;
        half _Opacity;
        half _MulStrength;
        fixed _MulOrBlend;

        // Outline variable
        fixed4 _MainColor;
        fixed _TurnOnSilhouette;
        float _ChangeColorSpeed;
        fixed _MinAlpha;
        fixed _MaxAlpha;
        int _BlendMode;

        float Remap(float In, float2 InMinMax, float2 OutMinMax)
        {
            return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            c = c * _Color;
            fixed4 decor = tex2D(_Decor, IN.uv_Decor);
            const fixed decor_mask = tex2D(_Mask, IN.uv_Mask).r;
            const half mask = _Opacity * decor_mask * decor.a;

            decor = decor * decor_mask;

            const fixed4 fully_decor = lerp(decor * _MulStrength, c * decor * _MulStrength, _MulOrBlend);
            c = lerp(c, fully_decor, mask);


            fixed4 color;
            fixed4 alpha = 1;
            switch (_BlendMode)
            {
            case 0:
                color = _TurnOnSilhouette > 0
                            ? lerp(_Color, _MainColor,
                                   Remap(sin(_ChangeColorSpeed * _Time.y), float2(-1, 1),
                                         float2(_MinAlpha, _MaxAlpha)))
                            : _Color;
                c *= color;
                break;
            case 1:
                alpha = _TurnOnSilhouette > 0
                            ? Remap(sin(_ChangeColorSpeed * _Time.y), float2(-1, 1),
                                    float2(_MinAlpha, _MaxAlpha))
                            : 0;
                c = c + _MainColor * alpha;
                break;
            case 2:
                alpha = _TurnOnSilhouette > 0
                            ? Remap(sin(_ChangeColorSpeed * _Time.y), float2(-1, 1),
                                    float2(_MinAlpha, _MaxAlpha))
                            : 0;
                c = c * (1 - alpha) + _MainColor * alpha;
                break;
            default: break;
            }

            o.Albedo = c.rgb;

            const half metallic = lerp(_Metallic, _DecorMetallic, mask);
            const half glossiness = lerp(_Glossiness, _DecorGlossiness, mask);

            o.Metallic = metallic;
            o.Smoothness = glossiness;
            o.Alpha = c.a;
            fixed3 normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            o.Normal = normal.rgb;
        }
        ENDCG
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            colormask 0
            Stencil
            {
                ref [_Stencil]
                comp always
                pass replace
            }
        }
        Pass
        {
            Tags
            {
                "Queue"="Overlay"
            }
            Cull off
            Stencil
            {
                ref [_Stencil]
                comp notequal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _TurnOnOutline;

            struct appdata
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float4 clipPos:SV_POSITION;
                float3 normal:TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.normal = v.normal;
                float drawSize = _TurnOnOutline > 0 ? _OutlineSize : 0;
                o.clipPos = UnityObjectToClipPos(v.vertex + v.normal * drawSize);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed alpha = _TurnOnOutline > 0 ? 1 : 0;
                return fixed4(_OutlineColor.xyz, alpha);
            }
            ENDCG
        }
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            Stencil
            {
                ref [_Stencil]
                comp always
                pass replace
            }

            Name "Main"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            half4 _Color;
            sampler2D _Decor;
            sampler2D _Mask;
            half _MulStrength;
            half _Opacity;

            // Outline
            fixed4 _MainColor;
            float _ChangeColorSpeed;
            float _TurnOnSilhouette;
            fixed _MinAlpha;
            fixed _MaxAlpha;
            int _BlendMode;

            struct mesh_data
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_Decor : TEXCOORD2;
                float2 uv_Mask : TEXCOORD3;
            };

            float4 _MainTex_ST;
            float4 _Decor_ST;
            float4 _Mask_ST;
            fixed _MulOrBlend;

            float Remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            Interpolators vert(mesh_data v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_Decor = TRANSFORM_TEX(v.uv, _Decor);
                o.uv_Mask = TRANSFORM_TEX(v.uv, _Mask);
                return o;
            }

            fixed4 frag(Interpolators IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                fixed4 decor = tex2D(_Decor, IN.uv_Decor);
                const fixed decor_mask = tex2D(_Mask, IN.uv_Mask).r;
                const half mask = _Opacity * decor_mask * decor.a;

                decor = decor * decor_mask;

                const fixed4 fully_decor = lerp(decor * _MulStrength, c * decor * _MulStrength, _MulOrBlend);
                c = lerp(c, fully_decor, mask);

                fixed4 color;
                fixed4 alpha = 1;

                switch (_BlendMode)
                {
                case 0:
                    color = _TurnOnSilhouette > 0
                                ? lerp(_Color, _MainColor,
                                       Remap(sin(_ChangeColorSpeed * _Time.y), float2(-1, 1),
                                             float2(_MinAlpha, _MaxAlpha)))
                                : _Color;
                    c = tex2D(_MainTex, IN.uv_MainTex) * color;
                    break;
                case 1:
                    alpha = _TurnOnSilhouette > 0
                                ? Remap(sin(_ChangeColorSpeed * _Time.y), float2(-1, 1),
                                        float2(_MinAlpha, _MaxAlpha))
                                : 0;
                    c = tex2D(_MainTex, IN.uv_MainTex) + _MainColor * alpha;
                    break;
                case 2:
                    alpha = _TurnOnSilhouette > 0
                                ? Remap(sin(_ChangeColorSpeed * _Time.y), float2(-1, 1),
                                        float2(_MinAlpha, _MaxAlpha))
                                : 0;
                    c = tex2D(_MainTex, IN.uv_MainTex) * (1 - alpha) + _MainColor * alpha;
                    break;
                default: break;
                }

                return c;
            }
            ENDCG
        }

        Pass
        {
            Tags
            {
                "Queue"="Overlay"
            }
            Cull off
            Stencil
            {
                ref [_Stencil]
                comp notequal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _TurnOnOutline;

            struct appdata
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float4 clipPos:SV_POSITION;
                float3 normal:TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.normal = v.normal;
                float drawSize = _TurnOnOutline > 0 ? _OutlineSize : 0;
                o.clipPos = UnityObjectToClipPos(v.vertex + v.normal * drawSize);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed alpha = _TurnOnOutline > 0 ? 1 : 0;
                return fixed4(_OutlineColor.xyz, alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}