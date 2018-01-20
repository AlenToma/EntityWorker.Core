using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	internal abstract class SQLiteBase : SQLiteConvert, IDisposable
	{
		internal const int COR_E_EXCEPTION = -2146233088;

		private bool disposed;

		private static string[] _errorMessages;

		internal abstract bool AutoCommit
		{
			get;
		}

		internal abstract int Changes
		{
			get;
		}

		internal abstract IDictionary<SQLiteFunctionAttribute, SQLiteFunction> Functions
		{
			get;
		}

		internal abstract long LastInsertRowId
		{
			get;
		}

		internal abstract long MemoryHighwater
		{
			get;
		}

		internal abstract long MemoryUsed
		{
			get;
		}

		internal abstract bool OwnHandle
		{
			get;
		}

		internal abstract string Version
		{
			get;
		}

		internal abstract int VersionNumber
		{
			get;
		}

		static SQLiteBase()
		{
			string[] strArrays = new string[] { "not an error", "SQL logic error or missing database", "internal logic error", "access permission denied", "callback requested query abort", "database is locked", "database table is locked", "out of memory", "attempt to write a readonly database", "interrupted", "disk I/O error", "database disk image is malformed", "unknown operation", "database or disk is full", "unable to open database file", "locking protocol", "table contains no data", "database schema has changed", "string or blob too big", "constraint failed", "datatype mismatch", "library routine called out of sequence", "large file support is disabled", "authorization denied", "auxiliary database format error", "bind or column index out of range", "file is encrypted or is not a database", "notification message", "warning message" };
			SQLiteBase._errorMessages = strArrays;
		}

		internal SQLiteBase(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString) : base(fmt, kind, fmtString)
		{
		}

		internal abstract IntPtr AggregateContext(IntPtr context);

		internal abstract int AggregateCount(IntPtr context);

		internal abstract void Bind_Blob(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, byte[] blobData);

		internal abstract void Bind_Boolean(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, bool value);

		internal abstract void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt);

		internal abstract void Bind_Double(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, double value);

		internal abstract void Bind_Int32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, int value);

		internal abstract void Bind_Int64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, long value);

		internal abstract void Bind_Null(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index);

		internal abstract int Bind_ParamCount(SQLiteStatement stmt, SQLiteConnectionFlags flags);

		internal abstract int Bind_ParamIndex(SQLiteStatement stmt, SQLiteConnectionFlags flags, string paramName);

		internal abstract string Bind_ParamName(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index);

		internal abstract void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value);

		internal abstract void Bind_UInt32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, uint value);

		internal abstract void Bind_UInt64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, ulong value);

		internal abstract void BindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteFunction function, SQLiteConnectionFlags flags);

		internal abstract void Cancel();

		internal abstract void ChangePassword(byte[] newPasswordBytes);

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteBase).Name);
			}
		}

		internal abstract void ClearPool();

		internal abstract void Close(bool canThrow);

		internal static void CloseBlob(SQLiteConnectionHandle hdl, IntPtr blob)
		{
			if (hdl == null || blob == IntPtr.Zero)
			{
				return;
			}
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_close_interop(blob);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, null);
					}
				}
			}
		}

		internal static void CloseConnection(SQLiteConnectionHandle hdl, IntPtr db)
		{
			if (hdl == null || db == IntPtr.Zero)
			{
				return;
			}
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_close_interop(db);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, SQLiteBase.GetLastError(hdl, db));
					}
				}
			}
		}

		internal static void CloseConnectionV2(SQLiteConnectionHandle hdl, IntPtr db)
		{
			if (hdl == null || db == IntPtr.Zero)
			{
				return;
			}
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_close_interop(db);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, SQLiteBase.GetLastError(hdl, db));
					}
				}
			}
		}

		internal abstract TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index);

		internal abstract int ColumnCount(SQLiteStatement stmt);

		internal abstract string ColumnDatabaseName(SQLiteStatement stmt, int index);

		internal abstract int ColumnIndex(SQLiteStatement stmt, string columnName);

		internal abstract void ColumnMetaData(string dataBase, string table, string column, ref string dataType, ref string collateSequence, ref bool notNull, ref bool primaryKey, ref bool autoIncrement);

		internal abstract string ColumnName(SQLiteStatement stmt, int index);

		internal abstract string ColumnOriginalName(SQLiteStatement stmt, int index);

		internal abstract string ColumnTableName(SQLiteStatement stmt, int index);

		internal abstract string ColumnType(SQLiteStatement stmt, int index, ref TypeAffinity nAffinity);

		internal abstract int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2);

		internal abstract int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2);

		internal abstract int CountPool();

		internal abstract SQLiteErrorCode CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16, bool @throw);

		internal abstract SQLiteErrorCode CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal, bool @throw);

		internal abstract void CreateModule(SQLiteModule module, SQLiteConnectionFlags flags);

		internal abstract SQLiteErrorCode DeclareVirtualFunction(SQLiteModule module, int argumentCount, string name, ref string error);

		internal abstract SQLiteErrorCode DeclareVirtualTable(SQLiteModule module, string strSql, ref string error);

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				this.disposed = true;
			}
		}

		internal abstract void DisposeModule(SQLiteModule module, SQLiteConnectionFlags flags);

		internal abstract SQLiteErrorCode ExtendedResultCode();

		protected static string FallbackGetErrorString(SQLiteErrorCode rc)
		{
			if (SQLiteBase._errorMessages == null)
			{
				return null;
			}
			int num = (int)rc;
			if (num < 0 || num >= (int)SQLiteBase._errorMessages.Length)
			{
				num = 1;
			}
			return SQLiteBase._errorMessages[num];
		}

		internal abstract SQLiteErrorCode FileControl(string zDbName, int op, IntPtr pArg);

		protected void Finalize()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//base.Finalize();
			}
		}

		internal static void FinalizeStatement(SQLiteConnectionHandle hdl, IntPtr stmt)
		{
			if (hdl == null || stmt == IntPtr.Zero)
			{
				return;
			}
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_finalize_interop(stmt);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, null);
					}
				}
			}
		}

		internal abstract void FinishBackup(SQLiteBackup backup);

		internal static void FinishBackup(SQLiteConnectionHandle hdl, IntPtr backup)
		{
			if (hdl == null || backup == IntPtr.Zero)
			{
				return;
			}
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_backup_finish_interop(backup);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, null);
					}
				}
			}
		}

		internal abstract bool GetBoolean(SQLiteStatement stmt, int index);

		internal abstract byte GetByte(SQLiteStatement stmt, int index);

		internal abstract long GetBytes(SQLiteStatement stmt, int index, int nDataoffset, byte[] bDest, int nStart, int nLength);

		internal abstract char GetChar(SQLiteStatement stmt, int index);

		internal abstract long GetChars(SQLiteStatement stmt, int index, int nDataoffset, char[] bDest, int nStart, int nLength);

		internal abstract CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context);

		internal abstract int GetCursorForTable(SQLiteStatement stmt, int database, int rootPage);

		internal abstract DateTime GetDateTime(SQLiteStatement stmt, int index);

		internal abstract double GetDouble(SQLiteStatement stmt, int index);

		internal abstract string GetFileName(string dbName);

		internal abstract void GetIndexColumnExtendedInfo(string database, string index, string column, ref int sortMode, ref int onError, ref string collationSequence);

		internal abstract short GetInt16(SQLiteStatement stmt, int index);

		internal abstract int GetInt32(SQLiteStatement stmt, int index);

		internal abstract long GetInt64(SQLiteStatement stmt, int index);

		internal abstract string GetLastError();

		internal abstract string GetLastError(string defValue);

		internal static string GetLastError(SQLiteConnectionHandle hdl, IntPtr db)
		{
			if (hdl == null || db == IntPtr.Zero)
			{
				return "null connection or database handle";
			}
			string str = null;
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					if (hdl.IsInvalid || hdl.IsClosed)
					{
						str = "closed or invalid connection handle";
					}
					else
					{
						int num = 0;
						str = SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_errmsg_interop(db, ref num), num);
					}
				}
			}
			GC.KeepAlive(hdl);
			return str;
		}

		internal abstract object GetObject(SQLiteStatement stmt, int index);

		internal abstract long GetParamValueBytes(IntPtr ptr, int nDataOffset, byte[] bDest, int nStart, int nLength);

		internal abstract double GetParamValueDouble(IntPtr ptr);

		internal abstract int GetParamValueInt32(IntPtr ptr);

		internal abstract long GetParamValueInt64(IntPtr ptr);

		internal abstract string GetParamValueText(IntPtr ptr);

		internal abstract TypeAffinity GetParamValueType(IntPtr ptr);

		internal abstract long GetRowIdForCursor(SQLiteStatement stmt, int cursor);

		internal abstract sbyte GetSByte(SQLiteStatement stmt, int index);

		internal abstract string GetText(SQLiteStatement stmt, int index);

		internal abstract ushort GetUInt16(SQLiteStatement stmt, int index);

		internal abstract uint GetUInt32(SQLiteStatement stmt, int index);

		internal abstract ulong GetUInt64(SQLiteStatement stmt, int index);

		internal abstract object GetValue(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, SQLiteType typ);

		internal abstract SQLiteBackup InitializeBackup(SQLiteConnection destCnn, string destName, string sourceName);

		internal static bool IsAutocommit(SQLiteConnectionHandle hdl, IntPtr db)
		{
			if (hdl == null || db == IntPtr.Zero)
			{
				return false;
			}
			bool flag = false;
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					if (!hdl.IsInvalid && !hdl.IsClosed)
					{
						flag = UnsafeNativeMethods.sqlite3_get_autocommit(db) == 1;
					}
				}
			}
			GC.KeepAlive(hdl);
			return flag;
		}

		internal abstract bool IsInitialized();

		internal abstract bool IsNull(SQLiteStatement stmt, int index);

		internal abstract bool IsOpen();

		internal abstract bool IsReadOnly(string name);

		internal abstract bool IsReadOnly(SQLiteStatement stmt);

		internal abstract void LoadExtension(string fileName, string procName);

		internal abstract void LogMessage(SQLiteErrorCode iErrCode, string zMessage);

		internal abstract void Open(string strFilename, string vfsName, SQLiteConnectionFlags connectionFlags, SQLiteOpenFlagsEnum openFlags, int maxPoolSize, bool usePool);

		internal abstract int PageCountBackup(SQLiteBackup backup);

		internal abstract SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, ref string strRemain);

		internal abstract SQLiteErrorCode ReleaseMemory();

		internal abstract int RemainingBackup(SQLiteBackup backup);

		internal abstract SQLiteErrorCode Reset(SQLiteStatement stmt);

		internal static bool ResetConnection(SQLiteConnectionHandle hdl, IntPtr db, bool canThrow)
		{
			SQLiteErrorCode sQLiteErrorCode;
			if (hdl == null || db == IntPtr.Zero)
			{
				return false;
			}
			bool flag = false;
			try
			{
			}
			finally
			{
				lock (hdl)
				{
					if (canThrow && hdl.IsInvalid)
					{
						throw new InvalidOperationException("The connection handle is invalid.");
					}
					if (canThrow && hdl.IsClosed)
					{
						throw new InvalidOperationException("The connection handle is closed.");
					}
					if (!hdl.IsInvalid && !hdl.IsClosed)
					{
						IntPtr zero = IntPtr.Zero;
						do
						{
							zero = UnsafeNativeMethods.sqlite3_next_stmt(db, zero);
							if (zero == IntPtr.Zero)
							{
								continue;
							}
							sQLiteErrorCode = UnsafeNativeMethods.sqlite3_reset_interop(zero);
						}
						while (zero != IntPtr.Zero);
						if (!SQLiteBase.IsAutocommit(hdl, db))
						{
							sQLiteErrorCode = UnsafeNativeMethods.sqlite3_exec(db, SQLiteConvert.ToUTF8("ROLLBACK"), IntPtr.Zero, IntPtr.Zero, ref zero);
							if (sQLiteErrorCode == SQLiteErrorCode.Ok)
							{
								flag = true;
							}
							else if (canThrow)
							{
								throw new SQLiteException(sQLiteErrorCode, SQLiteBase.GetLastError(hdl, db));
							}
						}
						else
						{
							flag = true;
						}
					}
				}
			}
			GC.KeepAlive(hdl);
			return flag;
		}

		internal abstract SQLiteErrorCode ResultCode();

		internal abstract void ReturnBlob(IntPtr context, byte[] value);

		internal abstract void ReturnDouble(IntPtr context, double value);

		internal abstract void ReturnError(IntPtr context, string value);

		internal abstract void ReturnInt32(IntPtr context, int value);

		internal abstract void ReturnInt64(IntPtr context, long value);

		internal abstract void ReturnNull(IntPtr context);

		internal abstract void ReturnText(IntPtr context, string value);

		internal abstract void SetAuthorizerHook(SQLiteAuthorizerCallback func);

		internal abstract void SetCommitHook(SQLiteCommitCallback func);

		internal abstract SQLiteErrorCode SetConfigurationOption(SQLiteConfigDbOpsEnum option, bool bOnOff);

		internal abstract void SetExtendedResultCodes(bool bOnOff);

		internal abstract void SetLoadExtension(bool bOnOff);

		internal abstract SQLiteErrorCode SetLogCallback(SQLiteLogCallback func);

		internal abstract SQLiteErrorCode SetMemoryStatus(bool value);

		internal abstract void SetPassword(byte[] passwordBytes);

		internal abstract void SetProgressHook(int nOps, SQLiteProgressCallback func);

		internal abstract void SetRollbackHook(SQLiteRollbackCallback func);

		internal abstract void SetTimeout(int nTimeoutMS);

		internal abstract void SetTraceCallback(SQLiteTraceCallback func);

		internal abstract void SetUpdateHook(SQLiteUpdateCallback func);

		internal abstract SQLiteErrorCode Shutdown();

		internal abstract bool Step(SQLiteStatement stmt);

		internal abstract bool StepBackup(SQLiteBackup backup, int nPage, ref bool retry);

		internal abstract bool UnbindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteConnectionFlags flags);
	}
}