using MathEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenEx
{
	public static class PlaneMesh
	{
		public class PlaneMeshBuilder : MeshBuilder
		{
			public PlaneMeshBuilder(int VertexCount, int TriangleCount) : base(VertexCount, TriangleCount)
			{
			}
		}

		public static PlaneMeshBuilder Create()
		{
			PlaneMeshBuilder mesh = new PlaneMeshBuilder(4, 2);

			mesh.AddQuad((-vec3.right + vec3.forward) * 0.5f
				, (vec3.right + vec3.forward) * 0.5f
				, (vec3.right - vec3.forward) *0.5f
				, (-vec3.right - vec3.forward) *0.5f);

			return mesh;
		}

#if false

		public uint[] AddPlane(plane p, vec3 origin, vec2 size, vec2i step)
		{
			return AddPlane(p, origin, size, step, vec3.forward);
		}

		public uint[] AddPlane(plane p, vec3 origin, vec2 size, vec2i step, vec3 forward)
		{
			var n = MathEx.Convert.ToVec3(p.normal);
			vec3 right = vec3.Cross(n, forward);
			int cn = step.x + 1;
			int rn = step.y + 1;
			int vn = cn * rn;
			int tn = step.x * step.y * 2;
			vec2 dv = size.Div(step);

			uint[] result = new uint[vn];

			Grow(vn, tn);

			int ri = 0;
			vec3 v;
			for (int i = 0; i < cn; i++)
			{
				v = origin + i * right * dv.x;
				for (int j = 0; j < rn; j++, v += forward * dv.y)
				{
					result[ri++] = CreateVertex(v, n);
				}
			}

			int vi = 0;
			for (int i = 0; i < tn / 2; i++, vi++)
			{
				if (((vi + 1) % rn) == 0)
					vi++;

				MakeQuad(result[vi + 0], result[vi + 1], result[vi + 1 + rn], result[vi + 0 + rn]);
			}

			return result;
		}

		public uint[] AddPlane(IEnumerator<Tuple<vec3, vec3>> vertices, vec2i step, bool revert = false)
		{
			int cn = step.x + 1;
			int rn = step.y + 1;
			int vn = cn * rn;
			int tn = step.x * step.y * 2;

			uint[] result = new uint[vn];

			Grow(vn, tn);

			int ri = 0;
			int i = 0;
			int j = 0;
			while (vertices.MoveNext())
			{
				var v = vertices.Current;
				result[ri++] = CreateVertex(v.Item1, v.Item2);

				j++;
				if (j == rn)
				{
					i++;
					j = 0;
					if (i == cn)
						break;
				}
			}

			int vi = 0;
			for (i = 0; i < tn / 2; i++, vi++)
			{
				if (((vi + 1) % rn) == 0)
					vi++;

				if (!revert)
					MakeQuad(result[vi + 0], result[vi + 1], result[vi + 1 + rn], result[vi + 0 + rn]);
				else
					MakeQuad(result[vi + 1 + rn], result[vi + 1], result[vi + 0], result[vi + 0 + rn]);
			}

			return result;
		}
#endif
	}
}
