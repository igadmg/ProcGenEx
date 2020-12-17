using MathEx;
using System;
using System.Collections.Generic;
using SystemEx;
using UnityEngine;

namespace ProcGenEx
{
	public static class Icosahedron
	{
		public class IcosahedronMeshBuilder : MeshBuilder
		{
			public IcosahedronMeshBuilder(int VertexCount, int TriangleCount) : base(VertexCount, TriangleCount)
			{
			}
		}

		public static IcosahedronMeshBuilder CreateSimple()
		{
			IcosahedronMeshBuilder mesh = new IcosahedronMeshBuilder(12, 20);
			List<uint> vs = new List<uint>(12);

			vec3 north = vec3.up / 2.0f;
			vec3 south = vec3.down / 2.0f;
			float vi = 90 * Mathf.Deg2Rad - Mathf.Atan(0.5f);
			float da = 72 * Mathf.Deg2Rad;
			svec3 sv;

			vs.Add(mesh.CreateVertex(north, north));

			sv = new svec3(1.0f / 2.0f, vi, 0);
			for (int i = 0; i < 5; i++, sv.a += da)
			{
				vec3 cv = sv.ToVec3().normalized.xzy() / 2.0f;
				vs.Add(mesh.CreateVertex(cv, cv));
			}

			sv = new svec3(1.0f / 2.0f, 180 * Mathf.Deg2Rad - vi, da / 2.0f);
			for (int i = 0; i < 5; i++, sv.a += da)
			{
				vec3 cv = sv.ToVec3().normalized.xzy() / 2.0f;
				vs.Add(mesh.CreateVertex(cv, cv));
			}

			vs.Add(mesh.CreateVertex(south, south));

			mesh.MakeTriangle(vs[0], vs[2], vs[1]);
			mesh.MakeTriangle(vs[0], vs[3], vs[2]);
			mesh.MakeTriangle(vs[0], vs[4], vs[3]);
			mesh.MakeTriangle(vs[0], vs[5], vs[4]);
			mesh.MakeTriangle(vs[0], vs[1], vs[5]);

			mesh.MakeTriangle(vs[2], vs[6], vs[1]);
			mesh.MakeTriangle(vs[3], vs[7], vs[2]);
			mesh.MakeTriangle(vs[4], vs[8], vs[3]);
			mesh.MakeTriangle(vs[5], vs[9], vs[4]);
			mesh.MakeTriangle(vs[1], vs[10], vs[5]);

			mesh.MakeTriangle(vs[6], vs[2], vs[7]);
			mesh.MakeTriangle(vs[7], vs[3], vs[8]);
			mesh.MakeTriangle(vs[8], vs[4], vs[9]);
			mesh.MakeTriangle(vs[9], vs[5], vs[10]);
			mesh.MakeTriangle(vs[10], vs[1], vs[6]);

			mesh.MakeTriangle(vs[11], vs[6], vs[7]);
			mesh.MakeTriangle(vs[11], vs[7], vs[8]);
			mesh.MakeTriangle(vs[11], vs[8], vs[9]);
			mesh.MakeTriangle(vs[11], vs[9], vs[10]);
			mesh.MakeTriangle(vs[11], vs[10], vs[6]);

			return mesh;
		}

		public static IcosahedronMeshBuilder Create()
		{
			IcosahedronMeshBuilder mesh = new IcosahedronMeshBuilder(22, 20);
			List<uint> vs = new List<uint>(22);
			List<uint> polarvs = new List<uint>();
			List<uint> edgevs = new List<uint>();
			List<uint> commonvs = new List<uint>();

			vec3 north = vec3.up / 2.0f;
			vec3 south = vec3.down / 2.0f;
			float vi = 90 * Mathf.Deg2Rad - Mathf.Atan(0.5f);
			float da = 72 * Mathf.Deg2Rad;
			float uvx = 0, uvy = 0;
			svec3 sv;

			float uvdx = 1.0f / 5.0f;
			uvx = uvdx / 2.0f;
			for (int i = 0; i < 5; i++, uvx += uvdx)
			{
				vs.Add(mesh.CreateVertex(north, north, new vec2(uvx, 1.0f)));
			}

			uvy = 2.0f / 3.0f;
			uvx = 0;
			sv = new svec3(1.0f / 2.0f, vi, 0);
			for (int i = 0; i < 5; i++, uvx += uvdx, sv.a += da)
			{
				vec3 cv = sv.ToVec3().normalized.xzy() / 2.0f;
				vs.Add(mesh.CreateVertex(cv, cv, new vec2(uvx, uvy)));
			}
			{
				vec3 cv = sv.ToVec3().normalized.xzy() / 2.0f;
				vs.Add(mesh.CreateVertex(cv, cv, new vec2(uvx, uvy)));
			}

			uvy = 1.0f / 3.0f;
			uvx = uvdx / 2.0f;
			sv = new svec3(1.0f / 2.0f, 180 * Mathf.Deg2Rad - vi, da / 2.0f);
			for (int i = 0; i < 5; i++, uvx += uvdx, sv.a += da)
			{
				vec3 cv = sv.ToVec3().normalized.xzy() / 2.0f;
				vs.Add(mesh.CreateVertex(cv, cv, new vec2(uvx, uvy)));
			}
			{
				vec3 cv = sv.ToVec3().normalized.xzy() / 2.0f;
				vs.Add(mesh.CreateVertex(cv, cv, new vec2(uvx, uvy)));
			}

			uvx = uvdx;
			for (int i = 0; i < 5; i++, uvx += uvdx)
			{
				vs.Add(mesh.CreateVertex(south, south, new vec2(uvx, 0.0f)));
			}

			mesh.MakeTriangle(vs[0], vs[6], vs[5]);
			mesh.MakeTriangle(vs[1], vs[7], vs[6]);
			mesh.MakeTriangle(vs[2], vs[8], vs[7]);
			mesh.MakeTriangle(vs[3], vs[9], vs[8]);
			mesh.MakeTriangle(vs[4], vs[10], vs[9]);

			mesh.MakeTriangle(vs[6], vs[11], vs[5]);
			mesh.MakeTriangle(vs[7], vs[12], vs[6]);
			mesh.MakeTriangle(vs[8], vs[13], vs[7]);
			mesh.MakeTriangle(vs[9], vs[14], vs[8]);
			mesh.MakeTriangle(vs[10], vs[15], vs[9]);

			mesh.MakeTriangle(vs[11], vs[6], vs[12]);
			mesh.MakeTriangle(vs[12], vs[7], vs[13]);
			mesh.MakeTriangle(vs[13], vs[8], vs[14]);
			mesh.MakeTriangle(vs[14], vs[9], vs[15]);
			mesh.MakeTriangle(vs[15], vs[10], vs[16]);

			mesh.MakeTriangle(vs[17], vs[11], vs[12]);
			mesh.MakeTriangle(vs[18], vs[12], vs[13]);
			mesh.MakeTriangle(vs[19], vs[13], vs[14]);
			mesh.MakeTriangle(vs[20], vs[14], vs[15]);
			mesh.MakeTriangle(vs[21], vs[15], vs[16]);

			return mesh;
		}

		public static MeshBuilder UpdateUV(this IcosahedronMeshBuilder mesh)
		{
			for (int i = 0; i < mesh.vertices.Count; i++)
			{
				var v = mesh.vertices[i];

				if (v.y == 0.5f || v.y == -0.5f)
					continue;

				svec3 uv = (svec3)v.xzy();
				vec2 textureCoordinates;
				textureCoordinates.x = uv.a / (2.0f * Mathf.PI);
				if (textureCoordinates.x < 0f)
				{
					textureCoordinates.x += 1f;
				}
				if ((mesh.uvs[i].x - textureCoordinates.x) > 0.5f)
				{ // hehehehe
					textureCoordinates.x += 1f;
				}
				//textureCoordinates.y = uv.i / Mathf.PI;
				textureCoordinates.y = Mathf.Asin(2 * v.y) / Mathf.PI + 0.5f;
				mesh.uvs[i] = textureCoordinates;
			}

			return mesh;
		}
	}
}
