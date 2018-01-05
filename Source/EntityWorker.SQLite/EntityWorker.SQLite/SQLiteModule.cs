using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EntityWorker.SQLite
{
	public abstract class SQLiteModule : ISQLiteManagedModule, IDisposable
	{
		private readonly static int DefaultModuleVersion;

		private UnsafeNativeMethods.sqlite3_module nativeModule;

		private UnsafeNativeMethods.xDestroyModule destroyModule;

		private IntPtr disposableModule;

		private Dictionary<IntPtr, SQLiteVirtualTable> tables;

		private Dictionary<IntPtr, SQLiteVirtualTableCursor> cursors;

		private Dictionary<string, SQLiteFunction> functions;

		private bool logErrors;

		private bool logExceptions;

		private bool declared;

		private string name;

		private bool disposed;

		public virtual bool Declared
		{
			get
			{
				this.CheckDisposed();
				return this.declared;
			}
			internal set
			{
				this.declared = value;
			}
		}

		public virtual bool LogErrors
		{
			get
			{
				this.CheckDisposed();
				return this.LogErrorsNoThrow;
			}
			set
			{
				this.CheckDisposed();
				this.LogErrorsNoThrow = value;
			}
		}

		protected virtual bool LogErrorsNoThrow
		{
			get
			{
				return this.logErrors;
			}
			set
			{
				this.logErrors = value;
			}
		}

		public virtual bool LogExceptions
		{
			get
			{
				this.CheckDisposed();
				return this.LogExceptionsNoThrow;
			}
			set
			{
				this.CheckDisposed();
				this.LogExceptionsNoThrow = value;
			}
		}

		protected virtual bool LogExceptionsNoThrow
		{
			get
			{
				return this.logExceptions;
			}
			set
			{
				this.logExceptions = value;
			}
		}

		public virtual string Name
		{
			get
			{
				this.CheckDisposed();
				return this.name;
			}
		}

		static SQLiteModule()
		{
			SQLiteModule.DefaultModuleVersion = 2;
		}

		public SQLiteModule(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.name = name;
			this.tables = new Dictionary<IntPtr, SQLiteVirtualTable>();
			this.cursors = new Dictionary<IntPtr, SQLiteVirtualTableCursor>();
			this.functions = new Dictionary<string, SQLiteFunction>();
		}

		protected virtual IntPtr AllocateCursor()
		{
			return SQLiteMemory.Allocate(Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_vtab_cursor)));
		}

		private UnsafeNativeMethods.sqlite3_module AllocateNativeModule()
		{
			return this.AllocateNativeModule(this.GetNativeModuleImpl());
		}

		private UnsafeNativeMethods.sqlite3_module AllocateNativeModule(ISQLiteNativeModule module)
		{
			this.nativeModule = new UnsafeNativeMethods.sqlite3_module()
			{
				iVersion = SQLiteModule.DefaultModuleVersion
			};
			if (module == null)
			{
				this.nativeModule.xCreate = new UnsafeNativeMethods.xCreate(this.xCreate);
				this.nativeModule.xConnect = new UnsafeNativeMethods.xConnect(this.xConnect);
				this.nativeModule.xBestIndex = new UnsafeNativeMethods.xBestIndex(this.xBestIndex);
				this.nativeModule.xDisconnect = new UnsafeNativeMethods.xDisconnect(this.xDisconnect);
				this.nativeModule.xDestroy = new UnsafeNativeMethods.xDestroy(this.xDestroy);
				this.nativeModule.xOpen = new UnsafeNativeMethods.xOpen(this.xOpen);
				this.nativeModule.xClose = new UnsafeNativeMethods.xClose(this.xClose);
				this.nativeModule.xFilter = new UnsafeNativeMethods.xFilter(this.xFilter);
				this.nativeModule.xNext = new UnsafeNativeMethods.xNext(this.xNext);
				this.nativeModule.xEof = new UnsafeNativeMethods.xEof(this.xEof);
				this.nativeModule.xColumn = new UnsafeNativeMethods.xColumn(this.xColumn);
				this.nativeModule.xRowId = new UnsafeNativeMethods.xRowId(this.xRowId);
				this.nativeModule.xUpdate = new UnsafeNativeMethods.xUpdate(this.xUpdate);
				this.nativeModule.xBegin = new UnsafeNativeMethods.xBegin(this.xBegin);
				this.nativeModule.xSync = new UnsafeNativeMethods.xSync(this.xSync);
				this.nativeModule.xCommit = new UnsafeNativeMethods.xCommit(this.xCommit);
				this.nativeModule.xRollback = new UnsafeNativeMethods.xRollback(this.xRollback);
				this.nativeModule.xFindFunction = new UnsafeNativeMethods.xFindFunction(this.xFindFunction);
				this.nativeModule.xRename = new UnsafeNativeMethods.xRename(this.xRename);
				this.nativeModule.xSavepoint = new UnsafeNativeMethods.xSavepoint(this.xSavepoint);
				this.nativeModule.xRelease = new UnsafeNativeMethods.xRelease(this.xRelease);
				this.nativeModule.xRollbackTo = new UnsafeNativeMethods.xRollbackTo(this.xRollbackTo);
			}
			else
			{
				ISQLiteNativeModule sQLiteNativeModule = module;
				this.nativeModule.xCreate = new UnsafeNativeMethods.xCreate(sQLiteNativeModule.xCreate);
				ISQLiteNativeModule sQLiteNativeModule1 = module;
				this.nativeModule.xConnect = new UnsafeNativeMethods.xConnect(sQLiteNativeModule1.xConnect);
				ISQLiteNativeModule sQLiteNativeModule2 = module;
				this.nativeModule.xBestIndex = new UnsafeNativeMethods.xBestIndex(sQLiteNativeModule2.xBestIndex);
				ISQLiteNativeModule sQLiteNativeModule3 = module;
				this.nativeModule.xDisconnect = new UnsafeNativeMethods.xDisconnect(sQLiteNativeModule3.xDisconnect);
				ISQLiteNativeModule sQLiteNativeModule4 = module;
				this.nativeModule.xDestroy = new UnsafeNativeMethods.xDestroy(sQLiteNativeModule4.xDestroy);
				ISQLiteNativeModule sQLiteNativeModule5 = module;
				this.nativeModule.xOpen = new UnsafeNativeMethods.xOpen(sQLiteNativeModule5.xOpen);
				ISQLiteNativeModule sQLiteNativeModule6 = module;
				this.nativeModule.xClose = new UnsafeNativeMethods.xClose(sQLiteNativeModule6.xClose);
				ISQLiteNativeModule sQLiteNativeModule7 = module;
				this.nativeModule.xFilter = new UnsafeNativeMethods.xFilter(sQLiteNativeModule7.xFilter);
				ISQLiteNativeModule sQLiteNativeModule8 = module;
				this.nativeModule.xNext = new UnsafeNativeMethods.xNext(sQLiteNativeModule8.xNext);
				ISQLiteNativeModule sQLiteNativeModule9 = module;
				this.nativeModule.xEof = new UnsafeNativeMethods.xEof(sQLiteNativeModule9.xEof);
				ISQLiteNativeModule sQLiteNativeModule10 = module;
				this.nativeModule.xColumn = new UnsafeNativeMethods.xColumn(sQLiteNativeModule10.xColumn);
				ISQLiteNativeModule sQLiteNativeModule11 = module;
				this.nativeModule.xRowId = new UnsafeNativeMethods.xRowId(sQLiteNativeModule11.xRowId);
				ISQLiteNativeModule sQLiteNativeModule12 = module;
				this.nativeModule.xUpdate = new UnsafeNativeMethods.xUpdate(sQLiteNativeModule12.xUpdate);
				ISQLiteNativeModule sQLiteNativeModule13 = module;
				this.nativeModule.xBegin = new UnsafeNativeMethods.xBegin(sQLiteNativeModule13.xBegin);
				ISQLiteNativeModule sQLiteNativeModule14 = module;
				this.nativeModule.xSync = new UnsafeNativeMethods.xSync(sQLiteNativeModule14.xSync);
				ISQLiteNativeModule sQLiteNativeModule15 = module;
				this.nativeModule.xCommit = new UnsafeNativeMethods.xCommit(sQLiteNativeModule15.xCommit);
				ISQLiteNativeModule sQLiteNativeModule16 = module;
				this.nativeModule.xRollback = new UnsafeNativeMethods.xRollback(sQLiteNativeModule16.xRollback);
				ISQLiteNativeModule sQLiteNativeModule17 = module;
				this.nativeModule.xFindFunction = new UnsafeNativeMethods.xFindFunction(sQLiteNativeModule17.xFindFunction);
				ISQLiteNativeModule sQLiteNativeModule18 = module;
				this.nativeModule.xRename = new UnsafeNativeMethods.xRename(sQLiteNativeModule18.xRename);
				ISQLiteNativeModule sQLiteNativeModule19 = module;
				this.nativeModule.xSavepoint = new UnsafeNativeMethods.xSavepoint(sQLiteNativeModule19.xSavepoint);
				ISQLiteNativeModule sQLiteNativeModule20 = module;
				this.nativeModule.xRelease = new UnsafeNativeMethods.xRelease(sQLiteNativeModule20.xRelease);
				ISQLiteNativeModule sQLiteNativeModule21 = module;
				this.nativeModule.xRollbackTo = new UnsafeNativeMethods.xRollbackTo(sQLiteNativeModule21.xRollbackTo);
			}
			return this.nativeModule;
		}

		protected virtual IntPtr AllocateTable()
		{
			return SQLiteMemory.Allocate(Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_vtab)));
		}

		public abstract SQLiteErrorCode Begin(SQLiteVirtualTable table);

		public abstract SQLiteErrorCode BestIndex(SQLiteVirtualTable table, SQLiteIndex index);

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteModule).Name);
			}
		}

		public abstract SQLiteErrorCode Close(SQLiteVirtualTableCursor cursor);

		public abstract SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index);

		public abstract SQLiteErrorCode Commit(SQLiteVirtualTable table);

		public abstract SQLiteErrorCode Connect(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error);

		private UnsafeNativeMethods.sqlite3_module CopyNativeModule(UnsafeNativeMethods.sqlite3_module module)
		{
			UnsafeNativeMethods.sqlite3_module sqlite3Module = new UnsafeNativeMethods.sqlite3_module()
			{
				iVersion = module.iVersion,
				xCreate = new UnsafeNativeMethods.xCreate((module.xCreate != null ? module.xCreate : new UnsafeNativeMethods.xCreate(this.xCreate)).Invoke),
				xConnect = new UnsafeNativeMethods.xConnect((module.xConnect != null ? module.xConnect : new UnsafeNativeMethods.xConnect(this.xConnect)).Invoke),
				xBestIndex = new UnsafeNativeMethods.xBestIndex((module.xBestIndex != null ? module.xBestIndex : new UnsafeNativeMethods.xBestIndex(this.xBestIndex)).Invoke),
				xDisconnect = new UnsafeNativeMethods.xDisconnect((module.xDisconnect != null ? module.xDisconnect : new UnsafeNativeMethods.xDisconnect(this.xDisconnect)).Invoke),
				xDestroy = new UnsafeNativeMethods.xDestroy((module.xDestroy != null ? module.xDestroy : new UnsafeNativeMethods.xDestroy(this.xDestroy)).Invoke),
				xOpen = new UnsafeNativeMethods.xOpen((module.xOpen != null ? module.xOpen : new UnsafeNativeMethods.xOpen(this.xOpen)).Invoke),
				xClose = new UnsafeNativeMethods.xClose((module.xClose != null ? module.xClose : new UnsafeNativeMethods.xClose(this.xClose)).Invoke),
				xFilter = new UnsafeNativeMethods.xFilter((module.xFilter != null ? module.xFilter : new UnsafeNativeMethods.xFilter(this.xFilter)).Invoke),
				xNext = new UnsafeNativeMethods.xNext((module.xNext != null ? module.xNext : new UnsafeNativeMethods.xNext(this.xNext)).Invoke),
				xEof = new UnsafeNativeMethods.xEof((module.xEof != null ? module.xEof : new UnsafeNativeMethods.xEof(this.xEof)).Invoke),
				xColumn = new UnsafeNativeMethods.xColumn((module.xColumn != null ? module.xColumn : new UnsafeNativeMethods.xColumn(this.xColumn)).Invoke),
				xRowId = new UnsafeNativeMethods.xRowId((module.xRowId != null ? module.xRowId : new UnsafeNativeMethods.xRowId(this.xRowId)).Invoke),
				xUpdate = new UnsafeNativeMethods.xUpdate((module.xUpdate != null ? module.xUpdate : new UnsafeNativeMethods.xUpdate(this.xUpdate)).Invoke),
				xBegin = new UnsafeNativeMethods.xBegin((module.xBegin != null ? module.xBegin : new UnsafeNativeMethods.xBegin(this.xBegin)).Invoke),
				xSync = new UnsafeNativeMethods.xSync((module.xSync != null ? module.xSync : new UnsafeNativeMethods.xSync(this.xSync)).Invoke),
				xCommit = new UnsafeNativeMethods.xCommit((module.xCommit != null ? module.xCommit : new UnsafeNativeMethods.xCommit(this.xCommit)).Invoke),
				xRollback = new UnsafeNativeMethods.xRollback((module.xRollback != null ? module.xRollback : new UnsafeNativeMethods.xRollback(this.xRollback)).Invoke),
				xFindFunction = new UnsafeNativeMethods.xFindFunction((module.xFindFunction != null ? module.xFindFunction : new UnsafeNativeMethods.xFindFunction(this.xFindFunction)).Invoke),
				xRename = new UnsafeNativeMethods.xRename((module.xRename != null ? module.xRename : new UnsafeNativeMethods.xRename(this.xRename)).Invoke),
				xSavepoint = new UnsafeNativeMethods.xSavepoint((module.xSavepoint != null ? module.xSavepoint : new UnsafeNativeMethods.xSavepoint(this.xSavepoint)).Invoke),
				xRelease = new UnsafeNativeMethods.xRelease((module.xRelease != null ? module.xRelease : new UnsafeNativeMethods.xRelease(this.xRelease)).Invoke),
				xRollbackTo = new UnsafeNativeMethods.xRollbackTo((module.xRollbackTo != null ? module.xRollbackTo : new UnsafeNativeMethods.xRollbackTo(this.xRollbackTo)).Invoke)
			};
			return sqlite3Module;
		}

		public abstract SQLiteErrorCode Create(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error);

		internal bool CreateDisposableModule(IntPtr pDb)
		{
			bool zero;
			if (this.disposableModule != IntPtr.Zero)
			{
				return true;
			}
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = SQLiteString.Utf8IntPtrFromString(this.name);
				UnsafeNativeMethods.sqlite3_module sqlite3Module = this.AllocateNativeModule();
				this.destroyModule = new UnsafeNativeMethods.xDestroyModule(this.xDestroyModule);
				this.disposableModule = UnsafeNativeMethods.sqlite3_create_disposable_module(pDb, intPtr, ref sqlite3Module, IntPtr.Zero, this.destroyModule);
				zero = this.disposableModule != IntPtr.Zero;
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					SQLiteMemory.Free(intPtr);
					intPtr = IntPtr.Zero;
				}
			}
			return zero;
		}

		protected virtual ISQLiteNativeModule CreateNativeModuleImpl()
		{
			return new SQLiteModule.SQLiteNativeModule(this);
		}

		private SQLiteErrorCode CreateOrConnect(bool create, IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError)
		{
			try
			{
				string str = SQLiteString.StringFromUtf8IntPtr(UnsafeNativeMethods.sqlite3_db_filename(pDb, IntPtr.Zero));
				using (SQLiteConnection sQLiteConnection = new SQLiteConnection(pDb, str, false))
				{
					SQLiteVirtualTable sQLiteVirtualTable = null;
					string str1 = null;
					if ((!create || this.Create(sQLiteConnection, pAux, SQLiteString.StringArrayFromUtf8SizeAndIntPtr(argc, argv), ref sQLiteVirtualTable, ref str1) != SQLiteErrorCode.Ok) && (create || this.Connect(sQLiteConnection, pAux, SQLiteString.StringArrayFromUtf8SizeAndIntPtr(argc, argv), ref sQLiteVirtualTable, ref str1) != SQLiteErrorCode.Ok))
					{
						pError = SQLiteString.Utf8IntPtrFromString(str1);
					}
					else if (sQLiteVirtualTable == null)
					{
						pError = SQLiteString.Utf8IntPtrFromString("no table was created");
					}
					else
					{
						pVtab = this.TableToIntPtr(sQLiteVirtualTable);
						return SQLiteErrorCode.Ok;
					}
				}
			}
			catch (Exception exception)
			{
				pError = SQLiteString.Utf8IntPtrFromString(exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		protected virtual SQLiteVirtualTableCursor CursorFromIntPtr(IntPtr pVtab, IntPtr pCursor)
		{
			SQLiteVirtualTableCursor sQLiteVirtualTableCursor;
			if (pCursor == IntPtr.Zero)
			{
				this.SetTableError(pVtab, "invalid native cursor");
				return null;
			}
			if (this.cursors != null && this.cursors.TryGetValue(pCursor, out sQLiteVirtualTableCursor))
			{
				return sQLiteVirtualTableCursor;
			}
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { pCursor };
			this.SetTableError(pVtab, HelperMethods.StringFormat(currentCulture, "managed cursor for {0} not found", objArray));
			return null;
		}

		protected virtual IntPtr CursorToIntPtr(SQLiteVirtualTableCursor cursor)
		{
			if (cursor == null || this.cursors == null)
			{
				return IntPtr.Zero;
			}
			IntPtr zero = IntPtr.Zero;
			bool flag = false;
			try
			{
				zero = this.AllocateCursor();
				if (zero != IntPtr.Zero)
				{
					cursor.NativeHandle = zero;
					this.cursors.Add(zero, cursor);
					flag = true;
				}
			}
			finally
			{
				if (!flag && zero != IntPtr.Zero)
				{
					this.FreeCursor(zero);
					zero = IntPtr.Zero;
				}
			}
			return zero;
		}

		protected virtual SQLiteErrorCode DeclareFunction(SQLiteConnection connection, int argumentCount, string name, ref string error)
		{
			if (connection == null)
			{
				error = "invalid connection";
				return SQLiteErrorCode.Error;
			}
			SQLiteBase sQLiteBase = connection._sql;
			if (sQLiteBase == null)
			{
				error = "connection has invalid handle";
				return SQLiteErrorCode.Error;
			}
			return sQLiteBase.DeclareVirtualFunction(this, argumentCount, name, ref error);
		}

		protected virtual SQLiteErrorCode DeclareTable(SQLiteConnection connection, string sql, ref string error)
		{
			if (connection == null)
			{
				error = "invalid connection";
				return SQLiteErrorCode.Error;
			}
			SQLiteBase sQLiteBase = connection._sql;
			if (sQLiteBase == null)
			{
				error = "connection has invalid handle";
				return SQLiteErrorCode.Error;
			}
			if (sql == null)
			{
				error = "invalid SQL statement";
				return SQLiteErrorCode.Error;
			}
			return sQLiteBase.DeclareVirtualTable(this, sql, ref error);
		}

		public abstract SQLiteErrorCode Destroy(SQLiteVirtualTable table);

		private SQLiteErrorCode DestroyOrDisconnect(bool destroy, IntPtr pVtab)
		{
			SQLiteErrorCode sQLiteErrorCode;
			try
			{
				try
				{
					SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
					if (sQLiteVirtualTable != null && (destroy && this.Destroy(sQLiteVirtualTable) == SQLiteErrorCode.Ok || !destroy && this.Disconnect(sQLiteVirtualTable) == SQLiteErrorCode.Ok))
					{
						if (this.tables != null)
						{
							this.tables.Remove(pVtab);
						}
						sQLiteErrorCode = SQLiteErrorCode.Ok;
						return sQLiteErrorCode;
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						if (this.LogExceptionsNoThrow)
						{
							CultureInfo currentCulture = CultureInfo.CurrentCulture;
							object[] objArray = new object[] { (destroy ? "xDestroy" : "xDisconnect"), exception };
							SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
						}
					}
					catch
					{
					}
				}
				return SQLiteErrorCode.Error;
			}
			finally
			{
				this.FreeTable(pVtab);
			}
			return sQLiteErrorCode;
		}

		public abstract SQLiteErrorCode Disconnect(SQLiteVirtualTable table);

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing && this.functions != null)
				{
					this.functions.Clear();
				}
				try
				{
					if (this.disposableModule != IntPtr.Zero)
					{
						UnsafeNativeMethods.sqlite3_dispose_module(this.disposableModule);
						this.disposableModule = IntPtr.Zero;
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						if (this.LogExceptionsNoThrow)
						{
							CultureInfo currentCulture = CultureInfo.CurrentCulture;
							object[] objArray = new object[] { "Dispose", exception };
							SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
						}
					}
					catch
					{
					}
				}
				this.disposed = true;
			}
		}

		public abstract bool Eof(SQLiteVirtualTableCursor cursor);

		public abstract SQLiteErrorCode Filter(SQLiteVirtualTableCursor cursor, int indexNumber, string indexString, SQLiteValue[] values);

		~SQLiteModule()
		{
			this.Dispose(false);
		}

		public abstract bool FindFunction(SQLiteVirtualTable table, int argumentCount, string name, ref SQLiteFunction function, ref IntPtr pClientData);

		protected virtual void FreeCursor(IntPtr pCursor)
		{
			SQLiteMemory.Free(pCursor);
		}

		protected virtual void FreeTable(IntPtr pVtab)
		{
			this.SetTableError(pVtab, null);
			SQLiteMemory.Free(pVtab);
		}

		protected virtual string GetFunctionKey(int argumentCount, string name, SQLiteFunction function)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { argumentCount, name };
			return HelperMethods.StringFormat(invariantCulture, "{0}:{1}", objArray);
		}

		protected virtual ISQLiteNativeModule GetNativeModuleImpl()
		{
			return null;
		}

		public abstract SQLiteErrorCode Next(SQLiteVirtualTableCursor cursor);

		public abstract SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor);

		public abstract SQLiteErrorCode Release(SQLiteVirtualTable table, int savepoint);

		public abstract SQLiteErrorCode Rename(SQLiteVirtualTable table, string newName);

		public abstract SQLiteErrorCode Rollback(SQLiteVirtualTable table);

		public abstract SQLiteErrorCode RollbackTo(SQLiteVirtualTable table, int savepoint);

		public abstract SQLiteErrorCode RowId(SQLiteVirtualTableCursor cursor, ref long rowId);

		public abstract SQLiteErrorCode Savepoint(SQLiteVirtualTable table, int savepoint);

		private static bool SetCursorError(SQLiteModule module, IntPtr pCursor, bool logErrors, bool logExceptions, string error)
		{
			if (pCursor == IntPtr.Zero)
			{
				return false;
			}
			IntPtr intPtr = SQLiteModule.TableFromCursor(module, pCursor);
			if (intPtr == IntPtr.Zero)
			{
				return false;
			}
			return SQLiteModule.SetTableError(module, intPtr, logErrors, logExceptions, error);
		}

		private static bool SetCursorError(SQLiteModule module, SQLiteVirtualTableCursor cursor, bool logErrors, bool logExceptions, string error)
		{
			if (cursor == null)
			{
				return false;
			}
			IntPtr nativeHandle = cursor.NativeHandle;
			if (nativeHandle == IntPtr.Zero)
			{
				return false;
			}
			return SQLiteModule.SetCursorError(module, nativeHandle, logErrors, logExceptions, error);
		}

		protected virtual bool SetCursorError(SQLiteVirtualTableCursor cursor, string error)
		{
			return SQLiteModule.SetCursorError(this, cursor, this.LogErrorsNoThrow, this.LogExceptionsNoThrow, error);
		}

		protected virtual bool SetEstimatedCost(SQLiteIndex index, double? estimatedCost)
		{
			if (index == null || index.Outputs == null)
			{
				return false;
			}
			index.Outputs.EstimatedCost = estimatedCost;
			return true;
		}

		protected virtual bool SetEstimatedCost(SQLiteIndex index)
		{
			return this.SetEstimatedCost(index, null);
		}

		protected virtual bool SetEstimatedRows(SQLiteIndex index, long? estimatedRows)
		{
			if (index == null || index.Outputs == null)
			{
				return false;
			}
			index.Outputs.EstimatedRows = estimatedRows;
			return true;
		}

		protected virtual bool SetEstimatedRows(SQLiteIndex index)
		{
			return this.SetEstimatedRows(index, null);
		}

		protected virtual bool SetIndexFlags(SQLiteIndex index, SQLiteIndexFlags? indexFlags)
		{
			if (index == null || index.Outputs == null)
			{
				return false;
			}
			index.Outputs.IndexFlags = indexFlags;
			return true;
		}

		protected virtual bool SetIndexFlags(SQLiteIndex index)
		{
			return this.SetIndexFlags(index, null);
		}

		private static bool SetTableError(SQLiteModule module, IntPtr pVtab, bool logErrors, bool logExceptions, string error)
		{
			bool flag;
			try
			{
				if (logErrors)
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray = new object[] { error };
					SQLiteLog.LogMessage(SQLiteErrorCode.Error, HelperMethods.StringFormat(currentCulture, "Virtual table error: {0}", objArray));
				}
			}
			catch
			{
			}
			bool flag1 = false;
			IntPtr zero = IntPtr.Zero;
			try
			{
				try
				{
					if (pVtab != IntPtr.Zero)
					{
						int num = 0;
						num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
						num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
						IntPtr intPtr = SQLiteMarshal.ReadIntPtr(pVtab, num);
						if (intPtr != IntPtr.Zero)
						{
							SQLiteMemory.Free(intPtr);
							intPtr = IntPtr.Zero;
							SQLiteMarshal.WriteIntPtr(pVtab, num, intPtr);
						}
						if (error != null)
						{
							zero = SQLiteString.Utf8IntPtrFromString(error);
							SQLiteMarshal.WriteIntPtr(pVtab, num, zero);
							flag1 = true;
						}
						else
						{
							flag = true;
							return flag;
						}
					}
					else
					{
						flag = false;
						return flag;
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						if (logExceptions)
						{
							CultureInfo cultureInfo = CultureInfo.CurrentCulture;
							object[] objArray1 = new object[] { "SetTableError", exception };
							SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(cultureInfo, "Caught exception in \"{0}\" method: {1}", objArray1));
						}
					}
					catch
					{
					}
				}
				return flag1;
			}
			finally
			{
				if (!flag1 && zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
			return flag;
		}

		private static bool SetTableError(SQLiteModule module, SQLiteVirtualTable table, bool logErrors, bool logExceptions, string error)
		{
			if (table == null)
			{
				return false;
			}
			IntPtr nativeHandle = table.NativeHandle;
			if (nativeHandle == IntPtr.Zero)
			{
				return false;
			}
			return SQLiteModule.SetTableError(module, nativeHandle, logErrors, logExceptions, error);
		}

		protected virtual bool SetTableError(IntPtr pVtab, string error)
		{
			return SQLiteModule.SetTableError(this, pVtab, this.LogErrorsNoThrow, this.LogExceptionsNoThrow, error);
		}

		protected virtual bool SetTableError(SQLiteVirtualTable table, string error)
		{
			return SQLiteModule.SetTableError(this, table, this.LogErrorsNoThrow, this.LogExceptionsNoThrow, error);
		}

		public abstract SQLiteErrorCode Sync(SQLiteVirtualTable table);

		private static IntPtr TableFromCursor(SQLiteModule module, IntPtr pCursor)
		{
			if (pCursor == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			return Marshal.ReadIntPtr(pCursor);
		}

		protected virtual IntPtr TableFromCursor(IntPtr pCursor)
		{
			return SQLiteModule.TableFromCursor(this, pCursor);
		}

		protected virtual SQLiteVirtualTable TableFromIntPtr(IntPtr pVtab)
		{
			SQLiteVirtualTable sQLiteVirtualTable;
			if (pVtab == IntPtr.Zero)
			{
				this.SetTableError(pVtab, "invalid native table");
				return null;
			}
			if (this.tables != null && this.tables.TryGetValue(pVtab, out sQLiteVirtualTable))
			{
				return sQLiteVirtualTable;
			}
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { pVtab };
			this.SetTableError(pVtab, HelperMethods.StringFormat(currentCulture, "managed table for {0} not found", objArray));
			return null;
		}

		protected virtual IntPtr TableToIntPtr(SQLiteVirtualTable table)
		{
			if (table == null || this.tables == null)
			{
				return IntPtr.Zero;
			}
			IntPtr zero = IntPtr.Zero;
			bool flag = false;
			try
			{
				zero = this.AllocateTable();
				if (zero != IntPtr.Zero)
				{
					this.ZeroTable(zero);
					table.NativeHandle = zero;
					this.tables.Add(zero, table);
					flag = true;
				}
			}
			finally
			{
				if (!flag && zero != IntPtr.Zero)
				{
					this.FreeTable(zero);
					zero = IntPtr.Zero;
				}
			}
			return zero;
		}

		public abstract SQLiteErrorCode Update(SQLiteVirtualTable table, SQLiteValue[] values, ref long rowId);

		private SQLiteErrorCode xBegin(IntPtr pVtab)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Begin(sQLiteVirtualTable);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xBestIndex(IntPtr pVtab, IntPtr pIndex)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					SQLiteIndex sQLiteIndex = null;
					SQLiteIndex.FromIntPtr(pIndex, true, ref sQLiteIndex);
					if (this.BestIndex(sQLiteVirtualTable, sQLiteIndex) == SQLiteErrorCode.Ok)
					{
						SQLiteIndex.ToIntPtr(sQLiteIndex, pIndex, true);
						return SQLiteErrorCode.Ok;
					}
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xClose(IntPtr pCursor)
		{
			SQLiteErrorCode sQLiteErrorCode;
			IntPtr zero = IntPtr.Zero;
			try
			{
				try
				{
					zero = this.TableFromCursor(pCursor);
					SQLiteVirtualTableCursor sQLiteVirtualTableCursor = this.CursorFromIntPtr(zero, pCursor);
					if (sQLiteVirtualTableCursor != null && this.Close(sQLiteVirtualTableCursor) == SQLiteErrorCode.Ok)
					{
						if (this.cursors != null)
						{
							this.cursors.Remove(pCursor);
						}
						sQLiteErrorCode = SQLiteErrorCode.Ok;
						return sQLiteErrorCode;
					}
				}
				catch (Exception exception)
				{
					this.SetTableError(zero, exception.ToString());
				}
				return SQLiteErrorCode.Error;
			}
			finally
			{
				this.FreeCursor(pCursor);
			}
			return sQLiteErrorCode;
		}

		private SQLiteErrorCode xColumn(IntPtr pCursor, IntPtr pContext, int index)
		{
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = this.TableFromCursor(pCursor);
				SQLiteVirtualTableCursor sQLiteVirtualTableCursor = this.CursorFromIntPtr(zero, pCursor);
				if (sQLiteVirtualTableCursor != null)
				{
					return this.Column(sQLiteVirtualTableCursor, new SQLiteContext(pContext), index);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(zero, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xCommit(IntPtr pVtab)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Commit(sQLiteVirtualTable);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xConnect(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError)
		{
			return this.CreateOrConnect(false, pDb, pAux, argc, argv, ref pVtab, ref pError);
		}

		private SQLiteErrorCode xCreate(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError)
		{
			return this.CreateOrConnect(true, pDb, pAux, argc, argv, ref pVtab, ref pError);
		}

		private SQLiteErrorCode xDestroy(IntPtr pVtab)
		{
			return this.DestroyOrDisconnect(true, pVtab);
		}

		private void xDestroyModule(IntPtr pClientData)
		{
			this.disposableModule = IntPtr.Zero;
		}

		private SQLiteErrorCode xDisconnect(IntPtr pVtab)
		{
			return this.DestroyOrDisconnect(false, pVtab);
		}

		private int xEof(IntPtr pCursor)
		{
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = this.TableFromCursor(pCursor);
				SQLiteVirtualTableCursor sQLiteVirtualTableCursor = this.CursorFromIntPtr(zero, pCursor);
				if (sQLiteVirtualTableCursor != null)
				{
					return (this.Eof(sQLiteVirtualTableCursor) ? 1 : 0);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(zero, exception.ToString());
			}
			return 1;
		}

		private SQLiteErrorCode xFilter(IntPtr pCursor, int idxNum, IntPtr idxStr, int argc, IntPtr argv)
		{
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = this.TableFromCursor(pCursor);
				SQLiteVirtualTableCursor sQLiteVirtualTableCursor = this.CursorFromIntPtr(zero, pCursor);
				if (sQLiteVirtualTableCursor != null && this.Filter(sQLiteVirtualTableCursor, idxNum, SQLiteString.StringFromUtf8IntPtr(idxStr), SQLiteValue.ArrayFromSizeAndIntPtr(argc, argv)) == SQLiteErrorCode.Ok)
				{
					return SQLiteErrorCode.Ok;
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(zero, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private int xFindFunction(IntPtr pVtab, int nArg, IntPtr zName, ref SQLiteCallback callback, ref IntPtr pClientData)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					string str = SQLiteString.StringFromUtf8IntPtr(zName);
					SQLiteFunction sQLiteFunction = null;
					if (this.FindFunction(sQLiteVirtualTable, nArg, str, ref sQLiteFunction, ref pClientData))
					{
						if (sQLiteFunction == null)
						{
							this.SetTableError(pVtab, "no function was created");
						}
						else
						{
							string functionKey = this.GetFunctionKey(nArg, str, sQLiteFunction);
							this.functions[functionKey] = sQLiteFunction;
							callback = new SQLiteCallback(sQLiteFunction.ScalarCallback);
							return 1;
						}
					}
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return 0;
		}

		private SQLiteErrorCode xNext(IntPtr pCursor)
		{
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = this.TableFromCursor(pCursor);
				SQLiteVirtualTableCursor sQLiteVirtualTableCursor = this.CursorFromIntPtr(zero, pCursor);
				if (sQLiteVirtualTableCursor != null && this.Next(sQLiteVirtualTableCursor) == SQLiteErrorCode.Ok)
				{
					return SQLiteErrorCode.Ok;
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(zero, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xOpen(IntPtr pVtab, ref IntPtr pCursor)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					SQLiteVirtualTableCursor sQLiteVirtualTableCursor = null;
					if (this.Open(sQLiteVirtualTable, ref sQLiteVirtualTableCursor) == SQLiteErrorCode.Ok)
					{
						if (sQLiteVirtualTableCursor == null)
						{
							this.SetTableError(pVtab, "no managed cursor was created");
						}
						else
						{
							pCursor = this.CursorToIntPtr(sQLiteVirtualTableCursor);
							if (pCursor == IntPtr.Zero)
							{
								this.SetTableError(pVtab, "no native cursor was created");
							}
							else
							{
								return SQLiteErrorCode.Ok;
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xRelease(IntPtr pVtab, int iSavepoint)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Release(sQLiteVirtualTable, iSavepoint);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xRename(IntPtr pVtab, IntPtr zNew)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Rename(sQLiteVirtualTable, SQLiteString.StringFromUtf8IntPtr(zNew));
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xRollback(IntPtr pVtab)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Rollback(sQLiteVirtualTable);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xRollbackTo(IntPtr pVtab, int iSavepoint)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.RollbackTo(sQLiteVirtualTable, iSavepoint);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xRowId(IntPtr pCursor, ref long rowId)
		{
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = this.TableFromCursor(pCursor);
				SQLiteVirtualTableCursor sQLiteVirtualTableCursor = this.CursorFromIntPtr(zero, pCursor);
				if (sQLiteVirtualTableCursor != null)
				{
					return this.RowId(sQLiteVirtualTableCursor, ref rowId);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(zero, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xSavepoint(IntPtr pVtab, int iSavepoint)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Savepoint(sQLiteVirtualTable, iSavepoint);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xSync(IntPtr pVtab)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					return this.Sync(sQLiteVirtualTable);
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		private SQLiteErrorCode xUpdate(IntPtr pVtab, int argc, IntPtr argv, ref long rowId)
		{
			try
			{
				SQLiteVirtualTable sQLiteVirtualTable = this.TableFromIntPtr(pVtab);
				if (sQLiteVirtualTable != null)
				{
					SQLiteErrorCode sQLiteErrorCode = this.Update(sQLiteVirtualTable, SQLiteValue.ArrayFromSizeAndIntPtr(argc, argv), ref rowId);
					return sQLiteErrorCode;
				}
			}
			catch (Exception exception)
			{
				this.SetTableError(pVtab, exception.ToString());
			}
			return SQLiteErrorCode.Error;
		}

		protected virtual void ZeroTable(IntPtr pVtab)
		{
			if (pVtab == IntPtr.Zero)
			{
				return;
			}
			int num = 0;
			SQLiteMarshal.WriteIntPtr(pVtab, num, IntPtr.Zero);
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
			SQLiteMarshal.WriteInt32(pVtab, num, 0);
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			SQLiteMarshal.WriteIntPtr(pVtab, num, IntPtr.Zero);
		}

		private sealed class SQLiteNativeModule : ISQLiteNativeModule, IDisposable
		{
			private const bool DefaultLogErrors = true;

			private const bool DefaultLogExceptions = true;

			private const string ModuleNotAvailableErrorMessage = "native module implementation not available";

			private SQLiteModule module;

			private bool disposed;

			public SQLiteNativeModule(SQLiteModule module)
			{
				this.module = module;
			}

			private void CheckDisposed()
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException(typeof(SQLiteModule.SQLiteNativeModule).Name);
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
					if (this.module != null)
					{
						this.module = null;
					}
					this.disposed = true;
				}
			}

			~SQLiteNativeModule()
			{
				this.Dispose(false);
			}

			private static SQLiteErrorCode ModuleNotAvailableCursorError(IntPtr pCursor)
			{
				SQLiteModule.SetCursorError(null, pCursor, true, true, "native module implementation not available");
				return SQLiteErrorCode.Error;
			}

			private static SQLiteErrorCode ModuleNotAvailableTableError(IntPtr pVtab)
			{
				SQLiteModule.SetTableError(null, pVtab, true, true, "native module implementation not available");
				return SQLiteErrorCode.Error;
			}

			public SQLiteErrorCode xBegin(IntPtr pVtab)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xBegin(pVtab);
			}

			public SQLiteErrorCode xBestIndex(IntPtr pVtab, IntPtr pIndex)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xBestIndex(pVtab, pIndex);
			}

			public SQLiteErrorCode xClose(IntPtr pCursor)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableCursorError(pCursor);
				}
				return this.module.xClose(pCursor);
			}

			public SQLiteErrorCode xColumn(IntPtr pCursor, IntPtr pContext, int index)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableCursorError(pCursor);
				}
				return this.module.xColumn(pCursor, pContext, index);
			}

			public SQLiteErrorCode xCommit(IntPtr pVtab)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xCommit(pVtab);
			}

			public SQLiteErrorCode xConnect(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError)
			{
				if (this.module == null)
				{
					pError = SQLiteString.Utf8IntPtrFromString("native module implementation not available");
					return SQLiteErrorCode.Error;
				}
				return this.module.xConnect(pDb, pAux, argc, argv, ref pVtab, ref pError);
			}

			public SQLiteErrorCode xCreate(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError)
			{
				if (this.module == null)
				{
					pError = SQLiteString.Utf8IntPtrFromString("native module implementation not available");
					return SQLiteErrorCode.Error;
				}
				return this.module.xCreate(pDb, pAux, argc, argv, ref pVtab, ref pError);
			}

			public SQLiteErrorCode xDestroy(IntPtr pVtab)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xDestroy(pVtab);
			}

			public SQLiteErrorCode xDisconnect(IntPtr pVtab)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xDisconnect(pVtab);
			}

			public int xEof(IntPtr pCursor)
			{
				if (this.module == null)
				{
					SQLiteModule.SQLiteNativeModule.ModuleNotAvailableCursorError(pCursor);
					return 1;
				}
				return this.module.xEof(pCursor);
			}

			public SQLiteErrorCode xFilter(IntPtr pCursor, int idxNum, IntPtr idxStr, int argc, IntPtr argv)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableCursorError(pCursor);
				}
				return this.module.xFilter(pCursor, idxNum, idxStr, argc, argv);
			}

			public int xFindFunction(IntPtr pVtab, int nArg, IntPtr zName, ref SQLiteCallback callback, ref IntPtr pClientData)
			{
				if (this.module == null)
				{
					SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
					return 0;
				}
				return this.module.xFindFunction(pVtab, nArg, zName, ref callback, ref pClientData);
			}

			public SQLiteErrorCode xNext(IntPtr pCursor)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableCursorError(pCursor);
				}
				return this.module.xNext(pCursor);
			}

			public SQLiteErrorCode xOpen(IntPtr pVtab, ref IntPtr pCursor)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xOpen(pVtab, ref pCursor);
			}

			public SQLiteErrorCode xRelease(IntPtr pVtab, int iSavepoint)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xRelease(pVtab, iSavepoint);
			}

			public SQLiteErrorCode xRename(IntPtr pVtab, IntPtr zNew)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xRename(pVtab, zNew);
			}

			public SQLiteErrorCode xRollback(IntPtr pVtab)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xRollback(pVtab);
			}

			public SQLiteErrorCode xRollbackTo(IntPtr pVtab, int iSavepoint)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xRollbackTo(pVtab, iSavepoint);
			}

			public SQLiteErrorCode xRowId(IntPtr pCursor, ref long rowId)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableCursorError(pCursor);
				}
				return this.module.xRowId(pCursor, ref rowId);
			}

			public SQLiteErrorCode xSavepoint(IntPtr pVtab, int iSavepoint)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xSavepoint(pVtab, iSavepoint);
			}

			public SQLiteErrorCode xSync(IntPtr pVtab)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xSync(pVtab);
			}

			public SQLiteErrorCode xUpdate(IntPtr pVtab, int argc, IntPtr argv, ref long rowId)
			{
				if (this.module == null)
				{
					return SQLiteModule.SQLiteNativeModule.ModuleNotAvailableTableError(pVtab);
				}
				return this.module.xUpdate(pVtab, argc, argv, ref rowId);
			}
		}
	}
}