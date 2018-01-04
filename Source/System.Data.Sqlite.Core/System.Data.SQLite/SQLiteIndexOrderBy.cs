using System;

namespace System.Data.SQLite
{
	public sealed class SQLiteIndexOrderBy
	{
		public int iColumn;

		public byte desc;

		internal SQLiteIndexOrderBy(UnsafeNativeMethods.sqlite3_index_orderby orderBy) : this(orderBy.iColumn, orderBy.desc)
		{
		}

		private SQLiteIndexOrderBy(int iColumn, byte desc)
		{
			this.iColumn = iColumn;
			this.desc = desc;
		}
	}
}