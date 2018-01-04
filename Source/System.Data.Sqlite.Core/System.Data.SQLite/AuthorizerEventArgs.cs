using System;

namespace System.Data.SQLite
{
	public class AuthorizerEventArgs : EventArgs
	{
		public readonly IntPtr UserData;

		public readonly SQLiteAuthorizerActionCode ActionCode;

		public readonly string Argument1;

		public readonly string Argument2;

		public readonly string Database;

		public readonly string Context;

		public SQLiteAuthorizerReturnCode ReturnCode;

		private AuthorizerEventArgs()
		{
			this.UserData = IntPtr.Zero;
			this.ActionCode = SQLiteAuthorizerActionCode.None;
			this.Argument1 = null;
			this.Argument2 = null;
			this.Database = null;
			this.Context = null;
			this.ReturnCode = SQLiteAuthorizerReturnCode.Ok;
		}

		internal AuthorizerEventArgs(IntPtr pUserData, SQLiteAuthorizerActionCode actionCode, string argument1, string argument2, string database, string context, SQLiteAuthorizerReturnCode returnCode) : this()
		{
			this.UserData = pUserData;
			this.ActionCode = actionCode;
			this.Argument1 = argument1;
			this.Argument2 = argument2;
			this.Database = database;
			this.Context = context;
			this.ReturnCode = returnCode;
		}
	}
}