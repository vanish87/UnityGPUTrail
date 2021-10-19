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
            2,1,3,
        };

		[SerializeField] protected float width = 1;
		[SerializeField] protected bool drawPoint = false;
		protected float2 p0;
		protected float2 p1;
		protected float2 p2;
		protected float2 p3;

		protected void OnEnable()
		{
			this.p0 = this.points[0];
			this.p1 = this.points[1];
			this.p2 = this.points[2];
			this.p3 = this.points[3];
		}
        protected void Update()
        {


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

            if(math.sign(pos.y) == -sigma)
            {
                var p = 0.5f * normal * -sigma * this.width / math.dot(normal, p01Normal);
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
                this.DrawLine(this.points[i], this.points[i+1], this.points[i+2], this.points[i+3]);
            }
		}
	}
}
