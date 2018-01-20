using System;

namespace EntityWorker.Core.SQLite
{
	[Flags]
	public enum SQLiteIndexFlags
	{
		None,
		ScanUnique
	}
}