using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.SQLite;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.Postgres;

namespace EntityWorker.Core.Helper
{
    /// <summary>
    /// UseFull Methods 
    /// </summary>
    public static class MethodHelper
    {

        ///// <summary>
        /////  Get All types that containe Property with PrimaryId Attribute
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
        internal static DbCommandExtended ProcessSql(this IRepository repository, IDbConnection connection, IDbTransaction tran, string sql)
        {
            var i = 1;
            var dicCols = new Custom_ValueType<string, Tuple<object, SqlDbType>>();
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
                dicCols.TryAdd(col, new Tuple<object, SqlDbType>(str.ConvertValue<DateTime>(), SqlDbType.DateTime));
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
            if (repository.DataBaseTypes == DataBaseTypes.Mssql)
                cmd = tran != null ? new SqlCommand(sql, connection as SqlConnection, tran as SqlTransaction) : new SqlCommand(sql, connection as SqlConnection);
            else if (repository.DataBaseTypes == DataBaseTypes.Sqllight)
                cmd = tran == null ? new SQLiteCommand(sql, connection as SQLiteConnection) : new SQLiteCommand(sql, connection as SQLiteConnection, tran as SQLiteTransaction);
            else cmd = tran == null ? new NpgsqlCommand(sql, connection as NpgsqlConnection) : new NpgsqlCommand(sql, connection as NpgsqlConnection, tran as NpgsqlTransaction);
            var dbCommandExtended = new DbCommandExtended(cmd, repository);
            foreach (var dic in dicCols)
                repository.AddInnerParameter(dbCommandExtended, dic.Key, dic.Value.Item1, dic.Value.Item2);
            return dbCommandExtended;
        }
    }
}
