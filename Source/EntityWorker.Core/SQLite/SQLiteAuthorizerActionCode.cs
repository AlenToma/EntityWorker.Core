using System;

namespace EntityWorker.Core.SQLite
{
	public enum SQLiteAuthorizerActionCode
	{
		None = -1,
		Copy = 0,
		CreateIndex = 1,
		CreateTable = 2,
		CreateTempIndex = 3,
		CreateTempTable = 4,
		CreateTempTrigger = 5,
		CreateTempView = 6,
		CreateTrigger = 7,
		CreateView = 8,
		Delete = 9,
		DropIndex = 10,
		DropTable = 11,
		DropTempIndex = 12,
		DropTempTable = 13,
		DropTempTrigger = 14,
		DropTempView = 15,
		DropTrigger = 16,
		DropView = 17,
		Insert = 18,
		Pragma = 19,
		Read = 20,
		Select = 21,
		Transaction = 22,
		Update = 23,
		Attach = 24,
		Detach = 25,
		AlterTable = 26,
		Reindex = 27,
		Analyze = 28,
		CreateVtable = 29,
		DropVtable = 30,
		Function = 31,
		Savepoint = 32,
		Recursive = 33
	}
}