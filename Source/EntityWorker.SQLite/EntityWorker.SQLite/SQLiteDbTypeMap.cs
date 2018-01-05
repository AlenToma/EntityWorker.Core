using System;
using System.Collections.Generic;
using System.Data;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteDbTypeMap : Dictionary<string, SQLiteDbTypeMapping>
	{
		private Dictionary<DbType, SQLiteDbTypeMapping> reverse;

		public SQLiteDbTypeMap() : base(new TypeNameStringComparer())
		{
			this.reverse = new Dictionary<DbType, SQLiteDbTypeMapping>();
		}

		public SQLiteDbTypeMap(IEnumerable<SQLiteDbTypeMapping> collection) : this()
		{
			this.Add(collection);
		}

		public void Add(IEnumerable<SQLiteDbTypeMapping> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			foreach (SQLiteDbTypeMapping sQLiteDbTypeMapping in collection)
			{
				this.Add(sQLiteDbTypeMapping);
			}
		}

		public void Add(SQLiteDbTypeMapping item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (item.typeName == null)
			{
				throw new ArgumentException("item type name cannot be null");
			}
			base.Add(item.typeName, item);
			if (item.primary)
			{
				this.reverse.Add(item.dataType, item);
			}
		}

		public new int Clear()
		{
			int count = 0;
			if (this.reverse != null)
			{
				count += this.reverse.Count;
				this.reverse.Clear();
			}
			count += base.Count;
			base.Clear();
			return count;
		}

		public bool ContainsKey(DbType key)
		{
			if (this.reverse == null)
			{
				return false;
			}
			return this.reverse.ContainsKey(key);
		}

		public bool Remove(DbType key)
		{
			if (this.reverse == null)
			{
				return false;
			}
			return this.reverse.Remove(key);
		}

		public bool TryGetValue(DbType key, out SQLiteDbTypeMapping value)
		{
			if (this.reverse == null)
			{
				value = null;
				return false;
			}
			return this.reverse.TryGetValue(key, out value);
		}
	}
}