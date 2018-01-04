using System;

namespace System.Data.SQLite
{
	public class CommitEventArgs : EventArgs
	{
		public bool AbortTransaction;

		internal CommitEventArgs()
		{
		}
	}
}