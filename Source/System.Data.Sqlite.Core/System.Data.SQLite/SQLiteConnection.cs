using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Transactions;

namespace System.Data.SQLite
{
    public sealed class SQLiteConnection : DbConnection, ICloneable, IDisposable
    {
        internal const DbType BadDbType = DbType.Binary | DbType.Byte | DbType.Boolean | DbType.Currency | DbType.Date | DbType.DateTime | DbType.Decimal | DbType.Double | DbType.Guid | DbType.Int16 | DbType.Int32 | DbType.Int64 | DbType.Object | DbType.SByte | DbType.Single | DbType.String | DbType.Time | DbType.UInt16 | DbType.UInt32 | DbType.UInt64 | DbType.VarNumeric | DbType.AnsiStringFixedLength | DbType.StringFixedLength | DbType.Xml | DbType.DateTime2 | DbType.DateTimeOffset;

        internal const string DefaultBaseSchemaName = "sqlite_default_schema";

        private const string MemoryFileName = ":memory:";

        internal const System.Data.IsolationLevel DeferredIsolationLevel = System.Data.IsolationLevel.ReadCommitted;

        internal const System.Data.IsolationLevel ImmediateIsolationLevel = System.Data.IsolationLevel.Serializable;

        private const SQLiteConnectionFlags FallbackDefaultFlags = SQLiteConnectionFlags.Default;

        private const SQLiteSynchronousEnum DefaultSynchronous = SQLiteSynchronousEnum.Default;

        private const SQLiteJournalModeEnum DefaultJournalMode = SQLiteJournalModeEnum.Default;

        private const System.Data.IsolationLevel DefaultIsolationLevel = System.Data.IsolationLevel.Serializable;

        internal const SQLiteDateFormats DefaultDateTimeFormat = SQLiteDateFormats.ISO8601;

        internal const DateTimeKind DefaultDateTimeKind = DateTimeKind.Unspecified;

        internal const string DefaultDateTimeFormatString = null;

        private const string DefaultDataSource = null;

        private const string DefaultUri = null;

        private const string DefaultFullUri = null;

        private const string DefaultHexPassword = null;

        private const string DefaultPassword = null;

        private const int DefaultVersion = 3;

        private const int DefaultPageSize = 4096;

        private const int DefaultMaxPageCount = 0;

        private const int DefaultCacheSize = -2000;

        private const int DefaultMaxPoolSize = 100;

        private const int DefaultConnectionTimeout = 30;

        private const int DefaultBusyTimeout = 0;

        private const bool DefaultNoDefaultFlags = false;

        private const bool DefaultNoSharedFlags = false;

        private const bool DefaultFailIfMissing = false;

        private const bool DefaultReadOnly = false;

        internal const bool DefaultBinaryGUID = true;

        private const bool DefaultUseUTF16Encoding = false;

        private const bool DefaultToFullPath = true;

        private const bool DefaultPooling = false;

        private const bool DefaultLegacyFormat = false;

        private const bool DefaultForeignKeys = false;

        private const bool DefaultRecursiveTriggers = false;

        private const bool DefaultEnlist = true;

        private const bool DefaultSetDefaults = true;

        internal const int DefaultPrepareRetries = 3;

        private const string DefaultVfsName = null;

        private const int DefaultProgressOps = 0;

        private const int SQLITE_FCNTL_CHUNK_SIZE = 6;

        private const int SQLITE_FCNTL_WIN32_AV_RETRY = 9;

        private const string _dataDirectory = "|DataDirectory|";

        private const string _masterdb = "sqlite_master";

        private const string _tempmasterdb = "sqlite_temp_master";

        private readonly static Assembly _assembly;

        private readonly static object _syncRoot;

        private static SQLiteConnectionFlags _sharedFlags;

        [ThreadStatic]
        private static SQLiteConnection _lastConnectionInOpen;

        private ConnectionState _connectionState;

        private string _connectionString;

        internal int _transactionLevel;

        internal int _transactionSequence;

        internal bool _noDispose;

        private bool _disposing;

        private System.Data.IsolationLevel _defaultIsolation;

        internal SQLiteEnlistment _enlistment;

        internal SQLiteDbTypeMap _typeNames;

        private SQLiteTypeCallbacksMap _typeCallbacks;

        internal SQLiteBase _sql;

        private string _dataSource;

        private byte[] _password;

        internal string _baseSchemaName;

        private SQLiteConnectionFlags _flags;

        private Dictionary<string, object> _cachedSettings;

        private DbType? _defaultDbType;

        private string _defaultTypeName;

        private string _vfsName;

        private int _defaultTimeout = 30;

        private int _busyTimeout;

        internal int _prepareRetries = 3;

        private int _progressOps;

        private bool _parseViaFramework;

        internal bool _binaryGuid;

        internal int _version;

        private SQLiteProgressCallback _progressCallback;

        private SQLiteAuthorizerCallback _authorizerCallback;

        private SQLiteUpdateCallback _updateCallback;

        private SQLiteCommitCallback _commitCallback;

        private SQLiteTraceCallback _traceCallback;

        private SQLiteRollbackCallback _rollbackCallback;

        private bool disposed;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCommit
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for getting autocommit mode.");
                }
                return this._sql.AutoCommit;
            }
        }

        public int BusyTimeout
        {
            get
            {
                this.CheckDisposed();
                return this._busyTimeout;
            }
            set
            {
                this.CheckDisposed();
                this._busyTimeout = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Changes
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for getting number of changes.");
                }
                return this._sql.Changes;
            }
        }

        public static ISQLiteConnectionPool ConnectionPool
        {
            get
            {
                return SQLiteConnectionPool.GetConnectionPool();
            }
            set
            {
                SQLiteConnectionPool.SetConnectionPool(value);
            }
        }

        [DefaultValue("")]
        [Editor("SQLite.Designer.SQLiteConnectionStringEditor, SQLite.Designer, Version=1.0.106.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [RefreshProperties(RefreshProperties.All)]
        public override string ConnectionString
        {
            get
            {
                this.CheckDisposed();
                return this._connectionString;
            }
            set
            {
                this.CheckDisposed();
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (this._connectionState != ConnectionState.Closed)
                {
                    throw new InvalidOperationException();
                }
                this._connectionString = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Database
        {
            get
            {
                this.CheckDisposed();
                return "main";
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string DataSource
        {
            get
            {
                this.CheckDisposed();
                return this._dataSource;
            }
        }

        protected override System.Data.Common.DbProviderFactory DbProviderFactory
        {
            get
            {
                return SQLiteFactory.Instance;
            }
        }

        public DbType? DefaultDbType
        {
            get
            {
                this.CheckDisposed();
                return this._defaultDbType;
            }
            set
            {
                this.CheckDisposed();
                this._defaultDbType = value;
            }
        }

        public static SQLiteConnectionFlags DefaultFlags
        {
            get
            {
                object settingValue;
                string str = "DefaultFlags_SQLiteConnection";
                if (!SQLiteConnection.TryGetLastCachedSetting(str, null, out settingValue))
                {
                    settingValue = System.Data.SQLite.UnsafeNativeMethods.GetSettingValue(str, null);
                    SQLiteConnection.SetLastCachedSetting(str, settingValue);
                }
                if (settingValue == null)
                {
                    return SQLiteConnectionFlags.Default;
                }
                object obj = SQLiteConnection.TryParseEnum(typeof(SQLiteConnectionFlags), settingValue.ToString(), true);
                if (!(obj is SQLiteConnectionFlags))
                {
                    return SQLiteConnectionFlags.Default;
                }
                return (SQLiteConnectionFlags)obj;
            }
        }

        public int DefaultTimeout
        {
            get
            {
                this.CheckDisposed();
                return this._defaultTimeout;
            }
            set
            {
                this.CheckDisposed();
                this._defaultTimeout = value;
            }
        }

        public string DefaultTypeName
        {
            get
            {
                this.CheckDisposed();
                return this._defaultTypeName;
            }
            set
            {
                this.CheckDisposed();
                this._defaultTypeName = value;
            }
        }

        public static string DefineConstants
        {
            get
            {
                return SQLite3.DefineConstants;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FileName
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for getting file name.");
                }
                return this._sql.GetFileName("main");
            }
        }

        public SQLiteConnectionFlags Flags
        {
            get
            {
                this.CheckDisposed();
                return this._flags;
            }
            set
            {
                this.CheckDisposed();
                this._flags = value;
            }
        }

        public static string InteropCompileOptions
        {
            get
            {
                return SQLite3.InteropCompileOptions;
            }
        }

        public static string InteropSourceId
        {
            get
            {
                return SQLite3.InteropSourceId;
            }
        }

        public static string InteropVersion
        {
            get
            {
                return SQLite3.InteropVersion;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long LastInsertRowId
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for getting last insert rowid.");
                }
                return this._sql.LastInsertRowId;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long MemoryHighwater
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for getting maximum memory used.");
                }
                return this._sql.MemoryHighwater;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long MemoryUsed
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for getting memory used.");
                }
                return this._sql.MemoryUsed;
            }
        }

        public bool OwnHandle
        {
            get
            {
                this.CheckDisposed();
                if (this._sql == null)
                {
                    throw new InvalidOperationException("Database connection not valid for checking handle.");
                }
                return this._sql.OwnHandle;
            }
        }

        public bool ParseViaFramework
        {
            get
            {
                this.CheckDisposed();
                return this._parseViaFramework;
            }
            set
            {
                this.CheckDisposed();
                this._parseViaFramework = value;
            }
        }

        public int PoolCount
        {
            get
            {
                if (this._sql == null)
                {
                    return 0;
                }
                return this._sql.CountPool();
            }
        }

        public int PrepareRetries
        {
            get
            {
                this.CheckDisposed();
                return this._prepareRetries;
            }
            set
            {
                this.CheckDisposed();
                this._prepareRetries = value;
            }
        }

        public int ProgressOps
        {
            get
            {
                this.CheckDisposed();
                return this._progressOps;
            }
            set
            {
                this.CheckDisposed();
                this._progressOps = value;
            }
        }

        public static string ProviderSourceId
        {
            get
            {
                if (SQLiteConnection._assembly == null)
                {
                    return null;
                }
                string sourceId = null;
                if (SQLiteConnection._assembly.IsDefined(typeof(AssemblySourceIdAttribute), false))
                {
                    AssemblySourceIdAttribute customAttributes = (AssemblySourceIdAttribute)SQLiteConnection._assembly.GetCustomAttributes(typeof(AssemblySourceIdAttribute), false)[0];
                    sourceId = customAttributes.SourceId;
                }
                string sourceTimeStamp = null;
                if (SQLiteConnection._assembly.IsDefined(typeof(AssemblySourceTimeStampAttribute), false))
                {
                    AssemblySourceTimeStampAttribute assemblySourceTimeStampAttribute = (AssemblySourceTimeStampAttribute)SQLiteConnection._assembly.GetCustomAttributes(typeof(AssemblySourceTimeStampAttribute), false)[0];
                    sourceTimeStamp = assemblySourceTimeStampAttribute.SourceTimeStamp;
                }
                if (sourceId == null && sourceTimeStamp == null)
                {
                    return null;
                }
                if (sourceId == null)
                {
                    sourceId = "0000000000000000000000000000000000000000";
                }
                if (sourceTimeStamp == null)
                {
                    sourceTimeStamp = "0000-00-00 00:00:00 UTC";
                }
                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                object[] objArray = new object[] { sourceId, sourceTimeStamp };
                return HelperMethods.StringFormat(invariantCulture, "{0} {1}", objArray);
            }
        }

        public static string ProviderVersion
        {
            get
            {
                if (SQLiteConnection._assembly == null)
                {
                    return null;
                }
                return SQLiteConnection._assembly.GetName().Version.ToString();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string ServerVersion
        {
            get
            {
                this.CheckDisposed();
                return SQLiteConnection.SQLiteVersion;
            }
        }

        public static SQLiteConnectionFlags SharedFlags
        {
            get
            {
                SQLiteConnectionFlags sQLiteConnectionFlag;
                lock (SQLiteConnection._syncRoot)
                {
                    sQLiteConnectionFlag = SQLiteConnection._sharedFlags;
                }
                return sQLiteConnectionFlag;
            }
            set
            {
                lock (SQLiteConnection._syncRoot)
                {
                    SQLiteConnection._sharedFlags = value;
                }
            }
        }

        public static string SQLiteCompileOptions
        {
            get
            {
                return SQLite3.SQLiteCompileOptions;
            }
        }

        public static string SQLiteSourceId
        {
            get
            {
                return SQLite3.SQLiteSourceId;
            }
        }

        public static string SQLiteVersion
        {
            get
            {
                return SQLite3.SQLiteVersion;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ConnectionState State
        {
            get
            {
                this.CheckDisposed();
                return this._connectionState;
            }
        }

        public string VfsName
        {
            get
            {
                this.CheckDisposed();
                return this._vfsName;
            }
            set
            {
                this.CheckDisposed();
                this._vfsName = value;
            }
        }

        static SQLiteConnection()
        {
            SQLiteConnection._assembly = typeof(SQLiteConnection).Assembly;
            SQLiteConnection._syncRoot = new object();
        }

        public SQLiteConnection() : this((string)null)
        {
        }

        public SQLiteConnection(string connectionString) : this(connectionString, false)
        {
        }

        internal SQLiteConnection(IntPtr db, string fileName, bool ownHandle) : this()
        {
            this._sql = new SQLite3(SQLiteDateFormats.ISO8601, DateTimeKind.Unspecified, null, db, fileName, ownHandle);
            this._flags = SQLiteConnectionFlags.None;
            this._connectionState = (db != IntPtr.Zero ? ConnectionState.Open : ConnectionState.Closed);
            this._connectionString = null;
        }

        public SQLiteConnection(string connectionString, bool parseViaFramework)
        {
            this._noDispose = false;
            System.Data.SQLite.UnsafeNativeMethods.Initialize();
            SQLiteLog.Initialize();
            this._cachedSettings = new Dictionary<string, object>(new TypeNameStringComparer());
            this._typeNames = new SQLiteDbTypeMap();
            this._typeCallbacks = new SQLiteTypeCallbacksMap();
            this._parseViaFramework = parseViaFramework;
            this._flags = SQLiteConnectionFlags.None;
            this._defaultDbType = null;
            this._defaultTypeName = null;
            this._vfsName = null;
            this._connectionState = ConnectionState.Closed;
            this._connectionString = null;
            if (connectionString != null)
            {
                this.ConnectionString = connectionString;
            }
        }

        public SQLiteConnection(SQLiteConnection connection) : this(connection.ConnectionString, connection.ParseViaFramework)
        {
            if (connection.State == ConnectionState.Open)
            {
                this.Open();
                using (DataTable schema = connection.GetSchema("Catalogs"))
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        string str = row[0].ToString();
                        if (string.Compare(str, "main", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(str, "temp", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            continue;
                        }
                        using (SQLiteCommand sQLiteCommand = this.CreateCommand())
                        {
                            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                            object[] item = new object[] { row[1], row[0] };
                            sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture, "ATTACH DATABASE '{0}' AS [{1}]", item);
                            sQLiteCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public int AddTypeMapping(string typeName, DbType dataType, bool primary)
        {
            this.CheckDisposed();
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            int num = -1;
            if (this._typeNames != null)
            {
                num = 0;
                if (primary && this._typeNames.ContainsKey(dataType))
                {
                    num = num + (this._typeNames.Remove(dataType) ? 1 : 0);
                }
                if (this._typeNames.ContainsKey(typeName))
                {
                    num = num + (this._typeNames.Remove(typeName) ? 1 : 0);
                }
                this._typeNames.Add(new SQLiteDbTypeMapping(typeName, dataType, primary));
            }
            return num;
        }

        private SQLiteAuthorizerReturnCode AuthorizerCallback(IntPtr pUserData, SQLiteAuthorizerActionCode actionCode, IntPtr pArgument1, IntPtr pArgument2, IntPtr pDatabase, IntPtr pAuthContext)
        {
            SQLiteAuthorizerReturnCode returnCode;
            try
            {
                AuthorizerEventArgs authorizerEventArg = new AuthorizerEventArgs(pUserData, actionCode, SQLiteConvert.UTF8ToString(pArgument1, -1), SQLiteConvert.UTF8ToString(pArgument2, -1), SQLiteConvert.UTF8ToString(pDatabase, -1), SQLiteConvert.UTF8ToString(pAuthContext, -1), SQLiteAuthorizerReturnCode.Ok);
                if (this._authorizerHandler != null)
                {
                    this._authorizerHandler(this, authorizerEventArg);
                }
                returnCode = authorizerEventArg.ReturnCode;
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (HelperMethods.LogCallbackExceptions(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { "Authorize", exception };
                        SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
                    }
                }
                catch
                {
                }
                if ((this._flags & SQLiteConnectionFlags.DenyOnException) == SQLiteConnectionFlags.DenyOnException)
                {
                    return SQLiteAuthorizerReturnCode.Deny;
                }
                return SQLiteAuthorizerReturnCode.Ok;
            }
            return returnCode;
        }

        public void BackupDatabase(SQLiteConnection destination, string destinationName, string sourceName, int pages, SQLiteBackupCallback callback, int retryMilliseconds)
        {
            this.CheckDisposed();
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException("Source database is not open.");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (destination._connectionState != ConnectionState.Open)
            {
                throw new ArgumentException("Destination database is not open.", "destination");
            }
            if (destinationName == null)
            {
                throw new ArgumentNullException("destinationName");
            }
            if (sourceName == null)
            {
                throw new ArgumentNullException("sourceName");
            }
            SQLiteBase sQLiteBase = this._sql;
            if (sQLiteBase == null)
            {
                throw new InvalidOperationException("Connection object has an invalid handle.");
            }
            SQLiteBackup sQLiteBackup = null;
            try
            {
                try
                {
                    sQLiteBackup = sQLiteBase.InitializeBackup(destination, destinationName, sourceName);
                    bool flag = false;
                    do
                    {
                        if (!sQLiteBase.StepBackup(sQLiteBackup, pages, ref flag) || callback != null && !callback(this, sourceName, destination, destinationName, pages, sQLiteBase.RemainingBackup(sQLiteBackup), sQLiteBase.PageCountBackup(sQLiteBackup), flag))
                        {
                            break;
                        }
                        if (!flag || retryMilliseconds < 0)
                        {
                            continue;
                        }
                        Thread.Sleep(retryMilliseconds);
                    }
                    while (pages != 0);
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    if (HelperMethods.LogBackup(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { exception };
                        SQLiteLog.LogMessage(HelperMethods.StringFormat(currentCulture, "Caught exception while backing up database: {0}", objArray));
                    }
                    throw;
                }
            }
            finally
            {
                if (sQLiteBackup != null)
                {
                    sQLiteBase.FinishBackup(sQLiteBackup);
                }
            }
        }

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            SQLiteTransaction sQLiteTransaction;
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException();
            }
            if (isolationLevel == System.Data.IsolationLevel.Unspecified)
            {
                isolationLevel = this._defaultIsolation;
            }
            isolationLevel = this.GetEffectiveIsolationLevel(isolationLevel);
            if (isolationLevel != System.Data.IsolationLevel.Serializable && isolationLevel != System.Data.IsolationLevel.ReadCommitted)
            {
                throw new ArgumentException("isolationLevel");
            }
            if ((this._flags & SQLiteConnectionFlags.AllowNestedTransactions) != SQLiteConnectionFlags.AllowNestedTransactions)
            {
                sQLiteTransaction = new SQLiteTransaction(this, isolationLevel != System.Data.IsolationLevel.Serializable);
            }
            else
            {
                sQLiteTransaction = new SQLiteTransaction2(this, isolationLevel != System.Data.IsolationLevel.Serializable);
            }
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.NewTransaction, null, sQLiteTransaction, null, null, null, null, null));
            return sQLiteTransaction;
        }

        [Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
        public SQLiteTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel, bool deferredLock)
        {
            this.CheckDisposed();
            return (SQLiteTransaction)this.BeginDbTransaction((!deferredLock ? System.Data.IsolationLevel.Serializable : System.Data.IsolationLevel.ReadCommitted));
        }

        [Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
        public SQLiteTransaction BeginTransaction(bool deferredLock)
        {
            this.CheckDisposed();
            return (SQLiteTransaction)this.BeginDbTransaction((!deferredLock ? System.Data.IsolationLevel.Serializable : System.Data.IsolationLevel.ReadCommitted));
        }

        public new SQLiteTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            this.CheckDisposed();
            return (SQLiteTransaction)this.BeginDbTransaction(isolationLevel);
        }

        public new SQLiteTransaction BeginTransaction()
        {
            this.CheckDisposed();
            return (SQLiteTransaction)this.BeginDbTransaction(this._defaultIsolation);
        }

        public void BindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteFunction function)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for binding functions.");
            }
            this._sql.BindFunction(functionAttribute, function, this._flags);
        }

        public void BindFunction(SQLiteFunctionAttribute functionAttribute, Delegate callback1, Delegate callback2)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for binding functions.");
            }
            this._sql.BindFunction(functionAttribute, new SQLiteDelegateFunction(callback1, callback2), this._flags);
        }

        public void Cancel()
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for query cancellation.");
            }
            this._sql.Cancel();
        }

        public override void ChangeDatabase(string databaseName)
        {
            this.CheckDisposed();
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.ChangeDatabase, null, null, null, null, null, databaseName, null));
            throw new NotImplementedException();
        }

        public void ChangePassword(string newPassword)
        {
            byte[] bytes;
            this.CheckDisposed();
            if (string.IsNullOrEmpty(newPassword))
            {
                bytes = null;
            }
            else
            {
                bytes = Encoding.UTF8.GetBytes(newPassword);
            }
            this.ChangePassword(bytes);
        }

        public void ChangePassword(byte[] newPassword)
        {
            this.CheckDisposed();
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database must be opened before changing the password.");
            }
            this._sql.ChangePassword(newPassword);
        }

        [Conditional("CHECK_STATE")]
        internal static void Check(SQLiteConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.CheckDisposed();
            if (connection._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open.");
            }
            SQLite3 sQLite3 = connection._sql as SQLite3;
            if (sQLite3 == null)
            {
                throw new InvalidOperationException("The connection handle wrapper is null.");
            }
            SQLiteConnectionHandle sQLiteConnectionHandle = sQLite3._sql;
            if (sQLiteConnectionHandle == null)
            {
                throw new InvalidOperationException("The connection handle is null.");
            }
            if (sQLiteConnectionHandle.IsInvalid)
            {
                throw new InvalidOperationException("The connection handle is invalid.");
            }
            if (sQLiteConnectionHandle.IsClosed)
            {
                throw new InvalidOperationException("The connection handle is closed.");
            }
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(typeof(SQLiteConnection).Name);
            }
        }

        public static void ClearAllPools()
        {
            SQLiteConnectionPool.ClearAllPools();
        }

        public int ClearCachedSettings()
        {
            this.CheckDisposed();
            int count = -1;
            if (this._cachedSettings != null)
            {
                count = this._cachedSettings.Count;
                this._cachedSettings.Clear();
            }
            return count;
        }

        public static void ClearPool(SQLiteConnection connection)
        {
            if (connection._sql == null)
            {
                return;
            }
            connection._sql.ClearPool();
        }

        public int ClearTypeCallbacks()
        {
            this.CheckDisposed();
            int count = -1;
            if (this._typeCallbacks != null)
            {
                count = this._typeCallbacks.Count;
                this._typeCallbacks.Clear();
            }
            return count;
        }

        public int ClearTypeMappings()
        {
            this.CheckDisposed();
            int num = -1;
            if (this._typeNames != null)
            {
                num = this._typeNames.Clear();
            }
            return num;
        }

        public object Clone()
        {
            this.CheckDisposed();
            return new SQLiteConnection(this);
        }

        public override void Close()
        {
            this.CheckDisposed();
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Closing, null, null, null, null, null, null, null));
            if (this._sql != null)
            {
                if (this._enlistment != null)
                {
                    SQLiteConnection sQLiteConnection = new SQLiteConnection()
                    {
                        _sql = this._sql,
                        _transactionLevel = this._transactionLevel,
                        _transactionSequence = this._transactionSequence,
                        _enlistment = this._enlistment,
                        _connectionState = this._connectionState,
                        _version = this._version
                    };
                    sQLiteConnection._enlistment._transaction._cnn = sQLiteConnection;
                    sQLiteConnection._enlistment._disposeConnection = true;
                    this._sql = null;
                    this._enlistment = null;
                }
                if (this._sql != null)
                {
                    this._sql.Close(!this._disposing);
                    this._sql = null;
                }
                this._transactionLevel = 0;
                this._transactionSequence = 0;
            }
            StateChangeEventArgs stateChangeEventArg = null;
            this.OnStateChange(ConnectionState.Closed, ref stateChangeEventArg);
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Closed, stateChangeEventArg, null, null, null, null, null, null));
        }

        private int CommitCallback(IntPtr parg)
        {
            int num;
            try
            {
                CommitEventArgs commitEventArg = new CommitEventArgs();
                if (this._commitHandler != null)
                {
                    this._commitHandler(this, commitEventArg);
                }
                num = (commitEventArg.AbortTransaction ? 1 : 0);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (HelperMethods.LogCallbackExceptions(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { "Commit", exception };
                        SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
                    }
                }
                catch
                {
                }
                if ((this._flags & SQLiteConnectionFlags.RollbackOnException) == SQLiteConnectionFlags.RollbackOnException)
                {
                    return 1;
                }
                return 0;
            }
            return num;
        }

        public ISQLiteChangeGroup CreateChangeGroup()
        {
            this.CheckDisposed();
            return new SQLiteChangeGroup(this._flags);
        }

        public ISQLiteChangeSet CreateChangeSet(byte[] rawData)
        {
            this.CheckDisposed();
            return new SQLiteMemoryChangeSet(rawData, SQLiteConnection.GetNativeHandle(this), this._flags);
        }

        public ISQLiteChangeSet CreateChangeSet(Stream inputStream, Stream outputStream)
        {
            this.CheckDisposed();
            return new SQLiteStreamChangeSet(inputStream, outputStream, SQLiteConnection.GetNativeHandle(this), this._flags);
        }

        public new SQLiteCommand CreateCommand()
        {
            this.CheckDisposed();
            return new SQLiteCommand(this);
        }

        protected override DbCommand CreateDbCommand()
        {
            return this.CreateCommand();
        }

        public static void CreateFile(string databaseFileName)
        {
            File.Create(databaseFileName).Close();
        }

        public static object CreateHandle(IntPtr nativeHandle)
        {
            SQLiteConnectionHandle sQLiteConnectionHandle = null;
            SQLiteConnectionHandle sQLiteConnectionHandle1;
            try
            {
            }
            finally
            {
                if (nativeHandle != IntPtr.Zero)
                {
                    sQLiteConnectionHandle1 = new SQLiteConnectionHandle(nativeHandle, true);
                }
                else
                {
                    sQLiteConnectionHandle1 = null;
                }
                sQLiteConnectionHandle = sQLiteConnectionHandle1;
            }
            if (sQLiteConnectionHandle != null)
            {
                object[] objArray = new object[] { typeof(SQLiteConnection), nativeHandle };
                SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteConnectionHandle, null, objArray));
            }
            return sQLiteConnectionHandle;
        }

        public void CreateModule(SQLiteModule module)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for creating modules.");
            }
            if ((this._flags & SQLiteConnectionFlags.NoCreateModule) == SQLiteConnectionFlags.NoCreateModule)
            {
                throw new SQLiteException("Creating modules is disabled for this database connection.");
            }
            this._sql.CreateModule(module, this._flags);
        }

        public ISQLiteSession CreateSession(string databaseName)
        {
            this.CheckDisposed();
            return new SQLiteSession(SQLiteConnection.GetNativeHandle(this), this._flags, databaseName);
        }

        public new void Dispose()
        {
            if (this._noDispose)
            {
                return;
            }
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if ((this._flags & SQLiteConnectionFlags.TraceWarning) == SQLiteConnectionFlags.TraceWarning && this._noDispose)
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                object[] objArray = new object[] { this._connectionString };
                System.Diagnostics.Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "WARNING: Disposing of connection \"{0}\" with the no-dispose flag set.", objArray));
            }
            this._disposing = true;
            try
            {
                if (!this.disposed)
                {
                    this.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
                this.disposed = true;
            }
        }

        public void EnableExtensions(bool enable)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                object[] objArray = new object[] { (enable ? "enabling" : "disabling") };
                throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "Database connection not valid for {0} extensions.", objArray));
            }
            if ((this._flags & SQLiteConnectionFlags.NoLoadExtension) == SQLiteConnectionFlags.NoLoadExtension)
            {
                throw new SQLiteException("Loading extensions is disabled for this database connection.");
            }
            this._sql.SetLoadExtension(enable);
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            this.CheckDisposed();
            if (this._enlistment != null && transaction == this._enlistment._scope)
            {
                return;
            }
            if (this._enlistment != null)
            {
                throw new ArgumentException("Already enlisted in a transaction");
            }
            if (this._transactionLevel > 0 && transaction != null)
            {
                throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("Unable to enlist in transaction, it is null");
            }
            bool flag = (this._flags & SQLiteConnectionFlags.StrictEnlistment) == SQLiteConnectionFlags.StrictEnlistment;
            this._enlistment = new SQLiteEnlistment(this, transaction, SQLiteConnection.GetFallbackDefaultIsolationLevel(), flag, flag);
            object[] objArray = new object[] { this._enlistment };
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.EnlistTransaction, null, null, null, null, null, null, objArray));
        }

        private static string ExpandFileName(string sourceFile, bool toFullPath)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                return sourceFile;
            }
            if (sourceFile.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                string dataDirectory = SQLiteConnection.GetDataDirectory();
                if (sourceFile.Length > "|DataDirectory|".Length && (sourceFile["|DataDirectory|".Length] == Path.DirectorySeparatorChar || sourceFile["|DataDirectory|".Length] == Path.AltDirectorySeparatorChar))
                {
                    sourceFile = sourceFile.Remove("|DataDirectory|".Length, 1);
                }
                sourceFile = Path.Combine(dataDirectory, sourceFile.Substring("|DataDirectory|".Length));
            }
            if (toFullPath)
            {
                sourceFile = Path.GetFullPath(sourceFile);
            }
            return sourceFile;
        }

        public SQLiteErrorCode ExtendedResultCode()
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for getting extended result code.");
            }
            return this._sql.ExtendedResultCode();
        }

        internal static string FindKey(SortedList<string, string> items, string key, string defValue)
        {
            string str;
            if (string.IsNullOrEmpty(key))
            {
                return defValue;
            }
            if (items.TryGetValue(key, out str))
            {
                return str;
            }
            if (items.TryGetValue(key.Replace(" ", string.Empty), out str))
            {
                return str;
            }
            if (items.TryGetValue(key.Replace(" ", "_"), out str))
            {
                return str;
            }
            return defValue;
        }

        internal static byte[] FromHexString(string text)
        {
            string str = null;
            return SQLiteConnection.FromHexString(text, ref str);
        }

        private static byte[] FromHexString(string text, ref string error)
        {
            if (text == null)
            {
                error = "string is null";
                return null;
            }
            if (text.Length % 2 != 0)
            {
                error = "string contains an odd number of characters";
                return null;
            }
            byte[] numArray = new byte[text.Length / 2];
            for (int i = 0; i < text.Length; i += 2)
            {
                string str = text.Substring(i, 2);
                if (!SQLiteConnection.TryParseByte(str, NumberStyles.HexNumber, out numArray[i / 2]))
                {
                    CultureInfo currentCulture = CultureInfo.CurrentCulture;
                    object[] objArray = new object[] { str };
                    error = HelperMethods.StringFormat(currentCulture, "string contains \"{0}\", which cannot be converted to a byte value", objArray);
                    return null;
                }
            }
            return numArray;
        }

        private static string GetDataDirectory()
        {
            string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(data))
            {
                data = AppDomain.CurrentDomain.BaseDirectory;
            }
            return data;
        }

        internal System.Data.IsolationLevel GetDefaultIsolationLevel()
        {
            return this._defaultIsolation;
        }

        private bool GetDefaultPooling()
        {
            bool flag = false;
            if (!flag)
            {
                if ((this._flags & SQLiteConnectionFlags.UseConnectionPool) == SQLiteConnectionFlags.UseConnectionPool)
                {
                    flag = true;
                }
                if ((this._flags & SQLiteConnectionFlags.NoConnectionPool) == SQLiteConnectionFlags.NoConnectionPool)
                {
                    flag = false;
                }
            }
            else
            {
                if ((this._flags & SQLiteConnectionFlags.NoConnectionPool) == SQLiteConnectionFlags.NoConnectionPool)
                {
                    flag = false;
                }
                if ((this._flags & SQLiteConnectionFlags.UseConnectionPool) == SQLiteConnectionFlags.UseConnectionPool)
                {
                    flag = true;
                }
            }
            return flag;
        }

        private System.Data.IsolationLevel GetEffectiveIsolationLevel(System.Data.IsolationLevel isolationLevel)
        {
            if ((this._flags & SQLiteConnectionFlags.MapIsolationLevels) != SQLiteConnectionFlags.MapIsolationLevels)
            {
                return isolationLevel;
            }
            System.Data.IsolationLevel isolationLevel1 = isolationLevel;
            if (isolationLevel1 > System.Data.IsolationLevel.ReadUncommitted)
            {
                if (isolationLevel1 > System.Data.IsolationLevel.RepeatableRead)
                {
                    if (isolationLevel1 == System.Data.IsolationLevel.Serializable || isolationLevel1 == System.Data.IsolationLevel.Snapshot)
                    {
                        return System.Data.IsolationLevel.Serializable;
                    }
                    return SQLiteConnection.GetFallbackDefaultIsolationLevel();
                }
                else
                {
                    if (isolationLevel1 == System.Data.IsolationLevel.ReadCommitted)
                    {
                        return System.Data.IsolationLevel.ReadCommitted;
                    }
                    if (isolationLevel1 == System.Data.IsolationLevel.RepeatableRead)
                    {
                        return System.Data.IsolationLevel.Serializable;
                    }
                    return SQLiteConnection.GetFallbackDefaultIsolationLevel();
                }
                return System.Data.IsolationLevel.Serializable;
            }
            else
            {
                if (isolationLevel1 == System.Data.IsolationLevel.Unspecified || isolationLevel1 == System.Data.IsolationLevel.Chaos || isolationLevel1 == System.Data.IsolationLevel.ReadUncommitted)
                {
                    return System.Data.IsolationLevel.ReadCommitted;
                }
                return SQLiteConnection.GetFallbackDefaultIsolationLevel();
            }
            return System.Data.IsolationLevel.ReadCommitted;
        }

        private static System.Data.IsolationLevel GetFallbackDefaultIsolationLevel()
        {
            return System.Data.IsolationLevel.Serializable;
        }

        public static void GetMemoryStatistics(ref IDictionary<string, long> statistics)
        {
            if (statistics == null)
            {
                statistics = new Dictionary<string, long>();
            }
            statistics["MemoryUsed"] = SQLite3.StaticMemoryUsed;
            statistics["MemoryHighwater"] = SQLite3.StaticMemoryHighwater;
        }

        private static SQLiteConnectionHandle GetNativeHandle(SQLiteConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SQLite3 sQLite3 = connection._sql as SQLite3;
            if (sQLite3 == null)
            {
                throw new InvalidOperationException("Connection has no wrapper");
            }
            SQLiteConnectionHandle sQLiteConnectionHandle = sQLite3._sql;
            if (sQLiteConnectionHandle == null)
            {
                throw new InvalidOperationException("Connection has an invalid handle.");
            }
            if (sQLiteConnectionHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Connection has an invalid handle pointer.");
            }
            return sQLiteConnectionHandle;
        }

        public override DataTable GetSchema()
        {
            this.CheckDisposed();
            return this.GetSchema("MetaDataCollections", null);
        }

        public override DataTable GetSchema(string collectionName)
        {
            this.CheckDisposed();
            return this.GetSchema(collectionName, new string[0]);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            this.CheckDisposed();
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException();
            }
            string[] strArrays = new string[5];
            if (restrictionValues == null)
            {
                restrictionValues = new string[0];
            }
            restrictionValues.CopyTo(strArrays, 0);
            string upper = collectionName.ToUpper(CultureInfo.InvariantCulture);
            string str = upper;
            if (upper != null)
            {
                switch (str)
                {
                    case "METADATACOLLECTIONS":
                        {
                            return SQLiteConnection.Schema_MetaDataCollections();
                        }
                    case "DATASOURCEINFORMATION":
                        {
                            return this.Schema_DataSourceInformation();
                        }
                    case "DATATYPES":
                        {
                            return this.Schema_DataTypes();
                        }
                    case "COLUMNS":
                    case "TABLECOLUMNS":
                        {
                            return this.Schema_Columns(strArrays[0], strArrays[2], strArrays[3]);
                        }
                    case "INDEXES":
                        {
                            return this.Schema_Indexes(strArrays[0], strArrays[2], strArrays[3]);
                        }
                    case "TRIGGERS":
                        {
                            return this.Schema_Triggers(strArrays[0], strArrays[2], strArrays[3]);
                        }
                    case "INDEXCOLUMNS":
                        {
                            return this.Schema_IndexColumns(strArrays[0], strArrays[2], strArrays[3], strArrays[4]);
                        }
                    case "TABLES":
                        {
                            return this.Schema_Tables(strArrays[0], strArrays[2], strArrays[3]);
                        }
                    case "VIEWS":
                        {
                            return this.Schema_Views(strArrays[0], strArrays[2]);
                        }
                    case "VIEWCOLUMNS":
                        {
                            return this.Schema_ViewColumns(strArrays[0], strArrays[2], strArrays[3]);
                        }
                    case "FOREIGNKEYS":
                        {
                            return this.Schema_ForeignKeys(strArrays[0], strArrays[2], strArrays[3]);
                        }
                    case "CATALOGS":
                        {
                            return this.Schema_Catalogs(strArrays[0]);
                        }
                    case "RESERVEDWORDS":
                        {
                            return SQLiteConnection.Schema_ReservedWords();
                        }
                }
            }
            throw new NotSupportedException();
        }

        public Dictionary<string, object> GetTypeMappings()
        {
            this.CheckDisposed();
            Dictionary<string, object> strs = null;
            if (this._typeNames != null)
            {
                strs = new Dictionary<string, object>(this._typeNames.Count, this._typeNames.Comparer);
                foreach (KeyValuePair<string, SQLiteDbTypeMapping> _typeName in this._typeNames)
                {
                    SQLiteDbTypeMapping value = _typeName.Value;
                    object obj = null;
                    object obj1 = null;
                    object obj2 = null;
                    if (value != null)
                    {
                        obj = value.typeName;
                        obj1 = value.dataType;
                        obj2 = value.primary;
                    }
                    string key = _typeName.Key;
                    object[] objArray = new object[] { obj, obj1, obj2 };
                    strs.Add(key, objArray);
                }
            }
            return strs;
        }

        public bool IsReadOnly(string name)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for checking read-only status.");
            }
            return this._sql.IsReadOnly(name);
        }

        public void LoadExtension(string fileName)
        {
            this.CheckDisposed();
            this.LoadExtension(fileName, null);
        }

        public void LoadExtension(string fileName, string procName)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for loading extensions.");
            }
            if ((this._flags & SQLiteConnectionFlags.NoLoadExtension) == SQLiteConnectionFlags.NoLoadExtension)
            {
                throw new SQLiteException("Loading extensions is disabled for this database connection.");
            }
            this._sql.LoadExtension(fileName, procName);
        }

        public void LogMessage(SQLiteErrorCode iErrCode, string zMessage)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for logging message.");
            }
            this._sql.LogMessage(iErrCode, zMessage);
        }

        public void LogMessage(int iErrCode, string zMessage)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for logging message.");
            }
            this._sql.LogMessage((SQLiteErrorCode)iErrCode, zMessage);
        }

        internal static string MapUriPath(string path)
        {
            if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(7);
            }
            if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(5);
            }
            if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid connection string: invalid URI");
            }
            return path;
        }

        internal static void OnChanged(SQLiteConnection connection, ConnectionEventArgs e)
        {
            SQLiteConnectionEventHandler sQLiteConnectionEventHandler;
            if (connection != null && !connection.CanRaiseEvents)
            {
                return;
            }
            lock (SQLiteConnection._syncRoot)
            {
                if (SQLiteConnection._handlers == null)
                {
                    sQLiteConnectionEventHandler = null;
                }
                else
                {
                    sQLiteConnectionEventHandler = SQLiteConnection._handlers.Clone() as SQLiteConnectionEventHandler;
                }
            }
            if (sQLiteConnectionEventHandler != null)
            {
                sQLiteConnectionEventHandler(connection, e);
            }
        }

        internal void OnStateChange(ConnectionState newState, ref StateChangeEventArgs eventArgs)
        {
            ConnectionState connectionState = this._connectionState;
            this._connectionState = newState;
            if (this.StateChange != null && newState != connectionState)
            {
                StateChangeEventArgs stateChangeEventArg = new StateChangeEventArgs(connectionState, newState);
                this.StateChange(this, stateChangeEventArg);
                eventArgs = stateChangeEventArg;
            }
        }

        public override void Open()
        {
            int num;
            DbType? nullable;
            this.CheckDisposed();
            SQLiteConnection._lastConnectionInOpen = this;
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Opening, null, null, null, null, null, null, null));
            if (this._connectionState != ConnectionState.Closed)
            {
                throw new InvalidOperationException();
            }
            this.Close();
            SortedList<string, string> strs = SQLiteConnection.ParseConnectionString(this, this._connectionString, this._parseViaFramework, false);
            string str = this._connectionString;
            object[] objArray = new object[] { strs };
            SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.ConnectionString, null, null, null, null, null, str, objArray));
            object obj = SQLiteConnection.TryParseEnum(typeof(SQLiteConnectionFlags), SQLiteConnection.FindKey(strs, "Flags", null), true);
            bool flag = false;
            bool flag1 = SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "NoDefaultFlags", flag.ToString()));
            if (obj is SQLiteConnectionFlags)
            {
                this._flags |= (SQLiteConnectionFlags)obj;
            }
            else if (!flag1)
            {
                this._flags |= SQLiteConnection.DefaultFlags;
            }
            if (!SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "NoSharedFlags", false.ToString())))
            {
                lock (SQLiteConnection._syncRoot)
                {
                    this._flags |= SQLiteConnection._sharedFlags;
                }
            }
            obj = SQLiteConnection.TryParseEnum(typeof(DbType), SQLiteConnection.FindKey(strs, "DefaultDbType", null), true);
            if (obj as DbType? != null && obj as DbType? != DbType.AnsiString)
            {
                nullable = new DbType?((DbType)obj);
            }
            else
            {
                nullable = null;
            }
            this._defaultDbType = nullable;
            if (this._defaultDbType.HasValue && this._defaultDbType.Value == (DbType.Binary | DbType.Byte | DbType.Boolean | DbType.Currency | DbType.Date | DbType.DateTime | DbType.Decimal | DbType.Double | DbType.Guid | DbType.Int16 | DbType.Int32 | DbType.Int64 | DbType.Object | DbType.SByte | DbType.Single | DbType.String | DbType.Time | DbType.UInt16 | DbType.UInt32 | DbType.UInt64 | DbType.VarNumeric | DbType.AnsiStringFixedLength | DbType.StringFixedLength | DbType.Xml | DbType.DateTime2 | DbType.DateTimeOffset))
            {
                this._defaultDbType = null;
            }
            this._defaultTypeName = SQLiteConnection.FindKey(strs, "DefaultTypeName", null);
            this._vfsName = SQLiteConnection.FindKey(strs, "VfsName", null);
            bool flag2 = false;
            bool flag3 = false;
            if (Convert.ToInt32(SQLiteConnection.FindKey(strs, "Version", SQLiteConvert.ToString(3)), CultureInfo.InvariantCulture) != 3)
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                object[] objArray1 = new object[] { 3 };
                throw new NotSupportedException(HelperMethods.StringFormat(currentCulture, "Only SQLite Version {0} is supported at this time", objArray1));
            }
            string str1 = SQLiteConnection.FindKey(strs, "Data Source", null);
            if (string.IsNullOrEmpty(str1))
            {
                str1 = SQLiteConnection.FindKey(strs, "Uri", null);
                if (!string.IsNullOrEmpty(str1))
                {
                    str1 = SQLiteConnection.MapUriPath(str1);
                    flag2 = true;
                }
                else
                {
                    str1 = SQLiteConnection.FindKey(strs, "FullUri", null);
                    if (string.IsNullOrEmpty(str1))
                    {
                        CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                        object[] objArray2 = new object[] { ":memory:" };
                        throw new ArgumentException(HelperMethods.StringFormat(cultureInfo, "Data Source cannot be empty.  Use {0} to open an in-memory database", objArray2));
                    }
                    flag3 = true;
                }
            }
            bool flag4 = string.Compare(str1, ":memory:", StringComparison.OrdinalIgnoreCase) == 0;
            if ((this._flags & SQLiteConnectionFlags.TraceWarning) == SQLiteConnectionFlags.TraceWarning && !flag2 && !flag3 && !flag4 && !string.IsNullOrEmpty(str1) && str1.StartsWith("\\", StringComparison.OrdinalIgnoreCase) && !str1.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
            {
                CultureInfo currentCulture1 = CultureInfo.CurrentCulture;
                object[] objArray3 = new object[] { str1 };
                System.Diagnostics.Trace.WriteLine(HelperMethods.StringFormat(currentCulture1, "WARNING: Detected a possibly malformed UNC database file name \"{0}\" that may have originally started with two backslashes; however, four leading backslashes may be required, e.g.: \"Data Source=\\\\\\{0};\"", objArray3));
            }
            if (!flag3)
            {
                if (!flag4)
                {
                    bool flag5 = true;
                    bool flag6 = SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "ToFullPath", flag5.ToString()));
                    str1 = SQLiteConnection.ExpandFileName(str1, flag6);
                }
                else
                {
                    str1 = ":memory:";
                }
            }
            try
            {
                bool defaultPooling = this.GetDefaultPooling();
                bool flag7 = SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "Pooling", defaultPooling.ToString()));
                int num1 = Convert.ToInt32(SQLiteConnection.FindKey(strs, "Max Pool Size", SQLiteConvert.ToString(100)), CultureInfo.InvariantCulture);
                this._defaultTimeout = Convert.ToInt32(SQLiteConnection.FindKey(strs, "Default Timeout", SQLiteConvert.ToString(30)), CultureInfo.InvariantCulture);
                this._busyTimeout = Convert.ToInt32(SQLiteConnection.FindKey(strs, "BusyTimeout", SQLiteConvert.ToString(0)), CultureInfo.InvariantCulture);
                this._prepareRetries = Convert.ToInt32(SQLiteConnection.FindKey(strs, "PrepareRetries", SQLiteConvert.ToString(3)), CultureInfo.InvariantCulture);
                this._progressOps = Convert.ToInt32(SQLiteConnection.FindKey(strs, "ProgressOps", SQLiteConvert.ToString(0)), CultureInfo.InvariantCulture);
                obj = SQLiteConnection.TryParseEnum(typeof(System.Data.IsolationLevel), SQLiteConnection.FindKey(strs, "Default IsolationLevel", System.Data.IsolationLevel.Serializable.ToString()), true);
                this._defaultIsolation = (obj is System.Data.IsolationLevel ? (System.Data.IsolationLevel)obj : System.Data.IsolationLevel.Serializable);
                this._defaultIsolation = this.GetEffectiveIsolationLevel(this._defaultIsolation);
                if (this._defaultIsolation != System.Data.IsolationLevel.Serializable && this._defaultIsolation != System.Data.IsolationLevel.ReadCommitted)
                {
                    throw new NotSupportedException("Invalid Default IsolationLevel specified");
                }
                this._baseSchemaName = SQLiteConnection.FindKey(strs, "BaseSchemaName", "sqlite_default_schema");
                if (this._sql == null)
                {
                    this.SetupSQLiteBase(strs);
                }
                SQLiteOpenFlagsEnum sQLiteOpenFlagsEnum = SQLiteOpenFlagsEnum.None;
                if (!SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "FailIfMissing", false.ToString())))
                {
                    sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.Create;
                }
                if (!SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "Read Only", false.ToString())))
                {
                    sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.ReadWrite;
                }
                else
                {
                    sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.ReadOnly;
                    sQLiteOpenFlagsEnum = sQLiteOpenFlagsEnum & (SQLiteOpenFlagsEnum.ReadOnly | SQLiteOpenFlagsEnum.ReadWrite | SQLiteOpenFlagsEnum.Uri | SQLiteOpenFlagsEnum.Memory);
                }
                if (flag3)
                {
                    sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.Uri;
                }
                this._sql.Open(str1, this._vfsName, this._flags, sQLiteOpenFlagsEnum, num1, flag7);
                bool flag8 = true;
                this._binaryGuid = SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "BinaryGUID", flag8.ToString()));
                string str2 = SQLiteConnection.FindKey(strs, "HexPassword", null);
                if (str2 == null)
                {
                    string str3 = SQLiteConnection.FindKey(strs, "Password", null);
                    if (str3 != null)
                    {
                        this._sql.SetPassword(Encoding.UTF8.GetBytes(str3));
                    }
                    else if (this._password != null)
                    {
                        this._sql.SetPassword(this._password);
                    }
                }
                else
                {
                    string str4 = null;
                    byte[] numArray = SQLiteConnection.FromHexString(str2, ref str4);
                    if (numArray == null)
                    {
                        CultureInfo cultureInfo1 = CultureInfo.CurrentCulture;
                        object[] objArray4 = new object[] { str4 };
                        throw new FormatException(HelperMethods.StringFormat(cultureInfo1, "Cannot parse 'HexPassword' property value into byte values: {0}", objArray4));
                    }
                    this._sql.SetPassword(numArray);
                }
                this._password = null;
                if (flag3)
                {
                    this._dataSource = str1;
                }
                else
                {
                    this._dataSource = Path.GetFileNameWithoutExtension(str1);
                }
                this._version++;
                ConnectionState connectionState = this._connectionState;
                this._connectionState = ConnectionState.Open;
                try
                {
                    bool flag9 = true;
                    string str5 = SQLiteConnection.FindKey(strs, "SetDefaults", flag9.ToString());
                    bool flag10 = SQLiteConvert.ToBoolean(str5);
                    if (flag10)
                    {
                        using (SQLiteCommand sQLiteCommand = this.CreateCommand())
                        {
                            if (this._busyTimeout != 0)
                            {
                                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                                object[] objArray5 = new object[] { this._busyTimeout };
                                sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture, "PRAGMA busy_timeout={0}", objArray5);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            if (!flag3 && !flag4)
                            {
                                str5 = SQLiteConnection.FindKey(strs, "Page Size", SQLiteConvert.ToString(4096));
                                num = Convert.ToInt32(str5, CultureInfo.InvariantCulture);
                                if (num != 4096)
                                {
                                    CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
                                    object[] objArray6 = new object[] { num };
                                    sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture1, "PRAGMA page_size={0}", objArray6);
                                    sQLiteCommand.ExecuteNonQuery();
                                }
                            }
                            str5 = SQLiteConnection.FindKey(strs, "Max Page Count", SQLiteConvert.ToString(0));
                            num = Convert.ToInt32(str5, CultureInfo.InvariantCulture);
                            if (num != 0)
                            {
                                CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
                                object[] objArray7 = new object[] { num };
                                sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture2, "PRAGMA max_page_count={0}", objArray7);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            bool flag11 = false;
                            str5 = SQLiteConnection.FindKey(strs, "Legacy Format", flag11.ToString());
                            flag10 = SQLiteConvert.ToBoolean(str5);
                            if (flag10)
                            {
                                SQLiteCommand sQLiteCommand1 = sQLiteCommand;
                                CultureInfo cultureInfo2 = CultureInfo.InvariantCulture;
                                object[] objArray8 = new object[] { (flag10 ? "ON" : "OFF") };
                                sQLiteCommand1.CommandText = HelperMethods.StringFormat(cultureInfo2, "PRAGMA legacy_file_format={0}", objArray8);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            str5 = SQLiteConnection.FindKey(strs, "Synchronous", SQLiteSynchronousEnum.Default.ToString());
                            obj = SQLiteConnection.TryParseEnum(typeof(SQLiteSynchronousEnum), str5, true);
                            if (obj as SQLiteSynchronousEnum? == SQLiteSynchronousEnum.Off || ((SQLiteSynchronousEnum?)obj != null && (SQLiteSynchronousEnum)obj != SQLiteSynchronousEnum.Default))
                            {
                                CultureInfo invariantCulture3 = CultureInfo.InvariantCulture;
                                object[] objArray9 = new object[] { str5 };
                                sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture3, "PRAGMA synchronous={0}", objArray9);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            str5 = SQLiteConnection.FindKey(strs, "Cache Size", SQLiteConvert.ToString(-2000));
                            num = Convert.ToInt32(str5, CultureInfo.InvariantCulture);
                            if (num != -2000)
                            {
                                CultureInfo cultureInfo3 = CultureInfo.InvariantCulture;
                                object[] objArray10 = new object[] { num };
                                sQLiteCommand.CommandText = HelperMethods.StringFormat(cultureInfo3, "PRAGMA cache_size={0}", objArray10);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            str5 = SQLiteConnection.FindKey(strs, "Journal Mode", SQLiteJournalModeEnum.Default.ToString());
                            obj = SQLiteConnection.TryParseEnum(typeof(SQLiteJournalModeEnum), str5, true);
                            if (obj as SQLiteJournalModeEnum? == SQLiteJournalModeEnum.Delete || ((SQLiteJournalModeEnum?)obj != null && (SQLiteJournalModeEnum)obj != SQLiteJournalModeEnum.Default))
                            {
                                string str6 = "PRAGMA journal_mode={0}";
                                CultureInfo invariantCulture4 = CultureInfo.InvariantCulture;
                                object[] objArray11 = new object[] { str5 };
                                sQLiteCommand.CommandText = HelperMethods.StringFormat(invariantCulture4, str6, objArray11);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            bool flag12 = false;
                            str5 = SQLiteConnection.FindKey(strs, "Foreign Keys", flag12.ToString());
                            flag10 = SQLiteConvert.ToBoolean(str5);
                            if (flag10)
                            {
                                SQLiteCommand sQLiteCommand2 = sQLiteCommand;
                                CultureInfo cultureInfo4 = CultureInfo.InvariantCulture;
                                object[] objArray12 = new object[] { (flag10 ? "ON" : "OFF") };
                                sQLiteCommand2.CommandText = HelperMethods.StringFormat(cultureInfo4, "PRAGMA foreign_keys={0}", objArray12);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            bool flag13 = false;
                            str5 = SQLiteConnection.FindKey(strs, "Recursive Triggers", flag13.ToString());
                            flag10 = SQLiteConvert.ToBoolean(str5);
                            if (flag10)
                            {
                                SQLiteCommand sQLiteCommand3 = sQLiteCommand;
                                CultureInfo invariantCulture5 = CultureInfo.InvariantCulture;
                                object[] objArray13 = new object[] { (flag10 ? "ON" : "OFF") };
                                sQLiteCommand3.CommandText = HelperMethods.StringFormat(invariantCulture5, "PRAGMA recursive_triggers={0}", objArray13);
                                sQLiteCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    if (this._progressHandler != null)
                    {
                        this._sql.SetProgressHook(this._progressOps, this._progressCallback);
                    }
                    if (this._authorizerHandler != null)
                    {
                        this._sql.SetAuthorizerHook(this._authorizerCallback);
                    }
                    if (this._commitHandler != null)
                    {
                        this._sql.SetCommitHook(this._commitCallback);
                    }
                    if (this._updateHandler != null)
                    {
                        this._sql.SetUpdateHook(this._updateCallback);
                    }
                    if (this._rollbackHandler != null)
                    {
                        this._sql.SetRollbackHook(this._rollbackCallback);
                    }
                    Transaction current = Transaction.Current;
                    if (current != null && SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(strs, "Enlist", true.ToString())))
                    {
                        this.EnlistTransaction(current);
                    }
                    this._connectionState = connectionState;
                    StateChangeEventArgs stateChangeEventArg = null;
                    this.OnStateChange(ConnectionState.Open, ref stateChangeEventArg);
                    SQLiteConnection.OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Opened, stateChangeEventArg, null, null, null, null, null, null));
                }
                catch
                {
                    this._connectionState = connectionState;
                    throw;
                }
            }
            catch (SQLiteException sQLiteException)
            {
                this.Close();
                throw;
            }
        }

        public SQLiteConnection OpenAndReturn()
        {
            this.CheckDisposed();
            this.Open();
            return this;
        }

        internal static SortedList<string, string> ParseConnectionString(string connectionString, bool parseViaFramework, bool allowNameOnly)
        {
            return SQLiteConnection.ParseConnectionString(null, connectionString, parseViaFramework, allowNameOnly);
        }

        private static SortedList<string, string> ParseConnectionString(SQLiteConnection connection, string connectionString, bool parseViaFramework, bool allowNameOnly)
        {
            if (!parseViaFramework)
            {
                return SQLiteConnection.ParseConnectionString(connection, connectionString, allowNameOnly);
            }
            return SQLiteConnection.ParseConnectionStringViaFramework(connection, connectionString, false);
        }

        private static SortedList<string, string> ParseConnectionString(string connectionString, bool allowNameOnly)
        {
            return SQLiteConnection.ParseConnectionString(null, connectionString, allowNameOnly);
        }

        private static SortedList<string, string> ParseConnectionString(SQLiteConnection connection, string connectionString, bool allowNameOnly)
        {
            string[] strArrays;
            string str = connectionString;
            SortedList<string, string> strs = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
            string str1 = null;
            strArrays = (!SQLiteConnection.ShouldUseLegacyConnectionStringParser(connection) ? SQLiteConvert.NewSplit(str, ';', true, ref str1) : SQLiteConvert.Split(str, ';'));
            if (strArrays == null)
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                object[] objArray = new object[] { (str1 != null ? str1 : "could not split connection string into properties") };
                throw new ArgumentException(HelperMethods.StringFormat(currentCulture, "Invalid ConnectionString format, cannot parse: {0}", objArray));
            }
            int num = (strArrays != null ? (int)strArrays.Length : 0);
            for (int i = 0; i < num; i++)
            {
                if (strArrays[i] != null)
                {
                    strArrays[i] = strArrays[i].Trim();
                    if (strArrays[i].Length != 0)
                    {
                        int num1 = strArrays[i].IndexOf('=');
                        if (num1 == -1)
                        {
                            if (!allowNameOnly)
                            {
                                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                                object[] objArray1 = new object[] { strArrays[i] };
                                throw new ArgumentException(HelperMethods.StringFormat(cultureInfo, "Invalid ConnectionString format for part \"{0}\", no equal sign found", objArray1));
                            }
                            strs.Add(SQLiteConnection.UnwrapString(strArrays[i].Trim()), string.Empty);
                        }
                        else
                        {
                            strs.Add(SQLiteConnection.UnwrapString(strArrays[i].Substring(0, num1).Trim()), SQLiteConnection.UnwrapString(strArrays[i].Substring(num1 + 1).Trim()));
                        }
                    }
                }
            }
            return strs;
        }

        private static SortedList<string, string> ParseConnectionStringViaFramework(SQLiteConnection connection, string connectionString, bool strict)
        {
            DbConnectionStringBuilder dbConnectionStringBuilders = new DbConnectionStringBuilder()
            {
                ConnectionString = connectionString
            };
            SortedList<string, string> strs = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in dbConnectionStringBuilders.Keys)
            {
                object item = dbConnectionStringBuilders[key];
                string str = null;
                if (!(item is string))
                {
                    if (strict)
                    {
                        throw new ArgumentException("connection property value is not a string", key);
                    }
                    if (item != null)
                    {
                        str = item.ToString();
                    }
                }
                else
                {
                    str = (string)item;
                }
                strs.Add(key, str);
            }
            return strs;
        }

        private SQLiteProgressReturnCode ProgressCallback(IntPtr pUserData)
        {
            SQLiteProgressReturnCode returnCode;
            try
            {
                ProgressEventArgs progressEventArg = new ProgressEventArgs(pUserData, SQLiteProgressReturnCode.Continue);
                if (this._progressHandler != null)
                {
                    this._progressHandler(this, progressEventArg);
                }
                returnCode = progressEventArg.ReturnCode;
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (HelperMethods.LogCallbackExceptions(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { "Progress", exception };
                        SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
                    }
                }
                catch
                {
                }
                if ((this._flags & (SQLiteConnectionFlags.InterruptOnException | SQLiteConnectionFlags.UnbindFunctionsOnClose | SQLiteConnectionFlags.NoVerifyTextAffinity | SQLiteConnectionFlags.UseConnectionBindValueCallbacks | SQLiteConnectionFlags.UseConnectionReadValueCallbacks | SQLiteConnectionFlags.UseParameterNameForTypeName | SQLiteConnectionFlags.UseParameterDbTypeForTypeName | SQLiteConnectionFlags.NoVerifyTypeAffinity | SQLiteConnectionFlags.AllowNestedTransactions | SQLiteConnectionFlags.BindDecimalAsText | SQLiteConnectionFlags.GetDecimalAsText | SQLiteConnectionFlags.BindInvariantDecimal | SQLiteConnectionFlags.GetInvariantDecimal | SQLiteConnectionFlags.UseConnectionAllValueCallbacks | SQLiteConnectionFlags.UseParameterAnythingForTypeName)) == (SQLiteConnectionFlags.InterruptOnException | SQLiteConnectionFlags.UnbindFunctionsOnClose | SQLiteConnectionFlags.NoVerifyTextAffinity | SQLiteConnectionFlags.UseConnectionBindValueCallbacks | SQLiteConnectionFlags.UseConnectionReadValueCallbacks | SQLiteConnectionFlags.UseParameterNameForTypeName | SQLiteConnectionFlags.UseParameterDbTypeForTypeName | SQLiteConnectionFlags.NoVerifyTypeAffinity | SQLiteConnectionFlags.AllowNestedTransactions | SQLiteConnectionFlags.BindDecimalAsText | SQLiteConnectionFlags.GetDecimalAsText | SQLiteConnectionFlags.BindInvariantDecimal | SQLiteConnectionFlags.GetInvariantDecimal | SQLiteConnectionFlags.UseConnectionAllValueCallbacks | SQLiteConnectionFlags.UseParameterAnythingForTypeName))
                {
                    return SQLiteProgressReturnCode.Interrupt;
                }
                return SQLiteProgressReturnCode.Continue;
            }
            return returnCode;
        }

        public void ReleaseMemory()
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for releasing memory.");
            }
            SQLiteErrorCode sQLiteErrorCode = this._sql.ReleaseMemory();
            if (sQLiteErrorCode != SQLiteErrorCode.Ok)
            {
                throw new SQLiteException(sQLiteErrorCode, this._sql.GetLastError("Could not release connection memory."));
            }
        }

        public static SQLiteErrorCode ReleaseMemory(int nBytes, bool reset, bool compact, ref int nFree, ref bool resetOk, ref uint nLargest)
        {
            return SQLite3.StaticReleaseMemory(nBytes, reset, compact, ref nFree, ref resetOk, ref nLargest);
        }

        public SQLiteErrorCode ResultCode()
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for getting result code.");
            }
            return this._sql.ResultCode();
        }

        private void RollbackCallback(IntPtr parg)
        {
            try
            {
                if (this._rollbackHandler != null)
                {
                    this._rollbackHandler(this, EventArgs.Empty);
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (HelperMethods.LogCallbackExceptions(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { "Rollback", exception };
                        SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
                    }
                }
                catch
                {
                }
            }
        }

        private DataTable Schema_Catalogs(string strCatalog)
        {
            DataTable dataTable = new DataTable("Catalogs")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("CATALOG_NAME", typeof(string));
            dataTable.Columns.Add("DESCRIPTION", typeof(string));
            dataTable.Columns.Add("ID", typeof(long));
            dataTable.BeginLoadData();
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand("PRAGMA database_list", this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        if (string.Compare(sQLiteDataReader.GetString(1), strCatalog, StringComparison.OrdinalIgnoreCase) != 0 && strCatalog != null)
                        {
                            continue;
                        }
                        DataRow str = dataTable.NewRow();
                        str["CATALOG_NAME"] = sQLiteDataReader.GetString(1);
                        str["DESCRIPTION"] = sQLiteDataReader.GetString(2);
                        str["ID"] = sQLiteDataReader.GetInt64(0);
                        dataTable.Rows.Add(str);
                    }
                }
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_Columns(string strCatalog, string strTable, string strColumn)
        {
            DataTable dataTable = new DataTable("Columns")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("COLUMN_NAME", typeof(string));
            dataTable.Columns.Add("COLUMN_GUID", typeof(Guid));
            dataTable.Columns.Add("COLUMN_PROPID", typeof(long));
            dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
            dataTable.Columns.Add("COLUMN_HASDEFAULT", typeof(bool));
            dataTable.Columns.Add("COLUMN_DEFAULT", typeof(string));
            dataTable.Columns.Add("COLUMN_FLAGS", typeof(long));
            dataTable.Columns.Add("IS_NULLABLE", typeof(bool));
            dataTable.Columns.Add("DATA_TYPE", typeof(string));
            dataTable.Columns.Add("TYPE_GUID", typeof(Guid));
            dataTable.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            dataTable.Columns.Add("CHARACTER_OCTET_LENGTH", typeof(int));
            dataTable.Columns.Add("NUMERIC_PRECISION", typeof(int));
            dataTable.Columns.Add("NUMERIC_SCALE", typeof(int));
            dataTable.Columns.Add("DATETIME_PRECISION", typeof(long));
            dataTable.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
            dataTable.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
            dataTable.Columns.Add("CHARACTER_SET_NAME", typeof(string));
            dataTable.Columns.Add("COLLATION_CATALOG", typeof(string));
            dataTable.Columns.Add("COLLATION_SCHEMA", typeof(string));
            dataTable.Columns.Add("COLLATION_NAME", typeof(string));
            dataTable.Columns.Add("DOMAIN_CATALOG", typeof(string));
            dataTable.Columns.Add("DOMAIN_NAME", typeof(string));
            dataTable.Columns.Add("DESCRIPTION", typeof(string));
            dataTable.Columns.Add("PRIMARY_KEY", typeof(bool));
            dataTable.Columns.Add("EDM_TYPE", typeof(string));
            dataTable.Columns.Add("AUTOINCREMENT", typeof(bool));
            dataTable.Columns.Add("UNIQUE", typeof(bool));
            dataTable.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table' OR [type] LIKE 'view'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        if (!string.IsNullOrEmpty(strTable) && string.Compare(strTable, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        try
                        {
                            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                            object[] objArray1 = new object[] { strCatalog, sQLiteDataReader.GetString(2) };
                            using (SQLiteCommand sQLiteCommand1 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo, "SELECT * FROM [{0}].[{1}]", objArray1), this))
                            {
                                using (SQLiteDataReader sQLiteDataReader1 = sQLiteCommand1.ExecuteReader(CommandBehavior.SchemaOnly))
                                {
                                    using (DataTable schemaTable = sQLiteDataReader1.GetSchemaTable(true, true))
                                    {
                                        foreach (DataRow row in schemaTable.Rows)
                                        {
                                            if (string.Compare(row[SchemaTableColumn.ColumnName].ToString(), strColumn, StringComparison.OrdinalIgnoreCase) != 0 && strColumn != null)
                                            {
                                                continue;
                                            }
                                            DataRow item = dataTable.NewRow();
                                            item["NUMERIC_PRECISION"] = row[SchemaTableColumn.NumericPrecision];
                                            item["NUMERIC_SCALE"] = row[SchemaTableColumn.NumericScale];
                                            item["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                                            item["COLUMN_NAME"] = row[SchemaTableColumn.ColumnName];
                                            item["TABLE_CATALOG"] = strCatalog;
                                            item["ORDINAL_POSITION"] = row[SchemaTableColumn.ColumnOrdinal];
                                            item["COLUMN_HASDEFAULT"] = row[SchemaTableOptionalColumn.DefaultValue] != DBNull.Value;
                                            item["COLUMN_DEFAULT"] = row[SchemaTableOptionalColumn.DefaultValue];
                                            item["IS_NULLABLE"] = row[SchemaTableColumn.AllowDBNull];
                                            item["DATA_TYPE"] = row["DataTypeName"].ToString().ToLower(CultureInfo.InvariantCulture);
                                            item["EDM_TYPE"] = SQLiteConvert.DbTypeToTypeName(this, (DbType)row[SchemaTableColumn.ProviderType], this._flags).ToString().ToLower(CultureInfo.InvariantCulture);
                                            item["CHARACTER_MAXIMUM_LENGTH"] = row[SchemaTableColumn.ColumnSize];
                                            item["TABLE_SCHEMA"] = row[SchemaTableColumn.BaseSchemaName];
                                            item["PRIMARY_KEY"] = row[SchemaTableColumn.IsKey];
                                            item["AUTOINCREMENT"] = row[SchemaTableOptionalColumn.IsAutoIncrement];
                                            item["COLLATION_NAME"] = row["CollationType"];
                                            item["UNIQUE"] = row[SchemaTableColumn.IsUnique];
                                            dataTable.Rows.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                        catch (SQLiteException sQLiteException)
                        {
                        }
                    }
                }
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_DataSourceInformation()
        {
            DataTable dataTable = new DataTable("DataSourceInformation")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add(DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductName, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersion, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersionNormalized, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.GroupByBehavior, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.IdentifierPattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.IdentifierCase, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.OrderByColumnsInSelect, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterMarkerFormat, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterMarkerPattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterNameMaxLength, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterNamePattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierPattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierCase, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.StatementSeparatorPattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.StringLiteralPattern, typeof(string));
            dataTable.Columns.Add(DbMetaDataColumnNames.SupportedJoinOperators, typeof(int));
            dataTable.BeginLoadData();
            DataRow dataRow = dataTable.NewRow();
            object[] version = new object[] { null, "SQLite", this._sql.Version, this._sql.Version, 3, "(^\\[\\p{Lo}\\p{Lu}\\p{Ll}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Nd}@$#_]*$)|(^\\[[^\\]\\0]|\\]\\]+\\]$)|(^\\\"[^\\\"\\0]|\\\"\\\"+\\\"$)", 1, false, "{0}", "@[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)", 255, "^[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)", "(([^\\[]|\\]\\])*)", 1, ";", "'(([^']|'')*)'", 15 };
            dataRow.ItemArray = version;
            dataTable.Rows.Add(dataRow);
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_DataTypes()
        {
            DataTable dataTable = new DataTable("DataTypes")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("TypeName", typeof(string));
            dataTable.Columns.Add("ProviderDbType", typeof(int));
            dataTable.Columns.Add("ColumnSize", typeof(long));
            dataTable.Columns.Add("CreateFormat", typeof(string));
            dataTable.Columns.Add("CreateParameters", typeof(string));
            dataTable.Columns.Add("DataType", typeof(string));
            dataTable.Columns.Add("IsAutoIncrementable", typeof(bool));
            dataTable.Columns.Add("IsBestMatch", typeof(bool));
            dataTable.Columns.Add("IsCaseSensitive", typeof(bool));
            dataTable.Columns.Add("IsFixedLength", typeof(bool));
            dataTable.Columns.Add("IsFixedPrecisionScale", typeof(bool));
            dataTable.Columns.Add("IsLong", typeof(bool));
            dataTable.Columns.Add("IsNullable", typeof(bool));
            dataTable.Columns.Add("IsSearchable", typeof(bool));
            dataTable.Columns.Add("IsSearchableWithLike", typeof(bool));
            dataTable.Columns.Add("IsLiteralSupported", typeof(bool));
            dataTable.Columns.Add("LiteralPrefix", typeof(string));
            dataTable.Columns.Add("LiteralSuffix", typeof(string));
            dataTable.Columns.Add("IsUnsigned", typeof(bool));
            dataTable.Columns.Add("MaximumScale", typeof(short));
            dataTable.Columns.Add("MinimumScale", typeof(short));
            dataTable.Columns.Add("IsConcurrencyType", typeof(bool));
            dataTable.BeginLoadData();
            StringReader stringReader = new StringReader(System.Data.SQLite.SR.DataTypes);
            dataTable.ReadXml(stringReader);
            stringReader.Close();
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_ForeignKeys(string strCatalog, string strTable, string strKeyName)
        {
            object item;
            object empty;
            object obj;
            DataTable dataTable = new DataTable("ForeignKeys")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
            dataTable.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
            dataTable.Columns.Add("CONSTRAINT_NAME", typeof(string));
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("CONSTRAINT_TYPE", typeof(string));
            dataTable.Columns.Add("IS_DEFERRABLE", typeof(bool));
            dataTable.Columns.Add("INITIALLY_DEFERRED", typeof(bool));
            dataTable.Columns.Add("FKEY_ID", typeof(int));
            dataTable.Columns.Add("FKEY_FROM_COLUMN", typeof(string));
            dataTable.Columns.Add("FKEY_FROM_ORDINAL_POSITION", typeof(int));
            dataTable.Columns.Add("FKEY_TO_CATALOG", typeof(string));
            dataTable.Columns.Add("FKEY_TO_SCHEMA", typeof(string));
            dataTable.Columns.Add("FKEY_TO_TABLE", typeof(string));
            dataTable.Columns.Add("FKEY_TO_COLUMN", typeof(string));
            dataTable.Columns.Add("FKEY_ON_UPDATE", typeof(string));
            dataTable.Columns.Add("FKEY_ON_DELETE", typeof(string));
            dataTable.Columns.Add("FKEY_MATCH", typeof(string));
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            dataTable.BeginLoadData();
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        if (!string.IsNullOrEmpty(strTable) && string.Compare(strTable, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        try
                        {
                            using (SQLiteCommandBuilder sQLiteCommandBuilder = new SQLiteCommandBuilder())
                            {
                                CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                                object[] objArray1 = new object[] { strCatalog, sQLiteDataReader.GetString(2) };
                                using (SQLiteCommand sQLiteCommand1 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo, "PRAGMA [{0}].foreign_key_list([{1}])", objArray1), this))
                                {
                                    using (SQLiteDataReader sQLiteDataReader1 = sQLiteCommand1.ExecuteReader())
                                    {
                                        while (sQLiteDataReader1.Read())
                                        {
                                            DataRow dataRow = dataTable.NewRow();
                                            dataRow["CONSTRAINT_CATALOG"] = strCatalog;
                                            CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
                                            object[] item1 = new object[] { sQLiteDataReader[2], sQLiteDataReader1.GetInt32(0), sQLiteDataReader1.GetInt32(1) };
                                            dataRow["CONSTRAINT_NAME"] = HelperMethods.StringFormat(invariantCulture1, "FK_{0}_{1}_{2}", item1);
                                            dataRow["TABLE_CATALOG"] = strCatalog;
                                            dataRow["TABLE_NAME"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader.GetString(2));
                                            dataRow["CONSTRAINT_TYPE"] = "FOREIGN KEY";
                                            dataRow["IS_DEFERRABLE"] = false;
                                            dataRow["INITIALLY_DEFERRED"] = false;
                                            dataRow["FKEY_ID"] = sQLiteDataReader1[0];
                                            dataRow["FKEY_FROM_COLUMN"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader1[3].ToString());
                                            dataRow["FKEY_TO_CATALOG"] = strCatalog;
                                            dataRow["FKEY_TO_TABLE"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader1[2].ToString());
                                            dataRow["FKEY_TO_COLUMN"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader1[4].ToString());
                                            dataRow["FKEY_FROM_ORDINAL_POSITION"] = sQLiteDataReader1[1];
                                            DataRow dataRow1 = dataRow;
                                            if (sQLiteDataReader1.FieldCount > 5)
                                            {
                                                item = sQLiteDataReader1[5];
                                            }
                                            else
                                            {
                                                item = string.Empty;
                                            }
                                            dataRow1["FKEY_ON_UPDATE"] = item;
                                            DataRow dataRow2 = dataRow;
                                            if (sQLiteDataReader1.FieldCount > 6)
                                            {
                                                empty = sQLiteDataReader1[6];
                                            }
                                            else
                                            {
                                                empty = string.Empty;
                                            }
                                            dataRow2["FKEY_ON_DELETE"] = empty;
                                            DataRow dataRow3 = dataRow;
                                            if (sQLiteDataReader1.FieldCount > 7)
                                            {
                                                obj = sQLiteDataReader1[7];
                                            }
                                            else
                                            {
                                                obj = string.Empty;
                                            }
                                            dataRow3["FKEY_MATCH"] = obj;
                                            if (!string.IsNullOrEmpty(strKeyName) && string.Compare(strKeyName, dataRow["CONSTRAINT_NAME"].ToString(), StringComparison.OrdinalIgnoreCase) != 0)
                                            {
                                                continue;
                                            }
                                            dataTable.Rows.Add(dataRow);
                                        }
                                    }
                                }
                            }
                        }
                        catch (SQLiteException sQLiteException)
                        {
                        }
                    }
                }
            }
            dataTable.EndLoadData();
            dataTable.AcceptChanges();
            return dataTable;
        }

        private DataTable Schema_IndexColumns(string strCatalog, string strTable, string strIndex, string strColumn)
        {
            DataRow str;
            string str1;
            DataTable dataTable = new DataTable("IndexColumns");
            List<KeyValuePair<int, string>> keyValuePairs = new List<KeyValuePair<int, string>>();
            dataTable.Locale = CultureInfo.InvariantCulture;
            dataTable.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
            dataTable.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
            dataTable.Columns.Add("CONSTRAINT_NAME", typeof(string));
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("COLUMN_NAME", typeof(string));
            dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
            dataTable.Columns.Add("INDEX_NAME", typeof(string));
            dataTable.Columns.Add("COLLATION_NAME", typeof(string));
            dataTable.Columns.Add("SORT_MODE", typeof(string));
            dataTable.Columns.Add("CONFLICT_OPTION", typeof(int));
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str2 = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            dataTable.BeginLoadData();
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str2 };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        bool flag = false;
                        keyValuePairs.Clear();
                        if (!string.IsNullOrEmpty(strTable) && string.Compare(sQLiteDataReader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        try
                        {
                            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                            object[] objArray1 = new object[] { strCatalog, sQLiteDataReader.GetString(2) };
                            using (SQLiteCommand sQLiteCommand1 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo, "PRAGMA [{0}].table_info([{1}])", objArray1), this))
                            {
                                using (SQLiteDataReader sQLiteDataReader1 = sQLiteCommand1.ExecuteReader())
                                {
                                    while (sQLiteDataReader1.Read())
                                    {
                                        if (sQLiteDataReader1.GetInt32(5) != 1)
                                        {
                                            continue;
                                        }
                                        keyValuePairs.Add(new KeyValuePair<int, string>(sQLiteDataReader1.GetInt32(0), sQLiteDataReader1.GetString(1)));
                                        if (string.Compare(sQLiteDataReader1.GetString(2), "INTEGER", StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            continue;
                                        }
                                        flag = true;
                                    }
                                }
                            }
                        }
                        catch (SQLiteException sQLiteException)
                        {
                        }
                        if (keyValuePairs.Count == 1 && flag)
                        {
                            str = dataTable.NewRow();
                            str["CONSTRAINT_CATALOG"] = strCatalog;
                            CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
                            object[] objArray2 = new object[] { sQLiteDataReader.GetString(2), str2 };
                            str["CONSTRAINT_NAME"] = HelperMethods.StringFormat(invariantCulture1, "{1}_PK_{0}", objArray2);
                            str["TABLE_CATALOG"] = strCatalog;
                            str["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                            str["COLUMN_NAME"] = keyValuePairs[0].Value;
                            str["INDEX_NAME"] = str["CONSTRAINT_NAME"];
                            str["ORDINAL_POSITION"] = 0;
                            str["COLLATION_NAME"] = "BINARY";
                            str["SORT_MODE"] = "ASC";
                            str["CONFLICT_OPTION"] = 2;
                            if (string.IsNullOrEmpty(strIndex) || string.Compare(strIndex, (string)str["INDEX_NAME"], StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                dataTable.Rows.Add(str);
                            }
                        }
                        CultureInfo cultureInfo1 = CultureInfo.InvariantCulture;
                        object[] objArray3 = new object[] { strCatalog, sQLiteDataReader.GetString(2).Replace("'", "''"), str2 };
                        using (SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo1, "SELECT * FROM [{0}].[{2}] WHERE [type] LIKE 'index' AND [tbl_name] LIKE '{1}'", objArray3), this))
                        {
                            using (SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader())
                            {
                                while (sQLiteDataReader2.Read())
                                {
                                    int num = 0;
                                    if (!string.IsNullOrEmpty(strIndex) && string.Compare(strIndex, sQLiteDataReader2.GetString(1), StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
                                        object[] objArray4 = new object[] { strCatalog, sQLiteDataReader2.GetString(1) };
                                        using (SQLiteCommand sQLiteCommand3 = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture2, "PRAGMA [{0}].index_info([{1}])", objArray4), this))
                                        {
                                            using (SQLiteDataReader sQLiteDataReader3 = sQLiteCommand3.ExecuteReader())
                                            {
                                                while (sQLiteDataReader3.Read())
                                                {
                                                    if (sQLiteDataReader3.IsDBNull(2))
                                                    {
                                                        str1 = null;
                                                    }
                                                    else
                                                    {
                                                        str1 = sQLiteDataReader3.GetString(2);
                                                    }
                                                    string str3 = str1;
                                                    str = dataTable.NewRow();
                                                    str["CONSTRAINT_CATALOG"] = strCatalog;
                                                    str["CONSTRAINT_NAME"] = sQLiteDataReader2.GetString(1);
                                                    str["TABLE_CATALOG"] = strCatalog;
                                                    str["TABLE_NAME"] = sQLiteDataReader2.GetString(2);
                                                    str["COLUMN_NAME"] = str3;
                                                    str["INDEX_NAME"] = sQLiteDataReader2.GetString(1);
                                                    str["ORDINAL_POSITION"] = num;
                                                    string str4 = null;
                                                    int num1 = 0;
                                                    int num2 = 0;
                                                    if (str3 != null)
                                                    {
                                                        this._sql.GetIndexColumnExtendedInfo(strCatalog, sQLiteDataReader2.GetString(1), str3, ref num1, ref num2, ref str4);
                                                    }
                                                    if (!string.IsNullOrEmpty(str4))
                                                    {
                                                        str["COLLATION_NAME"] = str4;
                                                    }
                                                    str["SORT_MODE"] = (num1 == 0 ? "ASC" : "DESC");
                                                    str["CONFLICT_OPTION"] = num2;
                                                    num++;
                                                    if (strColumn != null && string.Compare(strColumn, str3, StringComparison.OrdinalIgnoreCase) != 0)
                                                    {
                                                        continue;
                                                    }
                                                    dataTable.Rows.Add(str);
                                                }
                                            }
                                        }
                                    }
                                    catch (SQLiteException sQLiteException1)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            dataTable.EndLoadData();
            dataTable.AcceptChanges();
            return dataTable;
        }

        private DataTable Schema_Indexes(string strCatalog, string strTable, string strIndex)
        {
            DataRow str;
            DataTable dataTable = new DataTable("Indexes");
            List<int> nums = new List<int>();
            dataTable.Locale = CultureInfo.InvariantCulture;
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("INDEX_CATALOG", typeof(string));
            dataTable.Columns.Add("INDEX_SCHEMA", typeof(string));
            dataTable.Columns.Add("INDEX_NAME", typeof(string));
            dataTable.Columns.Add("PRIMARY_KEY", typeof(bool));
            dataTable.Columns.Add("UNIQUE", typeof(bool));
            dataTable.Columns.Add("CLUSTERED", typeof(bool));
            dataTable.Columns.Add("TYPE", typeof(int));
            dataTable.Columns.Add("FILL_FACTOR", typeof(int));
            dataTable.Columns.Add("INITIAL_SIZE", typeof(int));
            dataTable.Columns.Add("NULLS", typeof(int));
            dataTable.Columns.Add("SORT_BOOKMARKS", typeof(bool));
            dataTable.Columns.Add("AUTO_UPDATE", typeof(bool));
            dataTable.Columns.Add("NULL_COLLATION", typeof(int));
            dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
            dataTable.Columns.Add("COLUMN_NAME", typeof(string));
            dataTable.Columns.Add("COLUMN_GUID", typeof(Guid));
            dataTable.Columns.Add("COLUMN_PROPID", typeof(long));
            dataTable.Columns.Add("COLLATION", typeof(short));
            dataTable.Columns.Add("CARDINALITY", typeof(decimal));
            dataTable.Columns.Add("PAGES", typeof(int));
            dataTable.Columns.Add("FILTER_CONDITION", typeof(string));
            dataTable.Columns.Add("INTEGRATED", typeof(bool));
            dataTable.Columns.Add("INDEX_DEFINITION", typeof(string));
            dataTable.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str1 = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str1 };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        bool flag = false;
                        nums.Clear();
                        if (!string.IsNullOrEmpty(strTable) && string.Compare(sQLiteDataReader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        try
                        {
                            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                            object[] objArray1 = new object[] { strCatalog, sQLiteDataReader.GetString(2) };
                            using (SQLiteCommand sQLiteCommand1 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo, "PRAGMA [{0}].table_info([{1}])", objArray1), this))
                            {
                                using (SQLiteDataReader sQLiteDataReader1 = sQLiteCommand1.ExecuteReader())
                                {
                                    while (sQLiteDataReader1.Read())
                                    {
                                        if (sQLiteDataReader1.GetInt32(5) == 0)
                                        {
                                            continue;
                                        }
                                        nums.Add(sQLiteDataReader1.GetInt32(0));
                                        if (string.Compare(sQLiteDataReader1.GetString(2), "INTEGER", StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            continue;
                                        }
                                        flag = true;
                                    }
                                }
                            }
                        }
                        catch (SQLiteException sQLiteException)
                        {
                        }
                        if (nums.Count == 1 && flag)
                        {
                            str = dataTable.NewRow();
                            str["TABLE_CATALOG"] = strCatalog;
                            str["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                            str["INDEX_CATALOG"] = strCatalog;
                            str["PRIMARY_KEY"] = true;
                            CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
                            object[] str2 = new object[] { sQLiteDataReader.GetString(2), str1 };
                            str["INDEX_NAME"] = HelperMethods.StringFormat(invariantCulture1, "{1}_PK_{0}", str2);
                            str["UNIQUE"] = true;
                            if (string.Compare((string)str["INDEX_NAME"], strIndex, StringComparison.OrdinalIgnoreCase) == 0 || strIndex == null)
                            {
                                dataTable.Rows.Add(str);
                            }
                            nums.Clear();
                        }
                        try
                        {
                            CultureInfo cultureInfo1 = CultureInfo.InvariantCulture;
                            object[] objArray2 = new object[] { strCatalog, sQLiteDataReader.GetString(2) };
                            using (SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo1, "PRAGMA [{0}].index_list([{1}])", objArray2), this))
                            {
                                using (SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader())
                                {
                                    while (sQLiteDataReader2.Read())
                                    {
                                        if (string.Compare(sQLiteDataReader2.GetString(1), strIndex, StringComparison.OrdinalIgnoreCase) != 0 && strIndex != null)
                                        {
                                            continue;
                                        }
                                        str = dataTable.NewRow();
                                        str["TABLE_CATALOG"] = strCatalog;
                                        str["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                                        str["INDEX_CATALOG"] = strCatalog;
                                        str["INDEX_NAME"] = sQLiteDataReader2.GetString(1);
                                        str["UNIQUE"] = SQLiteConvert.ToBoolean(sQLiteDataReader2.GetValue(2), CultureInfo.InvariantCulture, false);
                                        str["PRIMARY_KEY"] = false;
                                        CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
                                        object[] objArray3 = new object[] { strCatalog, sQLiteDataReader2.GetString(1).Replace("'", "''"), str1 };
                                        using (SQLiteCommand sQLiteCommand3 = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture2, "SELECT * FROM [{0}].[{2}] WHERE [type] LIKE 'index' AND [name] LIKE '{1}'", objArray3), this))
                                        {
                                            using (SQLiteDataReader sQLiteDataReader3 = sQLiteCommand3.ExecuteReader())
                                            {
                                                if (sQLiteDataReader3.Read() && !sQLiteDataReader3.IsDBNull(4))
                                                {
                                                    str["INDEX_DEFINITION"] = sQLiteDataReader3.GetString(4);
                                                }
                                            }
                                        }
                                        if (nums.Count > 0 && sQLiteDataReader2.GetString(1).StartsWith(string.Concat("sqlite_autoindex_", sQLiteDataReader.GetString(2)), StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            CultureInfo cultureInfo2 = CultureInfo.InvariantCulture;
                                            object[] objArray4 = new object[] { strCatalog, sQLiteDataReader2.GetString(1) };
                                            using (SQLiteCommand sQLiteCommand4 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo2, "PRAGMA [{0}].index_info([{1}])", objArray4), this))
                                            {
                                                using (SQLiteDataReader sQLiteDataReader4 = sQLiteCommand4.ExecuteReader())
                                                {
                                                    int num = 0;
                                                    while (sQLiteDataReader4.Read())
                                                    {
                                                        if (nums.Contains(sQLiteDataReader4.GetInt32(1)))
                                                        {
                                                            num++;
                                                        }
                                                        else
                                                        {
                                                            num = 0;
                                                            break;
                                                        }
                                                    }
                                                    if (num == nums.Count)
                                                    {
                                                        str["PRIMARY_KEY"] = true;
                                                        nums.Clear();
                                                    }
                                                }
                                            }
                                        }
                                        dataTable.Rows.Add(str);
                                    }
                                }
                            }
                        }
                        catch (SQLiteException sQLiteException1)
                        {
                        }
                    }
                }
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private static DataTable Schema_MetaDataCollections()
        {
            DataTable dataTable = new DataTable("MetaDataCollections")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("CollectionName", typeof(string));
            dataTable.Columns.Add("NumberOfRestrictions", typeof(int));
            dataTable.Columns.Add("NumberOfIdentifierParts", typeof(int));
            dataTable.BeginLoadData();
            StringReader stringReader = new StringReader(System.Data.SQLite.SR.MetaDataCollections);
            dataTable.ReadXml(stringReader);
            stringReader.Close();
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private static DataTable Schema_ReservedWords()
        {
            DataTable dataTable = new DataTable("ReservedWords")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("ReservedWord", typeof(string));
            dataTable.Columns.Add("MaximumVersion", typeof(string));
            dataTable.Columns.Add("MinimumVersion", typeof(string));
            dataTable.BeginLoadData();
            string[] strArrays = System.Data.SQLite.SR.Keywords.Split(new char[] { ',' });
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str = strArrays[i];
                DataRow dataRow = dataTable.NewRow();
                dataRow[0] = str;
                dataTable.Rows.Add(dataRow);
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_Tables(string strCatalog, string strTable, string strType)
        {
            DataTable dataTable = new DataTable("Tables")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("TABLE_TYPE", typeof(string));
            dataTable.Columns.Add("TABLE_ID", typeof(long));
            dataTable.Columns.Add("TABLE_ROOTPAGE", typeof(int));
            dataTable.Columns.Add("TABLE_DEFINITION", typeof(string));
            dataTable.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{0}].[{1}] WHERE [type] LIKE 'table'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        string str1 = sQLiteDataReader.GetString(0);
                        if (string.Compare(sQLiteDataReader.GetString(2), 0, "SQLITE_", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            str1 = "SYSTEM_TABLE";
                        }
                        if (string.Compare(strType, str1, StringComparison.OrdinalIgnoreCase) != 0 && strType != null || string.Compare(sQLiteDataReader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) != 0 && strTable != null)
                        {
                            continue;
                        }
                        DataRow num = dataTable.NewRow();
                        num["TABLE_CATALOG"] = strCatalog;
                        num["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                        num["TABLE_TYPE"] = str1;
                        num["TABLE_ID"] = sQLiteDataReader.GetInt64(5);
                        num["TABLE_ROOTPAGE"] = sQLiteDataReader.GetInt32(3);
                        num["TABLE_DEFINITION"] = sQLiteDataReader.GetString(4);
                        dataTable.Rows.Add(num);
                    }
                }
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_Triggers(string catalog, string table, string triggerName)
        {
            DataTable dataTable = new DataTable("Triggers")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("TRIGGER_NAME", typeof(string));
            dataTable.Columns.Add("TRIGGER_DEFINITION", typeof(string));
            dataTable.BeginLoadData();
            if (string.IsNullOrEmpty(table))
            {
                table = null;
            }
            if (string.IsNullOrEmpty(catalog))
            {
                catalog = "main";
            }
            string str = (string.Compare(catalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { catalog, str };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{0}].[{1}] WHERE [type] LIKE 'trigger'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        if (string.Compare(sQLiteDataReader.GetString(1), triggerName, StringComparison.OrdinalIgnoreCase) != 0 && triggerName != null || table != null && string.Compare(table, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["TABLE_CATALOG"] = catalog;
                        dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                        dataRow["TRIGGER_NAME"] = sQLiteDataReader.GetString(1);
                        dataRow["TRIGGER_DEFINITION"] = sQLiteDataReader.GetString(4);
                        dataTable.Rows.Add(dataRow);
                    }
                }
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        private DataTable Schema_ViewColumns(string strCatalog, string strView, string strColumn)
        {
            DataTable dataTable = new DataTable("ViewColumns")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("VIEW_CATALOG", typeof(string));
            dataTable.Columns.Add("VIEW_SCHEMA", typeof(string));
            dataTable.Columns.Add("VIEW_NAME", typeof(string));
            dataTable.Columns.Add("VIEW_COLUMN_NAME", typeof(string));
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("COLUMN_NAME", typeof(string));
            dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
            dataTable.Columns.Add("COLUMN_HASDEFAULT", typeof(bool));
            dataTable.Columns.Add("COLUMN_DEFAULT", typeof(string));
            dataTable.Columns.Add("COLUMN_FLAGS", typeof(long));
            dataTable.Columns.Add("IS_NULLABLE", typeof(bool));
            dataTable.Columns.Add("DATA_TYPE", typeof(string));
            dataTable.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            dataTable.Columns.Add("NUMERIC_PRECISION", typeof(int));
            dataTable.Columns.Add("NUMERIC_SCALE", typeof(int));
            dataTable.Columns.Add("DATETIME_PRECISION", typeof(long));
            dataTable.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
            dataTable.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
            dataTable.Columns.Add("CHARACTER_SET_NAME", typeof(string));
            dataTable.Columns.Add("COLLATION_CATALOG", typeof(string));
            dataTable.Columns.Add("COLLATION_SCHEMA", typeof(string));
            dataTable.Columns.Add("COLLATION_NAME", typeof(string));
            dataTable.Columns.Add("PRIMARY_KEY", typeof(bool));
            dataTable.Columns.Add("EDM_TYPE", typeof(string));
            dataTable.Columns.Add("AUTOINCREMENT", typeof(bool));
            dataTable.Columns.Add("UNIQUE", typeof(bool));
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            dataTable.BeginLoadData();
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'view'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        if (!string.IsNullOrEmpty(strView) && string.Compare(strView, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                        object[] objArray1 = new object[] { strCatalog, sQLiteDataReader.GetString(2) };
                        using (SQLiteCommand sQLiteCommand1 = new SQLiteCommand(HelperMethods.StringFormat(cultureInfo, "SELECT * FROM [{0}].[{1}]", objArray1), this))
                        {
                            string str1 = sQLiteDataReader.GetString(4).Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                            int i = CultureInfo.InvariantCulture.CompareInfo.IndexOf(str1, " AS ", CompareOptions.IgnoreCase);
                            if (i >= 0)
                            {
                                str1 = str1.Substring(i + 4);
                                using (SQLiteCommand sQLiteCommand2 = new SQLiteCommand(str1, this))
                                {
                                    using (SQLiteDataReader sQLiteDataReader1 = sQLiteCommand1.ExecuteReader(CommandBehavior.SchemaOnly))
                                    {
                                        using (SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader(CommandBehavior.SchemaOnly))
                                        {
                                            using (DataTable schemaTable = sQLiteDataReader1.GetSchemaTable(false, false))
                                            {
                                                using (DataTable schemaTable1 = sQLiteDataReader2.GetSchemaTable(false, false))
                                                {
                                                    for (i = 0; i < schemaTable1.Rows.Count; i++)
                                                    {
                                                        DataRow item = schemaTable.Rows[i];
                                                        DataRow dataRow = schemaTable1.Rows[i];
                                                        if (string.Compare(item[SchemaTableColumn.ColumnName].ToString(), strColumn, StringComparison.OrdinalIgnoreCase) == 0 || strColumn == null)
                                                        {
                                                            DataRow lower = dataTable.NewRow();
                                                            lower["VIEW_CATALOG"] = strCatalog;
                                                            lower["VIEW_NAME"] = sQLiteDataReader.GetString(2);
                                                            lower["TABLE_CATALOG"] = strCatalog;
                                                            lower["TABLE_SCHEMA"] = dataRow[SchemaTableColumn.BaseSchemaName];
                                                            lower["TABLE_NAME"] = dataRow[SchemaTableColumn.BaseTableName];
                                                            lower["COLUMN_NAME"] = dataRow[SchemaTableColumn.BaseColumnName];
                                                            lower["VIEW_COLUMN_NAME"] = item[SchemaTableColumn.ColumnName];
                                                            lower["COLUMN_HASDEFAULT"] = item[SchemaTableOptionalColumn.DefaultValue] != DBNull.Value;
                                                            lower["COLUMN_DEFAULT"] = item[SchemaTableOptionalColumn.DefaultValue];
                                                            lower["ORDINAL_POSITION"] = item[SchemaTableColumn.ColumnOrdinal];
                                                            lower["IS_NULLABLE"] = item[SchemaTableColumn.AllowDBNull];
                                                            lower["DATA_TYPE"] = item["DataTypeName"];
                                                            lower["EDM_TYPE"] = SQLiteConvert.DbTypeToTypeName(this, (DbType)item[SchemaTableColumn.ProviderType], this._flags).ToString().ToLower(CultureInfo.InvariantCulture);
                                                            lower["CHARACTER_MAXIMUM_LENGTH"] = item[SchemaTableColumn.ColumnSize];
                                                            lower["TABLE_SCHEMA"] = item[SchemaTableColumn.BaseSchemaName];
                                                            lower["PRIMARY_KEY"] = item[SchemaTableColumn.IsKey];
                                                            lower["AUTOINCREMENT"] = item[SchemaTableOptionalColumn.IsAutoIncrement];
                                                            lower["COLLATION_NAME"] = item["CollationType"];
                                                            lower["UNIQUE"] = item[SchemaTableColumn.IsUnique];
                                                            dataTable.Rows.Add(lower);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            dataTable.EndLoadData();
            dataTable.AcceptChanges();
            return dataTable;
        }

        private DataTable Schema_Views(string strCatalog, string strView)
        {
            DataTable dataTable = new DataTable("Views")
            {
                Locale = CultureInfo.InvariantCulture
            };
            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("VIEW_DEFINITION", typeof(string));
            dataTable.Columns.Add("CHECK_OPTION", typeof(bool));
            dataTable.Columns.Add("IS_UPDATABLE", typeof(bool));
            dataTable.Columns.Add("DESCRIPTION", typeof(string));
            dataTable.Columns.Add("DATE_CREATED", typeof(DateTime));
            dataTable.Columns.Add("DATE_MODIFIED", typeof(DateTime));
            dataTable.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", StringComparison.OrdinalIgnoreCase) == 0 ? "sqlite_temp_master" : "sqlite_master");
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            object[] objArray = new object[] { strCatalog, str };
            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(invariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'view'", objArray), this))
            {
                using (SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader())
                {
                    while (sQLiteDataReader.Read())
                    {
                        if (string.Compare(sQLiteDataReader.GetString(1), strView, StringComparison.OrdinalIgnoreCase) != 0 && !string.IsNullOrEmpty(strView))
                        {
                            continue;
                        }
                        string str1 = sQLiteDataReader.GetString(4).Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                        int num = CultureInfo.InvariantCulture.CompareInfo.IndexOf(str1, " AS ", CompareOptions.IgnoreCase);
                        if (num <= -1)
                        {
                            continue;
                        }
                        str1 = str1.Substring(num + 4).Trim();
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["TABLE_CATALOG"] = strCatalog;
                        dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
                        dataRow["IS_UPDATABLE"] = false;
                        dataRow["VIEW_DEFINITION"] = str1;
                        dataTable.Rows.Add(dataRow);
                    }
                }
            }
            dataTable.AcceptChanges();
            dataTable.EndLoadData();
            return dataTable;
        }

        public SQLiteErrorCode SetAvRetry(ref int count, ref int interval)
        {
            SQLiteErrorCode sQLiteErrorCode;
            this.CheckDisposed();
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database must be opened before changing the AV retry parameters.");
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = Marshal.AllocHGlobal(8);
                Marshal.WriteInt32(zero, 0, count);
                Marshal.WriteInt32(zero, 4, interval);
                sQLiteErrorCode = this._sql.FileControl(null, 9, zero);
                if (sQLiteErrorCode == SQLiteErrorCode.Ok)
                {
                    count = Marshal.ReadInt32(zero, 0);
                    interval = Marshal.ReadInt32(zero, 4);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return sQLiteErrorCode;
        }

        internal void SetCachedSetting(string name, object value)
        {
            if (name == null || this._cachedSettings == null)
            {
                return;
            }
            this._cachedSettings[name] = value;
        }

        public SQLiteErrorCode SetChunkSize(int size)
        {
            SQLiteErrorCode sQLiteErrorCode;
            this.CheckDisposed();
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database must be opened before changing the chunk size.");
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = Marshal.AllocHGlobal(4);
                Marshal.WriteInt32(zero, 0, size);
                sQLiteErrorCode = this._sql.FileControl(null, 6, zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return sQLiteErrorCode;
        }

        public void SetConfigurationOption(SQLiteConfigDbOpsEnum option, bool enable)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                object[] objArray = new object[] { (enable ? "enabling" : "disabling") };
                throw new InvalidOperationException(HelperMethods.StringFormat(currentCulture, "Database connection not valid for {0} a configuration option.", objArray));
            }
            if (option == SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION && (this._flags & SQLiteConnectionFlags.NoLoadExtension) == SQLiteConnectionFlags.NoLoadExtension)
            {
                throw new SQLiteException("Loading extensions is disabled for this database connection.");
            }
            this._sql.SetConfigurationOption(option, enable);
        }

        public void SetExtendedResultCodes(bool bOnOff)
        {
            this.CheckDisposed();
            if (this._sql != null)
            {
                this._sql.SetExtendedResultCodes(bOnOff);
            }
        }

        private static void SetLastCachedSetting(string name, object value)
        {
            if (SQLiteConnection._lastConnectionInOpen == null)
            {
                return;
            }
            SQLiteConnection._lastConnectionInOpen.SetCachedSetting(name, value);
        }

        public static SQLiteErrorCode SetMemoryStatus(bool value)
        {
            return SQLite3.StaticSetMemoryStatus(value);
        }

        public void SetPassword(string databasePassword)
        {
            byte[] bytes;
            this.CheckDisposed();
            if (string.IsNullOrEmpty(databasePassword))
            {
                bytes = null;
            }
            else
            {
                bytes = Encoding.UTF8.GetBytes(databasePassword);
            }
            this.SetPassword(bytes);
        }

        public void SetPassword(byte[] databasePassword)
        {
            this.CheckDisposed();
            if (this._connectionState != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Password can only be set before the database is opened.");
            }
            if (databasePassword != null && (int)databasePassword.Length == 0)
            {
                databasePassword = null;
            }
            this._password = databasePassword;
        }

        public bool SetTypeCallbacks(string typeName, SQLiteTypeCallbacks callbacks)
        {
            this.CheckDisposed();
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (this._typeCallbacks == null)
            {
                return false;
            }
            if (callbacks == null)
            {
                return this._typeCallbacks.Remove(typeName);
            }
            callbacks.TypeName = typeName;
            this._typeCallbacks[typeName] = callbacks;
            return true;
        }

        private void SetupSQLiteBase(SortedList<string, string> opts)
        {
            object obj = SQLiteConnection.TryParseEnum(typeof(SQLiteDateFormats), SQLiteConnection.FindKey(opts, "DateTimeFormat", SQLiteDateFormats.ISO8601.ToString()), true);
            SQLiteDateFormats sQLiteDateFormat = (obj as SQLiteDateFormats? != SQLiteDateFormats.Ticks ? (SQLiteDateFormats)obj : SQLiteDateFormats.ISO8601);
            obj = SQLiteConnection.TryParseEnum(typeof(DateTimeKind), SQLiteConnection.FindKey(opts, "DateTimeKind", DateTimeKind.Unspecified.ToString()), true);
            DateTimeKind dateTimeKind = (obj as DateTimeKind? != DateTimeKind.Unspecified ? (DateTimeKind)obj : DateTimeKind.Unspecified);
            string str = SQLiteConnection.FindKey(opts, "DateTimeFormatString", null);
            if (SQLiteConvert.ToBoolean(SQLiteConnection.FindKey(opts, "UseUTF16Encoding", false.ToString())))
            {
                this._sql = new SQLite3_UTF16(sQLiteDateFormat, dateTimeKind, str, IntPtr.Zero, null, false);
                return;
            }
            this._sql = new SQLite3(sQLiteDateFormat, dateTimeKind, str, IntPtr.Zero, null, false);
        }

        private static bool ShouldUseLegacyConnectionStringParser(SQLiteConnection connection)
        {
            object settingValue;
            string str = "No_SQLiteConnectionNewParser";
            if (connection != null && connection.TryGetCachedSetting(str, null, out settingValue))
            {
                return settingValue != null;
            }
            if (connection == null && SQLiteConnection.TryGetLastCachedSetting(str, null, out settingValue))
            {
                return settingValue != null;
            }
            settingValue = System.Data.SQLite.UnsafeNativeMethods.GetSettingValue(str, null);
            if (connection == null)
            {
                SQLiteConnection.SetLastCachedSetting(str, settingValue);
            }
            else
            {
                connection.SetCachedSetting(str, settingValue);
            }
            return settingValue != null;
        }

        public SQLiteErrorCode Shutdown()
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for shutdown.");
            }
            this._sql.Close(true);
            return this._sql.Shutdown();
        }

        public static void Shutdown(bool directories, bool noThrow)
        {
            SQLiteErrorCode sQLiteErrorCode = SQLite3.StaticShutdown(directories);
            if (sQLiteErrorCode != SQLiteErrorCode.Ok && !noThrow)
            {
                throw new SQLiteException(sQLiteErrorCode, null);
            }
        }

        internal static string ToHexString(byte[] array)
        {
            if (array == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            int length = (int)array.Length;
            for (int i = 0; i < length; i++)
            {
                stringBuilder.AppendFormat("{0:x2}", array[i]);
            }
            return stringBuilder.ToString();
        }

        private void TraceCallback(IntPtr puser, IntPtr statement)
        {
            try
            {
                if (this._traceHandler != null)
                {
                    this._traceHandler(this, new TraceEventArgs(SQLiteConvert.UTF8ToString(statement, -1)));
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (HelperMethods.LogCallbackExceptions(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { "Trace", exception };
                        SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
                    }
                }
                catch
                {
                }
            }
        }

        internal bool TryGetCachedSetting(string name, object @default, out object value)
        {
            if (name == null || this._cachedSettings == null)
            {
                value = @default;
                return false;
            }
            return this._cachedSettings.TryGetValue(name, out value);
        }

        private static bool TryGetLastCachedSetting(string name, object @default, out object value)
        {
            if (SQLiteConnection._lastConnectionInOpen == null)
            {
                value = @default;
                return false;
            }
            return SQLiteConnection._lastConnectionInOpen.TryGetCachedSetting(name, @default, out value);
        }

        public bool TryGetTypeCallbacks(string typeName, out SQLiteTypeCallbacks callbacks)
        {
            this.CheckDisposed();
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (this._typeCallbacks == null)
            {
                callbacks = null;
                return false;
            }
            return this._typeCallbacks.TryGetValue(typeName, out callbacks);
        }

        private static bool TryParseByte(string value, NumberStyles style, out byte result)
        {
            return byte.TryParse(value, style, (IFormatProvider)null, out result);
        }

        internal static object TryParseEnum(Type type, string value, bool ignoreCase)
        {
            object obj;
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    obj = Enum.Parse(type, value, ignoreCase);
                }
                catch
                {
                    return null;
                }
                return obj;
            }
            return null;
        }

        public bool UnbindAllFunctions(bool registered)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for unbinding functions.");
            }
            return SQLiteFunction.UnbindAllFunctions(this._sql, this._flags, registered);
        }

        public bool UnbindFunction(SQLiteFunctionAttribute functionAttribute)
        {
            this.CheckDisposed();
            if (this._sql == null)
            {
                throw new InvalidOperationException("Database connection not valid for unbinding functions.");
            }
            return this._sql.UnbindFunction(functionAttribute, this._flags);
        }

        private static string UnwrapString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            int length = value.Length;
            if ((value[0] != '\'' || value[length - 1] != '\'') && (value[0] != '\"' || value[length - 1] != '\"'))
            {
                return value;
            }
            return value.Substring(1, length - 2);
        }

        private void UpdateCallback(IntPtr puser, int type, IntPtr database, IntPtr table, long rowid)
        {
            try
            {
                this._updateHandler(this, new UpdateEventArgs(SQLiteConvert.UTF8ToString(database, -1), SQLiteConvert.UTF8ToString(table, -1), (UpdateEventType)type, rowid));
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (HelperMethods.LogCallbackExceptions(this._flags))
                    {
                        CultureInfo currentCulture = CultureInfo.CurrentCulture;
                        object[] objArray = new object[] { "Update", exception };
                        SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
                    }
                }
                catch
                {
                }
            }
        }

        private event SQLiteAuthorizerEventHandler _authorizerHandler;

        private event SQLiteCommitHandler _commitHandler;

        private static event SQLiteConnectionEventHandler _handlers;

        private event SQLiteProgressEventHandler _progressHandler;

        private event EventHandler _rollbackHandler;

        private event SQLiteTraceEventHandler _traceHandler;

        private event SQLiteUpdateEventHandler _updateHandler;

        public event SQLiteAuthorizerEventHandler Authorize
        {
            add
            {
                this.CheckDisposed();
                if (this._authorizerHandler == null)
                {
                    this._authorizerCallback = new SQLiteAuthorizerCallback(this.AuthorizerCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetAuthorizerHook(this._authorizerCallback);
                    }
                }
                this._authorizerHandler += value;
            }
            remove
            {
                this.CheckDisposed();
                this._authorizerHandler -= value;
                if (this._authorizerHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetAuthorizerHook(null);
                    }
                    this._authorizerCallback = null;
                }
            }
        }

        public static event SQLiteConnectionEventHandler Changed
        {
            add
            {
                lock (SQLiteConnection._syncRoot)
                {
                    SQLiteConnection._handlers -= value;
                    SQLiteConnection._handlers += value;
                }
            }
            remove
            {
                lock (SQLiteConnection._syncRoot)
                {
                    SQLiteConnection._handlers -= value;
                }
            }
        }

        public event SQLiteCommitHandler Commit
        {
            add
            {
                this.CheckDisposed();
                if (this._commitHandler == null)
                {
                    this._commitCallback = new SQLiteCommitCallback(this.CommitCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetCommitHook(this._commitCallback);
                    }
                }
                this._commitHandler += value;
            }
            remove
            {
                this.CheckDisposed();
                this._commitHandler -= value;
                if (this._commitHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetCommitHook(null);
                    }
                    this._commitCallback = null;
                }
            }
        }

        public event SQLiteProgressEventHandler Progress
        {
            add
            {
                this.CheckDisposed();
                if (this._progressHandler == null)
                {
                    this._progressCallback = new SQLiteProgressCallback(this.ProgressCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetProgressHook(this._progressOps, this._progressCallback);
                    }
                }
                this._progressHandler += value;
            }
            remove
            {
                this.CheckDisposed();
                this._progressHandler -= value;
                if (this._progressHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetProgressHook(0, null);
                    }
                    this._progressCallback = null;
                }
            }
        }

        public event EventHandler RollBack
        {
            add
            {
                this.CheckDisposed();
                if (this._rollbackHandler == null)
                {
                    this._rollbackCallback = new SQLiteRollbackCallback(this.RollbackCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetRollbackHook(this._rollbackCallback);
                    }
                }
                this._rollbackHandler += value;
            }
            remove
            {
                this.CheckDisposed();
                this._rollbackHandler -= value;
                if (this._rollbackHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetRollbackHook(null);
                    }
                    this._rollbackCallback = null;
                }
            }
        }

        public override event StateChangeEventHandler StateChange;

        public event SQLiteTraceEventHandler Trace
        {
            add
            {
                this.CheckDisposed();
                if (this._traceHandler == null)
                {
                    this._traceCallback = new SQLiteTraceCallback(this.TraceCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetTraceCallback(this._traceCallback);
                    }
                }
                this._traceHandler += value;
            }
            remove
            {
                this.CheckDisposed();
                this._traceHandler -= value;
                if (this._traceHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetTraceCallback(null);
                    }
                    this._traceCallback = null;
                }
            }
        }

        public event SQLiteUpdateEventHandler Update
        {
            add
            {
                this.CheckDisposed();
                if (this._updateHandler == null)
                {
                    this._updateCallback = new SQLiteUpdateCallback(this.UpdateCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetUpdateHook(this._updateCallback);
                    }
                }
                this._updateHandler += value;
            }
            remove
            {
                this.CheckDisposed();
                this._updateHandler -= value;
                if (this._updateHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetUpdateHook(null);
                    }
                    this._updateCallback = null;
                }
            }
        }
    }
}