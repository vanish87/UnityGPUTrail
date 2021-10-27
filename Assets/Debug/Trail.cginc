
static const float LineBaseWidth = 0.5f;

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

    float2 xBasis = p2 - p1;
    float2 yBasis = GetNormal(p1, p2);
    float len = LineBaseWidth * width / max(0.1,dot(normal, p01normal));
    float2 p = normal * (sigma==0?1:-sigma) * len;

    float2 t = float2(0, sigma>0?LineBaseWidth:-LineBaseWidth);
    np1 = (p1 + xBasis * t.x + yBasis * width * t.y);
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

void GenerateMainLine(inout TriangleStream<PSType> outStream, float3 p0, float3 p1, float3 p2, float3 p3, float width, float2 uv12)
{
    float2 np1 = 0;
    float2 np2 = 0;
    float2 np3 = 0;
    float2 np4 = 0;

    GenerateMainPoint(p0, p1, p2, width, np1, np2);
    GenerateMainPoint(p3, p2, p1, width, np3, np4);

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

void GenerateCornerPoint(inout TriangleStream<PSType> outStream, float2 p0, float2 p1, float2 p2, float width, float z, float2 uv12)
{
    float2 from = 0;
    float2 to = 0;
    float2 origin = 0;

    float2 normal = GetNormal(p0, p1, p2);
    float2 p01normal = GetNormal(p0, p1);
    float2 p01 = p1 - p0;
    float2 p21 = p1 - p2;
    float sigma = sign(dot(p01 + p21, normal));

    if(sigma == 0) return;

    float2 xBasis = p2 - p1;
    float2 yBasis = GetNormal(p1, p2);
    float len = LineBaseWidth * width / max(0.1,dot(normal, p01normal));
    origin = p1 - normal * sigma * len;

    float2 t = float2(0, sigma * LineBaseWidth);
    from = 0;
    to = (p1 + xBasis * t.x + yBasis * width * t.y);

    from = origin + sigma * normal * length(to - origin);

    if(sigma < 0) Swap(from, to);

    outStream.Append(GenerateVertex(origin, z, float2(uv12.x, sigma<0?0:1)));
    outStream.Append(GenerateVertex(from, z, float2(uv12.x, sigma<0?1:0)));
    outStream.Append(GenerateVertex(to, z, float2(uv12.x, sigma<0?1:0)));
    outStream.RestartStrip();
}

void GenerateCorner(inout TriangleStream<PSType> outStream, float3 p0, float3 p1, float3 p2, float3 p3, float width, float2 uv12)
{
    GenerateCornerPoint(outStream, p0, p1, p2, width, p1.z, uv12.xy);
    GenerateCornerPoint(outStream, p3, p2, p1, width, p1.z, uv12.yx);
    
    // var res = 8;
    // foreach(var i in Enumerable.Range(0, res))
    // {
    //     var dt = 1.0f * i / (res-1);
    //     var dir = math.lerp(from, to, 1.0f * i / (res-1)) - origin;
    //     var flen = math.length(from-origin);
    //     var tlen = math.length(to-origin);
    //     var rlen = math.lerp(flen, tlen, dt);
    //     var np = origin + math.normalize(dir) * rlen;
    //     ret.Add(np);
    // }
    // ret.Add(to);
}

