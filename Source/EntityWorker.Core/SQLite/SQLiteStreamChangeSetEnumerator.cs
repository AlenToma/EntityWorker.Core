using System;
using System.IO;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	internal sealed class SQLiteStreamChangeSetEnumerator : SQLiteChangeSetEnumerator
	{
		private bool disposed;

		public SQLiteStreamChangeSetEnumerator(Stream stream, SQLiteConnectionFlags flags) : base(SQLiteStreamChangeSetIterator.Create(stream, flags))
		{
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteStreamChangeSetEnumerator).Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}
	}
}