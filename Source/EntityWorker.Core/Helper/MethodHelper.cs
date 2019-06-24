using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.Interface;
using EntityWorker.Core.Object.Library.DbWrapper;
using FastDeepCloner;

namespace EntityWorker.Core.Helper
{
    /// <summary>
    /// UseFull Methods 
    /// </summary>
    public static class MethodHelper
    {

        ///// <summary>
        /////  Get All types that containe Property with PrimaryKey Attribute
        ///// </summary>
        ///// <param name="assembly"></param>
        ///// <returns></returns>
        public static List<Type> GetDbEntitys(Assembly assembly) => assembly?.DefinedTypes.Where(type => type.GetPrimaryKey() != null).Cast<Type>().ToList();

        /// <summary>
        /// Convert Value from Type to Type
        /// when fail a default value will be loaded.
        /// can handle all known types like datetime, time span, string, long etc
        /// ex
        ///  "1115rd" to int? will return null
        ///  "152" to int? 152
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        public static T ConvertValue<T>(this object value) => (T)ConvertValue(value, typeof(T));

        /// <summary>
        /// Convert Value from Type to Type
        /// when fail a default value will be loaded.
        /// can handle all known types like datetime, time span, string, long etc
        /// ex
        ///  "1115rd" to int? will return null
        ///  "152" to int? 152
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        public static object ConvertValue(this object value, Type toType)
        {
            var data = new LightDataTable();
            data.AddColumn("value", toType, value);
            data.AddRow(new object[1] { value });
            return data.Rows.First().TryValueAndConvert(toType, "value", true);
        }

        /// <summary>
        /// Convert String ToBase64String
        /// </summary>
        /// <param name="stringToEncode"></param>
        /// <returns></returns>
        public static string EncodeStringToBase64(string stringToEncode)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringToEncode));
        }

        /// <summary>
        /// Convert Base64String To String 
        /// </summary>
        /// <param name="stringToDecode"></param>
        /// <returns></returns>
        public static string DecodeStringFromBase64(string stringToDecode)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(stringToDecode));
        }

        private static Regex stringExp = new Regex(@"String\[.*?\]|String\[.?\]");
        private static Regex dateExp = new Regex(@"Date\[.*?\]|Date\[.?\]");
        private static Regex guidExp = new Regex(@"Guid\[.*?\]|Guid\[.?\]");
        internal static ISqlCommand ProcessSql(this IRepository repository, TransactionSqlConnection connection, IDbTransaction tran, string sql)
        {
            var i = 1;
            var dicCols = new SafeValueType<string, Tuple<object, SqlDbType>>();
            MatchCollection matches = null;
            while ((matches = stringExp.Matches(sql)).Count > 0)
            {
                var exp = matches[0];
                var col = "@CO" + i + "L";
                object str = exp.Value.TrimEnd(']').Substring(@"String\[".Length - 1);
                sql = sql.Remove(exp.Index, exp.Value.Length);
                sql = sql.Insert(exp.Index, col);
                dicCols.TryAdd(col, new Tuple<object, SqlDbType>(str.ConvertValue<string>(), SqlDbType.NVarChar));
                i++;
            }

            while ((matches = dateExp.Matches(sql)).Count > 0)
            {
                var exp = matches[0];
                var col = "@CO" + i + "L";
                object str = exp.Value.TrimEnd(']').Substring(@"Date\[".Length - 1);
                sql = sql.Remove(exp.Index, exp.Value.Length);
                sql = sql.Insert(exp.Index, col);
                dicCols.TryAdd(col, new Tuple<object, SqlDbType>(str.ConvertValue<DateTime?>(), SqlDbType.DateTime));
                i++;
            }

            while ((matches = guidExp.Matches(sql)).Count > 0)
            {
                var exp = matches[0];
                var col = "@CO" + i + "L";
                object str = exp.Value.TrimEnd(']').Substring(@"Guid\[".Length - 1);
                sql = sql.Remove(exp.Index, exp.Value.Length);
                sql = sql.Insert(exp.Index, col);
                dicCols.TryAdd(col, new Tuple<object, SqlDbType>(str.ConvertValue<Guid?>(), SqlDbType.UniqueIdentifier));
                i++;
            }

            sql = sql.CleanValidSqlName(repository.DataBaseTypes);
            DbCommand cmd = null;

            try
            {
                switch (repository.DataBaseTypes)
                {
                    case DataBaseTypes.Mssql:
                        cmd = tran != null ?
                            "System.Data.SqlClient.SqlCommand".GetObjectType("System.Data.SqlClient").CreateInstance(new object[] { sql, connection.DBConnection, tran }) as DbCommand :
                                  "System.Data.SqlClient.SqlCommand".GetObjectType("System.Data.SqlClient").CreateInstance(new object[] { sql, connection.DBConnection }) as DbCommand;
                        break;

                    case DataBaseTypes.PostgreSql:
                        cmd = tran != null ?
                           "Npgsql.NpgsqlCommand".GetObjectType("Npgsql").CreateInstance(new object[] { sql, connection.DBConnection, tran }) as DbCommand :
                                 "Npgsql.NpgsqlCommand".GetObjectType("Npgsql").CreateInstance(new object[] { sql, connection.DBConnection }) as DbCommand;
                        break;

                    case DataBaseTypes.Sqllight:
                        cmd = tran != null ?
                             "System.Data.SQLite.SQLiteCommand".GetObjectType("System.Data.SQLite").CreateInstance(new object[] { sql, connection.DBConnection, tran }) as DbCommand :
                                   "System.Data.SQLite.SQLiteCommand".GetObjectType("System.Data.SQLite").CreateInstance(new object[] { sql, connection.DBConnection }) as DbCommand;
                        break;
                }
            }
            catch (Exception e)
            {
                switch (repository.DataBaseTypes)
                {
                    case DataBaseTypes.Mssql:
                        throw new EntityException($"Please make sure that nuget 'System.Data.SqlClient' is installed \n orginal exception: \n {e.Message}");

                    case DataBaseTypes.PostgreSql:
                        throw new EntityException($"Please make sure that nuget 'Npgsql' is installed \n orginal exception: \n {e.Message}");

                    case DataBaseTypes.Sqllight:
                        throw new EntityException($"Please make sure that nuget 'System.Data.SQLite' is installed \n orginal exception: \n {e.Message}");
                }
            }

            var dbCommandExtended = new DbCommandExtended(cmd, repository);
            foreach (var dic in dicCols)
                dbCommandExtended.AddInnerParameter(dic.Key, dic.Value.Item1);
            return dbCommandExtended;
        }
    }
}
