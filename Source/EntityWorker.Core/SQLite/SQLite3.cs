using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace EntityWorker.Core.SQLite
{
	internal class SQLite3 : SQLiteBase
	{
		internal const string PublicKey = "002400000480000094000000060200000024000052534131000400000100010005a288de5687c4e1b621ddff5d844727418956997f475eb829429e411aff3e93f97b70de698b972640925bdd44280df0a25a843266973704137cbb0e7441c1fe7cae4e2440ae91ab8cde3933febcb1ac48dd33b40e13c421d8215c18a4349a436dd499e3c385cc683015f886f6c10bd90115eb2bd61b67750839e3a19941dc9c";

		internal const string DesignerVersion = "1.0.106.0";

		private static object syncRoot;

		protected internal SQLiteConnectionHandle _sql;

		protected string _fileName;

		protected SQLiteConnectionFlags _flags;

		protected bool _usePool;

		protected int _poolVersion;

		private int _cancelCount;

		private bool _buildingSchema;

		protected Dictionary<SQLiteFunctionAttribute, SQLiteFunction> _functions;

		protected string _shimExtensionFileName;

		protected bool? _shimIsLoadNeeded = null;

		protected string _shimExtensionProcName = "sqlite3_vtshim_init";

		protected Dictionary<string, SQLiteModule> _modules;

		private bool disposed;

		private static bool? have_errstr;

		private static bool? have_stmt_readonly;

		private static bool? forceLogPrepare;

		internal override bool AutoCommit
		{
			get
			{
				return SQLiteBase.IsAutocommit(this._sql, this._sql);
			}
		}

		internal override int Changes
		{
			get
			{
				return UnsafeNativeMethods.sqlite3_changes_interop(this._sql);
			}
		}

		internal static string DefineConstants
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				IList<string> optionList = SQLiteDefineConstants.OptionList;
				if (optionList != null)
				{
					foreach (string str in optionList)
					{
						if (str == null)
						{
							continue;
						}
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(' ');
						}
						stringBuilder.Append(str);
					}
				}
				return stringBuilder.ToString();
			}
		}

		internal override IDictionary<SQLiteFunctionAttribute, SQLiteFunction> Functions
		{
			get
			{
				return this._functions;
			}
		}

		internal static string InteropCompileOptions
		{
			get
			{
				int num = 0;
				StringBuilder stringBuilder = new StringBuilder();
				int num1 = 0;
				int num2 = num1;
				num1 = num2 + 1;
				for (IntPtr i = UnsafeNativeMethods.interop_compileoption_get(num2); i != IntPtr.Zero; i = UnsafeNativeMethods.interop_compileoption_get(num))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append(SQLiteConvert.UTF8ToString(i, -1));
					num = num1;
					num1 = num + 1;
				}
				return stringBuilder.ToString();
			}
		}

		internal static string InteropSourceId
		{
			get
			{
				return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.interop_sourceid(), -1);
			}
		}

		internal static string InteropVersion
		{
			get
			{
				return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.interop_libversion(), -1);
			}
		}

		internal override long LastInsertRowId
		{
			get
			{
				return UnsafeNativeMethods.sqlite3_last_insert_rowid(this._sql);
			}
		}

		internal override long MemoryHighwater
		{
			get
			{
				return SQLite3.StaticMemoryHighwater;
			}
		}

		internal override long MemoryUsed
		{
			get
			{
				return SQLite3.StaticMemoryUsed;
			}
		}

		internal override bool OwnHandle
		{
			get
			{
				if (this._sql == null)
				{
					throw new SQLiteException("no connection handle available");
				}
				return this._sql.OwnHandle;
			}
		}

		internal static string SQLiteCompileOptions
		{
			get
			{
				int num = 0;
				StringBuilder stringBuilder = new StringBuilder();
				int num1 = 0;
				int num2 = num1;
				num1 = num2 + 1;
				for (IntPtr i = UnsafeNativeMethods.sqlite3_compileoption_get(num2); i != IntPtr.Zero; i = UnsafeNativeMethods.sqlite3_compileoption_get(num))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append(SQLiteConvert.UTF8ToString(i, -1));
					num = num1;
					num1 = num + 1;
				}
				return stringBuilder.ToString();
			}
		}

		internal static string SQLiteSourceId
		{
			get
			{
				return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_sourceid(), -1);
			}
		}

		internal static string SQLiteVersion
		{
			get
			{
				return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_libversion(), -1);
			}
		}

		internal static int SQLiteVersionNumber
		{
			get
			{
				return UnsafeNativeMethods.sqlite3_libversion_number();
			}
		}

		internal static long StaticMemoryHighwater
		{
			get
			{
				return UnsafeNativeMethods.sqlite3_memory_highwater(0);
			}
		}

		internal static long StaticMemoryUsed
		{
			get
			{
				return UnsafeNativeMethods.sqlite3_memory_used();
			}
		}

		internal override string Version
		{
			get
			{
				return SQLite3.SQLiteVersion;
			}
		}

		internal override int VersionNumber
		{
			get
			{
				return SQLite3.SQLiteVersionNumber;
			}
		}

		static SQLite3()
		{
			SQLite3.syncRoot = new object();
			SQLite3.have_errstr = null;
			SQLite3.have_stmt_readonly = null;
			SQLite3.forceLogPrepare = null;
		}

		internal SQLite3(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString, IntPtr db, string fileName, bool ownHandle) : base(fmt, kind, fmtString)
		{
			if (db != IntPtr.Zero)
			{
				this._sql = new SQLiteConnectionHandle(db, ownHandle);
				this._fileName = fileName;
				SQLiteConnectionHandle sQLiteConnectionHandle = this._sql;
				object[] objArray = new object[] { typeof(SQLite3), fmt, kind, fmtString, db, fileName, ownHandle };
				SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteConnectionHandle, fileName, objArray));
			}
		}

		internal override IntPtr AggregateContext(IntPtr context)
		{
			return UnsafeNativeMethods.sqlite3_aggregate_context(context, 1);
		}

		internal override int AggregateCount(IntPtr context)
		{
			return UnsafeNativeMethods.sqlite3_aggregate_count(context);
		}

		internal override void Bind_Blob(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, byte[] blobData)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, blobData);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_blob(_sqliteStmt, index, blobData, (int)blobData.Length, (IntPtr)(-1));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_Boolean(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, bool value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int(_sqliteStmt, index, (value ? 1 : 0));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt)
		{
			byte[] uTF8;
			SQLiteErrorCode sQLiteErrorCode;
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, dt);
			}
			if ((flags & SQLiteConnectionFlags.BindDateTimeWithKind) == SQLiteConnectionFlags.BindDateTimeWithKind && this._datetimeKind != DateTimeKind.Unspecified && dt.Kind != DateTimeKind.Unspecified && dt.Kind != this._datetimeKind)
			{
				if (this._datetimeKind == DateTimeKind.Utc)
				{
					dt = dt.ToUniversalTime();
				}
				else if (this._datetimeKind == DateTimeKind.Local)
				{
					dt = dt.ToLocalTime();
				}
			}
			switch (this._datetimeFormat)
			{
				case SQLiteDateFormats.Ticks:
				{
					long ticks = dt.Ticks;
					if (HelperMethods.LogBind(flags))
					{
						SQLite3.LogBind(_sqliteStmt, index, ticks);
					}
					SQLiteErrorCode sQLiteErrorCode1 = UnsafeNativeMethods.sqlite3_bind_int64(_sqliteStmt, index, ticks);
					if (sQLiteErrorCode1 == SQLiteErrorCode.Ok)
					{
						break;
					}
					throw new SQLiteException(sQLiteErrorCode1, this.GetLastError());
				}
				case SQLiteDateFormats.ISO8601:
				{
					uTF8 = base.ToUTF8(dt);
					if (HelperMethods.LogBind(flags))
					{
						SQLite3.LogBind(_sqliteStmt, index, uTF8);
					}
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text(_sqliteStmt, index, uTF8, (int)uTF8.Length - 1, (IntPtr)(-1));
					if (sQLiteErrorCode == SQLiteErrorCode.Ok)
					{
						break;
					}
					throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
				}
				case SQLiteDateFormats.JulianDay:
				{
					double julianDay = SQLiteConvert.ToJulianDay(dt);
					if (HelperMethods.LogBind(flags))
					{
						SQLite3.LogBind(_sqliteStmt, index, julianDay);
					}
					SQLiteErrorCode sQLiteErrorCode2 = UnsafeNativeMethods.sqlite3_bind_double(_sqliteStmt, index, julianDay);
					if (sQLiteErrorCode2 == SQLiteErrorCode.Ok)
					{
						break;
					}
					throw new SQLiteException(sQLiteErrorCode2, this.GetLastError());
				}
				case SQLiteDateFormats.UnixEpoch:
				{
					TimeSpan timeSpan = dt.Subtract(SQLiteConvert.UnixEpoch);
					long num = Convert.ToInt64(timeSpan.TotalSeconds);
					if (HelperMethods.LogBind(flags))
					{
						SQLite3.LogBind(_sqliteStmt, index, num);
					}
					SQLiteErrorCode sQLiteErrorCode3 = UnsafeNativeMethods.sqlite3_bind_int64(_sqliteStmt, index, num);
					if (sQLiteErrorCode3 == SQLiteErrorCode.Ok)
					{
						break;
					}
					throw new SQLiteException(sQLiteErrorCode3, this.GetLastError());
				}
				default:
				{
					uTF8 = base.ToUTF8(dt);
					if (HelperMethods.LogBind(flags))
					{
						SQLite3.LogBind(_sqliteStmt, index, uTF8);
					}
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text(_sqliteStmt, index, uTF8, (int)uTF8.Length - 1, (IntPtr)(-1));
					if (sQLiteErrorCode == SQLiteErrorCode.Ok)
					{
						break;
					}
					throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
				}
			}
		}

		internal override void Bind_Double(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, double value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_double(_sqliteStmt, index, value);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_Int32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, int value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int(_sqliteStmt, index, value);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_Int64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, long value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int64(_sqliteStmt, index, value);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_Null(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_null(_sqliteStmt, index);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override int Bind_ParamCount(SQLiteStatement stmt, SQLiteConnectionFlags flags)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			int num = UnsafeNativeMethods.sqlite3_bind_parameter_count(_sqliteStmt);
			if (HelperMethods.LogBind(flags))
			{
				IntPtr intPtr = _sqliteStmt;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { intPtr, num };
				SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Statement {0} paramter count is {1}.", objArray));
			}
			return num;
		}

		internal override int Bind_ParamIndex(SQLiteStatement stmt, SQLiteConnectionFlags flags, string paramName)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			int num = UnsafeNativeMethods.sqlite3_bind_parameter_index(_sqliteStmt, SQLiteConvert.ToUTF8(paramName));
			if (HelperMethods.LogBind(flags))
			{
				IntPtr intPtr = _sqliteStmt;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { intPtr, paramName, num };
				SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Statement {0} paramter index of name {{{1}}} is #{2}.", objArray));
			}
			return num;
		}

		internal override string Bind_ParamName(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			int num = 0;
			string str = SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name_interop(_sqliteStmt, index, ref num), num);
			if (HelperMethods.LogBind(flags))
			{
				IntPtr intPtr = _sqliteStmt;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { intPtr, index, str };
				SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Statement {0} paramter #{1} name is {{{2}}}.", objArray));
			}
			return str;
		}

		internal override void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			byte[] uTF8 = SQLiteConvert.ToUTF8(value);
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, uTF8);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text(_sqliteStmt, index, uTF8, (int)uTF8.Length - 1, (IntPtr)(-1));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_UInt32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, uint value)
		{
			SQLiteErrorCode sQLiteErrorCode;
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			sQLiteErrorCode = ((flags & SQLiteConnectionFlags.BindUInt32AsInt64) != SQLiteConnectionFlags.BindUInt32AsInt64 ? UnsafeNativeMethods.sqlite3_bind_uint(_sqliteStmt, index, value) : UnsafeNativeMethods.sqlite3_bind_int64(_sqliteStmt, index, (long)value));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void Bind_UInt64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, ulong value)
		{
			SQLiteStatementHandle _sqliteStmt = stmt._sqlite_stmt;
			if (HelperMethods.LogBind(flags))
			{
				SQLite3.LogBind(_sqliteStmt, index, value);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_uint64(_sqliteStmt, index, value);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void BindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteFunction function, SQLiteConnectionFlags flags)
		{
			if (functionAttribute == null)
			{
				throw new ArgumentNullException("functionAttribute");
			}
			if (function == null)
			{
				throw new ArgumentNullException("function");
			}
			SQLiteFunction.BindFunction(this, functionAttribute, function, flags);
			if (this._functions == null)
			{
				this._functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();
			}
			this._functions[functionAttribute] = function;
		}

		internal override void Cancel()
		{
			try
			{
			}
			finally
			{
				Interlocked.Increment(ref this._cancelCount);
				UnsafeNativeMethods.sqlite3_interrupt(this._sql);
			}
		}

		internal override void ChangePassword(byte[] newPasswordBytes)
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_rekey(this._sql, newPasswordBytes, (newPasswordBytes == null ? 0 : (int)newPasswordBytes.Length));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
			if (this._usePool)
			{
				this._usePool = false;
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLite3).Name);
			}
		}

		internal override void ClearPool()
		{
			SQLiteConnectionPool.ClearPool(this._fileName);
		}

		internal override void Close(bool canThrow)
		{
			if (this._sql != null)
			{
				if (!this._sql.OwnHandle)
				{
					this._sql = null;
					return;
				}
				bool flag = (this._flags & SQLiteConnectionFlags.UnbindFunctionsOnClose) == SQLiteConnectionFlags.UnbindFunctionsOnClose;
				if (!this._usePool)
				{
					if (flag)
					{
						SQLiteFunction.UnbindAllFunctions(this, this._flags, false);
					}
					this._sql.Dispose();
				}
				else if (SQLiteBase.ResetConnection(this._sql, this._sql, canThrow))
				{
					if (flag)
					{
						SQLiteFunction.UnbindAllFunctions(this, this._flags, false);
					}
					this.DisposeModules();
					SQLiteConnectionPool.Add(this._fileName, this._sql, this._poolVersion);
					SQLiteConnectionHandle sQLiteConnectionHandle = this._sql;
					string str = this._fileName;
					object[] objArray = new object[] { typeof(SQLite3), canThrow, this._fileName, this._poolVersion };
					SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.ClosedToPool, null, null, null, null, sQLiteConnectionHandle, str, objArray));
				}
				this._sql = null;
			}
		}

		internal override TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index)
		{
			return UnsafeNativeMethods.sqlite3_column_type(stmt._sqlite_stmt, index);
		}

		internal override int ColumnCount(SQLiteStatement stmt)
		{
			return UnsafeNativeMethods.sqlite3_column_count(stmt._sqlite_stmt);
		}

		internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override int ColumnIndex(SQLiteStatement stmt, string columnName)
		{
			int num = this.ColumnCount(stmt);
			for (int i = 0; i < num; i++)
			{
				if (string.Compare(columnName, this.ColumnName(stmt, i), StringComparison.OrdinalIgnoreCase) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		internal override void ColumnMetaData(string dataBase, string table, string column, ref string dataType, ref string collateSequence, ref bool notNull, ref bool primaryKey, ref bool autoIncrement)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_table_column_metadata_interop(this._sql, SQLiteConvert.ToUTF8(dataBase), SQLiteConvert.ToUTF8(table), SQLiteConvert.ToUTF8(column), ref zero, ref intPtr, ref num, ref num1, ref num2, ref num3, ref num4);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
			dataType = SQLiteConvert.UTF8ToString(zero, num3);
			collateSequence = SQLiteConvert.UTF8ToString(intPtr, num4);
			notNull = num == 1;
			primaryKey = num1 == 1;
			autoIncrement = num2 == 1;
		}

		internal override string ColumnName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_name_interop(stmt._sqlite_stmt, index, ref num);
			if (intPtr == IntPtr.Zero)
			{
				throw new SQLiteException(SQLiteErrorCode.NoMem, this.GetLastError());
			}
			return SQLiteConvert.UTF8ToString(intPtr, num);
		}

		internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override string ColumnTableName(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override string ColumnType(SQLiteStatement stmt, int index, ref TypeAffinity nAffinity)
		{
			int num = 0;
			IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_decltype_interop(stmt._sqlite_stmt, index, ref num);
			nAffinity = this.ColumnAffinity(stmt, index);
			if (intPtr != IntPtr.Zero && (num > 0 || num == -1))
			{
				string str = SQLiteConvert.UTF8ToString(intPtr, num);
				if (!string.IsNullOrEmpty(str))
				{
					return str;
				}
			}
			string[] typeDefinitions = stmt.TypeDefinitions;
			if (typeDefinitions == null || index >= (int)typeDefinitions.Length || typeDefinitions[index] == null)
			{
				return string.Empty;
			}
			return typeDefinitions[index];
		}

		internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2)
		{
			Encoding uTF8 = null;
			switch (enc)
			{
				case CollationEncodingEnum.UTF8:
				{
					uTF8 = Encoding.UTF8;
					break;
				}
				case CollationEncodingEnum.UTF16LE:
				{
					uTF8 = Encoding.Unicode;
					break;
				}
				case CollationEncodingEnum.UTF16BE:
				{
					uTF8 = Encoding.BigEndianUnicode;
					break;
				}
			}
			byte[] bytes = uTF8.GetBytes(s1);
			byte[] numArray = uTF8.GetBytes(s2);
			return UnsafeNativeMethods.sqlite3_context_collcompare_interop(context, bytes, (int)bytes.Length, numArray, (int)numArray.Length);
		}

		internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2)
		{
			Encoding uTF8 = null;
			switch (enc)
			{
				case CollationEncodingEnum.UTF8:
				{
					uTF8 = Encoding.UTF8;
					break;
				}
				case CollationEncodingEnum.UTF16LE:
				{
					uTF8 = Encoding.Unicode;
					break;
				}
				case CollationEncodingEnum.UTF16BE:
				{
					uTF8 = Encoding.BigEndianUnicode;
					break;
				}
			}
			byte[] bytes = uTF8.GetBytes(c1);
			byte[] numArray = uTF8.GetBytes(c2);
			return UnsafeNativeMethods.sqlite3_context_collcompare_interop(context, bytes, (int)bytes.Length, numArray, (int)numArray.Length);
		}

		internal override int CountPool()
		{
			Dictionary<string, int> strs = null;
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			SQLiteConnectionPool.GetCounts(this._fileName, ref strs, ref num, ref num1, ref num2);
			return num2;
		}

		internal override SQLiteErrorCode CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16, bool canThrow)
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_collation(this._sql, SQLiteConvert.ToUTF8(strCollation), 2, IntPtr.Zero, func16);
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_collation(this._sql, SQLiteConvert.ToUTF8(strCollation), 1, IntPtr.Zero, func);
			}
			if (canThrow && sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
			return sQLiteErrorCode;
		}

		internal override SQLiteErrorCode CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal, bool canThrow)
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_function_interop(this._sql, SQLiteConvert.ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq ? 1 : 0));
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_function_interop(this._sql, SQLiteConvert.ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq ? 1 : 0));
			}
			if (canThrow && sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
			return sQLiteErrorCode;
		}

		internal override void CreateModule(SQLiteModule module, SQLiteConnectionFlags flags)
		{
			if (module == null)
			{
				throw new ArgumentNullException("module");
			}
			if (HelperMethods.NoLogModule(flags))
			{
				module.LogErrors = HelperMethods.LogModuleError(flags);
				module.LogExceptions = HelperMethods.LogModuleException(flags);
			}
			if (this._sql == null)
			{
				throw new SQLiteException("connection has an invalid handle");
			}
			bool flag = false;
			string shimExtensionFileName = this.GetShimExtensionFileName(ref flag);
			if (flag)
			{
				if (shimExtensionFileName == null)
				{
					throw new SQLiteException("the file name for the \"vtshim\" extension is unknown");
				}
				if (this._shimExtensionProcName == null)
				{
					throw new SQLiteException("the entry point for the \"vtshim\" extension is unknown");
				}
				this.SetLoadExtension(true);
				this.LoadExtension(shimExtensionFileName, this._shimExtensionProcName);
			}
			if (!module.CreateDisposableModule(this._sql))
			{
				throw new SQLiteException(this.GetLastError());
			}
			if (this._modules == null)
			{
				this._modules = new Dictionary<string, SQLiteModule>();
			}
			this._modules.Add(module.Name, module);
			if (!this._usePool)
			{
				return;
			}
			this._usePool = false;
		}

		internal override SQLiteErrorCode DeclareVirtualFunction(SQLiteModule module, int argumentCount, string name, ref string error)
		{
			SQLiteErrorCode sQLiteErrorCode;
			if (this._sql == null)
			{
				error = "connection has an invalid handle";
				return SQLiteErrorCode.Error;
			}
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = SQLiteString.Utf8IntPtrFromString(name);
				SQLiteErrorCode sQLiteErrorCode1 = UnsafeNativeMethods.sqlite3_overload_function(this._sql, zero, argumentCount);
				if (sQLiteErrorCode1 != SQLiteErrorCode.Ok)
				{
					error = this.GetLastError();
				}
				sQLiteErrorCode = sQLiteErrorCode1;
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
			return sQLiteErrorCode;
		}

		internal override SQLiteErrorCode DeclareVirtualTable(SQLiteModule module, string strSql, ref string error)
		{
			SQLiteErrorCode sQLiteErrorCode;
			if (this._sql == null)
			{
				error = "connection has an invalid handle";
				return SQLiteErrorCode.Error;
			}
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = SQLiteString.Utf8IntPtrFromString(strSql);
				SQLiteErrorCode sQLiteErrorCode1 = UnsafeNativeMethods.sqlite3_declare_vtab(this._sql, zero);
				if (sQLiteErrorCode1 == SQLiteErrorCode.Ok && module != null)
				{
					module.Declared = true;
				}
				if (sQLiteErrorCode1 != SQLiteErrorCode.Ok)
				{
					error = this.GetLastError();
				}
				sQLiteErrorCode = sQLiteErrorCode1;
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
			return sQLiteErrorCode;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed)
				{
					this.DisposeModules();
					this.Close(false);
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		internal override void DisposeModule(SQLiteModule module, SQLiteConnectionFlags flags)
		{
			if (module == null)
			{
				throw new ArgumentNullException("module");
			}
			module.Dispose();
		}

		private void DisposeModules()
		{
			if (this._modules != null)
			{
				foreach (KeyValuePair<string, SQLiteModule> _module in this._modules)
				{
					SQLiteModule value = _module.Value;
					if (value == null)
					{
						continue;
					}
					value.Dispose();
				}
				this._modules.Clear();
			}
		}

		internal override SQLiteErrorCode ExtendedResultCode()
		{
			return UnsafeNativeMethods.sqlite3_extended_errcode(this._sql);
		}

		internal override SQLiteErrorCode FileControl(string zDbName, int op, IntPtr pArg)
		{
			byte[] uTF8;
			IntPtr intPtr = this._sql;
			if (zDbName != null)
			{
				uTF8 = SQLiteConvert.ToUTF8(zDbName);
			}
			else
			{
				uTF8 = null;
			}
			return UnsafeNativeMethods.sqlite3_file_control(intPtr, uTF8, op, pArg);
		}

		internal override void FinishBackup(SQLiteBackup backup)
		{
			if (backup == null)
			{
				throw new ArgumentNullException("backup");
			}
			SQLiteBackupHandle _sqliteBackup = backup._sqlite_backup;
			if (_sqliteBackup == null)
			{
				throw new InvalidOperationException("Backup object has an invalid handle.");
			}
			IntPtr intPtr = _sqliteBackup;
			if (intPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("Backup object has an invalid handle pointer.");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_backup_finish_interop(intPtr);
			_sqliteBackup.SetHandleAsInvalid();
			if (sQLiteErrorCode != SQLiteErrorCode.Ok && sQLiteErrorCode != backup._stepResult)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		private static bool ForceLogPrepare()
		{
			bool value;
			lock (SQLite3.syncRoot)
			{
				if (!SQLite3.forceLogPrepare.HasValue)
				{
					if (UnsafeNativeMethods.GetSettingValue("SQLite_ForceLogPrepare", null) == null)
					{
						SQLite3.forceLogPrepare = new bool?(false);
					}
					else
					{
						SQLite3.forceLogPrepare = new bool?(true);
					}
				}
				value = SQLite3.forceLogPrepare.Value;
			}
			return value;
		}

		private static string FormatDateTime(DateTime value)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"));
			stringBuilder.Append(' ');
			stringBuilder.Append(value.Kind);
			stringBuilder.Append(' ');
			stringBuilder.Append(value.Ticks);
			return stringBuilder.ToString();
		}

		internal override bool GetBoolean(SQLiteStatement stmt, int index)
		{
			return SQLiteConvert.ToBoolean(this.GetObject(stmt, index), CultureInfo.InvariantCulture, false);
		}

		internal override byte GetByte(SQLiteStatement stmt, int index)
		{
			return (byte)(this.GetInt32(stmt, index) & 255);
		}

		internal override long GetBytes(SQLiteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart, int nLength)
		{
			int num = UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index);
			if (bDest == null)
			{
				return (long)num;
			}
			int length = nLength;
			if (length + nStart > (int)bDest.Length)
			{
				length = (int)bDest.Length - nStart;
			}
			if (length + nDataOffset > num)
			{
				length = num - nDataOffset;
			}
			if (length <= 0)
			{
				length = 0;
			}
			else
			{
				IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_blob(stmt._sqlite_stmt, index);
				Marshal.Copy((IntPtr)(intPtr.ToInt64() + (long)nDataOffset), bDest, nStart, length);
			}
			return (long)length;
		}

		private int GetCancelCount()
		{
			return Interlocked.CompareExchange(ref this._cancelCount, 0, 0);
		}

		internal override char GetChar(SQLiteStatement stmt, int index)
		{
			return Convert.ToChar(this.GetUInt16(stmt, index));
		}

		internal override long GetChars(SQLiteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart, int nLength)
		{
			int length = nLength;
			string text = this.GetText(stmt, index);
			int num = text.Length;
			if (bDest == null)
			{
				return (long)num;
			}
			if (length + nStart > (int)bDest.Length)
			{
				length = (int)bDest.Length - nStart;
			}
			if (length + nDataOffset > num)
			{
				length = num - nDataOffset;
			}
			if (length <= 0)
			{
				length = 0;
			}
			else
			{
				text.CopyTo(nDataOffset, bDest, nStart, length);
			}
			return (long)length;
		}

		internal override CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context)
		{
			CollationSequence str = new CollationSequence();
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			IntPtr intPtr = UnsafeNativeMethods.sqlite3_context_collseq_interop(context, ref num1, ref num2, ref num);
			str.Name = SQLiteConvert.UTF8ToString(intPtr, num);
			str.Type = (CollationTypeEnum)num1;
			str._func = func;
			str.Encoding = (CollationEncodingEnum)num2;
			return str;
		}

		internal override int GetCursorForTable(SQLiteStatement stmt, int db, int rootPage)
		{
			return UnsafeNativeMethods.sqlite3_table_cursor_interop(stmt._sqlite_stmt, db, rootPage);
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
			if (this._datetimeFormat == SQLiteDateFormats.UnixEpoch)
			{
				return SQLiteConvert.UnixEpochToDateTime(this.GetInt64(stmt, index), this._datetimeKind);
			}
			int num = 0;
			return base.ToDateTime(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override double GetDouble(SQLiteStatement stmt, int index)
		{
			return UnsafeNativeMethods.sqlite3_column_double(stmt._sqlite_stmt, index);
		}

		internal static string GetErrorString(SQLiteErrorCode rc)
		{
			try
			{
				if (!SQLite3.have_errstr.HasValue)
				{
					SQLite3.have_errstr = new bool?(SQLite3.SQLiteVersionNumber >= 3007015);
				}
				if (SQLite3.have_errstr.Value)
				{
					IntPtr intPtr = UnsafeNativeMethods.sqlite3_errstr(rc);
					if (intPtr != IntPtr.Zero)
					{
						return Marshal.PtrToStringAnsi(intPtr);
					}
				}
			}
			catch (EntryPointNotFoundException entryPointNotFoundException)
			{
			}
			return SQLiteBase.FallbackGetErrorString(rc);
		}

		internal override string GetFileName(string dbName)
		{
			if (this._sql == null)
			{
				return null;
			}
			return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_db_filename_bytes(this._sql, SQLiteConvert.ToUTF8(dbName)), -1);
		}

		internal override void GetIndexColumnExtendedInfo(string database, string index, string column, ref int sortMode, ref int onError, ref string collationSequence)
		{
			IntPtr zero = IntPtr.Zero;
			int num = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_index_column_info_interop(this._sql, SQLiteConvert.ToUTF8(database), SQLiteConvert.ToUTF8(index), SQLiteConvert.ToUTF8(column), ref sortMode, ref onError, ref zero, ref num);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, null);
			}
			collationSequence = SQLiteConvert.UTF8ToString(zero, num);
		}

		internal override short GetInt16(SQLiteStatement stmt, int index)
		{
			return (short)(this.GetInt32(stmt, index) & 65535);
		}

		internal override int GetInt32(SQLiteStatement stmt, int index)
		{
			return UnsafeNativeMethods.sqlite3_column_int(stmt._sqlite_stmt, index);
		}

		internal override long GetInt64(SQLiteStatement stmt, int index)
		{
			return UnsafeNativeMethods.sqlite3_column_int64(stmt._sqlite_stmt, index);
		}

		internal override string GetLastError()
		{
			return this.GetLastError(null);
		}

		internal override string GetLastError(string defValue)
		{
			string lastError = SQLiteBase.GetLastError(this._sql, this._sql);
			if (string.IsNullOrEmpty(lastError))
			{
				lastError = defValue;
			}
			return lastError;
		}

		internal override object GetObject(SQLiteStatement stmt, int index)
		{
			switch (this.ColumnAffinity(stmt, index))
			{
				case TypeAffinity.Int64:
				{
					return this.GetInt64(stmt, index);
				}
				case TypeAffinity.Double:
				{
					return this.GetDouble(stmt, index);
				}
				case TypeAffinity.Text:
				{
					return this.GetText(stmt, index);
				}
				case TypeAffinity.Blob:
				{
					long bytes = this.GetBytes(stmt, index, 0, null, 0, 0);
					if (bytes <= (long)0 || bytes > (long)2147483647)
					{
						break;
					}
					byte[] numArray = new byte[(int)bytes];
					this.GetBytes(stmt, index, 0, numArray, 0, (int)bytes);
					return numArray;
				}
				case TypeAffinity.Null:
				{
					return DBNull.Value;
				}
			}
			throw new NotImplementedException();
		}

		internal override long GetParamValueBytes(IntPtr p, int nDataOffset, byte[] bDest, int nStart, int nLength)
		{
			int num = UnsafeNativeMethods.sqlite3_value_bytes(p);
			if (bDest == null)
			{
				return (long)num;
			}
			int length = nLength;
			if (length + nStart > (int)bDest.Length)
			{
				length = (int)bDest.Length - nStart;
			}
			if (length + nDataOffset > num)
			{
				length = num - nDataOffset;
			}
			if (length <= 0)
			{
				length = 0;
			}
			else
			{
				IntPtr intPtr = UnsafeNativeMethods.sqlite3_value_blob(p);
				Marshal.Copy((IntPtr)(intPtr.ToInt64() + (long)nDataOffset), bDest, nStart, length);
			}
			return (long)length;
		}

		internal override double GetParamValueDouble(IntPtr ptr)
		{
			return UnsafeNativeMethods.sqlite3_value_double(ptr);
		}

		internal override int GetParamValueInt32(IntPtr ptr)
		{
			return UnsafeNativeMethods.sqlite3_value_int(ptr);
		}

		internal override long GetParamValueInt64(IntPtr ptr)
		{
			return UnsafeNativeMethods.sqlite3_value_int64(ptr);
		}

		internal override string GetParamValueText(IntPtr ptr)
		{
			int num = 0;
			return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_value_text_interop(ptr, ref num), num);
		}

		internal override TypeAffinity GetParamValueType(IntPtr ptr)
		{
			return UnsafeNativeMethods.sqlite3_value_type(ptr);
		}

		internal override long GetRowIdForCursor(SQLiteStatement stmt, int cursor)
		{
			long num = (long)0;
			if (UnsafeNativeMethods.sqlite3_cursor_rowid_interop(stmt._sqlite_stmt, cursor, ref num) == SQLiteErrorCode.Ok)
			{
				return num;
			}
			return (long)0;
		}

		internal override sbyte GetSByte(SQLiteStatement stmt, int index)
		{
			return (sbyte)(this.GetInt32(stmt, index) & 255);
		}

		private string GetShimExtensionFileName(ref bool isLoadNeeded)
		{
			if (!this._shimIsLoadNeeded.HasValue)
			{
				isLoadNeeded = HelperMethods.IsWindows();
			}
			else
			{
				isLoadNeeded = this._shimIsLoadNeeded.Value;
			}
			string str = this._shimExtensionFileName;
			if (str != null)
			{
				return str;
			}
			return UnsafeNativeMethods.GetNativeLibraryFileNameOnly();
		}

		internal override string GetText(SQLiteStatement stmt, int index)
		{
			int num = 0;
			return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, ref num), num);
		}

		internal override ushort GetUInt16(SQLiteStatement stmt, int index)
		{
			return (ushort)(this.GetInt32(stmt, index) & 65535);
		}

		internal override uint GetUInt32(SQLiteStatement stmt, int index)
		{
			return (uint)this.GetInt32(stmt, index);
		}

		internal override ulong GetUInt64(SQLiteStatement stmt, int index)
		{
			return (ulong)this.GetInt64(stmt, index);
		}

		internal override object GetValue(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, SQLiteType typ)
		{
			TypeAffinity affinity = typ.Affinity;
			if (affinity == TypeAffinity.Null)
			{
				return DBNull.Value;
			}
			Type type = null;
			if (typ.Type != DbType.Object)
			{
				type = SQLiteConvert.SQLiteTypeToType(typ);
				affinity = SQLiteConvert.TypeToAffinity(type, flags);
			}
			if ((flags & SQLiteConnectionFlags.GetAllAsText) == SQLiteConnectionFlags.GetAllAsText)
			{
				return this.GetText(stmt, index);
			}
			TypeAffinity typeAffinity = affinity;
			switch (typeAffinity)
			{
				case TypeAffinity.Int64:
				{
					if (type == null)
					{
						return this.GetInt64(stmt, index);
					}
					if (type == typeof(bool))
					{
						return this.GetBoolean(stmt, index);
					}
					if (type == typeof(sbyte))
					{
						return this.GetSByte(stmt, index);
					}
					if (type == typeof(byte))
					{
						return this.GetByte(stmt, index);
					}
					if (type == typeof(short))
					{
						return this.GetInt16(stmt, index);
					}
					if (type == typeof(ushort))
					{
						return this.GetUInt16(stmt, index);
					}
					if (type == typeof(int))
					{
						return this.GetInt32(stmt, index);
					}
					if (type == typeof(uint))
					{
						return this.GetUInt32(stmt, index);
					}
					if (type == typeof(long))
					{
						return this.GetInt64(stmt, index);
					}
					if (type == typeof(ulong))
					{
						return this.GetUInt64(stmt, index);
					}
					return Convert.ChangeType(this.GetInt64(stmt, index), type, null);
				}
				case TypeAffinity.Double:
				{
					if (type == null)
					{
						return this.GetDouble(stmt, index);
					}
					return Convert.ChangeType(this.GetDouble(stmt, index), type, null);
				}
				case TypeAffinity.Text:
				{
					return this.GetText(stmt, index);
				}
				case TypeAffinity.Blob:
				{
					if (typ.Type == DbType.Guid && typ.Affinity == TypeAffinity.Text)
					{
						return new Guid(this.GetText(stmt, index));
					}
					int bytes = (int)this.GetBytes(stmt, index, 0, null, 0, 0);
					byte[] numArray = new byte[bytes];
					this.GetBytes(stmt, index, 0, numArray, 0, bytes);
					if (typ.Type != DbType.Guid || bytes != 16)
					{
						return numArray;
					}
					return new Guid(numArray);
				}
				default:
				{
					if (typeAffinity == TypeAffinity.DateTime)
					{
						return this.GetDateTime(stmt, index);
					}
					return this.GetText(stmt, index);
				}
			}
		}

		internal override SQLiteBackup InitializeBackup(SQLiteConnection destCnn, string destName, string sourceName)
		{
			if (destCnn == null)
			{
				throw new ArgumentNullException("destCnn");
			}
			if (destName == null)
			{
				throw new ArgumentNullException("destName");
			}
			if (sourceName == null)
			{
				throw new ArgumentNullException("sourceName");
			}
			SQLite3 sQLite3 = destCnn._sql as SQLite3;
			if (sQLite3 == null)
			{
				throw new ArgumentException("Destination connection has no wrapper.", "destCnn");
			}
			SQLiteConnectionHandle sQLiteConnectionHandle = sQLite3._sql;
			if (sQLiteConnectionHandle == null)
			{
				throw new ArgumentException("Destination connection has an invalid handle.", "destCnn");
			}
			SQLiteConnectionHandle sQLiteConnectionHandle1 = this._sql;
			if (sQLiteConnectionHandle1 == null)
			{
				throw new InvalidOperationException("Source connection has an invalid handle.");
			}
			byte[] uTF8 = SQLiteConvert.ToUTF8(destName);
			byte[] numArray = SQLiteConvert.ToUTF8(sourceName);
			SQLiteBackupHandle sQLiteBackupHandle = null;
			try
			{
			}
			finally
			{
				IntPtr intPtr = UnsafeNativeMethods.sqlite3_backup_init(sQLiteConnectionHandle, uTF8, sQLiteConnectionHandle1, numArray);
				if (intPtr == IntPtr.Zero)
				{
					SQLiteErrorCode sQLiteErrorCode = this.ResultCode();
					if (sQLiteErrorCode == SQLiteErrorCode.Ok)
					{
						throw new SQLiteException("failed to initialize backup");
					}
					throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
				}
				sQLiteBackupHandle = new SQLiteBackupHandle(sQLiteConnectionHandle, intPtr);
			}
			object[] objArray = new object[] { typeof(SQLite3), destCnn, destName, sourceName };
			SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteBackupHandle, null, objArray));
			return new SQLiteBackup(this, sQLiteBackupHandle, sQLiteConnectionHandle, uTF8, sQLiteConnectionHandle1, numArray);
		}

		internal override bool IsInitialized()
		{
			return SQLite3.StaticIsInitialized();
		}

		internal override bool IsNull(SQLiteStatement stmt, int index)
		{
			return this.ColumnAffinity(stmt, index) == TypeAffinity.Null;
		}

		internal override bool IsOpen()
		{
			if (this._sql == null || this._sql.IsInvalid)
			{
				return false;
			}
			return !this._sql.IsClosed;
		}

		internal override bool IsReadOnly(string name)
		{
			bool flag;
			IntPtr zero = IntPtr.Zero;
			try
			{
				if (name != null)
				{
					zero = SQLiteString.Utf8IntPtrFromString(name);
				}
				int num = UnsafeNativeMethods.sqlite3_db_readonly(this._sql, zero);
				if (num == -1)
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray = new object[] { name };
					throw new SQLiteException(HelperMethods.StringFormat(currentCulture, "database \"{0}\" not found", objArray));
				}
				flag = (num == 0 ? false : true);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
			return flag;
		}

		internal override bool IsReadOnly(SQLiteStatement stmt)
		{
			try
			{
				if (!SQLite3.have_stmt_readonly.HasValue)
				{
					SQLite3.have_stmt_readonly = new bool?(SQLite3.SQLiteVersionNumber >= 3007004);
				}
				if (SQLite3.have_stmt_readonly.Value)
				{
					return UnsafeNativeMethods.sqlite3_stmt_readonly(stmt._sqlite_stmt) != 0;
				}
			}
			catch (EntryPointNotFoundException entryPointNotFoundException)
			{
			}
			return false;
		}

		internal override void LoadExtension(string fileName, string procName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			IntPtr zero = IntPtr.Zero;
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(fileName, '\0'));
				byte[] numArray = null;
				if (procName != null)
				{
					numArray = Encoding.UTF8.GetBytes(string.Concat(procName, '\0'));
				}
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_load_extension(this._sql, bytes, numArray, ref zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, SQLiteConvert.UTF8ToString(zero, -1));
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.sqlite3_free(zero);
					zero = IntPtr.Zero;
				}
			}
		}

		protected static void LogBind(SQLiteStatementHandle handle, int index)
		{
			IntPtr intPtr = handle;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { intPtr, index };
			SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Binding statement {0} paramter #{1} as NULL...", objArray));
		}

		protected static void LogBind(SQLiteStatementHandle handle, int index, System.ValueType value)
		{
			IntPtr intPtr = handle;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { intPtr, index, value.GetType(), value };
			SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", objArray));
		}

		protected static void LogBind(SQLiteStatementHandle handle, int index, DateTime value)
		{
			IntPtr intPtr = handle;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { intPtr, index, typeof(DateTime), SQLite3.FormatDateTime(value) };
			SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", objArray));
		}

		protected static void LogBind(SQLiteStatementHandle handle, int index, string value)
		{
			IntPtr intPtr = handle;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { intPtr, index, typeof(string), null };
			objArray[3] = (value != null ? value : "<null>");
			SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", objArray));
		}

		protected static void LogBind(SQLiteStatementHandle handle, int index, byte[] value)
		{
			IntPtr intPtr = handle;
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { intPtr, index, typeof(byte[]), null };
			objArray[3] = (value != null ? SQLite3.ToHexadecimalString(value) : "<null>");
			SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", objArray));
		}

		internal override void LogMessage(SQLiteErrorCode iErrCode, string zMessage)
		{
			SQLite3.StaticLogMessage(iErrCode, zMessage);
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
				object[] objArray = new object[] { typeof(SQLite3), strFilename, vfsName, connectionFlags, openFlags, maxPoolSize, usePool, this._poolVersion };
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
					sQLiteErrorCode = (num == 0 ? UnsafeNativeMethods.sqlite3_open_v2(SQLiteConvert.ToUTF8(strFilename), ref zero, openFlags, SQLiteConvert.ToUTF8(vfsName)) : UnsafeNativeMethods.sqlite3_open_interop(SQLiteConvert.ToUTF8(strFilename), SQLiteConvert.ToUTF8(vfsName), openFlags, num, ref zero));
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
				object[] objArray1 = new object[] { typeof(SQLite3), strFilename, vfsName, connectionFlags, openFlags, maxPoolSize, usePool };
				SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteConnectionHandle1, strFilename, objArray1));
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

		internal override int PageCountBackup(SQLiteBackup backup)
		{
			if (backup == null)
			{
				throw new ArgumentNullException("backup");
			}
			SQLiteBackupHandle _sqliteBackup = backup._sqlite_backup;
			if (_sqliteBackup == null)
			{
				throw new InvalidOperationException("Backup object has an invalid handle.");
			}
			IntPtr intPtr = _sqliteBackup;
			if (intPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("Backup object has an invalid handle pointer.");
			}
			return UnsafeNativeMethods.sqlite3_backup_pagecount(intPtr);
		}

		internal override SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, ref string strRemain)
		{
			SQLiteStatement sQLiteStatement;
			string str;
			if (!string.IsNullOrEmpty(strSql))
			{
				strSql = strSql.Trim();
			}
			if (!string.IsNullOrEmpty(strSql))
			{
				if (cnn != null)
				{
					str = cnn._baseSchemaName;
				}
				else
				{
					str = null;
				}
				string str1 = str;
				if (!string.IsNullOrEmpty(str1))
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { str1 };
					strSql = strSql.Replace(HelperMethods.StringFormat(invariantCulture, "[{0}].", objArray), string.Empty);
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray1 = new object[] { str1 };
					strSql = strSql.Replace(HelperMethods.StringFormat(cultureInfo, "{0}.", objArray1), string.Empty);
				}
			}
			SQLiteConnectionFlags sQLiteConnectionFlag = (cnn != null ? cnn.Flags : SQLiteConnectionFlags.Default);
			if (SQLite3.ForceLogPrepare() || HelperMethods.LogPrepare(sQLiteConnectionFlag))
			{
				if (strSql == null || strSql.Length == 0 || strSql.Trim().Length == 0)
				{
					SQLiteLog.LogMessage("Preparing {<nothing>}...");
				}
				else
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray2 = new object[] { strSql };
					SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Preparing {{{0}}}...", objArray2));
				}
			}
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			int num = 0;
			SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Schema;
			int num1 = 0;
			int num2 = (cnn != null ? cnn._prepareRetries : 3);
			byte[] uTF8 = SQLiteConvert.ToUTF8(strSql);
			string str2 = null;
			SQLiteStatement sQLiteStatement1 = null;
			Random random = null;
			uint tickCount = (uint)Environment.TickCount;
			this.ResetCancelCount();
			GCHandle gCHandle = GCHandle.Alloc(uTF8, GCHandleType.Pinned);
			IntPtr intPtr1 = gCHandle.AddrOfPinnedObject();
			SQLiteStatementHandle sQLiteStatementHandle = null;
			try
			{
				while ((sQLiteErrorCode == SQLiteErrorCode.Schema || sQLiteErrorCode == SQLiteErrorCode.Locked || sQLiteErrorCode == SQLiteErrorCode.Busy) && num1 < num2)
				{
					try
					{
					}
					finally
					{
						zero = IntPtr.Zero;
						intPtr = IntPtr.Zero;
						num = 0;
						sQLiteErrorCode = UnsafeNativeMethods.sqlite3_prepare_interop(this._sql, intPtr1, (int)uTF8.Length - 1, ref zero, ref intPtr, ref num);
						if (sQLiteErrorCode == SQLiteErrorCode.Ok && zero != IntPtr.Zero)
						{
							if (sQLiteStatementHandle != null)
							{
								sQLiteStatementHandle.Dispose();
							}
							sQLiteStatementHandle = new SQLiteStatementHandle(this._sql, zero);
						}
					}
					if (sQLiteStatementHandle != null)
					{
						object[] objArray3 = new object[] { typeof(SQLite3), cnn, strSql, previous, timeoutMS };
						SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteStatementHandle, strSql, objArray3));
					}
					if (this.ShouldThrowForCancel())
					{
						if (sQLiteErrorCode == SQLiteErrorCode.Ok || sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
						{
							sQLiteErrorCode = SQLiteErrorCode.Interrupt;
						}
						throw new SQLiteException(sQLiteErrorCode, null);
					}
					if (sQLiteErrorCode == SQLiteErrorCode.Interrupt)
					{
						break;
					}
					if (sQLiteErrorCode == SQLiteErrorCode.Schema)
					{
						num1++;
					}
					else if (sQLiteErrorCode != SQLiteErrorCode.Error)
					{
						if (sQLiteErrorCode != SQLiteErrorCode.Locked && sQLiteErrorCode != SQLiteErrorCode.Busy)
						{
							continue;
						}
						if (random == null)
						{
							random = new Random();
						}
						if (Environment.TickCount - tickCount > timeoutMS)
						{
							throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
						}
						Thread.Sleep(random.Next(1, 150));
					}
					else if (string.Compare(this.GetLastError(), "near \"TYPES\": syntax error", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (this._buildingSchema || string.Compare(this.GetLastError(), 0, "no such table: TEMP.SCHEMA", 0, 26, StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
						strRemain = string.Empty;
						this._buildingSchema = true;
						try
						{
							ISQLiteSchemaExtensions service = ((IServiceProvider)SQLiteFactory.Instance).GetService(typeof(ISQLiteSchemaExtensions)) as ISQLiteSchemaExtensions;
							if (service != null)
							{
								service.BuildTempSchema(cnn);
							}
							while (sQLiteStatement1 == null && strSql.Length > 0)
							{
								sQLiteStatement1 = this.Prepare(cnn, strSql, previous, timeoutMS, ref strRemain);
								strSql = strRemain;
							}
							sQLiteStatement = sQLiteStatement1;
							return sQLiteStatement;
						}
						finally
						{
							this._buildingSchema = false;
						}
					}
					else
					{
						int length = strSql.IndexOf(';');
						if (length == -1)
						{
							length = strSql.Length - 1;
						}
						str2 = strSql.Substring(0, length + 1);
						strSql = strSql.Substring(length + 1);
						strRemain = string.Empty;
						while (sQLiteStatement1 == null && strSql.Length > 0)
						{
							sQLiteStatement1 = this.Prepare(cnn, strSql, previous, timeoutMS, ref strRemain);
							strSql = strRemain;
						}
						if (sQLiteStatement1 != null)
						{
							sQLiteStatement1.SetTypes(str2);
						}
						sQLiteStatement = sQLiteStatement1;
						return sQLiteStatement;
					}
				}
				if (this.ShouldThrowForCancel())
				{
					if (sQLiteErrorCode == SQLiteErrorCode.Ok || sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
					{
						sQLiteErrorCode = SQLiteErrorCode.Interrupt;
					}
					throw new SQLiteException(sQLiteErrorCode, null);
				}
				if (sQLiteErrorCode != SQLiteErrorCode.Interrupt)
				{
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
					}
					strRemain = SQLiteConvert.UTF8ToString(intPtr, num);
					if (sQLiteStatementHandle != null)
					{
						sQLiteStatement1 = new SQLiteStatement(this, sQLiteConnectionFlag, sQLiteStatementHandle, strSql.Substring(0, strSql.Length - strRemain.Length), previous);
					}
					sQLiteStatement = sQLiteStatement1;
				}
				else
				{
					sQLiteStatement = null;
				}
			}
			finally
			{
				gCHandle.Free();
			}
			return sQLiteStatement;
		}

		internal override SQLiteErrorCode ReleaseMemory()
		{
			return UnsafeNativeMethods.sqlite3_db_release_memory(this._sql);
		}

		internal override int RemainingBackup(SQLiteBackup backup)
		{
			if (backup == null)
			{
				throw new ArgumentNullException("backup");
			}
			SQLiteBackupHandle _sqliteBackup = backup._sqlite_backup;
			if (_sqliteBackup == null)
			{
				throw new InvalidOperationException("Backup object has an invalid handle.");
			}
			IntPtr intPtr = _sqliteBackup;
			if (intPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("Backup object has an invalid handle pointer.");
			}
			return UnsafeNativeMethods.sqlite3_backup_remaining(intPtr);
		}

		internal override SQLiteErrorCode Reset(SQLiteStatement stmt)
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_reset_interop(stmt._sqlite_stmt);
			if (sQLiteErrorCode != SQLiteErrorCode.Schema)
			{
				if (sQLiteErrorCode == SQLiteErrorCode.Locked || sQLiteErrorCode == SQLiteErrorCode.Busy)
				{
					return sQLiteErrorCode;
				}
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
				}
				return sQLiteErrorCode;
			}
			string str = null;
			using (SQLiteStatement sQLiteStatement = this.Prepare(null, stmt._sqlStatement, null, (uint)(stmt._command._commandTimeout * 1000), ref str))
			{
				stmt._sqlite_stmt.Dispose();
				if (sQLiteStatement != null)
				{
					stmt._sqlite_stmt = sQLiteStatement._sqlite_stmt;
					sQLiteStatement._sqlite_stmt = null;
				}
				stmt.BindParameters();
			}
			return SQLiteErrorCode.Unknown;
		}

		private int ResetCancelCount()
		{
			return Interlocked.CompareExchange(ref this._cancelCount, 0, this._cancelCount);
		}

		internal override SQLiteErrorCode ResultCode()
		{
			return UnsafeNativeMethods.sqlite3_errcode(this._sql);
		}

		internal override void ReturnBlob(IntPtr context, byte[] value)
		{
			UnsafeNativeMethods.sqlite3_result_blob(context, value, (int)value.Length, (IntPtr)(-1));
		}

		internal override void ReturnDouble(IntPtr context, double value)
		{
			UnsafeNativeMethods.sqlite3_result_double(context, value);
		}

		internal override void ReturnError(IntPtr context, string value)
		{
			UnsafeNativeMethods.sqlite3_result_error(context, SQLiteConvert.ToUTF8(value), value.Length);
		}

		internal override void ReturnInt32(IntPtr context, int value)
		{
			UnsafeNativeMethods.sqlite3_result_int(context, value);
		}

		internal override void ReturnInt64(IntPtr context, long value)
		{
			UnsafeNativeMethods.sqlite3_result_int64(context, value);
		}

		internal override void ReturnNull(IntPtr context)
		{
			UnsafeNativeMethods.sqlite3_result_null(context);
		}

		internal override void ReturnText(IntPtr context, string value)
		{
			byte[] uTF8 = SQLiteConvert.ToUTF8(value);
			UnsafeNativeMethods.sqlite3_result_text(context, SQLiteConvert.ToUTF8(value), (int)uTF8.Length - 1, (IntPtr)(-1));
		}

		internal override void SetAuthorizerHook(SQLiteAuthorizerCallback func)
		{
			UnsafeNativeMethods.sqlite3_set_authorizer(this._sql, func, IntPtr.Zero);
		}

		internal override void SetCommitHook(SQLiteCommitCallback func)
		{
			UnsafeNativeMethods.sqlite3_commit_hook(this._sql, func, IntPtr.Zero);
		}

		internal override SQLiteErrorCode SetConfigurationOption(SQLiteConfigDbOpsEnum option, bool bOnOff)
		{
			if (option < SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FKEY || option > SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FKEY, SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_TRIGGER, SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FTS3_TOKENIZER, SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION };
				throw new SQLiteException(HelperMethods.StringFormat(currentCulture, "unsupported configuration option, must be: {0}, {1}, {2}, or {3}", objArray));
			}
			int num = 0;
			return UnsafeNativeMethods.sqlite3_db_config_int_refint(this._sql, option, (bOnOff ? 1 : 0), ref num);
		}

		internal override void SetExtendedResultCodes(bool bOnOff)
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_extended_result_codes(this._sql, (bOnOff ? -1 : 0));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void SetLoadExtension(bool bOnOff)
		{
			SQLiteErrorCode sQLiteErrorCode;
			sQLiteErrorCode = (SQLite3.SQLiteVersionNumber < 3013000 ? UnsafeNativeMethods.sqlite3_enable_load_extension(this._sql, (bOnOff ? -1 : 0)) : this.SetConfigurationOption(SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION, bOnOff));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override SQLiteErrorCode SetLogCallback(SQLiteLogCallback func)
		{
			return UnsafeNativeMethods.sqlite3_config_log(SQLiteConfigOpsEnum.SQLITE_CONFIG_LOG, func, IntPtr.Zero);
		}

		internal override SQLiteErrorCode SetMemoryStatus(bool value)
		{
			return SQLite3.StaticSetMemoryStatus(value);
		}

		internal override void SetPassword(byte[] passwordBytes)
		{
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_key(this._sql, passwordBytes, (int)passwordBytes.Length);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
			if (this._usePool)
			{
				this._usePool = false;
			}
		}

		internal override void SetProgressHook(int nOps, SQLiteProgressCallback func)
		{
			UnsafeNativeMethods.sqlite3_progress_handler(this._sql, nOps, func, IntPtr.Zero);
		}

		internal override void SetRollbackHook(SQLiteRollbackCallback func)
		{
			UnsafeNativeMethods.sqlite3_rollback_hook(this._sql, func, IntPtr.Zero);
		}

		internal override void SetTimeout(int nTimeoutMS)
		{
			IntPtr intPtr = this._sql;
			if (intPtr == IntPtr.Zero)
			{
				throw new SQLiteException("no connection handle available");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_busy_timeout(intPtr, nTimeoutMS);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
		}

		internal override void SetTraceCallback(SQLiteTraceCallback func)
		{
			UnsafeNativeMethods.sqlite3_trace(this._sql, func, IntPtr.Zero);
		}

		internal override void SetUpdateHook(SQLiteUpdateCallback func)
		{
			UnsafeNativeMethods.sqlite3_update_hook(this._sql, func, IntPtr.Zero);
		}

		private bool ShouldThrowForCancel()
		{
			return this.GetCancelCount() > 0;
		}

		internal override SQLiteErrorCode Shutdown()
		{
			return SQLite3.StaticShutdown(false);
		}

		internal static bool StaticIsInitialized()
		{
			bool flag;
			lock (SQLite3.syncRoot)
			{
				bool enabled = SQLiteLog.Enabled;
				SQLiteLog.Enabled = false;
				try
				{
					flag = UnsafeNativeMethods.sqlite3_config_none(SQLiteConfigOpsEnum.SQLITE_CONFIG_NONE) == SQLiteErrorCode.Misuse;
				}
				finally
				{
					SQLiteLog.Enabled = enabled;
				}
			}
			return flag;
		}

		internal static void StaticLogMessage(SQLiteErrorCode iErrCode, string zMessage)
		{
			UnsafeNativeMethods.sqlite3_log(iErrCode, SQLiteConvert.ToUTF8(zMessage));
		}

		internal static SQLiteErrorCode StaticReleaseMemory(int nBytes, bool reset, bool compact, ref int nFree, ref bool resetOk, ref uint nLargest)
		{
			SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Ok;
			int num = UnsafeNativeMethods.sqlite3_release_memory(nBytes);
			uint num1 = 0;
			bool flag = false;
			if (HelperMethods.IsWindows())
			{
				if (sQLiteErrorCode == SQLiteErrorCode.Ok && reset)
				{
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_reset_heap();
					if (sQLiteErrorCode == SQLiteErrorCode.Ok)
					{
						flag = true;
					}
				}
				if (sQLiteErrorCode == SQLiteErrorCode.Ok && compact)
				{
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_compact_heap(ref num1);
				}
			}
			else if (reset || compact)
			{
				sQLiteErrorCode = SQLiteErrorCode.NotFound;
			}
			nFree = num;
			nLargest = num1;
			resetOk = flag;
			return sQLiteErrorCode;
		}

		internal static SQLiteErrorCode StaticSetMemoryStatus(bool value)
		{
			return UnsafeNativeMethods.sqlite3_config_int(SQLiteConfigOpsEnum.SQLITE_CONFIG_MEMSTATUS, (value ? 1 : 0));
		}

		internal static SQLiteErrorCode StaticShutdown(bool directories)
		{
			SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Ok;
			if (directories && HelperMethods.IsWindows())
			{
				if (sQLiteErrorCode == SQLiteErrorCode.Ok)
				{
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_set_directory(1, null);
				}
				if (sQLiteErrorCode == SQLiteErrorCode.Ok)
				{
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_set_directory(2, null);
				}
			}
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_shutdown();
			}
			return sQLiteErrorCode;
		}

		internal override bool Step(SQLiteStatement stmt)
		{
			SQLiteErrorCode sQLiteErrorCode = (SQLiteErrorCode)0;
			SQLiteErrorCode sQLiteErrorCode1;
			Random random = null;
			uint tickCount = (uint)Environment.TickCount;
			uint num = (uint)(stmt._command._commandTimeout * 1000);
			this.ResetCancelCount();
			while (true)
			{
				try
				{
				}
				finally
				{
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_step(stmt._sqlite_stmt);
				}
				if (this.ShouldThrowForCancel())
				{
					if (sQLiteErrorCode == SQLiteErrorCode.Ok || sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
					{
						sQLiteErrorCode = SQLiteErrorCode.Interrupt;
					}
					throw new SQLiteException(sQLiteErrorCode, null);
				}
				if (sQLiteErrorCode == SQLiteErrorCode.Interrupt)
				{
					return false;
				}
				if (sQLiteErrorCode == SQLiteErrorCode.Row)
				{
					return true;
				}
				if (sQLiteErrorCode == SQLiteErrorCode.Done)
				{
					return false;
				}
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					sQLiteErrorCode1 = this.Reset(stmt);
					if (sQLiteErrorCode1 == SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
					}
					if ((sQLiteErrorCode1 == SQLiteErrorCode.Locked || sQLiteErrorCode1 == SQLiteErrorCode.Busy) && stmt._command != null)
					{
						if (random == null)
						{
							random = new Random();
						}
						if (Environment.TickCount - tickCount > num)
						{
							break;
						}
						Thread.Sleep(random.Next(1, 150));
					}
				}
			}
			throw new SQLiteException(sQLiteErrorCode1, this.GetLastError());
		}

		internal override bool StepBackup(SQLiteBackup backup, int nPage, ref bool retry)
		{
			retry = false;
			if (backup == null)
			{
				throw new ArgumentNullException("backup");
			}
			SQLiteBackupHandle _sqliteBackup = backup._sqlite_backup;
			if (_sqliteBackup == null)
			{
				throw new InvalidOperationException("Backup object has an invalid handle.");
			}
			IntPtr intPtr = _sqliteBackup;
			if (intPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("Backup object has an invalid handle pointer.");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_backup_step(intPtr, nPage);
			backup._stepResult = sQLiteErrorCode;
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				return true;
			}
			if (sQLiteErrorCode == SQLiteErrorCode.Busy)
			{
				retry = true;
				return true;
			}
			if (sQLiteErrorCode == SQLiteErrorCode.Locked)
			{
				retry = true;
				return true;
			}
			if (sQLiteErrorCode != SQLiteErrorCode.Done)
			{
				throw new SQLiteException(sQLiteErrorCode, this.GetLastError());
			}
			return false;
		}

		private static string ToHexadecimalString(byte[] array)
		{
			if (array == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder((int)array.Length * 2);
			int length = (int)array.Length;
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		internal override bool UnbindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteConnectionFlags flags)
		{
			SQLiteFunction sQLiteFunction;
			if (functionAttribute == null)
			{
				throw new ArgumentNullException("functionAttribute");
			}
			if (this._functions == null)
			{
				return false;
			}
			if (this._functions.TryGetValue(functionAttribute, out sQLiteFunction) && SQLiteFunction.UnbindFunction(this, functionAttribute, sQLiteFunction, flags) && this._functions.Remove(functionAttribute))
			{
				return true;
			}
			return false;
		}
	}
}