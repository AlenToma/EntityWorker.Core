using System;
using System.IO;

namespace EntityWorker.SQLite
{
	public interface ISQLiteSession : IDisposable
	{
		void AttachTable(string name);

		void CreateChangeSet(ref byte[] rawData);

		void CreateChangeSet(Stream stream);

		void CreatePatchSet(ref byte[] rawData);

		void CreatePatchSet(Stream stream);

		bool IsEmpty();

		bool IsEnabled();

		bool IsIndirect();

		void LoadDifferencesFromTable(string fromDatabaseName, string tableName);

		void SetTableFilter(SessionTableFilterCallback callback, object clientData);

		void SetToDirect();

		void SetToDisabled();

		void SetToEnabled();

		void SetToIndirect();
	}
}