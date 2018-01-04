using System;

namespace System.Data.SQLite
{
	public delegate SQLiteChangeSetConflictResult SessionConflictCallback(object clientData, SQLiteChangeSetConflictType type, ISQLiteChangeSetMetadataItem item);
}