Shader "Custom/WaterShader"
{
    Properties
    {
        _Color("Color", Color) = (0.1,0.5,1,0.7)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Occlusion("Occlusion", Range(0,1)) = 0.4
		_SpecularLightColor("Specular colour", Color) = (0.7,0.8,0.9,1)
		_PeakColour("Peak Colour", Color) = (0.2,0.6,1,0.8)

		_WaterFogColour("Water Fog Colour", Color) = (0.1,0.5,1)
		_WaterMaxFogDepth("Water Max Fog Depth", Range(0,50)) = 10

		_Wave1("Wave 1 (dir, steepness, wavelength)", Vector) = (90, 0.3, 15, 0)
		_Wave2("Wave 2 (dir, steepness, wavelength)", Vector) = (80, 0.2, 8, 0)
		_Wave3("Wave 3 (dir, steepness, wavelength)", Vector) = (100, 0.1, 4, 0)
		_Wave4("Wave 4 (dir, steepness, wavelength)", Vector) = (95, 0.05, 2, 0)
		_Wave5("Wave 5 (dir, steepness, wavelength)", Vector) = (105, 0.05, 1, 0)
    }

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
		}

		GrabPass{ "_WaterBackground" }

		CGPROGRAM
		#pragma surface surf StandardSpecular vertex:vert addshadow finalcolor:ResetAlpha alpha:premul
		#include "Underwater.cginc"

		struct Input {
			float2 uv_SpecularNoise1;
			float2 uv_SpecularNoise2;
			float4 screenPos;
			float3 worldPos;
		};

		half _Glossiness;
		half _Occlusion;
		float4 _Color;
		float4 _SpecularLightColor;
		float4 _PeakColour;

		void surf(Input IN, inout SurfaceOutputStandardSpecular o)
		{
			float worldHeight = IN.worldPos.y;
			float lerpToPeakFactor = clamp(worldHeight + 0.5, 0.0, 1.0);
			fixed4 c = lerp(_Color, _PeakColour, lerpToPeakFactor);
			o.Albedo = c.rgb;
			o.Smoothness = _Glossiness;
			o.Occlusion = _Occlusion;
			o.Specular = _SpecularLightColor.rgb;
			o.Alpha = c.a;

			o.Emission = UnderwaterColour(IN.screenPos) * (1-c.a);
		}

		void ResetAlpha(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 colour)
		{
			colour.a = 1;
		}

		//float4 _Steepness;
		//float4 _Wavelength;
		//float4 _Direction;

		float4 _Wave1;
		float4 _Wave2;
		float4 _Wave3;
		float4 _Wave4;
		float4 _Wave5;

		static const float PI = 3.14159f;

		float2 directionComponents(float IN)
		{
			float angle_rad = radians(IN);
			float2 res;
			res.x = cos(angle_rad);
			res.y = sin(angle_rad);
			return res;
		}

		float3 gerstnerWave(float4 wave, float3 pos, inout float3 tangent, inout float3 binormal)
		{
			float steepness = wave.y;
			float wavelength = wave.z;
			float k = 2 * PI / wavelength; // wave number
			float c = sqrt(9.8 / k);
			float2 d = normalize(directionComponents(wave.x));
			float f = k * dot(d,pos.xz) - c * _Time.y;
			float a = steepness / k;

			tangent += float3(
				1 -d.x * d.x * steepness * cos(f),
				d.x * steepness * sin(f),
				-d.x * d.y * steepness * cos(f)
				);

			binormal += float3(
				-d.x * d.y * steepness * cos(f),
				d.y * steepness * sin(f),
				1 -d.y * d.y * steepness * cos(f)
				);

			return float3(
				d.x * a * sin(f),
				-a * cos(f),
				d.y * a * sin(f)
				);

		}


		void vert(inout appdata_full vertexData)
		{
			float waveScaleFactor = vertexData.vertex.y;
			vertexData.vertex.y = 0;
			float3 worldPos = mul(unity_ObjectToWorld, float4(vertexData.vertex.x, 0, vertexData.vertex.z, 1)).xyz;

			float3 tangent = (0, 0, 0);
			float3 binormal = (0, 0, 0);
			if(waveScaleFactor > 0)
			{
				worldPos += gerstnerWave(_Wave1, worldPos, tangent, binormal);
				worldPos += gerstnerWave(_Wave2, worldPos, tangent, binormal);
				worldPos += gerstnerWave(_Wave3, worldPos, tangent, binormal);
				worldPos += gerstnerWave(_Wave4, worldPos, tangent, binormal);
				worldPos += gerstnerWave(_Wave5, worldPos, tangent, binormal);
				worldPos.y *= waveScaleFactor;
			}

			float3 normal = normalize(cross(binormal, tangent));

			vertexData.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
			vertexData.normal = normal;
		}

		ENDCG
	}
}
