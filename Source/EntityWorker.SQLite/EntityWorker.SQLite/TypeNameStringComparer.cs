using System;
using System.Collections.Generic;

namespace EntityWorker.SQLite
{
	internal sealed class TypeNameStringComparer : IEqualityComparer<string>, IComparer<string>
	{
		public TypeNameStringComparer()
		{
		}

		public int Compare(string x, string y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			return x.CompareTo(y);
		}

		public bool Equals(string left, string right)
		{
			return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
		}

		public int GetHashCode(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return value.ToLowerInvariant().GetHashCode();
		}
	}
}