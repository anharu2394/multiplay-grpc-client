Shader "Toon/Hair"
{
	Properties
	{
		_Color("Color(RGBA)", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_SphereAddTex("Texture(Sphere)", 2D) = "black" {}
		_Shininess ("Shininess(0.0:)", Float) = 1.0

		_ShadowThreshold ("Shadow Threshold(0.0:1.0)", Float) = 0.5
		_ShadowColor ("Shadow Color(RGBA)", Color) = (0,0,0,0.5)
		_ShadowSharpness ("Shadow Sharpness(0.0:)", Float) = 100
	}

	SubShader
	{
		// Settings
		Tags {"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType" = "Transparent"}
		//Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}

		// Surface Shader Pass ( Front )
		Cull Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		//#pragma surface surfHair ToonHair vertex:vert
		#pragma surface surfHair ToonHair vertex:vert
		//	Toon Hair Surface Shader for Unity
		//	File		:	ToonSurfaceHair.cginc
		//	Title		:	Toon Surface Shader with Alpha
		//	Language	:	Cg Language
		//	Include		:	ToonSurfaceHair.cginc
		/*
			How To Use	:
			
			CGPROGRAM
			#pragma surface surfHair ToonHair vertex:vert
			#include "ToonSurfaceHair.cginc"
			ENDCG
		*/

		float4 _Color;
		sampler2D _MainTex;
		float	_ShadowThreshold;
		float4	_ShadowColor;
		float	_ShadowSharpness;
		float	_Shininess;
		sampler2D _SphereAddTex;



		struct ToonSurfaceOutput
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Gloss;
			half Specular;
			half Alpha;
			half4 Color;
		};

		inline half4 LightingToonHair (ToonSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			//Lighting paramater
			float4	lightColor = _LightColor0 * atten;
			float	lightStrength = dot(lightDir, s.Normal) * 0.5 + 0.5;
				
			//ToonMapping
			half shadowRate = abs( max( -1, ( min( lightStrength, _ShadowThreshold ) -_ShadowThreshold)*_ShadowSharpness ) )*_ShadowColor.a;
			float4 toon = float4(1,1,1,1) * (1-shadowRate) +  _ShadowColor *shadowRate;
			
			//Output
			//float4 color = saturate( _Color * lightColor*2 ) * s.Color;
			float4 color = _Color * (lightColor) * s.Color * (atten*2) * _Shininess;
			color *= toon;
			color.a = s.Alpha;
			return color;
		}


		struct Input
		{
			float2 uv_MainTex;
		    float3 customColor;
		};

		void vert (inout appdata_full v, out Input o) {
		    UNITY_INITIALIZE_OUTPUT(Input,o);
		    //o.customColor = abs(v.normal);
		    o.customColor = v.normal;
		}

		void surfHair (Input IN, inout ToonSurfaceOutput o)
		{

			// Defaults
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			
			
			// Sphere Map
			float3 viewNormal = normalize( IN.customColor );
			float3 viewNormal2 = normalize( mul( UNITY_MATRIX_MV, float4(normalize(o.Normal), 0.0) ).xyz );
			
			viewNormal = normalize(viewNormal*0.7 + viewNormal2*0.3);
			
			float2 sphereUv = viewNormal.xz * 0.5 + 0.5;
			
			float4 sphereAdd = tex2D( _SphereAddTex, sphereUv );
			
			
			half4 c		= tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Color		= c;
			o.Color		+= sphereAdd*0.2 * step(0,viewNormal.y); // SphereAddTex
			o.Alpha		= c.a;
		}

		ENDCG
	
	}

	// Other Environment
	Fallback "Transparent/Diffuse"
}
