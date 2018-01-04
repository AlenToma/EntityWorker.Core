using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Data.SQLite
{
	internal sealed class SQLiteConnectionHandle : CriticalHandle
	{
		private bool ownHandle;

		public override bool IsInvalid
		{
			get
			{
				return this.handle == IntPtr.Zero;
			}
		}

		public bool OwnHandle
		{
			get
			{
				return this.ownHandle;
			}
		}

		internal SQLiteConnectionHandle(IntPtr db, bool ownHandle) : this(ownHandle)
		{
			this.ownHandle = ownHandle;
			base.SetHandle(db);
		}

		private SQLiteConnectionHandle(bool ownHandle) : base(IntPtr.Zero)
		{
		}

		public static implicit operator IntPtr(SQLiteConnectionHandle db)
		{
			if (db == null)
			{
				return IntPtr.Zero;
			}
			return db.handle;
		}

		protected override bool ReleaseHandle()
		{
			if (!this.ownHandle)
			{
				return true;
			}
			try
			{
				try
				{
					IntPtr intPtr = Interlocked.Exchange(ref this.handle, IntPtr.Zero);
					if (intPtr != IntPtr.Zero)
					{
						SQLiteBase.CloseConnection(this, intPtr);
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