using System;
using System.Reflection;

namespace EntityWorker.SQLite
{
	public class SQLiteFunctionEx : SQLiteFunction
	{
		private bool disposed;

		public SQLiteFunctionEx()
		{
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteFunctionEx).Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				bool flag = this.disposed;
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		protected CollationSequence GetCollationSequence()
		{
			return this._base.GetCollationSequence(this, this._context);
		}
	}
}