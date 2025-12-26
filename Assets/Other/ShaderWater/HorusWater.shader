// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Horus/Water" {
	Properties{
		_BumpMap("Normals ", 2D) = "bump" {}

		_DistortParams("Distortions (Bump waves, Reflection, Fresnel power, Fresnel bias)", Vector) = (1.0 ,1.0, 2.0, 1.15)

		_BumpTiling("Bước Sóng", Vector) = (1.0 ,1.0, -2.0, 3.0)
		_BumpDirection("Tốc Độc Và Hướng Sóng", Vector) = (1.0 ,1.0, -1.0, 1.0)


		_BaseColor("Màu Sóng", COLOR) = (.54, .95, .99, 0.5)
		_ReflectionColor("Màu Phản Chiếu ", COLOR) = (.54, .95, .99, 0.5)
		_SpecularColor("Màu Ánh Sáng", COLOR) = (.72, .72, .72, 1)

		_WorldLightDir("Hướng Ánh Sáng", Vector) = (0.0, 0.1, -0.5, 0.0)
		_Shininess("Cường Độ Ánh Sáng", Range(2.0, 500.0)) = 200.0
		[Toggle] _ZWrite("Z Write", float) = 0
		_Opacity("Opcaity", range(0, 1)) = 1
	}


		CGINCLUDE

#include "UnityCG.cginc"
#include "WaterInclude.cginc"

			struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		// interpolator structs

		struct v2f
		{
			float4 pos : SV_POSITION;
			float4 normalInterpolator : TEXCOORD0;
			float4 viewInterpolator : TEXCOORD1;
			float4 bumpCoords : TEXCOORD2;
			float4 screenPos : TEXCOORD3;
			float4 grabPassPos : TEXCOORD4;
			UNITY_FOG_COORDS(5)
		};

		struct v2f_noGrab
		{
			float4 pos : SV_POSITION;
			float4 normalInterpolator : TEXCOORD0;
			float3 viewInterpolator : TEXCOORD1;
			float4 bumpCoords : TEXCOORD2;
			float4 screenPos : TEXCOORD3;
			UNITY_FOG_COORDS(4)
		};

		struct v2f_simple
		{
			float4 pos : SV_POSITION;
			float4 viewInterpolator : TEXCOORD0;
			float4 bumpCoords : TEXCOORD1;
			UNITY_FOG_COORDS(2)
		};

		// textures
		sampler2D _BumpMap;
		sampler2D_float _CameraDepthTexture;

		// colors in use
		uniform float4 _RefrColorDepth;
		uniform float4 _SpecularColor;
		uniform float4 _BaseColor;
		uniform float4 _ReflectionColor;

		// edge & shore fading

		// specularity
		uniform float _Shininess;
		uniform float4 _WorldLightDir;

		// fresnel, vertex & bump displacements & strength
		uniform float4 _DistortParams;
		//uniform float _FresnelScale;
		uniform float4 _BumpTiling;
		uniform float4 _BumpDirection;
		uniform float _Opacity;



		// shortcuts
#define PER_PIXEL_DISPLACE _DistortParams.x
#define REALTIME_DISTORTION _DistortParams.y
#define FRESNEL_POWER _DistortParams.z
#define VERTEX_WORLD_NORMAL i.normalInterpolator.xyz
#define FRESNEL_BIAS _DistortParams.w


//
// LQ VERSION
//

		v2f_simple vert200(appdata_full v)
		{
			v2f_simple o;

			half3 worldSpaceVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
			half2 tileableUv = worldSpaceVertex.xz;

			o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

			o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;

			o.pos = UnityObjectToClipPos(v.vertex);

			o.viewInterpolator.w = 1;//GetDistanceFadeout(ComputeNonStereoScreenPos(o.pos).w, DISTANCE_SCALE);

			UNITY_TRANSFER_FOG(o, o.pos);
			return o;

		}

		half4 frag200(v2f_simple i) : SV_Target
		{
			half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, half3(0,1,0), PER_PIXEL_DISPLACE);
			half3 viewVector = normalize(i.viewInterpolator.xyz);

			half3 reflectVector = normalize(reflect(viewVector, worldNormal));
			half3 h = normalize((_WorldLightDir.xyz) + viewVector.xyz);
			float nh = max(0, dot(worldNormal, -h));
			float spec = max(0.0,pow(nh, _Shininess));

			//worldNormal.xz *= _FresnelScale;
			half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);

			half4 baseColor = _BaseColor;
			baseColor = lerp(baseColor, _ReflectionColor, saturate(refl2Refr * 2.0));
			baseColor.a = saturate(2.0 * refl2Refr + 0.5);
			baseColor.a *= _Opacity;
			baseColor.rgb += spec * _SpecularColor.rgb;
			UNITY_APPLY_FOG(i.fogCoord, baseColor);
			return baseColor;
		}

			ENDCG



			Subshader
		{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

				Lod 200
				ColorMask RGB

				Pass{
						Blend SrcAlpha OneMinusSrcAlpha
						ZTest LEqual
						ZWrite[_ZWrite]
						Cull Off

						CGPROGRAM

						#pragma vertex vert200
						#pragma fragment frag200
						#pragma multi_compile_fog

						ENDCG
			}
		}

		Fallback "Transparent/Diffuse"
}
