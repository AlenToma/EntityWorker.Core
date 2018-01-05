using System;
using System.Collections.Generic;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteTypeCallbacksMap : Dictionary<string, SQLiteTypeCallbacks>
	{
		public SQLiteTypeCallbacksMap() : base(new TypeNameStringComparer())
		{
		}
	}
}