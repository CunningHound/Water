#if !defined(LOOKING_THROUGH_WATER_INCLUDED)
#define LOOKING_THROUGH_WATER_INCLUDED

#pragma multi_compile_fog

uniform sampler2D _CameraDepthTexture, _WaterBackground;
float4 _CameraDepthTexture_TexelSize;

float3 _WaterFogColour;
float _WaterMaxFogDepth;

float3 UnderwaterColour (float4 screenPos) {
	float2 uv = screenPos.xy / screenPos.w;
	float bgDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos)));
	float surfaceDepth = LinearEyeDepth(screenPos.w);
	float3 bgColour = tex2D(_WaterBackground,uv).rgb;
	float waterDepth = (bgDepth - surfaceDepth);
	float fogDensity = 1;
	if(_WaterMaxFogDepth > 0)
	{
		fogDensity = waterDepth / _WaterMaxFogDepth;
		fogDensity = clamp(fogDensity, 0, 1);
	}
	return lerp(bgColour, _WaterFogColour, fogDensity);
}

#endif