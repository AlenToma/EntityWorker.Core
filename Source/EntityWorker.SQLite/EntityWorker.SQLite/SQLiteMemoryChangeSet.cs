using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteMemoryChangeSet : SQLiteChangeSetBase, ISQLiteChangeSet, IEnumerable<ISQLiteChangeSetMetadataItem>, IEnumerable, IDisposable
	{
		private byte[] rawData;

		private bool disposed;

		internal SQLiteMemoryChangeSet(byte[] rawData, SQLiteConnectionHandle handle, SQLiteConnectionFlags flags) : base(handle, flags)
		{
			this.rawData = rawData;
		}

		public void Apply(SessionConflictCallback conflictCallback, object clientData)
		{
			this.CheckDisposed();
			this.Apply(conflictCallback, null, clientData);
		}

		public void Apply(SessionConflictCallback conflictCallback, SessionTableFilterCallback tableFilterCallback, object clientData)
		{
			this.CheckDisposed();
			SQLiteSessionHelpers.CheckRawData(this.rawData);
			if (conflictCallback == null)
			{
				throw new ArgumentNullException("conflictCallback");
			}
			UnsafeNativeMethods.xSessionFilter @delegate = base.GetDelegate(tableFilterCallback, clientData);
			UnsafeNativeMethods.xSessionConflict _xSessionConflict = base.GetDelegate(conflictCallback, clientData);
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = 0;
				zero = SQLiteBytes.ToIntPtr(this.rawData, ref num);
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_apply(base.GetIntPtr(), num, zero, @delegate, _xSessionConflict, IntPtr.Zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_apply");
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

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteMemoryChangeSet).Name);
			}
		}

		public ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet)
		{
			ISQLiteChangeSet sQLiteMemoryChangeSets;
			this.CheckDisposed();
			SQLiteSessionHelpers.CheckRawData(this.rawData);
			SQLiteMemoryChangeSet sQLiteMemoryChangeSets1 = changeSet as SQLiteMemoryChangeSet;
			if (sQLiteMemoryChangeSets1 == null)
			{
				throw new ArgumentException("not a memory based change set", "changeSet");
			}
			SQLiteSessionHelpers.CheckRawData(sQLiteMemoryChangeSets1.rawData);
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			try
			{
				int num = 0;
				zero = SQLiteBytes.ToIntPtr(this.rawData, ref num);
				int num1 = 0;
				intPtr = SQLiteBytes.ToIntPtr(sQLiteMemoryChangeSets1.rawData, ref num1);
				int num2 = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_concat(num, zero, num1, intPtr, ref num2, ref zero1);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_concat");
				}
				byte[] numArray = SQLiteBytes.FromIntPtr(zero1, num2);
				sQLiteMemoryChangeSets = new SQLiteMemoryChangeSet(numArray, base.GetHandle(), base.GetFlags());
			}
			finally
			{
				if (zero1 != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero1);
					zero1 = IntPtr.Zero;
				}
				if (intPtr != IntPtr.Zero)
				{
					SQLiteMemory.Free(intPtr);
					intPtr = IntPtr.Zero;
				}
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
			return sQLiteMemoryChangeSets;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing && this.rawData != null)
				{
					this.rawData = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		public IEnumerator<ISQLiteChangeSetMetadataItem> GetEnumerator()
		{
			return new SQLiteMemoryChangeSetEnumerator(this.rawData);
		}

		public ISQLiteChangeSet Invert()
		{
			ISQLiteChangeSet sQLiteMemoryChangeSets;
			this.CheckDisposed();
			SQLiteSessionHelpers.CheckRawData(this.rawData);
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				int num = 0;
				zero = SQLiteBytes.ToIntPtr(this.rawData, ref num);
				int num1 = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_invert(num, zero, ref num1, ref intPtr);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_invert");
				}
				byte[] numArray = SQLiteBytes.FromIntPtr(intPtr, num1);
				sQLiteMemoryChangeSets = new SQLiteMemoryChangeSet(numArray, base.GetHandle(), base.GetFlags());
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					SQLiteMemory.Free(intPtr);
					intPtr = IntPtr.Zero;
				}
				if (zero != IntPtr.Zero)
				{
					SQLiteMemory.Free(zero);
					zero = IntPtr.Zero;
				}
			}
			return sQLiteMemoryChangeSets;
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}