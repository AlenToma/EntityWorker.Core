using EntityWorker.Core.Helper;
using System;
using System.Collections.Generic;
using EntityWorker.Core.Postgres;

namespace EntityWorker.Core.Object.Library.DataBase
{
    public abstract class Database
    {
        internal static readonly Custom_ValueType<string, Custom_ValueType<string, ColumnSchema>> CachedColumnSchema = new Custom_ValueType<string, Custom_ValueType<string, ColumnSchema>>();

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
        /// Get the ColumnSchema  
        /// </summary>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public Custom_ValueType<string, ColumnSchema> GetColumnSchema(Type datatype)
        {
            try
            {
                var key = datatype.FullName + _transaction.DataBaseTypes.ToString();
                if (CachedColumnSchema.ContainsKey(key))
                    return CachedColumnSchema[key];
                datatype = datatype.GetActualType();
                var tableName = datatype.TableName();
                var sql = $"SELECT COLUMN_NAME as columnname, data_type as datatype ,TABLE_CATALOG as db,TABLE_NAME as tb , IS_NULLABLE as isnullable FROM INFORMATION_SCHEMA.COLUMNS WHERE LOWER(TABLE_NAME) = LOWER(String[{tableName}])";
                if (_transaction.DataBaseTypes == DataBaseTypes.Sqllight)
                    sql = $"SELECT name  as columnname, type as datatype FROM pragma_table_info(String[{tableName}])";

                var columns = (List<ColumnSchema>)_transaction.DataReaderConverter(_transaction.GetSqlCommand(sql), typeof(ColumnSchema));
                var dic = new Custom_ValueType<string, ColumnSchema>();
                if (columns != null)
                    columns.ForEach(x => dic.Add(x.ColumnName, x));
                return CachedColumnSchema.GetOrAdd(key, dic);
            }
            catch (NpgsqlException)
            {
                _transaction.Renew();
                return GetColumnSchema(datatype);
            }
        }

        /// <summary>
        /// Get the ColumnSchema  
        /// </summary>
        /// <typeparam name="T"> datatype ed Users</typeparam>
        /// <returns></returns>
        public Custom_ValueType<string, ColumnSchema> GetColumnSchema<T>()
        {
            return GetColumnSchema(typeof(T));
        }

    }
}
