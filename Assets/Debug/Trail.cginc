
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

    if(sigma > 0)
    {
        float2 t1 = float2(0, LineBaseWidth);
        np1 = (p1 + xBasis * t1.x + yBasis * width * t1.y);
        np2 = (p1 + p);
    }
    else
    {
        float2 t2 = float2(0, -LineBaseWidth);
        np1 = (p1 + p);
        np2 = (p1 + xBasis * t2.x + yBasis * width * t2.y);
    }
}

PSType GenerateVertex(float2 pos, float z)
{
    PSType p = (PSType)0;
    p.pos = mul(UNITY_MATRIX_P, float4(pos.xy,z,1));
    return p;
}

void GenerateMainLine(inout TriangleStream<PSType> outStream, float3 p0, float3 p1, float3 p2, float3 p3, float width)
{
    float2 np1 = 0;
    float2 np2 = 0;
    float2 np3 = 0;
    float2 np4 = 0;

    GenerateMainPoint(p0, p1, p2, width, np1, np2);
    GenerateMainPoint(p3, p2, p1, width, np3, np4);

    //clock wise vertice for culling
    //1---4
    //| / |
    //2---3
    outStream.Append(GenerateVertex(np1, p1.z));
    outStream.Append(GenerateVertex(np4, p1.z));
    outStream.Append(GenerateVertex(np2, p1.z));
    outStream.Append(GenerateVertex(np3, p1.z));
    outStream.RestartStrip();
}

bool GenerateCornerPoint(float2 p0, float2 p1, float2 p2, float width, out float2 origin, out float2 from, out float2 to)
{
    from = to = origin = 0;

    float2 normal = GetNormal(p0, p1, p2);
    float2 p01normal = GetNormal(p0, p1);
    float2 p01 = p1 - p0;
    float2 p21 = p1 - p2;
    float sigma = sign(dot(p01 + p21, normal));

    if(sigma == 0) return false;

    float2 xBasis = p2 - p1;
    float2 yBasis = GetNormal(p1, p2);
    float len = LineBaseWidth * width / max(0.1,dot(normal, p01normal));
    origin = p1 - normal * sigma * len;

    float2 t = float2(0, sigma * LineBaseWidth);
    from = 0;
    to = (p1 + xBasis * t.x + yBasis * width * t.y);

    from = origin + sigma * normal * length(to - origin);

    if(sigma < 0) 
    {
        float2 temp = to;
        to = from;
        from = temp;
    }

    return true;
}

void GenerateCorner(inout TriangleStream<PSType> outStream, float3 p0, float3 p1, float3 p2, float3 p3, float width)
{

    float2 origin;
    float2 from;
    float2 to;
    if(GenerateCornerPoint(p0, p1, p2, width, origin, from, to))
    {
        outStream.Append(GenerateVertex(origin, p1.z));
        outStream.Append(GenerateVertex(from, p1.z));
        outStream.Append(GenerateVertex(to, p1.z));
        outStream.RestartStrip();
    }
    if(GenerateCornerPoint(p3, p2, p1, width, origin, from, to))
    {
        outStream.Append(GenerateVertex(origin, p1.z));
        outStream.Append(GenerateVertex(from, p1.z));
        outStream.Append(GenerateVertex(to, p1.z));
        outStream.RestartStrip();
    }
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

