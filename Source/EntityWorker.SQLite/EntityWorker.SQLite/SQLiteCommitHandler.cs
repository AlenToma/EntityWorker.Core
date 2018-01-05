using System;

namespace EntityWorker.SQLite
{
	public delegate void SQLiteCommitHandler(object sender, CommitEventArgs e);
}