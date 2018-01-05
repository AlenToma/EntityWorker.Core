using System;
using System.Reflection;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteMemoryChangeSetEnumerator : SQLiteChangeSetEnumerator
	{
		private byte[] rawData;

		private bool disposed;

		public SQLiteMemoryChangeSetEnumerator(byte[] rawData) : base(SQLiteMemoryChangeSetIterator.Create(rawData))
		{
			this.rawData = rawData;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteMemoryChangeSetEnumerator).Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed)
				{
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		public override void Reset()
		{
			this.CheckDisposed();
			base.ResetIterator(SQLiteMemoryChangeSetIterator.Create(this.rawData));
		}
	}
}