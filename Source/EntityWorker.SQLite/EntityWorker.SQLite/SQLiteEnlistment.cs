using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Transactions;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteEnlistment : IDisposable, IEnlistmentNotification
	{
		internal SQLiteTransaction _transaction;

		internal Transaction _scope;

		internal bool _disposeConnection;

		private bool disposed;

		internal SQLiteEnlistment(SQLiteConnection cnn, Transaction scope, System.Data.IsolationLevel defaultIsolationLevel, bool throwOnUnavailable, bool throwOnUnsupported)
		{
			this._transaction = cnn.BeginTransaction(this.GetSystemDataIsolationLevel(cnn, scope, defaultIsolationLevel, throwOnUnavailable, throwOnUnsupported));
			this._scope = scope;
			this._scope.EnlistVolatile(this, EnlistmentOptions.None);
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteEnlistment).Name);
			}
		}

		private void Cleanup(SQLiteConnection cnn)
		{
			if (this._disposeConnection)
			{
				cnn.Dispose();
			}
			this._transaction = null;
			this._scope = null;
		}

		public void Commit(Enlistment enlistment)
		{
			this.CheckDisposed();
			SQLiteConnection connection = this._transaction.Connection;
			connection._enlistment = null;
			try
			{
				this._transaction.IsValid(true);
				this._transaction.Connection._transactionLevel = 1;
				this._transaction.Commit();
				enlistment.Done();
			}
			finally
			{
				this.Cleanup(connection);
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
					if (this._transaction != null)
					{
						this._transaction.Dispose();
						this._transaction = null;
					}
					if (this._scope != null)
					{
						this._scope = null;
					}
				}
				this.disposed = true;
			}
		}

		~SQLiteEnlistment()
		{
			this.Dispose(false);
		}

		private System.Data.IsolationLevel GetSystemDataIsolationLevel(SQLiteConnection connection, Transaction transaction, System.Data.IsolationLevel defaultIsolationLevel, bool throwOnUnavailable, bool throwOnUnsupported)
		{
			if (transaction == null)
			{
				if (connection != null)
				{
					return connection.GetDefaultIsolationLevel();
				}
				if (throwOnUnavailable)
				{
					throw new InvalidOperationException("isolation level is unavailable");
				}
				return defaultIsolationLevel;
			}
			System.Transactions.IsolationLevel isolationLevel = transaction.IsolationLevel;
			switch (isolationLevel)
			{
				case System.Transactions.IsolationLevel.Serializable:
				{
					return System.Data.IsolationLevel.Serializable;
				}
				case System.Transactions.IsolationLevel.RepeatableRead:
				{
					return System.Data.IsolationLevel.RepeatableRead;
				}
				case System.Transactions.IsolationLevel.ReadCommitted:
				{
					return System.Data.IsolationLevel.ReadCommitted;
				}
				case System.Transactions.IsolationLevel.ReadUncommitted:
				{
					return System.Data.IsolationLevel.ReadUncommitted;
				}
				case System.Transactions.IsolationLevel.Snapshot:
				{
					return System.Data.IsolationLevel.Snapshot;
				}
				case System.Transactions.IsolationLevel.Chaos:
				{
					return System.Data.IsolationLevel.Chaos;
				}
				case System.Transactions.IsolationLevel.Unspecified:
				{
					return System.Data.IsolationLevel.Unspecified;
				}
			}
			if (throwOnUnsupported)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { isolationLevel };
				throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "unsupported isolation level {0}", objArray));
			}
			return defaultIsolationLevel;
		}

		public void InDoubt(Enlistment enlistment)
		{
			this.CheckDisposed();
			enlistment.Done();
		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			this.CheckDisposed();
			if (!this._transaction.IsValid(false))
			{
				preparingEnlistment.ForceRollback();
				return;
			}
			preparingEnlistment.Prepared();
		}

		public void Rollback(Enlistment enlistment)
		{
			this.CheckDisposed();
			SQLiteConnection connection = this._transaction.Connection;
			connection._enlistment = null;
			try
			{
				this._transaction.Rollback();
				enlistment.Done();
			}
			finally
			{
				this.Cleanup(connection);
			}
		}
	}
}