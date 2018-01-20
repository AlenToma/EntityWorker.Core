using System;

namespace EntityWorker.Core.SQLite
{
	public interface ISQLiteManagedModule
	{
		bool Declared
		{
			get;
		}

		string Name
		{
			get;
		}

		SQLiteErrorCode Begin(SQLiteVirtualTable table);

		SQLiteErrorCode BestIndex(SQLiteVirtualTable table, SQLiteIndex index);

		SQLiteErrorCode Close(SQLiteVirtualTableCursor cursor);

		SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index);

		SQLiteErrorCode Commit(SQLiteVirtualTable table);

		SQLiteErrorCode Connect(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error);

		SQLiteErrorCode Create(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error);

		SQLiteErrorCode Destroy(SQLiteVirtualTable table);

		SQLiteErrorCode Disconnect(SQLiteVirtualTable table);

		bool Eof(SQLiteVirtualTableCursor cursor);

		SQLiteErrorCode Filter(SQLiteVirtualTableCursor cursor, int indexNumber, string indexString, SQLiteValue[] values);

		bool FindFunction(SQLiteVirtualTable table, int argumentCount, string name, ref SQLiteFunction function, ref IntPtr pClientData);

		SQLiteErrorCode Next(SQLiteVirtualTableCursor cursor);

		SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor);

		SQLiteErrorCode Release(SQLiteVirtualTable table, int savepoint);

		SQLiteErrorCode Rename(SQLiteVirtualTable table, string newName);

		SQLiteErrorCode Rollback(SQLiteVirtualTable table);

		SQLiteErrorCode RollbackTo(SQLiteVirtualTable table, int savepoint);

		SQLiteErrorCode RowId(SQLiteVirtualTableCursor cursor, ref long rowId);

		SQLiteErrorCode Savepoint(SQLiteVirtualTable table, int savepoint);

		SQLiteErrorCode Sync(SQLiteVirtualTable table);

		SQLiteErrorCode Update(SQLiteVirtualTable table, SQLiteValue[] values, ref long rowId);
	}
}