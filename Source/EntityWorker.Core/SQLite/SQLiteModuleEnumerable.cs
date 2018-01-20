using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	public class SQLiteModuleEnumerable : SQLiteModuleCommon
	{
		private IEnumerable enumerable;

		private bool objectIdentity;

		private bool disposed;

		public SQLiteModuleEnumerable(string name, IEnumerable enumerable) : this(name, enumerable, false)
		{
		}

		public SQLiteModuleEnumerable(string name, IEnumerable enumerable, bool objectIdentity) : base(name)
		{
			if (enumerable == null)
			{
				throw new ArgumentNullException("enumerable");
			}
			this.enumerable = enumerable;
			this.objectIdentity = objectIdentity;
		}

		public override SQLiteErrorCode BestIndex(SQLiteVirtualTable table, SQLiteIndex index)
		{
			this.CheckDisposed();
			if (table.BestIndex(index))
			{
				return SQLiteErrorCode.Ok;
			}
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] tableName = new object[] { table.TableName };
			this.SetTableError(table, HelperMethods.StringFormat(currentCulture, "failed to select best index for virtual table \"{0}\"", tableName));
			return SQLiteErrorCode.Error;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteModuleEnumerable).Name);
			}
		}

		public override SQLiteErrorCode Close(SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator;
			if (sQLiteVirtualTableCursorEnumerator != null)
			{
				sQLiteVirtualTableCursorEnumerator.Close();
				return SQLiteErrorCode.Ok;
			}
			return this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
		}

		public override SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator;
			if (sQLiteVirtualTableCursorEnumerator == null)
			{
				return this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
			}
			if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
			{
				return this.CursorEndOfEnumeratorError(cursor);
			}
			object current = sQLiteVirtualTableCursorEnumerator.Current;
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

		public override SQLiteErrorCode Connect(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
		{
			this.CheckDisposed();
			if (this.DeclareTable(connection, this.GetSqlForDeclareTable(), ref error) != SQLiteErrorCode.Ok)
			{
				return SQLiteErrorCode.Error;
			}
			table = new SQLiteVirtualTable(arguments);
			return SQLiteErrorCode.Ok;
		}

		public override SQLiteErrorCode Create(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
		{
			this.CheckDisposed();
			if (this.DeclareTable(connection, this.GetSqlForDeclareTable(), ref error) != SQLiteErrorCode.Ok)
			{
				return SQLiteErrorCode.Error;
			}
			table = new SQLiteVirtualTable(arguments);
			return SQLiteErrorCode.Ok;
		}

		protected virtual SQLiteErrorCode CursorEndOfEnumeratorError(SQLiteVirtualTableCursor cursor)
		{
			this.SetCursorError(cursor, "already hit end of enumerator");
			return SQLiteErrorCode.Error;
		}

		public override SQLiteErrorCode Destroy(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			table.Dispose();
			return SQLiteErrorCode.Ok;
		}

		public override SQLiteErrorCode Disconnect(SQLiteVirtualTable table)
		{
			this.CheckDisposed();
			table.Dispose();
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

		public override bool Eof(SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator;
			if (sQLiteVirtualTableCursorEnumerator != null)
			{
				return sQLiteVirtualTableCursorEnumerator.EndOfEnumerator;
			}
			return this.ResultCodeToEofResult(this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator)));
		}

		public override SQLiteErrorCode Filter(SQLiteVirtualTableCursor cursor, int indexNumber, string indexString, SQLiteValue[] values)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator;
			if (sQLiteVirtualTableCursorEnumerator == null)
			{
				return this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
			}
			sQLiteVirtualTableCursorEnumerator.Filter(indexNumber, indexString, values);
			sQLiteVirtualTableCursorEnumerator.Reset();
			sQLiteVirtualTableCursorEnumerator.MoveNext();
			return SQLiteErrorCode.Ok;
		}

		public override SQLiteErrorCode Next(SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator;
			if (sQLiteVirtualTableCursorEnumerator == null)
			{
				return this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
			}
			if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
			{
				return this.CursorEndOfEnumeratorError(cursor);
			}
			sQLiteVirtualTableCursorEnumerator.MoveNext();
			return SQLiteErrorCode.Ok;
		}

		public override SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor)
		{
			this.CheckDisposed();
			cursor = new SQLiteVirtualTableCursorEnumerator(table, this.enumerable.GetEnumerator());
			return SQLiteErrorCode.Ok;
		}

		public override SQLiteErrorCode Rename(SQLiteVirtualTable table, string newName)
		{
			this.CheckDisposed();
			if (table.Rename(newName))
			{
				return SQLiteErrorCode.Ok;
			}
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] tableName = new object[] { table.TableName, newName };
			this.SetTableError(table, HelperMethods.StringFormat(currentCulture, "failed to rename virtual table from \"{0}\" to \"{1}\"", tableName));
			return SQLiteErrorCode.Error;
		}

		public override SQLiteErrorCode RowId(SQLiteVirtualTableCursor cursor, ref long rowId)
		{
			this.CheckDisposed();
			SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator = cursor as SQLiteVirtualTableCursorEnumerator;
			if (sQLiteVirtualTableCursorEnumerator == null)
			{
				return this.CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
			}
			if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
			{
				return this.CursorEndOfEnumeratorError(cursor);
			}
			rowId = this.GetRowIdFromObject(cursor, sQLiteVirtualTableCursorEnumerator.Current);
			return SQLiteErrorCode.Ok;
		}

		public override SQLiteErrorCode Update(SQLiteVirtualTable table, SQLiteValue[] values, ref long rowId)
		{
			this.CheckDisposed();
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] tableName = new object[] { table.TableName };
			this.SetTableError(table, HelperMethods.StringFormat(currentCulture, "virtual table \"{0}\" is read-only", tableName));
			return SQLiteErrorCode.Error;
		}
	}
}