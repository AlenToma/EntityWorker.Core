using System;
using System.IO;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	internal sealed class SQLiteStreamChangeSetIterator : SQLiteChangeSetIterator
	{
		private SQLiteStreamAdapter streamAdapter;

		private bool disposed;

		private SQLiteStreamChangeSetIterator(SQLiteStreamAdapter streamAdapter, IntPtr iterator, bool ownHandle) : base(iterator, ownHandle)
		{
			this.streamAdapter = streamAdapter;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteStreamChangeSetIterator).Name);
			}
		}

		public static SQLiteStreamChangeSetIterator Create(Stream stream, SQLiteConnectionFlags flags)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			SQLiteStreamAdapter sQLiteStreamAdapter = null;
			SQLiteStreamChangeSetIterator sQLiteStreamChangeSetIterator = null;
			IntPtr zero = IntPtr.Zero;
			try
			{
				sQLiteStreamAdapter = new SQLiteStreamAdapter(stream, flags);
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_start_strm(ref zero, sQLiteStreamAdapter.GetInputDelegate(), IntPtr.Zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_start_strm");
				}
				sQLiteStreamChangeSetIterator = new SQLiteStreamChangeSetIterator(sQLiteStreamAdapter, zero, true);
			}
			finally
			{
				if (sQLiteStreamChangeSetIterator == null)
				{
					if (zero != IntPtr.Zero)
					{
						UnsafeNativeMethods.sqlite3changeset_finalize(zero);
						zero = IntPtr.Zero;
					}
					if (sQLiteStreamAdapter != null)
					{
						sQLiteStreamAdapter.Dispose();
						sQLiteStreamAdapter = null;
					}
				}
			}
			return sQLiteStreamChangeSetIterator;
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
	}
}