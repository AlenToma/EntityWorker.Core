using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Object.Library;
using FastDeepCloner;

namespace EntityWorker.Core.SqlQuerys
{
    internal class LightDataLinqToNoSql<T> : ExpressionVisitor
    {
        private StringBuilder sb;
        private ExpressionType? _overridedNodeType;
        private readonly List<string> _columns;

        public Dictionary<string, Tuple<string, string>> JoinClauses { get; private set; } = new Dictionary<string, Tuple<string, string>>();

        public int Skip { get; set; }

        public int Take { get; set; } = Int32.MaxValue;

        public string OrderBy { get; set; }

        public List<string> WhereClause { get; private set; } = new List<string>();

        public DataBaseTypes DataBaseTypes { get; set; }

        public LightDataLinqToNoSql()
        {
            _columns = new List<string>
            {
               "["+ (typeof(T).GetCustomAttribute<Table>()?.Name ?? typeof(T).Name) + "].*"
            };
            OrderBy = typeof(T).GetPrimaryKey()?.GetPropertyName();

        }

        public LightDataLinqToNoSql(DataBaseTypes dataBaseTypes)
        {
            DataBaseTypes = dataBaseTypes;
            _columns = new List<string>
            {
                (typeof(T).GetCustomAttribute<Table>()?.Name ?? typeof(T).Name) + ".*"
            };
            OrderBy = typeof(T).GetPrimaryKey().GetPropertyName();
        }

        public string Quary
        {
            get
            {
                WhereClause.RemoveAll(x => string.IsNullOrEmpty(x));
                var tableName = typeof(T).GetCustomAttribute<Table>()?.Name ?? typeof(T).Name;
                var query = "SELECT distinct " + string.Join(",", _columns) + " FROM [" + tableName + "] " + System.Environment.NewLine +
                       string.Join(System.Environment.NewLine, JoinClauses.Values.Select(x => x.Item2)) +
                       System.Environment.NewLine + (WhereClause.Any() ? "WHERE " : string.Empty) + string.Join(" AND ", WhereClause.ToArray());
                query = query.TrimEnd(" AND ").TrimEnd(" OR ");
                if (!string.IsNullOrEmpty(OrderBy))
                    query += System.Environment.NewLine + "ORDER BY " + OrderBy;
                else query += System.Environment.NewLine + "ORDER BY 1 ASC";

                if (DataBaseTypes == DataBaseTypes.Mssql)
                    query += System.Environment.NewLine + "OFFSET " + Skip + System.Environment.NewLine + "ROWS FETCH NEXT " + Take + " ROWS ONLY;";
                else query += System.Environment.NewLine + "LIMIT  " + Skip + System.Environment.NewLine + "," + Take + ";";

                return query;
            }
        }

        public string QuaryExist
        {
            get
            {
                var tableName = typeof(T).GetCustomAttribute<Table>()?.Name ?? typeof(T).Name;
                return " EXISTS (SELECT 1 FROM [" + tableName + "] " + System.Environment.NewLine +
                       string.Join(System.Environment.NewLine, JoinClauses.Values.Select(x => x.Item2)) +
                       System.Environment.NewLine + "WHERE " + string.Join(" AND ", WhereClause.ToArray()) + ")";
            }
        }

        public void Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            if (sb.ToString().Contains("{IsNullOrEmpty}"))
                sb = new StringBuilder(sb.ToString().Replace("{IsNullOrEmpty}", " = 1 "));
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
                return member?.Value.GetType().GetFields().First().GetValue(member.Value);
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
            var key = "NOT ";
            var str = sb.ToString().Substring(sb.Length - key.Length);
            if (str == key)
            {
                sb = sb.Remove(sb.Length - key.Length, key.Length);
                return key;
            }
            return string.Empty;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) || (m.Method.Name == "Any"))
            {
                var invert = GetInvert();
                var typecast = m.Arguments.First().Type.GenericTypeArguments.First();
                var type = typeof(LightDataLinqToNoSql<>).MakeGenericType(typecast);
                var cl = Activator.CreateInstance(type) as dynamic;
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
                sb.Append("(IIF(");
                this.Visit(m.Arguments[0]);
                sb.Append(" IS NULL ,1,IIF(");
                this.Visit(m.Arguments[0]);
                sb.Append(" = '', 1,0))");
                sb.Append(") {IsNullOrEmpty}");
                return m;
            }
            else if (m.Method.Name == "Contains")
            {
                var ex = (((MemberExpression)m.Object).Expression as ConstantExpression);
                
                if (ex == null)
                {
                    var invert = GetInvert();
                    //(m.Arguments[0] as MemberExpression).Expression as ConstantExpression
                    var value = GetSingleValue(m.Arguments[0]);
                    this.Visit(m.Object);
                    if (!string.IsNullOrEmpty(invert))
                        sb.Append(invert);
                    sb.Append(" like ");
                    var v = string.Format("String[%{0}%]", value);
                    sb.Append(v);
                }
                else
                {
                    var invert = GetInvert();
                    this.Visit(m.Arguments[0]);
                    if (!string.IsNullOrEmpty(invert))
                        sb.Append(invert);
                    sb.Append(" in ");
                    sb.Append("(");
                    this.Visit(ex);
                    sb.Append(")");
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
                    this.Visit(m.Object);
                    if (!string.IsNullOrEmpty(invert))
                        sb.Append(invert);
                    sb.Append(" like ");
                    var v = string.Format("String[{0}%]", value);
                    sb.Append(v);
                }
                else
                {
                    var invert = GetInvert();
                    this.Visit(m.Arguments[0]);
                    if (!string.IsNullOrEmpty(invert))
                        sb.Append(invert);
                    sb.Append(" like ");
                    var v = string.Format("String[{0}%]", ex.Value.GetType().GetFields().First().GetValue(ex.Value));
                    sb.Append(v);
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
                    this.Visit(m.Object);
                    if (!string.IsNullOrEmpty(invert))
                        sb.Append(invert);
                    sb.Append(" like ");
                    var v = string.Format("String[%{0}]", value);
                    sb.Append(v);
                }
                else
                {
                    var invert = GetInvert();
                    this.Visit(m.Arguments[0]);
                    if (!string.IsNullOrEmpty(invert))
                        sb.Append(invert);
                    sb.Append(" like ");
                    var v = string.Format("String[%{0}]", ex.Value.GetType().GetFields().First().GetValue(ex.Value));
                    sb.Append(v);
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
                sb.Append(ValuetoSql(Expression.Lambda(m).Compile().DynamicInvoke()));
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    if (!u.ToString().Contains("IsNullOrEmpty"))
                        sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    if (u.ToString().Contains("IsNullOrEmpty"))
                    {
                        if (sb.ToString().Length >= "{IsNullOrEmpty}".Length &&
                            sb.ToString().Substring(sb.ToString().Length - "{IsNullOrEmpty}".Length) == "{IsNullOrEmpty}"
                            && sb.ToString().Contains("{IsNullOrEmpty}"))
                            sb = new StringBuilder(sb.ToString().Substring(0, sb.ToString().Length - "{IsNullOrEmpty}".Length));
                        sb.Append(" = 0 ");


                    }
                    break;
                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
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
            if (sb.ToString().Length >= "{IsNullOrEmpty}".Length &&
                sb.ToString().Substring(sb.ToString().Length - "{IsNullOrEmpty}".Length) == "{IsNullOrEmpty}" &&
                sb.ToString().Contains("{IsNullOrEmpty}") && !(b.NodeType == ExpressionType.Or ||
                b.NodeType == ExpressionType.OrElse ||
                b.NodeType == ExpressionType.And ||
                b.NodeType == ExpressionType.AndAlso ||
                b.NodeType == ExpressionType.And
                ))
                sb = new StringBuilder(sb.ToString().Substring(0, sb.ToString().Length - "{IsNullOrEmpty}".Length));

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS ");
                    }
                    else
                    {
                        sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS NOT ");
                    }
                    else
                    {
                        sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }


        private string ValuetoSql(object value)
        {
            if (value == null)
                return "";
            var type = value.GetType();
            if (type == typeof(string))
                return string.Format("String[{0}]", value);
            if (type == typeof(DateTime) || type == typeof(DateTime?) || type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                return string.Format("Date[{0}]", value);

            if (value is IEnumerable && !value.GetType().IsInternalType())
            {
                var tValue = "";
                foreach (var v in value as IEnumerable)
                    tValue += ValuetoSql(v) + ",";
                return tValue.TrimEnd(',');
            }
            else
                return string.Format("{0}", value);

            return string.Empty;
        }

        protected Expression VisitConstantFixed(ConstantExpression c, string memName = "")
        {
            IQueryable q = c.Value as IQueryable;

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
                        sb.Append(string.Format("String[{0}]", c.Value));
                        break;

                    case TypeCode.DateTime:
                        sb.Append(string.Format("Date[{0}]", c.Value));
                        break;

                    case TypeCode.Object:

                        var field = string.IsNullOrEmpty(memName)
                            ? c.Value.GetType().GetFields().FirstOrDefault()
                            : c.Value.GetType().GetFields().FirstOrDefault(x => x.Name == memName);

                        object value = null;
                        value = field?.GetValue(c.Value) ?? c.Value.GetType().GetFields().FirstOrDefault()?.GetValue(c.Value);

                        if (value == null)
                            break;


                        sb.Append(ValuetoSql(value));
                        break;
                    default:
                        sb.Append(c.Value);
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



        protected dynamic VisitMember(MemberExpression m, bool columnOnly)
        {
            try
            {
                if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant && (_overridedNodeType == null))
                {

                    VisitConstantFixed(m.Expression as ConstantExpression, m.Member?.Name);
                    return m;
                }
                else if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter && (_overridedNodeType == null))
                {
                    _overridedNodeType = null;
                    var cl = m.Expression.Type;
                    var prop = DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.Name == m.Member.Name);
                    var name = prop.GetPropertyName();
                    var table = cl.GetCustomAttribute<Table>()?.Name ?? cl.Name;
                    var columnName = string.Format("[{0}].[{1}]", table, name);
                    if (columnOnly)
                        return columnName;
                    sb.Append(columnName);
                    return m;
                }
                else if (m.Expression != null && (m.Expression.NodeType == ExpressionType.MemberAccess))
                {
                    _overridedNodeType = null;
                    var key = string.Join("", m.ToString().Split('.').Take(m.ToString().Split('.').Length - 1));
                    var cl = m.Expression.Type;
                    var prop = FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.Name == m.Member.Name);
                    var name = prop.GetPropertyName();
                    var table = cl.GetCustomAttribute<Table>()?.Name ?? cl.Name;
                    var randomTableName = JoinClauses.ContainsKey(key) ? JoinClauses[key].Item1 : RandomKey();
                    var primaryId = FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.ContainAttribute<PrimaryKey>()).GetPropertyName();
                    var columnName = string.Format("[{0}].[{1}]", randomTableName, name);
                    if (columnOnly)
                        return columnName;
                    sb.Append(columnName);
                    if (JoinClauses.ContainsKey(key))
                        return m;
                    // Ok lets build inner join 
                    var parentType = (m.Expression as MemberExpression).Expression.Type;
                    var parentTable = parentType.GetCustomAttribute<Table>()?.Name ?? parentType.Name;
                    prop = DeepCloner.GetFastDeepClonerProperties(parentType).FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == cl);
                    var v = "";
                    if (prop != null)
                    {
                        v += string.Format("LEFT JOIN [{0}] {1} ON [{2}].[{3}] = [{4}].[{5}]", table, randomTableName, randomTableName, primaryId, parentTable, prop.GetPropertyName());
                    }
                    else
                    {
                        prop = DeepCloner.GetFastDeepClonerProperties(cl).FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == parentType);
                        if (prop != null)
                            v += string.Format("LEFT JOIN [{0}] {1} ON [{2}].[{3}] = [{4}].[{5}]", table, randomTableName, randomTableName, prop.GetPropertyName(), parentTable, primaryId);
                    }

                    if (string.IsNullOrEmpty(v))
                    {
                        sb = sb.Remove(sb.Length - columnName.Length, columnName.Length);
                        sb.Append(ValuetoSql(GetValue(m)));
                    }
                    else
                    {
                        JoinClauses.Add(key, new Tuple<string, string>(randomTableName, v));
                    }


                    return m;
                }
                else if (m.Expression != null && _overridedNodeType == ExpressionType.MemberAccess)
                {
                    _overridedNodeType = null;
                    var key = string.Join("", m.ToString().Split('.').Take(m.ToString().Split('.').Length - 1));
                    var cl = m.Expression.Type;
                    var prop = FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.Name == m.Member.Name);
                    var table = cl.GetCustomAttribute<Table>()?.Name ?? cl.Name;
                    var randomTableName = JoinClauses.ContainsKey(key) ? JoinClauses[key].Item1 : RandomKey();
                    var primaryId = FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(cl).First(x => x.ContainAttribute<PrimaryKey>()).GetPropertyName();
                    if (JoinClauses.ContainsKey(key))
                        return m;
                    // Ok lets build inner join 
                    var parentType = (m as MemberExpression).Type.GetActualType();
                    var parentTable = parentType.GetCustomAttribute<Table>()?.Name ?? parentType.Name;
                    prop = DeepCloner.GetFastDeepClonerProperties(parentType).FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == cl);
                    var v = "";
                    if (prop != null)
                    {
                        v += string.Format("INNER JOIN [{0}] {1} ON [{2}].[{3}] = [{4}].[{5}]", parentTable, randomTableName, table, primaryId, randomTableName, prop.GetPropertyName());
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("CLASS STRUCTURE IS NOT SUPPORTED MEMBER{0}", m.Member.Name));
                    }

                    if (!string.IsNullOrEmpty(v))
                        JoinClauses.Add(key, new Tuple<string, string>(randomTableName, v));
                    return m;
                }

            }
            catch
            {
                throw new NotSupportedException(string.Format("Expression '{0}' is not supported", m.ToString()));
            }

            if (m.Type.IsInternalType() && m.Expression.NodeType == ExpressionType.Call)
            {
                sb.Append(ValuetoSql(Expression.Lambda(m).Compile().DynamicInvoke()));
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }



        protected override Expression VisitMember(MemberExpression m)
        {
            return VisitMember(m, false);
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
            var col = VisitMember(body, true);
            var column = col + " as temp" + _columns.Count;
            if (_columns.All(x => x != column))
                _columns.Add(col + " as temp" + _columns.Count);
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