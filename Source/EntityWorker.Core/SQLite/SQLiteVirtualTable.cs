using System;
using System.Reflection;

namespace EntityWorker.Core.SQLite
{
	public class SQLiteVirtualTable : ISQLiteNativeHandle, IDisposable
	{
		private const int ModuleNameIndex = 0;

		private const int DatabaseNameIndex = 1;

		private const int TableNameIndex = 2;

		private string[] arguments;

		private SQLiteIndex index;

		private IntPtr nativeHandle;

		private bool disposed;

		public virtual string[] Arguments
		{
			get
			{
				this.CheckDisposed();
				return this.arguments;
			}
		}

		public virtual string DatabaseName
		{
			get
			{
				this.CheckDisposed();
				string[] arguments = this.Arguments;
				if (arguments == null || (int)arguments.Length <= 1)
				{
					return null;
				}
				return arguments[1];
			}
		}

		public virtual SQLiteIndex Index
		{
			get
			{
				this.CheckDisposed();
				return this.index;
			}
		}

		public virtual string ModuleName
		{
			get
			{
				this.CheckDisposed();
				string[] arguments = this.Arguments;
				if (arguments == null || (int)arguments.Length <= 0)
				{
					return null;
				}
				return arguments[0];
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

		public virtual string TableName
		{
			get
			{
				this.CheckDisposed();
				string[] arguments = this.Arguments;
				if (arguments == null || (int)arguments.Length <= 2)
				{
					return null;
				}
				return arguments[2];
			}
		}

		public SQLiteVirtualTable(string[] arguments)
		{
			this.arguments = arguments;
		}

		public virtual bool BestIndex(SQLiteIndex index)
		{
			this.CheckDisposed();
			this.index = index;
			return true;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteVirtualTable).Name);
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

		~SQLiteVirtualTable()
		{
			this.Dispose(false);
		}

		public virtual bool Rename(string name)
		{
			this.CheckDisposed();
			if (this.arguments == null || (int)this.arguments.Length <= 2)
			{
				return false;
			}
			this.arguments[2] = name;
			return true;
		}
	}
}