using System;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.SQLite
{
	public class SQLiteModuleCommon : SQLiteModuleNoop
	{
		private readonly static string declareSql;

		private bool objectIdentity;

		private bool disposed;

		static SQLiteModuleCommon()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] name = new object[] { typeof(SQLiteModuleCommon).Name };
			SQLiteModuleCommon.declareSql = HelperMethods.StringFormat(invariantCulture, "CREATE TABLE {0}(x);", name);
		}

		public SQLiteModuleCommon(string name) : this(name, false)
		{
		}

		public SQLiteModuleCommon(string name, bool objectIdentity) : base(name)
		{
			this.objectIdentity = objectIdentity;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteModuleCommon).Name);
			}
		}

		protected virtual SQLiteErrorCode CursorTypeMismatchError(SQLiteVirtualTableCursor cursor, Type type)
		{
			if (type == null)
			{
				this.SetCursorError(cursor, "cursor type mismatch");
			}
			else
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { type };
				this.SetCursorError(cursor, HelperMethods.StringFormat(currentCulture, "not a \"{0}\" cursor", objArray));
			}
			return SQLiteErrorCode.Error;
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

		protected virtual long GetRowIdFromObject(SQLiteVirtualTableCursor cursor, object value)
		{
			int num = (cursor != null ? cursor.GetRowIndex() : 0);
			int hashCode = SQLiteMarshal.GetHashCode(value, this.objectIdentity);
			return this.MakeRowId(num, hashCode);
		}

		protected virtual string GetSqlForDeclareTable()
		{
			return SQLiteModuleCommon.declareSql;
		}

		protected virtual string GetStringFromObject(SQLiteVirtualTableCursor cursor, object value)
		{
			if (value == null)
			{
				return null;
			}
			if (value is string)
			{
				return (string)value;
			}
			return value.ToString();
		}

		protected virtual long MakeRowId(int rowIndex, int hashCode)
		{
			return (long)((long)rowIndex << 32 | (long)hashCode);
		}
	}
}