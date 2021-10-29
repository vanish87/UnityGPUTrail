
static const float LineBaseWidth = 0.5f;
static const float MaxLineWidth = 1;

float2 GetNormal(float2 a, float2 b, float2 c)
{
    float2 tangent = normalize(normalize(c - b) + normalize(b - a));
    return float2(-tangent.y, tangent.x);
}
float2 GetNormal(float2 a, float2 b)
{
    float2 l = normalize(b-a);
    return float2(-l.y, l.x);
}
void Swap(inout float2 np1, inout float2 np2)
{
    float2 temp = np1;
    np1 = np2;
    np2 = temp;
}

void GenerateMainPoint(float2 p0, float2 p1, float2 p2, float width, out float2 np1, out float2 np2)
{
    np1 = 0;
    np2 = 0;

    float2 normal = GetNormal(p0, p1, p2);
    float2 p01normal = GetNormal(p0, p1);
    float2 p01 = p1 - p0;
    float2 p21 = p1 - p2;
    float sigma = sign(dot(p01 + p21, normal));
    float angle = dot(normalize(p01), normalize(p21));

    float2 xBasis = p2 - p1;
    float2 yBasis = GetNormal(p1, p2);
    float len = LineBaseWidth * width / max(MaxLineWidth,dot(normal, p01normal));
    float2 p = normal * (sigma==0?1:-sigma) * len;

    float2 t = float2(0, sigma>0?LineBaseWidth:-LineBaseWidth);
    np1 = (p1 + xBasis * t.x + yBasis * width * t.y);
    if(angle < 0.5f) np1 = (p1 - p);
    np2 = (p1 + p);

    if(sigma <= 0) Swap(np1, np2);
}

PSType GenerateVertex(float2 pos, float z, float2 uv)
{
    PSType p = (PSType)0;
    p.pos = mul(UNITY_MATRIX_P, float4(pos.xy,z,1));
    // p.pos = float4(pos.xy,z,1);
    p.uv = uv;
    return p;
}

void GenerateMainLine(inout TriangleStream<PSType> outStream, float3 p0, float3 p1, float3 p2, float3 p3, float2 width12, float2 uv12)
{
    float2 np1 = 0;
    float2 np2 = 0;
    float2 np3 = 0;
    float2 np4 = 0;

    GenerateMainPoint(p0, p1, p2, width12.x, np1, np2);
    GenerateMainPoint(p3, p2, p1, width12.y, np3, np4);

    //clock wise vertice for culling
    //   np1---np4
    //p1   | /  |   p2
    //   np2---np3
    outStream.Append(GenerateVertex(np1, p1.z, float2(uv12.x, 1)));
    outStream.Append(GenerateVertex(np4, p1.z, float2(uv12.y, 1)));
    outStream.Append(GenerateVertex(np2, p1.z, float2(uv12.x, 0)));
    outStream.Append(GenerateVertex(np3, p1.z, float2(uv12.y, 0)));
    outStream.RestartStrip();
}

void GenerateCornerPoint(inout TriangleStream<PSType> outStream, float2 p0, float2 p1, float2 p2, float width, float z, float2 uv12, int cornerDivision)
{
    float2 from = 0;
    float2 to = 0;
    float2 origin = 0;

    float2 normal = GetNormal(p0, p1, p2);
    float2 p01normal = GetNormal(p0, p1);
    float2 p01 = p1 - p0;
    float2 p21 = p1 - p2;
    float sigma = sign(dot(p01 + p21, normal));
    float angle = dot(normalize(p01), normalize(p21));

    if(sigma == 0 || angle < 0.5f) return;

    float2 xBasis = p2 - p1;
    float2 yBasis = GetNormal(p1, p2);
    float len = LineBaseWidth * width / max(MaxLineWidth,dot(normal, p01normal));
    origin = p1 - normal * sigma * len;

    float2 t = float2(0, sigma * LineBaseWidth);
    from = 0;
    to = (p1 + xBasis * t.x + yBasis * width * t.y);

    from = origin + sigma * normal * length(to - origin);

    if(sigma < 0) Swap(from, to);

    int res = cornerDivision;
    float2 prev = from;
    for(int i = 0 ; i < res; ++i)
    {
        float dt = 1.0f * i / (res-1);
        float2 dir = lerp(from, to, 1.0f * i / (res-1)) - origin;
        float flen = length(from-origin);
        float tlen = length(to-origin);
        float rlen = lerp(flen, tlen, dt);
        float2 np = origin + normalize(dir) * rlen;
        outStream.Append(GenerateVertex(origin, z, float2(uv12.x, sigma<0?0:1)));
        outStream.Append(GenerateVertex(prev, z, float2(uv12.x, sigma<0?1:0)));
        outStream.Append(GenerateVertex(np, z, float2(uv12.x, sigma<0?1:0)));
        outStream.RestartStrip();
        prev = np;
    }
}

void GenerateCorner(inout TriangleStream<PSType> outStream, float3 p0, float3 p1, float3 p2, float3 p3, float2 width, float2 uv12, int cornerDivision = 4)
{
    GenerateCornerPoint(outStream, p0, p1, p2, width.x, p1.z, uv12.xy, cornerDivision);
    GenerateCornerPoint(outStream, p3, p2, p1, width.y, p1.z, uv12.yx, cornerDivision);
}

