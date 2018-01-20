using System;
using System.Reflection;
using System.Threading;

namespace EntityWorker.Core.SQLite
{
	public class SQLiteTransaction : SQLiteTransactionBase
	{
		private bool disposed;

		internal SQLiteTransaction(SQLiteConnection connection, bool deferredLock) : base(connection, deferredLock)
		{
		}

		protected override void Begin(bool deferredLock)
		{
			SQLiteConnection sQLiteConnection = this._cnn;
			int num = sQLiteConnection._transactionLevel;
			int num1 = num;
			sQLiteConnection._transactionLevel = num + 1;
			if (num1 == 0)
			{
				try
				{
					using (SQLiteCommand sQLiteCommand = this._cnn.CreateCommand())
					{
						if (deferredLock)
						{
							sQLiteCommand.CommandText = "BEGIN;";
						}
						else
						{
							sQLiteCommand.CommandText = "BEGIN IMMEDIATE;";
						}
						sQLiteCommand.ExecuteNonQuery();
					}
				}
				catch (SQLiteException sQLiteException)
				{
					this._cnn._transactionLevel--;
					this._cnn = null;
					throw;
				}
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteTransaction).Name);
			}
		}

		public override void Commit()
		{
			this.CheckDisposed();
			base.IsValid(true);
			if (this._cnn._transactionLevel - 1 == 0)
			{
				using (SQLiteCommand sQLiteCommand = this._cnn.CreateCommand())
				{
					sQLiteCommand.CommandText = "COMMIT;";
					sQLiteCommand.ExecuteNonQuery();
				}
			}
			this._cnn._transactionLevel--;
			this._cnn = null;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing && base.IsValid(false))
				{
					this.IssueRollback(false);
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		protected override void IssueRollback(bool throwError)
		{
			SQLiteConnection sQLiteConnection = Interlocked.Exchange<SQLiteConnection>(ref this._cnn, null);
			if (sQLiteConnection != null)
			{
				try
				{
					using (SQLiteCommand sQLiteCommand = sQLiteConnection.CreateCommand())
					{
						sQLiteCommand.CommandText = "ROLLBACK;";
						sQLiteCommand.ExecuteNonQuery();
					}
				}
				catch
				{
					if (throwError)
					{
						throw;
					}
				}
				sQLiteConnection._transactionLevel = 0;
			}
		}
	}
}