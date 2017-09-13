using MathEx;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenEx
{
	public static class MeshBuilderEx
	{
		public static MeshBuilder Rotate(this MeshBuilder mb, Quaternion rotation)
		{
			for (int i = 0; i < mb.vertices.Count; i++) {
				mb.vertices[i] = (rotation * mb.vertices[i].ToVector3()).ToVec3();
				mb.normals[i] = (rotation * mb.normals[i].ToVector3()).ToVec3();
			}

			return mb;
		}

		public static MeshBuilder Hole(this MeshBuilder mb, int vertex)
		{
			int sparevertex = vertex;
			for (int i = 0; i < mb.triangles.Count; i+=3) {
				var vi = mb.triangles.IndexOf(vertex);
				if (vi < 0) continue;

				if (sparevertex == -1)
					sparevertex = mb.CopyVertex(vertex);
				mb.triangles[vi] = sparevertex;
				sparevertex = -1;
			}

			return mb;
		}

		public static MeshBuilder Cut(this MeshBuilder mb, params int[] vs)
		{
			for (int i = 2; i < vs.Length; i++) {

			}

			return mb;
		}

		public static MeshBuilder Extrude(this MeshBuilder mb, ray r, float radius, vec3 force)
		{
			List<int> inside =
			mb.Select(r, radius, out inside);

			return mb;
		}

		public static MeshBuilder Noize(this MeshBuilder mb, float random, float noiseDensity = 2f, float scale = .75f)
		{
			for (int i = 0; i < mb.vertices.Count; i++) {
				vec3 v = mb.vertices[i];
				v.Mul(new vec3(noiseDensity, noiseDensity, noiseDensity));
				float noise1 = Noise.GetOctaveNoise(v.x + random, v.y + random, v.z + random, 4) * scale;
				float factor = 1f - (scale / 2f) + noise1;
				mb.vertices[i] = mb.vertices[i].Mul(new vec3(factor, factor, factor));
			}

			return mb;
		}

		public static float Radius(this MeshBuilder mb)
		{
			float r = 0;
			for (int i = 0; i < mb.vertices.Count; i++) {
				float vm = mb.vertices[i].magnitude;
				if (r < vm)
					r = vm;
			}

			return Mathf.Sqrt(r);
		}
	}
}
