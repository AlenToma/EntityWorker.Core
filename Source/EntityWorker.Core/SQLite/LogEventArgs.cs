using System;

namespace EntityWorker.Core.SQLite
{
	public class LogEventArgs : EventArgs
	{
		public readonly object ErrorCode;

		public readonly string Message;

		public readonly object Data;

		internal LogEventArgs(IntPtr pUserData, object errorCode, string message, object data)
		{
			this.ErrorCode = errorCode;
			this.Message = message;
			this.Data = data;
		}
	}
}