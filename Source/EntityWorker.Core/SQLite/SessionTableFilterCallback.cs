using System;

namespace EntityWorker.Core.SQLite
{
	public delegate bool SessionTableFilterCallback(object clientData, string name);
}