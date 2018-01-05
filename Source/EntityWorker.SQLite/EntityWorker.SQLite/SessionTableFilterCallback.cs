using System;

namespace EntityWorker.SQLite
{
	public delegate bool SessionTableFilterCallback(object clientData, string name);
}