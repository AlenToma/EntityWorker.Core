using System;
using System.Linq.Expressions;
using System.Reflection;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;


namespace EntityWorker.Core.SqlQuerys
{
    public static class Querys
    {
        internal static string GetValueByType(object value)
        {
            if (value == null)
                return string.Format("'{0}'", "null");
            var type = value.GetType();
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float) || type == typeof(int) || type == typeof(long))
                return value.ToString();
            else return string.Format("'{0}'", value);

        }

        public static QueryWhere Select<T>(DataBaseTypes dataBaseTypes)
        {
            var type = typeof(T).GetActualType();
            return new QueryWhere("Select * from " + (type.GetCustomAttribute<Table>()?.Name ?? typeof(T).Name) + " ", dataBaseTypes);
        }

        public static QueryWhere Select(Type type, DataBaseTypes dataBaseTypes)
        {
            type = type.GetActualType();
            return new QueryWhere("Select * from " + (type.GetCustomAttribute<Table>()?.Name ?? type.Name) + " ", dataBaseTypes);
        }

        public static QueryWhere Select(string tableName , DataBaseTypes dataBaseTypes)
        {
            return new QueryWhere("Select * from " + tableName + " ", dataBaseTypes);
        }

        public static QueryItem Where(DataBaseTypes dataBaseTypes = DataBaseTypes.Mssql)
        {
                var item = new QueryItem(" Where ", dataBaseTypes);
                return item;
 
        }
    }



    public class QueryWhere
    {
        private readonly string _sql;

        private DataBaseTypes DataBaseTypes { get; set; }

        public QueryWhere(string sql, DataBaseTypes dataBaseTypes = DataBaseTypes.Mssql)
        {
            _sql = sql;
            DataBaseTypes = dataBaseTypes;
        }
        public QueryItem Where => new QueryItem(_sql + " Where ", DataBaseTypes);

        public string Execute()
        {
            return _sql;
        }
    }

    public sealed class QueryItem
    {
        private string _sql;
        private DataBaseTypes DataBaseTypes { get; set; }
        public QueryItem(string sql, DataBaseTypes dataBaseTypes)
        {
            _sql = sql;
            DataBaseTypes = dataBaseTypes;
        }

        public bool HasValue()
        {
            return _sql.Trim() != "Where";
        }

        public QueryConditions Column(string columnName)
        {
            _sql = _sql + " " + columnName;
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions Column<T>(string columnName)
        {
            var col = columnName;
            if (col.ToLower().Contains(" as "))
                col = col.Remove(col.ToLower().IndexOf(" as ", StringComparison.Ordinal));
            var type = typeof(T);
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                _sql += DataBaseTypes == DataBaseTypes.Mssql ? " " + "CONVERT(decimal(18,5)," + col + ") " : " Cast(" + col + " AS decimal(18,5)) ";
            else if (type == typeof(int) || type == typeof(long))
                _sql += DataBaseTypes == DataBaseTypes.Mssql ? " " + "CONVERT(decimal(18,5)," + col + ") " : " Cast(" + col + " AS bigint) ";
            else _sql += DataBaseTypes == DataBaseTypes.Mssql ? " " + "CONVERT(decimal(18,5)," + col + ") " : " Cast(" + col + " AS nvarchar(4000)) ";

            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions Column<T, P>(Expression<Func<T, P>> action) where T : class
        {
            var member = action.Body is UnaryExpression ? ((MemberExpression)((UnaryExpression)action.Body).Operand) : (action.Body is MethodCallExpression ? ((MemberExpression)((MethodCallExpression)action.Body).Object) : (MemberExpression)action.Body);
            if (member == null) return new QueryConditions(_sql, DataBaseTypes);
            var key = member.Member.Name;
            var type = typeof(P);
            var col = key;
            if (col.ToLower().Contains(" as "))
                col = col.Remove(col.ToLower().IndexOf(" as ", StringComparison.Ordinal));
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                _sql += DataBaseTypes == DataBaseTypes.Mssql ? " " + "CONVERT(decimal(18,5)," + col + ") " : " Cast(" + col + " AS decimal(18,5)) ";
            else if (type == typeof(int) || type == typeof(long))
                _sql += DataBaseTypes == DataBaseTypes.Mssql ? " " + "CONVERT(decimal(18,5)," + col + ") " : " Cast(" + col + " AS bigint) ";
            else _sql += DataBaseTypes == DataBaseTypes.Mssql ? " " + "CONVERT(decimal(18,5)," + col + ") " : " Cast(" + col + " AS nvarchar(4000)) ";
            return new QueryConditions(_sql, DataBaseTypes);
        }


        public QueryConditions ToQueryConditions => new QueryConditions(_sql, DataBaseTypes);

        public string Execute()
        {
            return _sql;
        }
    }

    public sealed class QueryConditions
    {

        private string _sql;
        private DataBaseTypes DataBaseTypes { get; set; }
        public QueryConditions(string sql, DataBaseTypes dataBaseTypes)
        {
            _sql = sql;
            DataBaseTypes = dataBaseTypes;
        }

        public QueryItem ToQueryItem => new QueryItem(_sql, DataBaseTypes);

        public QueryItem And
        {
            get
            {
                _sql += " AND";
                return new QueryItem(_sql, DataBaseTypes);
            }
        }

        public QueryItem Or
        {
            get
            {
                _sql += " OR";
                return new QueryItem(_sql, DataBaseTypes);
            }
        }

        public QueryConditions Comment(string text)
        {
            _sql += string.Format("\n--{0}--\n", text);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions Like(string value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" like '%{0}%'", value);
            else _sql += string.Format(" like '%'+{0}+'%'", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions BeginWith(string value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" like '{0}%'", value);
            else _sql += string.Format(" like {0}+'%'", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }


        public QueryConditions EndWith(string value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" like '%{0}'", value);
            else _sql += string.Format(" like '%'+{0}", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions Equal(object value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" = {0}", Querys.GetValueByType(value));
            else
                _sql += string.Format(" = {0}", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions NotEqual(object value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" != {0}", Querys.GetValueByType(value));
            else _sql += string.Format(" != {0}", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions GreaterThan(object value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" > {0}", Querys.GetValueByType(value));
            else _sql += string.Format(" > {0}", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions SmallerThan(object value, bool isColumn = false)
        {
            if (!isColumn)
                _sql += string.Format(" < {0}", Querys.GetValueByType(value));
            else _sql += string.Format(" < {0}", value);
            return new QueryConditions(_sql, DataBaseTypes);
        }

        public QueryConditions IsNull
        {
            get
            {
                _sql += " is null";
                return new QueryConditions(_sql, DataBaseTypes);
            }
        }

        public QueryConditions IsNotNull
        {
            get
            {
                _sql += " is not null";
                return new QueryConditions(_sql, DataBaseTypes);
            }
        }

        public QueryItem StartBracket
        {
            get
            {
                _sql += "(";
                return new QueryItem(_sql, DataBaseTypes);
            }
        }

        public QueryItem EndBracket
        {
            get
            {
                _sql += ")";
                return new QueryItem(_sql, DataBaseTypes);
            }
        }

        public string Execute()
        {
            return _sql;
        }

    }
}
