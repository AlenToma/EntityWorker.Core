using System;

namespace EntityWorker.SQLite
{
	public class CommitEventArgs : EventArgs
	{
		public bool AbortTransaction;

		internal CommitEventArgs()
		{
		}
	}
}