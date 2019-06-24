using EntityWorker.Core.Helper;
using FastDeepCloner;
using System;
using System.Data;
using System.Data.Common;

namespace EntityWorker.Core.Object.Library.DbWrapper
{
    public sealed class TransactionSqlConnection : IDbConnection
    {
        public DataBaseTypes DataBaseType { get; private set; }

        internal readonly DbConnection DBConnection;
        public string ConnectionString { get => DBConnection.ConnectionString; set => DBConnection.ConnectionString = value; }

        public string Database => DBConnection.Database;

        public string DataSource => DBConnection.DataSource;

        public string ServerVersion => DBConnection.ServerVersion;

        public ConnectionState State => DBConnection.State;

        public int ConnectionTimeout => DBConnection.ConnectionTimeout;

        public TransactionSqlConnection(DataBaseTypes dataBaseType, string connectionString)
        {
            DataBaseType = dataBaseType;
            try
            {
                switch (DataBaseType)
                {
                    case DataBaseTypes.Mssql:
                        DBConnection = "System.Data.SqlClient.SqlConnection".GetObjectType("System.Data.SqlClient").CreateInstance(new[] { connectionString }) as DbConnection;
                        break;

                    case DataBaseTypes.PostgreSql:
                        DBConnection = "Npgsql.NpgsqlConnection".GetObjectType("Npgsql").CreateInstance(new[] { connectionString }) as DbConnection;
                        break;

                    case DataBaseTypes.Sqllight:
                        DBConnection = "System.Data.SQLite.SQLiteConnection".GetObjectType("System.Data.SQLite").CreateInstance(new[] { connectionString }) as DbConnection;
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

        public IDbTransaction BeginTransaction()
        {
            return DBConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return DBConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            DBConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            DBConnection.Close();
        }

        public IDbCommand CreateCommand()
        {
            return DBConnection.CreateCommand();
        }

        public void Open()
        {
            DBConnection.Open();
        }

        public void Dispose()
        {
            DBConnection.Dispose();
        }

        public override string ToString()
        {
            return DBConnection.ToString();
        }


    }
}
