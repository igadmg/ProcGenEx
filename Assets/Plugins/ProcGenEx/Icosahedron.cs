using MathEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
				vec3 v = new vec3(0, (i & 2) != 0 ? -1 : 1, (i & 1) != 0 ? -phi : phi).normalized;// * ((i % 4) == 1 ? 1.5f : 1);
				vs.Add(mesh.CreateVertex(v, v));
			}

			for (int i = 4; i < 8; i++) {
				//v[ i ] = Vector( -(i&2), -(i&1)*t, 0 ) ; 
				vec3 v = new vec3((i & 2) != 0 ? -1 : 1, (i & 1) != 0 ? -phi : phi, 0).normalized;// * ((i % 4) != 2 ? 1.5f : 1);
				vs.Add(mesh.CreateVertex(v, v));
			}

			for (int i = 8; i < 12; i++) {
				//v[ i ] = Vector( -(i&1)*t, 0, -(i&2) ) ; 
				vec3 v = new vec3((i & 1) != 0 ? -phi : phi, 0, (i & 2) != 0 ? -1 : 1).normalized;// * ((i % 4) == 0 ? 1.5f : 1);
				vs.Add(mesh.CreateVertex(v, v));
			}


			mesh.MakeTriangle(vs[0], vs[2], vs[8]);
			mesh.MakeTriangle(vs[0], vs[8], vs[4]);
			mesh.MakeTriangle(vs[0], vs[4], vs[6]);
			mesh.MakeTriangle(vs[0], vs[6], vs[9]);
			mesh.MakeTriangle(vs[0], vs[9], vs[2]);

			mesh.MakeTriangle(vs[3], vs[5], vs[7]);
			mesh.MakeTriangle(vs[2], vs[7], vs[5]);
			mesh.MakeTriangle(vs[2], vs[9], vs[7]);
			mesh.MakeTriangle(vs[11], vs[3], vs[7]);
			mesh.MakeTriangle(vs[11], vs[7], vs[9]);
			
			mesh.MakeTriangle(vs[8], vs[5], vs[10]);
			mesh.MakeTriangle(vs[10], vs[5], vs[3]);
			mesh.MakeTriangle(vs[2], vs[5], vs[8]);
			
			mesh.MakeTriangle(vs[10], vs[3], vs[1]);
			mesh.MakeTriangle(vs[10], vs[1], vs[4]);
			mesh.MakeTriangle(vs[1], vs[6], vs[4]);
			mesh.MakeTriangle(vs[1], vs[3], vs[11]);
			mesh.MakeTriangle(vs[1], vs[11], vs[6]);

			mesh.MakeTriangle(vs[8], vs[10], vs[4]);

			mesh.MakeTriangle(vs[6], vs[11], vs[9]);

			
			return mesh;
		}
	}
}
