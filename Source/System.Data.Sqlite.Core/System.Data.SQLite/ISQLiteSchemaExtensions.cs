using System;

namespace System.Data.SQLite
{
	public interface ISQLiteSchemaExtensions
	{
		void BuildTempSchema(SQLiteConnection connection);
	}
}