Shader "Custom/WaterShader"
{
    Properties
    {
        _Color("Color", Color) = (0.1,0.5,1,1)
        _Amplitude("Amplitude", Vector) = (0.0,0.0,0.0,0.0)
        _Speed("Speed", Vector) = (0.0,0.0,0.0,0.0)
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
		float4 _Amplitude;
		float4 _Speed;
		float4 _Wavelength;
		float4 _Direction;

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

		float getDisplacementComponent(float pos, float directionalComponent, float wavelength, float speed)
		{
			return cos((pos * directionalComponent / wavelength) + speed * _Time);
		}


		vertexOutput vertexFunc(vertexInput IN)
		{
			vertexOutput o;

			float4 worldPos = mul(unity_ObjectToWorld, IN.vertex);
			float displacement = 0;
			for (int i = 0; i < 4; i++)
			{
				if (_Wavelength[i] > 0.)
				{
					float3 dir = directionComponents(_Direction[i]);
					float this_displacement = 0;
					this_displacement += getDisplacementComponent(worldPos.x, dir.x, _Wavelength[i], _Speed[i]);
					this_displacement += getDisplacementComponent(worldPos.z, dir.z, _Wavelength[i], _Speed[i]);
					displacement += this_displacement*_Amplitude[i];
				}
			}

			worldPos.y = worldPos.y + displacement;

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
