using System;

namespace System.Data.SQLite
{
	public enum SQLiteChangeSetConflictResult
	{
		Omit,
		Replace,
		Abort
	}
}