using System;
using System.Reflection;

namespace System.Data.SQLite
{
	internal sealed class SQLiteChangeSetMetadataItem : ISQLiteChangeSetMetadataItem, IDisposable
	{
		private SQLiteChangeSetIterator iterator;

		private string tableName;

		private int? numberOfColumns;

		private SQLiteAuthorizerActionCode? operationCode;

		private bool? indirect;

		private bool[] primaryKeyColumns;

		private int? numberOfForeignKeyConflicts;

		private bool disposed;

		public bool Indirect
		{
			get
			{
				this.CheckDisposed();
				this.PopulateOperationMetadata();
				return this.indirect.Value;
			}
		}

		public int NumberOfColumns
		{
			get
			{
				this.CheckDisposed();
				this.PopulateOperationMetadata();
				return this.numberOfColumns.Value;
			}
		}

		public int NumberOfForeignKeyConflicts
		{
			get
			{
				this.CheckDisposed();
				this.PopulateNumberOfForeignKeyConflicts();
				return this.numberOfForeignKeyConflicts.Value;
			}
		}

		public SQLiteAuthorizerActionCode OperationCode
		{
			get
			{
				this.CheckDisposed();
				this.PopulateOperationMetadata();
				return this.operationCode.Value;
			}
		}

		public bool[] PrimaryKeyColumns
		{
			get
			{
				this.CheckDisposed();
				this.PopulatePrimaryKeyColumns();
				return this.primaryKeyColumns;
			}
		}

		public string TableName
		{
			get
			{
				this.CheckDisposed();
				this.PopulateOperationMetadata();
				return this.tableName;
			}
		}

		public SQLiteChangeSetMetadataItem(SQLiteChangeSetIterator iterator)
		{
			this.iterator = iterator;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteChangeSetMetadataItem).Name);
			}
		}

		private void CheckIterator()
		{
			if (this.iterator == null)
			{
				throw new InvalidOperationException("iterator unavailable");
			}
			this.iterator.CheckHandle();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing && this.iterator != null)
				{
					this.iterator = null;
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		~SQLiteChangeSetMetadataItem()
		{
			this.Dispose(false);
		}

		public SQLiteValue GetConflictValue(int columnIndex)
		{
			this.CheckDisposed();
			this.CheckIterator();
			IntPtr zero = IntPtr.Zero;
			UnsafeNativeMethods.sqlite3changeset_conflict(this.iterator.GetIntPtr(), columnIndex, ref zero);
			return SQLiteValue.FromIntPtr(zero);
		}

		public SQLiteValue GetNewValue(int columnIndex)
		{
			this.CheckDisposed();
			this.CheckIterator();
			IntPtr zero = IntPtr.Zero;
			UnsafeNativeMethods.sqlite3changeset_new(this.iterator.GetIntPtr(), columnIndex, ref zero);
			return SQLiteValue.FromIntPtr(zero);
		}

		public SQLiteValue GetOldValue(int columnIndex)
		{
			this.CheckDisposed();
			this.CheckIterator();
			IntPtr zero = IntPtr.Zero;
			UnsafeNativeMethods.sqlite3changeset_old(this.iterator.GetIntPtr(), columnIndex, ref zero);
			return SQLiteValue.FromIntPtr(zero);
		}

		private void PopulateNumberOfForeignKeyConflicts()
		{
			if (!this.numberOfForeignKeyConflicts.HasValue)
			{
				this.CheckIterator();
				int num = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_fk_conflicts(this.iterator.GetIntPtr(), ref num);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_fk_conflicts");
				}
				this.numberOfForeignKeyConflicts = new int?(num);
			}
		}

		private void PopulateOperationMetadata()
		{
			if (this.tableName == null || !this.numberOfColumns.HasValue || !this.operationCode.HasValue || !this.indirect.HasValue)
			{
				this.CheckIterator();
				IntPtr zero = IntPtr.Zero;
				SQLiteAuthorizerActionCode sQLiteAuthorizerActionCode = SQLiteAuthorizerActionCode.None;
				int num = 0;
				int num1 = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_op(this.iterator.GetIntPtr(), ref zero, ref num1, ref sQLiteAuthorizerActionCode, ref num);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_op");
				}
				this.tableName = SQLiteString.StringFromUtf8IntPtr(zero);
				this.numberOfColumns = new int?(num1);
				this.operationCode = new SQLiteAuthorizerActionCode?(sQLiteAuthorizerActionCode);
				this.indirect = new bool?(num != 0);
			}
		}

		private void PopulatePrimaryKeyColumns()
		{
			if (this.primaryKeyColumns == null)
			{
				this.CheckIterator();
				IntPtr zero = IntPtr.Zero;
				int num = 0;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_pk(this.iterator.GetIntPtr(), ref zero, ref num);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_pk");
				}
				byte[] numArray = SQLiteBytes.FromIntPtr(zero, num);
				if (numArray != null)
				{
					this.primaryKeyColumns = new bool[num];
					for (int i = 0; i < (int)numArray.Length; i++)
					{
						this.primaryKeyColumns[i] = numArray[i] != 0;
					}
				}
			}
		}
	}
}