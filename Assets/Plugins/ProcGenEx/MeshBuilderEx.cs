using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MathEx;

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
	}
}
