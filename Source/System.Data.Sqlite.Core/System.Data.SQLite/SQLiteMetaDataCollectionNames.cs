using System;

namespace System.Data.SQLite
{
	public static class SQLiteMetaDataCollectionNames
	{
		public readonly static string Catalogs;

		public readonly static string Columns;

		public readonly static string Indexes;

		public readonly static string IndexColumns;

		public readonly static string Tables;

		public readonly static string Views;

		public readonly static string ViewColumns;

		public readonly static string ForeignKeys;

		public readonly static string Triggers;

		static SQLiteMetaDataCollectionNames()
		{
			SQLiteMetaDataCollectionNames.Catalogs = "Catalogs";
			SQLiteMetaDataCollectionNames.Columns = "Columns";
			SQLiteMetaDataCollectionNames.Indexes = "Indexes";
			SQLiteMetaDataCollectionNames.IndexColumns = "IndexColumns";
			SQLiteMetaDataCollectionNames.Tables = "Tables";
			SQLiteMetaDataCollectionNames.Views = "Views";
			SQLiteMetaDataCollectionNames.ViewColumns = "ViewColumns";
			SQLiteMetaDataCollectionNames.ForeignKeys = "ForeignKeys";
			SQLiteMetaDataCollectionNames.Triggers = "Triggers";
		}
	}
}