using MathEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemEx;
using UnityEngine;

namespace ProcGenEx
{
	public class Icosahedron
	{
		public static MeshBuilder Create()
		{
			MeshBuilder mesh = new MeshBuilder(12, 20);

			float phi = (1 + MathEx.MathEx.Sqrt(5)) / 2;
			List<int> vs = new List<int>(12);

			for (int i = 0; i < 4; i++) {
				//v[ i ] = Vector( 0, -(i&2), -(i&1)*t ) ; 
				vec3 v = new vec3(0, (i & 2) != 0 ? -1 : 1, (i & 1) != 0 ? -phi : phi).normalized * ((i == 0) ? 1.5f : 1);
				vs.Add(mesh.CreateVertex(v, v));
			}

			for (int i = 4; i < 8; i++) {
				//v[ i ] = Vector( -(i&2), -(i&1)*t, 0 ) ; 
				vec3 v = new vec3((i & 2) != 0 ? -1 : 1, (i & 1) != 0 ? -phi : phi, 0).normalized * ((i == 4) ? 1.5f : 1);
				vs.Add(mesh.CreateVertex(v, v));
			}

			for (int i = 8; i < 12; i++) {
				//v[ i ] = Vector( -(i&1)*t, 0, -(i&2) ) ; 
				vec3 v = new vec3((i & 1) != 0 ? -phi : phi, 0, (i & 2) != 0 ? -1 : 1).normalized * ((i == 10) ? 1.5f : 1);
				vs.Add(mesh.CreateVertex(v, v));
			}

			var v44 = mesh.CopyVertex(vs[4]);
			var v1010 = mesh.CopyVertex(vs[10]);

			mesh.MakeTriangle(vs[0], vs[2], vs[8]);
			mesh.MakeTriangle(vs[0], vs[6], vs[9]);
			mesh.MakeTriangle(vs[0], vs[9], vs[2]);
			mesh.MakeTriangle(vs[0], vs[8], vs[4]);
			mesh.MakeTriangle(vs[0], v44, vs[6]);

			mesh.MakeTriangle(vs[3], vs[5], vs[7]);
			mesh.MakeTriangle(vs[11], vs[3], vs[7]);
			mesh.MakeTriangle(vs[1], vs[3], vs[11]);
			mesh.MakeTriangle(vs[10], vs[5], vs[3]);
			mesh.MakeTriangle(v1010, vs[3], vs[1]);
			
			mesh.MakeTriangle(vs[8], vs[10], vs[4]);
			mesh.MakeTriangle(vs[8], vs[5], vs[10]);
			mesh.MakeTriangle(vs[1], vs[6], v44);
			mesh.MakeTriangle(v1010, vs[1], v44);
			mesh.MakeTriangle(vs[2], vs[7], vs[5]);
			mesh.MakeTriangle(vs[2], vs[9], vs[7]);
			mesh.MakeTriangle(vs[11], vs[7], vs[9]);
			mesh.MakeTriangle(vs[2], vs[5], vs[8]);
			mesh.MakeTriangle(vs[1], vs[11], vs[6]);
			mesh.MakeTriangle(vs[6], vs[11], vs[9]);

			mesh.Hole(vs[0]);
			mesh.Hole(vs[3]);			

			//mesh.Rotate(Quaternion.LookRotation(mesh.vertices[vs[0]].ToVector3()));
			var v0 = mesh.vertices[vs[0]];
			var v4 = mesh.vertices[vs[4]];

			var up = v0;
			var right = v4;
			var forward = (up % right).normalized;
			//right = (up % forward).normalized;

			/*
			var r = Matrix4x4.identity
				.Column(0, right.ToVector3())
				.Column(1, up.ToVector3())
				.Column(2, forward.ToVector3()).ToQuaternion();*/
			var r = Quaternion.LookRotation(right.ToVector3());
			mesh.Rotate(r);

			for (int i = 0; i < mesh.vertices.Count; i++) {
				var v = mesh.vertices[i];
				
				if (v.y < -0.98f)
					Debug.Log("*** South POLE {0}".format(i));
			}

			return mesh;
		}

		public static MeshBuilder Subdivide(MeshBuilder mesh)
		{
			Dictionary<Tuple<int, int>, int> vertices = new Dictionary<Tuple<int,int>, int>();

			int icount = mesh.triangles.Count;
			for (int i = 0; i < icount; i += 3) {
				var ta = mesh.triangles[i];
				var tb = mesh.triangles[i + 1];
				var tc = mesh.triangles[i + 2];


				Func<int, int, int> createFn = (int va, int vb) => {
					int vr;
					var key = Tuple.Create(va, vb).Sort();
					if (!vertices.TryGetValue(key, out vr)) {
						var v = (mesh.vertices[va] + (mesh.vertices[vb] - mesh.vertices[va]) / 2).normalized;
						vr = mesh.CreateVertex(v , v);
						vertices.Add(key, vr);
					}
					return vr;
				};
				int td = createFn(ta, tb);
				int te = createFn(tb, tc);
				int tf = createFn(tc, ta);

				mesh.triangles[i] = td;
				mesh.triangles[i + 1] = te;
				mesh.triangles[i + 2] = tf;

				mesh.MakeTriangle(ta, td, tf);
				mesh.MakeTriangle(tb, te, td);
				mesh.MakeTriangle(tc, tf, te);
			}

			return mesh;
		}
	}
}
