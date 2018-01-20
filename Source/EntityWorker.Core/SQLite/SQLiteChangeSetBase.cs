using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EntityWorker.Core.SQLite
{
	internal class SQLiteChangeSetBase : SQLiteConnectionLock
	{
		private bool disposed;

		internal SQLiteChangeSetBase(SQLiteConnectionHandle handle, SQLiteConnectionFlags flags) : base(handle, flags, true)
		{
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteChangeSetBase).Name);
			}
		}

		private ISQLiteChangeSetMetadataItem CreateMetadataItem(IntPtr iterator)
		{
			return new SQLiteChangeSetMetadataItem(SQLiteChangeSetIterator.Attach(iterator));
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed)
				{
					base.Unlock();
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		protected UnsafeNativeMethods.xSessionFilter GetDelegate(SessionTableFilterCallback tableFilterCallback, object clientData)
		{
			if (tableFilterCallback == null)
			{
				return null;
			}
			return (IntPtr context, IntPtr pTblName) => {
				int num;
				try
				{
					string str = SQLiteString.StringFromUtf8IntPtr(pTblName);
					num = (tableFilterCallback(clientData, str) ? 1 : 0);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						if (HelperMethods.LogCallbackExceptions(base.GetFlags()))
						{
							SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", new object[] { "xSessionFilter", exception }));
						}
					}
					catch
					{
					}
					return 0;
				}
				return num;
			};
		}

		protected UnsafeNativeMethods.xSessionConflict GetDelegate(SessionConflictCallback conflictCallback, object clientData)
		{
			if (conflictCallback == null)
			{
				return null;
			}
			return (IntPtr context, SQLiteChangeSetConflictType type, IntPtr iterator) => {
				SQLiteChangeSetConflictResult sQLiteChangeSetConflictResult;
				try
				{
					ISQLiteChangeSetMetadataItem sQLiteChangeSetMetadataItem = this.CreateMetadataItem(iterator);
					if (sQLiteChangeSetMetadataItem == null)
					{
						throw new SQLiteException("could not create metadata item");
					}
					sQLiteChangeSetConflictResult = conflictCallback(clientData, type, sQLiteChangeSetMetadataItem);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						if (HelperMethods.LogCallbackExceptions(base.GetFlags()))
						{
							SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", new object[] { "xSessionConflict", exception }));
						}
					}
					catch
					{
					}
					return SQLiteChangeSetConflictResult.Abort;
				}
				return sQLiteChangeSetConflictResult;
			};
		}
	}
}