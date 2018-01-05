using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteKeyReader : IDisposable
	{
		private SQLiteKeyReader.KeyInfo[] _keyInfo;

		private SQLiteStatement _stmt;

		private bool _isValid;

		private SQLiteKeyReader.RowIdInfo[] _rowIdInfo;

		private bool disposed;

		internal int Count
		{
			get
			{
				if (this._keyInfo == null)
				{
					return 0;
				}
				return (int)this._keyInfo.Length;
			}
		}

		internal SQLiteKeyReader(SQLiteConnection cnn, SQLiteDataReader reader, SQLiteStatement stmt)
		{
			List<string> item;
			Dictionary<string, int> strs = new Dictionary<string, int>();
			Dictionary<string, List<string>> strs1 = new Dictionary<string, List<string>>();
			List<SQLiteKeyReader.KeyInfo> keyInfos = new List<SQLiteKeyReader.KeyInfo>();
			List<SQLiteKeyReader.RowIdInfo> rowIdInfos = new List<SQLiteKeyReader.RowIdInfo>();
			this._stmt = stmt;
			using (DataTable schema = cnn.GetSchema("Catalogs"))
			{
				foreach (DataRow row in schema.Rows)
				{
					strs.Add((string)row["CATALOG_NAME"], Convert.ToInt32(row["ID"], CultureInfo.InvariantCulture));
				}
			}
			using (DataTable schemaTable = reader.GetSchemaTable(false, false))
			{
				foreach (DataRow dataRow in schemaTable.Rows)
				{
					if (dataRow[SchemaTableOptionalColumn.BaseCatalogName] == DBNull.Value)
					{
						continue;
					}
					string str = (string)dataRow[SchemaTableOptionalColumn.BaseCatalogName];
					string item1 = (string)dataRow[SchemaTableColumn.BaseTableName];
					if (strs1.ContainsKey(str))
					{
						item = strs1[str];
					}
					else
					{
						item = new List<string>();
						strs1.Add(str, item);
					}
					if (item.Contains(item1))
					{
						continue;
					}
					item.Add(item1);
				}
				foreach (KeyValuePair<string, List<string>> keyValuePair in strs1)
				{
					for (int i = 0; i < keyValuePair.Value.Count; i++)
					{
						string str1 = keyValuePair.Value[i];
						DataRow dataRow1 = null;
						string[] key = new string[] { keyValuePair.Key, null, str1 };
						using (DataTable dataTable = cnn.GetSchema("Indexes", key))
						{
							for (int j = 0; j < 2 && dataRow1 == null; j++)
							{
								foreach (DataRow row1 in dataTable.Rows)
								{
									if (j != 0 || !(bool)row1["PRIMARY_KEY"])
									{
										if (j != 1 || !(bool)row1["UNIQUE"])
										{
											continue;
										}
										dataRow1 = row1;
										break;
									}
									else
									{
										dataRow1 = row1;
										break;
									}
								}
							}
							if (dataRow1 != null)
							{
								string[] strArrays = new string[] { keyValuePair.Key, null, str1 };
								using (DataTable schema1 = cnn.GetSchema("Tables", strArrays))
								{
									int num = strs[keyValuePair.Key];
									int num1 = Convert.ToInt32(schema1.Rows[0]["TABLE_ROOTPAGE"], CultureInfo.InvariantCulture);
									int cursorForTable = stmt._sql.GetCursorForTable(stmt, num, num1);
									string[] key1 = new string[] { keyValuePair.Key, null, str1, (string)dataRow1["INDEX_NAME"] };
									using (DataTable dataTable1 = cnn.GetSchema("IndexColumns", key1))
									{
										bool flag = (string)dataRow1["INDEX_NAME"] == string.Concat("sqlite_master_PK_", str1);
										SQLiteKeyReader.KeyQuery keyQuery = null;
										List<string> strs2 = new List<string>();
										for (int k = 0; k < dataTable1.Rows.Count; k++)
										{
											string stringOrNull = SQLiteConvert.GetStringOrNull(dataTable1.Rows[k]["COLUMN_NAME"]);
											bool flag1 = true;
											foreach (DataRow row2 in schemaTable.Rows)
											{
												if (row2.IsNull(SchemaTableColumn.BaseColumnName) || !((string)row2[SchemaTableColumn.BaseColumnName] == stringOrNull) || !((string)row2[SchemaTableColumn.BaseTableName] == str1) || !((string)row2[SchemaTableOptionalColumn.BaseCatalogName] == keyValuePair.Key))
												{
													continue;
												}
												if (flag)
												{
													SQLiteKeyReader.RowIdInfo rowIdInfo = new SQLiteKeyReader.RowIdInfo()
													{
														databaseName = keyValuePair.Key,
														tableName = str1,
														column = (int)row2[SchemaTableColumn.ColumnOrdinal]
													};
													rowIdInfos.Add(rowIdInfo);
												}
												dataTable1.Rows.RemoveAt(k);
												k--;
												flag1 = false;
												break;
											}
											if (flag1)
											{
												strs2.Add(stringOrNull);
											}
										}
										if (!flag && strs2.Count > 0)
										{
											string[] strArrays1 = new string[strs2.Count];
											strs2.CopyTo(strArrays1);
											keyQuery = new SQLiteKeyReader.KeyQuery(cnn, keyValuePair.Key, str1, strArrays1);
										}
										for (int l = 0; l < dataTable1.Rows.Count; l++)
										{
											string stringOrNull1 = SQLiteConvert.GetStringOrNull(dataTable1.Rows[l]["COLUMN_NAME"]);
											SQLiteKeyReader.KeyInfo keyInfo = new SQLiteKeyReader.KeyInfo()
											{
												rootPage = num1,
												cursor = cursorForTable,
												database = num,
												databaseName = keyValuePair.Key,
												tableName = str1,
												columnName = stringOrNull1,
												query = keyQuery,
												column = l
											};
											keyInfos.Add(keyInfo);
										}
									}
								}
							}
							else
							{
								keyValuePair.Value.RemoveAt(i);
								i--;
							}
						}
					}
				}
			}
			this._keyInfo = new SQLiteKeyReader.KeyInfo[keyInfos.Count];
			keyInfos.CopyTo(this._keyInfo);
			this._rowIdInfo = new SQLiteKeyReader.RowIdInfo[rowIdInfos.Count];
			rowIdInfos.CopyTo(this._rowIdInfo);
		}

		internal void AppendSchemaTable(DataTable tbl)
		{
			SQLiteKeyReader.KeyQuery keyQuery = null;
			for (int i = 0; i < (int)this._keyInfo.Length; i++)
			{
				if (this._keyInfo[i].query == null || this._keyInfo[i].query != keyQuery)
				{
					keyQuery = this._keyInfo[i].query;
					if (keyQuery != null)
					{
						keyQuery.Sync((long)0);
						using (DataTable schemaTable = keyQuery._reader.GetSchemaTable())
						{
							foreach (DataRow row in schemaTable.Rows)
							{
								object[] itemArray = row.ItemArray;
								DataRow count = tbl.Rows.Add(itemArray);
								count[SchemaTableOptionalColumn.IsHidden] = true;
								count[SchemaTableColumn.ColumnOrdinal] = tbl.Rows.Count - 1;
							}
						}
					}
					else
					{
						DataRow dataRow = tbl.NewRow();
						dataRow[SchemaTableColumn.ColumnName] = this._keyInfo[i].columnName;
						dataRow[SchemaTableColumn.ColumnOrdinal] = tbl.Rows.Count;
						dataRow[SchemaTableColumn.ColumnSize] = 8;
						dataRow[SchemaTableColumn.NumericPrecision] = 255;
						dataRow[SchemaTableColumn.NumericScale] = 255;
						dataRow[SchemaTableColumn.ProviderType] = DbType.Int64;
						dataRow[SchemaTableColumn.IsLong] = false;
						dataRow[SchemaTableColumn.AllowDBNull] = false;
						dataRow[SchemaTableOptionalColumn.IsReadOnly] = false;
						dataRow[SchemaTableOptionalColumn.IsRowVersion] = false;
						dataRow[SchemaTableColumn.IsUnique] = false;
						dataRow[SchemaTableColumn.IsKey] = true;
						dataRow[SchemaTableColumn.DataType] = typeof(long);
						dataRow[SchemaTableOptionalColumn.IsHidden] = true;
						dataRow[SchemaTableColumn.BaseColumnName] = this._keyInfo[i].columnName;
						dataRow[SchemaTableColumn.IsExpression] = false;
						dataRow[SchemaTableColumn.IsAliased] = false;
						dataRow[SchemaTableColumn.BaseTableName] = this._keyInfo[i].tableName;
						dataRow[SchemaTableOptionalColumn.BaseCatalogName] = this._keyInfo[i].databaseName;
						dataRow[SchemaTableOptionalColumn.IsAutoIncrement] = true;
						dataRow["DataTypeName"] = "integer";
						tbl.Rows.Add(dataRow);
					}
				}
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteKeyReader).Name);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this._stmt = null;
					if (this._keyInfo != null)
					{
						for (int i = 0; i < (int)this._keyInfo.Length; i++)
						{
							if (this._keyInfo[i].query != null)
							{
								this._keyInfo[i].query.Dispose();
							}
						}
						this._keyInfo = null;
					}
				}
				this.disposed = true;
			}
		}

		~SQLiteKeyReader()
		{
			this.Dispose(false);
		}

		internal SQLiteBlob GetBlob(int i, bool readOnly)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetBlob(this._keyInfo[i].column, readOnly);
		}

		internal bool GetBoolean(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetBoolean(this._keyInfo[i].column);
		}

		internal byte GetByte(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetByte(this._keyInfo[i].column);
		}

		internal long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetBytes(this._keyInfo[i].column, fieldOffset, buffer, bufferoffset, length);
		}

		internal char GetChar(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetChar(this._keyInfo[i].column);
		}

		internal long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetChars(this._keyInfo[i].column, fieldOffset, buffer, bufferoffset, length);
		}

		internal string GetDataTypeName(int i)
		{
			this.Sync();
			if (this._keyInfo[i].query == null)
			{
				return "integer";
			}
			return this._keyInfo[i].query._reader.GetDataTypeName(this._keyInfo[i].column);
		}

		internal DateTime GetDateTime(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetDateTime(this._keyInfo[i].column);
		}

		internal decimal GetDecimal(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetDecimal(this._keyInfo[i].column);
		}

		internal double GetDouble(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetDouble(this._keyInfo[i].column);
		}

		internal Type GetFieldType(int i)
		{
			this.Sync();
			if (this._keyInfo[i].query == null)
			{
				return typeof(long);
			}
			return this._keyInfo[i].query._reader.GetFieldType(this._keyInfo[i].column);
		}

		internal float GetFloat(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetFloat(this._keyInfo[i].column);
		}

		internal Guid GetGuid(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetGuid(this._keyInfo[i].column);
		}

		internal short GetInt16(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query != null)
			{
				return this._keyInfo[i].query._reader.GetInt16(this._keyInfo[i].column);
			}
			long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
			if (rowIdForCursor == (long)0)
			{
				throw new InvalidCastException();
			}
			return Convert.ToInt16(rowIdForCursor);
		}

		internal int GetInt32(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query != null)
			{
				return this._keyInfo[i].query._reader.GetInt32(this._keyInfo[i].column);
			}
			long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
			if (rowIdForCursor == (long)0)
			{
				throw new InvalidCastException();
			}
			return Convert.ToInt32(rowIdForCursor);
		}

		internal long GetInt64(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query != null)
			{
				return this._keyInfo[i].query._reader.GetInt64(this._keyInfo[i].column);
			}
			long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
			if (rowIdForCursor == (long)0)
			{
				throw new InvalidCastException();
			}
			return rowIdForCursor;
		}

		internal string GetName(int i)
		{
			return this._keyInfo[i].columnName;
		}

		internal int GetOrdinal(string name)
		{
			for (int i = 0; i < (int)this._keyInfo.Length; i++)
			{
				if (string.Compare(name, this._keyInfo[i].columnName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		internal long? GetRowId(string databaseName, string tableName)
		{
			if (this._keyInfo != null && databaseName != null && tableName != null)
			{
				for (int i = 0; i < (int)this._keyInfo.Length; i++)
				{
					if (this._keyInfo[i].databaseName == databaseName && this._keyInfo[i].tableName == tableName)
					{
						long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
						if (rowIdForCursor != (long)0)
						{
							return new long?(rowIdForCursor);
						}
					}
				}
			}
			return null;
		}

		internal int GetRowIdIndex(string databaseName, string tableName)
		{
			if (this._rowIdInfo != null && databaseName != null && tableName != null)
			{
				for (int i = 0; i < (int)this._rowIdInfo.Length; i++)
				{
					if (this._rowIdInfo[i].databaseName == databaseName && this._rowIdInfo[i].tableName == tableName)
					{
						return this._rowIdInfo[i].column;
					}
				}
			}
			return -1;
		}

		internal string GetString(int i)
		{
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				throw new InvalidCastException();
			}
			return this._keyInfo[i].query._reader.GetString(this._keyInfo[i].column);
		}

		internal object GetValue(int i)
		{
			if (this._keyInfo[i].cursor == -1)
			{
				return DBNull.Value;
			}
			this.Sync(i);
			if (this._keyInfo[i].query == null)
			{
				if (this.IsDBNull(i))
				{
					return DBNull.Value;
				}
				return this.GetInt64(i);
			}
			return this._keyInfo[i].query._reader.GetValue(this._keyInfo[i].column);
		}

		internal bool IsDBNull(int i)
		{
			if (this._keyInfo[i].cursor == -1)
			{
				return true;
			}
			this.Sync(i);
			if (this._keyInfo[i].query != null)
			{
				return this._keyInfo[i].query._reader.IsDBNull(this._keyInfo[i].column);
			}
			return this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor) == (long)0;
		}

		internal void Reset()
		{
			this._isValid = false;
			if (this._keyInfo == null)
			{
				return;
			}
			for (int i = 0; i < (int)this._keyInfo.Length; i++)
			{
				if (this._keyInfo[i].query != null)
				{
					this._keyInfo[i].query.IsValid = false;
				}
			}
		}

		private void Sync(int i)
		{
			this.Sync();
			if (this._keyInfo[i].cursor == -1)
			{
				throw new InvalidCastException();
			}
		}

		private void Sync()
		{
			if (this._isValid)
			{
				return;
			}
			SQLiteKeyReader.KeyQuery keyQuery = null;
			for (int i = 0; i < (int)this._keyInfo.Length; i++)
			{
				if (this._keyInfo[i].query == null || this._keyInfo[i].query != keyQuery)
				{
					keyQuery = this._keyInfo[i].query;
					if (keyQuery != null)
					{
						keyQuery.Sync(this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor));
					}
				}
			}
			this._isValid = true;
		}

		private struct KeyInfo
		{
			internal string databaseName;

			internal string tableName;

			internal string columnName;

			internal int database;

			internal int rootPage;

			internal int cursor;

			internal SQLiteKeyReader.KeyQuery query;

			internal int column;
		}

		private sealed class KeyQuery : IDisposable
		{
			private SQLiteCommand _command;

			internal SQLiteDataReader _reader;

			private bool disposed;

			internal bool IsValid
			{
				set
				{
					if (value)
					{
						throw new ArgumentException();
					}
					if (this._reader != null)
					{
						this._reader.Dispose();
						this._reader = null;
					}
				}
			}

			internal KeyQuery(SQLiteConnection cnn, string database, string table, params string[] columns)
			{
				using (SQLiteCommandBuilder sQLiteCommandBuilder = new SQLiteCommandBuilder())
				{
					this._command = cnn.CreateCommand();
					for (int i = 0; i < (int)columns.Length; i++)
					{
						columns[i] = sQLiteCommandBuilder.QuoteIdentifier(columns[i]);
					}
				}
				SQLiteCommand sQLiteCommand = this._command;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { string.Join(",", columns), database, table };
				sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture, "SELECT {0} FROM [{1}].[{2}] WHERE ROWID = ?", objArray);
				this._command.Parameters.AddWithValue(null, (long)0);
			}

			private void CheckDisposed()
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException(typeof(SQLiteKeyReader.KeyQuery).Name);
				}
			}

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if (!this.disposed)
				{
					if (disposing)
					{
						this.IsValid = false;
						if (this._command != null)
						{
							this._command.Dispose();
						}
						this._command = null;
					}
					this.disposed = true;
				}
			}

			~KeyQuery()
			{
				this.Dispose(false);
			}

			internal void Sync(long rowid)
			{
				this.IsValid = false;
				this._command.Parameters[0].Value = rowid;
				this._reader = this._command.ExecuteReader();
				this._reader.Read();
			}
		}

		private struct RowIdInfo
		{
			internal string databaseName;

			internal string tableName;

			internal int column;
		}
	}
}