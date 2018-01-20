using System;

namespace EntityWorker.Core.SQLite
{
	public delegate void SQLiteBindValueCallback(SQLiteConvert convert, SQLiteCommand command, SQLiteConnectionFlags flags, SQLiteParameter parameter, string typeName, int index, object userData, out bool complete);
}