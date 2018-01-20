using System;
using System.Data;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	public class ConnectionEventArgs : System.EventArgs
	{
		public readonly SQLiteConnectionEventType EventType;

		public readonly StateChangeEventArgs EventArgs;

		public readonly IDbTransaction Transaction;

		public readonly IDbCommand Command;

		public readonly IDataReader DataReader;

		public readonly System.Runtime.InteropServices.CriticalHandle CriticalHandle;

		public readonly string Text;

		public readonly object Data;

		internal ConnectionEventArgs(SQLiteConnectionEventType eventType, StateChangeEventArgs eventArgs, IDbTransaction transaction, IDbCommand command, IDataReader dataReader, System.Runtime.InteropServices.CriticalHandle criticalHandle, string text, object data)
		{
			this.EventType = eventType;
			this.EventArgs = eventArgs;
			this.Transaction = transaction;
			this.Command = command;
			this.DataReader = dataReader;
			this.CriticalHandle = criticalHandle;
			this.Text = text;
			this.Data = data;
		}
	}
}