using System;
using System.Data;

namespace System.Data.SQLite
{
	internal sealed class SQLiteType
	{
		internal DbType Type;

		internal TypeAffinity Affinity;

		public SQLiteType()
		{
		}

		public SQLiteType(TypeAffinity affinity, DbType type) : this()
		{
			this.Affinity = affinity;
			this.Type = type;
		}
	}
}