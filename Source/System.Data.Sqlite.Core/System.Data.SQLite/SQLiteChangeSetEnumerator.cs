using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Data.SQLite
{
	internal abstract class SQLiteChangeSetEnumerator : IEnumerator<ISQLiteChangeSetMetadataItem>, IDisposable, IEnumerator
	{
		private SQLiteChangeSetIterator iterator;

		private bool disposed;

		public ISQLiteChangeSetMetadataItem Current
		{
			get
			{
				this.CheckDisposed();
				return new SQLiteChangeSetMetadataItem(this.iterator);
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				this.CheckDisposed();
				return this.Current;
			}
		}

		public SQLiteChangeSetEnumerator(SQLiteChangeSetIterator iterator)
		{
			this.SetIterator(iterator);
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteChangeSetEnumerator).Name);
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

		private void CloseIterator()
		{
			if (this.iterator != null)
			{
				this.iterator.Dispose();
				this.iterator = null;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing)
				{
					this.CloseIterator();
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		~SQLiteChangeSetEnumerator()
		{
			this.Dispose(false);
		}

		public bool MoveNext()
		{
			this.CheckDisposed();
			this.CheckIterator();
			return this.iterator.Next();
		}

		public virtual void Reset()
		{
			this.CheckDisposed();
			throw new NotImplementedException();
		}

		protected void ResetIterator(SQLiteChangeSetIterator iterator)
		{
			this.CloseIterator();
			this.SetIterator(iterator);
		}

		private void SetIterator(SQLiteChangeSetIterator iterator)
		{
			this.iterator = iterator;
		}
	}
}