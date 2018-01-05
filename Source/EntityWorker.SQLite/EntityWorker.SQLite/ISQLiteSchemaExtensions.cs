using System;

namespace EntityWorker.SQLite
{
	public interface ISQLiteSchemaExtensions
	{
		void BuildTempSchema(SQLiteConnection connection);
	}
}