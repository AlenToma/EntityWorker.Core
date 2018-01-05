using System;

namespace EntityWorker.SQLite
{
	public enum SQLiteChangeSetConflictResult
	{
		Omit,
		Replace,
		Abort
	}
}