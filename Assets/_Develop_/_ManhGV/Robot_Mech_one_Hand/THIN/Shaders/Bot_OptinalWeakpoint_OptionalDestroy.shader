Shader "Horus/Bot/Optimized Weakpoint, Destroy"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _IsWeakness ("Is Weakness", Float) = 0
        _WeaknessColor ("Weakness Color", Color) = (1, 0, 0, 1)
        _WeaknessScale ("Weakness Scale", Float) = 1
        _WeaknessBias ("Weakness Bias", Float) = 0
        _WeaknessBlinkSpeed ("Weakness Blink Speed", Float) = 10
        [Toggle] _IsDestroyed ("Is Destroyed", Float) = 0
        _BurnTex ("Burn Texture", 2D) = "gray" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Main"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normals : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed _IsWeakness;
            fixed4 _WeaknessColor;
            float _WeaknessScale;
            float _WeaknessBias;
            float _WeaknessBlinkSpeed;

            fixed _IsDestroyed;
            sampler2D _BurnTex;
            float4 _BurnTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = lerp(
                    TRANSFORM_TEX(v.uv, _MainTex),
                    TRANSFORM_TEX(v.uv, _BurnTex),
                    _IsDestroyed
                );

                o.worldNormal = UnityObjectToWorldNormal(v.normals);
                o.viewDir = WorldSpaceViewDir(v.vertex);

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 col;

                col = lerp(
                    tex2D(_MainTex, i.uv),
                    tex2D(_BurnTex, i.uv),
                    _IsDestroyed
                );

                if (_IsWeakness)
                {
                    float3 normal = normalize(i.worldNormal);
                    float3 viewDir = normalize(i.viewDir);
                    float NdotV = dot(normal, viewDir);

                    float blink = (sin(_Time.y * _WeaknessBlinkSpeed) + 1.0) * 0.5;
                    float strength = _WeaknessBias + _WeaknessScale * (1.0 - NdotV) * blink;

                    fixed4 weaknessColor = strength * _WeaknessColor;
                    col = col + weaknessColor;
                }

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}