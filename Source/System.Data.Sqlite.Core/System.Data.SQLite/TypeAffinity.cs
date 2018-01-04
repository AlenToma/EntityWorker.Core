using System;

namespace System.Data.SQLite
{
	public enum TypeAffinity
	{
		Uninitialized = 0,
		Int64 = 1,
		Double = 2,
		Text = 3,
		Blob = 4,
		Null = 5,
		DateTime = 10,
		None = 11
	}
}