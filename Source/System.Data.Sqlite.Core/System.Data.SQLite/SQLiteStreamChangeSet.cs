using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.Data.SQLite
{
	internal sealed class SQLiteStreamChangeSet : SQLiteChangeSetBase, ISQLiteChangeSet, IEnumerable<ISQLiteChangeSetMetadataItem>, IEnumerable, IDisposable
	{
		private SQLiteStreamAdapter inputStreamAdapter;

		private SQLiteStreamAdapter outputStreamAdapter;

		private Stream inputStream;

		private Stream outputStream;

		private bool disposed;

		internal SQLiteStreamChangeSet(Stream inputStream, Stream outputStream, SQLiteConnectionHandle handle, SQLiteConnectionFlags flags) : base(handle, flags)
		{
			this.inputStream = inputStream;
			this.outputStream = outputStream;
		}

		public void Apply(SessionConflictCallback conflictCallback, object clientData)
		{
			this.CheckDisposed();
			this.Apply(conflictCallback, null, clientData);
		}

		public void Apply(SessionConflictCallback conflictCallback, SessionTableFilterCallback tableFilterCallback, object clientData)
		{
			this.CheckDisposed();
			this.CheckInputStream();
			if (conflictCallback == null)
			{
				throw new ArgumentNullException("conflictCallback");
			}
			UnsafeNativeMethods.xSessionFilter @delegate = base.GetDelegate(tableFilterCallback, clientData);
			UnsafeNativeMethods.xSessionConflict _xSessionConflict = base.GetDelegate(conflictCallback, clientData);
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_apply_strm(base.GetIntPtr(), this.inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, @delegate, _xSessionConflict, IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_apply_strm");
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteStreamChangeSet).Name);
			}
		}

		private void CheckInputStream()
		{
			if (this.inputStream == null)
			{
				throw new InvalidOperationException("input stream unavailable");
			}
			if (this.inputStreamAdapter == null)
			{
				this.inputStreamAdapter = new SQLiteStreamAdapter(this.inputStream, base.GetFlags());
			}
		}

		private void CheckOutputStream()
		{
			if (this.outputStream == null)
			{
				throw new InvalidOperationException("output stream unavailable");
			}
			if (this.outputStreamAdapter == null)
			{
				this.outputStreamAdapter = new SQLiteStreamAdapter(this.outputStream, base.GetFlags());
			}
		}

		public ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet)
		{
			this.CheckDisposed();
			this.CheckInputStream();
			this.CheckOutputStream();
			SQLiteStreamChangeSet sQLiteStreamChangeSets = changeSet as SQLiteStreamChangeSet;
			if (sQLiteStreamChangeSets == null)
			{
				throw new ArgumentException("not a stream based change set", "changeSet");
			}
			sQLiteStreamChangeSets.CheckInputStream();
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_concat_strm(this.inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, sQLiteStreamChangeSets.inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, this.outputStreamAdapter.GetOutputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_concat_strm");
			}
			return null;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing)
				{
					if (this.outputStreamAdapter != null)
					{
						this.outputStreamAdapter.Dispose();
						this.outputStreamAdapter = null;
					}
					if (this.inputStreamAdapter != null)
					{
						this.inputStreamAdapter.Dispose();
						this.inputStreamAdapter = null;
					}
					if (this.outputStream != null)
					{
						this.outputStream = null;
					}
					if (this.inputStream != null)
					{
						this.inputStream = null;
					}
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
			return new SQLiteStreamChangeSetEnumerator(this.inputStream, base.GetFlags());
		}

		public ISQLiteChangeSet Invert()
		{
			this.CheckDisposed();
			this.CheckInputStream();
			this.CheckOutputStream();
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_invert_strm(this.inputStreamAdapter.GetInputDelegate(), IntPtr.Zero, this.outputStreamAdapter.GetOutputDelegate(), IntPtr.Zero);
			if (sQLiteErrorCode != SQLiteErrorCode.Ok)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_invert_strm");
			}
			return null;
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}