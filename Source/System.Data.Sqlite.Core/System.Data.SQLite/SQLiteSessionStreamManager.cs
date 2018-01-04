using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.Data.SQLite
{
	internal sealed class SQLiteSessionStreamManager : IDisposable
	{
		private Dictionary<Stream, SQLiteStreamAdapter> streamAdapters;

		private SQLiteConnectionFlags flags;

		private bool disposed;

		public SQLiteSessionStreamManager(SQLiteConnectionFlags flags)
		{
			this.flags = flags;
			this.InitializeStreamAdapters();
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteSessionStreamManager).Name);
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
				if (!this.disposed && disposing)
				{
					this.DisposeStreamAdapters();
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		private void DisposeStreamAdapters()
		{
			if (this.streamAdapters == null)
			{
				return;
			}
			foreach (KeyValuePair<Stream, SQLiteStreamAdapter> streamAdapter in this.streamAdapters)
			{
				SQLiteStreamAdapter value = streamAdapter.Value;
				if (value == null)
				{
					continue;
				}
				value.Dispose();
			}
			this.streamAdapters.Clear();
			this.streamAdapters = null;
		}

		~SQLiteSessionStreamManager()
		{
			this.Dispose(false);
		}

		public SQLiteStreamAdapter GetAdapter(Stream stream)
		{
			SQLiteStreamAdapter sQLiteStreamAdapter;
			this.CheckDisposed();
			if (stream == null)
			{
				return null;
			}
			if (this.streamAdapters.TryGetValue(stream, out sQLiteStreamAdapter))
			{
				return sQLiteStreamAdapter;
			}
			sQLiteStreamAdapter = new SQLiteStreamAdapter(stream, this.flags);
			this.streamAdapters.Add(stream, sQLiteStreamAdapter);
			return sQLiteStreamAdapter;
		}

		private void InitializeStreamAdapters()
		{
			if (this.streamAdapters != null)
			{
				return;
			}
			this.streamAdapters = new Dictionary<Stream, SQLiteStreamAdapter>();
		}
	}
}