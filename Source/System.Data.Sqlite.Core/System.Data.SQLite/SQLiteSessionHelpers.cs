using System;

namespace System.Data.SQLite
{
	internal static class SQLiteSessionHelpers
	{
		public static void CheckRawData(byte[] rawData)
		{
			if (rawData == null)
			{
				throw new ArgumentNullException("rawData");
			}
			if ((int)rawData.Length == 0)
			{
				throw new ArgumentException("empty change set data", "rawData");
			}
		}
	}
}