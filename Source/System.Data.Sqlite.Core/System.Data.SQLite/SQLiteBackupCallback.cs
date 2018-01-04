using System;

namespace System.Data.SQLite
{
	public delegate bool SQLiteBackupCallback(SQLiteConnection source, string sourceName, SQLiteConnection destination, string destinationName, int pages, int remainingPages, int totalPages, bool retry);
}