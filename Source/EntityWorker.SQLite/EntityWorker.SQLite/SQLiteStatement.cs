using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EntityWorker.SQLite
{
	internal sealed class SQLiteStatement : IDisposable
	{
		internal SQLiteBase _sql;

		internal string _sqlStatement;

		internal SQLiteStatementHandle _sqlite_stmt;

		internal int _unnamedParameters;

		internal string[] _paramNames;

		internal SQLiteParameter[] _paramValues;

		internal SQLiteCommand _command;

		private SQLiteConnectionFlags _flags;

		private string[] _types;

		private bool disposed;

		internal string[] TypeDefinitions
		{
			get
			{
				return this._types;
			}
		}

		internal SQLiteStatement(SQLiteBase sqlbase, SQLiteConnectionFlags flags, SQLiteStatementHandle stmt, string strCommand, SQLiteStatement previous)
		{
			this._sql = sqlbase;
			this._sqlite_stmt = stmt;
			this._sqlStatement = strCommand;
			this._flags = flags;
			int num = 0;
			int num1 = this._sql.Bind_ParamCount(this, this._flags);
			if (num1 > 0)
			{
				if (previous != null)
				{
					num = previous._unnamedParameters;
				}
				this._paramNames = new string[num1];
				this._paramValues = new SQLiteParameter[num1];
				for (int i = 0; i < num1; i++)
				{
					string str = this._sql.Bind_ParamName(this, this._flags, i + 1);
					if (string.IsNullOrEmpty(str))
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						object[] objArray = new object[] { num };
						str = HelperMethods.StringFormat(invariantCulture, ";{0}", objArray);
						num++;
						this._unnamedParameters++;
					}
					this._paramNames[i] = str;
					this._paramValues[i] = null;
				}
			}
		}

		private void BindParameter(int index, SQLiteParameter param)
		{
			bool flag;
			SQLiteBase sQLiteBase;
			SQLiteConnectionFlags sQLiteConnectionFlag;
			int num;
			string str;
			if (param == null)
			{
				throw new SQLiteException("Insufficient parameters supplied to the command");
			}
			if ((this._flags & SQLiteConnectionFlags.UseConnectionBindValueCallbacks) == SQLiteConnectionFlags.UseConnectionBindValueCallbacks)
			{
				this.InvokeBindValueCallback(index, param, out flag);
				if (flag)
				{
					return;
				}
			}
			object value = param.Value;
			DbType dbType = param.DbType;
			if (value != null && dbType == DbType.Object)
			{
				dbType = SQLiteConvert.TypeToDbType(value.GetType());
			}
			if (HelperMethods.LogPreBind(this._flags))
			{
				IntPtr _sqliteStmt = this._sqlite_stmt;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { _sqliteStmt, index, dbType, value };
				SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Binding statement {0} paramter #{1} with database type {2} and raw value {{{3}}}...", objArray));
			}
			if (value == null || Convert.IsDBNull(value))
			{
				this._sql.Bind_Null(this, this._flags, index);
				return;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			bool flag1 = (this._flags & SQLiteConnectionFlags.BindInvariantText) == SQLiteConnectionFlags.BindInvariantText;
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			if ((this._flags & SQLiteConnectionFlags.ConvertInvariantText) == SQLiteConnectionFlags.ConvertInvariantText)
			{
				cultureInfo = invariantCulture;
			}
			if ((this._flags & SQLiteConnectionFlags.BindAllAsText) == SQLiteConnectionFlags.BindAllAsText)
			{
				if (value is DateTime)
				{
					this._sql.Bind_DateTime(this, this._flags, index, (DateTime)value);
					return;
				}
				this._sql.Bind_Text(this, this._flags, index, (flag1 ? SQLiteConvert.ToStringWithProvider(value, invariantCulture) : SQLiteConvert.ToStringWithProvider(value, cultureInfo)));
				return;
			}
			bool flag2 = (this._flags & SQLiteConnectionFlags.BindInvariantDecimal) == SQLiteConnectionFlags.BindInvariantDecimal;
			if ((this._flags & SQLiteConnectionFlags.BindDecimalAsText) == SQLiteConnectionFlags.BindDecimalAsText && value is decimal)
			{
				this._sql.Bind_Text(this, this._flags, index, (flag1 || flag2 ? SQLiteConvert.ToStringWithProvider(value, invariantCulture) : SQLiteConvert.ToStringWithProvider(value, cultureInfo)));
				return;
			}
			switch (dbType)
			{
				case DbType.Binary:
				{
					this._sql.Bind_Blob(this, this._flags, index, (byte[])value);
					return;
				}
				case DbType.Byte:
				{
					this._sql.Bind_UInt32(this, this._flags, index, Convert.ToByte(value, cultureInfo));
					return;
				}
				case DbType.Boolean:
				{
					this._sql.Bind_Boolean(this, this._flags, index, SQLiteConvert.ToBoolean(value, cultureInfo, true));
					return;
				}
				case DbType.Currency:
				case DbType.Double:
				case DbType.Single:
				{
					this._sql.Bind_Double(this, this._flags, index, Convert.ToDouble(value, cultureInfo));
					return;
				}
				case DbType.Date:
				case DbType.DateTime:
				case DbType.Time:
				{
					this._sql.Bind_DateTime(this, this._flags, index, (value is string ? this._sql.ToDateTime((string)value) : Convert.ToDateTime(value, cultureInfo)));
					return;
				}
				case DbType.Decimal:
				{
					this._sql.Bind_Text(this, this._flags, index, (flag1 || flag2 ? SQLiteConvert.ToStringWithProvider(Convert.ToDecimal(value, cultureInfo), invariantCulture) : SQLiteConvert.ToStringWithProvider(Convert.ToDecimal(value, cultureInfo), cultureInfo)));
					return;
				}
				case DbType.Guid:
				{
					if (this._command.Connection._binaryGuid)
					{
						SQLiteBase sQLiteBase1 = this._sql;
						SQLiteConnectionFlags sQLiteConnectionFlag1 = this._flags;
						Guid guid = (Guid)value;
						sQLiteBase1.Bind_Blob(this, sQLiteConnectionFlag1, index, guid.ToByteArray());
						return;
					}
					this._sql.Bind_Text(this, this._flags, index, (flag1 ? SQLiteConvert.ToStringWithProvider(value, invariantCulture) : SQLiteConvert.ToStringWithProvider(value, cultureInfo)));
					return;
				}
				case DbType.Int16:
				{
					this._sql.Bind_Int32(this, this._flags, index, Convert.ToInt16(value, cultureInfo));
					return;
				}
				case DbType.Int32:
				{
					this._sql.Bind_Int32(this, this._flags, index, Convert.ToInt32(value, cultureInfo));
					return;
				}
				case DbType.Int64:
				{
					this._sql.Bind_Int64(this, this._flags, index, Convert.ToInt64(value, cultureInfo));
					return;
				}
				case DbType.Object:
				case DbType.String:
				{
					sQLiteBase = this._sql;
					sQLiteConnectionFlag = this._flags;
					num = index;
					str = (flag1 ? SQLiteConvert.ToStringWithProvider(value, invariantCulture) : SQLiteConvert.ToStringWithProvider(value, cultureInfo));
					sQLiteBase.Bind_Text(this, sQLiteConnectionFlag, num, str);
					return;
				}
				case DbType.SByte:
				{
					this._sql.Bind_Int32(this, this._flags, index, Convert.ToSByte(value, cultureInfo));
					return;
				}
				case DbType.UInt16:
				{
					this._sql.Bind_UInt32(this, this._flags, index, Convert.ToUInt16(value, cultureInfo));
					return;
				}
				case DbType.UInt32:
				{
					this._sql.Bind_UInt32(this, this._flags, index, Convert.ToUInt32(value, cultureInfo));
					return;
				}
				case DbType.UInt64:
				{
					this._sql.Bind_UInt64(this, this._flags, index, Convert.ToUInt64(value, cultureInfo));
					return;
				}
				default:
				{
					sQLiteBase = this._sql;
					sQLiteConnectionFlag = this._flags;
					num = index;
					str = (flag1 ? SQLiteConvert.ToStringWithProvider(value, invariantCulture) : SQLiteConvert.ToStringWithProvider(value, cultureInfo));
					sQLiteBase.Bind_Text(this, sQLiteConnectionFlag, num, str);
					return;
				}
			}
		}

		internal void BindParameters()
		{
			if (this._paramNames == null)
			{
				return;
			}
			int length = (int)this._paramNames.Length;
			for (int i = 0; i < length; i++)
			{
				this.BindParameter(i + 1, this._paramValues[i]);
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteStatement).Name);
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
					if (this._sqlite_stmt != null)
					{
						this._sqlite_stmt.Dispose();
						this._sqlite_stmt = null;
					}
					this._paramNames = null;
					this._paramValues = null;
					this._sql = null;
					this._sqlStatement = null;
				}
				this.disposed = true;
			}
		}

		~SQLiteStatement()
		{
			this.Dispose(false);
		}

		private static SQLiteConnection GetConnection(SQLiteStatement statement)
		{
			try
			{
				if (statement != null)
				{
					SQLiteCommand sQLiteCommand = statement._command;
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

		private void InvokeBindValueCallback(int index, SQLiteParameter parameter, out bool complete)
		{
			SQLiteTypeCallbacks sQLiteTypeCallback;
			complete = false;
			SQLiteConnectionFlags sQLiteConnectionFlag = this._flags;
			SQLiteStatement sQLiteStatement = this;
			sQLiteStatement._flags = sQLiteStatement._flags & (SQLiteConnectionFlags.LogPrepare | SQLiteConnectionFlags.LogPreBind | SQLiteConnectionFlags.LogBind | SQLiteConnectionFlags.LogCallbackException | SQLiteConnectionFlags.LogBackup | SQLiteConnectionFlags.NoExtensionFunctions | SQLiteConnectionFlags.BindUInt32AsInt64 | SQLiteConnectionFlags.BindAllAsText | SQLiteConnectionFlags.GetAllAsText | SQLiteConnectionFlags.NoLoadExtension | SQLiteConnectionFlags.NoCreateModule | SQLiteConnectionFlags.NoBindFunctions | SQLiteConnectionFlags.NoLogModule | SQLiteConnectionFlags.LogModuleError | SQLiteConnectionFlags.LogModuleException | SQLiteConnectionFlags.TraceWarning | SQLiteConnectionFlags.ConvertInvariantText | SQLiteConnectionFlags.BindInvariantText | SQLiteConnectionFlags.NoConnectionPool | SQLiteConnectionFlags.UseConnectionPool | SQLiteConnectionFlags.UseConnectionTypes | SQLiteConnectionFlags.NoGlobalTypes | SQLiteConnectionFlags.StickyHasRows | SQLiteConnectionFlags.StrictEnlistment | SQLiteConnectionFlags.MapIsolationLevels | SQLiteConnectionFlags.DetectTextAffinity | SQLiteConnectionFlags.DetectStringType | SQLiteConnectionFlags.NoConvertSettings | SQLiteConnectionFlags.BindDateTimeWithKind | SQLiteConnectionFlags.RollbackOnException | SQLiteConnectionFlags.DenyOnException | SQLiteConnectionFlags.InterruptOnException | SQLiteConnectionFlags.UnbindFunctionsOnClose | SQLiteConnectionFlags.NoVerifyTextAffinity | SQLiteConnectionFlags.UseConnectionReadValueCallbacks | SQLiteConnectionFlags.UseParameterNameForTypeName | SQLiteConnectionFlags.UseParameterDbTypeForTypeName | SQLiteConnectionFlags.NoVerifyTypeAffinity | SQLiteConnectionFlags.AllowNestedTransactions | SQLiteConnectionFlags.BindDecimalAsText | SQLiteConnectionFlags.GetDecimalAsText | SQLiteConnectionFlags.BindInvariantDecimal | SQLiteConnectionFlags.GetInvariantDecimal | SQLiteConnectionFlags.BindAndGetAllAsText | SQLiteConnectionFlags.ConvertAndBindInvariantText | SQLiteConnectionFlags.BindAndGetAllAsInvariantText | SQLiteConnectionFlags.ConvertAndBindAndGetAllAsInvariantText | SQLiteConnectionFlags.UseParameterAnythingForTypeName | SQLiteConnectionFlags.LogAll | SQLiteConnectionFlags.LogDefault | SQLiteConnectionFlags.Default | SQLiteConnectionFlags.DefaultAndLogAll);
			try
			{
				if (parameter != null)
				{
					SQLiteConnection connection = SQLiteStatement.GetConnection(this);
					if (connection != null)
					{
						string typeName = parameter.TypeName;
						if (typeName == null && (this._flags & SQLiteConnectionFlags.UseParameterNameForTypeName) == SQLiteConnectionFlags.UseParameterNameForTypeName)
						{
							typeName = parameter.ParameterName;
						}
						if (typeName == null && (this._flags & SQLiteConnectionFlags.UseParameterDbTypeForTypeName) == SQLiteConnectionFlags.UseParameterDbTypeForTypeName)
						{
							typeName = SQLiteConvert.DbTypeToTypeName(connection, parameter.DbType, this._flags);
						}
						if (typeName != null)
						{
							if (connection.TryGetTypeCallbacks(typeName, out sQLiteTypeCallback) && sQLiteTypeCallback != null)
							{
								SQLiteBindValueCallback bindValueCallback = sQLiteTypeCallback.BindValueCallback;
								if (bindValueCallback != null)
								{
									object bindValueUserData = sQLiteTypeCallback.BindValueUserData;
									bindValueCallback(this._sql, this._command, sQLiteConnectionFlag, parameter, typeName, index, bindValueUserData, out complete);
								}
							}
						}
					}
				}
			}
			finally
			{
				this._flags |= SQLiteConnectionFlags.UseConnectionBindValueCallbacks;
			}
		}

		internal bool MapParameter(string s, SQLiteParameter p)
		{
			if (this._paramNames == null)
			{
				return false;
			}
			int num = 0;
			if (s.Length > 0 && ":$@;".IndexOf(s[0]) == -1)
			{
				num = 1;
			}
			int length = (int)this._paramNames.Length;
			for (int i = 0; i < length; i++)
			{
				if (string.Compare(this._paramNames[i], num, s, 0, Math.Max(this._paramNames[i].Length - num, s.Length), StringComparison.OrdinalIgnoreCase) == 0)
				{
					this._paramValues[i] = p;
					return true;
				}
			}
			return false;
		}

		internal void SetTypes(string typedefs)
		{
			int num = typedefs.IndexOf("TYPES", 0, StringComparison.OrdinalIgnoreCase);
			if (num == -1)
			{
				throw new ArgumentOutOfRangeException();
			}
			string str = typedefs.Substring(num + 6).Replace(" ", string.Empty).Replace(";", string.Empty).Replace("\"", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("`", string.Empty);
			char[] chrArray = new char[] { ',', '\r', '\n', '\t' };
			string[] strArrays = str.Split(chrArray);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				if (string.IsNullOrEmpty(strArrays[i]))
				{
					strArrays[i] = null;
				}
			}
			this._types = strArrays;
		}

		internal bool TryGetChanges(ref int changes, ref bool readOnly)
		{
			if (this._sql == null || !this._sql.IsOpen())
			{
				return false;
			}
			changes = this._sql.Changes;
			readOnly = this._sql.IsReadOnly(this);
			return true;
		}
	}
}