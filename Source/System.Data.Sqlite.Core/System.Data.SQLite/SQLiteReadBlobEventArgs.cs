using System;

namespace System.Data.SQLite
{
	public class SQLiteReadBlobEventArgs : SQLiteReadEventArgs
	{
		private bool readOnly;

		public bool ReadOnly
		{
			get
			{
				return this.readOnly;
			}
			set
			{
				this.readOnly = value;
			}
		}

		internal SQLiteReadBlobEventArgs(bool readOnly)
		{
			this.readOnly = readOnly;
		}
	}
}