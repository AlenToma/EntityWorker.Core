using System;

namespace EntityWorker.SQLite
{
	public enum SQLiteDateFormats
	{
		Ticks = 0,
		Default = 1,
		ISO8601 = 1,
		JulianDay = 2,
		UnixEpoch = 3,
		InvariantCulture = 4,
		CurrentCulture = 5
	}
}