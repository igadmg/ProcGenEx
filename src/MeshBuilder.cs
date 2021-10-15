using MathEx;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemEx;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEx;

namespace ProcGenEx
{
	public class TriangleGraph
	{
		public struct Node
		{
			public int i;
			public List<int> links;
		}

		Dictionary<int, Node> nodes = new Dictionary<int, Node>();

		public TriangleGraph(List<int> triangles)
		{
			var sides = new Dictionary<Tuple<int, int>, List<int>>();
			Action<Tuple<int, int>, int> sidesIn = (Tuple<int, int> p, int ti) => {
				List<int> tp;
				if (!sides.TryGetValue(p.Sort(), out tp))
				{
					tp = new List<int>(2);
					tp.Add(ti);
					sides.Add(p.Sort(), tp);
				}
				else
				{
					tp.Add(ti);
				}
			};
			Action<int, int> nodesIn = (int t0, int t1) => {
				Node n;
				if (!nodes.TryGetValue(t0, out n))
				{
					n = new Node();
					n.i = t0;
					n.links = new List<int>(3);
					n.links.Add(t1);
					nodes.Add(t0, n);
				}
				else
				{
					n.links.Add(t1);
					nodes[t0] = n;
				}
			};

			for (int i = 0; i < triangles.Count; i++)
			{
				var ta = triangles[i];
				var tb = triangles[i + 1];
				var tc = triangles[i + 2];

				sidesIn(Tuple.Create(ta, tb), i);
				sidesIn(Tuple.Create(tb, tc), i);
				sidesIn(Tuple.Create(tc, ta), i);
			}

			foreach (var link in sides.Values)
			{
				nodesIn(link[0], link[1]);
				nodesIn(link[1], link[0]);
			}
		}
	}

	public class MeshBuilder
	{
		public List<vec3> vertices = null;
		public List<vec3> normals = null;
		public List<vec2> uvs = null;
		public List<uint> triangles = null;

		public MeshBuilder()
			: this(0, 0)
		{
		}

		public MeshBuilder(int VertexCount, int TriangleCount)
		{
			vertices = new List<vec3>(VertexCount);
			normals = new List<vec3>(VertexCount);
			uvs = new List<vec2>(VertexCount);
			triangles = new List<uint>(TriangleCount * 3);
		}


		struct MeshVertex
		{
			public Vector3 position;
			public Vector2 normal;
			public Vector2 uv;
		}

		public Mesh ToMesh()
		{
			Mesh m = new Mesh();

			if (vertices.Count <= UInt16.MaxValue)
			{
				m.vertices = vertices.ConvertAll(v => v.ToVector3()).ToArray();
				m.normals = normals.ConvertAll(v => v.ToVector3()).ToArray();
				m.uv = uvs.ConvertAll(v => v.ToVector2()).ToArray();
				m.triangles = triangles.Select(i => (int)i).ToArray();
			}
			else /*if(vertices.Count <= UInt32.MaxValue)*/
			{
				m.indexFormat = IndexFormat.UInt32;

				var layout = new[] {
					new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
					new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 2),
					new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
				};

				m.SetVertexBufferParams(vertices.Count, layout);
				var verts = new NativeArray<MeshVertex>(vertices.Count, Allocator.Temp);
				for (int i = 0; i < vertices.Count; i++)
				{
					var vertex = new MeshVertex();
					vertex.position = vertices[i];
					if (i < normals.Count)
						vertex.normal = normals[i].ToVector3();
					if (i < uvs.Count)
						vertex.uv = uvs[i];
					verts[i] = vertex;
				}
				m.SetVertexBufferData(verts, 0, 0, vertices.Count);
				m.SetIndexBufferParams(triangles.Count, IndexFormat.UInt32);
				m.SetIndexBufferData(triangles, 0, 0, triangles.Count);

				m.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Count));
			}

			m.Apply();

			return m;
		}

		public void Grow(int VertexCount, int TriangleCount)
		{
			int dvc = Math.Max(vertices.Capacity, vertices.Count + VertexCount);
			int dtc = Math.Max(triangles.Capacity, triangles.Count + TriangleCount * 3);
			vertices.Capacity = dvc;
			normals.Capacity = dvc;
			uvs.Capacity = dvc;
			triangles.Capacity = dtc;
		}

		public void RecodeVertices(List<uint> newVertexIndices)
		{
			if (newVertexIndices.Count != vertices.Count)
			{
				return;
			}

			List<vec3> nvertices = new List<vec3>(new vec3[vertices.Count]);
			List<vec3> nnormals = new List<vec3>(new vec3[vertices.Count]);
			List<vec2> nuvs = new List<vec2>(new vec2[vertices.Count]);

			for (int i = 0; i < vertices.Count; i++)
			{
				nvertices.at(newVertexIndices[i], vertices[i]);
				nnormals.at(newVertexIndices[i], normals[i]);
				nuvs.at(newVertexIndices[i], uvs[i]);
			}
			vertices = nvertices;
			normals = nnormals;
			nuvs = uvs;

			for (int i = 0; i < triangles.Count; i++)
			{
				triangles[i] = newVertexIndices[i];
			}
		}

		#region Simple figures

		public uint[] AddTriangle(vec3 a, vec3 b, vec3 c)
		{
			uint[] result = new uint[3];
			return AddTriangle(a, b, c, ref result);
		}

		public uint[] AddTriangle(vec3 a, vec3 b, vec3 c, ref uint[] result)
		{
			var n = vec3.Cross((c - a), (a - b)).normalized;

			Grow(3, 1);
			result[0] = CreateVertex(a, n);
			result[1] = CreateVertex(b, n);
			result[2] = CreateVertex(c, n);

			MakeTriangle(result[0], result[1], result[2]);

			return result;
		}

		public uint[] AddTriangle(vec3[] v, vec2[] uv)
		{
			uint[] result = new uint[3];
			return AddTriangle(v, uv, ref result);
		}

		public uint[] AddTriangle(vec3[] v, vec2[] uv, ref uint[] result)
		{
			var n = vec3.Cross((v[2] - v[0]), (v[0] - v[1])).normalized;

			Grow(3, 1);
			result[0] = CreateVertex(v[0], n, uv[0]);
			result[1] = CreateVertex(v[1], n, uv[1]);
			result[2] = CreateVertex(v[2], n, uv[2]);

			MakeTriangle(result[0], result[1], result[2]);

			return result;
		}

		public uint[] AddQuad(vec3 a, vec3 b, vec3 c, vec3 d)
		{
			uint[] result = new uint[4];
			return AddQuad(a, b, c, d, ref result);
		}

		public uint[] AddQuad(vec3 a, vec3 b, vec3 c, vec3 d, ref uint[] result)
		{
			var n = vec3.Cross((c - a), (a - b)).normalized;

			Grow(4, 2);
			result[0] = CreateVertex(a, n);
			result[1] = CreateVertex(b, n);
			result[2] = CreateVertex(c, n);
			result[3] = CreateVertex(d, n);

			MakeQuad(result[0], result[1], result[2], result[3]);

			return result;
		}

		public uint[] AddQuad(vec3[] v, vec2[] uv)
		{
			uint[] result = new uint[4];
			return AddQuad(v, uv, ref result);
		}

		public uint[] AddQuad(vec3[] v, vec2[] uv, ref uint[] result)
		{
			var n = vec3.Cross((v[2] - v[0]), (v[0] - v[1])).normalized;

			Grow(4, 2);
			result[0] = CreateVertex(v[0], n, uv[0]);
			result[1] = CreateVertex(v[1], n, uv[1]);
			result[2] = CreateVertex(v[2], n, uv[2]);
			result[3] = CreateVertex(v[3], n, uv[3]);

			MakeQuad(result[0], result[1], result[2], result[3]);

			return result;
		}

		public uint[] AddStripe(curve<vec3> c, float width, vec3 up)
		{
			return AddStripe(c, new vec2(-width / 2f, width / 2f), up);
		}

		public uint[] AddStripe(curve<vec3> c, vec2 width, vec3 up)
		{
			return AddStripe(c, width
				, new Tuple<float, vec3>[] { Tuple.Create(0f, up), Tuple.Create(1f, up) }, Easing.makeEaseMirror(Easing.easeExpoOut)
				, 0, c.numberOfNodes, 1f);
		}

		public uint[] AddStripe(curve<vec3> c, vec2 width, vec3 up, float stepMultiplier)
		{
			return AddStripe(c, width
				, new Tuple<float, vec3>[] { Tuple.Create(0f, up), Tuple.Create(1f, up) }, Easing.makeEaseMirror(Easing.easeExpoOut)
				, 0, c.numberOfNodes, stepMultiplier);
		}

		public uint[] AddStripe(curve<vec3> c, vec2 width, Tuple<float, vec3>[] ups, Func<float, float> upEaseFn, int startNode, int endNode, float stepMultiplier)
		{
			List<uint> result = new List<uint>();

			float sl = c.length;
			float islsl = stepMultiplier / (sl * sl);
			result.Capacity = (int)(sl / 0.01f * 2 / stepMultiplier);

			int upi = 0;
			vec3 up = ups[upi].Item2;
			foreach (var i in c.Iterate(stepMultiplier))
			{
				float upa = i.t.InvLerp(ups[upi].Item1, ups[upi + 1].Item1);
				vec3 nup = upa.Lerp(ups[upi].Item2, ups[upi + 1].Item2);

				up = (1 - upEaseFn(upa)).Lerp(up, nup);
				vec3 forward = i.velocity.normalized;
				vec3 right = (up % forward).normalized;
				up = (i.velocity % right).normalized;

				if (i.t == 0)
				{
					Grow(2, 0);

					result.Add(CreateVertex(i.value + right * width.x, up));
					result.Add(CreateVertex(i.value + right * width.y, up));
				}
				else
				{
					Grow(2, 2);

					result.Add(CreateVertex(i.value + right * width.x, up));
					MakeTriangle(result[result.Count - 3], result[result.Count - 2], result[result.Count - 1]);

					result.Add(CreateVertex(i.value + right * width.y, up));
					MakeTriangle(result[result.Count - 2], result[result.Count - 3], result[result.Count - 1]);
				}
			}

			return result.ToArray();
		}

		#endregion

		public uint CreateVertex(vec3 v, vec3 n)
		{
			var vi = (uint)vertices.Count;

			vertices.Add(v);
			normals.Add(n);

			return vi;
		}

		public uint CreateVertex(vec3 v, vec3 n, vec2 u)
		{
			var vi = (uint)vertices.Count;

			vertices.Add(v);
			normals.Add(n);
			uvs.Add(u);

			return vi;
		}

		public uint CopyVertex(uint vi)
		{
			return CreateVertex(vertices.at(vi), normals.at(vi), uvs.at(vi));
		}

		public uint CopyVertex(uint vi, vec3 dv)
		{
			return CreateVertex(vertices.at(vi) + dv, normals.at(vi), uvs.at(vi));
		}

		public void MakeTriangle(uint a, uint b, uint c)
		{
			var ti = triangles.Count;

			triangles.Add(a);
			triangles.Add(b);
			triangles.Add(c);
		}

		public void MakeQuad(uint a, uint b, uint c, uint d)
		{
			MakeTriangle(a, b, c);
			MakeTriangle(a, c, d);
		}

		public void MakeFan(params uint[] ps)
		{
			for (int i = 1; i < ps.Length - 1; i++)
			{
				MakeTriangle(ps[0], ps[i], ps[i + 1]);
			}
		}

		public MeshBuilder Simplify(float eps = float.Epsilon)
		{
			uint vi = 0;
			var sortedVertices = vertices.Select(v => (v: v, i: vi++)).ToArray().Sort((a, b) => a.v.CompareTo(b.v, eps));

			int i = 0;
			var indexRemap = new Dictionary<uint, uint>();
			for (i = 0; i < sortedVertices.Length - 1; i++)
			{
				if (sortedVertices[i].v.Equals(sortedVertices[i + 1].v, eps))
				{
					indexRemap.Add(sortedVertices[i+1].i, indexRemap.Get(sortedVertices[i].i, sortedVertices[i].i));
				}
			}

			for (i = 0; i < triangles.Count; i++)
			{
				triangles[i] = indexRemap.Get(triangles[i], triangles[i]);
			}

			return this;
		}

		public void Extrude(uint[] contour, vec3 direction, int steps)
		{
			// light overgrow in triangle count expected.
			Grow(contour.Length * steps, contour.Length * steps * 2);

			vec3 dv = direction / steps;
			for (int si = 0; si < steps; si++)
			{
				var pv = CopyVertex(contour[0], dv);
				for (int i = 1; i < contour.Length; i++)
				{
					var cv = CopyVertex(contour[i], dv);

					MakeQuad(contour[i - 1], pv, cv, contour[i]);

					contour[i - 1] = pv;
					pv = cv;
				}
				contour[contour.Length - 1] = pv;
			}
		}

		public void Slice(Plane plane)
		{
			uint[] v2v = new uint[vertices.Count].Fill(uint.MaxValue);
			List<vec3> vs = vertices;
			List<vec3> ns = normals;
			List<vec2> us = uvs;
			List<uint> ts = triangles;

			vertices = new List<vec3>(vs.Count); ;
			normals = new List<vec3>(vs.Count);
			uvs = new List<vec2>(vs.Count);
			triangles = new List<uint>(ts.Count);


			for (int i = 0; i < vs.Count; i++)
			{
				if (plane.GetDistanceToPoint(MathEx.Convert.ToVector3(vs[i])) >= 0)
				{
					v2v[i] = CreateVertex(vs[i], ns[i], us[i]);
				}
			}


			for (int i = 0; i < ts.Count; i += 3)
			{
				int st = ((v2v[ts[i]] == uint.MaxValue ? 0 : 1) << 0) + ((v2v[ts[i + 1]] == uint.MaxValue ? 0 : 1) << 1) + ((v2v[ts[i + 2]] == uint.MaxValue ? 0 : 1) << 2);

				if (st == 0)
					continue;
				if (st == 7)
				{
					MakeTriangle(v2v[ts[i]], v2v[ts[i + 1]], v2v[ts[i + 2]]);
					continue;
				}

				vec3 ab, bc, ca;
				float abd, bcd, cad;
				bool abi = plane.Intersect(vs.at(ts[i]), vs.at(ts[i + 1]), out ab, out abd);
				bool bci = plane.Intersect(vs.at(ts[i + 1]), vs.at(ts[i + 2]), out bc, out bcd);
				bool cai = plane.Intersect(vs.at(ts[i + 2]), vs.at(ts[i]), out ca, out cad);


				List<uint> nvs = new List<uint>(4);

				if (!(v2v[ts[i]] == uint.MaxValue))
				{
					nvs.Add(v2v[ts[i]]);
				}
				if (abi && (abd > 0 && abd < 1))
				{
					nvs.Add(CreateVertex(ab, abd.Slerp(ns.at(ts[i]), ns.at(ts[i + 1]))));
				}

				if (!(v2v[ts[i + 1]] == uint.MaxValue))
				{
					nvs.Add(v2v[ts[i + 1]]);
				}
				if (bci && (bcd > 0 && bcd < 1))
				{
					nvs.Add(CreateVertex(bc, bcd.Slerp(ns.at(ts[i + 1]), ns.at(ts[i + 2]))));
				}

				if (!(v2v[ts[i + 2]] == uint.MaxValue))
				{
					nvs.Add(v2v[ts[i + 2]]);
				}
				if (cai && (cad > 0 && cad < 1))
				{
					nvs.Add(CreateVertex(ca, cad.Slerp(ns.at(ts[i + 2]), ns.at(ts[i]))));
				}

				MakeFan(nvs.ToArray());
			}
		}

		public int[] Select(Plane plane)
		{
			List<int> result = new List<int>(vertices.Count);

			for (int i = 0; i < vertices.Count; i++)
			{
				if (plane.GetDistanceToPoint(MathEx.Convert.ToVector3(vertices[i])) >= 0)
				{
					result.Add(i);
				}
			}

			return result.ToArray();
		}

		public List<int> Select(Plane plane, out List<int> sidea, out List<int> sideb)
		{
			sidea = new List<int>(vertices.Count);
			sideb = new List<int>(vertices.Count);

			for (int i = 0; i < vertices.Count; i++)
			{
				if (plane.GetDistanceToPoint(MathEx.Convert.ToVector3(vertices[i])) >= 0)
				{
					sidea.Add(i);
				}
				else
					sideb.Add(i);
			}

			return sidea;
		}

		public List<int> Select(ray r, float radius, out List<int> sidea)
		{
			sidea = new List<int>(vertices.Count);

			for (int i = 0; i < vertices.Count; i++)
			{
				if (r.distance(vertices[i]) < radius)
				{
					sidea.Add(i);
				}
			}

			return sidea;
		}

		public List<int> Select(ray r, float radius, out List<int> sidea, out List<int> sideb)
		{
			sidea = new List<int>(vertices.Count);
			sideb = new List<int>(vertices.Count);

			for (int i = 0; i < vertices.Count; i++)
			{
				if (r.distance(vertices[i]) < radius)
				{
					sidea.Add(i);
				}
				else
					sideb.Add(i);
			}

			return sidea;
		}

		public void SmoothNormals()
		{
			for (int i = 0; i < normals.Count; i++)
				normals[i] = vec3.empty;

			for (int i = 0; i < triangles.Count; i += 3)
			{
				var ai = triangles[i];
				var bi = triangles[i + 1];
				var ci = triangles[i + 2];

				vec3 a = vertices.at(ai);
				vec3 b = vertices.at(bi);
				vec3 c = vertices.at(ci);

				vec3 n = ((b - a) % (c - a)).normalized;

				normals.at(ai, normals.at(ai).isEmpty ? n : (normals.at(ai) + n) / 2f);
				normals.at(bi, normals.at(bi).isEmpty ? n : (normals.at(bi) + n) / 2f);
				normals.at(ci, normals.at(ci).isEmpty ? n : (normals.at(ci) + n) / 2f);
			}
		}

		public int[] Project(Plane plane)
		{
			List<int> contour = new List<int>();



			return contour.ToArray();
		}

		public void UVMapPlane(Plane plane, vec3 forward, int[] vs)
		{
			UVMapPlane(plane, forward, aabb2.one, vs);
		}

		public void UVMapPlane(Plane plane, vec3 forward, aabb2 uvrect, int[] vs)
		{
			vec2[] pvs = new vec2[vs.Length];

			aabb2 b = aabb2.empty;
			Quaternion q = Quaternion.LookRotation(MathEx.Convert.ToVector3(forward), plane.normal);
			for (int i = 0; i < vs.Length; i++)
			{
				pvs[i] = MathEx.Convert.ToVec3(q * MathEx.Convert.ToVector3(vertices[vs[i]])).xz();
				b = b.Extend(pvs[i]);
			}

			Debug.Log(b);
			for (int i = 0; i < vs.Length; i++)
			{
				uvs[vs[i]] = uvrect.a + uvrect.size.Mul((pvs[i] - b.a).Div(b.size));
				Debug.Log(uvs[vs[i]]);
			}
		}
	}
}
