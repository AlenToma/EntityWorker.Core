using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteBlobHandle : CriticalHandle
	{
		private SQLiteConnectionHandle cnn;

		public override bool IsInvalid
		{
			get
			{
				return this.handle == IntPtr.Zero;
			}
		}

		internal SQLiteBlobHandle(SQLiteConnectionHandle cnn, IntPtr blob) : this()
		{
			this.cnn = cnn;
			base.SetHandle(blob);
		}

		private SQLiteBlobHandle() : base(IntPtr.Zero)
		{
		}

		public static implicit operator IntPtr(SQLiteBlobHandle blob)
		{
			if (blob == null)
			{
				return IntPtr.Zero;
			}
			return blob.handle;
		}

		protected override bool ReleaseHandle()
		{
			try
			{
				try
				{
					IntPtr intPtr = Interlocked.Exchange(ref this.handle, IntPtr.Zero);
					if (intPtr != IntPtr.Zero)
					{
						SQLiteBase.CloseBlob(this.cnn, intPtr);
					}
				}
				catch (SQLiteException sQLiteException)
				{
				}
			}
			finally
			{
				base.SetHandleAsInvalid();
			}
			return true;
		}
	}
}