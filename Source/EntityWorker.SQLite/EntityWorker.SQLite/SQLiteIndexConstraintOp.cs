using System;

namespace EntityWorker.SQLite
{
	public enum SQLiteIndexConstraintOp : byte
	{
		EqualTo = 2,
		GreaterThan = 4,
		LessThanOrEqualTo = 8,
		LessThan = 16,
		GreaterThanOrEqualTo = 32,
		Match = 64,
		Like = 65,
		Glob = 66,
		Regexp = 67,
		NotEqualTo = 68,
		IsNot = 69,
		IsNotNull = 70,
		IsNull = 71,
		Is = 72
	}
}