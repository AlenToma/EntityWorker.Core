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
using System.IO;
using EntityWorker.Core.Object.Library.Gzip;
using EntityWorker.Core.Object.Library.DataBase;
using EntityWorker.Core.Object.Library.JSON;

namespace EntityWorker.Core.Transaction
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// EntityWorker.Core Repository
    /// </summary>
    public abstract class Transaction : Database, IRepository
    {
        internal Custom_ValueType<IDataReader, bool> OpenedDataReaders = new Custom_ValueType<IDataReader, bool>();

        private static object MigrationLocker = new object();

        internal readonly DbSchema _dbSchema;

        internal readonly Custom_ValueType<string, object> _attachedObjects;

        /// <summary>
        /// DataBase Type
        /// </summary>
        public DataBaseTypes DataBaseTypes { get; private set; }

        internal DbTransaction Trans { get; private set; }

        /// <summary>
        /// DataBase Connection
        /// </summary>
        protected DbConnection SqlConnection { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Full connection string</param>
        /// <param name="dataBaseTypes">The type of the database Ms-sql or Sql-light</param>
        public Transaction(string connectionString, DataBaseTypes dataBaseTypes) : base()
        {
            base._transaction = this;
            _attachedObjects = new Custom_ValueType<string, object>();
            if (string.IsNullOrEmpty(connectionString))
                if (string.IsNullOrEmpty(connectionString))
                    throw new EntityException("ConnectionString can not be empty");

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
        /// <param name="assembly">null for the current Assembly</param>
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
                else throw new EntityException($"EntityWorker.Core could not find IMigrationConfig in the current Assembly {assembly.GetName()}");
                MigrationConfig(config);
            }
        }


        /// <summary>
        /// Return the new added column, tables or modified Properties
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
                throw new EntityException("InitialCatalog can not be null or empty");

            if (DataBaseTypes == DataBaseTypes.PostgreSql && string.IsNullOrEmpty(npSqlBuilder.Database))
                throw new EntityException("Database can not be null or empty");

            if (DataBaseTypes == DataBaseTypes.Mssql)
            {
                sqlBuild.InitialCatalog = "master";
                using (var rep = new DbRepository(sqlBuild.ToString(), DataBaseTypes))
                    return rep.GetSqlCommand($"SELECT  CAST(CASE WHEN db_id(String[{dbName}]) is not null THEN 1 ELSE 0 END AS BIT)").ExecuteScalar().ConvertValue<bool>();

            }
            else if (DataBaseTypes == DataBaseTypes.Sqllight)
            {
                try
                {
                    using (var rep = new DbRepository(sqlBuild.ToString(), DataBaseTypes))
                        rep.ValidateConnection();
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
                using (var rep = new DbRepository(npSqlBuilder.ToString(), DataBaseTypes))
                    return rep.GetSqlCommand($"SELECT CAST(CASE WHEN datname is not null THEN 1 ELSE 0 END AS BIT) from pg_database WHERE lower(datname) = lower(String[{dbName}])").ExecuteScalar().ConvertValue<bool>();
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
                    throw new EntityException("InitialCatalog can not be null or empty");

                if (DataBaseTypes == DataBaseTypes.PostgreSql && string.IsNullOrEmpty(npSqlBuilder.Database))
                    throw new EntityException("Database can not be null or empty");

                if (DataBaseTypes == DataBaseTypes.Mssql)
                {
                    sqlBuild.InitialCatalog = "master";
                    using (var rep = new DbRepository(sqlBuild.ToString(), DataBaseTypes))
                        rep.GetSqlCommand($"Create DataBase [{dbName.Trim()}]").ExecuteNonQuery();
                }
                else if (DataBaseTypes == DataBaseTypes.Sqllight)
                    SQLiteConnection.CreateFile(dbName.Trim());
                else
                {
                    dbName = npSqlBuilder.Database;
                    npSqlBuilder.Database = "";
                    using (var rep = new DbRepository(npSqlBuilder.ToString(), DataBaseTypes))
                        rep.GetSqlCommand($"Create DataBase {dbName}").ExecuteNonQuery();
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
        /// the migration is executed automatic(InitializeMigration()) as long as you have class that inherit from IMigrationConfig
        /// or you could manually execute a migration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        protected void MigrationConfig<T>(T config) where T : IMigrationConfig
        {
            try
            {
                GlobalConfiguration.Log?.Info("Migration", "Initialize");
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
                    this.CreateTransaction();
                    migration.ExecuteMigration(this);
                    this.Save(item);

                }
                SaveChanges();
            }
            catch (Exception e)
            {
                GlobalConfiguration.Log?.Error(e);
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
        /// Validate Connection is Open or broken then reopen it
        /// </summary>
        protected void ValidateConnection()
        {
            try
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
            catch (Exception e)
            {
                throw new EntityException(e.Message);
            }
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
        public ISqlQueryable<T> DataReaderConverter<T>(ISqlCommand command)
        {
            GlobalConfiguration.Log?.Info("Execute", command);
            return new SqlQueryable<T>(this, ((List<T>)DataReaderConverter(command, typeof(T))));
        }

        /// <summary>
        /// Convert to unknown type
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList DataReaderConverter(ISqlCommand command, Type type)
        {
            IList result;
            ValidateConnection();
            GlobalConfiguration.Log?.Info("Execute", command);
            try
            {
                var o = command.Command.ExecuteReader();
                OpenedDataReaders.GetOrAdd(o, true);
                result = Extension.DataReaderConverter(this, o, command, type);
            }
            catch (Exception e)
            {
                throw new EntityException(e.Message);
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
        public object ExecuteScalar(ISqlCommand cmd)
        {
            ValidateConnection();
            var o = cmd.Command.ExecuteScalar();
            CloseifPassible();
            return o;
        }

        /// <inheritdoc />
        public int ExecuteNonQuery(ISqlCommand cmd)
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
            catch (Exception ex)
            {
                Rollback();
                throw new EntityException(ex.Message);
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
        /// DbType By System.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public DbType GetDbType(Type type)
        {
            if (type == typeof(string))
                return DbType.String;

            if (type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);
            try
            {
                var param = new SqlParameter("", type == typeof(byte[]) ? new byte[0] : type.CreateInstance(true));
                return param.DbType;
            }
            catch
            {
                var param = new SqlParameter("", new object());
                return param.DbType;
            }
        }

        /// <summary>
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        public IRepository AddInnerParameter(ISqlCommand cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar)
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
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        public IRepository AddInnerParameter(ISqlCommand cmd, string attrName, object value, DbType dbType)
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
                    DbType = dbType,
                    Value = sqlDbTypeValue,
                    ParameterName = attrName
                };
                cmd.Command.Parameters.Add(param);
            }
            else if (DataBaseTypes == DataBaseTypes.Sqllight)
            {
                (cmd.Command as SQLiteCommand).Parameters.Add(new SQLiteParameter()
                {
                    DbType = dbType,
                    Value = value ?? DBNull.Value,
                    ParameterName = attrName
                });
            }
            else
            {
                (cmd.Command as NpgsqlCommand).Parameters.Add(new NpgsqlParameter()
                {
                    DbType = dbType,
                    Value = value ?? DBNull.Value,
                    ParameterName = attrName
                });
            }

            return this;
        }

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public ISqlCommand GetSqlCommand(string sql)
        {
            ValidateConnection();
            return this.ProcessSql(SqlConnection, Trans, sql);
        }

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="storedProcedure">Name</param>
        /// <returns></returns>
        public ISqlCommand GetStoredProcedure(string storedProcedure)
        {
            ValidateConnection();
            var cmd = this.ProcessSql(SqlConnection, Trans, storedProcedure);
            cmd.Command.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        /// <summary>
        /// return a list of LightDataTable e.g. DataSet
        /// </summary>
        /// <param name="cmd">sqlCommand that are create from GetSqlCommand</param>
        /// <param name="primaryKeyId"> Table primaryKeyId, so LightDataTable.FindByPrimaryKey could be used </param>
        /// <returns></returns>
        protected List<ILightDataTable> GetLightDataTableList(ISqlCommand cmd, string primaryKeyId = null)
        {
            GlobalConfiguration.Log?.Info("Execute", cmd);
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
        internal void Attach(object objcDbEntity, bool overwrite = false)
        {
            var key = objcDbEntity.EntityKey();
            GlobalConfiguration.Log?.Info("Attaching", key);
            if (objcDbEntity == null)
                throw new EntityException("DbEntity cant be null");
            if (Extension.ObjectIsNew(objcDbEntity.GetPrimaryKeyValue()))
                throw new EntityException("Id is IsNullOrEmpty, it cant be attached");

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
            var key = objcDbEntity.EntityKey();
            GlobalConfiguration.Log?.Info("Attaching", key);
            if (objcDbEntity == null)
                throw new EntityException("DbEntity cant be null");
            if (objcDbEntity.GetPrimaryKey() == null || Extension.ObjectIsNew(objcDbEntity.GetPrimaryKeyValue()))
                return;
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
                throw new EntityException("Object is not attached");
            var originalObject = _attachedObjects[entity.EntityKey()];
            if (originalObject == null)
                throw new EntityException("Object need to be attached");
            foreach (var prop in DeepCloner.GetFastDeepClonerProperties(entity.GetType()))
            {
                var aValue = prop.GetValue(entity);
                var bValue = prop.GetValue(originalObject);
                if ((!prop.IsInternalType &&
                    !prop.ContainAttribute<JsonDocument>()) ||
                    prop.ContainAttribute<ExcludeFromAbstract>() ||
                    prop.ContainAttribute<PrimaryKey>() ||
                    prop.ContainAttribute<XmlDocument>() ||
                    (aValue == null && bValue == null))
                    continue;
                if ((aValue != null && aValue.Equals(bValue)) || (bValue != null && bValue.Equals(aValue)))
                    continue;
                changes.Add(new EntityChanges
                {
                    EntityType = entity.GetType(),
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
                throw new EntityException("Object need to be attached");
            foreach (var prop in DeepCloner.GetFastDeepClonerProperties(entityA.GetType()))
            {
                var aValue = prop.GetValue(entityB);
                var bValue = prop.GetValue(originalObject);
                if ((!prop.IsInternalType && !prop.ContainAttribute<JsonDocument>()) ||
                    prop.ContainAttribute<ExcludeFromAbstract>() ||
                    prop.ContainAttribute<PrimaryKey>() ||
                    prop.ContainAttribute<XmlDocument>() ||
                    (aValue == null && bValue == null))
                    continue;
                if ((aValue != null && aValue.Equals(bValue)) || (bValue != null && bValue.Equals(aValue)))
                    continue;
                changes.Add(new EntityChanges
                {
                    EntityType = entityB.GetType(),
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
        /// check if object is already attached
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> primaryId >0 is mandatory </returns>
        public bool IsAttached(string key)
        {
            return _attachedObjects.ContainsKey(key);
        }

        /// <summary>
        /// return LightDataTable e.g. DataTable
        /// </summary>
        /// <param name="cmd">sqlCommand that are create from GetSqlCommand</param>
        /// <param name="primaryKey">Table primaryKeyId, so LightDataTable.FindByPrimaryKey could be used </param>
        /// <returns></returns>
        public ILightDataTable GetLightDataTable(ISqlCommand cmd, string primaryKey = null)
        {
            GlobalConfiguration.Log?.Info("Execute", cmd);
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
        public virtual async Task<IRepository> DeleteAsync(object entity)
        {
            await Task.Run(() =>
            {
                _dbSchema.DeleteAbstract(entity);
            });
            return await Task.FromResult<IRepository>(this);
        }

        /// <summary>
        /// Remove Row
        /// </summary>
        /// <param name="entity"></param>
        public virtual IRepository Delete(object entity)
        {
            _dbSchema.DeleteAbstract(entity);
            return this;
        }

        /// <summary>
        /// Save Entity 
        /// ignore/execlude updateing some properties to the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="ignoredProperties"></param>
        /// <returns></returns>
        public virtual IRepository Save<T>(T entity, params Expression<Func<T, object>>[] ignoredProperties)
        {
            var parames = new List<string>();
            if (ignoredProperties != null)
                parames = ignoredProperties.ConvertExpressionToIncludeList(true);

            _dbSchema.Save(entity, parames);
            return this;
        }

        /// <summary>
        /// Save Entity 
        /// ignore/execlude updateing some properties to the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="ignoredProperties"></param>
        /// <returns></returns>
        public virtual async Task<IRepository> SaveAsync<T>(T entity, params Expression<Func<T, object>>[] ignoredProperties)
        {
            await Task.Run(() =>
            {
                this.Save(entity, ignoredProperties);
            });
            return await Task.FromResult<IRepository>(this);
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

        public IRepository CreateTable<T>(bool force = false)
        {
            _dbSchema.CreateTable(typeof(T), null, force);

            return this;
        }

        /// <summary>
        /// This will recreate the table and if it has a ForeignKey to other tables it will also recreate those table to
        /// use it wisely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="force"> remove and recreate all</param>

        public IRepository CreateTable(Type type, bool force = false)
        {
            _dbSchema.CreateTable(type, null, force);

            return this;
        }

        /// <summary>
        /// This will remove the table and if it has a ForeignKey to other tables it will also remove those table to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public IRepository RemoveTable<T>()
        {
            _dbSchema.RemoveTable(typeof(T));
            return this;
        }

        /// <summary>
        /// This will remove the table and if it has a ForeignKey to other tables it will also remove those table to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        public IRepository RemoveTable(Type type)
        {
            _dbSchema.RemoveTable(type);
            return this;
        }

        /// <summary>
        /// Generic Get Quary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISqlQueryable<T> Get<T>()
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new EntityException("Primary Id not found for object " + typeof(T).FullName);
            return new SqlQueryable<T>(null, this);
        }

        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param">The Default is GlobalConfigration.JSONParameters</param>
        /// <returns></returns>
        public ISqlQueryable<T> FromJson<T>(string json, JSONParameters param = null) where T : class
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new EntityException("Primary Id not found for object " + typeof(T).FullName);
            return new SqlQueryable<T>(this, json.FromJson<List<T>>(this, param));
        }

        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param">The Default is GlobalConfigration.JSONParameters</param>
        /// <returns></returns>
        public async Task<ISqlQueryable<T>> FromJsonAsync<T>(string json, JSONParameters param = null) where T : class
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new EntityException("Primary Id not found for object " + typeof(T).FullName);
            return await Task.FromResult<ISqlQueryable<T>>(new SqlQueryable<T>(this, json.FromJson<List<T>>(this, param)));
        }

        /// <summary>
        /// Get ISqlQueryable from Xml.
        /// All XmlIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISqlQueryable<T> FromXml<T>(string xmlString) where T : class
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new EntityException("Primary Id not found for object " + typeof(T).FullName);
            return new SqlQueryable<T>(this, xmlString.FromXml<List<T>>(this));
        }

        /// <summary>
        /// Get ISqlQueryable from Xml.
        /// All XmlIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<ISqlQueryable<T>> FromXmlAsync<T>(string xmlString) where T : class
        {
            if (typeof(T).GetPrimaryKey() == null)
                throw new EntityException("Primary Id not found for object " + typeof(T).FullName);
            return await Task.FromResult<ISqlQueryable<T>>(new SqlQueryable<T>(this, xmlString.FromXml<List<T>>(this)));
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
            var _expression = new LinqToSql(expression.Type.GetActualType(), this);
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
            var _expression = new LinqToSql(typeof(TResult).GetActualType(), this);
            _expression.Translate(expression);
            if (!isEnumerable)
                return Select<TResult>(_expression.Quary).First();
            else return (TResult)_dbSchema.Select(expression.Type, _expression.Quary);
        }

        #endregion

        #region Package Handler
        /// <summary>
        /// Create Protected package that contain files or data for backup purpose or moving data from one location to another.
        /// Note that this package can only be readed by EntityWorker.Core
        /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public byte[] CreatePackage<T>(T package) where T : PackageEntity
        {
            if (package == null)
                throw new EntityException("Package cant be null");
            using (var mem = new MemoryStream())
            {
                using (var db = new LiteDB.LiteDatabase(mem))
                {
                    var packageCollection = db.GetCollection<T>("Packages");
                    packageCollection.Insert(package);
                    return GzipUtility.Compress(new ByteCipher(GlobalConfiguration.PackageDataEncode_Key, DataCipherKeySize.Key_128).Encrypt(mem.ToArray()));
                }
            }
        }

        /// <summary>
        /// Read the package and get its content
        /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public T GetPackage<T>(byte[] package) where T : PackageEntity
        {
            try
            {
                var uncompressedFile = GzipUtility.Decompress(package);
                using (var msi = new MemoryStream(new ByteCipher(GlobalConfiguration.PackageDataEncode_Key, DataCipherKeySize.Key_128).Decrypt(uncompressedFile)))
                {
                    // now read the file
                    using (var db = new LiteDB.LiteDatabase(msi))
                    {
                        var packageCollection = db.GetCollection<T>("Packages");
                        return packageCollection.FindOne(x => x.Data != null || x.Files != null);
                    }
                }

            }
            catch (Exception exception)
            {
                throw new EntityException($"Error the package structure is not valid.\n Orginal exception\n{exception.Message}");
            }
        }



        #endregion
    }
}