using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	public abstract class SQLiteTransactionBase : DbTransaction
	{
		internal SQLiteConnection _cnn;

		internal int _version;

		private System.Data.IsolationLevel _level;

		private bool disposed;

		public new SQLiteConnection Connection
		{
			get
			{
				this.CheckDisposed();
				return this._cnn;
			}
		}

		protected override System.Data.Common.DbConnection DbConnection
		{
			get
			{
				return this.Connection;
			}
		}

		public override System.Data.IsolationLevel IsolationLevel
		{
			get
			{
				this.CheckDisposed();
				return this._level;
			}
		}

		internal SQLiteTransactionBase(SQLiteConnection connection, bool deferredLock)
		{
			this._cnn = connection;
			this._version = this._cnn._version;
			this._level = (deferredLock ? System.Data.IsolationLevel.ReadCommitted : System.Data.IsolationLevel.Serializable);
			this.Begin(deferredLock);
		}

		protected abstract void Begin(bool deferredLock);

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteTransactionBase).Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing && this.IsValid(false))
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

		protected abstract void IssueRollback(bool throwError);

		internal bool IsValid(bool throwError)
		{
			if (this._cnn == null)
			{
				if (throwError)
				{
					throw new ArgumentNullException("No connection associated with this transaction");
				}
				return false;
			}
			if (this._cnn._version != this._version)
			{
				if (throwError)
				{
					throw new SQLiteException("The connection was closed and re-opened, changes were already rolled back");
				}
				return false;
			}
			if (this._cnn.State != ConnectionState.Open)
			{
				if (throwError)
				{
					throw new SQLiteException("Connection was closed");
				}
				return false;
			}
			if (this._cnn._transactionLevel != 0 && !this._cnn._sql.AutoCommit)
			{
				return true;
			}
			this._cnn._transactionLevel = 0;
			if (throwError)
			{
				throw new SQLiteException("No transaction is active on this connection");
			}
			return false;
		}

		public override void Rollback()
		{
			this.CheckDisposed();
			this.IsValid(true);
			this.IssueRollback(true);
		}
	}
}