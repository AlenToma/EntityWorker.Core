using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Data.SQLite
{
	public sealed class SQLiteBlob : IDisposable
	{
		internal SQLiteBase _sql;

		internal SQLiteBlobHandle _sqlite_blob;

		private bool disposed;

		private SQLiteBlob(SQLiteBase sqlbase, SQLiteBlobHandle blob)
		{
			this._sql = sqlbase;
			this._sqlite_blob = blob;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteBlob).Name);
			}
		}

		private void CheckOpen()
		{
			if (this._sqlite_blob == IntPtr.Zero)
			{
				throw new InvalidOperationException("Blob is not open");
			}
		}

		public void Close()
		{
			this.Dispose();
		}

		public static SQLiteBlob Create(SQLiteDataReader dataReader, int i, bool readOnly)
		{
			SQLiteConnection connection = SQLiteDataReader.GetConnection(dataReader);
			if (connection == null)
			{
				throw new InvalidOperationException("Connection not available");
			}
			SQLite3 sQLite3 = connection._sql as SQLite3;
			if (sQLite3 == null)
			{
				throw new InvalidOperationException("Connection has no wrapper");
			}
			SQLiteConnectionHandle sQLiteConnectionHandle = sQLite3._sql;
			if (sQLiteConnectionHandle == null)
			{
				throw new InvalidOperationException("Connection has an invalid handle.");
			}
			long? rowId = dataReader.GetRowId(i);
			if (!rowId.HasValue)
			{
				throw new InvalidOperationException("No RowId is available");
			}
			SQLiteBlobHandle sQLiteBlobHandle = null;
			try
			{
			}
			finally
			{
				IntPtr zero = IntPtr.Zero;
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_open(sQLiteConnectionHandle, SQLiteConvert.ToUTF8(dataReader.GetDatabaseName(i)), SQLiteConvert.ToUTF8(dataReader.GetTableName(i)), SQLiteConvert.ToUTF8(dataReader.GetName(i)), rowId.Value, (readOnly ? 0 : 1), ref zero);
				if (sQLiteErrorCode != SQLiteErrorCode.Ok)
				{
					throw new SQLiteException(sQLiteErrorCode, null);
				}
				sQLiteBlobHandle = new SQLiteBlobHandle(sQLiteConnectionHandle, zero);
			}
			object[] objArray = new object[] { typeof(SQLiteBlob), dataReader, i, readOnly };
			SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, dataReader, sQLiteBlobHandle, null, objArray));
			return new SQLiteBlob(sQLite3, sQLiteBlobHandle);
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
				if (disposing)
				{
					if (this._sqlite_blob != null)
					{
						this._sqlite_blob.Dispose();
						this._sqlite_blob = null;
					}
					this._sql = null;
				}
				this.disposed = true;
			}
		}

		~SQLiteBlob()
		{
			this.Dispose(false);
		}

		public int GetCount()
		{
			this.CheckDisposed();
			this.CheckOpen();
			return UnsafeNativeMethods.sqlite3_blob_bytes(this._sqlite_blob);
		}

		public void Read(byte[] buffer, int count, int offset)
		{
			this.CheckDisposed();
			this.CheckOpen();
			this.VerifyParameters(buffer, count, offset);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_read(this._sqlite_blob, buffer, count, offset);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, null);
			}
		}

		public void Reopen(long rowId)
		{
			this.CheckDisposed();
			this.CheckOpen();
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_reopen(this._sqlite_blob, rowId);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				this.Dispose();
				throw new SQLiteException(sQLiteErrorCode, null);
			}
		}

		private void VerifyParameters(byte[] buffer, int count, int offset)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentException("Negative offset not allowed.");
			}
			if (count < 0)
			{
				throw new ArgumentException("Negative count not allowed.");
			}
			if (count > (int)buffer.Length)
			{
				throw new ArgumentException("Buffer is too small.");
			}
		}

		public void Write(byte[] buffer, int count, int offset)
		{
			this.CheckDisposed();
			this.CheckOpen();
			this.VerifyParameters(buffer, count, offset);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_blob_write(this._sqlite_blob, buffer, count, offset);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, null);
			}
		}
	}
}