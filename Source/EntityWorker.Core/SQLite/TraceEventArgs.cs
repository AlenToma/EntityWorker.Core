using System;

namespace EntityWorker.Core.SQLite
{
	public class TraceEventArgs : EventArgs
	{
		public readonly string Statement;

		internal TraceEventArgs(string statement)
		{
			this.Statement = statement;
		}
	}
}