using System;
using System.Collections.Generic;

namespace EntityWorker.Core.SQLite.Generic
{
	public class SQLiteModuleEnumerable<T> : SQLiteModuleEnumerable
	{
		private IEnumerable<T> enumerable;

		private bool disposed;

		public SQLiteModuleEnumerable(string name, IEnumerable<T> enumerable) : base(name, enumerable)
		{
			this.enumerable = enumerable;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteModuleEnumerable<T>).Name);
			}
		}

		public override SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator<T> sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator<T>;
			if (sQLiteVirtualTableCursorEnumerator == null)
			{
				return this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
			}
			if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
			{
				return this.CursorEndOfEnumeratorError(cursor);
			}
			T current = ((IEnumerator<T>)sQLiteVirtualTableCursorEnumerator).Current;
			if (current == null)
			{
				context.SetNull();
			}
			else
			{
				context.SetString(this.GetStringFromObject(cursor, current));
			}
			return SQLiteErrorCode.Ok;
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

		public override SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			cursor = new SQLiteVirtualTableCursorEnumerator<T>(table, this.enumerable.GetEnumerator());
			return SQLiteErrorCode.Ok;
		}
	}
}