using MathEx;
using System;
using System.Collections.Generic;
using SystemEx;
using UnityEngine;
using UnityEngineEx;

namespace ProcGenEx
{
	public class MeshBuilder
	{
		public List<Vector3> vertices = null;
		public List<Vector3> normals = null;
		public List<Vector2> uvs = null;
		public List<int> triangles = null;

		public MeshBuilder(int VertexCount, int TriangleCount)
		{
			vertices = new List<Vector3>(VertexCount);
			normals = new List<Vector3>(VertexCount);
			uvs = new List<Vector2>(VertexCount);
			triangles = new List<int>(TriangleCount * 3);
		}

		public Mesh ToMesh()
		{
			Mesh m = new Mesh();

			m.vertices = vertices.ToArray();
			m.normals = normals.ToArray();
			m.uv = uvs.ToArray();
			m.triangles = triangles.ToArray();

			m.Finalize();
			
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

#region Simple figures

		public int[] AddTriangle(Vector3 a, Vector3 b, Vector3 c)
		{
			int[] result = null;
			return AddTriangle(a, b, c, ref result);
		}

		public int[] AddTriangle(Vector3 a, Vector3 b, Vector3 c, ref int[] result)
		{		
			if (result == null)
				result = new int[3];

			var n = Vector3.Cross((c - a), (a - b)).normalized;

			Grow(3, 1);
			result[0] = CreateVertex(a, n);
			result[1] = CreateVertex(b, n);
			result[2] = CreateVertex(c, n);

			MakeTriangle(result[0], result[1], result[2]);

			return result;
		}

		public int[] AddTriangle(Vector3[] v, Vector2[] uv)
		{
			int[] result = null;
			return AddTriangle(v, uv, ref result);
		}

		public int[] AddTriangle(Vector3[] v, Vector2[] uv, ref int[] result)
		{
			if (result == null)
				result = new int[3];
			
			var n = Vector3.Cross((v[2] - v[0]), (v[0] - v[1])).normalized;

			Grow(3, 1);
			result[0] = CreateVertex(v[0], n, uv[0]);
			result[1] = CreateVertex(v[1], n, uv[1]);
			result[2] = CreateVertex(v[2], n, uv[2]);

			MakeTriangle(result[0], result[1], result[2]);
			
			return result;
		}

		public int[] AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			int[] result = null;
			return AddQuad(a, b, c, d, ref result);
		}

		public int[] AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ref int[] result)
		{
			if (result == null)
				result = new int[4];
			
			var n = Vector3.Cross((c - a), (a - b)).normalized;

			Grow(4, 2);
			result[0] = CreateVertex(a, n);
			result[1] = CreateVertex(b, n);
			result[2] = CreateVertex(c, n);
			result[3] = CreateVertex(d, n);

			MakeQuad(result[0], result[1], result[2], result[3]);
			
			return result;
		}

		public int[] AddQuad(Vector3[] v, Vector2[] uv)
		{
			int[] result = null;
			return AddQuad(v, uv, ref result);
		}

		public int[] AddQuad(Vector3[] v, Vector2[] uv, ref int[] result)
		{
			if (result == null)
				result = new int[4];
			
			var n = Vector3.Cross((v[2] - v[0]), (v[0] - v[1])).normalized;

			Grow(4, 2);
			result[0] = CreateVertex(v[0], n, uv[0]);
			result[1] = CreateVertex(v[1], n, uv[1]);
			result[2] = CreateVertex(v[2], n, uv[2]);
			result[3] = CreateVertex(v[3], n, uv[3]);

			MakeQuad(result[0], result[1], result[2], result[3]);
			
			return result;
		}

		public int[] AddPlane(Plane p, Vector3 origin, Vector2 size, vec2i step)
		{
			return AddPlane(p, origin, size, step, Vector3.forward);
		}

		public int[] AddPlane(Plane p, Vector3 origin, Vector2 size, vec2i step, Vector3 forward)
		{
			Vector3 right = Vector3.Cross(p.normal, forward);
			int cn = step.x + 1;
			int rn = step.y + 1;
			int vn = cn * rn;
			int tn = step.x * step.y * 2;
			Vector2 dv = size.Div(step);

			int[] result = new int[vn];

			var n = p.normal;

			Grow(vn, tn);

			int ri = 0;
			Vector3 v;
			for (int i = 0; i < cn; i++) {
				v = origin + i * right * dv.x;
				for (int j = 0; j < rn; j++, v += forward * dv.y) {					
					result[ri++] = CreateVertex(v, n);
				}
			}

			int vi = 0;
			for (int i = 0; i < tn / 2; i++, vi++) {
				if (((vi + 1) % rn) == 0)
					vi++;

				MakeQuad(result[vi + 0], result[vi + 1], result[vi + 1 + rn], result[vi + 0 + rn]);
			}

			return result;
		}

#endregion

		public int CreateVertex(Vector3 v, Vector3 n)
		{
			return CreateVertex(v, n, VectorEx.empty2);
		}

		public int CreateVertex(Vector3 v, Vector3 n, Vector2 u)
		{
			var vi = vertices.Count;

			vertices.Add(v);
			normals.Add(n);
			uvs.Add(u);

			return vi;
		}

		public int CopyVertex(int vi, Vector3 dv)
		{
			return CreateVertex(vertices[vi] + dv, normals[vi], uvs[vi]);
		}

		public void MakeTriangle(int a, int b, int c)
		{
			var ti = triangles.Count;			

			triangles.Add(a);
			triangles.Add(b);
			triangles.Add(c);
		}

		public void MakeQuad(int a, int b, int c, int d)
		{
			MakeTriangle(a, b, c);
			MakeTriangle(a, c, d);
		}

		public void MakeFan(params int[] ps)
		{
			for (int i = 1; i < ps.Length - 1; i++) {
				MakeTriangle(ps[0], ps[i], ps[i + 1]);
			}
		}

		public void Extrude(int[] contour, Vector3 direction, int steps)
		{
			// light overgrow in triangle count expected.
			Grow(contour.Length * steps, contour.Length * steps * 2);

			Vector3 dv = direction / steps;
			for (int si = 0; si < steps; si++) {
				int pv = CopyVertex(contour[0], dv);
				for (int i = 1; i < contour.Length; i++) {
					int cv = CopyVertex(contour[i], dv);

					MakeQuad(contour[i-1], pv, cv, contour[i]);

					contour[i - 1] = pv;
					pv = cv;
				}
				contour[contour.Length - 1] = pv;
			}
		}

		public void Slice(Plane plane)
		{
			int[] v2v = new int[vertices.Count].Initialize(-1);
			List<Vector3> vs = vertices;
			List<Vector3> ns = normals;
			List<Vector2> us = uvs;
			List<int> ts = triangles;

			vertices = new List<Vector3>(vs.Count);;
			normals = new List<Vector3>(vs.Count);
			uvs = new List<Vector2>(vs.Count);
			triangles = new List<int>(ts.Count);


			for (int i = 0; i < vs.Count; i++) {
				if (plane.GetDistanceToPoint(vs[i]) >= 0) {
					v2v[i] = CreateVertex(vs[i], ns[i], us[i]);					
				}
			}


			for (int i = 0; i < ts.Count; i += 3) {
				int st = ((v2v[ts[i]] < 0 ? 0 : 1) << 0) + ((v2v[ts[i + 1]] < 0 ? 0 : 1) << 1) + ((v2v[ts[i + 2]] < 0 ? 0 : 1) << 2);
				
				if (st == 0)
					continue;
				if (st == 7) {
					MakeTriangle(v2v[ts[i]], v2v[ts[i + 1]], v2v[ts[i + 2]]);
					continue;
				}

				Vector3 ab, bc, ca;
				float abd, bcd, cad;
				bool abi = plane.Intersect(vs[ts[i]], vs[ts[i + 1]], out ab, out abd);
				bool bci = plane.Intersect(vs[ts[i + 1]], vs[ts[i + 2]], out bc, out bcd);
				bool cai = plane.Intersect(vs[ts[i + 2]], vs[ts[i]], out ca, out cad);


				List<int> nvs = new List<int>(4);

				if (!(v2v[ts[i]] < 0)) {
					nvs.Add(v2v[ts[i]]);
				}
				if (abi && (abd > 0 && abd < 1)) {
					nvs.Add(CreateVertex(ab, Vector3.Slerp(ns[ts[i]], ns[ts[i + 1]], abd)));
				}

				if (!(v2v[ts[i + 1]] < 0)) {
					nvs.Add(v2v[ts[i + 1]]);
				}
				if (bci && (bcd > 0 && bcd < 1)) {
					nvs.Add(CreateVertex(bc, Vector3.Slerp(ns[ts[i + 1]], ns[ts[i + 2]], bcd)));
				}

				if (!(v2v[ts[i + 2]] < 0)) {
					nvs.Add(v2v[ts[i + 2]]);
				}
				if (cai && (cad > 0 && cad < 1)) {
					nvs.Add(CreateVertex(ca, Vector3.Slerp(ns[ts[i + 2]], ns[ts[i]], cad)));
				}

				MakeFan(nvs.ToArray());
			}
		}

		public int[] Select(Plane plane)
		{
			List<int> result = new List<int>(vertices.Count);

			for (int i = 0; i < vertices.Count; i++) {
				if (plane.GetDistanceToPoint(vertices[i]) >= 0) {
					result.Add(i);
				}
			}

			return result.ToArray();
		}

		public int[] Project(Plane plane)
		{
			List<int> contour = new List<int>();



			return contour.ToArray();
		}

		public void UVMapPlane(Plane plane, Vector3 forward, int[] vs)
		{
			UVMapPlane(plane, forward, aabb2.one, vs);
		}

		public void UVMapPlane(Plane plane, Vector3 forward, aabb2 uvrect, int[] vs)
		{
			vec2[] pvs = new vec2[vs.Length];

			aabb2 b = aabb2.empty;
			Quaternion q = Quaternion.LookRotation(forward, plane.normal);
			for (int i = 0; i < vs.Length; i++) {
				pvs[i] = MathEx.Convert.ToVec2((q * vertices[vs[i]]).xz());
				b = b.Extend(pvs[i]);
			}

			Debug.Log(b);
			for (int i = 0; i < vs.Length; i++) {
				uvs[vs[i]] = MathEx.Convert.ToVector2(uvrect.a + uvrect.size.Mul((pvs[i] - b.a).Div(b.size)));
				Debug.Log(uvs[vs[i]]);
			}
		}
	}
}
