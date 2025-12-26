Shader "Custom/AutoTilingLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TilePerUnit ("Tile Per Unit", Float) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float _TilePerUnit;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Get world scale of object
            float3 scaleX = float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20);
            float3 scaleZ = float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22);

            float scaleXLen = length(scaleX);
            float scaleZLen = length(scaleZ);

            // World tiling based on scale and tile density
            float2 tiling = float2(scaleXLen, scaleZLen) * _TilePerUnit;

            // Adjust UVs accordingly
            float2 tiledUV = IN.uv_MainTex * tiling;

            fixed4 col = tex2D(_MainTex, tiledUV) * _Color;
            o.Albedo = col.rgb;
            o.Alpha = col.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
