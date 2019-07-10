using EntityWorker.Core.Helper;
using FastDeepCloner;
using System;
using System.Data.Common;

namespace EntityWorker.Core.Object.Library.DbWrapper
{
    public sealed class TransactionConnectionString
    {

        private readonly SafeValueType<DataBaseTypes, IFastDeepClonerProperty> _initialCatalog = new SafeValueType<DataBaseTypes, IFastDeepClonerProperty>();
        private readonly DbConnectionStringBuilder dbConnectionStringBuilder;
        public DataBaseTypes DataBaseType { get; private set; }
        public string InitialCatalog {
            get => (string)_initialCatalog[DataBaseType].GetValue(dbConnectionStringBuilder);
            set => _initialCatalog[DataBaseType].SetValue(dbConnectionStringBuilder, value); }


        public override string ToString()
        {
            return dbConnectionStringBuilder.ToString();
        }
        public TransactionConnectionString(DataBaseTypes dataBaseType, string connectionString)
        {
            DataBaseType = dataBaseType;
            try
            {
                switch (DataBaseType)
                {
                    case DataBaseTypes.Mssql:
                        dbConnectionStringBuilder = "System.Data.SqlClient.SqlConnectionStringBuilder".GetObjectType("System.Data.SqlClient").CreateInstance(new[] { connectionString }) as DbConnectionStringBuilder;
                        if (!_initialCatalog.ContainsKey(DataBaseType))
                        {
                            _initialCatalog.TryAdd(DataBaseType, DeepCloner.GetProperty(dbConnectionStringBuilder.GetType(), "InitialCatalog"));
                        }
                        break;

                    case DataBaseTypes.PostgreSql:
                        dbConnectionStringBuilder = "Npgsql.NpgsqlConnectionStringBuilder".GetObjectType("Npgsql").CreateInstance(new[] { connectionString }) as DbConnectionStringBuilder;

                        if (!_initialCatalog.ContainsKey(DataBaseType))
                        {
                            _initialCatalog.TryAdd(DataBaseType, DeepCloner.GetProperty(dbConnectionStringBuilder.GetType(), "Database"));
                        }
                        break;

                    case DataBaseTypes.Sqllight:
                        dbConnectionStringBuilder = "System.Data.SQLite.SQLiteConnectionStringBuilder".GetObjectType("System.Data.SQLite").CreateInstance(new[] { connectionString }) as DbConnectionStringBuilder;
                        if (!_initialCatalog.ContainsKey(DataBaseType))
                        {
                            _initialCatalog.TryAdd(DataBaseType, DeepCloner.GetProperty(dbConnectionStringBuilder.GetType(), "DataSource"));
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                switch (DataBaseType)
                {
                    case DataBaseTypes.Mssql:
                        throw new EntityException($"Please make sure that nuget 'System.Data.SqlClient' is installed \n orginal exception: \n {e.Message}");

                    case DataBaseTypes.PostgreSql:
                        throw new EntityException($"Please make sure that nuget 'Npgsql' is installed \n orginal exception: \n {e.Message}");
                    /// create dbConnection from NpgsqlConnection

                    case DataBaseTypes.Sqllight:
                        throw new EntityException($"Please make sure that nuget 'System.Data.SQLite' is installed \n orginal exception: \n {e.Message}");
                }
            }
        }
    }
}
