using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.SqlQuerys;
using FastDeepCloner;
using Rule = EntityWorker.Core.Attributes.Rule;

namespace EntityWorker.Core
{
    internal class DbSchema
    {
        private static readonly Dictionary<string, ILightDataTable> CachedObjectColumn = new Dictionary<string, ILightDataTable>();

        private static readonly Dictionary<Type, object> CachedIDbRuleTrigger = new Dictionary<Type, object>();

        private static readonly Dictionary<string, string> CachedSql = new Dictionary<string, string>();

        public static object ObjectLocker = new object();

        private readonly IRepository _repository;

        public DbSchema(IRepository repository)
        {
            _repository = repository;
        }

        public ILightDataTable ObjectColumns(Type type)
        {
            var key = type.FullName + _repository.DataBaseTypes.ToString();
            if (CachedObjectColumn.ContainsKey(key))
                return CachedObjectColumn[key];
            var table = type.GetCustomAttribute<Table>()?.Name ?? type.Name;
            var cmd = _repository.GetSqlCommand(_repository.DataBaseTypes == DataBaseTypes.Mssql
                ? "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + table +
                  "'"
                : "SELECT name as COLUMN_NAME, type as DATA_TYPE  FROM pragma_table_info('" + table + "');");
            var data = _repository.GetLightDataTable(cmd, "COLUMN_NAME");
            if (data.Rows.Any())
            {
                if (!CachedObjectColumn.ContainsKey(key))
                    CachedObjectColumn.Add(key, data);
            }
            else return data;
            return CachedObjectColumn[key];

        }

        /// <summary>
        /// Get all by object
        /// PrimaryKey attr must be set ins Where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="query"></param>
        /// <returns></returns>

        public List<T> Select<T>(string query = null)
        {
            if (query == null || !CachedSql.ContainsKey(query))
            {
                var type = typeof(T);
                if (string.IsNullOrEmpty(query))
                    query = Querys.Select(type, _repository.DataBaseTypes).Execute();
                CachedSql.Add(query, query);
            }
            return _repository.DataReaderConverter<T>(_repository.GetSqlCommand(CachedSql[query]));
        }


        /// <summary>
        /// Get all by object
        /// PrimaryKey attr must be set ins Where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="query"></param>
        /// <returns></returns>

        public IList Select(Type type, string query = null)
        {
            if (query == null || !CachedSql.ContainsKey(query))
            {
                if (string.IsNullOrEmpty(query))
                    query = Querys.Select(type, _repository.DataBaseTypes).Execute();
                CachedSql.Add(query, query);
            }
            return _repository.DataReaderConverter(_repository.GetSqlCommand(CachedSql[query]), type);
        }

        /// <summary>
        /// Get object by ID
        /// Primary Key attribute must be set
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetById(object id, Type type)
        {
            var k = type.FullName + _repository.DataBaseTypes.ToString();
            var primaryKey = type.GetActualType().GetPrimaryKey();
            if (!CachedSql.ContainsKey(k))
            {
                var key = primaryKey.GetPropertyName();
                CachedSql.Add(k, Querys.Select(type.GetActualType(), _repository.DataBaseTypes).Where.Column(key).Equal("@ID", true).Execute());
            }
            var cmd = _repository.GetSqlCommand(CachedSql[k]);
            _repository.AddInnerParameter(cmd, "@ID", id.ConvertValue(primaryKey.PropertyType), _repository.GetSqlType(primaryKey.PropertyType));
            return type.GetActualType() != type ? _repository.DataReaderConverter(cmd, type) : _repository.DataReaderConverter(cmd, type).Cast<object>().FirstOrDefault();
        }


        /// <summary>
        /// Get all by object
        /// PrimaryKey attr must be set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="quary"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList GetSqlAll(Type type)
        {
            var k = type.FullName + _repository.DataBaseTypes.ToString();
            if (!CachedSql.ContainsKey(k))
                CachedSql.Add(k, Querys.Select(type, _repository.DataBaseTypes).Execute());
            return _repository.DataReaderConverter(_repository.GetSqlCommand(CachedSql[k]), type);
        }

        /// <summary>
        /// Get all by object
        /// Get object by column, as foreignKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <param name="repository"></param>
        /// <param name="quary"></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetByColumn(object id, string column, Type type)
        {
            var k = type.FullName + column + _repository.DataBaseTypes.ToString();
            if (!CachedSql.ContainsKey(k))
                CachedSql.Add(k, Querys.Select(type, _repository.DataBaseTypes).Where.Column<string>(column).Equal("@ID", true).Execute());

            var cmd = _repository.GetSqlCommand(CachedSql[k]);
            _repository.AddInnerParameter(cmd, "@ID", id, _repository.GetSqlType(id.GetType()));
            return type.GetActualType() != type ? _repository.DataReaderConverter(cmd, type) : _repository.DataReaderConverter(cmd, type).Cast<object>().FirstOrDefault();
        }


        public T LoadChildren<T>(T item, bool onlyFirstLevel = false, List<string> classes = null, List<string> ignoreList = null, Dictionary<string, List<string>> pathLoaded = null, string parentProb = null, string id = null)
        {
            if (pathLoaded == null)
                pathLoaded = new Dictionary<string, List<string>>();
            if (item == null)
                return default(T);
            switch (item)
            {
                case IList _:
                    foreach (var tItem in (IList)item)
                    {
                        var entity = tItem;
                        if (entity == null)
                            continue;
                        LoadChildren(entity, onlyFirstLevel, classes, ignoreList, pathLoaded, parentProb, entity.GetPrimaryKeyValue().ConvertValue<string>());
                    }
                    break;
                default:
                    if ((item) == null)
                        return item;
                    var props = DeepCloner.GetFastDeepClonerProperties(item.GetType());

                    id = item.GetPrimaryKeyValue().ToString();
                    foreach (var prop in props.Where(x => !x.IsInternalType && !x.ContainAttribute<ExcludeFromAbstract>()))
                    {
                        var path = string.Format("{0}.{1}", parentProb ?? "", prop.Name).TrimEnd('.').TrimStart('.');
                        var propCorrectPathName = path?.Split('.').Length >= 2 ? string.Join(".", path.Split('.').Reverse().Take(2).Reverse()) : path;

                        if (classes != null && classes.All(x => x != propCorrectPathName))
                            continue;
                        if (ignoreList != null && ignoreList.Any(x => x == propCorrectPathName))
                            continue;

                        var propValue = prop.GetValue(item);
                        if (propValue != null)
                            continue;
                        if (pathLoaded.ContainsKey(id) && pathLoaded[id].Any(x => x == path))
                            continue;

                        if (!pathLoaded.ContainsKey(id))
                            pathLoaded.Add(id, new List<string>() { path });
                        else if (pathLoaded[id].All(x => x != path)) pathLoaded[id].Add(path);

                        var propertyName = prop.Name;
                        if (path?.Split('.').Length >= 2)
                            propertyName = string.Join(".", path.Split('.').Reverse().Take(3).Reverse()) + "." + parentProb.Split('.').Last() + "." + propertyName;

                        var type = prop.PropertyType.GetActualType();

                        var key = props.FirstOrDefault(x => x.ContainAttribute<ForeignKey>() && x.GetCustomAttribute<ForeignKey>().Type == type && (string.IsNullOrEmpty(x.GetCustomAttribute<ForeignKey>().PropertyName) || x.GetCustomAttribute<ForeignKey>().PropertyName == prop.Name));
                        if (key == null)
                        {
                            var column = DeepCloner.GetFastDeepClonerProperties(type).FirstOrDefault(x => x.GetCustomAttribute<ForeignKey>()?.Type == item.GetType() && string.IsNullOrEmpty(x.GetCustomAttribute<ForeignKey>().PropertyName));
                            var primaryKey = item.GetType().GetPrimaryKey();
                            if (column == null || primaryKey == null)
                                continue;
                            var keyValue = primaryKey.GetValue(item);
                            if (keyValue.ObjectIsNew()) continue;
                            var result = GetByColumn(keyValue, column.Name, prop.PropertyType);
                            prop.SetValue(item, result);
                            if (result != null && !onlyFirstLevel)
                                LoadChildren(result, onlyFirstLevel, classes, ignoreList, pathLoaded, propertyName, id);
                        }
                        else
                        {
                            var isGeneric = prop.PropertyType.GetActualType() != prop.PropertyType;
                            var keyValue = key.GetValue(item);
                            if (keyValue.ObjectIsNew() && !isGeneric) continue;
                            object result = null;
                            if (isGeneric && key.GetCustomAttribute<ForeignKey>().Type == item.GetType()) // trying to load children 
                                result = GetByColumn(item.GetPrimaryKeyValue(), key.GetPropertyName(), prop.PropertyType);
                            else
                                result = GetById(keyValue, prop.PropertyType);

                            prop.SetValue(item, result);
                            if (result != null && !onlyFirstLevel)
                                LoadChildren(result, onlyFirstLevel, classes, ignoreList, pathLoaded, propertyName, id);
                        }
                    }

                    break;
            }

            return item;
        }

        #region Save Methods


        public void DeleteAbstract(object o)
        {
            lock (ObjectLocker)
            {
                DeleteAbstract(o, true);
            }
        }

        private List<string> DeleteAbstract(object o, bool save)
        {
            var type = o.GetType().GetActualType();
            var props = DeepCloner.GetFastDeepClonerProperties(type);
            var table = "[" + (type.GetCustomAttribute<Table>()?.Name ?? type.Name) + "]";
            var primaryKey = o.GetType().GetPrimaryKey();
            var primaryKeyValue = o.GetPrimaryKeyValue();
            if (primaryKeyValue.ObjectIsNew())
                return new List<string>();
            var sql = new List<string>() { "DELETE " + (_repository.DataBaseTypes == DataBaseTypes.Sqllight ? "From " : "") + table + Querys.Where(_repository.DataBaseTypes).Column(primaryKey.GetPropertyName()).Equal(string.Format("Guid[{0}]", primaryKeyValue), true).Execute() };

            foreach (var prop in props.Where(x => !x.IsInternalType && x.GetCustomAttribute<IndependentData>() == null && x.GetCustomAttribute<ExcludeFromAbstract>() == null))
            {
                var value = prop.GetValue(o);

                if (value == null)
                    continue;
                var subSql = new List<string>();
                var propType = prop.PropertyType.GetActualType();
                var insertBefore = props.Any(x => x.GetCustomAttribute<ForeignKey>()?.Type == propType);
                if (DeepCloner.GetFastDeepClonerProperties(propType).All(x => x.GetCustomAttribute<ForeignKey>()?.Type != type))
                    if (!insertBefore)
                        continue;
                if (value is IList)
                    foreach (var item in value as IList)
                    {
                        subSql.AddRange(DeleteAbstract(item, false));
                    }
                else
                    subSql.AddRange(DeleteAbstract(value, false));

                if (insertBefore)
                    sql.InsertRange(sql.Count - 1, subSql);
                else sql.AddRange(subSql);
            }

            if (!save) return sql;
            try
            {
                _repository.CreateTransaction();

                var i = sql.Count - 1;
                var exceptionCount = 0;
                Exception firstChanceExcepion = null;

                while (sql.Count > 0 && exceptionCount <= 10)
                {
                    try
                    {
                        if (i < 0)
                            i = sql.Count - 1;
                        var s = sql[i];
                        var cmd = _repository.GetSqlCommand(s);
                        cmd.Command.ExecuteNonQuery();
                        sql.RemoveAt(i);
                        i--;

                    }
                    catch (Exception e)
                    {
                        firstChanceExcepion = e;
                        exceptionCount++;
                        i--;
                    }


                }

                if (exceptionCount >= 10)
                {
                    throw firstChanceExcepion;
                }
            }
            catch
            {
                _repository.Rollback();
                throw;
            }
            return sql;
        }

        public void Save(object o)
        {
            lock (ObjectLocker)
            {
                Save(o, false);
            }
        }


        private object Save(object o, bool isIndependentData, bool updateOnly = false)
        {
            try
            {
                _repository.CreateTransaction();
                var props = DeepCloner.GetFastDeepClonerProperties(o.GetType());
                var primaryKey = o.GetPrimaryKey();

                if (primaryKey == null)
                    throw new NullReferenceException("Object must have a PrimaryKey");

                var primaryKeyId = !Extension.ObjectIsNew(o.GetPrimaryKeyValue()) ? o.GetPrimaryKeyValue() : null;
                var availableColumns = ObjectColumns(o.GetType());
                var objectRules = o.GetType().GetCustomAttribute<Rule>();
                var tableName = o.GetType().GetCustomAttribute<Table>()?.Name ?? o.GetType().Name;


                object dbTrigger = null;
                if (objectRules != null && !CachedIDbRuleTrigger.ContainsKey(o.GetType()))
                {
                    dbTrigger = objectRules.RuleType.CreateInstance(true);
                    CachedIDbRuleTrigger.Add(o.GetType(), dbTrigger);
                }
                else if (objectRules != null)
                    dbTrigger = CachedIDbRuleTrigger[o.GetType()];

                if (primaryKeyId != null && !updateOnly) // lets attach the object
                {
                    var data = GetById(primaryKeyId, o.GetType());
                    if (data == null)
                    {
                        primaryKeyId = null;
                        o.SetPrimaryKeyValue();
                    }
                    else
                    {
                        if (!_repository.IsAttached(o))
                            _repository.Attach(data);

                        var changes = _repository.GetObjectChanges(o);
                        foreach (var item in props.Where(x => x.CanRead && !changes.Any(a => a.PropertyName == x.Name) && x.IsInternalType))
                            item.SetValue(o, item.GetValue(data));
                    }
                }

                if (!updateOnly)
                    dbTrigger?.GetType().GetMethod("BeforeSave").Invoke(dbTrigger, new List<object>() { _repository, o }.ToArray()); // Check the Rule before save
                object tempPrimaryKey = null;
                var sql = "UPDATE [" + (o.GetType().GetCustomAttribute<Table>()?.Name ?? o.GetType().Name) + "] SET ";
                var cols = props.FindAll(x => availableColumns.FindByPrimaryKey<bool>(x.GetPropertyName()) && x.IsInternalType && !x.ContainAttribute<ExcludeFromAbstract>() && x.GetCustomAttribute<PrimaryKey>() == null);
                if (primaryKeyId == null)
                {

                    if (primaryKey.PropertyType.IsNumeric())
                    {
                        sql = "INSERT INTO [" + tableName + "](" + string.Join(",", cols.Select(x => "[" + x.GetPropertyName() + "]")) + ") Values(";
                        sql += string.Join(",", cols.Select(x => "@" + x.GetPropertyName())) + ");";
                        sql += _repository.DataBaseTypes == DataBaseTypes.Sqllight ? " select last_insert_rowid();" : " SELECT IDENT_CURRENT('" + tableName + "');";

                    }
                    else
                    {
                        var colList = new List<IFastDeepClonerProperty>();
                        tempPrimaryKey = Guid.NewGuid();
                        colList.Insert(0, primaryKey);
                        colList.AddRange(cols);
                        sql = "INSERT INTO [" + tableName + "](" + string.Join(",", colList.Select(x => "[" + x.GetPropertyName() + "]")) + ") Values(";
                        sql += string.Join(",", colList.Select(x => "@" + x.GetPropertyName())) + "); select '" + tempPrimaryKey + "'";
                    }
                }
                else
                {
                    sql += string.Join(",", cols.Select(x => "[" + x.GetPropertyName() + "]" + " = @" + x.GetPropertyName()));
                    sql += Querys.Where(_repository.DataBaseTypes).Column(o.GetType().GetActualType().GetPrimaryKey().GetPropertyName()).Equal(string.Format("Guid[{0}]", primaryKeyId), true).Execute();
                }

                var cmd = _repository.GetSqlCommand(sql);
                if (!primaryKey.PropertyType.IsNumeric() && primaryKeyId == null)
                    _repository.AddInnerParameter(cmd, primaryKey.GetPropertyName(), tempPrimaryKey, _repository.GetSqlType(primaryKey.PropertyType));


                foreach (var col in cols)
                {
                    var v = col.GetValue(o);
                    var defaultOnEmpty = col.GetCustomAttribute<DefaultOnEmpty>();
                    if (col.ContainAttribute<ForeignKey>() && (v?.ObjectIsNew() ?? true))
                    {
                        var ob = props.FirstOrDefault(x => x.PropertyType == col.GetCustomAttribute<ForeignKey>().Type && (string.IsNullOrEmpty(col.GetCustomAttribute<ForeignKey>().PropertyName) || col.GetCustomAttribute<ForeignKey>().PropertyName == x.Name));
                        var obValue = ob?.GetValue(o);
                        var independentData = ob?.GetCustomAttribute<IndependentData>() != null;
                        if (obValue != null)
                        {
                            v = obValue.GetType().GetPrimaryKey().GetValue(obValue)?.ObjectIsNew() ?? true ?
                                Save(obValue, independentData) :
                                obValue.GetType().GetPrimaryKey().GetValue(obValue);
                            col.SetValue(o, v);
                        }
                    }

                    if (col.ContainAttribute<ToBase64String>())
                    {
                        if (!v?.ConvertValue<string>().IsBase64String() ?? false)
                        {
                            v = MethodHelper.EncodeStringToBase64(v.ConvertValue<string>());
                        }
                    }

                    if (col.ContainAttribute<StringFy>() || col.ContainAttribute<DataEncode>())
                        v = v?.ConvertValue<string>();

                    if (col.ContainAttribute<DataEncode>())
                    {
                        if (col.PropertyType != typeof(string))
                            throw new NoNullAllowedException(string.Format("Property {0} Contain DataEncode. PropertyType must be of type String .", col.FullName));
                        v = new DataCipher(col.GetCustomAttribute<DataEncode>().Key, col.GetCustomAttribute<DataEncode>().KeySize).Encrypt(v.ToString());

                    }

                    if (col.ContainAttribute<NotNullable>() && v == null && defaultOnEmpty == null)
                        throw new NoNullAllowedException(string.Format("Property {0} dose not allow null.", col.FullName));


                    if (v == null && defaultOnEmpty != null)
                        v = defaultOnEmpty.Value.ConvertValue(col.PropertyType);

                    _repository.AddInnerParameter(cmd, col.GetPropertyName(), v, (col.ContainAttribute<StringFy>() || col.ContainAttribute<DataEncode>() || col.ContainAttribute<ToBase64String>() ? _repository.GetSqlType(typeof(string)) : _repository.GetSqlType(col.PropertyType)));
                }

                if (primaryKeyId == null)
                    primaryKeyId = _repository.ExecuteScalar(cmd).ConvertValue(primaryKey.PropertyType);
                else _repository.ExecuteNonQuery(cmd);
                var oState = dbTrigger != null ? DeepCloner.Clone(o) : null;
                if (updateOnly)
                    return primaryKeyId;
                dbTrigger?.GetType().GetMethod("AfterSave").Invoke(dbTrigger, new List<object>() { _repository, o, primaryKeyId }.ToArray()); // Check the Rule before save

                foreach (var prop in props.Where(x => !x.IsInternalType && !x.ContainAttribute<ExcludeFromAbstract>()))
                {
                    var independentData = prop.GetCustomAttribute<IndependentData>() != null;
                    var type = prop.PropertyType.GetActualType();
                    var oValue = prop.GetValue(o);
                    if (oValue == null)
                        continue;
                    var vList = oValue is IList ? (IList)oValue : new List<object>() { oValue };

                    foreach (var item in vList)
                    {
                        var foreignKey = DeepCloner.GetFastDeepClonerProperties(item.GetType()).FirstOrDefault(x => x.GetCustomAttribute<ForeignKey>()?.Type == o.GetType() && string.IsNullOrEmpty(x.GetCustomAttribute<ForeignKey>().PropertyName));
                        foreignKey?.SetValue(item, primaryKeyId);
                        var res = Save(item, independentData);
                        foreignKey = props.FirstOrDefault(x => x.GetCustomAttribute<ForeignKey>()?.Type == type && (x.GetCustomAttribute<ForeignKey>().PropertyName == prop.Name || string.IsNullOrEmpty(x.GetCustomAttribute<ForeignKey>().PropertyName)));
                        if (foreignKey == null || !foreignKey.GetValue(o).ObjectIsNew()) continue;
                        if (o.GetType() == foreignKey.GetCustomAttribute<ForeignKey>().Type) continue;
                        foreignKey.SetValue(o, res);
                    }
                }

                if (oState != null && _repository.GetObjectChanges(o, oState).Count > 0) // a change has been made outside the function Save then resave 
                    Save(o, false);

                o.GetType().GetPrimaryKey().SetValue(o, primaryKeyId.ConvertValue(primaryKey.PropertyType));
                _repository.Attach(o, true);
                return primaryKeyId;
            }
            catch
            {
                _repository.Rollback();
                throw;
            }

        }
        #endregion

        #region DataBase Creation Logic

        public void CreateTable(Type tableType, List<Type> createdTables = null, bool commit = true, bool force = false, Dictionary<string, Tuple<string, ForeignKey>> keys = null, Dictionary<Type, string> sqlList = null)
        {
            tableType = tableType.GetActualType();
            if (createdTables == null)
                createdTables = new List<Type>();
            var tableData = ObjectColumns(tableType);
            if (createdTables.Any(x => x == tableType))
                return;
            if (!force && tableData.Rows.Any())
                return;

            _repository.CreateTransaction();
            RemoveTable(tableType);
            createdTables.Add(tableType);
            if (keys == null)
                keys = new Dictionary<string, Tuple<string, ForeignKey>>();

            sqlList = sqlList ?? new Dictionary<Type, string>();
            try
            {
                var props = DeepCloner.GetFastDeepClonerProperties(tableType);
                var tableName = tableType.GetCustomAttribute<Table>()?.Name ?? tableType.Name;
                var sql = new StringBuilder("CREATE TABLE " + (_repository.DataBaseTypes == DataBaseTypes.Mssql ? "[dbo]." : "") + "[" + tableName + "](");
                var isPrimaryKey = "";
                foreach (var prop in props.Where(x => (x.PropertyType.GetDbTypeByType(_repository.DataBaseTypes) != null || !x.IsInternalType) && !x.ContainAttribute<ExcludeFromAbstract>()).GroupBy(x => x.Name).Select(x => x.First()))
                {
                    if (!prop.IsInternalType)
                    {
                        if (!createdTables.Contains(prop.PropertyType.GetActualType()))
                            CreateTable(prop.PropertyType.GetActualType(), createdTables, false, force, keys, sqlList);
                        continue;
                    }
                    isPrimaryKey = prop.ContainAttribute<PrimaryKey>() ? prop.GetPropertyName() : isPrimaryKey;
                    var foreignKey = prop.GetCustomAttribute<ForeignKey>();
                    var dbType = prop.PropertyType.GetDbTypeByType(_repository.DataBaseTypes);
                    var propName = string.Format("[{0}]", prop.GetPropertyName());
                    sql.Append(propName + " ");

                    if (prop.ContainAttribute<StringFy>() || prop.ContainAttribute<DataEncode>() || prop.ContainAttribute<ToBase64String>())
                        dbType = typeof(string).GetDbTypeByType(_repository.DataBaseTypes);

                    if (!prop.ContainAttribute<PrimaryKey>() || _repository.DataBaseTypes == DataBaseTypes.Mssql)
                        sql.Append(dbType + " ");


                    if (foreignKey != null)
                        CreateTable(foreignKey.Type.GetActualType(), createdTables, false, force, keys, sqlList);

                    if (prop.ContainAttribute<PrimaryKey>())
                    {
                        if (prop.PropertyType.IsNumeric())
                            sql.Append(_repository.DataBaseTypes == DataBaseTypes.Mssql ? "IDENTITY(1,1) NOT NULL," : " Integer PRIMARY KEY AUTOINCREMENT,");
                        else sql.Append(_repository.DataBaseTypes == DataBaseTypes.Mssql ? "NOT NULL," : " " + dbType + "  PRIMARY KEY,");
                        continue;
                    }

                    if (foreignKey != null)
                    {
                        var key = propName + "-" + tableName;
                        if (!keys.ContainsKey(key))
                            keys.Add(key, new Tuple<string, ForeignKey>(tableName, foreignKey));
                    }

                    sql.Append((Nullable.GetUnderlyingType(prop.PropertyType) != null || prop.PropertyType == typeof(string)) && !prop.ContainAttribute<NotNullable>() ? " NULL," : " NOT NULL,");
                }

                if (keys.Any() && _repository.DataBaseTypes == DataBaseTypes.Sqllight)
                {
                    while (keys.Any(x => x.Value.Item1 == tableName))
                    {

                        var key = keys.FirstOrDefault(x => x.Value.Item1 == tableName);
                        var type = key.Value.Item2.Type.GetActualType();
                        var keyPrimary = type.GetPrimaryKey().GetPropertyName();
                        var tb = type.GetCustomAttribute<Table>()?.Name ?? type.Name;
                        sql.Append("FOREIGN KEY(" + key.Key.Split('-')[0] + ") REFERENCES " + tb + "(" + keyPrimary + "),");
                        keys.Remove(key.Key);
                    }

                }

                if (!string.IsNullOrEmpty(isPrimaryKey) && _repository.DataBaseTypes == DataBaseTypes.Mssql)
                {
                    sql.Append(" CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED");
                    sql.Append(" ([" + isPrimaryKey + "] ASC");
                    sql.Append(")");
                    sql.Append("WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                    sql.Append(") ON [PRIMARY]");
                }
                else
                {
                    if (_repository.DataBaseTypes == DataBaseTypes.Sqllight)
                        sql = new StringBuilder(sql.ToString().TrimEnd(','));
                    sql.Append(")");
                }

                if (!commit)
                {
                    sqlList.Add(tableType, sql.ToString());
                    return;
                }

                foreach (var prop in props.Where(x => !x.IsInternalType && !x.ContainAttribute<ExcludeFromAbstract>()).GroupBy(x => x.Name).Select(x => x.First()))
                {
                    var type = prop.PropertyType.GetActualType();
                    CreateTable(type, createdTables, false, force, keys, sqlList);
                }


                if (!sqlList.ContainsKey(tableType))
                    sqlList.Add(tableType, sql.ToString());
                sqlList = sqlList.Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key, x => x.Value);
                var c = sqlList.Count;
                while (c > 0)
                {

                    for (var i = sqlList.Keys.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            var key = sqlList.Keys.ToList()[i];
                            if (!ObjectColumns(key).Rows.Any())
                            {
                                var sSql = sqlList[key];
                                var cmd = _repository.GetSqlCommand(sSql);
                                _repository.ExecuteNonQuery(cmd);
                            }
                            c--;
                        }
                        catch (Exception ex)
                        {
                            var test = ex;
                        }
                    }
                }
                sql = new StringBuilder();

                if (keys.Any() && _repository.DataBaseTypes == DataBaseTypes.Mssql)
                {
                    foreach (var key in keys)
                    {
                        var type = key.Value.Item2.Type.GetActualType();
                        var keyPrimary = type.GetPrimaryKey().GetPropertyName();
                        var tb = type.GetCustomAttribute<Table>()?.Name ?? type.Name;
                        sql.Append("ALTER TABLE [" + key.Value.Item1 + "] ADD FOREIGN KEY (" + key.Key.Split('-')[0] + ") REFERENCES [" + tb + "](" + keyPrimary + ");");

                    }
                    var s = sql.ToString();
                    var cmd = _repository.GetSqlCommand(s);
                    _repository.ExecuteNonQuery(cmd);

                }
                CachedObjectColumn.Clear();
                Extension.CachedDataRecord.Clear();
            }
            catch
            {
                _repository.Rollback();
                throw;
            }

        }


        public void RemoveTable(Type tableType, List<Type> tableRemoved = null, bool remove = true)
        {
            _repository.CreateTransaction();
            if (tableRemoved == null)
                tableRemoved = new List<Type>();
            if (tableRemoved.Any(x => x == tableType))
                return;
            tableRemoved.Insert(0, tableType);
            var props = DeepCloner.GetFastDeepClonerProperties(tableType);

            foreach (var prop in props.Where(x => (!x.IsInternalType || x.ContainAttribute<ForeignKey>()) && !x.ContainAttribute<ExcludeFromAbstract>()))
            {
                var key = prop.GetCustomAttribute<ForeignKey>();
                if (key != null && tableRemoved.Any(x => x == key.Type))
                    continue;
                if (key != null)
                    RemoveTable(key.Type, tableRemoved, false);
                else RemoveTable(prop.PropertyType.GetActualType(), tableRemoved, false);

            }

            if (!remove)
                return;

            var tableData = ObjectColumns(tableType);
            if (!tableData.Rows.Any())
                return;
            var c = tableRemoved.Count;
            while (c > 0)
            {
                for (var i = tableRemoved.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var tType = tableRemoved[i];
                        if (ObjectColumns(tType).Rows.Any())
                        {

                            CachedObjectColumn.Remove(tType.FullName + _repository.DataBaseTypes.ToString());
                            if (Extension.CachedDataRecord.ContainsKey(tType))
                                Extension.CachedDataRecord.Remove(tType);
                            var tableName = tType.GetCustomAttribute<Table>()?.Name ?? tType.Name;
                            _repository.ExecuteNonQuery(_repository.GetSqlCommand("DELETE FROM [" + tableName + "];"));
                            var cmd = _repository.GetSqlCommand("DROP TABLE [" + tableName + "];");
                            _repository.ExecuteNonQuery(cmd);
                            CachedObjectColumn.Remove(tType.FullName + _repository.DataBaseTypes.ToString());
                        }
                        c--;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
        }

        #endregion

    }
}
