using System;

namespace ProcGenEx
{
	public struct Edge
	{
		public uint a;
		public uint b;

		public Edge(uint a, uint b)
		{
			this.a = a;
			this.b = b;
		}

		public Edge Reverse()
			=> new Edge(b, a);
	}
}
