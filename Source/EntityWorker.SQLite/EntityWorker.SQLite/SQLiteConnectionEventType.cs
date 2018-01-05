using System;

namespace EntityWorker.SQLite
{
	public enum SQLiteConnectionEventType
	{
		Invalid = -1,
		Unknown = 0,
		Opening = 1,
		ConnectionString = 2,
		Opened = 3,
		ChangeDatabase = 4,
		NewTransaction = 5,
		EnlistTransaction = 6,
		NewCommand = 7,
		NewDataReader = 8,
		NewCriticalHandle = 9,
		Closing = 10,
		Closed = 11,
		DisposingCommand = 12,
		DisposingDataReader = 13,
		ClosingDataReader = 14,
		OpenedFromPool = 15,
		ClosedToPool = 16
	}
}