using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.SQLite
{
	public sealed class SQLiteCommandBuilder : DbCommandBuilder
	{
		private bool disposed;

		[Browsable(false)]
		public override System.Data.Common.CatalogLocation CatalogLocation
		{
			get
			{
				this.CheckDisposed();
				return base.CatalogLocation;
			}
			set
			{
				this.CheckDisposed();
				base.CatalogLocation = value;
			}
		}

		[Browsable(false)]
		public override string CatalogSeparator
		{
			get
			{
				this.CheckDisposed();
				return base.CatalogSeparator;
			}
			set
			{
				this.CheckDisposed();
				base.CatalogSeparator = value;
			}
		}

		public new SQLiteDataAdapter DataAdapter
		{
			get
			{
				this.CheckDisposed();
				return (SQLiteDataAdapter)base.DataAdapter;
			}
			set
			{
				this.CheckDisposed();
				base.DataAdapter = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("[")]
		public override string QuotePrefix
		{
			get
			{
				this.CheckDisposed();
				return base.QuotePrefix;
			}
			set
			{
				this.CheckDisposed();
				base.QuotePrefix = value;
			}
		}

		[Browsable(false)]
		public override string QuoteSuffix
		{
			get
			{
				this.CheckDisposed();
				return base.QuoteSuffix;
			}
			set
			{
				this.CheckDisposed();
				base.QuoteSuffix = value;
			}
		}

		[Browsable(false)]
		public override string SchemaSeparator
		{
			get
			{
				this.CheckDisposed();
				return base.SchemaSeparator;
			}
			set
			{
				this.CheckDisposed();
				base.SchemaSeparator = value;
			}
		}

		public SQLiteCommandBuilder() : this(null)
		{
		}

		public SQLiteCommandBuilder(SQLiteDataAdapter adp)
		{
			this.QuotePrefix = "[";
			this.QuoteSuffix = "]";
			this.DataAdapter = adp;
		}

		protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause)
		{
			((SQLiteParameter)parameter).DbType = (DbType)row[SchemaTableColumn.ProviderType];
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteCommandBuilder).Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				bool flag = this.disposed;
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		public new SQLiteCommand GetDeleteCommand()
		{
			this.CheckDisposed();
			return (SQLiteCommand)base.GetDeleteCommand();
		}

		public new SQLiteCommand GetDeleteCommand(bool useColumnsForParameterNames)
		{
			this.CheckDisposed();
			return (SQLiteCommand)base.GetDeleteCommand(useColumnsForParameterNames);
		}

		public new SQLiteCommand GetInsertCommand()
		{
			this.CheckDisposed();
			return (SQLiteCommand)base.GetInsertCommand();
		}

		public new SQLiteCommand GetInsertCommand(bool useColumnsForParameterNames)
		{
			this.CheckDisposed();
			return (SQLiteCommand)base.GetInsertCommand(useColumnsForParameterNames);
		}

		protected override string GetParameterName(string parameterName)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { parameterName };
			return HelperMethods.StringFormat(invariantCulture, "@{0}", objArray);
		}

		protected override string GetParameterName(int parameterOrdinal)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { parameterOrdinal };
			return HelperMethods.StringFormat(invariantCulture, "@param{0}", objArray);
		}

		protected override string GetParameterPlaceholder(int parameterOrdinal)
		{
			return this.GetParameterName(parameterOrdinal);
		}

		protected override DataTable GetSchemaTable(DbCommand sourceCommand)
		{
			DataTable dataTable;
			using (IDataReader dataReader = sourceCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
			{
				DataTable schemaTable = dataReader.GetSchemaTable();
				if (this.HasSchemaPrimaryKey(schemaTable))
				{
					this.ResetIsUniqueSchemaColumn(schemaTable);
				}
				dataTable = schemaTable;
			}
			return dataTable;
		}

		public new SQLiteCommand GetUpdateCommand()
		{
			this.CheckDisposed();
			return (SQLiteCommand)base.GetUpdateCommand();
		}

		public new SQLiteCommand GetUpdateCommand(bool useColumnsForParameterNames)
		{
			this.CheckDisposed();
			return (SQLiteCommand)base.GetUpdateCommand(useColumnsForParameterNames);
		}

		private bool HasSchemaPrimaryKey(DataTable schema)
		{
			bool flag;
			DataColumn item = schema.Columns[SchemaTableColumn.IsKey];
			IEnumerator enumerator = schema.Rows.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (!(bool)((DataRow)enumerator.Current)[item])
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return flag;
		}

		public override string QuoteIdentifier(string unquotedIdentifier)
		{
			this.CheckDisposed();
			if (string.IsNullOrEmpty(this.QuotePrefix) || string.IsNullOrEmpty(this.QuoteSuffix) || string.IsNullOrEmpty(unquotedIdentifier))
			{
				return unquotedIdentifier;
			}
			return string.Concat(this.QuotePrefix, unquotedIdentifier.Replace(this.QuoteSuffix, string.Concat(this.QuoteSuffix, this.QuoteSuffix)), this.QuoteSuffix);
		}

		private void ResetIsUniqueSchemaColumn(DataTable schema)
		{
			DataColumn item = schema.Columns[SchemaTableColumn.IsUnique];
			DataColumn dataColumn = schema.Columns[SchemaTableColumn.IsKey];
			foreach (DataRow row in schema.Rows)
			{
				if ((bool)row[dataColumn])
				{
					continue;
				}
				row[item] = false;
			}
			schema.AcceptChanges();
		}

		private void RowUpdatingEventHandler(object sender, RowUpdatingEventArgs e)
		{
			base.RowUpdatingHandler(e);
		}

		protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
		{
			if (adapter == base.DataAdapter)
			{
				((SQLiteDataAdapter)adapter).RowUpdating -= new EventHandler<RowUpdatingEventArgs>(this.RowUpdatingEventHandler);
				return;
			}
			((SQLiteDataAdapter)adapter).RowUpdating += new EventHandler<RowUpdatingEventArgs>(this.RowUpdatingEventHandler);
		}

		public override string UnquoteIdentifier(string quotedIdentifier)
		{
			this.CheckDisposed();
			if (string.IsNullOrEmpty(this.QuotePrefix) || string.IsNullOrEmpty(this.QuoteSuffix) || string.IsNullOrEmpty(quotedIdentifier))
			{
				return quotedIdentifier;
			}
			if (!quotedIdentifier.StartsWith(this.QuotePrefix, StringComparison.OrdinalIgnoreCase) || !quotedIdentifier.EndsWith(this.QuoteSuffix, StringComparison.OrdinalIgnoreCase))
			{
				return quotedIdentifier;
			}
			return quotedIdentifier.Substring(this.QuotePrefix.Length, quotedIdentifier.Length - (this.QuotePrefix.Length + this.QuoteSuffix.Length)).Replace(string.Concat(this.QuoteSuffix, this.QuoteSuffix), this.QuoteSuffix);
		}
	}
}