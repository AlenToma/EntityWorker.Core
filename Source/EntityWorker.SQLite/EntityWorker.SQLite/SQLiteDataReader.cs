using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.SQLite
{
	public sealed class SQLiteDataReader : DbDataReader
	{
		private SQLiteCommand _command;

		private SQLiteConnectionFlags _flags;

		private int _activeStatementIndex;

		private SQLiteStatement _activeStatement;

		private int _readingState;

		private int _rowsAffected;

		private int _fieldCount;

		private int _stepCount;

		private Dictionary<string, int> _fieldIndexes;

		private SQLiteType[] _fieldTypeArray;

		private CommandBehavior _commandBehavior;

		internal bool _disposeCommand;

		internal bool _throwOnDisposed;

		private SQLiteKeyReader _keyInfo;

		internal int _version;

		private string _baseSchemaName;

		private bool disposed;

		public override int Depth
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				return 0;
			}
		}

		public override int FieldCount
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				if (this._keyInfo == null)
				{
					return this._fieldCount;
				}
				return this._fieldCount + this._keyInfo.Count;
			}
		}

		public override bool HasRows
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				if ((this._flags & SQLiteConnectionFlags.StickyHasRows) != SQLiteConnectionFlags.StickyHasRows)
				{
					return this._readingState != 1;
				}
				if (this._readingState != 1)
				{
					return true;
				}
				return this._stepCount > 0;
			}
		}

		public override bool IsClosed
		{
			get
			{
				this.CheckDisposed();
				return this._command == null;
			}
		}

		public override object this[string name]
		{
			get
			{
				this.CheckDisposed();
				return this.GetValue(this.GetOrdinal(name));
			}
		}

		public override object this[int i]
		{
			get
			{
				this.CheckDisposed();
				return this.GetValue(i);
			}
		}

		private int PrivateVisibleFieldCount
		{
			get
			{
				return this._fieldCount;
			}
		}

		public override int RecordsAffected
		{
			get
			{
				this.CheckDisposed();
				return this._rowsAffected;
			}
		}

		public int StepCount
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				return this._stepCount;
			}
		}

		public override int VisibleFieldCount
		{
			get
			{
				this.CheckDisposed();
				this.CheckClosed();
				return this.PrivateVisibleFieldCount;
			}
		}

		internal SQLiteDataReader(SQLiteCommand cmd, CommandBehavior behave)
		{
			this._throwOnDisposed = true;
			this._command = cmd;
			this._version = this._command.Connection._version;
			this._baseSchemaName = this._command.Connection._baseSchemaName;
			this._commandBehavior = behave;
			this._activeStatementIndex = -1;
			this._rowsAffected = -1;
			this.RefreshFlags();
			SQLiteConnection connection = SQLiteDataReader.GetConnection(this);
			SQLiteCommand sQLiteCommand = this._command;
			object[] objArray = new object[] { behave };
			SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(SQLiteConnectionEventType.NewDataReader, null, null, sQLiteCommand, this, null, null, objArray));
			if (this._command != null)
			{
				this.NextResult();
			}
		}

		internal void Cancel()
		{
			this._version = 0;
		}

		private void CheckClosed()
		{
			if (!this._throwOnDisposed)
			{
				return;
			}
			if (this._command == null)
			{
				throw new InvalidOperationException("DataReader has been closed");
			}
			if (this._version == 0)
			{
				throw new SQLiteException("Execution was aborted by the user");
			}
			SQLiteConnection connection = this._command.Connection;
			if (connection._version != this._version || connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection was closed, statement was terminated");
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed && this._throwOnDisposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteDataReader).Name);
			}
		}

		private void CheckValidRow()
		{
			if (this._readingState != 0)
			{
				throw new InvalidOperationException("No current row");
			}
		}

		public override void Close()
		{
			this.CheckDisposed();
			SQLiteConnection connection = SQLiteDataReader.GetConnection(this);
			SQLiteCommand sQLiteCommand = this._command;
			object[] objArray = new object[] { this._commandBehavior, this._readingState, this._rowsAffected, this._stepCount, this._fieldCount, this._disposeCommand, this._throwOnDisposed };
			SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(SQLiteConnectionEventType.ClosingDataReader, null, null, sQLiteCommand, this, null, null, objArray));
			try
			{
				if (this._command != null)
				{
					try
					{
						try
						{
							if (this._version != 0)
							{
								try
								{
									while (this.NextResult())
									{
									}
								}
								catch (SQLiteException sQLiteException)
								{
								}
							}
							this._command.ResetDataReader();
						}
						finally
						{
							if ((this._commandBehavior & CommandBehavior.CloseConnection) != CommandBehavior.Default && this._command.Connection != null)
							{
								this._command.Connection.Close();
							}
						}
					}
					finally
					{
						if (this._disposeCommand)
						{
							this._command.Dispose();
						}
					}
				}
				this._command = null;
				this._activeStatement = null;
				this._fieldIndexes = null;
				this._fieldTypeArray = null;
			}
			finally
			{
				if (this._keyInfo != null)
				{
					this._keyInfo.Dispose();
					this._keyInfo = null;
				}
			}
		}

		private static int CountParents(Dictionary<SQLiteDataReader.ColumnParent, List<int>> parentToColumns)
		{
			int num = 0;
			if (parentToColumns != null)
			{
				foreach (SQLiteDataReader.ColumnParent key in parentToColumns.Keys)
				{
					if (key == null || string.IsNullOrEmpty(key.TableName))
					{
						continue;
					}
					num++;
				}
			}
			return num;
		}

		protected override void Dispose(bool disposing)
		{
			SQLiteConnection connection = SQLiteDataReader.GetConnection(this);
			SQLiteCommand sQLiteCommand = this._command;
			object[] objArray = new object[] { disposing, this.disposed, this._commandBehavior, this._readingState, this._rowsAffected, this._stepCount, this._fieldCount, this._disposeCommand, this._throwOnDisposed };
			SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(SQLiteConnectionEventType.DisposingDataReader, null, null, sQLiteCommand, this, null, null, objArray));
			try
			{
				if (!this.disposed)
				{
					this._throwOnDisposed = false;
				}
			}
			finally
			{
				base.Dispose(disposing);
				this.disposed = true;
			}
		}

		public SQLiteBlob GetBlob(int i, bool readOnly)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetBlob", new SQLiteReadBlobEventArgs(readOnly), sQLiteDataReaderValue), out flag);
				if (flag)
				{
					return sQLiteDataReaderValue.BlobValue;
				}
			}
			if (i < this.PrivateVisibleFieldCount || this._keyInfo == null)
			{
				return SQLiteBlob.Create(this, i, readOnly);
			}
			return this._keyInfo.GetBlob(i - this.PrivateVisibleFieldCount, readOnly);
		}

		public override bool GetBoolean(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetBoolean", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.BooleanValue.HasValue)
					{
						throw new SQLiteException("missing boolean return value");
					}
					return sQLiteDataReaderValue.BooleanValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetBoolean(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Boolean);
			return Convert.ToBoolean(this.GetValue(i), CultureInfo.CurrentCulture);
		}

		public override byte GetByte(int i)
		{
			bool flag;
			int? nullable;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetByte", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					byte? byteValue = sQLiteDataReaderValue.ByteValue;
					if (byteValue.HasValue)
					{
						nullable = new int?((int)byteValue.GetValueOrDefault());
					}
					else
					{
						nullable = null;
					}
					if (!nullable.HasValue)
					{
						throw new SQLiteException("missing byte return value");
					}
					return sQLiteDataReaderValue.ByteValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetByte(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Byte);
			return this._activeStatement._sql.GetByte(this._activeStatement, i);
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteReadArrayEventArgs sQLiteReadArrayEventArg = new SQLiteReadArrayEventArgs(fieldOffset, buffer, bufferoffset, length);
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetBytes", sQLiteReadArrayEventArg, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					byte[] bytesValue = sQLiteDataReaderValue.BytesValue;
					if (bytesValue == null)
					{
						return (long)-1;
					}
					Array.Copy(bytesValue, sQLiteReadArrayEventArg.DataOffset, sQLiteReadArrayEventArg.ByteBuffer, (long)sQLiteReadArrayEventArg.BufferOffset, (long)sQLiteReadArrayEventArg.Length);
					return (long)sQLiteReadArrayEventArg.Length;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetBytes(i - this.PrivateVisibleFieldCount, fieldOffset, buffer, bufferoffset, length);
			}
			this.VerifyType(i, DbType.Binary);
			return this._activeStatement._sql.GetBytes(this._activeStatement, i, (int)fieldOffset, buffer, bufferoffset, length);
		}

		public override char GetChar(int i)
		{
			bool flag;
			int? nullable;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetChar", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					char? charValue = sQLiteDataReaderValue.CharValue;
					if (charValue.HasValue)
					{
						nullable = new int?(charValue.GetValueOrDefault());
					}
					else
					{
						nullable = null;
					}
					if (!nullable.HasValue)
					{
						throw new SQLiteException("missing character return value");
					}
					return sQLiteDataReaderValue.CharValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetChar(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.SByte);
			return this._activeStatement._sql.GetChar(this._activeStatement, i);
		}

		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteReadArrayEventArgs sQLiteReadArrayEventArg = new SQLiteReadArrayEventArgs(fieldoffset, buffer, bufferoffset, length);
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetChars", sQLiteReadArrayEventArg, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					char[] charsValue = sQLiteDataReaderValue.CharsValue;
					if (charsValue == null)
					{
						return (long)-1;
					}
					Array.Copy(charsValue, sQLiteReadArrayEventArg.DataOffset, sQLiteReadArrayEventArg.CharBuffer, (long)sQLiteReadArrayEventArg.BufferOffset, (long)sQLiteReadArrayEventArg.Length);
					return (long)sQLiteReadArrayEventArg.Length;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetChars(i - this.PrivateVisibleFieldCount, fieldoffset, buffer, bufferoffset, length);
			}
			if ((this._flags & SQLiteConnectionFlags.NoVerifyTextAffinity) != SQLiteConnectionFlags.NoVerifyTextAffinity)
			{
				this.VerifyType(i, DbType.String);
			}
			return this._activeStatement._sql.GetChars(this._activeStatement, i, (int)fieldoffset, buffer, bufferoffset, length);
		}

		internal static SQLiteConnection GetConnection(SQLiteDataReader dataReader)
		{
			try
			{
				if (dataReader != null)
				{
					SQLiteCommand sQLiteCommand = dataReader._command;
					if (sQLiteCommand != null)
					{
						SQLiteConnection connection = sQLiteCommand.Connection;
						if (connection != null)
						{
							return connection;
						}
					}
				}
			}
			catch (ObjectDisposedException objectDisposedException)
			{
			}
			return null;
		}

		public string GetDatabaseName(int i)
		{
			this.CheckDisposed();
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetName(i - this.PrivateVisibleFieldCount);
			}
			return this._activeStatement._sql.ColumnDatabaseName(this._activeStatement, i);
		}

		public override string GetDataTypeName(int i)
		{
			this.CheckDisposed();
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetDataTypeName(i - this.PrivateVisibleFieldCount);
			}
			TypeAffinity typeAffinity = TypeAffinity.Uninitialized;
			return this._activeStatement._sql.ColumnType(this._activeStatement, i, ref typeAffinity);
		}

		public override DateTime GetDateTime(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetDateTime", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.DateTimeValue.HasValue)
					{
						throw new SQLiteException("missing date/time return value");
					}
					return sQLiteDataReaderValue.DateTimeValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetDateTime(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.DateTime);
			return this._activeStatement._sql.GetDateTime(this._activeStatement, i);
		}

		public override decimal GetDecimal(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetDecimal", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.DecimalValue.HasValue)
					{
						throw new SQLiteException("missing decimal return value");
					}
					return sQLiteDataReaderValue.DecimalValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetDecimal(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Decimal);
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			if ((this._flags & SQLiteConnectionFlags.GetInvariantDecimal) == SQLiteConnectionFlags.GetInvariantDecimal)
			{
				currentCulture = CultureInfo.InvariantCulture;
			}
			return decimal.Parse(this._activeStatement._sql.GetText(this._activeStatement, i), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, currentCulture);
		}

		public override double GetDouble(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetDouble", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.DoubleValue.HasValue)
					{
						throw new SQLiteException("missing double return value");
					}
					return (double)sQLiteDataReaderValue.DoubleValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetDouble(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Double);
			return this._activeStatement._sql.GetDouble(this._activeStatement, i);
		}

		public override IEnumerator GetEnumerator()
		{
			this.CheckDisposed();
			return new DbEnumerator(this, (this._commandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection);
		}

		public override Type GetFieldType(int i)
		{
			this.CheckDisposed();
			if (i < this.PrivateVisibleFieldCount || this._keyInfo == null)
			{
				return SQLiteConvert.SQLiteTypeToType(this.GetSQLiteType(this._flags, i));
			}
			return this._keyInfo.GetFieldType(i - this.PrivateVisibleFieldCount);
		}

		public override float GetFloat(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetFloat", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.FloatValue.HasValue)
					{
						throw new SQLiteException("missing float return value");
					}
					return (float)sQLiteDataReaderValue.FloatValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetFloat(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Single);
			return Convert.ToSingle(this._activeStatement._sql.GetDouble(this._activeStatement, i));
		}

		public override Guid GetGuid(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetGuid", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.GuidValue.HasValue)
					{
						throw new SQLiteException("missing guid return value");
					}
					return sQLiteDataReaderValue.GuidValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetGuid(i - this.PrivateVisibleFieldCount);
			}
			if (this.VerifyType(i, DbType.Guid) != TypeAffinity.Blob)
			{
				return new Guid(this._activeStatement._sql.GetText(this._activeStatement, i));
			}
			byte[] numArray = new byte[16];
			this._activeStatement._sql.GetBytes(this._activeStatement, i, 0, numArray, 0, 16);
			return new Guid(numArray);
		}

		public override short GetInt16(int i)
		{
			bool flag;
			int? nullable;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetInt16", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					short? int16Value = sQLiteDataReaderValue.Int16Value;
					if (int16Value.HasValue)
					{
						nullable = new int?(int16Value.GetValueOrDefault());
					}
					else
					{
						nullable = null;
					}
					if (!nullable.HasValue)
					{
						throw new SQLiteException("missing int16 return value");
					}
					return sQLiteDataReaderValue.Int16Value.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetInt16(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Int16);
			return this._activeStatement._sql.GetInt16(this._activeStatement, i);
		}

		public override int GetInt32(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetInt32", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.Int32Value.HasValue)
					{
						throw new SQLiteException("missing int32 return value");
					}
					return sQLiteDataReaderValue.Int32Value.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetInt32(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Int32);
			return this._activeStatement._sql.GetInt32(this._activeStatement, i);
		}

		public override long GetInt64(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetInt64", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					if (!sQLiteDataReaderValue.Int64Value.HasValue)
					{
						throw new SQLiteException("missing int64 return value");
					}
					return sQLiteDataReaderValue.Int64Value.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetInt64(i - this.PrivateVisibleFieldCount);
			}
			this.VerifyType(i, DbType.Int64);
			return this._activeStatement._sql.GetInt64(this._activeStatement, i);
		}

		public override string GetName(int i)
		{
			this.CheckDisposed();
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetName(i - this.PrivateVisibleFieldCount);
			}
			return this._activeStatement._sql.ColumnName(this._activeStatement, i);
		}

		public override int GetOrdinal(string name)
		{
			int ordinal;
			this.CheckDisposed();
			bool flag = this._throwOnDisposed;
			if (this._fieldIndexes == null)
			{
				this._fieldIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			}
			if (!this._fieldIndexes.TryGetValue(name, out ordinal))
			{
				ordinal = this._activeStatement._sql.ColumnIndex(this._activeStatement, name);
				if (ordinal == -1 && this._keyInfo != null)
				{
					ordinal = this._keyInfo.GetOrdinal(name);
					if (ordinal > -1)
					{
						ordinal += this.PrivateVisibleFieldCount;
					}
				}
				this._fieldIndexes.Add(name, ordinal);
			}
			return ordinal;
		}

		public string GetOriginalName(int i)
		{
			this.CheckDisposed();
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetName(i - this.PrivateVisibleFieldCount);
			}
			return this._activeStatement._sql.ColumnOriginalName(this._activeStatement, i);
		}

		internal long? GetRowId(int i)
		{
			this.VerifyForGet();
			if (this._keyInfo == null)
			{
				return null;
			}
			string databaseName = this.GetDatabaseName(i);
			string tableName = this.GetTableName(i);
			int rowIdIndex = this._keyInfo.GetRowIdIndex(databaseName, tableName);
			if (rowIdIndex != -1)
			{
				return new long?(this.GetInt64(rowIdIndex));
			}
			return this._keyInfo.GetRowId(databaseName, tableName);
		}

		public override DataTable GetSchemaTable()
		{
			this.CheckDisposed();
			return this.GetSchemaTable(true, false);
		}

		internal DataTable GetSchemaTable(bool wantUniqueInfo, bool wantDefaultValue)
		{
			this.CheckClosed();
			bool flag = this._throwOnDisposed;
			Dictionary<SQLiteDataReader.ColumnParent, List<int>> columnParents = null;
			Dictionary<int, SQLiteDataReader.ColumnParent> nums = null;
			SQLiteDataReader.GetStatementColumnParents(this._command.Connection._sql, this._activeStatement, this._fieldCount, ref columnParents, ref nums);
			DataTable dataTable = new DataTable("SchemaTable");
			DataTable schema = null;
			string empty = string.Empty;
			string item = string.Empty;
			string columnName = string.Empty;
			dataTable.Locale = CultureInfo.InvariantCulture;
			dataTable.Columns.Add(SchemaTableColumn.ColumnName, typeof(string));
			dataTable.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
			dataTable.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
			dataTable.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(int));
			dataTable.Columns.Add(SchemaTableColumn.NumericScale, typeof(int));
			dataTable.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool));
			dataTable.Columns.Add(SchemaTableColumn.IsKey, typeof(bool));
			dataTable.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
			dataTable.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
			dataTable.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string));
			dataTable.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string));
			dataTable.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string));
			dataTable.Columns.Add(SchemaTableColumn.DataType, typeof(Type));
			dataTable.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool));
			dataTable.Columns.Add(SchemaTableColumn.ProviderType, typeof(int));
			dataTable.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool));
			dataTable.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool));
			dataTable.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
			dataTable.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
			dataTable.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool));
			dataTable.Columns.Add(SchemaTableColumn.IsLong, typeof(bool));
			dataTable.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
			dataTable.Columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
			dataTable.Columns.Add(SchemaTableOptionalColumn.DefaultValue, typeof(object));
			dataTable.Columns.Add("DataTypeName", typeof(string));
			dataTable.Columns.Add("CollationType", typeof(string));
			dataTable.BeginLoadData();
			for (int i = 0; i < this._fieldCount; i++)
			{
				SQLiteType sQLiteType = this.GetSQLiteType(this._flags, i);
				DataRow name = dataTable.NewRow();
				DbType type = sQLiteType.Type;
				name[SchemaTableColumn.ColumnName] = this.GetName(i);
				name[SchemaTableColumn.ColumnOrdinal] = i;
				name[SchemaTableColumn.ColumnSize] = SQLiteConvert.DbTypeToColumnSize(type);
				name[SchemaTableColumn.NumericPrecision] = SQLiteConvert.DbTypeToNumericPrecision(type);
				name[SchemaTableColumn.NumericScale] = SQLiteConvert.DbTypeToNumericScale(type);
				name[SchemaTableColumn.ProviderType] = sQLiteType.Type;
				name[SchemaTableColumn.IsLong] = false;
				name[SchemaTableColumn.AllowDBNull] = true;
				name[SchemaTableOptionalColumn.IsReadOnly] = false;
				name[SchemaTableOptionalColumn.IsRowVersion] = false;
				name[SchemaTableColumn.IsUnique] = false;
				name[SchemaTableColumn.IsKey] = false;
				name[SchemaTableOptionalColumn.IsAutoIncrement] = false;
				name[SchemaTableColumn.DataType] = this.GetFieldType(i);
				name[SchemaTableOptionalColumn.IsHidden] = false;
				name[SchemaTableColumn.BaseSchemaName] = this._baseSchemaName;
				columnName = nums[i].ColumnName;
				if (!string.IsNullOrEmpty(columnName))
				{
					name[SchemaTableColumn.BaseColumnName] = columnName;
				}
				name[SchemaTableColumn.IsExpression] = string.IsNullOrEmpty(columnName);
				name[SchemaTableColumn.IsAliased] = string.Compare(this.GetName(i), columnName, StringComparison.OrdinalIgnoreCase) != 0;
				string tableName = nums[i].TableName;
				if (!string.IsNullOrEmpty(tableName))
				{
					name[SchemaTableColumn.BaseTableName] = tableName;
				}
				tableName = nums[i].DatabaseName;
				if (!string.IsNullOrEmpty(tableName))
				{
					name[SchemaTableOptionalColumn.BaseCatalogName] = tableName;
				}
				string str = null;
				if (!string.IsNullOrEmpty(columnName))
				{
					string str1 = null;
					bool flag1 = false;
					bool flag2 = false;
					bool flag3 = false;
					this._command.Connection._sql.ColumnMetaData((string)name[SchemaTableOptionalColumn.BaseCatalogName], (string)name[SchemaTableColumn.BaseTableName], columnName, ref str, ref str1, ref flag1, ref flag2, ref flag3);
					if (flag1 || flag2)
					{
						name[SchemaTableColumn.AllowDBNull] = false;
					}
					name[SchemaTableColumn.IsKey] = (!flag2 ? false : SQLiteDataReader.CountParents(columnParents) <= 1);
					name[SchemaTableOptionalColumn.IsAutoIncrement] = flag3;
					name["CollationType"] = str1;
					string[] strArrays = str.Split(new char[] { '(' });
					if ((int)strArrays.Length > 1)
					{
						str = strArrays[0];
						strArrays = strArrays[1].Split(new char[] { ')' });
						if ((int)strArrays.Length > 1)
						{
							string str2 = strArrays[0];
							char[] chrArray = new char[] { ',', '.' };
							strArrays = str2.Split(chrArray);
							if (sQLiteType.Type == DbType.Binary || SQLiteConvert.IsStringDbType(sQLiteType.Type))
							{
								name[SchemaTableColumn.ColumnSize] = Convert.ToInt32(strArrays[0], CultureInfo.InvariantCulture);
							}
							else
							{
								name[SchemaTableColumn.NumericPrecision] = Convert.ToInt32(strArrays[0], CultureInfo.InvariantCulture);
								if ((int)strArrays.Length > 1)
								{
									name[SchemaTableColumn.NumericScale] = Convert.ToInt32(strArrays[1], CultureInfo.InvariantCulture);
								}
							}
						}
					}
					if (wantDefaultValue)
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						object[] objArray = new object[] { name[SchemaTableOptionalColumn.BaseCatalogName], name[SchemaTableColumn.BaseTableName] };
						using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "PRAGMA [{0}].TABLE_INFO([{1}])", objArray), this._command.Connection))
						{
							using (DbDataReader dbDataReaders = sQLiteCommand.ExecuteReader())
							{
								while (dbDataReaders.Read())
								{
									if (string.Compare((string)name[SchemaTableColumn.BaseColumnName], dbDataReaders.GetString(1), StringComparison.OrdinalIgnoreCase) != 0)
									{
										continue;
									}
									if (dbDataReaders.IsDBNull(4))
									{
										break;
									}
									name[SchemaTableOptionalColumn.DefaultValue] = dbDataReaders[4];
									break;
								}
							}
						}
					}
					if (wantUniqueInfo)
					{
						if ((string)name[SchemaTableOptionalColumn.BaseCatalogName] != empty || (string)name[SchemaTableColumn.BaseTableName] != item)
						{
							empty = (string)name[SchemaTableOptionalColumn.BaseCatalogName];
							item = (string)name[SchemaTableColumn.BaseTableName];
							SQLiteConnection connection = this._command.Connection;
							string[] item1 = new string[] { (string)name[SchemaTableOptionalColumn.BaseCatalogName], null, (string)name[SchemaTableColumn.BaseTableName], null };
							schema = connection.GetSchema("Indexes", item1);
						}
						foreach (DataRow row in schema.Rows)
						{
							SQLiteConnection sQLiteConnection = this._command.Connection;
							string[] strArrays1 = new string[] { (string)name[SchemaTableOptionalColumn.BaseCatalogName], null, (string)name[SchemaTableColumn.BaseTableName], (string)row["INDEX_NAME"], null };
							DataTable schema1 = sQLiteConnection.GetSchema("IndexColumns", strArrays1);
							foreach (DataRow dataRow in schema1.Rows)
							{
								if (string.Compare(SQLiteConvert.GetStringOrNull(dataRow["COLUMN_NAME"]), columnName, StringComparison.OrdinalIgnoreCase) != 0)
								{
									continue;
								}
								if (columnParents.Count == 1 && schema1.Rows.Count == 1 && !(bool)name[SchemaTableColumn.AllowDBNull])
								{
									name[SchemaTableColumn.IsUnique] = row["UNIQUE"];
								}
								if (schema1.Rows.Count != 1 || !(bool)row["PRIMARY_KEY"] || string.IsNullOrEmpty(str) || string.Compare(str, "integer", StringComparison.OrdinalIgnoreCase) != 0)
								{
									break;
								}
								break;
							}
						}
					}
					if (string.IsNullOrEmpty(str))
					{
						TypeAffinity typeAffinity = TypeAffinity.Uninitialized;
						str = this._activeStatement._sql.ColumnType(this._activeStatement, i, ref typeAffinity);
					}
					if (!string.IsNullOrEmpty(str))
					{
						name["DataTypeName"] = str;
					}
				}
				dataTable.Rows.Add(name);
			}
			if (this._keyInfo != null)
			{
				this._keyInfo.AppendSchemaTable(dataTable);
			}
			dataTable.AcceptChanges();
			dataTable.EndLoadData();
			return dataTable;
		}

		private SQLiteType GetSQLiteType(SQLiteType oldType, string text)
		{
			if (SQLiteConvert.LooksLikeNull(text))
			{
				return new SQLiteType(TypeAffinity.Null, DbType.Object);
			}
			if (SQLiteConvert.LooksLikeInt64(text))
			{
				return new SQLiteType(TypeAffinity.Int64, DbType.Int64);
			}
			if (SQLiteConvert.LooksLikeDouble(text))
			{
				return new SQLiteType(TypeAffinity.Double, DbType.Double);
			}
			if (this._activeStatement == null || !SQLiteConvert.LooksLikeDateTime(this._activeStatement._sql, text))
			{
				return oldType;
			}
			return new SQLiteType(TypeAffinity.DateTime, DbType.DateTime);
		}

		private SQLiteType GetSQLiteType(SQLiteConnectionFlags flags, int i)
		{
			SQLiteType dbType = this._fieldTypeArray[i];
			if (dbType == null)
			{
				SQLiteType[] sQLiteTypeArray = this._fieldTypeArray;
				SQLiteType sQLiteType = new SQLiteType();
				SQLiteType sQLiteType1 = sQLiteType;
				sQLiteTypeArray[i] = sQLiteType;
				dbType = sQLiteType1;
			}
			if (dbType.Affinity != TypeAffinity.Uninitialized)
			{
				dbType.Affinity = this._activeStatement._sql.ColumnAffinity(this._activeStatement, i);
			}
			else
			{
				dbType.Type = SQLiteConvert.TypeNameToDbType(SQLiteDataReader.GetConnection(this), this._activeStatement._sql.ColumnType(this._activeStatement, i, ref dbType.Affinity), flags);
			}
			return dbType;
		}

		private static void GetStatementColumnParents(SQLiteBase sql, SQLiteStatement stmt, int fieldCount, ref Dictionary<SQLiteDataReader.ColumnParent, List<int>> parentToColumns, ref Dictionary<int, SQLiteDataReader.ColumnParent> columnToParent)
		{
			List<int> nums;
			if (parentToColumns == null)
			{
				parentToColumns = new Dictionary<SQLiteDataReader.ColumnParent, List<int>>(new SQLiteDataReader.ColumnParent());
			}
			if (columnToParent == null)
			{
				columnToParent = new Dictionary<int, SQLiteDataReader.ColumnParent>();
			}
			for (int i = 0; i < fieldCount; i++)
			{
				string str = sql.ColumnDatabaseName(stmt, i);
				string str1 = sql.ColumnTableName(stmt, i);
				string str2 = sql.ColumnOriginalName(stmt, i);
				SQLiteDataReader.ColumnParent columnParent = new SQLiteDataReader.ColumnParent(str, str1, null);
				SQLiteDataReader.ColumnParent columnParent1 = new SQLiteDataReader.ColumnParent(str, str1, str2);
				if (!parentToColumns.TryGetValue(columnParent, out nums))
				{
					int[] numArray = new int[] { i };
					parentToColumns.Add(columnParent, new List<int>(numArray));
				}
				else if (nums == null)
				{
					int[] numArray1 = new int[] { i };
					parentToColumns[columnParent] = new List<int>(numArray1);
				}
				else
				{
					nums.Add(i);
				}
				columnToParent.Add(i, columnParent1);
			}
		}

		public override string GetString(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetString", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					return sQLiteDataReaderValue.StringValue;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetString(i - this.PrivateVisibleFieldCount);
			}
			if ((this._flags & SQLiteConnectionFlags.NoVerifyTextAffinity) != SQLiteConnectionFlags.NoVerifyTextAffinity)
			{
				this.VerifyType(i, DbType.String);
			}
			return this._activeStatement._sql.GetText(this._activeStatement, i);
		}

		public string GetTableName(int i)
		{
			this.CheckDisposed();
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetName(i - this.PrivateVisibleFieldCount);
			}
			return this._activeStatement._sql.ColumnTableName(this._activeStatement, i);
		}

		public override object GetValue(int i)
		{
			bool flag;
			this.CheckDisposed();
			this.VerifyForGet();
			if ((this._flags & SQLiteConnectionFlags.UseConnectionReadValueCallbacks) == SQLiteConnectionFlags.UseConnectionReadValueCallbacks)
			{
				SQLiteDataReaderValue sQLiteDataReaderValue = new SQLiteDataReaderValue();
				this.InvokeReadValueCallback(i, new SQLiteReadValueEventArgs("GetValue", null, sQLiteDataReaderValue), out flag);
				if (flag)
				{
					return sQLiteDataReaderValue.Value;
				}
			}
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.GetValue(i - this.PrivateVisibleFieldCount);
			}
			SQLiteType sQLiteType = this.GetSQLiteType(this._flags, i);
			if ((this._flags & SQLiteConnectionFlags.DetectTextAffinity) == SQLiteConnectionFlags.DetectTextAffinity && (sQLiteType == null || sQLiteType.Affinity == TypeAffinity.Text))
			{
				sQLiteType = this.GetSQLiteType(sQLiteType, this._activeStatement._sql.GetText(this._activeStatement, i));
			}
			else if ((this._flags & SQLiteConnectionFlags.DetectStringType) == SQLiteConnectionFlags.DetectStringType && (sQLiteType == null || SQLiteConvert.IsStringDbType(sQLiteType.Type)))
			{
				sQLiteType = this.GetSQLiteType(sQLiteType, this._activeStatement._sql.GetText(this._activeStatement, i));
			}
			return this._activeStatement._sql.GetValue(this._activeStatement, this._flags, i, sQLiteType);
		}

		public override int GetValues(object[] values)
		{
			this.CheckDisposed();
			int fieldCount = this.FieldCount;
			if ((int)values.Length < fieldCount)
			{
				fieldCount = (int)values.Length;
			}
			for (int i = 0; i < fieldCount; i++)
			{
				values[i] = this.GetValue(i);
			}
			return fieldCount;
		}

		public NameValueCollection GetValues()
		{
			this.CheckDisposed();
			if (this._activeStatement == null || this._activeStatement._sql == null)
			{
				throw new InvalidOperationException();
			}
			int privateVisibleFieldCount = this.PrivateVisibleFieldCount;
			NameValueCollection nameValueCollection = new NameValueCollection(privateVisibleFieldCount);
			for (int i = 0; i < privateVisibleFieldCount; i++)
			{
				string str = this._activeStatement._sql.ColumnName(this._activeStatement, i);
				string text = this._activeStatement._sql.GetText(this._activeStatement, i);
				nameValueCollection.Add(str, text);
			}
			return nameValueCollection;
		}

		private void InvokeReadValueCallback(int index, SQLiteReadEventArgs eventArgs, out bool complete)
		{
			SQLiteTypeCallbacks sQLiteTypeCallback;
			complete = false;
			SQLiteConnectionFlags sQLiteConnectionFlag = this._flags;
			SQLiteDataReader sQLiteDataReader = this;
			sQLiteDataReader._flags = sQLiteDataReader._flags & (SQLiteConnectionFlags.LogPrepare | SQLiteConnectionFlags.LogPreBind | SQLiteConnectionFlags.LogBind | SQLiteConnectionFlags.LogCallbackException | SQLiteConnectionFlags.LogBackup | SQLiteConnectionFlags.NoExtensionFunctions | SQLiteConnectionFlags.BindUInt32AsInt64 | SQLiteConnectionFlags.BindAllAsText | SQLiteConnectionFlags.GetAllAsText | SQLiteConnectionFlags.NoLoadExtension | SQLiteConnectionFlags.NoCreateModule | SQLiteConnectionFlags.NoBindFunctions | SQLiteConnectionFlags.NoLogModule | SQLiteConnectionFlags.LogModuleError | SQLiteConnectionFlags.LogModuleException | SQLiteConnectionFlags.TraceWarning | SQLiteConnectionFlags.ConvertInvariantText | SQLiteConnectionFlags.BindInvariantText | SQLiteConnectionFlags.NoConnectionPool | SQLiteConnectionFlags.UseConnectionPool | SQLiteConnectionFlags.UseConnectionTypes | SQLiteConnectionFlags.NoGlobalTypes | SQLiteConnectionFlags.StickyHasRows | SQLiteConnectionFlags.StrictEnlistment | SQLiteConnectionFlags.MapIsolationLevels | SQLiteConnectionFlags.DetectTextAffinity | SQLiteConnectionFlags.DetectStringType | SQLiteConnectionFlags.NoConvertSettings | SQLiteConnectionFlags.BindDateTimeWithKind | SQLiteConnectionFlags.RollbackOnException | SQLiteConnectionFlags.DenyOnException | SQLiteConnectionFlags.InterruptOnException | SQLiteConnectionFlags.UnbindFunctionsOnClose | SQLiteConnectionFlags.NoVerifyTextAffinity | SQLiteConnectionFlags.UseConnectionBindValueCallbacks | SQLiteConnectionFlags.UseParameterNameForTypeName | SQLiteConnectionFlags.UseParameterDbTypeForTypeName | SQLiteConnectionFlags.NoVerifyTypeAffinity | SQLiteConnectionFlags.AllowNestedTransactions | SQLiteConnectionFlags.BindDecimalAsText | SQLiteConnectionFlags.GetDecimalAsText | SQLiteConnectionFlags.BindInvariantDecimal | SQLiteConnectionFlags.GetInvariantDecimal | SQLiteConnectionFlags.BindAndGetAllAsText | SQLiteConnectionFlags.ConvertAndBindInvariantText | SQLiteConnectionFlags.BindAndGetAllAsInvariantText | SQLiteConnectionFlags.ConvertAndBindAndGetAllAsInvariantText | SQLiteConnectionFlags.UseParameterAnythingForTypeName | SQLiteConnectionFlags.LogAll | SQLiteConnectionFlags.LogDefault | SQLiteConnectionFlags.Default | SQLiteConnectionFlags.DefaultAndLogAll);
			try
			{
				string dataTypeName = this.GetDataTypeName(index);
				if (dataTypeName != null)
				{
					SQLiteConnection connection = SQLiteDataReader.GetConnection(this);
					if (connection != null)
					{
						if (connection.TryGetTypeCallbacks(dataTypeName, out sQLiteTypeCallback) && sQLiteTypeCallback != null)
						{
							SQLiteReadValueCallback readValueCallback = sQLiteTypeCallback.ReadValueCallback;
							if (readValueCallback != null)
							{
								object readValueUserData = sQLiteTypeCallback.ReadValueUserData;
								readValueCallback(this._activeStatement._sql, this, sQLiteConnectionFlag, eventArgs, dataTypeName, index, readValueUserData, out complete);
							}
						}
					}
				}
			}
			finally
			{
				this._flags |= SQLiteConnectionFlags.UseConnectionReadValueCallbacks;
			}
		}

		public override bool IsDBNull(int i)
		{
			this.CheckDisposed();
			this.VerifyForGet();
			if (i >= this.PrivateVisibleFieldCount && this._keyInfo != null)
			{
				return this._keyInfo.IsDBNull(i - this.PrivateVisibleFieldCount);
			}
			return this._activeStatement._sql.IsNull(this._activeStatement, i);
		}

		private void LoadKeyInfo()
		{
			if (this._keyInfo != null)
			{
				this._keyInfo.Dispose();
				this._keyInfo = null;
			}
			this._keyInfo = new SQLiteKeyReader(this._command.Connection, this, this._activeStatement);
		}

		public override bool NextResult()
		{
			int num;
			this.CheckDisposed();
			this.CheckClosed();
			bool flag = this._throwOnDisposed;
			SQLiteStatement statement = null;
			bool flag1 = (this._commandBehavior & CommandBehavior.SchemaOnly) != CommandBehavior.Default;
			while (true)
			{
				if (statement == null && this._activeStatement != null && this._activeStatement._sql != null && this._activeStatement._sql.IsOpen())
				{
					if (!flag1)
					{
						this._activeStatement._sql.Reset(this._activeStatement);
					}
					if ((this._commandBehavior & CommandBehavior.SingleResult) != CommandBehavior.Default)
					{
						while (true)
						{
							statement = this._command.GetStatement(this._activeStatementIndex + 1);
							if (statement == null)
							{
								break;
							}
							this._activeStatementIndex++;
							if (!flag1 && statement._sql.Step(statement))
							{
								this._stepCount++;
							}
							if (statement._sql.ColumnCount(statement) == 0)
							{
								int num1 = 0;
								bool flag2 = false;
								if (!statement.TryGetChanges(ref num1, ref flag2))
								{
									return false;
								}
								if (!flag2)
								{
									if (this._rowsAffected == -1)
									{
										this._rowsAffected = 0;
									}
									this._rowsAffected += num1;
								}
							}
							if (!flag1)
							{
								statement._sql.Reset(statement);
							}
						}
						return false;
					}
				}
				statement = this._command.GetStatement(this._activeStatementIndex + 1);
				if (statement == null)
				{
					return false;
				}
				if (this._readingState < 1)
				{
					this._readingState = 1;
				}
				this._activeStatementIndex++;
				num = statement._sql.ColumnCount(statement);
				if (flag1 && num != 0)
				{
					break;
				}
				if (!flag1 && statement._sql.Step(statement))
				{
					this._stepCount++;
					this._readingState = -1;
					break;
				}
				else if (num != 0)
				{
					this._readingState = 1;
					break;
				}
				else
				{
					int num2 = 0;
					bool flag3 = false;
					if (!statement.TryGetChanges(ref num2, ref flag3))
					{
						return false;
					}
					if (!flag3)
					{
						if (this._rowsAffected == -1)
						{
							this._rowsAffected = 0;
						}
						this._rowsAffected += num2;
					}
					if (!flag1)
					{
						statement._sql.Reset(statement);
					}
				}
			}
			this._activeStatement = statement;
			this._fieldCount = num;
			this._fieldIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			this._fieldTypeArray = new SQLiteType[this.PrivateVisibleFieldCount];
			if ((this._commandBehavior & CommandBehavior.KeyInfo) != CommandBehavior.Default)
			{
				this.LoadKeyInfo();
			}
			return true;
		}

		public override bool Read()
		{
			this.CheckDisposed();
			this.CheckClosed();
			bool flag = this._throwOnDisposed;
			if ((this._commandBehavior & CommandBehavior.SchemaOnly) != CommandBehavior.Default)
			{
				return false;
			}
			if (this._readingState == -1)
			{
				this._readingState = 0;
				return true;
			}
			if (this._readingState == 0)
			{
				if ((this._commandBehavior & CommandBehavior.SingleRow) == CommandBehavior.Default && this._activeStatement._sql.Step(this._activeStatement))
				{
					this._stepCount++;
					if (this._keyInfo != null)
					{
						this._keyInfo.Reset();
					}
					return true;
				}
				this._readingState = 1;
			}
			return false;
		}

		public void RefreshFlags()
		{
			this.CheckDisposed();
			this._flags = SQLiteCommand.GetFlags(this._command);
		}

		private void VerifyForGet()
		{
			this.CheckClosed();
			this.CheckValidRow();
		}

		private TypeAffinity VerifyType(int i, DbType typ)
		{
			if ((this._flags & SQLiteConnectionFlags.NoVerifyTypeAffinity) == SQLiteConnectionFlags.NoVerifyTypeAffinity)
			{
				return TypeAffinity.None;
			}
			TypeAffinity affinity = this.GetSQLiteType(this._flags, i).Affinity;
			switch (affinity)
			{
				case TypeAffinity.Int64:
				{
					if (typ == DbType.Int64)
					{
						return affinity;
					}
					if (typ == DbType.Int32)
					{
						return affinity;
					}
					if (typ == DbType.Int16)
					{
						return affinity;
					}
					if (typ == DbType.Byte)
					{
						return affinity;
					}
					if (typ == DbType.SByte)
					{
						return affinity;
					}
					if (typ == DbType.Boolean)
					{
						return affinity;
					}
					if (typ == DbType.DateTime)
					{
						return affinity;
					}
					if (typ == DbType.Double)
					{
						return affinity;
					}
					if (typ == DbType.Single)
					{
						return affinity;
					}
					if (typ != DbType.Decimal)
					{
						break;
					}
					return affinity;
				}
				case TypeAffinity.Double:
				{
					if (typ == DbType.Double)
					{
						return affinity;
					}
					if (typ == DbType.Single)
					{
						return affinity;
					}
					if (typ == DbType.Decimal)
					{
						return affinity;
					}
					if (typ != DbType.DateTime)
					{
						break;
					}
					return affinity;
				}
				case TypeAffinity.Text:
				{
					if (typ == DbType.String)
					{
						return affinity;
					}
					if (typ == DbType.Guid)
					{
						return affinity;
					}
					if (typ == DbType.DateTime)
					{
						return affinity;
					}
					if (typ != DbType.Decimal)
					{
						break;
					}
					return affinity;
				}
				case TypeAffinity.Blob:
				{
					if (typ == DbType.Guid)
					{
						return affinity;
					}
					if (typ == DbType.Binary)
					{
						return affinity;
					}
					if (typ != DbType.String)
					{
						break;
					}
					return affinity;
				}
			}
			throw new InvalidCastException();
		}

		private sealed class ColumnParent : IEqualityComparer<SQLiteDataReader.ColumnParent>
		{
			public string DatabaseName;

			public string TableName;

			public string ColumnName;

			public ColumnParent()
			{
			}

			public ColumnParent(string databaseName, string tableName, string columnName) : this()
			{
				this.DatabaseName = databaseName;
				this.TableName = tableName;
				this.ColumnName = columnName;
			}

			public bool Equals(SQLiteDataReader.ColumnParent x, SQLiteDataReader.ColumnParent y)
			{
				if (x == null && y == null)
				{
					return true;
				}
				if (x == null || y == null)
				{
					return false;
				}
				if (!string.Equals(x.DatabaseName, y.DatabaseName, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if (!string.Equals(x.TableName, y.TableName, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if (!string.Equals(x.ColumnName, y.ColumnName, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				return true;
			}

			public int GetHashCode(SQLiteDataReader.ColumnParent obj)
			{
				int hashCode = 0;
				if (obj != null && obj.DatabaseName != null)
				{
					hashCode ^= obj.DatabaseName.GetHashCode();
				}
				if (obj != null && obj.TableName != null)
				{
					hashCode ^= obj.TableName.GetHashCode();
				}
				if (obj != null && obj.ColumnName != null)
				{
					hashCode ^= obj.ColumnName.GetHashCode();
				}
				return hashCode;
			}
		}
	}
}