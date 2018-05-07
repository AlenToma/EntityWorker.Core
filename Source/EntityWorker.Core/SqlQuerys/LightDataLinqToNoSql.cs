using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.FastDeepCloner;

namespace EntityWorker.Core.SqlQuerys
{
    internal class LightDataLinqToNoSql : ExpressionVisitor
    {
        BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private Transaction.Transaction _transaction;
        private StringBuilder sb;
        private ExpressionType? _overridedNodeType;
        private readonly List<string> _columns;
        private static string stringyFy = "<Stringify>[#]</Stringify>";
        private static Regex StringyFyExp = new Regex(@"<Stringify>\[.*?\]</Stringify>");
        private static string boolString = "<bool>[#]</bool>";
        private static Regex BoolExp = new Regex(@"<bool>\[.*?\]</bool>");
        internal static Custom_ValueType<Type, List<string>> CachedColumns = new Custom_ValueType<Type, List<string>>();

        private static string dataEncodeString = "<DataEncode>[#]</DataEncode>";
        private static Regex DataEncodeExp = new Regex(@"<DataEncode>\[.*?\]</DataEncode>");
        private string _primaryId;

        private static Custom_ValueType<string, Type> SavedTypes = new Custom_ValueType<string, Type>();
        public Custom_ValueType<string, Tuple<string, string>> JoinClauses { get; private set; } = new Custom_ValueType<string, Tuple<string, string>>();

        public int Skip { get; set; }

        public int Take { get; set; } = Int32.MaxValue;

        public string OrderBy { get; set; }

        public List<string> WhereClause { get; private set; } = new List<string>();

        public DataBaseTypes DataBaseTypes { get; set; }

        private Type _obType;

        public LightDataLinqToNoSql(Type type, Transaction.Transaction transaction)
        {

            _transaction = transaction;
            _obType = type.GetActualType();
            if (!CachedColumns.ContainsKey(_obType))
                _columns = CachedColumns.GetOrAdd(_obType, _transaction.GetColumnSchema(_obType).Select(x => $"[{_obType.TableName()}].[{x.Key}]").ToList());
            else
                _columns = CachedColumns[_obType];
            _primaryId = OrderBy = _obType.GetPrimaryKey()?.GetPropertyName();

        }

        public LightDataLinqToNoSql(DataBaseTypes dataBaseTypes)
        {
            DataBaseTypes = dataBaseTypes;
            if (!CachedColumns.ContainsKey(_obType))
                _columns = CachedColumns.GetOrAdd(_obType, _transaction.GetColumnSchema(_obType).Select(x => $"[{_obType.TableName()}].[{x.Key}]").ToList());
            else
                _columns = CachedColumns[_obType];
            OrderBy = _obType.GetPrimaryKey().GetPropertyName();
        }

        public string Quary
        {
            get
            {
                WhereClause.RemoveAll(x => string.IsNullOrEmpty(x));
                var tableName = _obType.TableName();
                var query = "SELECT " + string.Join(",", _columns) + " FROM " + DataBaseTypes.GetValidSqlName(tableName) + " " + System.Environment.NewLine +
                       string.Join(Environment.NewLine, JoinClauses.Values.Select(x => x.Item2)) +
                       Environment.NewLine + (WhereClause.Any() ? "WHERE " : string.Empty) + string.Join(" AND ", WhereClause.ToArray());
                query = query.TrimEnd(" AND ").TrimEnd(" OR ");

                query += Environment.NewLine + $"GROUP BY {string.Join(",", _columns)} ";

                if (!string.IsNullOrEmpty(OrderBy))
                    query += Environment.NewLine + "ORDER BY " + OrderBy;
                else query += Environment.NewLine + "ORDER BY 1 ASC";

                if (DataBaseTypes == DataBaseTypes.Mssql || DataBaseTypes == DataBaseTypes.PostgreSql)
                    query += Environment.NewLine + "OFFSET " + Skip + Environment.NewLine + "ROWS FETCH NEXT " + Take + " ROWS ONLY;";
                else query += Environment.NewLine + "LIMIT  " + Skip + Environment.NewLine + "," + Take + ";";

                return query;
            }
        }

        public string QuaryFirst
        {
            get
            {
                if (DataBaseTypes == DataBaseTypes.Mssql)
                {
                    return $"SELECT TOP (1) * from ({Quary.TrimEnd(';') }) AS [RESULT]";
                }
                else
                {
                    if (DataBaseTypes == DataBaseTypes.Sqllight)
                        return Quary.Substring(0, Quary.LastIndexOf("LIMIT")) + "LIMIT 1";
                    else return Quary.Substring(0, Quary.LastIndexOf("OFFSET")) + "LIMIT 1";
                }
            }
        }

        public string Count
        {
            get
            {
                WhereClause.RemoveAll(x => string.IsNullOrEmpty(x));
                var tableName = _obType.TableName();
                var query = "SELECT count(distinct " + DataBaseTypes.GetValidSqlName(tableName) + "." + _primaryId + ") as items FROM " + DataBaseTypes.GetValidSqlName(tableName) + " " + Environment.NewLine +
                       string.Join(Environment.NewLine, JoinClauses.Values.Select(x => x.Item2)) +
                       Environment.NewLine + (WhereClause.Any() ? "WHERE " : string.Empty) + string.Join(" AND ", WhereClause.ToArray());
                query = query.TrimEnd(" AND ").TrimEnd(" OR ");
                return query;
            }
        }

        public string QuaryExist
        {
            get
            {
                var tableName = _obType.TableName();
                return " EXISTS (SELECT 1 FROM " + DataBaseTypes.GetValidSqlName(tableName) + " " + Environment.NewLine +
                       string.Join(Environment.NewLine, JoinClauses.Values.Select(x => x.Item2)) +
                       Environment.NewLine + "WHERE " + string.Join(" AND ", WhereClause.ToArray()) + ")";
            }
        }

        public void Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            validateBinaryExpression(null, null);
            CleanDecoder(string.Empty);
            WhereClause.Add(this.sb.ToString());
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        public override Expression Visit(Expression node)
        {
            var m = base.Visit(node);

            _overridedNodeType = null;
            return m;
        }


        protected object GetSingleValue(Expression ex)
        {
            if (ex.NodeType == ExpressionType.MemberAccess)
            {

                var member = (ex as MemberExpression).Expression as ConstantExpression;
                if (member?.Value.GetType().GetFields(_bindingFlags).Length > 0)
                    return member?.Value.GetType().GetFields(_bindingFlags).First().GetValue(member.Value);
                else if (member?.Value.GetType().GetProperties().Length > 0)
                    return member?.Value.GetType().GetProperties().First().GetValue(member.Value);
                else return null;
            }
            else
            {
                var member = (ex as ConstantExpression);
                return member?.Value;
            }
        }

        private object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        public string GetInvert()
        {
            if (sb.Length <= 4)
                return string.Empty;
            var key = "NOT ";
            var str = sb.ToString().Substring(sb.Length - key.Length);
            if (str == key)
            {
                sb = sb.Remove(sb.Length - key.Length, key.Length);
                return " " + key;
            }
            return string.Empty;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) || (m.Method.Name == "Any"))
            {
                var invert = GetInvert();
                var typecast = m.Arguments.First().Type.GenericTypeArguments.First();
                var cl = new LightDataLinqToNoSql(typecast, _transaction);
                cl._generatedKeys = _generatedKeys;
                cl.DataBaseTypes = DataBaseTypes;
                cl.Translate(m.Arguments.Last() as Expression);
                cl._overridedNodeType = ExpressionType.MemberAccess;
                cl.Visit(m.Arguments[0]);
                if (!string.IsNullOrEmpty(invert))
                    sb.Append(invert);
                sb.Append(cl.QuaryExist);
                cl._overridedNodeType = null;
                _generatedKeys = cl._generatedKeys;
                return m;
            }
            else if (m.Method.Name == "IsNullOrEmpty")
            {

                var invert = sb.ToString().EndsWith("NOT ");
                GetInvert();
                sb.Append("((CASE WHEN ");
                this.Visit(m.Arguments[0]);
                CleanDecoder("");
                sb.Append(" IS NULL THEN 1 ELSE CASE WHEN ");
                this.Visit(m.Arguments[0]);
                CleanDecoder("");
                sb.Append(" = String[] THEN 1 ELSE 0 END END)");
                sb.Append(")");
                sb.Append(boolString.Replace("#", invert ? "0" : "1"));
                return m;
            }
            else if (m.Method.Name == "Contains")
            {
                var ex = (((MemberExpression)m.Object).Expression as ConstantExpression);

                if (ex == null)
                {
                    var invert = GetInvert();
                    var value = GetSingleValue(m.Arguments[0]);
                    sb.Append("(CASE WHEN ");
                    this.Visit(m.Object);
                    InsertBeforeDecoder(" LIKE ");
                    var v = string.Format("String[%{0}%]", value);
                    CleanDecoder(v);
                    sb.Append(" THEN 1 ELSE 0 END) ");
                    sb.Append(boolString.Replace("#", !string.IsNullOrEmpty(invert) ? "0" : "1"));
                }
                else
                {
                    var invert = GetInvert();
                    sb.Append("(CASE WHEN ");
                    this.Visit(m.Arguments[0]);
                    InsertBeforeDecoder(" in (");
                    try
                    {
                        var Stringify = (m.Arguments[0] as MemberExpression).Member as PropertyInfo != null ? ((m.Arguments[0] as MemberExpression).Member as PropertyInfo).GetCustomAttributes<Stringify>() != null : false;
                        var value = (((MemberExpression)m.Object).Member as FieldInfo) != null ? (((MemberExpression)m.Object).Member as FieldInfo)?.GetValue(ex.Value) : (((MemberExpression)m.Object).Member as PropertyInfo)?.GetValue(ex.Value);
                        if (value == null)
                            this.Visit(ex);
                        else
                        {
                            var v = ValuetoSql(value, Stringify);
                            if (string.IsNullOrEmpty(v))
                            {
                                if (Stringify)
                                    v = ValuetoSql(string.Format("DefaultValueForEmptyArray({0})", Guid.NewGuid().ToString()), Stringify);
                                else v = ValuetoSql(-1, Stringify);
                            }

                            CleanDecoder(v);
                        }
                    }
                    catch
                    {
                        this.Visit(ex);
                    }
                    sb.Append(") THEN 1 ELSE 0 END) ");
                    sb.Append(boolString.Replace("#", !string.IsNullOrEmpty(invert) ? "0" : "1"));
                }
                return m;
            }
            else if (m.Method.Name == "StartsWith")
            {
                var ex = (((MemberExpression)m.Object).Expression as ConstantExpression);
                if (ex == null)
                {
                    var invert = GetInvert();
                    var value = GetSingleValue(m.Arguments[0]);
                    sb.Append("(CASE WHEN ");
                    this.Visit(m.Object);
                    InsertBeforeDecoder(" LIKE ");
                    var v = string.Format("String[{0}%]", value);
                    CleanDecoder(v);
                    sb.Append(" THEN 1 ELSE 0 END) ");
                    sb.Append(boolString.Replace("#", !string.IsNullOrEmpty(invert) ? "0" : "1"));

                }
                else
                {

                    var invert = GetInvert();
                    sb.Append("(CASE WHEN ");
                    this.Visit(m.Arguments[0]);
                    InsertBeforeDecoder(" LIKE ");
                    var value = (((MemberExpression)m.Object).Member as FieldInfo) != null ? (((MemberExpression)m.Object).Member as FieldInfo)?.GetValue(ex.Value) : (((MemberExpression)m.Object).Member as PropertyInfo)?.GetValue(ex.Value);
                    var v = string.Format("String[{0}%]", value);
                    CleanDecoder(v);
                    sb.Append(" THEN 1 ELSE 0 END) ");
                    sb.Append(boolString.Replace("#", !string.IsNullOrEmpty(invert) ? "0" : "1"));

                }
                return m;
            }
            else
            if (m.Method.Name == "EndsWith")
            {
                var ex = (((MemberExpression)m.Object).Expression as ConstantExpression);
                if (ex == null)
                {
                    var invert = GetInvert();
                    var value = GetSingleValue(m.Arguments[0]);
                    sb.Append("(CASE WHEN ");
                    this.Visit(m.Object);
                    InsertBeforeDecoder(" LIKE ");
                    var v = string.Format("String[%{0}]", value);
                    CleanDecoder(v);
                    sb.Append(" THEN 1 ELSE 0 END) ");
                    sb.Append(boolString.Replace("#", !string.IsNullOrEmpty(invert) ? "0" : "1"));

                }
                else
                {

                    var invert = GetInvert();
                    sb.Append("(CASE WHEN ");
                    this.Visit(m.Arguments[0]);
                    InsertBeforeDecoder(" LIKE ");
                    var value = (((MemberExpression)m.Object).Member as FieldInfo) != null ? (((MemberExpression)m.Object).Member as FieldInfo)?.GetValue(ex.Value) : (((MemberExpression)m.Object).Member as PropertyInfo)?.GetValue(ex.Value);
                    var v = string.Format("String[%{0}]", value);
                    CleanDecoder(v);
                    sb.Append(" THEN 1 ELSE 0 END) ");
                    sb.Append(boolString.Replace("#", !string.IsNullOrEmpty(invert) ? "0" : "1"));

                }
                return m;
            }
            else if (m.Method.Name == "Take")
            {
                if (this.ParseTakeExpression(m))
                {
                    return null;
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (this.ParseSkipExpression(m))
                {
                    return null;
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (this.ParseOrderByExpression(m, "ASC"))
                {
                    return null;
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (this.ParseOrderByExpression(m, "DESC"))
                {
                    return null;
                }
            }
            else if (m.Method.ReturnType.IsInternalType())
            {
                CleanDecoder(ValuetoSql(Expression.Lambda(m).Compile().DynamicInvoke()));
            }

            throw new EntityException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new EntityException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        private string CleanText(string replaceWith = null)
        {
            MatchCollection matches = null;
            var result = "";
            while ((matches = StringyFyExp.Matches(sb.ToString())).Count > 0)
            {
                var exp = matches[0];

                result = exp.Value.Replace("</Stringify>", "").TrimEnd(']').Substring(@"<Stringify>\[".Length - 1);
                sb = sb.Remove(exp.Index, exp.Value.Length);
                if (replaceWith != null)
                    sb = sb.Insert(exp.Index, replaceWith);

            }
            return result;
        }

        public bool EndWithDecoder()
        {
            return sb.ToString().Trim().EndsWith("</DataEncode>");
        }

        public void InsertBeforeDecoder(string text)
        {
            if (EndWithDecoder())
                sb = sb.InsertBefore(text, "<DataEncode>");
            else sb.Append(text);
        }

        internal void CleanDecoder(string replaceWith)
        {
            if (!EndWithDecoder())
                sb.Append(replaceWith);
            else
            {
                MatchCollection matches = null;
                var result = new string[0];
                while ((matches = DataEncodeExp.Matches(sb.ToString())).Count > 0)
                {
                    var m = matches[0];
                    result = m.Value.Replace("</DataEncode>", "").TrimEnd(']').Substring(@"<DataEncode>\[".Length - 1).Split('|'); // get the key
                    sb = sb.Remove(m.Index, m.Value.Length);
                    if (replaceWith.Contains("String["))
                    {
                        var spt = replaceWith.Split(new string[] { "]," }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x));
                        var isFirst = true;
                        foreach (var str in spt)
                        {
                            var xValue = str.Trim().Replace("String[", "").TrimEnd("]");
                            var rValue = xValue.TrimStart('%').TrimEnd("%");
                            var codedValue = new DataCipher(result.First(), result.Last().ConvertValue<int>().ConvertValue<DataCipherKeySize>()).Encrypt(rValue);
                            if (xValue.StartsWith("%"))
                                codedValue = "%" + codedValue;
                            if (xValue.EndsWith("%"))
                                codedValue += "%";
                            sb.Insert(m.Index, (!isFirst ? "," : "") + "String[" + codedValue + "]");
                            isFirst = false;
                        }
                    }
                    else if (replaceWith.Contains("Date["))
                    {
                        var spt = replaceWith.Split(new string[] { "]," }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x));
                        var isFirst = true;
                        foreach (var str in spt)
                        {
                            var xValue = str.Trim().Replace("Date[", "").TrimEnd("]");
                            var rValue = xValue.TrimStart('%').TrimEnd("%");
                            var codedValue = new DataCipher(result.First(), result.Last().ConvertValue<int>().ConvertValue<DataCipherKeySize>()).Encrypt(rValue);
                            if (xValue.StartsWith("%"))
                                codedValue = "%" + codedValue;
                            if (xValue.EndsWith("%"))
                                codedValue += "%";
                            sb.Insert(m.Index, (!isFirst ? "," : "") + "Date[" + codedValue + "]");
                            isFirst = false;
                        }
                    }
                    else
                        sb = sb.Insert(m.Index, new DataCipher(result.First(), result.Last().ConvertValue<int>().ConvertValue<DataCipherKeySize>()).Encrypt(replaceWith));

                }
            }
        }

        private void validateBinaryExpression(BinaryExpression b, Expression exp)
        {
            if (b != null && exp != null)
            {

                var StringifyText = StringyFyExp.Matches(sb.ToString()).Cast<Match>().FirstOrDefault();
                var isEnum = StringifyText != null;
                if ((exp.NodeType == ExpressionType.MemberAccess || exp.NodeType == ExpressionType.Not)
                    && b.NodeType != ExpressionType.Equal
                    && b.NodeType != ExpressionType.NotEqual && (exp.Type == typeof(bool) || exp.Type == typeof(bool?)))
                {
                    if (exp.NodeType != ExpressionType.Not)
                        sb.Append(" = 1");
                    else sb.Append(" = 0");
                }
            }
            else
            {
                MatchCollection matches = null;
                var result = "";
                while ((matches = BoolExp.Matches(sb.ToString())).Count > 0)
                {
                    var m = matches[0];
                    result = m.Value.Replace("</bool>", "").TrimEnd(']').Substring(@"<bool>\[".Length - 1);
                    var addValue = m.Index + boolString.Length + 3 <= sb.Length ? sb.ToString().Substring(m.Index, boolString.Length + 3) : string.Empty;
                    sb = sb.Remove(m.Index, m.Value.Length);
                    if (!addValue.Contains("="))
                    {
                        sb = sb.Insert(m.Index, " = " + result);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);
            var StringifyText = StringyFyExp.Matches(sb.ToString()).Cast<Match>().FirstOrDefault();
            var isEnum = StringifyText != null;
            validateBinaryExpression(b, b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        if (isEnum)
                        {
                            CleanText();
                            InsertBeforeDecoder(" IS ");
                            InsertBeforeDecoder(StringifyText.ToString());
                        }
                        else
                            CleanDecoder(" IS ");
                    }
                    else
                    {
                        if (isEnum)
                        {
                            CleanText();
                            InsertBeforeDecoder(" = ");
                            InsertBeforeDecoder(StringifyText.ToString());
                        }
                        else
                            InsertBeforeDecoder(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        if (isEnum)
                        {
                            CleanText();
                            InsertBeforeDecoder(" IS NOT ");
                            InsertBeforeDecoder(StringifyText.ToString());
                        }
                        else
                            InsertBeforeDecoder(" IS NOT ");

                    }
                    else
                    {
                        if (isEnum)
                        {
                            CleanText();
                            InsertBeforeDecoder(" <> ");
                            InsertBeforeDecoder(StringifyText.ToString());
                        }
                        else
                            InsertBeforeDecoder(" <> ");

                    }
                    break;

                case ExpressionType.LessThan:
                    InsertBeforeDecoder(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    InsertBeforeDecoder(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    InsertBeforeDecoder(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    InsertBeforeDecoder(" >= ");
                    break;

                default:
                    throw new EntityException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            this.Visit(b.Right);
            validateBinaryExpression(b, b.Right);
            sb.Append(")");
            return b;
        }


        private string ValuetoSql(object value, bool singleValueToString = false)
        {
            CleanText();
            if (value == null)
                return "String[]";
            var type = value.GetType();

            if (type == typeof(bool) || type == typeof(bool?))
                return value.ConvertValue<bool>() ? "1" : "0";

            if (type == typeof(string))
                return string.Format("String[{0}]", value);

            if (type == typeof(Guid) || type == typeof(Guid))
                return string.Format("Guid[{0}]", value);

            if (type == typeof(DateTime) || type == typeof(DateTime?) || type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                if (!singleValueToString)
                    return string.Format("Date[{0}]", value);
                else return string.Format("String[{0}]", value);
            }

            if (value is IEnumerable && !value.GetType().IsInternalType())
            {
                var tValue = "";
                foreach (var v in value as IEnumerable)
                {
                    if (type.IsEnum && !singleValueToString)
                        tValue += ValuetoSql(((int)v).ToString(), singleValueToString) + ",";
                    else
                        tValue += ValuetoSql(v, singleValueToString) + ",";
                }
                return tValue.TrimEnd(',');
            }
            else
            {
                if (!singleValueToString)
                {
                    if (type.IsEnum)
                        return string.Format("{0}", (int)value);

                    if (value.GetType().IsNumeric())
                        return string.Format("{0}", value);
                    else return string.Format("String[{0}]", value);
                }
                else return string.Format("String[{0}]", value);
            }

        }

        protected Expression VisitConstantFixed(ConstantExpression c, string memName = "")
        {
            IQueryable q = c.Value as IQueryable;
            var StringifyText = StringyFyExp.Matches(sb.ToString()).Cast<Match>().FirstOrDefault();
            var isEnum = StringifyText != null;
            string type = null;
            if (isEnum)
                type = CleanText();
            if (q == null && c.Value == null)
            {
                sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        CleanDecoder(string.Format("String[{0}]", c.Value));
                        break;

                    case TypeCode.DateTime:
                        CleanDecoder(string.Format("Date[{0}]", c.Value));
                        break;

                    case TypeCode.Object:

                        object value = null;
                        if (c.Value.GetType().GetFields(_bindingFlags).Length > 0 && (string.IsNullOrEmpty(memName) || c.Value.GetType().GetFields(_bindingFlags).Any(x => x.Name == memName)))
                        {
                            var field = string.IsNullOrEmpty(memName)
                                 ? c.Value.GetType().GetFields(_bindingFlags).FirstOrDefault()
                                 : c.Value.GetType().GetFields(_bindingFlags).FirstOrDefault(x => x.Name == memName);

                            value = field?.GetValue(c.Value) ?? c.Value.GetType().GetFields(_bindingFlags).FirstOrDefault()?.GetValue(c.Value);
                        }
                        else
                        {
                            var field = string.IsNullOrEmpty(memName)
                            ? c.Value.GetType().GetProperties().FirstOrDefault()
                            : c.Value.GetType().GetProperties().FirstOrDefault(x => x.Name == memName);
                            value = field?.GetValue(c.Value) ?? c.Value.GetType().GetProperties().FirstOrDefault()?.GetValue(c.Value);
                        }



                        if (value == null)
                            break;
                        CleanDecoder(ValuetoSql(value, isEnum));
                        break;
                    default:
                        if (isEnum && SavedTypes.ContainsKey(type))
                        {
                            var enumtype = SavedTypes[type];
                            var v = c.Value.ConvertValue(enumtype);
                            CleanDecoder(ValuetoSql(v, isEnum));
                            break;
                        }
                        CleanDecoder(ValuetoSql(c.Value, isEnum));
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            return VisitConstantFixed(c);
        }

        private Dictionary<string, string> _generatedKeys = new Dictionary<string, string>();

        private const string Valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string RandomKey()
        {
            var result = "";
            var length = _generatedKeys.Values.Any() ? _generatedKeys.Last().Value.Length + 3 : 4;
            var rnd = new Random();
            while (0 < length--)
            {
                result += Valid[rnd.Next(Valid.Length)];
            }
            _generatedKeys.Add(result, result);
            return result;
        }



        protected object VisitMember(MemberExpression m, bool columnOnly)
        {
            try
            {
                if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant && (_overridedNodeType == null))
                {
                    VisitConstantFixed(m.Expression as ConstantExpression, m.Member?.Name);
                    return m;
                }
                else if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter || (m.ToString().EndsWith(".HasValue") && m.Expression.NodeType == ExpressionType.MemberAccess)) && (_overridedNodeType == null))
                {
                    var hasValueAttr = m.ToString().EndsWith(".HasValue");
                    _overridedNodeType = null;
                    var cl = hasValueAttr ? (m.Expression as MemberExpression).Expression.Type : m.Expression.Type;
                    var prop = DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.Name == (hasValueAttr ? (m.Expression as MemberExpression).Member.Name : m.Member.Name));
                    var name = prop.GetPropertyName();
                    var table = cl.TableName();
                    var columnName = string.Format("[{0}].[{1}]", table, name).CleanValidSqlName(DataBaseTypes);
                    var dataEncode = prop.GetCustomAttribute<DataEncode>();
                    if (columnOnly)
                        return columnName;

                    bool isNot = sb.ToString().EndsWith("NOT ");
                    if (prop.PropertyType.IsEnum && prop.ContainAttribute<Stringify>())
                    {
                        if (!SavedTypes.ContainsKey(prop.FullName))
                            SavedTypes.TryAdd(prop.FullName, prop.PropertyType);
                        columnName += stringyFy.Replace("#", prop.FullName);
                    }
                    if (isNot)
                    {
                        if (!hasValueAttr)
                            columnName = $"(CASE WHEN {columnName} = 0 THEN 1 ELSE 0 END) {boolString.Replace("#", "0")}";
                        else columnName = $"(CASE WHEN {columnName} IS NULL THEN 1 ELSE 0 END) {boolString.Replace("#", "0")}";
                    }
                    else if (hasValueAttr)
                    {
                        columnName = $"(CASE WHEN {columnName} IS NULL THEN 0 ELSE 1 END) {boolString.Replace("#", "1")}";
                    }
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                        columnName = columnName + boolString.Replace("#", "1");

                    if (dataEncode != null)
                        columnName = columnName + dataEncodeString.Replace("#", dataEncode.Key + "|" + ((int)dataEncode.KeySize).ToString());
                    sb.Append(columnName);
                    return m;
                }
                else if (m.Expression != null && (m.Expression.NodeType == ExpressionType.MemberAccess))
                {
                    _overridedNodeType = null;
                    var key = string.Join("", m.ToString().Split('.').Take(m.ToString().Split('.').Length - 1));
                    var cl = m.Expression.Type;
                    if (cl.IsInterface)
                    {
                        var pr = (m.Expression as MemberExpression).Expression.Type;
                        var tb = m.Expression.ToString().Split('.').Last();
                        cl = DeepCloner.GetProperty(pr, tb)?.PropertyType ?? cl;

                    }


                    var prop = DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.Name == m.Member.Name);
                    var name = prop.GetPropertyName();
                    var table = cl.TableName();
                    var randomTableName = JoinClauses.ContainsKey(key) ? JoinClauses[key].Item1 : RandomKey();
                    var primaryId = DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.ContainAttribute<PrimaryKey>()).GetPropertyName();
                    var columnName = string.Format("[{0}].[{1}]", randomTableName, name).CleanValidSqlName(DataBaseTypes);
                    if (columnOnly)
                        return columnName;
                    sb.Append(columnName);
                    if (JoinClauses.ContainsKey(key))
                        return m;
                    // Ok lets build inner join 
                    var parentType = (m.Expression as MemberExpression).Expression.Type;
                    var parentTable = parentType.TableName();
                    prop = DeepCloner.GetFastDeepClonerProperties(parentType).FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == cl);
                    var v = "";
                    if (prop != null)
                    {
                        v += string.Format("LEFT JOIN [{0}] {1} ON [{2}].[{3}] = [{4}].[{5}]", table, randomTableName, randomTableName, primaryId, parentTable, prop.GetPropertyName()).CleanValidSqlName(DataBaseTypes);
                    }
                    else
                    {
                        prop = DeepCloner.GetFastDeepClonerProperties(cl).FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == parentType);
                        if (prop != null)
                            v += string.Format("LEFT JOIN [{0}] {1} ON [{2}].[{3}] = [{4}].[{5}]", table, randomTableName, randomTableName, prop.GetPropertyName(), parentTable, primaryId).CleanValidSqlName(DataBaseTypes);
                    }

                    if (string.IsNullOrEmpty(v))
                    {
                        sb = sb.Remove(sb.Length - columnName.Length, columnName.Length);
                        CleanDecoder(ValuetoSql(GetValue(m)));
                    }
                    else
                    {
                        JoinClauses.TryAdd(key, new Tuple<string, string>(randomTableName, v));
                    }


                    return m;
                }
                else if (m.Expression != null && _overridedNodeType == ExpressionType.MemberAccess)
                {
                    _overridedNodeType = null;
                    var key = string.Join("", m.ToString().Split('.').Take(m.ToString().Split('.').Length - 1));
                    var cl = m.Expression.Type;
                    if (cl.IsInterface)
                    {
                        var pr = (m.Expression as MemberExpression).Expression.Type.GetActualType();
                        var tb = m.Expression.ToString().Split('.').Last();
                        cl = DeepCloner.GetProperty(pr, tb)?.PropertyType ?? cl;
                    }

                    var prop = DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.Name == m.Member.Name);
                    var table = cl.TableName();
                    var randomTableName = JoinClauses.ContainsKey(key) ? JoinClauses[key].Item1 : RandomKey();
                    var primaryId = DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.ContainAttribute<PrimaryKey>()).GetPropertyName();
                    if (JoinClauses.ContainsKey(key))
                        return m;
                    // Ok lets build inner join 
                    var parentType = (m as MemberExpression).Type.GetActualType();
                    var parentTable = parentType.TableName();
                    prop = DeepCloner.GetFastDeepClonerProperties(parentType).FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == cl);
                    var v = "";
                    if (prop != null)
                    {
                        v += string.Format("INNER JOIN [{0}] {1} ON [{2}].[{3}] = [{4}].[{5}]", parentTable, randomTableName, table, primaryId, randomTableName, prop.GetPropertyName()).CleanValidSqlName(DataBaseTypes);
                    }
                    else
                    {
                        throw new EntityException(string.Format("Expression STRUCTURE IS NOT SUPPORTED MEMBER{0} for EntityWorker", m.Member.Name));
                    }

                    if (!string.IsNullOrEmpty(v))
                        JoinClauses.TryAdd(key, new Tuple<string, string>(randomTableName, v));
                    return m;
                }

            }
            catch
            {
                throw new EntityException(string.Format("Expression '{0}' is not supported", m.ToString()));
            }

            if (m.Type.IsInternalType() && m.Expression.NodeType == ExpressionType.Call)
            {
                CleanDecoder(ValuetoSql(Expression.Lambda(m).Compile().DynamicInvoke()));
                return m;
            }
            throw new EntityException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }



        protected override Expression VisitMember(MemberExpression m)
        {
            return (VisitMember(m, false) as Expression);
        }

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }


        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            var unary = expression.Arguments[1] as UnaryExpression;
            var lambdaExpression = (LambdaExpression)unary?.Operand;

            lambdaExpression = (LambdaExpression)Evaluator.Eval(lambdaExpression ?? (expression.Arguments[1]) as LambdaExpression);

            var body = lambdaExpression.Body as MemberExpression;
            if (body == null) return false;
            var col = VisitMember(body, true)?.ToString();
            var column = col + " as temp" + _columns.Count;
            //if (_columns.All(x => x != column))
            //    _columns.Add(col + " as temp" + _columns.Count);
            if (string.IsNullOrEmpty(OrderBy))
            {
                OrderBy = string.Format("{0} {1}", col, order);
            }
            else
            {
                OrderBy = string.Format("{0}, {1} {2}", OrderBy, col, order);
            }

            return true;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            var sizeExpression = (ConstantExpression)expression.Arguments[1];

            if (!int.TryParse(sizeExpression.Value.ToString(), out var size)) return false;
            Take = size;
            return true;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            var sizeExpression = (ConstantExpression)expression.Arguments[1];

            if (!int.TryParse(sizeExpression.Value.ToString(), out var size)) return false;
            Skip = size;
            return true;
        }
    }
}