using System;

namespace EntityWorker.SQLite
{
	public delegate SQLiteChangeSetConflictResult SessionConflictCallback(object clientData, SQLiteChangeSetConflictType type, ISQLiteChangeSetMetadataItem item);
}