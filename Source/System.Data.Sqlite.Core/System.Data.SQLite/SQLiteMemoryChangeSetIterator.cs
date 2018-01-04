using System;
using System.Reflection;

namespace System.Data.SQLite
{
	internal sealed class SQLiteMemoryChangeSetIterator : SQLiteChangeSetIterator
	{
		private IntPtr pData;

		private bool disposed;

		private SQLiteMemoryChangeSetIterator(IntPtr pData, IntPtr iterator, bool ownHandle) : base(iterator, ownHandle)
		{
			this.pData = pData;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteMemoryChangeSetIterator).Name);
			}
		}

		public static SQLiteMemoryChangeSetIterator Create(byte[] rawData)
		{
			SQLiteSessionHelpers.CheckRawData(rawData);
			SQLiteMemoryChangeSetIterator sQLiteMemoryChangeSetIterator = null;
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				int num = 0;
				zero = SQLiteBytes.ToIntPtr(rawData, ref num);
				if (zero == IntPtr.Zero)
				{
					throw new SQLiteException(SQLiteErrorCode.NoMem, null);
				}
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_start(ref intPtr, num, zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_start");
				}
				sQLiteMemoryChangeSetIterator = new SQLiteMemoryChangeSetIterator(zero, intPtr, true);
			}
			finally
			{
				if (sQLiteMemoryChangeSetIterator == null)
				{
					if (intPtr != IntPtr.Zero)
					{
						UnsafeNativeMethods.sqlite3changeset_finalize(intPtr);
						intPtr = IntPtr.Zero;
					}
					if (zero != IntPtr.Zero)
					{
						SQLiteMemory.Free(zero);
						zero = IntPtr.Zero;
					}
				}
			}
			return sQLiteMemoryChangeSetIterator;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			try
			{
				if (!this.disposed && this.pData != IntPtr.Zero)
				{
					SQLiteMemory.Free(this.pData);
					this.pData = IntPtr.Zero;
				}
			}
			finally
			{
				this.disposed = true;
			}
		}
	}
}