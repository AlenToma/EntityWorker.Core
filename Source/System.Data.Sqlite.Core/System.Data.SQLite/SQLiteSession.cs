using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace System.Data.SQLite
{
	internal sealed class SQLiteSession : SQLiteConnectionLock, ISQLiteSession, IDisposable
	{
		private SQLiteSessionStreamManager streamManager;

		private string databaseName;

		private IntPtr session;

		private UnsafeNativeMethods.xSessionFilter xFilter;

		private SessionTableFilterCallback tableFilterCallback;

		private object tableFilterClientData;

		private bool disposed;

		public SQLiteSession(SQLiteConnectionHandle handle, SQLiteConnectionFlags flags, string databaseName) : base(handle, flags, true)
		{
			this.databaseName = databaseName;
			this.InitializeHandle();
		}

		private UnsafeNativeMethods.xSessionFilter ApplyTableFilter(SessionTableFilterCallback callback, object clientData)
		{
			this.tableFilterCallback = callback;
			this.tableFilterClientData = clientData;
			if (callback == null)
			{
				if (this.xFilter != null)
				{
					this.xFilter = null;
				}
				return null;
			}
			if (this.xFilter == null)
			{
				this.xFilter = new UnsafeNativeMethods.xSessionFilter(this.Filter);
			}
			return this.xFilter;
		}

		public void AttachTable(string name)
		{
			this.CheckDisposed();
			this.CheckHandle();
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_attach(this.session, SQLiteString.GetUtf8BytesFromString(name));
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_attach");
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteSession).Name);
			}
		}

		private void CheckHandle()
		{
			if (this.session == IntPtr.Zero)
			{
				throw new InvalidOperationException("session is not open");
			}
		}

		public void CreateChangeSet(ref byte[] rawData)
		{
			this.CheckDisposed();
			this.CheckHandle();
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_changeset(this.session, ref num, ref zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3session_changeset");
				}
				rawData = SQLiteBytes.FromIntPtr(zero, num);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
		}

		public void CreateChangeSet(Stream stream)
		{
			this.CheckDisposed();
			this.CheckHandle();
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			SQLiteStreamAdapter streamAdapter = this.GetStreamAdapter(stream);
			if (streamAdapter == null)
			{
				throw new SQLiteException("could not get or create adapter for output stream");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_changeset_strm(this.session, streamAdapter.GetOutputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_changeset_strm");
			}
		}

		public void CreatePatchSet(ref byte[] rawData)
		{
			this.CheckDisposed();
			this.CheckHandle();
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_patchset(this.session, ref num, ref zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3session_patchset");
				}
				rawData = SQLiteBytes.FromIntPtr(zero, num);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
		}

		public void CreatePatchSet(Stream stream)
		{
			this.CheckDisposed();
			this.CheckHandle();
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			SQLiteStreamAdapter streamAdapter = this.GetStreamAdapter(stream);
			if (streamAdapter == null)
			{
				throw new SQLiteException("could not get or create adapter for output stream");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_patchset_strm(this.session, streamAdapter.GetOutputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_patchset_strm");
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed)
				{
					if (disposing)
					{
						if (this.xFilter != null)
						{
							this.xFilter = null;
						}
						if (this.streamManager != null)
						{
							this.streamManager.Dispose();
							this.streamManager = null;
						}
					}
					if (this.session != IntPtr.Zero)
					{
						UnsafeNativeMethods.sqlite3session_delete(this.session);
						this.session = IntPtr.Zero;
					}
					base.Unlock();
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		private int Filter(IntPtr context, IntPtr pTblName)
		{
			int num;
			try
			{
				num = (this.tableFilterCallback(this.tableFilterClientData, SQLiteString.StringFromUtf8IntPtr(pTblName)) ? 1 : 0);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(base.GetFlags()))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "xSessionFilter", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
				return 0;
			}
			return num;
		}

		private SQLiteStreamAdapter GetStreamAdapter(Stream stream)
		{
			this.InitializeStreamManager();
			return this.streamManager.GetAdapter(stream);
		}

		private void InitializeHandle()
		{
			if (this.session != IntPtr.Zero)
			{
				return;
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_create(base.GetIntPtr(), SQLiteString.GetUtf8BytesFromString(this.databaseName), ref this.session);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3session_create");
			}
		}

		private void InitializeStreamManager()
		{
			if (this.streamManager != null)
			{
				return;
			}
			this.streamManager = new SQLiteSessionStreamManager(base.GetFlags());
		}

		public bool IsEmpty()
		{
			this.CheckDisposed();
			this.CheckHandle();
			return UnsafeNativeMethods.sqlite3session_isempty(this.session) != 0;
		}

		public bool IsEnabled()
		{
			this.CheckDisposed();
			this.CheckHandle();
			return UnsafeNativeMethods.sqlite3session_enable(this.session, -1) != 0;
		}

		public bool IsIndirect()
		{
			this.CheckDisposed();
			this.CheckHandle();
			return UnsafeNativeMethods.sqlite3session_indirect(this.session, -1) != 0;
		}

		public void LoadDifferencesFromTable(string fromDatabaseName, string tableName)
		{
			this.CheckDisposed();
			this.CheckHandle();
			if (fromDatabaseName == null)
			{
				throw new ArgumentNullException("fromDatabaseName");
			}
			if (tableName == null)
			{
				throw new ArgumentNullException("tableName");
			}
			IntPtr zero = IntPtr.Zero;
			try
			{
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3session_diff(this.session, SQLiteString.GetUtf8BytesFromString(fromDatabaseName), SQLiteString.GetUtf8BytesFromString(tableName), ref zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					string str = null;
					if (zero != IntPtr.Zero)
					{
						str = SQLiteString.StringFromUtf8IntPtr(zero);
						if (!string.IsNullOrEmpty(str))
						{
							CultureInfo currentCulture = CultureInfo.CurrentCulture;
							object[] objArray = new object[] { str };
							str = HelperMethods.StringFormat(currentCulture, ": {0}", objArray);
						}
					}
					CultureInfo cultureInfo = CultureInfo.CurrentCulture;
					object[] objArray1 = new object[] { "sqlite3session_diff", str };
					throw new SQLiteException(sQLiteErrorCode, HelperMethods.StringFormat(cultureInfo, "{0}{1}", objArray1));
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
		}

		public void SetTableFilter(SessionTableFilterCallback callback, object clientData)
		{
			this.CheckDisposed();
			this.CheckHandle();
			UnsafeNativeMethods.sqlite3session_table_filter(this.session, this.ApplyTableFilter(callback, clientData), IntPtr.Zero);
		}

		public void SetToDirect()
		{
			this.CheckDisposed();
			this.CheckHandle();
			UnsafeNativeMethods.sqlite3session_indirect(this.session, 0);
		}

		public void SetToDisabled()
		{
			this.CheckDisposed();
			this.CheckHandle();
			UnsafeNativeMethods.sqlite3session_enable(this.session, 0);
		}

		public void SetToEnabled()
		{
			this.CheckDisposed();
			this.CheckHandle();
			UnsafeNativeMethods.sqlite3session_enable(this.session, 1);
		}

		public void SetToIndirect()
		{
			this.CheckDisposed();
			this.CheckHandle();
			UnsafeNativeMethods.sqlite3session_indirect(this.session, 1);
		}
	}
}