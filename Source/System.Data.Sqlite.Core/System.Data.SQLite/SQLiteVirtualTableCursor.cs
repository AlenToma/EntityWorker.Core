using System;
using System.Reflection;

namespace System.Data.SQLite
{
	public class SQLiteVirtualTableCursor : ISQLiteNativeHandle, IDisposable
	{
		protected readonly static int InvalidRowIndex;

		private int rowIndex;

		private SQLiteVirtualTable table;

		private int indexNumber;

		private string indexString;

		private SQLiteValue[] values;

		private IntPtr nativeHandle;

		private bool disposed;

		public virtual int IndexNumber
		{
			get
			{
				this.CheckDisposed();
				return this.indexNumber;
			}
		}

		public virtual string IndexString
		{
			get
			{
				this.CheckDisposed();
				return this.indexString;
			}
		}

		public virtual IntPtr NativeHandle
		{
			get
			{
				this.CheckDisposed();
				return this.nativeHandle;
			}
			internal set
			{
				this.nativeHandle = value;
			}
		}

		public virtual SQLiteVirtualTable Table
		{
			get
			{
				this.CheckDisposed();
				return this.table;
			}
		}

		public virtual SQLiteValue[] Values
		{
			get
			{
				this.CheckDisposed();
				return this.values;
			}
		}

		static SQLiteVirtualTableCursor()
		{
		}

		public SQLiteVirtualTableCursor(SQLiteVirtualTable table) : this()
		{
			this.table = table;
		}

		private SQLiteVirtualTableCursor()
		{
			this.rowIndex = SQLiteVirtualTableCursor.InvalidRowIndex;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteVirtualTableCursor).Name);
			}
		}

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

		public virtual void Filter(int indexNumber, string indexString, SQLiteValue[] values)
		{
			this.CheckDisposed();
			if (values != null && this.TryPersistValues(values) != (int)values.Length)
			{
				throw new SQLiteException("failed to persist one or more values");
			}
			this.indexNumber = indexNumber;
			this.indexString = indexString;
			this.values = values;
		}

		~SQLiteVirtualTableCursor()
		{
			this.Dispose(false);
		}

		public virtual int GetRowIndex()
		{
			return this.rowIndex;
		}

		public virtual void NextRowIndex()
		{
			this.rowIndex++;
		}

		protected virtual int TryPersistValues(SQLiteValue[] values)
		{
			int num = 0;
			if (values != null)
			{
				SQLiteValue[] sQLiteValueArray = values;
				for (int i = 0; i < (int)sQLiteValueArray.Length; i++)
				{
					SQLiteValue sQLiteValue = sQLiteValueArray[i];
					if (sQLiteValue != null && sQLiteValue.Persist())
					{
						num++;
					}
				}
			}
			return num;
		}
	}
}