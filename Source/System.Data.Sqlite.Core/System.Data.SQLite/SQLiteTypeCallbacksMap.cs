using System;
using System.Collections.Generic;

namespace System.Data.SQLite
{
	internal sealed class SQLiteTypeCallbacksMap : Dictionary<string, SQLiteTypeCallbacks>
	{
		public SQLiteTypeCallbacksMap() : base(new TypeNameStringComparer())
		{
		}
	}
}