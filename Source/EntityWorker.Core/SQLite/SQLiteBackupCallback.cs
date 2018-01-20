using System;

namespace EntityWorker.Core.SQLite
{
	public delegate bool SQLiteBackupCallback(SQLiteConnection source, string sourceName, SQLiteConnection destination, string destinationName, int pages, int remainingPages, int totalPages, bool retry);
}