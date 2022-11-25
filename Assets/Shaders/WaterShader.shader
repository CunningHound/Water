Shader "Custom/WaterShader"
{
    Properties
    {
        _Color("Color", Color) = (0.1,0.5,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.4
		_Alpha("Alpha", Range(0,1)) = 0.5
        _Steepness("Steepness", Vector) = (0.0,0.0,0.0,0.0)
		_Wavelength("Wavelength", Vector) = (0.0,0.0,0.0,0.0)
		_Direction("Direction", Vector) = (0.0,0.0,0.0,0.0)
    }

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
		}
		LOD 200
		CULL OFF

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow


		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;

		half _Glossiness;
		half _Metallic;
		float4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}


		float4 _Steepness;
		float4 _Wavelength;
		float4 _Direction;

		static const float PI = 3.14159f;

		float2 directionComponents(float IN)
		{
			float angle_rad = radians(IN);
			float2 res;
			res.x = cos(angle_rad);
			res.y = sin(angle_rad);
			return res;
		}

		float3 gerstnerWave(float3 wave, float3 pos, inout float3 tangent, inout float3 binormal)
		{
			float steepness = wave.y;
			float wavelength = wave.z;
			float k = 2 * PI / wavelength; // wave number
			float c = sqrt(9.8 / k);
			float2 d = normalize(directionComponents(wave.x));
			float f = k * dot(d,pos.xz) - c * _Time;
			float a = steepness / k;

			tangent += float3(
				-d.x * d.x * steepness * cos(f),
				d.x * steepness * sin(f),
				-d.x * d.y * steepness * cos(f)
				);

			binormal += float3(
				-d.x * d.y * steepness * cos(f),
				d.y * steepness * sin(f),
				-d.y * d.y * steepness * cos(f)
				);

			return float3(
				d.x * a * sin(f),
				-a * cos(f),
				d.y * a * sin(f)
				);

		}


		void vert(inout appdata_full vertexData)
		{
			float3 worldPos = vertexData.vertex.xyz;

			float3 displacement = (0,0,0);
			float3 tangent = (1, 0, 0);
			float3 binormal = (0, 0, 1);
			for (int i = 0; i < 4; i++)
			{
				float3 wave = float3(_Direction[i], _Steepness[i], _Wavelength[i]);
				worldPos += gerstnerWave(wave, worldPos, tangent, binormal);
			}

			float3 normal = normalize(cross(binormal, tangent));

			vertexData.vertex.xyz = worldPos;
			// vertexData.normal = worldPos;
			vertexData.normal = normal;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
