using System;
using System.IO;

namespace EntityWorker.SQLite
{
	public interface ISQLiteChangeGroup : IDisposable
	{
		void AddChangeSet(byte[] rawData);

		void AddChangeSet(Stream stream);

		void CreateChangeSet(ref byte[] rawData);

		void CreateChangeSet(Stream stream);
	}
}