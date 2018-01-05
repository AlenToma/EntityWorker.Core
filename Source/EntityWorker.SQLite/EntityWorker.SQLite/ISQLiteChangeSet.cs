using System;
using System.Collections;
using System.Collections.Generic;

namespace EntityWorker.SQLite
{
	public interface ISQLiteChangeSet : IEnumerable<ISQLiteChangeSetMetadataItem>, IEnumerable, IDisposable
	{
		void Apply(SessionConflictCallback conflictCallback, object clientData);

		void Apply(SessionConflictCallback conflictCallback, SessionTableFilterCallback tableFilterCallback, object clientData);

		ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet);

		ISQLiteChangeSet Invert();
	}
}