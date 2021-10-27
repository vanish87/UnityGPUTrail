Shader "Unlit/NewTrailShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}


	CGINCLUDE
	#include "UnityCG.cginc"

	struct v2g
	{
		float3 p0 : TEXCOORD0;
		float3 p1 : TEXCOORD1;
		float3 p2 : TEXCOORD2;
		float3 p3 : TEXCOORD3;
		float2 uv12  : TEXCOORD4;
	};

	struct g2f
	{
		float4 pos : POSITION;
		float2 uv  : TEXCOORD;
		float4 col : COLOR;
	};
    #define PSType g2f
    #include "Trail.cginc"

	sampler2D _MainTex;
	float4 _ST;

    float4 _Color;
	StructuredBuffer<float4> _TrailData;
	int _TrailDataCount;

	float _Thickness;
	float _MiterLimit;
	int _zScale;

	static const int TrailThicknessGradient = 0;
	static const int TrailHorizontalAlphaGradient = 1;
	static const int TrailVerticalAlphaGradient = 2;
	static const int TrailColorGradient = 3;

	sampler2D _TrailGradientTexture;
	int _TrailGradientTextureHeight;

	float2 GradientToUV(float uvx, int gradient)
	{
		float texSize = 1.0f/_TrailGradientTextureHeight;
		return float2(uvx, gradient * texSize + texSize * 0.5f);
	}

	float4 SampleXLod(sampler2D tex, float2 uv, int gradient)
	{
		float2 tuv = GradientToUV(uv.x, gradient);
		return tex2Dlod(tex, float4(tuv,0,0));
	}
	float4 SampleX(sampler2D tex, float2 uv, int gradient)
	{
		return tex2D(tex, GradientToUV(uv.x, gradient));
	}
	float4 SampleY(sampler2D tex, float2 uv, int gradient)
	{
		return tex2D(tex, GradientToUV(uv.y, gradient));
	}

	v2g vert(uint vid : SV_VertexID) 
	{
		v2g o = (v2g)0;
		int id = vid;
		bool prev  = id > 0;
		bool next  = id+1<_TrailDataCount;
		bool nnext = id+2<_TrailDataCount;

		int i0 = prev?id-1:id;
		int i1 = id;
		int i2 = next?id+1:id;
		int i3 = nnext?id+2:next?id+1:id;

		float4 p0 = _TrailData[i0];
		float4 p1 = _TrailData[i1];
		float4 p2 = _TrailData[i2];
		float4 p3 = _TrailData[i3];

		p0 = prev?p0:p1+normalize(p1-p2);
		p3 = nnext?p3:p2+normalize(p2-p1);

		p0 = float4(UnityObjectToViewPos(p0), 1);
		p1 = float4(UnityObjectToViewPos(p1), 1);
		p2 = float4(UnityObjectToViewPos(p2), 1);
		p3 = float4(UnityObjectToViewPos(p3), 1);

		// p0 = UnityObjectToClipPos(p0);
		// p1 = UnityObjectToClipPos(p1);
		// p2 = UnityObjectToClipPos(p2);
		// p3 = UnityObjectToClipPos(p3);

        o.p0 = p0;
        o.p1 = p1;
        o.p2 = p2;
        o.p3 = p3;
		o.uv12 = saturate(float2(id, id+1)/(_TrailDataCount-1));
		return o;
	}

    inline float4 Generate(float4 pos)
    {
		// return pos;
        return mul(UNITY_MATRIX_P, pos);
    }



	[maxvertexcount(16)]
	void geom(point v2g p[1], inout TriangleStream<g2f> outStream)
	{
		g2f pIn = (g2f)0;

        float3 p0 = p[0].p0;
		float3 p1 = p[0].p1;
		float3 p2 = p[0].p2;
		float3 p3 = p[0].p3;

		float2 uv12 = p[0].uv12;

        GenerateMainLine(outStream, p0, p1, p2, p3, _Thickness, uv12);
        GenerateCorner(outStream, p0, p1, p2, p3, _Thickness, uv12);
    }

	fixed4 frag(g2f i) : SV_Target
	{
		float2 uv = i.uv;
		float4 col = float4(uv, 0, 1);
		float4 colGradient = SampleX(_TrailGradientTexture, uv, TrailColorGradient);
		col = colGradient;
		float4 alphaX = SampleX(_TrailGradientTexture, uv, TrailHorizontalAlphaGradient);
		float4 alphaY = SampleY(_TrailGradientTexture, uv, TrailVerticalAlphaGradient);
		col.a *= alphaX.a * alphaY.a;
        col.a = 0.5;
		return col;
		return i.col;
	}

	ENDCG

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        // ZWrite Off
        // Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}
	}
}

