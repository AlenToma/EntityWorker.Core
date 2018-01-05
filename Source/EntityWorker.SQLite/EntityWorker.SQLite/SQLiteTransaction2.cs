using System;
using System.Reflection;
using System.Threading;

namespace EntityWorker.SQLite
{
	public sealed class SQLiteTransaction2 : SQLiteTransaction
	{
		private int _beginLevel;

		private string _savePointName;

		private bool disposed;

		internal SQLiteTransaction2(SQLiteConnection connection, bool deferredLock) : base(connection, deferredLock)
		{
		}

		protected override void Begin(bool deferredLock)
		{
			SQLiteConnection sQLiteConnection = this._cnn;
			int num = sQLiteConnection._transactionLevel;
			int num1 = num;
			sQLiteConnection._transactionLevel = num + 1;
			int num2 = num1;
			int num3 = num2;
			if (num2 != 0)
			{
				try
				{
					using (SQLiteCommand sQLiteCommand = this._cnn.CreateCommand())
					{
						this._savePointName = this.GetSavePointName();
						sQLiteCommand.CommandText = string.Format("SAVEPOINT {0};", this._savePointName);
						sQLiteCommand.ExecuteNonQuery();
						this._beginLevel = num3;
					}
				}
				catch (SQLiteException sQLiteException)
				{
					this._cnn._transactionLevel--;
					this._cnn = null;
					throw;
				}
			}
			else
			{
				try
				{
					using (SQLiteCommand sQLiteCommand1 = this._cnn.CreateCommand())
					{
						if (deferredLock)
						{
							sQLiteCommand1.CommandText = "BEGIN;";
						}
						else
						{
							sQLiteCommand1.CommandText = "BEGIN IMMEDIATE;";
						}
						sQLiteCommand1.ExecuteNonQuery();
						this._beginLevel = num3;
					}
				}
				catch (SQLiteException sQLiteException1)
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
				throw new ObjectDisposedException(typeof(SQLiteTransaction2).Name);
			}
		}

		public override void Commit()
		{
			this.CheckDisposed();
			base.IsValid(true);
			if (this._beginLevel == 0)
			{
				using (SQLiteCommand sQLiteCommand = this._cnn.CreateCommand())
				{
					sQLiteCommand.CommandText = "COMMIT;";
					sQLiteCommand.ExecuteNonQuery();
				}
				this._cnn._transactionLevel = 0;
				this._cnn = null;
				return;
			}
			using (SQLiteCommand sQLiteCommand1 = this._cnn.CreateCommand())
			{
				if (string.IsNullOrEmpty(this._savePointName))
				{
					throw new SQLiteException("Cannot commit, unknown SAVEPOINT");
				}
				sQLiteCommand1.CommandText = string.Format("RELEASE {0};", this._savePointName);
				sQLiteCommand1.ExecuteNonQuery();
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

		private string GetSavePointName()
		{
			SQLiteConnection sQLiteConnection = this._cnn;
			int num = sQLiteConnection._transactionSequence + 1;
			int num1 = num;
			sQLiteConnection._transactionSequence = num;
			return string.Format("sqlite_dotnet_savepoint_{0}", num1);
		}

		protected override void IssueRollback(bool throwError)
		{
			SQLiteConnection sQLiteConnection = Interlocked.Exchange<SQLiteConnection>(ref this._cnn, null);
			if (sQLiteConnection != null)
			{
				if (this._beginLevel != 0)
				{
					try
					{
						using (SQLiteCommand sQLiteCommand = sQLiteConnection.CreateCommand())
						{
							if (string.IsNullOrEmpty(this._savePointName))
							{
								throw new SQLiteException("Cannot rollback, unknown SAVEPOINT");
							}
							sQLiteCommand.CommandText = string.Format("ROLLBACK TO {0};", this._savePointName);
							sQLiteCommand.ExecuteNonQuery();
						}
						sQLiteConnection._transactionLevel--;
					}
					catch
					{
						if (throwError)
						{
							throw;
						}
					}
				}
				else
				{
					try
					{
						using (SQLiteCommand sQLiteCommand1 = sQLiteConnection.CreateCommand())
						{
							sQLiteCommand1.CommandText = "ROLLBACK;";
							sQLiteCommand1.ExecuteNonQuery();
						}
						sQLiteConnection._transactionLevel = 0;
					}
					catch
					{
						if (throwError)
						{
							throw;
						}
					}
				}
			}
		}
	}
}