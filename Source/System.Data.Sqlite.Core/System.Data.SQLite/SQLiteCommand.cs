using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;

namespace System.Data.SQLite
{
	[Designer("SQLite.Designer.SQLiteCommandDesigner, SQLite.Designer, Version=1.0.106.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139")]
	[ToolboxItem(true)]
	public sealed class SQLiteCommand : DbCommand, ICloneable
	{
		private readonly static string DefaultConnectionString;

		private string _commandText;

		private SQLiteConnection _cnn;

		private int _version;

		private WeakReference _activeReader;

		internal int _commandTimeout;

		private bool _designTimeVisible;

		private UpdateRowSource _updateRowSource;

		private SQLiteParameterCollection _parameterCollection;

		internal List<SQLiteStatement> _statementList;

		internal string _remainingText;

		private SQLiteTransaction _transaction;

		private bool disposed;

		[DefaultValue("")]
		[Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[RefreshProperties(RefreshProperties.All)]
		public override string CommandText
		{
			get
			{
				this.CheckDisposed();
				return this._commandText;
			}
			set
			{
				this.CheckDisposed();
				if (this._commandText == value)
				{
					return;
				}
				if (this._activeReader != null && this._activeReader.IsAlive)
				{
					throw new InvalidOperationException("Cannot set CommandText while a DataReader is active");
				}
				this.ClearCommands();
				this._commandText = value;
				SQLiteConnection sQLiteConnection = this._cnn;
			}
		}

		[DefaultValue(30)]
		public override int CommandTimeout
		{
			get
			{
				this.CheckDisposed();
				return this._commandTimeout;
			}
			set
			{
				this.CheckDisposed();
				this._commandTimeout = value;
			}
		}

		[DefaultValue(System.Data.CommandType.Text)]
		[RefreshProperties(RefreshProperties.All)]
		public override System.Data.CommandType CommandType
		{
			get
			{
				this.CheckDisposed();
				return System.Data.CommandType.Text;
			}
			set
			{
				this.CheckDisposed();
				if (value != System.Data.CommandType.Text)
				{
					throw new NotSupportedException();
				}
			}
		}

		[DefaultValue(null)]
		[Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public new SQLiteConnection Connection
		{
			get
			{
				this.CheckDisposed();
				return this._cnn;
			}
			set
			{
				this.CheckDisposed();
				if (this._activeReader != null && this._activeReader.IsAlive)
				{
					throw new InvalidOperationException("Cannot set Connection while a DataReader is active");
				}
				if (this._cnn != null)
				{
					this.ClearCommands();
				}
				this._cnn = value;
				if (this._cnn != null)
				{
					this._version = this._cnn._version;
				}
			}
		}

		protected override System.Data.Common.DbConnection DbConnection
		{
			get
			{
				return this.Connection;
			}
			set
			{
				this.Connection = (SQLiteConnection)value;
			}
		}

		protected override System.Data.Common.DbParameterCollection DbParameterCollection
		{
			get
			{
				return this.Parameters;
			}
		}

		protected override System.Data.Common.DbTransaction DbTransaction
		{
			get
			{
				return this.Transaction;
			}
			set
			{
				this.Transaction = (SQLiteTransaction)value;
			}
		}

		[Browsable(false)]
		[DefaultValue(true)]
		[DesignOnly(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool DesignTimeVisible
		{
			get
			{
				this.CheckDisposed();
				return this._designTimeVisible;
			}
			set
			{
				this.CheckDisposed();
				this._designTimeVisible = value;
				TypeDescriptor.Refresh(this);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new SQLiteParameterCollection Parameters
		{
			get
			{
				this.CheckDisposed();
				return this._parameterCollection;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new SQLiteTransaction Transaction
		{
			get
			{
				this.CheckDisposed();
				return this._transaction;
			}
			set
			{
				this.CheckDisposed();
				if (this._cnn == null)
				{
					if (value != null)
					{
						this.Connection = value.Connection;
					}
					this._transaction = value;
					return;
				}
				if (this._activeReader != null && this._activeReader.IsAlive)
				{
					throw new InvalidOperationException("Cannot set Transaction while a DataReader is active");
				}
				if (value != null && value._cnn != this._cnn)
				{
					throw new ArgumentException("Transaction is not associated with the command's connection");
				}
				this._transaction = value;
			}
		}

		[DefaultValue(UpdateRowSource.None)]
		public override UpdateRowSource UpdatedRowSource
		{
			get
			{
				this.CheckDisposed();
				return this._updateRowSource;
			}
			set
			{
				this.CheckDisposed();
				this._updateRowSource = value;
			}
		}

		static SQLiteCommand()
		{
			SQLiteCommand.DefaultConnectionString = "Data Source=:memory:;";
		}

		public SQLiteCommand() : this(null, null)
		{
		}

		public SQLiteCommand(string commandText) : this(commandText, null, null)
		{
		}

		public SQLiteCommand(string commandText, SQLiteConnection connection) : this(commandText, connection, null)
		{
		}

		public SQLiteCommand(SQLiteConnection connection) : this(null, connection, null)
		{
		}

		private SQLiteCommand(SQLiteCommand source) : this(source.CommandText, source.Connection, source.Transaction)
		{
			this.CommandTimeout = source.CommandTimeout;
			this.DesignTimeVisible = source.DesignTimeVisible;
			this.UpdatedRowSource = source.UpdatedRowSource;
			foreach (SQLiteParameter sQLiteParameter in source._parameterCollection)
			{
				this.Parameters.Add(sQLiteParameter.Clone());
			}
		}

		public SQLiteCommand(string commandText, SQLiteConnection connection, SQLiteTransaction transaction)
		{
			this._commandTimeout = 30;
			this._parameterCollection = new SQLiteParameterCollection(this);
			this._designTimeVisible = true;
			this._updateRowSource = UpdateRowSource.None;
			if (commandText != null)
			{
				this.CommandText = commandText;
			}
			if (connection != null)
			{
				this.DbConnection = connection;
				this._commandTimeout = connection.DefaultTimeout;
			}
			if (transaction != null)
			{
				this.Transaction = transaction;
			}
			SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(SQLiteConnectionEventType.NewCommand, null, transaction, this, null, null, null, null));
		}

		internal SQLiteStatement BuildNextCommand()
		{
			SQLiteStatement sQLiteStatement;
			SQLiteStatement item;
			SQLiteStatement sQLiteStatement1 = null;
			try
			{
				if (this._cnn != null && this._cnn._sql != null)
				{
					if (this._statementList == null)
					{
						this._remainingText = this._commandText;
					}
					SQLiteBase sQLiteBase = this._cnn._sql;
					SQLiteConnection sQLiteConnection = this._cnn;
					string str = this._remainingText;
					if (this._statementList == null)
					{
						item = null;
					}
					else
					{
						item = this._statementList[this._statementList.Count - 1];
					}
					sQLiteStatement1 = sQLiteBase.Prepare(sQLiteConnection, str, item, (uint)(this._commandTimeout * 1000), ref this._remainingText);
					if (sQLiteStatement1 != null)
					{
						sQLiteStatement1._command = this;
						if (this._statementList == null)
						{
							this._statementList = new List<SQLiteStatement>();
						}
						this._statementList.Add(sQLiteStatement1);
						this._parameterCollection.MapParameters(sQLiteStatement1);
						sQLiteStatement1.BindParameters();
					}
				}
				sQLiteStatement = sQLiteStatement1;
			}
			catch (Exception exception)
			{
				if (sQLiteStatement1 != null)
				{
					if (this._statementList != null && this._statementList.Contains(sQLiteStatement1))
					{
						this._statementList.Remove(sQLiteStatement1);
					}
					sQLiteStatement1.Dispose();
				}
				this._remainingText = null;
				throw;
			}
			return sQLiteStatement;
		}

		public override void Cancel()
		{
			this.CheckDisposed();
			if (this._activeReader != null)
			{
				SQLiteDataReader target = this._activeReader.Target as SQLiteDataReader;
				if (target != null)
				{
					target.Cancel();
				}
			}
		}

		[Conditional("CHECK_STATE")]
		internal static void Check(SQLiteCommand command)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}
			command.CheckDisposed();
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteCommand).Name);
			}
		}

		internal void ClearCommands()
		{
			this.ClearDataReader();
			this.DisposeStatements();
			this._parameterCollection.Unbind();
		}

		private void ClearDataReader()
		{
			if (this._activeReader != null)
			{
				SQLiteDataReader target = null;
				try
				{
					target = this._activeReader.Target as SQLiteDataReader;
				}
				catch (InvalidOperationException invalidOperationException)
				{
				}
				if (target != null)
				{
					target.Close();
				}
				this._activeReader = null;
			}
		}

		public object Clone()
		{
			this.CheckDisposed();
			return new SQLiteCommand(this);
		}

		protected override DbParameter CreateDbParameter()
		{
			return this.CreateParameter();
		}

		public new SQLiteParameter CreateParameter()
		{
			this.CheckDisposed();
			return new SQLiteParameter(this);
		}

		protected override void Dispose(bool disposing)
		{
			SQLiteConnection sQLiteConnection = this._cnn;
			SQLiteTransaction sQLiteTransaction = this._transaction;
			object[] objArray = new object[] { disposing, this.disposed };
			SQLiteConnection.OnChanged(sQLiteConnection, new ConnectionEventArgs(SQLiteConnectionEventType.DisposingCommand, null, sQLiteTransaction, this, null, null, null, objArray));
			bool flag = false;
			try
			{
				if (!this.disposed && disposing)
				{
					SQLiteDataReader target = null;
					if (this._activeReader != null)
					{
						try
						{
							target = this._activeReader.Target as SQLiteDataReader;
						}
						catch (InvalidOperationException invalidOperationException)
						{
						}
					}
					if (target == null)
					{
						this.Connection = null;
						this._parameterCollection.Clear();
						this._commandText = null;
					}
					else
					{
						target._disposeCommand = true;
						this._activeReader = null;
						flag = true;
						return;
					}
				}
			}
			finally
			{
				if (!flag)
				{
					base.Dispose(disposing);
					this.disposed = true;
				}
			}
		}

		private void DisposeStatements()
		{
			if (this._statementList == null)
			{
				return;
			}
			int count = this._statementList.Count;
			for (int i = 0; i < count; i++)
			{
				SQLiteStatement item = this._statementList[i];
				if (item != null)
				{
					item.Dispose();
				}
			}
			this._statementList = null;
		}

		public static object Execute(string commandText, SQLiteExecuteType executeType, string connectionString, params object[] args)
		{
			return SQLiteCommand.Execute(commandText, executeType, CommandBehavior.Default, connectionString, args);
		}

		public static object Execute(string commandText, SQLiteExecuteType executeType, CommandBehavior commandBehavior, string connectionString, params object[] args)
		{
			object obj;
			SQLiteConnection sQLiteConnection = null;
			try
			{
				if (connectionString == null)
				{
					connectionString = SQLiteCommand.DefaultConnectionString;
				}
				SQLiteConnection sQLiteConnection1 = new SQLiteConnection(connectionString);
				sQLiteConnection = sQLiteConnection1;
				using (sQLiteConnection1)
				{
					sQLiteConnection.Open();
					using (SQLiteCommand sQLiteCommand = sQLiteConnection.CreateCommand())
					{
						sQLiteCommand.CommandText = commandText;
						if (args != null)
						{
							object[] objArray = args;
							for (int i = 0; i < (int)objArray.Length; i++)
							{
								object obj1 = objArray[i];
								SQLiteParameter sQLiteParameter = obj1 as SQLiteParameter;
								if (sQLiteParameter == null)
								{
									sQLiteParameter = sQLiteCommand.CreateParameter();
									sQLiteParameter.DbType = DbType.Object;
									sQLiteParameter.Value = obj1;
								}
								sQLiteCommand.Parameters.Add(sQLiteParameter);
							}
						}
						switch (executeType)
						{
							case SQLiteExecuteType.None:
							{
								break;
							}
							case SQLiteExecuteType.NonQuery:
							{
								obj = sQLiteCommand.ExecuteNonQuery(commandBehavior);
								return obj;
							}
							case SQLiteExecuteType.Scalar:
							{
								obj = sQLiteCommand.ExecuteScalar(commandBehavior);
								return obj;
							}
							case SQLiteExecuteType.Reader:
							{
								bool flag = true;
								try
								{
									try
									{
										obj = sQLiteCommand.ExecuteReader(commandBehavior | CommandBehavior.CloseConnection);
										return obj;
									}
									catch
									{
										flag = false;
										throw;
									}
								}
								finally
								{
									if (flag)
									{
										sQLiteConnection._noDispose = true;
									}
								}
								break;
							}
							default:
							{
								goto case SQLiteExecuteType.None;
							}
						}
					}
				}
				return null;
			}
			finally
			{
				if (sQLiteConnection != null)
				{
					sQLiteConnection._noDispose = false;
				}
			}
			return obj;
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return this.ExecuteReader(behavior);
		}

		public override int ExecuteNonQuery()
		{
			this.CheckDisposed();
			return this.ExecuteNonQuery(CommandBehavior.Default);
		}

		public int ExecuteNonQuery(CommandBehavior behavior)
		{
			int recordsAffected;
			this.CheckDisposed();
			using (SQLiteDataReader sQLiteDataReader = this.ExecuteReader(behavior | CommandBehavior.SingleRow | CommandBehavior.SingleResult))
			{
				while (sQLiteDataReader.NextResult())
				{
				}
				recordsAffected = sQLiteDataReader.RecordsAffected;
			}
			return recordsAffected;
		}

		public new SQLiteDataReader ExecuteReader(CommandBehavior behavior)
		{
			this.CheckDisposed();
			this.InitializeForReader();
			SQLiteDataReader sQLiteDataReader = new SQLiteDataReader(this, behavior);
			this._activeReader = new WeakReference(sQLiteDataReader, false);
			return sQLiteDataReader;
		}

		public new SQLiteDataReader ExecuteReader()
		{
			this.CheckDisposed();
			return this.ExecuteReader(CommandBehavior.Default);
		}

		public override object ExecuteScalar()
		{
			this.CheckDisposed();
			return this.ExecuteScalar(CommandBehavior.Default);
		}

		public object ExecuteScalar(CommandBehavior behavior)
		{
			object item;
			this.CheckDisposed();
			using (SQLiteDataReader sQLiteDataReader = this.ExecuteReader(behavior | CommandBehavior.SingleRow | CommandBehavior.SingleResult))
			{
				if (!sQLiteDataReader.Read() || sQLiteDataReader.FieldCount <= 0)
				{
					return null;
				}
				else
				{
					item = sQLiteDataReader[0];
				}
			}
			return item;
		}

		internal static SQLiteConnectionFlags GetFlags(SQLiteCommand command)
		{
			try
			{
				if (command != null)
				{
					SQLiteConnection sQLiteConnection = command._cnn;
					if (sQLiteConnection != null)
					{
						return sQLiteConnection.Flags;
					}
				}
			}
			catch (ObjectDisposedException objectDisposedException)
			{
			}
			return SQLiteConnectionFlags.Default;
		}

		internal SQLiteStatement GetStatement(int index)
		{
			if (this._statementList == null)
			{
				return this.BuildNextCommand();
			}
			if (index == this._statementList.Count)
			{
				if (string.IsNullOrEmpty(this._remainingText))
				{
					return null;
				}
				return this.BuildNextCommand();
			}
			SQLiteStatement item = this._statementList[index];
			item.BindParameters();
			return item;
		}

		private void InitializeForReader()
		{
			if (this._activeReader != null && this._activeReader.IsAlive)
			{
				throw new InvalidOperationException("DataReader already active on this command");
			}
			if (this._cnn == null)
			{
				throw new InvalidOperationException("No connection associated with this command");
			}
			if (this._cnn.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Database is not open");
			}
			if (this._cnn._version != this._version)
			{
				this._version = this._cnn._version;
				this.ClearCommands();
			}
			this._parameterCollection.MapParameters(null);
		}

		public override void Prepare()
		{
			this.CheckDisposed();
		}

		public void Reset()
		{
			this.CheckDisposed();
			this.Reset(true, false);
		}

		public void Reset(bool clearBindings, bool ignoreErrors)
		{
			this.CheckDisposed();
			if (clearBindings && this._parameterCollection != null)
			{
				this._parameterCollection.Unbind();
			}
			this.ClearDataReader();
			if (this._statementList == null)
			{
				return;
			}
			SQLiteBase sQLiteBase = this._cnn._sql;
			foreach (SQLiteStatement sQLiteStatement in this._statementList)
			{
				if (sQLiteStatement == null)
				{
					continue;
				}
				SQLiteStatementHandle _sqliteStmt = sQLiteStatement._sqlite_stmt;
				if (_sqliteStmt == null)
				{
					continue;
				}
				SQLiteErrorCode sQLiteErrorCode = sQLiteBase.Reset(sQLiteStatement);
				if (sQLiteErrorCode == SQLiteErrorCode.Ok && clearBindings && SQLite3.SQLiteVersionNumber >= 3003007)
				{
					sQLiteErrorCode = System.Data.SQLite.UnsafeNativeMethods.sqlite3_clear_bindings(_sqliteStmt);
				}
				if (ignoreErrors || sQLiteErrorCode == SQLiteErrorCode.Ok)
				{
					continue;
				}
				throw new SQLiteException(sQLiteErrorCode, sQLiteBase.GetLastError());
			}
		}

		internal void ResetDataReader()
		{
			this._activeReader = null;
		}

		public void VerifyOnly()
		{
			this.CheckDisposed();
			SQLiteConnection sQLiteConnection = this._cnn;
			SQLiteBase sQLiteBase = sQLiteConnection._sql;
			if (sQLiteConnection == null || sQLiteBase == null)
			{
				throw new SQLiteException("invalid or unusable connection");
			}
			List<SQLiteStatement> sQLiteStatements = null;
			SQLiteStatement sQLiteStatement = null;
			try
			{
				string str = this._commandText;
				uint num = (uint)(this._commandTimeout * 1000);
				SQLiteStatement sQLiteStatement1 = null;
				while (str != null && str.Length > 0)
				{
					sQLiteStatement = sQLiteBase.Prepare(sQLiteConnection, str, sQLiteStatement1, num, ref str);
					sQLiteStatement1 = sQLiteStatement;
					if (sQLiteStatement != null)
					{
						if (sQLiteStatements == null)
						{
							sQLiteStatements = new List<SQLiteStatement>();
						}
						sQLiteStatements.Add(sQLiteStatement);
						sQLiteStatement = null;
					}
					if (str == null)
					{
						continue;
					}
					str = str.Trim();
				}
			}
			finally
			{
				if (sQLiteStatement != null)
				{
					sQLiteStatement.Dispose();
					sQLiteStatement = null;
				}
				if (sQLiteStatements != null)
				{
					foreach (SQLiteStatement sQLiteStatement2 in sQLiteStatements)
					{
						if (sQLiteStatement2 == null)
						{
							continue;
						}
						sQLiteStatement2.Dispose();
					}
					sQLiteStatements.Clear();
					sQLiteStatements = null;
				}
			}
		}
	}
}