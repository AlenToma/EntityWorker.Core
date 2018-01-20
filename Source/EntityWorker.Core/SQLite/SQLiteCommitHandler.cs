using System;

namespace EntityWorker.Core.SQLite
{
	public delegate void SQLiteCommitHandler(object sender, CommitEventArgs e);
}