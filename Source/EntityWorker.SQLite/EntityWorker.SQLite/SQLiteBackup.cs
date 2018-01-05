using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteBackup : IDisposable
	{
		internal SQLiteBase _sql;

		internal SQLiteBackupHandle _sqlite_backup;

		internal IntPtr _destDb;

		internal byte[] _zDestName;

		internal IntPtr _sourceDb;

		internal byte[] _zSourceName;

		internal SQLiteErrorCode _stepResult;

		private bool disposed;

		internal SQLiteBackup(SQLiteBase sqlbase, SQLiteBackupHandle backup, IntPtr destDb, byte[] zDestName, IntPtr sourceDb, byte[] zSourceName)
		{
			this._sql = sqlbase;
			this._sqlite_backup = backup;
			this._destDb = destDb;
			this._zDestName = zDestName;
			this._sourceDb = sourceDb;
			this._zSourceName = zSourceName;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteBackup).Name);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this._sqlite_backup != null)
					{
						this._sqlite_backup.Dispose();
						this._sqlite_backup = null;
					}
					this._zSourceName = null;
					this._sourceDb = IntPtr.Zero;
					this._zDestName = null;
					this._destDb = IntPtr.Zero;
					this._sql = null;
				}
				this.disposed = true;
			}
		}

		~SQLiteBackup()
		{
			this.Dispose(false);
		}
	}
}