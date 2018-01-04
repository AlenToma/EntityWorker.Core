using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Data.SQLite
{
	internal sealed class SQLite3_UTF16 : SQLite3
	{
		private bool disposed;

		internal SQLite3_UTF16(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString, IntPtr db, string fileName, bool ownHandle) : base(fmt, kind, fmtString, db, fileName, ownHandle)
		{
		}

		internal override void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt)
		{
			SQLiteStatementHandle _sqliteStmt;
			switch (this._datetimeFormat)
			{
				case SQLiteDateFormats.Ticks:
				case SQLiteDateFormats.JulianDay:
				case SQLiteDateFormats.UnixEpoch:
				{
					base.Bind_DateTime(stmt, flags, index, dt);
					return;
				}
				case SQLiteDateFormats.ISO8601:
				{
					if (HelperMethods.LogBind(flags))
					{
						if (stmt != null)
						{
							_sqliteStmt = stmt._sqlite_stmt;
						}
						else
						{
							_sqliteStmt = null;
						}
						SQLite3.LogBind(_sqliteStmt, index, dt);
					}
					this.Bind_Text(stmt, flags, index, base.ToString(dt));
					return;
				}
				default:
				{
					if (HelperMethods.LogBind(flags))
					{
						if (stmt != null)
						{
							_sqliteStmt = stmt._sqlite_stmt;
						}
						else
						{
							_sqliteStmt = null;
						}
						SQLite3.LogBind(_sqliteStmt, index, dt);
					}
					this.Bind_Text(stmt, flags, index, base.ToString(dt));
					return;
				}
			}
		}

		internal override void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text16(_sqliteStmt, index, value, value.Length * 2, (IntPtr)(-1));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLite3_UTF16).Name);
			}
		}

		internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLite3_UTF16.UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override string ColumnName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_name16_interop(stmt._sqlite_stmt, index, ref num);
			if (intPtr == IntPtr.Zero)
			{
				throw new SQLiteException(SQLiteErrorCode.NoMem, this.GetLastError());
			}
			return SQLite3_UTF16.UTF16ToString(intPtr, num);
		}

		internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLite3_UTF16.UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override string ColumnTableName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLite3_UTF16.UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16_interop(stmt._sqlite_stmt, index, ref num), num);
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

		internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
		{
			if (this._datetimeFormat == SQLiteDateFormats.Ticks)
			{
				return SQLiteConvert.TicksToDateTime(this.GetInt64(stmt, index), this._datetimeKind);
			}
			if (this._datetimeFormat == SQLiteDateFormats.JulianDay)
			{
				return SQLiteConvert.ToDateTime(this.GetDouble(stmt, index), this._datetimeKind);
			}
			if (this._datetimeFormat != SQLiteDateFormats.UnixEpoch)
			{
				return base.ToDateTime(this.GetText(stmt, index));
			}
			return SQLiteConvert.UnixEpochToDateTime(this.GetInt64(stmt, index), this._datetimeKind);
		}

		internal override string GetParamValueText(IntPtr ptr)
		{
			int num = 0;
			return SQLite3_UTF16.UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16_interop(ptr, ref num), num);
		}

		internal override string GetText(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLite3_UTF16.UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override void Open(string strFilename, string vfsName, SQLiteConnectionFlags connectionFlags, SQLiteOpenFlagsEnum openFlags, int maxPoolSize, bool usePool)
		{
			SQLiteErrorCode sQLiteErrorCode;
			if (this._sql != null)
			{
				this.Close(true);
			}
			if (this._sql != null)
			{
				throw new SQLiteException("connection handle is still active");
			}
			this._usePool = usePool;
			this._fileName = strFilename;
			this._flags = connectionFlags;
			if (usePool)
			{
				this._sql = SQLiteConnectionPool.Remove(strFilename, maxPoolSize, out this._poolVersion);
				SQLiteConnectionHandle sQLiteConnectionHandle = this._sql;
				object[] objArray = new object[] { typeof(SQLite3_UTF16), strFilename, vfsName, connectionFlags, openFlags, maxPoolSize, usePool, this._poolVersion };
				SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.OpenedFromPool, null, null, null, null, sQLiteConnectionHandle, strFilename, objArray));
			}
			if (this._sql == null)
			{
				try
				{
				}
				finally
				{
					IntPtr zero = IntPtr.Zero;
					int num = ((connectionFlags & SQLiteConnectionFlags.NoExtensionFunctions) != SQLiteConnectionFlags.NoExtensionFunctions ? 1 : 0);
					if (vfsName != null || num != 0)
					{
						sQLiteErrorCode = UnsafeNativeMethods.sqlite3_open16_interop(SQLiteConvert.ToUTF8(strFilename), SQLiteConvert.ToUTF8(vfsName), openFlags, num, ref zero);
					}
					else
					{
						if ((openFlags & SQLiteOpenFlagsEnum.Create) != SQLiteOpenFlagsEnum.Create && !File.Exists(strFilename))
						{
							throw new SQLiteException(SQLiteErrorCode.CantOpen, strFilename);
						}
						if (vfsName != null)
						{
							CultureInfo currentCulture = CultureInfo.CurrentCulture;
							object[] objArray1 = new object[] { vfsName };
							throw new SQLiteException(SQLiteErrorCode.CantOpen, HelperMethods.StringFormat(currentCulture, "cannot open using UTF-16 and VFS \"{0}\": need interop assembly", objArray1));
						}
						sQLiteErrorCode = UnsafeNativeMethods.sqlite3_open16(strFilename, ref zero);
					}
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, null);
					}
					this._sql = new SQLiteConnectionHandle(zero, true);
				}
				lock (this._sql)
				{
				}
				SQLiteConnectionHandle sQLiteConnectionHandle1 = this._sql;
				object[] objArray2 = new object[] { typeof(SQLite3_UTF16), strFilename, vfsName, connectionFlags, openFlags, maxPoolSize, usePool };
				SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteConnectionHandle1, strFilename, objArray2));
			}
			if ((connectionFlags & SQLiteConnectionFlags.NoBindFunctions) != SQLiteConnectionFlags.NoBindFunctions)
			{
				if (this._functions == null)
				{
					this._functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();
				}
				foreach (KeyValuePair<SQLiteFunctionAttribute, SQLiteFunction> value in SQLiteFunction.BindFunctions(this, connectionFlags))
				{
					this._functions[value.Key] = value.Value;
				}
			}
			this.SetTimeout(0);
			GC.KeepAlive(this._sql);
		}

		internal override void ReturnError(IntPtr context, string value)
		{
			UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length * 2);
		}

		internal override void ReturnText(IntPtr context, string value)
		{
			UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length * 2, (IntPtr)(-1));
		}

		public override string ToString(IntPtr b, int nbytelen)
		{
			this.CheckDisposed();
			return SQLite3_UTF16.UTF16ToString(b, nbytelen);
		}

		public static string UTF16ToString(IntPtr b, int nbytelen)
		{
			if (nbytelen == 0 || b == IntPtr.Zero)
			{
				return string.Empty;
			}
			if (nbytelen == -1)
			{
				return Marshal.PtrToStringUni(b);
			}
			return Marshal.PtrToStringUni(b, nbytelen / 2);
		}
	}
}