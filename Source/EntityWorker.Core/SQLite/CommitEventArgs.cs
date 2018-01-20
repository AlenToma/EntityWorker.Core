using System;

namespace EntityWorker.Core.SQLite
{
	public class CommitEventArgs : EventArgs
	{
		public bool AbortTransaction;

		internal CommitEventArgs()
		{
		}
	}
}