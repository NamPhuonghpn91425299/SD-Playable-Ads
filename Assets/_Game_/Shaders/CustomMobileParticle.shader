// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Horus/Mobile/Particles"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _Boost ("Boost", Range(1, 10)) = 2
        _Clip("Clip", Range(0,1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull ("Cull", Range(0, 1)) = 0
        [Toggle]_ZWrite ("ZWrite", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("ZTest", Range(0, 8)) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcFactor ("Source Factor", float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstFactor ("Destination Factor", float) = 10
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_ST;
    fixed4 _TintColor;
    float _Boost;
    float _Clip;

    struct appdata_t
    {
        float4 position : POSITION;
        float4 texcoord : TEXCOORD0;
        fixed4 color : COLOR;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        fixed4 color : COLOR;
    };

    v2f vert(appdata_t v)
    {
        v2f o;
        o.position = UnityObjectToClipPos(v.position);
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.color = v.color;
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        fixed4 col = _Boost * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
        clip(col.a - _Clip);
        return col;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="opaque" }
        Blend [_SrcFactor] [_DstFactor]
        Cull [_Cull]
        Lighting off
        ZWrite [_ZWrite]
        ZTest [_ZTest]
        Fog { Mode off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            ENDCG
        }
    }
}