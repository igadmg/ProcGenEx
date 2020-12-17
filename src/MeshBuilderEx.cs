using MathEx;
using System;
using System.Collections.Generic;
using SystemEx;
using UnityEngine;

namespace ProcGenEx
{
	class EdgeVertexMap
	{
		Dictionary<Edge, List<uint>> vertices = new Dictionary<Edge, List<uint>>();

		public void Add(Edge edge, uint v)
		{
			vertices.GetOrAdd(edge, () => new List<uint>()).Add(v);
		}

		public uint[] Get(Edge edge)
		{
			return vertices.Get(edge, vertices.Get(edge.Reverse())?.Reversed())?.ToArray();
		}
	}

	public static class MeshBuilderEx
	{
		public static MeshBuilder Rotate(this MeshBuilder mb, Quaternion rotation)
		{
			for (int i = 0; i < mb.vertices.Count; i++)
			{
				mb.vertices[i] = (rotation * mb.vertices[i].ToVector3()).ToVec3();
				mb.normals[i] = (rotation * mb.normals[i].ToVector3()).ToVec3();
			}

			return mb;
		}

		public static MeshBuilder Stretch(this MeshBuilder mb, aabb3 newSize)
			=> mb.Stretch(new aabb3(vec3.one / 2.0f), newSize);

		public static MeshBuilder Stretch(this MeshBuilder mb, aabb3 oldSize, aabb3 newSize)
		{
			var scale = newSize.size / oldSize.size;

			for (int i = 0; i < mb.vertices.Count; i++)
			{
				var v = mb.vertices[i];
				mb.vertices[i] = newSize.a + (v - oldSize.a) * scale;
			}

			return mb;
		}

		public static MeshBuilder Sphere(this MeshBuilder mb, float radius = 1.0f)
		{
			for (int i = 0; i < mb.vertices.Count; i++)
			{
				var v = mb.vertices[i];
				v.length = radius;
				mb.vertices[i] = v;
			}

			return mb;
		}

		public static MeshBuilder Hole(this MeshBuilder mb, uint vertex)
		{
			uint sparevertex = vertex;
			for (int i = 0; i < mb.triangles.Count; i += 3)
			{
				var vi = mb.triangles.IndexOf(vertex);
				if (vi < 0) continue;

				if (sparevertex == uint.MaxValue)
					sparevertex = mb.CopyVertex(vertex);
				mb.triangles[vi] = sparevertex;
				sparevertex = uint.MaxValue;
			}

			return mb;
		}

		public static MeshBuilder Cut(this MeshBuilder mb, params int[] vs)
		{
			for (int i = 2; i < vs.Length; i++)
			{

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
			for (int i = 0; i < mb.vertices.Count; i++)
			{
				vec3 v = mb.vertices[i];
				v.Mul(new vec3(noiseDensity, noiseDensity, noiseDensity));
				float noise1 = Noise.GetOctaveNoise(v.x + random, v.y + random, v.z + random, 4) * scale;
				float factor = 1f - (scale / 2f) + noise1;
				mb.vertices[i] = mb.vertices[i].Mul(new vec3(factor, factor, factor));
			}

			return mb;
		}

		public static MeshBuilder Subdivide(this MeshBuilder mesh, int steps = 1)
		{
			var edgeVertexMap = new EdgeVertexMap();

			int icount = mesh.triangles.Count;
			for (int i = 0; i < icount; i += 3)
			{
				var ta = mesh.triangles[i];
				var tb = mesh.triangles[i + 1];
				var tc = mesh.triangles[i + 2];
				var va = mesh.vertices.at(ta);
				var vb = mesh.vertices.at(tb);
				var vc = mesh.vertices.at(tc);

				Func<int, int> newVerticesNumFn = n => n.Asum(2) + n;
				Func<int, int> newTrianglesNumFn = n => n.Asum(3, 2);
				int newVerticesNum = newVerticesNumFn(steps);
				int newTrianglesNum = newTrianglesNumFn(steps);

				mesh.Grow(newVerticesNum, newTrianglesNum);
				uint[] newVertices = new uint[newVerticesNum];
				int nvi = 0;

				// Generate new vertices.
				{
					var eA = new Edge(ta, tc);
					var eAv = edgeVertexMap.Get(eA);
					var eB = new Edge(ta, tb);
					var eBv = edgeVertexMap.Get(eB);
					var eC = new Edge(tc, tb);
					var eCv = edgeVertexMap.Get(eC);

					for (int j = 0; j < steps; j++)
					{
						float a = (j + 1) * 1.0f / (steps + 1);
						var vaj = a.Lerp(va, vc);
						var vbj = a.Lerp(va, vb);
						int ke = 2 + j;

						if (eAv == null)
						{
							var nv = mesh.CreateVertex(vaj, vaj);
							edgeVertexMap.Add(eA, nv);
							newVertices[nvi++] = nv;
						}
						else
						{
							newVertices[nvi++] = eAv[j];
						}

						for (int k = 1; k < ke - 1; k++)
						{
							float ak = k * 1.0f / (ke - 1);
							var nv = ak.Lerp(vaj, vbj);
							newVertices[nvi++] = mesh.CreateVertex(nv, nv);
						}

						if (eBv == null)
						{
							var nv = mesh.CreateVertex(vbj, vbj);
							edgeVertexMap.Add(eB, nv);
							newVertices[nvi++] = nv;
						}
						else
						{
							newVertices[nvi++] = eBv[j];
						}
					}
					{
						if (eCv == null)
						{
							for (int k = 0; k < steps; k++)
							{
								float ak = (k + 1) * 1.0f / (steps + 1);
								var nv = ak.Lerp(vc, vb);
								var nvv = mesh.CreateVertex(nv, nv);
								edgeVertexMap.Add(eC, nvv);
								newVertices[nvi++] = nvv;
							}
						}
						else
						{
							for (int k = 0; k < steps; k++)
							{
								newVertices[nvi++] = eCv[k];
							}
						}
					}
				}

				// Convert original triangle to top triangle
				mesh.triangles[i] = ta;
				mesh.triangles[i + 1] = newVertices[1];
				mesh.triangles[i + 2] = newVertices[0];

				// build the middle section
				for (int j = 0; j < steps - 1; j++)
				{
					int ti = j.Asum(2);
					int tj = (j + 1).Asum(2);
					int tk = (j + 2).Asum(2);

					for (int k = 0; k < (tj - ti - 1); k++)
					{
						mesh.MakeTriangle(newVertices[ti + k], newVertices[tj + k + 1], newVertices[tj + k]);
						mesh.MakeTriangle(newVertices[ti + k], newVertices[ti + k + 1], newVertices[tj + k + 1]);
					}
					mesh.MakeTriangle(newVertices[tj - 1], newVertices[tk - 1], newVertices[tk - 2]);
				}

				// build last row
				{
					int ti = (steps - 1).Asum(2);
					int tj = steps.Asum(2);

					for (int k = 0; k < (tj - ti - 1); k++)
					{
						if (k == 0)
						{
							mesh.MakeTriangle(newVertices[ti], newVertices[tj], tc);
						}
						else
						{
							mesh.MakeTriangle(newVertices[ti + k], newVertices[tj + k], newVertices[tj + k - 1]);
						}
						mesh.MakeTriangle(newVertices[ti + k], newVertices[ti + k + 1], newVertices[tj + k]);
					}

					mesh.MakeTriangle(newVertices[tj - 1], tb, newVertices[newVerticesNum - 1]);
				}
			}

			return mesh;
		}

		public static float Radius(this MeshBuilder mb)
		{
			float r = 0;
			for (int i = 0; i < mb.vertices.Count; i++)
			{
				float vm = mb.vertices[i].magnitude;
				if (r < vm)
					r = vm;
			}

			return Mathf.Sqrt(r);
		}
	}
}
