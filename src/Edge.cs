using System;

namespace ProcGenEx
{
	public struct Edge
	{
		public uint a;
		public uint b;

		public Edge(uint b, uint a)
		{
			this.b = b;
			this.a = a;
		}

		public Edge Reverse()
			=> new Edge(b, a);
	}
}
