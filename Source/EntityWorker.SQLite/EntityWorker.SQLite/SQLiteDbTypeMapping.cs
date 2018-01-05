using System;
using System.Data;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteDbTypeMapping
	{
		internal string typeName;

		internal DbType dataType;

		internal bool primary;

		internal SQLiteDbTypeMapping(string newTypeName, DbType newDataType, bool newPrimary)
		{
			this.typeName = newTypeName;
			this.dataType = newDataType;
			this.primary = newPrimary;
		}
	}
}