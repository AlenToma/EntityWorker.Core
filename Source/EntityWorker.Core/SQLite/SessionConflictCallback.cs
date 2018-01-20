using System;

namespace EntityWorker.Core.SQLite
{
	public delegate SQLiteChangeSetConflictResult SessionConflictCallback(object clientData, SQLiteChangeSetConflictType type, ISQLiteChangeSetMetadataItem item);
}