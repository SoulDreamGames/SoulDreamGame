Shader "PGATR/Grass"
{
    Properties
    {
		[Header(Shading)]
        _TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
		_BladeWidth("Blade Width", Float) = 0.5
		_BladeWidthRandom("Blade Width Random", Float) = 0.02
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WIndStrength("Wind Strength", Float) = 1
		_BladeForward("Blade Forward Amount", Float) = 0.38
		_BladeCurve("Blade Curvature Amount", Range(1, 4)) = 2
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#define BLADE_SEGMENTS 3

	float _BendRotationRandom;
	float _BladeHeight;
	float _BladeWidth;
	float _BladeWidthRandom;
	float _BladeHeightRandom;
	sampler2D _WindDistortionMap;
	float4 _WindDistortionMap_ST;
	float2 _WindFrequency;
	float _WIndStrength;
	float _BladeCurve;
	float _BladeForward;

	struct geometryOutput
	{
		float4 pos: SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	struct vertexInput{
		float4 vertex: POSITION;
		float3 normal: NORMAL;
		float4 tangent: TANGENT;
	};
	struct vertexOutput{
		float4 vertex : SV_POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

	geometryOutput VertexOutput(float3 pos, float2 uv){
		geometryOutput o;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = uv;
		return o;
	}

	// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
	// Extended discussion on this function can be found at the following link:
	// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
	// Returns a number in the 0...1 range.
	float rand(float3 co)
	{
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}

	// Construct a rotation matrix that rotates around the provided axis, sourced from:
	// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
	}

	vertexOutput vert(vertexInput v)
	{
		// return UnityObjectToClipPos(vertex);
		vertexOutput o;
		o.vertex = v.vertex;
		o.normal = v.normal;
		o.tangent = v.tangent;
		return o;
	}
	geometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, float forward, float2 uv, float3x3 transfrmMatrix)
	{
		float3 tangentPoint = float3(width, forward, height);
		float3 localPosition = vertexPosition + mul(transfrmMatrix, tangentPoint);
		return VertexOutput(localPosition, uv);
	}

	[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
	void geo(triangle vertexOutput IN[3], inout TriangleStream<geometryOutput> triStream)
	{
		float3 pos = IN[0].vertex;

		float3 vNormal = IN[0].normal;
		float4 vTangent = IN[0].tangent;
		float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;
		float3x3 tangentToLocal = float3x3(
			vTangent.x, vBinormal.x, vNormal.x,
			vTangent.y, vBinormal.y, vNormal.y,
			vTangent.z, vBinormal.z, vNormal.z
		);
		float3x3 facingRotatingMatrix = AngleAxis3x3(
			rand(pos) * UNITY_TWO_PI, float3(0,0,1)
		);

		float3x3 bendRotationMatrix = AngleAxis3x3(
			rand(pos.zzx) * _BendRotationRandom * UNITY_PI, float3(-1,0,0)
		);

		float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency*_Time.y;
		float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0,0)).xy*2 -1) * _WIndStrength;
		float3 wind = normalize(float3(windSample.x, windSample.y, 0));
		float3x3 windRotation = AngleAxis3x3(UNITY_PI* windSample, wind);

		float3x3 transformationMatrix = mul(
			mul(
				mul(tangentToLocal, windRotation), facingRotatingMatrix), bendRotationMatrix 
		);
		float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotatingMatrix);

		float height = (rand(pos.zyx) * 2 -1) * _BladeHeightRandom + _BladeHeight;
		float width = (rand(pos.zyx) * 2 -1) * _BladeWidthRandom + _BladeWidth;
		float forward = rand(pos.yyz) * _BladeForward;

		for (int i = 0; i < BLADE_SEGMENTS; i++){
			float t = i / (float) BLADE_SEGMENTS;
			float segmentHeight = height * t;
			float segmentWidth = width * (1-t);
			float segmentForward = pow(t, _BladeCurve) * forward;
			float3x3 transformMatrix = i == 0 ? transformationMatrixFacing : transformationMatrix;
			triStream.Append(
				GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, float2(0,t), transformMatrix)
			);
			triStream.Append(
				GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, float2(1,t), transformMatrix)
			);
		}
		triStream.Append(
			GenerateGrassVertex(pos, 0, height, forward, float2(0.5,1), transformationMatrix)
		);
	}

	ENDCG

    SubShader
    {
		Cull Off

        Pass
        {
			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry geo
			#pragma target 4.6
            
			#include "Lighting.cginc"

			float4 _TopColor;
			float4 _BottomColor;
			float _TranslucentGain;

			float4 frag (geometryOutput i, fixed facing : VFACE) : SV_Target
            {	
				return lerp(_BottomColor, _TopColor, i.uv.y);
            }
            ENDCG
        }
    }
}