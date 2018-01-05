using System;
using System.IO;
using System.Reflection;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteChangeGroup : ISQLiteChangeGroup, IDisposable
	{
		private SQLiteSessionStreamManager streamManager;

		private SQLiteConnectionFlags flags;

		private IntPtr changeGroup;

		private bool disposed;

		public SQLiteChangeGroup(SQLiteConnectionFlags flags)
		{
			this.flags = flags;
			this.InitializeHandle();
		}

		public void AddChangeSet(byte[] rawData)
		{
			this.CheckDisposed();
			this.CheckHandle();
			SQLiteSessionHelpers.CheckRawData(rawData);
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = 0;
				zero = SQLiteBytes.ToIntPtr(rawData, ref num);
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_add(this.changeGroup, num, zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_add");
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

		public void AddChangeSet(Stream stream)
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
				throw new SQLiteException("could not get or create adapter for input stream");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_add_strm(this.changeGroup, streamAdapter.GetInputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_add_strm");
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteChangeGroup).Name);
			}
		}

		private void CheckHandle()
		{
			if (this.changeGroup == IntPtr.Zero)
			{
				throw new InvalidOperationException("change group not open");
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
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_output(this.changeGroup, ref num, ref zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_output");
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
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_output_strm(this.changeGroup, streamAdapter.GetOutputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_output_strm");
			}
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
				if (!this.disposed)
				{
					if (disposing && this.streamManager != null)
					{
						this.streamManager.Dispose();
						this.streamManager = null;
					}
					if (this.changeGroup != IntPtr.Zero)
					{
						UnsafeNativeMethods.sqlite3changegroup_delete(this.changeGroup);
						this.changeGroup = IntPtr.Zero;
					}
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		~SQLiteChangeGroup()
		{
			this.Dispose(false);
		}

		private SQLiteStreamAdapter GetStreamAdapter(Stream stream)
		{
			this.InitializeStreamManager();
			return this.streamManager.GetAdapter(stream);
		}

		private void InitializeHandle()
		{
			if (this.changeGroup != IntPtr.Zero)
			{
				return;
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changegroup_new(ref this.changeGroup);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changegroup_new");
			}
		}

		private void InitializeStreamManager()
		{
			if (this.streamManager != null)
			{
				return;
			}
			this.streamManager = new SQLiteSessionStreamManager(this.flags);
		}
	}
}