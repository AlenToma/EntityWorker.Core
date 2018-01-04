using System;

namespace System.Data.SQLite
{
	public class SQLiteReadValueEventArgs : SQLiteReadEventArgs
	{
		private string methodName;

		private SQLiteReadEventArgs extraEventArgs;

		private SQLiteDataReaderValue @value;

		public SQLiteReadEventArgs ExtraEventArgs
		{
			get
			{
				return this.extraEventArgs;
			}
		}

		public string MethodName
		{
			get
			{
				return this.methodName;
			}
		}

		public SQLiteDataReaderValue Value
		{
			get
			{
				return this.@value;
			}
		}

		internal SQLiteReadValueEventArgs(string methodName, SQLiteReadEventArgs extraEventArgs, SQLiteDataReaderValue value)
		{
			this.methodName = methodName;
			this.extraEventArgs = extraEventArgs;
			this.@value = value;
		}
	}
}