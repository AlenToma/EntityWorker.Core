using System;
using System.Collections;
using System.Reflection;

namespace EntityWorker.SQLite
{
	public class SQLiteVirtualTableCursorEnumerator : SQLiteVirtualTableCursor, IEnumerator
	{
		private IEnumerator enumerator;

		private bool endOfEnumerator;

		private bool disposed;

		public virtual object Current
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				if (this.enumerator == null)
				{
					return null;
				}
				return this.enumerator.Current;
			}
		}

		public virtual bool EndOfEnumerator
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				return this.endOfEnumerator;
			}
		}

		public virtual bool IsOpen
		{
			get
			{
				this.CheckDisposed();
				return this.enumerator != null;
			}
		}

		public SQLiteVirtualTableCursorEnumerator(SQLiteVirtualTable table, IEnumerator enumerator) : base(table)
		{
			this.enumerator = enumerator;
			this.endOfEnumerator = true;
		}

		public virtual void CheckClosed()
		{
			this.CheckDisposed();
			if (!this.IsOpen)
			{
				throw new InvalidOperationException("virtual table cursor is closed");
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteVirtualTableCursorEnumerator).Name);
			}
		}

		public virtual void Close()
		{
			if (this.enumerator != null)
			{
				this.enumerator = null;
			}
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

		public virtual bool MoveNext()
		{
			this.CheckDisposed();
			this.CheckClosed();
			if (this.enumerator == null)
			{
				return false;
			}
			this.endOfEnumerator = !this.enumerator.MoveNext();
			if (!this.endOfEnumerator)
			{
				this.NextRowIndex();
			}
			return !this.endOfEnumerator;
		}

		public virtual void Reset()
		{
			this.CheckDisposed();
			this.CheckClosed();
			if (this.enumerator == null)
			{
				return;
			}
			this.enumerator.Reset();
		}
	}
}