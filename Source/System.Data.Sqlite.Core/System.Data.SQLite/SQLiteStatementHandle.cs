using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Data.SQLite
{
	internal sealed class SQLiteStatementHandle : CriticalHandle
	{
		private SQLiteConnectionHandle cnn;

		public override bool IsInvalid
		{
			get
			{
				return this.handle == IntPtr.Zero;
			}
		}

		internal SQLiteStatementHandle(SQLiteConnectionHandle cnn, IntPtr stmt) : this()
		{
			this.cnn = cnn;
			base.SetHandle(stmt);
		}

		private SQLiteStatementHandle() : base(IntPtr.Zero)
		{
		}

		public static implicit operator IntPtr(SQLiteStatementHandle stmt)
		{
			if (stmt == null)
			{
				return IntPtr.Zero;
			}
			return stmt.handle;
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
						SQLiteBase.FinalizeStatement(this.cnn, intPtr);
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