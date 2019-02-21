Shader "Toon/Cutoff"
{
	Properties
	{
		_Color("Color(RGBA)", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_Shininess ("Shininess(0.0:)", Float) = 1.0
		
		_ShadowThreshold ("Shadow Threshold(0.0:1.0)", Float) = 0.5
		_ShadowColor ("Shadow Color(RGBA)", Color) = (0,0,0,0.5)
		_ShadowSharpness ("Shadow Sharpness(0.0:)", Float) = 100
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.9
	}

	SubShader
	{
		// Settings
		Tags {"Queue" = "Transparent" "IgnoreProjector"="False" "RenderType" = "TransparentCutout"}
		
		
		 
		// Surface Shader Pass ( Front )
		Cull Back
		ZWrite On
		AlphaTest Greater 0.9
		//AlphaTest Equal 1
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma surface surf Toon alphatest:_Cutoff vertex:vert
		//	Toon Surface Shader for Unity
		//	File		:	ToonSurface.cginc
		//	Title		:	Toon Surface Shader with Alpha
		//	Language	:	Cg Language
		//	Include		:	ToonSurface.cginc
		/*
			How To Use	:
			
			CGPROGRAM
			#pragma surface surf Toon
			#include "ToonSurface.cginc"
			ENDCG
		*/

		float4 _Color;
		sampler2D _MainTex;
		float	_ShadowThreshold;
		float4	_ShadowColor;
		float	_ShadowSharpness;
		float	_Shininess;


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

		inline half4 LightingToon (ToonSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			//Lighting paramater
			float4	lightColor = _LightColor0 * atten;
			float	lightStrength = dot(lightDir, s.Normal) * 0.5 + 0.5;
				
			//ToonMapping
			half shadowRate = abs( max( -1, ( min( lightStrength, _ShadowThreshold ) -_ShadowThreshold)*_ShadowSharpness ) )*_ShadowColor.a;
			float4 toon = float4(1,1,1,1) * (1-shadowRate) +  _ShadowColor *shadowRate;
			
			//Output
			//float4 color = saturate( _Color * (lightColor*2) ) * s.Color;
			float4 color = _Color * (lightColor) * s.Color * (atten*2) * _Shininess;
			
			color *= toon;
			color.a = s.Alpha;
			return color;
		}

		struct Input
		{
			float2 uv_MainTex;
		};


		void vert (inout appdata_full v, out Input o) {
		    UNITY_INITIALIZE_OUTPUT(Input,o);
		}

		void surf (Input IN, inout ToonSurfaceOutput o)
		{

			// Defaults
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;

			half4 c		= tex2D(_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo	= c;
			o.Color		= c;
			o.Alpha		= c.a;
		}

		ENDCG
		
//		// Outline Pass
//		Pass
//		{
//			Cull Off
//			ZWrite On
//			Lighting Off
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			#include "ToonOutlineVertFrag.cginc"
//			ENDCG
//		}
		
		
		// Surface Shader Pass ( Back )
		Cull Front
		ZWrite On
		//AlphaTest Equal 1
		AlphaTest Greater 0.9
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma surface surf Toon alphatest:_Cutoff  vertex:vert
		//	Toon Surface Shader for Unity
		//	File		:	ToonSurface.cginc
		//	Title		:	Toon Surface Shader with Alpha
		//	Language	:	Cg Language
		//	Include		:	ToonSurface.cginc
		/*
			How To Use	:
			
			CGPROGRAM
			#pragma surface surf Toon
			#include "ToonSurface.cginc"
			ENDCG
		*/

		float4 _Color;
		sampler2D _MainTex;
		float	_ShadowThreshold;
		float4	_ShadowColor;
		float	_ShadowSharpness;
		float	_Shininess;


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

		inline half4 LightingToon (ToonSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			//Lighting paramater
			float4	lightColor = _LightColor0 * atten;
			float	lightStrength = dot(lightDir, s.Normal) * 0.5 + 0.5;
				
			//ToonMapping
			half shadowRate = abs( max( -1, ( min( lightStrength, _ShadowThreshold ) -_ShadowThreshold)*_ShadowSharpness ) )*_ShadowColor.a;
			float4 toon = float4(1,1,1,1) * (1-shadowRate) +  _ShadowColor *shadowRate;
			
			//Output
			//float4 color = saturate( _Color * (lightColor*2) ) * s.Color;
			float4 color = _Color * (lightColor) * s.Color * (atten*2) * _Shininess;
			
			color *= toon;
			color.a = s.Alpha;
			return color;
		}

		struct Input
		{
			float2 uv_MainTex;
		};


		void vert (inout appdata_full v, out Input o) {
		    UNITY_INITIALIZE_OUTPUT(Input,o);
		}

		void surf (Input IN, inout ToonSurfaceOutput o)
		{

			// Defaults
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;

			half4 c		= tex2D(_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo	= c;
			o.Color		= c;
			o.Alpha		= c.a;
		}

		ENDCG
	
	}

	// Other Environment
	Fallback "Diffuse"
}
