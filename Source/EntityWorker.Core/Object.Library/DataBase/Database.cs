using EntityWorker.Core.Helper;
using FastDeepCloner;
using System;
using System.Collections.Generic;

namespace EntityWorker.Core.Object.Library.DataBase
{
    public abstract class Database
    {
        internal static readonly SafeValueType<string, SafeValueType<string, ColumnSchema>> CachedColumnSchema = new SafeValueType<string, SafeValueType<string, ColumnSchema>>();

        protected Transaction.Transaction _transaction { get; set; }

        /// <summary>
        /// DataBase FullConnectionString
        /// </summary>
        public string ConnectionString { get; protected set; }

        protected static Dictionary<DataBaseTypes, bool> _moduleIni = new Dictionary<DataBaseTypes, bool>()
        {
            { DataBaseTypes.Mssql, false },
            { DataBaseTypes.Sqllight, false },
            { DataBaseTypes.PostgreSql, false }
        };

        protected static Dictionary<DataBaseTypes, bool> _tableMigrationCheck = new Dictionary<DataBaseTypes, bool>()
        {
            { DataBaseTypes.Mssql, false },
            { DataBaseTypes.Sqllight, false },
            { DataBaseTypes.PostgreSql, false },
        };

        internal Database()
        {

        }

        /// <summary>
        /// create schema if not exist
        /// </summary>
        /// <param name="type"></param>
        public void CreateSchema(Type type)
        {
            try
            {
                var schema = type.GetActualType().TableName().Schema;
                if (string.IsNullOrEmpty(schema) || _transaction.DataBaseTypes == DataBaseTypes.Sqllight)
                    return;
                var sql = _transaction.GetSqlCommand(_transaction.DataBaseTypes == DataBaseTypes.Mssql ?
                    $"SELECT 1 FROM sys.schemas WHERE name = String[{schema}]" :
                    $"SELECT 1 FROM information_schema.schemata WHERE schema_name = String[{schema}]");
                var res = _transaction.ExecuteScalar(sql);
                if (res?.ToString() == "1")
                    return;
                else
                {
                    _transaction.ExecuteNonQuery(_transaction.GetSqlCommand("CREATE SCHEMA " + schema + ""));
                }
            }
            catch (Exception e)
            {
                throw new EntityException(e.Message);
            }

        }

        /// <summary>
        /// Get the ColumnSchema  
        /// </summary>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public SafeValueType<string, ColumnSchema> GetColumnSchema(Type datatype)
        {
            try
            {
                
                var key = datatype.FullName + _transaction.DataBaseTypes.ToString();
                if (CachedColumnSchema.ContainsKey(key))
                    return CachedColumnSchema[key];
                datatype = datatype.GetActualType();
                var tableName = datatype.TableName();
                var sql = $"SELECT COLUMN_NAME as columnname, data_type as datatype ,TABLE_CATALOG as db,TABLE_NAME as tb , IS_NULLABLE as isnullable FROM INFORMATION_SCHEMA.COLUMNS WHERE LOWER(TABLE_NAME) = LOWER(String[{tableName.Name}])";
                if (_transaction.DataBaseTypes == DataBaseTypes.Sqllight)
                    sql = $"SELECT name  as columnname, type as datatype FROM pragma_table_info(String[{tableName.Name}])";
                var columns = (List<ColumnSchema>)_transaction.DataReaderConverter(_transaction.GetSqlCommand(sql), typeof(ColumnSchema));
                var dic = new SafeValueType<string, ColumnSchema>();
                if (columns != null)
                    columns.ForEach(x => dic.Add(x.ColumnName, x));
                _transaction.CounterException = 0;
                return CachedColumnSchema.GetOrAdd(key, dic);
            }
            catch (Exception e)
            {
                if (_transaction.DataBaseTypes == DataBaseTypes.PostgreSql && _transaction.CounterException <= 3)
                {
                    _transaction.CounterException++;
                    _transaction.Renew();
                    return GetColumnSchema(datatype);
                }
                else throw new EntityException(e.Message);
            }
        }

        /// <summary>
        /// Get the ColumnSchema  
        /// </summary>
        /// <typeparam name="T"> datatype ed Users</typeparam>
        /// <returns></returns>
        public SafeValueType<string, ColumnSchema> GetColumnSchema<T>()
        {
            return GetColumnSchema(typeof(T));
        }

    }
}
