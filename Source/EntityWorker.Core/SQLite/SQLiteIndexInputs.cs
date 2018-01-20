using System;

namespace EntityWorker.Core.SQLite
{
	public sealed class SQLiteIndexInputs
	{
		private SQLiteIndexConstraint[] constraints;

		private SQLiteIndexOrderBy[] orderBys;

		public SQLiteIndexConstraint[] Constraints
		{
			get
			{
				return this.constraints;
			}
		}

		public SQLiteIndexOrderBy[] OrderBys
		{
			get
			{
				return this.orderBys;
			}
		}

		internal SQLiteIndexInputs(int nConstraint, int nOrderBy)
		{
			this.constraints = new SQLiteIndexConstraint[nConstraint];
			this.orderBys = new SQLiteIndexOrderBy[nOrderBy];
		}
	}
}