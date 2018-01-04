using System;

namespace System.Data.SQLite
{
	public sealed class SQLiteIndexConstraintUsage
	{
		public int argvIndex;

		public byte omit;

		internal SQLiteIndexConstraintUsage()
		{
		}

		internal SQLiteIndexConstraintUsage(UnsafeNativeMethods.sqlite3_index_constraint_usage constraintUsage) : this(constraintUsage.argvIndex, constraintUsage.omit)
		{
		}

		private SQLiteIndexConstraintUsage(int argvIndex, byte omit)
		{
			this.argvIndex = argvIndex;
			this.omit = omit;
		}
	}
}