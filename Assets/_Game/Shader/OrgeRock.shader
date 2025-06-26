Shader "Mobile/Orge Rock"
{
    Properties
    {
        _Appearance ("Appearance", Range(0, 1)) = 1
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ZTest LEqual

        LOD 250

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert alpha:blend
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        sampler2D _BumpMap;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float, _Appearance)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input
        {
            float2 uv_MainTex;
            float alpha;
        };

        inline float remap(float IN, float in_min, float in_max, float out_min, float out_max)
        {
            return out_min + (IN - in_min) * (out_max - out_min) / (in_max - in_min);
        }


        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float appearance = 1 - UNITY_ACCESS_INSTANCED_PROP(Props, _Appearance);
            worldPos.y -= appearance * 2;
            o.alpha = saturate(1 - appearance * 2);
            v.vertex = mul(unity_WorldToObject, float4(worldPos, 1));
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = IN.alpha;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
        }
        ENDCG
    }

    FallBack "Mobile/Diffuse"
}