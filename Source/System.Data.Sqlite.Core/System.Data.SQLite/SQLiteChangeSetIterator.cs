using System;
using System.Reflection;

namespace System.Data.SQLite
{
	internal class SQLiteChangeSetIterator : IDisposable
	{
		private IntPtr iterator;

		private bool ownHandle;

		private bool disposed;

		protected SQLiteChangeSetIterator(IntPtr iterator, bool ownHandle)
		{
			this.iterator = iterator;
			this.ownHandle = ownHandle;
		}

		public static SQLiteChangeSetIterator Attach(IntPtr iterator)
		{
			return new SQLiteChangeSetIterator(iterator, false);
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteChangeSetIterator).Name);
			}
		}

		internal void CheckHandle()
		{
			if (this.iterator == IntPtr.Zero)
			{
				throw new InvalidOperationException("iterator is not open");
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && this.iterator != IntPtr.Zero)
				{
					if (this.ownHandle)
					{
						UnsafeNativeMethods.sqlite3changeset_finalize(this.iterator);
					}
					this.iterator = IntPtr.Zero;
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		~SQLiteChangeSetIterator()
		{
			this.Dispose(false);
		}

		internal IntPtr GetIntPtr()
		{
			return this.iterator;
		}

		public bool Next()
		{
			this.CheckDisposed();
			this.CheckHandle();
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_next(this.iterator);
			SQLiteErrorCode sQLiteErrorCode1 = sQLiteErrorCode;
			if (sQLiteErrorCode1 == SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(SQLiteErrorCode.Ok, "sqlite3changeset_next: unexpected result Ok");
			}
			switch (sQLiteErrorCode1)
			{
				case SQLiteErrorCode.Row:
				{
					return true;
				}
				case SQLiteErrorCode.Done:
				{
					return false;
				}
			}
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_next");
		}
	}
}