using System;

namespace EntityWorker.SQLite
{
	public class ProgressEventArgs : EventArgs
	{
		public readonly IntPtr UserData;

		public SQLiteProgressReturnCode ReturnCode;

		private ProgressEventArgs()
		{
			this.UserData = IntPtr.Zero;
			this.ReturnCode = SQLiteProgressReturnCode.Continue;
		}

		internal ProgressEventArgs(IntPtr pUserData, SQLiteProgressReturnCode returnCode) : this()
		{
			this.UserData = pUserData;
			this.ReturnCode = returnCode;
		}
	}
}