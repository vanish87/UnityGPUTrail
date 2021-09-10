using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools;
using UnityTools.Common;

namespace GPUTrail
{
	[ExecuteInEditMode]
	public class TrailLineDebug : MonoBehaviour
	{
		[System.Serializable]
		public class TrailData : GPUContainer
		{
			[Shader(Name = "_Thiness")] public float thiness = 0.1f;
			[Shader(Name = "_MiterLimit")] public float miterLimit = 0.75f;
			[Shader(Name = "_zScale")] public bool zScale = true;
			[Shader(Name = "_TrailData")] public GPUBufferVariable<float4> trailGPUData = new GPUBufferVariable<float4>();
			
			public List<Gradient> gradients = new List<Gradient>();
			[Shader(Name = "_TrailGradientTexture")] public Texture2D gradientTexture;
			[Shader(Name = "_TrailGradientTextureHeight")] public int gradientTextureHeight;
		}
		[SerializeField] protected List<float4> trailPos = new List<float4>();
		[SerializeField] protected Shader shader;
		[SerializeField] protected TrailData trailData = new TrailData();
		protected DisposableMaterial material;

		protected void UpdateBuffer()
		{
			if (this.trailPos.Count != this.trailData.trailGPUData.Size) this.trailData.trailGPUData.InitBuffer(this.trailPos.Count, true);
			foreach (var i in Enumerable.Range(0, this.trailData.trailGPUData.Size))
			{
				this.trailData.trailGPUData.CPUData[i] = this.trailPos[i];
			}
		}
		protected void UpdateGradient()
		{
			var tex = this.trailData.gradientTexture;
			if (tex == null || tex.height != this.trailData.gradients.Count)
			{
				this.trailData.gradientTexture?.DestoryObj();
				this.trailData.gradientTexture = PalletTexture.GenerateGradientTexture(this.trailData.gradients);
			} 
			else 
			{
				PalletTexture.UpdateGradientTexture(this.trailData.gradientTexture, this.trailData.gradients);
			}

			this.trailData.gradientTextureHeight = this.trailData.gradientTexture.height;
		}
		protected void OnEnable()
		{
			this.material = new DisposableMaterial(this.shader);
		}
		protected void OnDisable()
		{
			this.material?.Dispose();
			this.trailData?.Release();
		}

		protected void Update()
		{
			this.UpdateBuffer();
			this.UpdateGradient();

			Material mat = this.material;
			this.trailData.UpdateGPU(mat);
			var b = new Bounds(Vector3.zero, Vector3.one * 10000);
			Graphics.DrawProcedural(material, b, MeshTopology.Points, this.trailData.trailGPUData.Size);
		}
		protected void OnDrawGizmos()
		{
			foreach (var p in this.trailPos)
			{
				Gizmos.DrawWireSphere(new Vector3(p.x, p.y, p.z), 0.1f);
			}
		}
	}
}
