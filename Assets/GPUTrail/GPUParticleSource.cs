
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;
using UnityTools.Rendering;

namespace GPUTrail
{
	[StructLayout(LayoutKind.Sequential)]
	public class Particle : TrailSource
	{

	}
	public class GPUParticleSource : MonoBehaviour, ITrailSource<Particle>, IDataBuffer<Particle>
	{
		public enum Kernel
		{
			InitParticle,
			UpdateParticle,
		}
		public GPUBufferVariable<Particle> Buffer => this.particleBuffer;
		protected const int ParticleNum = 10240;
		[SerializeField] protected ComputeShader particleCS;
		protected GPUBufferVariable<Particle> particleBuffer = new GPUBufferVariable<Particle>("_ParticleBuffer", ParticleNum);
		protected ComputeShaderDispatcher<Kernel> dispatcher;

		protected void OnEnable()
		{
			this.dispatcher = new ComputeShaderDispatcher<Kernel>(this.particleCS);
			foreach(Kernel k in Enum.GetValues(typeof(Kernel)))
			{
				this.dispatcher.AddParameter(k, this.particleBuffer);
			}
			this.dispatcher.Dispatch(Kernel.InitParticle, ParticleNum);
		}
		protected void OnDisable()
		{
			this.particleBuffer?.Release();
		}

		protected void Update()
		{
			this.dispatcher.Dispatch(Kernel.UpdateParticle, ParticleNum);
		}
	}

}
