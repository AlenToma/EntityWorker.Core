using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteBackupHandle : CriticalHandle
	{
		private SQLiteConnectionHandle cnn;

		public override bool IsInvalid
		{
			get
			{
				return this.handle == IntPtr.Zero;
			}
		}

		internal SQLiteBackupHandle(SQLiteConnectionHandle cnn, IntPtr backup) : this()
		{
			this.cnn = cnn;
			base.SetHandle(backup);
		}

		private SQLiteBackupHandle() : base(IntPtr.Zero)
		{
		}

		public static implicit operator IntPtr(SQLiteBackupHandle backup)
		{
			if (backup == null)
			{
				return IntPtr.Zero;
			}
			return backup.handle;
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
						SQLiteBase.FinishBackup(this.cnn, intPtr);
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