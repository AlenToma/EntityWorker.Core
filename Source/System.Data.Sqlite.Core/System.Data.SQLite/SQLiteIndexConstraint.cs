using System;

namespace System.Data.SQLite
{
	public sealed class SQLiteIndexConstraint
	{
		public int iColumn;

		public SQLiteIndexConstraintOp op;

		public byte usable;

		public int iTermOffset;

		internal SQLiteIndexConstraint(UnsafeNativeMethods.sqlite3_index_constraint constraint) : this(constraint.iColumn, constraint.op, constraint.usable, constraint.iTermOffset)
		{
		}

		private SQLiteIndexConstraint(int iColumn, SQLiteIndexConstraintOp op, byte usable, int iTermOffset)
		{
			this.iColumn = iColumn;
			this.op = op;
			this.usable = usable;
			this.iTermOffset = iTermOffset;
		}
	}
}