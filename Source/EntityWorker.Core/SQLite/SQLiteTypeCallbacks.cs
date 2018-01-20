using System;

namespace EntityWorker.Core.SQLite
{
	public sealed class SQLiteTypeCallbacks
	{
		private string typeName;

		private SQLiteBindValueCallback bindValueCallback;

		private SQLiteReadValueCallback readValueCallback;

		private object bindValueUserData;

		private object readValueUserData;

		public SQLiteBindValueCallback BindValueCallback
		{
			get
			{
				return this.bindValueCallback;
			}
		}

		public object BindValueUserData
		{
			get
			{
				return this.bindValueUserData;
			}
		}

		public SQLiteReadValueCallback ReadValueCallback
		{
			get
			{
				return this.readValueCallback;
			}
		}

		public object ReadValueUserData
		{
			get
			{
				return this.readValueUserData;
			}
		}

		public string TypeName
		{
			get
			{
				return this.typeName;
			}
			internal set
			{
				this.typeName = value;
			}
		}

		private SQLiteTypeCallbacks(SQLiteBindValueCallback bindValueCallback, SQLiteReadValueCallback readValueCallback, object bindValueUserData, object readValueUserData)
		{
			this.bindValueCallback = bindValueCallback;
			this.readValueCallback = readValueCallback;
			this.bindValueUserData = bindValueUserData;
			this.readValueUserData = readValueUserData;
		}

		public static SQLiteTypeCallbacks Create(SQLiteBindValueCallback bindValueCallback, SQLiteReadValueCallback readValueCallback, object bindValueUserData, object readValueUserData)
		{
			return new SQLiteTypeCallbacks(bindValueCallback, readValueCallback, bindValueUserData, readValueUserData);
		}
	}
}