using System;

namespace EntityWorker.Core.SQLite
{
	public interface ISQLiteSchemaExtensions
	{
		void BuildTempSchema(SQLiteConnection connection);
	}
}