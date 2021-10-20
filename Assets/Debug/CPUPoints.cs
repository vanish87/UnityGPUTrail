using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace GPUTrail
{
    [ExecuteInEditMode]
	public class CPUPoints : MonoBehaviour
	{
		[SerializeField] protected List<float2> points = new List<float2>();
        [SerializeField] protected List<float2> linePoints = new List<float2>()
        {
            new float2(0,  0.5f),
            new float2(0, -0.5f),
            new float2(1,  0.5f),
            new float2(1, -0.5f),
        };
        [SerializeField] protected List<int> lineIndex = new List<int>()
        {
            0,1,2,
            // 2,1,3,
        };

		[SerializeField] protected float width = 1;
		[SerializeField] protected bool drawPoint = false;

		protected void OnEnable()
		{
			// this.p0 = this.points[0];
			// this.p1 = this.points[1];
			// this.p2 = this.points[2];
			// this.p3 = this.points[3];
		}
        protected void Update()
        {


        }

        public bool debug = false;

        protected void DrawLine(float2 a, float2 b, Color c)
        {
            var old = Gizmos.color;
            Gizmos.color = c;
			Gizmos.DrawLine(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0));
			// UnityEditor.Handles.DrawDottedLine(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0), 10f);
            Gizmos.color = old;
        }
        protected float2 GetNormal(float2 a, float2 b, float2 c)
        {
			var tangent = math.normalize(math.normalize(c - b) + math.normalize(b - a));
			return new float2(-tangent.y, tangent.x);
        }
        protected float2 GetNormal(float2 a, float2 b)
        {
            var line = math.normalize(b-a);
			return new float2(-line.y, line.x);
        }
        protected List<float2> GeneratePoints(float2 a, float2 b, float2 c, float2 d)
        {
			var ret = new List<float2>();
            var lnormal = GetNormal(a, b, c);
            var rnormal = GetNormal(b, c, d);

			var abnormal = GetNormal(a, b);
			var cdnormal = GetNormal(c, d);


            return ret;
        }

        protected List<float2> GenerateCircle(float2 origin, float radius, float2 from, float2 to, int resolution = 8)
        {
			var ret = new List<float2>();
            resolution = math.max(1, resolution);
			foreach (var i in Enumerable.Range(0, resolution))
			{
				var dir = math.normalize(math.lerp(from, to, i * 1.0f / resolution));
                ret.Add(origin + dir * radius);
			}

			return ret;
        }
        protected List<float2> GetGeneratedCirclePoints(float2 p0, float2 p1, float2 p2)
        {
            var ret = new List<float2>();
            var normal = GetNormal(p0, p1, p2);
			var p01normal = GetNormal(p0, p1);
			var p01 = p1 - p0;
			var p21 = p1 - p2;
            var sigma = math.sign(math.dot(p01 + p21, normal));

            if(sigma == 0) return ret;

            var xBasis = p2 - p1;
            var yBasis = GetNormal(p1, p2);
			var len = 0.5f * this.width / math.dot(normal, p01normal);
			var p = normal * sigma * len;
            var org = p1 + p;
            var to = p1 - p;

            ret.Add(org);

            if(sigma > 0)
            {
                var t1 = new float2(0, 0.5f);
                // var t2 = new float2(0, -0.5f);

				var from = (p1 + xBasis * t1.x + yBasis * this.width * t1.y) - org;
                var circle = this.GenerateCircle(org, len, from, to);
                ret.AddRange(circle);
            }
            else
            {
                // var t1 = new float2(0, 0.5f);
                var t2 = new float2(0, -0.5f);

				var from = (p1 + xBasis * t2.x + yBasis * this.width * t2.y) - org;
                var circle = this.GenerateCircle(org, len, from, to);
                ret.AddRange(circle);
            }

            return ret;
        }

        protected List<float2> GetGeneratedPoints(float2 p0, float2 p1, float2 p2)
        {

            var ret = new List<float2>();
            var normal = GetNormal(p0, p1, p2);
			var p01normal = GetNormal(p0, p1);
			var p01 = p1 - p0;
			var p21 = p1 - p2;
            var sigma = math.sign(math.dot(p01 + p21, normal));

            if(sigma == 0) return ret;

            var xBasis = p2 - p1;
            var yBasis = GetNormal(p1, p2);
			var len = 0.5f * this.width / math.dot(normal, p01normal);
			var p = normal * sigma * len;
            var origin = p1 - p;
            var to = float2.zero;


            if(sigma > 0)
            {
                var t1 = new float2(0, 0.5f);
                // var t2 = new float2(0, -0.5f);

				to =(p1 + xBasis * t1.x + yBasis * this.width * t1.y);
            }
            else
            {
                // var t1 = new float2(0, 0.5f);
                var t2 = new float2(0, -0.5f);

				to = (p1 + xBasis * t2.x + yBasis * this.width * t2.y);
            }

            // var from = p1 + normal * sigma * 0.5f * this.width;
            // var from = p1 + p;
			var from = origin + sigma * normal * math.length(to - origin);

            ret.Add(origin);
            ret.Add(from);
            var res = 8;
            foreach(var i in Enumerable.Range(0, res))
            {
                var dt = 1.0f * i / (res-1);
                var dir = math.lerp(from, to, 1.0f * i / (res-1)) - origin;
                var flen = math.length(from-origin);
                var tlen = math.length(to-origin);
                var rlen = math.lerp(flen, tlen, dt);
                var np = origin + math.normalize(dir) * rlen;
                ret.Add(np);
            }
            ret.Add(to);

            return ret;
        }

        
        protected List<float2> GetModifiedPoints(float2 p0, float2 p1, float2 p2)
        {
            var ret = new List<float2>();
            var normal = GetNormal(p0, p1, p2);
			var p01normal = GetNormal(p0, p1);
			var p01 = p1 - p0;
			var p21 = p1 - p2;
            var sigma = math.sign(math.dot(p01 + p21, normal));

            var xBasis = p2 - p1;
            var yBasis = GetNormal(p1, p2);
			var len = 0.5f * this.width / math.dot(normal, p01normal);
			var p = normal * (sigma==0?1:-sigma) * len;

            if(sigma > 0)
            {
                var t1 = new float2(0, 0.5f);
                // var t2 = new float2(0, -0.5f);

				ret.Add(p1 + xBasis * t1.x + yBasis * this.width * t1.y);
                //modify lower one
				ret.Add(p1 + p);
            }
            else
            {
                // var t1 = new float2(0, 0.5f);
                var t2 = new float2(0, -0.5f);

                //modify upper one
				ret.Add(p1 + p);
				ret.Add(p1 + xBasis * t2.x + yBasis * this.width * t2.y);
            }
            return ret;
        }

        protected void DrawGenPoints(List<float2> genPoints)
        {
			if (genPoints.Count > 1)
			{
                foreach(var i in Enumerable.Range(0, genPoints.Count-1))
                {
                    this.DrawLine(genPoints[i], genPoints[i+1], Color.green);

                }
				this.DrawLine(genPoints[genPoints.Count-1], genPoints[0], Color.green);
			}
		}

        protected List<float2> GetPoints(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            var ret = new List<float2>();
            ret.AddRange(this.GetModifiedPoints(p0, p1, p2));
            ret.AddRange(this.GetModifiedPoints(p3, p2, p1));

            var genPoints = this.GetGeneratedPoints(p0, p1, p2);
            this.DrawGenPoints(genPoints);
            genPoints = this.GetGeneratedPoints(p3, p2, p1);
            this.DrawGenPoints(genPoints);

            // var genPoints = this.GetGeneratedCirclePoints(p0, p1, p2);
			// if (genPoints.Count > 1) foreach (var i in Enumerable.Range(0, genPoints.Count - 1)) this.DrawLine(genPoints[i], genPoints[i + 1], Color.blue);
            // genPoints = this.GetGeneratedCirclePoints(p3, p2, p1);

            return ret;
        }

		protected float2 GetModifedPoint(float2 a, float2 b, float2 c, float2 d, float2 pos)
        {
            var p0 = a;
            var p1 = b;
            var p2 = c;
            if(pos.x == 1)
            {
                p0 = d;
                p1 = c;
                p2 = b;
                pos = new float2(1-pos.x, -pos.y);
            }

			var tangent = math.normalize(math.normalize(p2 - p1) + math.normalize(p1 - p0));
			var normal = new float2(-tangent.y, tangent.x);
			var p01 = p1 - p0;
			var p21 = p1 - p2;
            var p01Normal = math.normalize(new float2(-p01.y, p01.x));
            var sigma = math.sign(math.dot(p01 + p21, normal));

            this.DrawLine(p1, p1 + normal, Color.red);
            this.DrawLine(p1, p1 + p01Normal, Color.green);

            if(math.sign(pos.y) == -sigma && debug)
            {
                var p = 0.5f * normal * -sigma * this.width / math.dot(normal, p01Normal);
                this.DrawLine(p1, p1 + p, Color.blue);

                // this.DrawLine(p1 + p, p1 - p, Color.blue);
                return p1 + p;
            }
            else
            {
                var xBasis = p2 - p1;
                var yBasis = math.normalize(new float2(-xBasis.y, xBasis.x));
				return p1 + xBasis * pos.x + yBasis * this.width * pos.y;
            }
        }

        protected void DrawLine(float2 a, float2 b, float2 c, float2 d)
        {
			foreach (var i in Enumerable.Range(0, this.lineIndex.Count / 3))
			{
				var p0 = this.linePoints[i];
				var p1 = this.linePoints[i + 1];
				var p2 = this.linePoints[i + 2];

				var wp0 = this.GetModifedPoint(a,b,c,d,p0);
				var wp1 = this.GetModifedPoint(a,b,c,d,p1);
				var wp2 = this.GetModifedPoint(a,b,c,d,p2);

				Gizmos.DrawLine(new Vector3(wp0.x, wp0.y, 0), new Vector3(wp1.x, wp1.y, 0));
				Gizmos.DrawLine(new Vector3(wp1.x, wp1.y, 0), new Vector3(wp2.x, wp2.y, 0));
				Gizmos.DrawLine(new Vector3(wp2.x, wp2.y, 0), new Vector3(wp0.x, wp0.y, 0));
			}

            var resolution = 8;
            foreach(var r in Enumerable.Range(0,resolution))
            {
                var tangent = math.normalize(math.normalize(c - b) + math.normalize(b - a));
                var normal = new float2(-tangent.y, tangent.x);
                var p01 = b - a;
                var p21 = b - c;
                var p01Normal = math.normalize(new float2(-p01.y, p01.x));
                var p1Normal = math.normalize(new float2(-p01.y, p01.x));
                var sigma = math.sign(math.dot(p01 + p21, normal));

                // if(sigma > 0)
                {
                    var radius = 0.5f * this.width / math.dot(normal, p01Normal);
					var p = normal * -sigma * radius;
					var miter = b - p;
					var center = b + p;
					var pleft = b - p01Normal * 0.5f * this.width;
					var pright = b - p01Normal * 0.5f * this.width;
                    // Gizmos.DrawLine()

                }
            }
        }

		protected void OnDrawGizmos()
		{
            if(this.drawPoint)
            {
				foreach (var p in this.points)
				{
					Gizmos.DrawWireSphere(new Vector3(p.x, p.y, 0), 0.1f);
				}
            }

			foreach(var i in Enumerable.Range(0, this.points.Count-3))
			{
			    // this.DrawLine(this.points[i], this.points[i+1], this.points[i+2], this.points[i+3]);
			    this.DrawTriangle(this.points[i], this.points[i+1], this.points[i+2], this.points[i+3]);
			}
			// this.DrawTriangle(this.points[0], this.points[1], this.points[2], this.points[3]);
			// this.DrawTriangle(this.points[3], this.points[2], this.points[1], true);
		}
        
        protected void DrawTriangle(float2 a, float2 b, float2 c, float2 d)
        {
            var triangles = this.GetPoints(a, b, c, d);
            this.DrawLine(triangles[0], triangles[1], Color.cyan);
            this.DrawLine(triangles[1], triangles[2], Color.cyan);
            this.DrawLine(triangles[2], triangles[3], Color.cyan);
            this.DrawLine(triangles[3], triangles[0], Color.cyan);
        }
	}
}
