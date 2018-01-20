using System;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	internal abstract class SQLiteConnectionLock : IDisposable
	{
		private const string LockNopSql = "SELECT 1;";

		private const string StatementMessageFormat = "Connection lock object was {0} with statement {1}";

		private SQLiteConnectionHandle handle;

		private SQLiteConnectionFlags flags;

		private IntPtr statement;

		private bool disposed;

		public SQLiteConnectionLock(SQLiteConnectionHandle handle, SQLiteConnectionFlags flags, bool autoLock)
		{
			this.handle = handle;
			this.flags = flags;
			if (autoLock)
			{
				this.Lock();
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteConnectionLock).Name);
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
				if (!this.disposed && this.statement != IntPtr.Zero)
				{
					try
					{
						if (HelperMethods.LogPrepare(this.GetFlags()))
						{
							CultureInfo currentCulture = CultureInfo.CurrentCulture;
							object[] objArray = new object[] { (disposing ? "disposed" : "finalized"), this.statement };
							SQLiteLog.LogMessage(SQLiteErrorCode.Misuse, HelperMethods.StringFormat(currentCulture, "Connection lock object was {0} with statement {1}", objArray));
						}
					}
					catch
					{
					}
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		~SQLiteConnectionLock()
		{
			this.Dispose(false);
		}

		protected SQLiteConnectionFlags GetFlags()
		{
			return this.flags;
		}

		protected SQLiteConnectionHandle GetHandle()
		{
			return this.handle;
		}

		protected IntPtr GetIntPtr()
		{
			if (this.handle == null)
			{
				throw new InvalidOperationException("Connection lock object has an invalid handle.");
			}
			IntPtr intPtr = this.handle;
			if (intPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("Connection lock object has an invalid handle pointer.");
			}
			return intPtr;
		}

		public void Lock()
		{
			this.CheckDisposed();
			if (this.statement != IntPtr.Zero)
			{
				return;
			}
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = 0;
				zero = SQLiteString.Utf8IntPtrFromString("SELECT 1;", ref num);
				IntPtr intPtr = IntPtr.Zero;
				int num1 = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_prepare_interop(this.GetIntPtr(), zero, num, ref this.statement, ref intPtr, ref num1);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3_prepare_interop");
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
		}

		public void Unlock()
		{
			this.CheckDisposed();
			if (this.statement == IntPtr.Zero)
			{
				return;
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_finalize_interop(this.statement);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3_finalize_interop");
			}
			this.statement = IntPtr.Zero;
		}
	}
}