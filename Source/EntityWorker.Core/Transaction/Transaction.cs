using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.FastDeepCloner;
using EntityWorker.Core.SQLite;
using System.Collections;
using EntityWorker.Core.SqlQuerys;
using EntityWorker.Core.Postgres;
using EntityWorker.Core.Object.Library.Modules;

namespace EntityWorker.Core.Transaction
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// EntityWorker.Core Repository
    /// </summary>
    public abstract class Transaction : IRepository
    {
        internal Custom_ValueType<IDataReader, bool> OpenedDataReaders = new Custom_ValueType<IDataReader, bool>();

        private static object MigrationLocker = new object();

        internal readonly DbSchema _dbSchema;

        private readonly Custom_ValueType<string, object> _attachedObjects;
        /// <summary>
        /// DataBase FullConnectionString
        /// </summary>
        public readonly string ConnectionString;


        private static Dictionary<DataBaseTypes, bool> _moduleIni = new Dictionary<DataBaseTypes, bool>()
        {
            { DataBaseTypes.Mssql, false },
            { DataBaseTypes.Sqllight, false },
            { DataBaseTypes.PostgreSql, false }
        };

        /// <summary>
        /// DataBase Type
        /// </summary>
        public DataBaseTypes DataBaseTypes { get; private set; }

        internal DbTransaction Trans { get; private set; }

        /// <summary>
        /// DataBase Connection
        /// </summary>
        protected DbConnection SqlConnection { get; private set; }

        private static Dictionary<DataBaseTypes, bool> _tableMigrationCheck = new Dictionary<DataBaseTypes, bool>()
        {
            { DataBaseTypes.Mssql, false },
            { DataBaseTypes.Sqllight, false },
            { DataBaseTypes.PostgreSql, false }
        };


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Full connection string</param>
        /// <param name="dataBaseTypes">The type of the database Ms-sql or Sql-light</param>
        public Transaction(string connectionString, DataBaseTypes dataBaseTypes)
        {

            _attachedObjects = new Custom_ValueType<string, object>();
            if (string.IsNullOrEmpty(connectionString))
                if (string.IsNullOrEmpty(connectionString))
                    throw new Exception("connectionString cant be empty");

            ConnectionString = connectionString;
            DataBaseTypes = dataBaseTypes;
            _dbSchema = new DbSchema(this);

            if (!_moduleIni[dataBaseTypes])
            {
                lock (this)
                {
                    if (!_moduleIni[dataBaseTypes])
                    {
                        OnModuleConfiguration(new ModuleBuilder(dataBaseTypes));
                        OnModuleStart();
                        _moduleIni[dataBaseTypes] = true;
                    }
                }
            }
            else _moduleIni[dataBaseTypes] = true;
        }

        /// <summary>
        /// Start once the Transaction first initialized and its threadsafe
        /// </summary>
        protected abstract void OnModuleStart();

        /// <summary>
        /// Configrate your modules here, add Primary keys , ForeignKey and so on here.
        /// </summary>
        /// <param name="moduleBuilder"></param>
        protected abstract void OnModuleConfiguration(IModuleBuilder moduleBuilder);

        /// <summary>
        /// Initialize the migration
        /// </summary>
        protected void InitializeMigration(Assembly assembly = null)
        {
            lock (MigrationLocker)
            {
                if (_tableMigrationCheck[DataBaseTypes])
                    return;
                assembly = assembly ?? this.GetType().Assembly;
                IMigrationConfig config;
                if (assembly.DefinedTypes.Any(a => typeof(IMigrationConfig).IsAssignableFrom(a)))
                    config = Activator.CreateInstance(assembly.DefinedTypes.First(a => typeof(IMigrationConfig).IsAssignableFrom(a))) as IMigrationConfig;
                else throw new Exception($"EntityWorker.Core could not find IMigrationConfig in the current Assembly {assembly.GetName()}");
                MigrationConfig(config);
            }
        }


        /// <summary>
        /// Return the new added column, tables or modified prooerty
        /// Property Rename is not supported. renaming a property x will end up removing the column x and adding column y so there will be dataloss
        /// Adding a primary key is not supported either
        /// Abstract classes are ignored by default
        /// </summary>
        /// <assembly> Null for the current executed Assembly </assembly>
        /// <returns></returns>
        protected CodeToDataBaseMergeCollection GetCodeLatestChanges(Assembly assembly = null)
        {
            var codeToDataBaseMergeCollection = new CodeToDataBaseMergeCollection(this);
            MethodHelper.GetDbEntitys(assembly ?? this.GetType().Assembly).ForEach(x =>
            {
                if (!x.IsAbstract) // Ignore abstract classes by default
                    _dbSchema.GetDatabase_Diff(x, codeToDataBaseMergeCollection);
            });
            return codeToDataBaseMergeCollection;
        }

        /// <summary>
        /// Validate if database exist 
        /// </summary>
        /// <returns></returns>
        protected bool DataBaseExist()
        {
            var sqlBuild = DataBaseTypes != DataBaseTypes.PostgreSql ? new SqlConnectionStringBuilder(ConnectionString) : null;
            var npSqlBuilder = DataBaseTypes == DataBaseTypes.PostgreSql ? new NpgsqlConnectionStringBuilder(ConnectionString) : null;
            var dbName = DataBaseTypes == DataBaseTypes.Mssql ? sqlBuild.InitialCatalog : sqlBuild?.DataSource;
            if (string.IsNullOrEmpty(dbName) && DataBaseTypes != DataBaseTypes.PostgreSql)
                throw new Exception("InitialCatalog cant be null or empty");

            if (DataBaseTypes == DataBaseTypes.PostgreSql && string.IsNullOrEmpty(npSqlBuilder.Database))
                throw new Exception("Database cant be null or empty");

            if (DataBaseTypes == DataBaseTypes.Mssql)
            {
                sqlBuild.InitialCatalog = "master";
                var tr = new DbRepository(sqlBuild.ToString(), DataBaseTypes);
                var cmd = tr.GetSqlCommand("SELECT  CAST(CASE WHEN db_id(String[" + dbName + "]) is not null THEN 1 ELSE 0 END AS BIT)");
                return tr.ExecuteScalar(cmd).ConvertValue<bool>();
            }
            else if (DataBaseTypes == DataBaseTypes.Sqllight)
            {
                try
                {
                    var tr = new DbRepository(sqlBuild.ToString(), DataBaseTypes);
                    tr.ValidateConnection();
                    return true;

                }
                catch
                {
                    return false;
                }
            }
            else
            {
                dbName = npSqlBuilder.Database;
                npSqlBuilder.Database = "";
                var tr = new DbRepository(npSqlBuilder.ToString(), DataBaseTypes);
                var cmd = tr.GetSqlCommand("SELECT CAST(CASE WHEN datname is not null THEN 1 ELSE 0 END AS BIT) from pg_database WHERE lower(datname) = lower(String[" + dbName + "])");
                return tr.ExecuteScalar(cmd).ConvertValue<bool>();

            }
        }

        /// <summary>
        /// Create DataBase if not exist
        /// </summary>
        protected void CreateDataBase()
        {
            lock (this)
            {
                if (DataBaseExist())
                    return;



                var sqlBuild = DataBaseTypes != DataBaseTypes.PostgreSql ? new SqlConnectionStringBuilder(ConnectionString) : null;
                var npSqlBuilder = DataBaseTypes == DataBaseTypes.PostgreSql ? new NpgsqlConnectionStringBuilder(ConnectionString) : null;
                var dbName = DataBaseTypes == DataBaseTypes.Mssql ? sqlBuild?.InitialCatalog : sqlBuild?.DataSource;
                if (string.IsNullOrEmpty(dbName) && DataBaseTypes != DataBaseTypes.PostgreSql)
                    throw new Exception("InitialCatalog can not be null or empty");

                if (DataBaseTypes == DataBaseTypes.PostgreSql && string.IsNullOrEmpty(npSqlBuilder.Database))
                    throw new Exception("Database can not be null or empty");

                if (DataBaseTypes == DataBaseTypes.Mssql)
                {
                    sqlBuild.InitialCatalog = "master";
                    var tr = new DbRepository(sqlBuild.ToString(), DataBaseTypes);
                    var cmd = tr.GetSqlCommand("Create DataBase [" + dbName.Trim() + "]");
                    tr.ExecuteNonQuery(cmd);

                }
                else if (DataBaseTypes == DataBaseTypes.Sqllight)
                    SQLiteConnection.CreateFile(dbName.Trim());
                else
                {
                    dbName = npSqlBuilder.Database;
                    npSqlBuilder.Database = "";
                    var tr = new DbRepository(npSqlBuilder.ToString(), DataBaseTypes);
                    var cmd = tr.GetSqlCommand("Create DataBase " + dbName.Trim());
                    tr.ExecuteNonQuery(cmd);
                }

                var latestChanges = GetCodeLatestChanges();
                if (latestChanges.Any())
                    latestChanges.Execute(true);
            }

        }


        /// <summary>
        /// Clone Items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="level"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public T Clone<T>(T o, CloneLevel level, FieldType fieldType = FieldType.PropertyInfo) where T : class
        {
            return DeepCloner.Clone(o, new FastDeepClonerSettings()
            {
                FieldType = fieldType,
                CloneLevel = level
            });
        }

        /// <summary>
        /// Specifies the migrationConfig which contain a list Migration to migrate
        /// the migration is executed automatic as long as you have class that inherit from IMigrationConfig
        /// or you could manually execute a migration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        protected void MigrationConfig<T>(T config) where T : IMigrationConfig
        {
            try
            {
                this.CreateTable<DBMigration>(false);
                var migrations = config.GetMigrations(this) ?? new List<Migration>();
                this.CreateTransaction();
                foreach (var migration in migrations)
                {
                    var name = migration.GetType().FullName + migration.MigrationIdentifier;
                    if (Get<DBMigration>().Where(x => x.Name == name).ExecuteAny())
                        continue;

                    var item = new DBMigration
                    {
                        Name = name,
                        DateCreated = DateTime.Now
                    };
                    migration.ExecuteMigration(this);
                    this.Save(item);

                }
                SaveChanges();
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }

            _tableMigrationCheck[DataBaseTypes] = true;
        }

        private void CloseifPassible()
        {
            if (OpenedDataReaders.Keys.Any(x => !x.IsClosed))
                return;
            if (Trans == null)
                SqlConnection.Close();
        }

        /// <summary>
        /// Validate Connection is Open or broken
        /// </summary>
        protected void ValidateConnection()
        {
            if (SqlConnection == null)
            {
                if (DataBaseTypes == DataBaseTypes.Sqllight)
                {
                    if (SqlConnection == null)
                        SqlConnection = new SQLiteConnection(ConnectionString);
                }
                else if (DataBaseTypes == DataBaseTypes.Mssql)
                {
                    if (SqlConnection == null)
                        SqlConnection = new SqlConnection(ConnectionString);
                }
                else
                {
                    if (SqlConnection == null)
                        SqlConnection = new NpgsqlConnection(ConnectionString);
                }
            }

            if (SqlConnection.State == ConnectionState.Broken || SqlConnection.State == ConnectionState.Closed)
                SqlConnection.Open();
        }


        internal void Renew()
        {
            if (Trans?.Connection == null)
            {
                SqlConnection.Close();
                SqlConnection.Open();
            }
            else
            {
                Trans.Commit();
                CreateTransaction();
            }

        }

        /// <summary>
        /// Create Transaction
        /// Only one Transaction will be created until it get disposed
        /// </summary>
        /// <returns></returns>
        public DbTransaction CreateTransaction()
        {
            ValidateConnection();
            if (Trans?.Connection == null)
                Trans = SqlConnection.BeginTransaction();

            return Trans;
        }

        /// <summary>
        /// Convert to known object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public ISqlQueryable<T> DataReaderConverter<T>(DbCommandExtended command)
        {
            return new SqlQueryable<T>(this, ((List<T>)DataReaderConverter(command, typeof(T))));
        }

        /// <summary>
        /// Convert to unknown type
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList DataReaderConverter(DbCommandExtended command, Type type)
        {
            IList result;
            ValidateConnection();
            try
            {
                var o = command.Command.ExecuteReader();
                OpenedDataReaders.GetOrAdd(o, true);
                result = Extension.DataReaderConverter(this, o, command, type);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                CloseifPassible();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public object ExecuteScalar(DbCommandExtended cmd)
        {
            ValidateConnection();
            var o = cmd.Command.ExecuteScalar();
            CloseifPassible();
            return o;
        }

        /// <inheritdoc />
        public int ExecuteNonQuery(DbCommandExtended cmd)
        {

            ValidateConnection();
            var o = cmd.Command.ExecuteNonQuery();
            CloseifPassible();
            return o;
        }

        /// <summary>
        /// Rollback transaction
        /// </summary>
        public void Rollback()
        {
            Trans?.Rollback();
            Dispose();
        }

        /// <summary>
        /// commit the transaction
        /// </summary>
        public void SaveChanges()
        {
            try
            {
                Trans?.Commit();
            }
            catch
            {
                Rollback();
                throw;
            }
            finally
            {
                OpenedDataReaders.Clear();
                Trans?.Dispose();
                SqlConnection?.Dispose();
                Trans = null;
                SqlConnection = null;
            }
        }

        /// <summary>
        /// Dispose the connection
        /// </summary>
        public virtual void Dispose()
        {
            OpenedDataReaders.Clear();
            _attachedObjects.Clear();
            Trans?.Dispose();
            SqlConnection?.Dispose();
            Trans = null;
            SqlConnection = null;
        }

        /// <summary>
        /// SqlDbType by system.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SqlDbType GetSqlType(Type type)
        {
            if (type == typeof(string))
                return SqlDbType.NVarChar;

            if (type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);
            try
            {
                var param = new SqlParameter("", type == typeof(byte[]) ? new byte[0] : type.CreateInstance(true));
                return param.SqlDbType;
            }
            catch
            {
                var param = new SqlParameter("", new object());
                return param.SqlDbType;
            }
        }

        /// <summary>
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        public IRepository AddInnerParameter(DbCommandExtended cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar)
        {
            if (attrName != null && attrName[0] != '@')
                attrName = "@" + attrName;

            if (value?.GetType().GetTypeInfo().IsEnum ?? false)
                value = value.ConvertValue<long>();

            var sqlDbTypeValue = value ?? DBNull.Value;

            if (DataBaseTypes == DataBaseTypes.Mssql)
            {
                var param = new SqlParameter
                {
                    SqlDbType = dbType,
                    Value = sqlDbTypeValue,
                    ParameterName = attrName
                };
                cmd.Command.Parameters.Add(param);
            }
            else if (DataBaseTypes == DataBaseTypes.Sqllight)
            {
                (cmd.Command as SQLiteCommand).Parameters.AddWithValue(attrName, value ?? DBNull.Value);
            }
            else
            {
                (cmd.Command as NpgsqlCommand).Parameters.AddWithValue(attrName, value ?? DBNull.Value);
            }

            return this;
        }

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type">Set for faster Converting of dbreader to object</param>
        /// <returns></returns>
        public DbCommandExtended GetSqlCommand(string sql, Type type = null)
        {
            ValidateConnection();
            return this.ProcessSql(SqlConnection, Trans, sql, type);
        }

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="type">Set for faster Converting of dbreader to object</param>
        /// <returns></returns>
        public DbCommandExtended GetStoredProcedure(string storedProcedure, Type type = null)
        {
            ValidateConnection();
            var cmd = this.ProcessSql(SqlConnection, Trans, storedProcedure, type);
            cmd.Command.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        /// <summary>
        /// return a list of LightDataTable e.g. DataSet
        /// </summary>
        /// <param name="cmd">sqlCommand that are create from GetSqlCommand</param>
        /// <param name="primaryKeyId"> Table primaryKeyId, so LightDataTable.FindByPrimaryKey could be used </param>
        /// <returns></returns>
        protected List<ILightDataTable> GetLightDataTableList(DbCommandExtended cmd, string primaryKeyId = null)
        {
            var returnList = new List<ILightDataTable>();
            var reader = cmd.Command.ExecuteReader();
            returnList.Add(new LightDataTable().ReadData(DataBaseTypes, reader, cmd, primaryKeyId, false));

            while (reader.NextResult())
                returnList.Add(new LightDataTable().ReadData(DataBaseTypes, reader, cmd, primaryKeyId, false));
            reader.Close();
            reader.Dispose();
            return returnList;
        }

        /// <summary>
        /// Attach object to WorkEntity to track changes
        /// </summary>
        /// <param name="objcDbEntity"></param>
        /// <param name="overwrite"></param>
        public void Attach(object objcDbEntity, bool overwrite = false)
        {
            if (objcDbEntity == null)
                throw new NullReferenceException("DbEntity cant be null");
            if (Extension.ObjectIsNew(objcDbEntity.GetPrimaryKeyValue()))
                throw new NullReferenceException("Id is IsNullOrEmpty, it cant be attached");
            var key = objcDbEntity.EntityKey();

            if (_attachedObjects.ContainsKey(key))
            {
                if (overwrite)
                    _attachedObjects.GetOrAdd(key, this.Clone(objcDbEntity, CloneLevel.FirstLevelOnly), true);

            }
            else
                _attachedObjects.GetOrAdd(key, this.Clone(objcDbEntity, CloneLevel.FirstLevelOnly));

        }

        /// <summary>
        /// Attach object to WorkEntity to track changes
        /// </summary>
        /// <param name="objcDbEntity"></param>
        /// <param name="overwrite"></param>
        internal void AttachNew(object objcDbEntity, bool overwrite = false)
        {
            if (objcDbEntity == null)
                throw new NullReferenceException("DbEntity cant be null");
            if (Extension.ObjectIsNew(objcDbEntity.GetPrimaryKeyValue()))
                throw new NullReferenceException("Id is IsNullOrEmpty, it cant be attached");
            var key = objcDbEntity.EntityKey();
            if (_attachedObjects.ContainsKey(key))
            {
                if (overwrite)
                    _attachedObjects.GetOrAdd(key, objcDbEntity, true);

            }
            else
                _attachedObjects.GetOrAdd(key, objcDbEntity);
        }

        /// <summary>
        /// Get object changes from already attached objects
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<EntityChanges> GetObjectChanges(object entity)
        {
            var changes = new List<EntityChanges>();
            if (!IsAttached(entity))
                throw new NullReferenceException("Object is not attached");
            var originalObject = _attachedObjects[entity.EntityKey()];
            if (originalObject == null)
                throw new Exception("Object need to be attached");
            foreach (var prop in DeepCloner.GetFastDeepClonerProperties(entity.GetType()))
            {
                var aValue = prop.GetValue(entity);
                var bValue = prop.GetValue(originalObject);
                if (!prop.IsInternalType || prop.ContainAttribute<ExcludeFromAbstract>() || prop.ContainAttribute<PrimaryKey>() || (aValue == null && bValue == null))
                    continue;
                if ((aValue != null && aValue.Equals(bValue)) || (bValue != null && bValue.Equals(aValue)))
                    continue;
                changes.Add(new EntityChanges
                {
                    PropertyName = prop.Name,
                    NewValue = prop.GetValue(entity),
                    OldValue = prop.GetValue(originalObject)
                });
            }
            return changes;
        }

        /// <summary>
        /// Get object changes
        /// </summary>
        /// <param name="entityA"></param>
        /// <param name="entityB"></param>
        /// <returns></returns>
        public List<EntityChanges> GetObjectChanges(object entityA, object entityB)
        {
            var changes = new List<EntityChanges>();
            var originalObject = entityA;
            if (originalObject == null)
                throw new Exception("Object need to be attached");
            foreach (var prop in DeepCloner.GetFastDeepClonerProperties(entityA.GetType()))
            {
                var aValue = prop.GetValue(entityB);
                var bValue = prop.GetValue(originalObject);
                if (!prop.IsInternalType || prop.ContainAttribute<ExcludeFromAbstract>() || prop.ContainAttribute<PrimaryKey>() || (aValue == null && bValue == null))
                    continue;
                if ((aValue != null && aValue.Equals(bValue)) || (bValue != null && bValue.Equals(aValue)))
                    continue;
                changes.Add(new EntityChanges
                {
                    PropertyName = prop.Name,
                    NewValue = prop.GetValue(entityA),
                    OldValue = prop.GetValue(entityB)
                });
            }
            return changes;
        }


        /// <summary>
        /// check if object is already attached
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> primaryId >0 is mandatory </returns>
        public bool IsAttached(object entity)
        {
            return _attachedObjects.ContainsKey(entity.EntityKey());
        }

        /// <summary>
        /// return LightDataTable e.g. DataTable
        /// </summary>
        /// <param name="cmd">sqlCommand that are create from GetSqlCommand</param>
        /// <param name="primaryKey">Table primaryKeyId, so LightDataTable.FindByPrimaryKey could be used </param>
        /// <returns></returns>
        public ILightDataTable GetLightDataTable(DbCommandExtended cmd, string primaryKey = null)
        {
            ValidateConnection();
            var reader = cmd.Command.ExecuteReader();
            return new LightDataTable().ReadData(DataBaseTypes, reader, cmd, primaryKey);
        }

        #region DataBase calls

        /// <summary>
        /// Remove Row
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task DeleteAsync(object entity)
        {
            await Task.Run(() =>
            {
                _dbSchema.DeleteAbstract(entity);
            });
        }

        /// <summary>
        /// Remove Row
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(object entity)
        {
            _dbSchema.DeleteAbstract(entity);
        }

        /// <summary>
        /// Save object
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task SaveAsync(object entity)
        {
            await Task.Run(() =>
            {
                _dbSchema.Save(entity);
            });
        }

        /// <summary>
        /// Save object
        /// </summary>
        /// <param name="entity"></param>
        public void Save(object entity)
        {
            _dbSchema.Save(entity);
        }

        /// <summary>
        /// Get All Rows
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IList<T>> GetAllAsync<T>()
        {
            return await Task.FromResult<IList<T>>(_dbSchema.GetSqlAll(typeof(T)).Cast<T>().ToList());
        }

        /// <summary>
        /// Get All Rows
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> GetAll<T>()
        {
            return _dbSchema.GetSqlAll(typeof(T)).Cast<T>().ToList();
        }


        /// <summary>
        /// select by quarry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public async Task<IList<T>> SelectAsync<T>(string sqlString)
        {
            return await Task.FromResult<IList<T>>(_dbSchema.Select<T>(sqlString));
        }

        /// <summary>
        /// select by quarry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public IList<T> Select<T>(string sqlString)
        {
            return _dbSchema.Select<T>(sqlString);
        }

        /// <summary>
        /// Load Object Children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="classes"></param>
        /// <param name="ignoreList"></param>
        /// <returns></returns>
        public async Task LoadChildrenAsync<T>(T item, bool onlyFirstLevel, List<string> classes, List<string> ignoreList)
        {
            await Task.Run(() =>
            {
                _dbSchema.LoadChildren(item, onlyFirstLevel, classes, ignoreList);
            });

        }

        /// <summary>
        /// Load Object Children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="classes"></param>
        /// <param name="ignoreList"></param>
        public void LoadChildren<T>(T item, bool onlyFirstLevel, List<string> classes, List<string> ignoreList)
        {
            _dbSchema.LoadChildren(item, onlyFirstLevel, classes, ignoreList);
        }

        /// <summary>
        /// Load Object Children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public async Task LoadChildrenAsync<T, TP>(T item, bool onlyFirstLevel = false, List<string> ignoreList = null, params Expression<Func<T, TP>>[] actions)
        {
            await Task.Run(() =>
            {
                var parames = new List<string>();
                if (actions != null)
                    parames = actions.ConvertExpressionToIncludeList();
                LoadChildren<T>(item, onlyFirstLevel, actions != null ? parames : null, ignoreList != null && ignoreList.Any() ? ignoreList : null);

            });
        }

        /// <summary>
        /// Load Object Children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>
        public void LoadChildren<T, TP>(T item, bool onlyFirstLevel = false, List<string> ignoreList = null, params Expression<Func<T, TP>>[] actions)
        {
            var parames = new List<string>();
            if (actions != null)
                parames = actions.ConvertExpressionToIncludeList();
            LoadChildren<T>(item, onlyFirstLevel, actions != null ? parames : null, ignoreList != null && ignoreList.Any() ? ignoreList : null);
        }


        /// <summary>
        /// This will recreate the table and if it has a ForeignKey to other tables it will also recreate those table to
        /// use it wisely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="force"> remove and recreate all</param>

        public void CreateTable<T>(bool force = false)
        {
            _dbSchema.CreateTable(typeof(T), null, force);
        }

        /// <summary>
        /// This will recreate the table and if it has a ForeignKey to other tables it will also recreate those table to
        /// use it wisely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="force"> remove and recreate all</param>

        public void CreateTable(Type type, bool force = false)
        {
            _dbSchema.CreateTable(type, null, force);
        }

        /// <summary>
        /// This will remove the table and if it has a ForeignKey to other tables it will also remove those table to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveTable<T>()
        {
            _dbSchema.RemoveTable(typeof(T));
        }

        /// <summary>
        /// This will remove the table and if it has a ForeignKey to other tables it will also remove those table to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        public void RemoveTable(Type type)
        {
            _dbSchema.RemoveTable(type);
        }

        /// <summary>
        /// Generic Get Quary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISqlQueryable<T> Get<T>()
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new ArgumentNullException("Primary Id not found for object " + typeof(T).FullName);
            return new SqlQueryable<T>(null, this);
        }


        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISqlQueryable<T> FromJson<T>(string json)
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new ArgumentNullException("Primary Id not found for object " + typeof(T).FullName);
            return new SqlQueryable<T>(this, json.FromJson<List<T>>(this));
        }

        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<ISqlQueryable<T>> FromJsonAsync<T>(string json)
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new ArgumentNullException("Primary Id not found for object " + typeof(T).FullName);
            return await Task.FromResult<ISqlQueryable<T>>(new SqlQueryable<T>(this, json.FromJson<List<T>>(this)));
        }

        /// <summary>
        /// Create IQueryable from Expression
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SqlQueryable<TElement>(expression, this) as IQueryable<TElement>;
        }

        /// <summary>
        /// Create IQueryable from Expression
        /// NotImplementedException
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute Quary
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object Execute(Expression expression)
        {
            var _expression = new LightDataLinqToNoSql(expression.Type, this);
            _expression.Translate(expression);
            return _dbSchema.Select(expression.Type, _expression.Quary);

        }

        /// <summary>
        /// Execute generic quary
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            var isEnumerable = (typeof(TResult).Name == "IEnumerable`1");
            var _expression = new LightDataLinqToNoSql(typeof(TResult).GetActualType(), this);
            _expression.Translate(expression);
            if (!isEnumerable)
                return Select<TResult>(_expression.Quary).First();
            else return (TResult)_dbSchema.Select(expression.Type, _expression.Quary);
        }
        #endregion
    }
}