using System;

namespace System.Data.SQLite
{
	public delegate void SQLiteCommitHandler(object sender, CommitEventArgs e);
}