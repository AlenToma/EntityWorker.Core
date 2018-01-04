using System;

namespace System.Data.SQLite
{
	public enum SQLiteJournalModeEnum
	{
		Default = -1,
		Delete = 0,
		Persist = 1,
		Off = 2,
		Truncate = 3,
		Memory = 4,
		Wal = 5
	}
}