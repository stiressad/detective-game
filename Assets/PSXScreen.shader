Shader "Custom/PSXScreen"
{
	Properties
	{
	_MainTex("Screen", 2D) = "white" {}

	_ColorSteps("Color Steps", Range(8, 256)) = 48
	_Grain("Film Grain", Range(0, 0.2)) = 0.025
	_Vignette("Vignette", Range(0, 1)) = 0.35
	_ColdTint("Color Tint", Color) = (0.9, 0.95, 1, 1)
	}

		SubShader
	{
	Tags
	{
	"Queue" = "Transparent"
	"RenderType" = "Transparent"
	}

	Cull Off
	ZWrite Off
	ZTest Always
	Blend SrcAlpha OneMinusSrcAlpha

	Pass
	{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	float _ColorSteps;
	float _Grain;
	float _Vignette;
	float4 _ColdTint;

	struct appdata
	{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	};

	struct v2f
	{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	};

	v2f vert(appdata v)
	{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
	float3 color = tex2D(_MainTex, i.uv).rgb;

	// Animated grain
	float2 pixel =
	floor(i.uv * _MainTex_TexelSize.zw);

	float timeStep = floor(_Time.y * 8.0);

	float noise = frac(sin(dot(
	pixel + timeStep,
	float2(12.9898, 78.233)
	)) * 43758.5453) - 0.5;

	color += noise * _Grain;

	// Reduced color precision
	color = floor(
	saturate(color) * _ColorSteps + 0.5
	) / _ColorSteps;

	// Vignette
	float2 centered = i.uv * 2.0 - 1.0;

	float vignette = 1.0 - _Vignette *
	smoothstep(
	0.35,
	1.8,
	dot(centered, centered)
	);

	color *= vignette;

	// Cold color grading
	color *= _ColdTint.rgb;

	return float4(saturate(color), 1.0);
	}
	ENDCG
	}
	}
}