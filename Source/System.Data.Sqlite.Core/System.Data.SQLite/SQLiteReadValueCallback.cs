using System;

namespace System.Data.SQLite
{
	public delegate void SQLiteReadValueCallback(SQLiteConvert convert, SQLiteDataReader dataReader, SQLiteConnectionFlags flags, SQLiteReadEventArgs eventArgs, string typeName, int index, object userData, out bool complete);
}