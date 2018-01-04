using System;

namespace System.Data.SQLite
{
	[Flags]
	internal enum SQLiteOpenFlagsEnum
	{
		None = 0,
		ReadOnly = 1,
		ReadWrite = 2,
		Create = 4,
		Default = 6,
		Uri = 64,
		Memory = 128
	}
}