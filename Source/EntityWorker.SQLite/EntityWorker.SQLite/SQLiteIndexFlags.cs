using System;

namespace EntityWorker.SQLite
{
	[Flags]
	public enum SQLiteIndexFlags
	{
		None,
		ScanUnique
	}
}