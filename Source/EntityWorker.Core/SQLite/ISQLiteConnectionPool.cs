using System;
using System.Collections.Generic;

namespace EntityWorker.Core.SQLite
{
	public interface ISQLiteConnectionPool
	{
		void Add(string fileName, object handle, int version);

		void ClearAllPools();

		void ClearPool(string fileName);

		void GetCounts(string fileName, ref Dictionary<string, int> counts, ref int openCount, ref int closeCount, ref int totalCount);

		object Remove(string fileName, int maxPoolSize, out int version);
	}
}