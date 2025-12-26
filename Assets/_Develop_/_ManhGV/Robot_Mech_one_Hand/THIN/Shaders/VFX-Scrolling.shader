Shader "Horus/VFX/Scrolling"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Speed ("Speed", float) = 1
        [Toggle] _Alpha ("Use Alpha", float) = 0
        _Cutout ("Cut out", range(0,1)) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", int) = 1
    }

    Category
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"
        }
        Blend [_SrcBlend] [_DstBlend]
        Cull Off Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles
                #pragma shader_feature _ALPHA_ON

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _TintColor;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                float4 _MainTex_ST;
                float _Speed, _Strength;
                fixed _Cutout;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.uv.x = o.uv.x + _Speed * _Time.x;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    float opacity = 0;
                    #if _ALPHA_ON
                    opacity = col.a;
                    #else
                    opacity = col.r;
                    #endif
                    clip(opacity - _Cutout);

                    return i.color * opacity;
                }
                ENDCG
            }
        }
    }
}