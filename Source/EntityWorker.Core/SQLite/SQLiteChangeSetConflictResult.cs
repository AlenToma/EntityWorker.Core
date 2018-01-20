using System;

namespace EntityWorker.Core.SQLite
{
	public enum SQLiteChangeSetConflictResult
	{
		Omit,
		Replace,
		Abort
	}
}