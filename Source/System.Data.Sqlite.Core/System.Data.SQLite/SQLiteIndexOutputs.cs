using System;

namespace System.Data.SQLite
{
	public sealed class SQLiteIndexOutputs
	{
		private SQLiteIndexConstraintUsage[] constraintUsages;

		private int indexNumber;

		private string indexString;

		private int needToFreeIndexString;

		private int orderByConsumed;

		private double? estimatedCost;

		private long? estimatedRows;

		private SQLiteIndexFlags? indexFlags;

		private long? columnsUsed;

		public long? ColumnsUsed
		{
			get
			{
				return this.columnsUsed;
			}
			set
			{
				this.columnsUsed = value;
			}
		}

		public SQLiteIndexConstraintUsage[] ConstraintUsages
		{
			get
			{
				return this.constraintUsages;
			}
		}

		public double? EstimatedCost
		{
			get
			{
				return this.estimatedCost;
			}
			set
			{
				this.estimatedCost = value;
			}
		}

		public long? EstimatedRows
		{
			get
			{
				return this.estimatedRows;
			}
			set
			{
				this.estimatedRows = value;
			}
		}

		public SQLiteIndexFlags? IndexFlags
		{
			get
			{
				return this.indexFlags;
			}
			set
			{
				this.indexFlags = value;
			}
		}

		public int IndexNumber
		{
			get
			{
				return this.indexNumber;
			}
			set
			{
				this.indexNumber = value;
			}
		}

		public string IndexString
		{
			get
			{
				return this.indexString;
			}
			set
			{
				this.indexString = value;
			}
		}

		public int NeedToFreeIndexString
		{
			get
			{
				return this.needToFreeIndexString;
			}
			set
			{
				this.needToFreeIndexString = value;
			}
		}

		public int OrderByConsumed
		{
			get
			{
				return this.orderByConsumed;
			}
			set
			{
				this.orderByConsumed = value;
			}
		}

		internal SQLiteIndexOutputs(int nConstraint)
		{
			this.constraintUsages = new SQLiteIndexConstraintUsage[nConstraint];
			for (int i = 0; i < nConstraint; i++)
			{
				this.constraintUsages[i] = new SQLiteIndexConstraintUsage();
			}
		}

		public bool CanUseColumnsUsed()
		{
			if (UnsafeNativeMethods.sqlite3_libversion_number() >= 3010000)
			{
				return true;
			}
			return false;
		}

		public bool CanUseEstimatedRows()
		{
			if (UnsafeNativeMethods.sqlite3_libversion_number() >= 3008002)
			{
				return true;
			}
			return false;
		}

		public bool CanUseIndexFlags()
		{
			if (UnsafeNativeMethods.sqlite3_libversion_number() >= 3009000)
			{
				return true;
			}
			return false;
		}
	}
}