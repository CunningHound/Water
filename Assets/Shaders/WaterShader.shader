Shader "Custom/WaterShader"
{
    Properties
    {
        _Color("Color", Color) = (0.1,0.5,1,1)
        _Steepness("Steepness", Vector) = (0.0,0.0,0.0,0.0)
		_Wavelength("Wavelength", Vector) = (0.0,0.0,0.0,0.0)
		_Direction("Direction", Vector) = (0.0,0.0,0.0,0.0)
    }

		SubShader
	{
		Tags
		{
			"RenderType" = "transparent"
		}

		Pass
		{

		Cull Off

		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma vertex vertexFunc
		#pragma fragment fragFunc

		float4 _Color;
		float4 _Steepness;
		float4 _Wavelength;
		float4 _Direction;

		static const float PI = 3.14159f;

		struct vertexInput {
			float4 vertex : POSITION;
		};

		struct vertexOutput {
			float4 pos : SV_POSITION;
		};

		float3 directionComponents(float IN)
		{
			float angle_rad = radians(IN);
			float3 res;
			res.x = cos(angle_rad);
			res.y = 0;
			res.z = sin(angle_rad);
			return res;
		}

		float3 gerstnerOffsets(float4 worldPos, float3 dir, float s, float k)
		{
			float3 result;

			float a = s / k;
			float c = sqrt(9.8 / k);

			float f = k * (dot(dir, worldPos.xyz) - c * _Time);
			result.x = dir.x * a * sin(f);
			result.y = a * cos(f);
			result.z = dir.z * a * sin(f);

			return result;
		}

		vertexOutput vertexFunc(vertexInput IN)
		{
			vertexOutput o;

			float4 worldPos = mul(unity_ObjectToWorld, IN.vertex);
			float3 displacement = (0,0,0);
			for (int i = 0; i < 4; i++)
			{
				if (_Wavelength[i] > 0.)
				{
					float waveNumber = 2 * PI / _Wavelength;
					float3 dir = directionComponents(_Direction[i]);

					displacement += gerstnerOffsets(worldPos, dir, _Steepness[i], waveNumber);
				}
			}

			worldPos.x += displacement.x;
			worldPos.y -= displacement.y;
			worldPos.z += displacement.z;

			o.pos = mul(UNITY_MATRIX_VP, worldPos);
			return o;
		}

		float4 fragFunc(vertexOutput IN) : COLOR
		{
			return _Color;
		}

		ENDCG
		}
	}
		FallBack "Diffuse"
}
