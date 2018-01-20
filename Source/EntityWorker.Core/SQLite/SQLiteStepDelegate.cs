using System;

namespace EntityWorker.Core.SQLite
{
	public delegate void SQLiteStepDelegate(string param0, object[] args, int stepNumber, ref object contextData);
}