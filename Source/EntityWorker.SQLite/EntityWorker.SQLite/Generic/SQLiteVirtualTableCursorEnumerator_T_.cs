using System;
using System.Collections;
using System.Collections.Generic;
using EntityWorker.SQLite;
using System.Reflection;

namespace EntityWorker.SQLite.Generic
{
	public class SQLiteVirtualTableCursorEnumerator<T> : SQLiteVirtualTableCursorEnumerator, IEnumerator<T>, IDisposable, IEnumerator
	{
		private IEnumerator<T> enumerator;

		private bool disposed;

		T System.Collections.Generic.IEnumerator<T>.Current
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				if (this.enumerator == null)
				{
					return default(T);
				}
				return this.enumerator.Current;
			}
		}

		public SQLiteVirtualTableCursorEnumerator(SQLiteVirtualTable table, IEnumerator<T> enumerator) : base(table, enumerator)
		{
			this.enumerator = enumerator;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteVirtualTableCursorEnumerator<T>).Name);
			}
		}

		public override void Close()
		{
			if (this.enumerator != null)
			{
				this.enumerator = null;
			}
			base.Close();
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed)
				{
					this.Close();
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}
	}
}