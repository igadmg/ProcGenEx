using MathEx;
using System;
using System.Linq;
using UnityDissolve;
using UnityEngine;
using UnityEngineEx;

namespace ProcGenEx
{
	public class PlaneMeshComponent : DissolvedMonoBehaviour
	{
		[Component]
		RectangleGridComponent grid;

		[Component]
		[SerializeField, RequireInterface(typeof(IGridSampler))]
		Component heightSampler_;
		IGridSampler heightSampler => (IGridSampler)heightSampler_;

		public float amplitude = 1;
		public bool normalized; // normalize plane coordinates when sampling the height.

		Lazy<Mesh> mesh = null;


#if UNITY_EDITOR
		public override void OnValidate()
		{
			base.OnValidate();

			mesh = new Lazy<Mesh>(() => {
				var cells = grid.Cells.Select(c => c.o).ToArray();
				var plane = PlaneMesh.Create(grid.GridSize, cells);

				if (heightSampler != null)
				{
					var boundsSize = grid.bounds.size.xzy(1);
					for (int i = 0; i < plane.vertices.Count; i++)
					{
						var c = plane.vertices[i];
						if (normalized)
							c /= boundsSize;

						float y = heightSampler.SampleGrid(c.xz()).y;
						plane.vertices[i] = plane.vertices[i].Y(y * amplitude);
					}
				}

				return plane.ToMesh();
			});
		}
#endif

	}
}
