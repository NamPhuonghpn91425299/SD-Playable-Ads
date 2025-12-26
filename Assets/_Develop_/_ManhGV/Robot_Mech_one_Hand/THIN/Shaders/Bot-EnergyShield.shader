Shader "Horus/Bot/Energy Shield"
{
    Properties
    {
        _ColorMask ("Color Mask", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _EffectSpeed ("Effect Speed", Float) = 1
        _EffectFrequency ("Effect Frequency", Float) = 1
        _EffectMask ("Effect Mask", Float) = 1
        _EffectNoise ("Effect Noise", 2D) = "black" {}
        _EffectVelocity ("Effect Velocity", Vector) = (0, 1, 0, 0)
        _EffectColor ("Effect Color", Color) = (1, 1, 1, 1)
        _NormalPush ("Normal Push", Float) = 1
        _ShrinkPush ("Shrink Push", Float) = 1

        _FresnelPower ("Fresnel Power", Float) = 1
        _FresnelContrast ("Fresnel Contrast", Float) = 1

        _ImpactColor ("Impact Color", Color) = (1, 1, 1, 1)
        _ImpactPush ("Impact Push", float) = 0.02
        _ImpactRange ("Impact Range", Float) = 1

        _ExplosionRange ("Explosion Range", Float) = 1
        _ExplosionPush ("Explosion Push", float) = 0.2
        _ExplosionThickness ("Explosion Thickness", Float) = 1
        _ExplosionColor ("Explosion Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        Blend SrcAlpha One
        LOD 100

        Pass
        {
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #include "Assets/_Develop_/_ThanhNT/Shader/MyLib.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float3 normals : NORMAL;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float effect_strength : FLOAT0;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD2;
                float3 view_dir : TEXCOORD3;
                float2 screen_uv : TEXCOORD4;
                float3 world_pos : TEXCOORD5;
            };

            sampler2D _ColorMask;
            float4 _ColorMask_ST;
            fixed4 _Color;

            float _EffectSpeed;
            float _EffectFrequency;
            float _EffectMask;
            sampler2D _EffectNoise;
            float4 _EffectNoise_ST;
            float4 _EffectVelocity;
            fixed4 _EffectColor;
            float _NormalPush;
            float _ShrinkPush;

            float _FresnelPower;
            float _FresnelContrast;

            // int _ImpactAmount;
            #define MAX_IMPACT 5
            float4 _ImpactArray[MAX_IMPACT];
            fixed4 _ImpactColor;
            float _ImpactRange, _ImpactPush;

            float _ExplosionRange;
            float4 _Explosion;
            float _ExplosionThickness, _ExplosionPush;
            fixed4 _ExplosionColor;

            v2f vert(appdata v)
            {
                v2f o;

                // const float effect_strength = sin(v.uv2.y * _EffectFrequency + _EffectSpeed * _Time);
                // const float effect_noise = tex2Dlod(_EffectNoise, float4(v.uv2 * _EffectNoise_ST.xy, 0, 0)).r;
                // o.effect_strength = smoothstep(_EffectMask, 1.0, effect_strength) * effect_noise;

                o.world_pos = mul(UNITY_MATRIX_M, v.vertex);
                o.effect_strength = tex2Dlod(_EffectNoise,
                                             float4(v.uv2 * _EffectNoise_ST.xy + float2(0, _EffectSpeed * _Time.x), 0,
                                                    0)).r;
                float normal_push = _NormalPush;
                for (int index = 0; index < MAX_IMPACT; index++)
                {
                    if (_ImpactArray[index].w > 0)
                    {
                        normal_push += _ImpactArray[index].w * _ImpactPush;
                    }
                }
                
                if (_Explosion.w < _ExplosionRange)
                {
                    const float var_distance = distance(o.world_pos, _Explosion.xyz);
                    float explosion_strength = saturate(1 - abs(_Explosion.w - var_distance) / _ExplosionThickness);
                    explosion_strength *= 1 - smoothstep(0, _ExplosionRange, var_distance);
                    normal_push += explosion_strength * _ExplosionPush;
                }
                const float3 var_rejection = rejection(v.vertex, v.normals) * (-1);
                float4 new_vertex = v.vertex;
                new_vertex.xyz += v.normals * normal_push * o.effect_strength;
                new_vertex.xyz += var_rejection * _ShrinkPush * o.effect_strength;
                o.uv = TRANSFORM_TEX(v.uv, _ColorMask);
                o.vertex = UnityObjectToClipPos(new_vertex);
                o.normal = UnityObjectToWorldNormal(v.normals);
                o.view_dir = WorldSpaceViewDir(new_vertex);
                o.screen_uv = screen_uv(o.vertex);


                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(const v2f i) : SV_Target
            {
                const fixed col_mask = tex2D(_ColorMask, i.uv).r;
                fixed4 col = lerp(_Color, _EffectColor, i.effect_strength);
                col *= col_mask;

                const float var_fresnel = fresnel(i.normal, i.view_dir, 0, _FresnelContrast, _FresnelPower);
                col.a *= var_fresnel;

                for (int index = 0; index < MAX_IMPACT; index++)
                {
                    if (_ImpactArray[index].w > 0)
                    {
                        const float impact_strength = saturate(
                            _ImpactArray[index].w * _ImpactRange - distance(i.world_pos, _ImpactArray[index].xyz));
                        col = lerp(col, _ImpactColor, impact_strength);
                    }
                }

                if (_Explosion.w < _ExplosionRange)
                {
                    const float var_distance = distance(i.world_pos, _Explosion.xyz);
                    float explosion_strength = saturate(1 - abs(_Explosion.w - var_distance) / _ExplosionThickness);
                    explosion_strength *= 1 - smoothstep(0, _ExplosionRange, var_distance);
                    col = lerp(col, _ExplosionColor, explosion_strength);
                }

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}