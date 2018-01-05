using System;

namespace EntityWorker.SQLite
{
	public interface ISQLiteChangeSetMetadataItem : IDisposable
	{
		bool Indirect
		{
			get;
		}

		int NumberOfColumns
		{
			get;
		}

		int NumberOfForeignKeyConflicts
		{
			get;
		}

		SQLiteAuthorizerActionCode OperationCode
		{
			get;
		}

		bool[] PrimaryKeyColumns
		{
			get;
		}

		string TableName
		{
			get;
		}

		SQLiteValue GetConflictValue(int columnIndex);

		SQLiteValue GetNewValue(int columnIndex);

		SQLiteValue GetOldValue(int columnIndex);
	}
}