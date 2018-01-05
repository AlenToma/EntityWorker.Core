using System;

namespace EntityWorker.SQLite
{
	public class UpdateEventArgs : EventArgs
	{
		public readonly string Database;

		public readonly string Table;

		public readonly UpdateEventType Event;

		public readonly long RowId;

		internal UpdateEventArgs(string database, string table, UpdateEventType eventType, long rowid)
		{
			this.Database = database;
			this.Table = table;
			this.Event = eventType;
			this.RowId = rowid;
		}
	}
}