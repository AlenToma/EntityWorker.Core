using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.SqlQuerys;
using FastDeepCloner;
using Rule = EntityWorker.Core.Attributes.Rule;
using EntityWorker.Core.Object.Library.DataBase;

namespace EntityWorker.Core
{
    internal class DbSchema
    {
        internal static readonly SafeValueType<Type, object> CachedIDbRuleTrigger = new SafeValueType<Type, object>();

        internal static readonly SafeValueType<string, string> CachedSql = new SafeValueType<string, string>();

        private readonly Transaction.Transaction _repository;

        private static ILightDataTable NotValidkeywords;

        public DbSchema(Transaction.Transaction repository)
        {
            _repository = repository;
        }

        public bool IsValidName(string column)
        {
            if (_repository.DataBaseTypes == DataBaseTypes.PostgreSql)
            {
                if (NotValidkeywords == null)
                    NotValidkeywords = _repository.GetLightDataTable(_repository.GetSqlCommand("select * from pg_get_keywords() where catdesc = String[reserved]"), "word");

                return !NotValidkeywords.FindByPrimaryKey<bool>(column.ToLower());
            }

            return true;
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
                CachedSql.GetOrAdd(query, query);
            }
            return _repository.DataReaderConverter<T>(_repository.GetSqlCommand(CachedSql[query])).Execute();
        }

        /// <summary>
        /// Get all by object
        /// PrimaryKey attr must be set ins Where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="repository"></param>
        /// <param name="query"></param>
        /// <returns></returns>

        public IList Select(Type type, string query = null)
        {
            if (query == null || !CachedSql.ContainsKey(query))
            {
                if (string.IsNullOrEmpty(query))
                    query = Querys.Select(type, _repository.DataBaseTypes).Execute();
                CachedSql.GetOrAdd(query, query);
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
                CachedSql.GetOrAdd(k, Querys.Select(type.GetActualType(), _repository.DataBaseTypes).Where.Column(key).Equal("@ID", true).Execute());
            }
            var cmd = _repository.GetSqlCommand(CachedSql[k]);
            _repository.AddInnerParameter(cmd, "@ID", id.ConvertValue(primaryKey.PropertyType));
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
                CachedSql.GetOrAdd(k, Querys.Select(type, _repository.DataBaseTypes).Execute());
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
                CachedSql.GetOrAdd(k, Querys.Select(type, _repository.DataBaseTypes).Where.Column(column).Equal("@ID", true).Execute());

            var cmd = _repository.GetSqlCommand(CachedSql[k]);
            _repository.AddInnerParameter(cmd, "@ID", id);
            return type.GetActualType() != type ? _repository.DataReaderConverter(cmd, type) : _repository.DataReaderConverter(cmd, type).Cast<object>().FirstOrDefault();
        }

        public T LoadChildren<T>(T item, bool onlyFirstLevel = false, List<string> classes = null, List<string> ignoreList = null, Dictionary<string, List<string>> pathLoaded = null, string parentProb = null, string id = null)
        {

            if (pathLoaded == null)
                pathLoaded = new Dictionary<string, List<string>>();
            if (item == null)
                return default(T);
            GlobalConfiguration.Log?.Info("Loading Children for " + item.GetType() + "", item);
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
                    foreach (var prop in props.Where(x => x.CanRead && !x.IsInternalType && !x.ContainAttribute<ExcludeFromAbstract>() && !x.ContainAttribute<JsonDocument>() && !x.ContainAttribute<XmlDocument>()))
                    {
                        var path = string.Format("{0}.{1}", parentProb ?? "", prop.Name).TrimEnd('.').TrimStart('.');
                        var propCorrectPathName = path?.Split('.').Length >= 2 ? string.Join(".", path.Split('.').Reverse().Take(2).Reverse()) : path;

                        if (classes != null && classes.All(x => x != propCorrectPathName))
                            continue;
                        if (ignoreList != null && ignoreList.Any(x => x == propCorrectPathName))
                            continue;

                        var propValue = prop.GetValue(item);
                        if (propValue != null)
                            if (!(propValue is IList) || (propValue as IList).Any())
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
            DeleteAbstract(o, true);
        }

        private List<string> DeleteAbstract(object o, bool save)
        {


            object dbTrigger = null;
            GlobalConfiguration.Log?.Info("Delete", o);
            var type = o.GetType().GetActualType();
            var props = DeepCloner.GetFastDeepClonerProperties(type);
            var table = type.TableName().GetName(_repository.DataBaseTypes);
            var primaryKey = o.GetType().GetPrimaryKey();
            var primaryKeyValue = o.GetPrimaryKeyValue();
            var objectRules = type.GetCustomAttribute<Rule>();
            if (primaryKeyValue.ObjectIsNew())
                return new List<string>();
            var sql = new List<string>() {

                "DELETE " + (_repository.DataBaseTypes == DataBaseTypes.Sqllight || _repository.DataBaseTypes == DataBaseTypes.PostgreSql ? "From " : "") +
                table +
                Querys.Where(_repository.DataBaseTypes).Column(primaryKey.GetPropertyName()).Equal(primaryKeyValue).Execute()
            };

            if (objectRules != null && !CachedIDbRuleTrigger.ContainsKey(type))
            {
                dbTrigger = objectRules.RuleType.CreateInstance();
                CachedIDbRuleTrigger.Add(o.GetType(), dbTrigger);
            }
            else if (objectRules != null || CachedIDbRuleTrigger.ContainsKey(type))
                dbTrigger = CachedIDbRuleTrigger[type];

            try
            {
                _repository.CreateTransaction();
                if (objectRules != null)
                    dbTrigger?.GetType().GetMethod("Delete").Invoke(dbTrigger, new List<object>() { _repository, o }.ToArray()); // Check the Rule before save
            }
            catch
            {
                _repository.Rollback();
                throw;
            }




            foreach (var prop in props.Where(x => x.CanRead && !x.IsInternalType && x.GetCustomAttribute<IndependentData>() == null && !x.ContainAttribute<JsonDocument>() && !x.ContainAttribute<XmlDocument>() && x.GetCustomAttribute<ExcludeFromAbstract>() == null))
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

        public void Save(object o, List<string> ignoredProperties = null)
        {
            Save(o, false, false, ignoredProperties);
        }

        private object Save(object o, bool isIndependentData, bool updateOnly = false, List<string> ignoredProperties = null, string lastProperty = null)
        {
            try
            {
                if (ignoredProperties == null)
                    ignoredProperties = new List<string>();
                if (lastProperty == null)
                    lastProperty = string.Empty; // not valid name

                GlobalConfiguration.Log?.Info("Save", o);
                _repository.CreateTransaction();
                var props = DeepCloner.GetFastDeepClonerProperties(o.GetType());
                var primaryKey = o.GetPrimaryKey();

                if (primaryKey == null)
                    throw new EntityException("Object must have a PrimaryKey");

                var primaryKeyId = !Extension.ObjectIsNew(o.GetPrimaryKeyValue()) ? o.GetPrimaryKeyValue() : null;
                var availableColumns = _repository.GetColumnSchema(o.GetType());
                var objectRules = o.GetType().GetCustomAttribute<Rule>();
                var tableName = o.GetType().TableName().GetName(_repository.DataBaseTypes);
                var primaryKeySubstitut = !primaryKey.GetCustomAttribute<PrimaryKey>().AutoGenerate ? primaryKeyId : null;

                object dbTrigger = null;
                if (objectRules != null && !CachedIDbRuleTrigger.ContainsKey(o.GetType()))
                {
                    dbTrigger = objectRules.RuleType.CreateInstance();
                    CachedIDbRuleTrigger.Add(o.GetType(), dbTrigger);
                }
                else if (objectRules != null || CachedIDbRuleTrigger.ContainsKey(o.GetType()))
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
                        foreach (var item in props.Where(x => x.CanRead && !changes.Any(a => a.PropertyName == x.Name) && (x.IsInternalType || x.ContainAttribute<JsonDocument>())))
                            item.SetValue(o, item.GetValue(data));
                    }
                }

                if (!updateOnly)
                    dbTrigger?.GetType().GetMethod("BeforeSave").Invoke(dbTrigger, new List<object>() { _repository, o }.ToArray()); // Check the Rule before save
                object tempPrimaryKey = null;
                var sql = "UPDATE " + (o.GetType().TableName().GetName(_repository.DataBaseTypes)) + " SET ";
                var cols = props.FindAll(x => x.CanRead && (availableColumns.ContainsKey(x.GetPropertyName()) || availableColumns.ContainsKey(x.GetPropertyName().ToLower())) && (x.IsInternalType || x.ContainAttribute<JsonDocument>() || x.ContainAttribute<XmlDocument>()) && !x.ContainAttribute<ExcludeFromAbstract>() && x.GetCustomAttribute<PrimaryKey>() == null);

                // Clean out all unwanted properties
                if (ignoredProperties.Any())
                    cols = cols.FindAll(x => !ignoredProperties.Any(a => a == x.Name || a == (o.GetType().Name + "." + x.Name) || a == (lastProperty + "." + x.Name)));

                if (primaryKeyId == null)
                {
                    if (primaryKey.PropertyType.IsNumeric() && primaryKey.GetCustomAttribute<PrimaryKey>().AutoGenerate)
                    {
                        sql = "INSERT INTO " + tableName + "(" + string.Join(",", cols.Select(x => "[" + x.GetPropertyName() + "]")) + ") Values(";
                        sql += string.Join(",", cols.Select(x => "@" + x.GetPropertyName())) + ");";
                        sql += _repository.DataBaseTypes == DataBaseTypes.Sqllight ? " select last_insert_rowid();" : (_repository.DataBaseTypes != DataBaseTypes.PostgreSql ? " SELECT IDENT_CURRENT('" + tableName + "');" : " SELECT currval('" + string.Format("{0}_{1}_seq", tableName, primaryKey.GetPropertyName()) + "');");
                    }
                    else
                    {
                        var colList = new List<IFastDeepClonerProperty>();
                        tempPrimaryKey = primaryKeySubstitut == null ? Guid.NewGuid() : primaryKeySubstitut;
                        if (primaryKeySubstitut == null && primaryKey.PropertyType.IsNumeric())
                            tempPrimaryKey = _repository.ExecuteScalar(_repository.GetSqlCommand(String.Format("SELECT MAX([{0}]) FROM {1}", primaryKey.GetPropertyName(), tableName))).ConvertValue<long>() + 1;
                        else if (primaryKey.PropertyType == typeof(string))
                            tempPrimaryKey = tempPrimaryKey.ToString();
                        colList.Insert(0, primaryKey);
                        colList.AddRange(cols);
                        sql = "INSERT INTO " + tableName + "(" + string.Join(",", colList.Select(x => "[" + x.GetPropertyName() + "]")) + ") Values(";
                        sql += string.Join(",", colList.Select(x => "@" + x.GetPropertyName())) + "); select '" + tempPrimaryKey + "'";
                    }
                }
                else
                {
                    sql += string.Join(",", cols.Select(x => "[" + x.GetPropertyName() + "]" + " = @" + x.GetPropertyName()));
                    sql += Querys.Where(_repository.DataBaseTypes).Column(o.GetType().GetActualType().GetPrimaryKey().GetPropertyName()).Equal(primaryKeyId).Execute();

                }

                var cmd = _repository.GetSqlCommand(sql);
                if ((!primaryKey.PropertyType.IsNumeric() || !primaryKey.GetCustomAttribute<PrimaryKey>().AutoGenerate) && primaryKeyId == null)
                    _repository.AddInnerParameter(cmd, primaryKey.GetPropertyName(), tempPrimaryKey);


                foreach (var col in cols)
                {
                    var v = col.GetValue(o);
                    var defaultOnEmpty = col.GetCustomAttribute<DefaultOnEmpty>();
                    if (col.ContainAttribute<ForeignKey>() && (v?.ObjectIsNew() ?? true))
                    {
                        var ob = props.FirstOrDefault(x => (x.PropertyType == col.GetCustomAttribute<ForeignKey>().Type) && (string.IsNullOrEmpty(col.GetCustomAttribute<ForeignKey>().PropertyName) || col.GetCustomAttribute<ForeignKey>().PropertyName == x.Name));
                        var obValue = ob?.GetValue(o);
                        var independentData = ob?.GetCustomAttribute<IndependentData>() != null;
                        if (obValue != null)
                        {
                            v = obValue.GetType().GetPrimaryKey().GetValue(obValue)?.ObjectIsNew() ?? true ?
                                Save(obValue, independentData, false, ignoredProperties, ob.Name) :
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

                    if (col.ContainAttribute<JsonDocument>())
                    {
                        v = v?.ToJson();
                    }

                    if (col.ContainAttribute<XmlDocument>())
                    {
                        v = v?.ToXml();
                    }

                    if (col.ContainAttribute<Stringify>() || col.ContainAttribute<DataEncode>())
                        v = v?.ConvertValue<string>();

                    if (col.ContainAttribute<DataEncode>())
                    {
                        if (col.PropertyType != typeof(string))
                            throw new EntityException(string.Format("Property {0} Contain DataEncode. PropertyType must be of type String .", col.FullName));
                        v = new DataCipher(col.GetCustomAttribute<DataEncode>().Key, col.GetCustomAttribute<DataEncode>().KeySize).Encrypt(v.ToString());

                    }

                    if (col.ContainAttribute<NotNullable>() && v == null && defaultOnEmpty == null)
                        throw new EntityException(string.Format("Property {0} dose not allow null.", col.FullName));


                    if (v == null && defaultOnEmpty != null)
                        v = defaultOnEmpty.Value.ConvertValue(col.PropertyType);

                    _repository.AddInnerParameter(cmd, col.GetPropertyName(), v);
                }

                if (primaryKeyId == null)
                    primaryKeyId = _repository.ExecuteScalar(cmd).ConvertValue(primaryKey.PropertyType);
                else _repository.ExecuteNonQuery(cmd);
                var oState = dbTrigger != null ? DeepCloner.Clone(o) : null;
                if (updateOnly)
                    return primaryKeyId;
                dbTrigger?.GetType().GetMethod("AfterSave").Invoke(dbTrigger, new List<object>() { _repository, o, primaryKeyId }.ToArray()); // Check the Rule before save

                foreach (var prop in props.Where(x => !x.IsInternalType && !x.ContainAttribute<JsonDocument>() && !x.ContainAttribute<XmlDocument>() && !x.ContainAttribute<ExcludeFromAbstract>()))
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
                        var res = Save(item, independentData, false, ignoredProperties, prop.Name);
                        foreignKey = props.FirstOrDefault(x => x.GetCustomAttribute<ForeignKey>()?.Type == type && (x.GetCustomAttribute<ForeignKey>().PropertyName == prop.Name || string.IsNullOrEmpty(x.GetCustomAttribute<ForeignKey>().PropertyName)));
                        if (foreignKey == null || !foreignKey.GetValue(o).ObjectIsNew()) continue;
                        if (o.GetType() == foreignKey.GetCustomAttribute<ForeignKey>().Type) continue;
                        foreignKey.SetValue(o, res);
                    }
                }


                if (oState != null && _repository.GetObjectChanges(o, oState).Count > 0) // a change has been made outside the function Save then resave          
                {
                    o.SetPrimaryKeyValue(primaryKeyId);
                    Save(o, false, true, ignoredProperties);
                }
                o.SetPrimaryKeyValue(primaryKeyId);
                _repository.Attach(o, true);
                return primaryKeyId;
            }
            catch (Exception e)
            {
                GlobalConfiguration.Log?.Error(e);
                _repository.Rollback();
                throw;
            }

        }
        #endregion
        #region DataBase Creation Logic
        private List<Type> _alreadyControlled = new List<Type>();
        private long counter;
        public CodeToDataBaseMergeCollection GetDatabase_Diff(Type tableType, CodeToDataBaseMergeCollection str = null, List<Type> createdTables = null)
        {

            str = str ?? new CodeToDataBaseMergeCollection(_repository);
            if (tableType.GetCustomAttribute<ExcludeFromAbstract>() != null || _alreadyControlled.Any(x => x == tableType))
                return str;
            _repository.CreateSchema(tableType);
            tableType = tableType.GetActualType();
            _alreadyControlled.Add(tableType);
            createdTables = createdTables ?? new List<Type>();
            if (createdTables.Any(x => x == tableType) || tableType.GetPrimaryKey() == null)
                return str;

            if (CodeToDataBaseMergeCollection.ExecutedData.ContainsKey(tableType.FullName + _repository.DataBaseTypes.ToString()))
                return str;

            createdTables.Add(tableType);
            var table = _repository.GetColumnSchema(tableType);
            var tableName = tableType.TableName();
            var props = DeepCloner.GetFastDeepClonerProperties(tableType).Where(x => x.CanRead && !x.ContainAttribute<ExcludeFromAbstract>());
            var codeToDataBaseMerge = new CodeToDataBaseMerge() { Object_Type = tableType };
            var isPrimaryKey = "";
            if (!IsValidName(tableName.Name))
                throw new EntityException(tableName.Name + " is not a valid Name for the current provider " + _repository.DataBaseTypes);


            if (!table.Values.Any())
            {
                codeToDataBaseMerge.Sql = new StringBuilder($"CREATE TABLE {tableName.GetName(_repository.DataBaseTypes)} (");
                foreach (var prop in props.Where(x => (x.GetDbTypeByType(_repository.DataBaseTypes) != null || !x.IsInternalType ||
                x.ContainAttribute<JsonDocument>() || x.ContainAttribute<XmlDocument>()) &&
                !x.ContainAttribute<ExcludeFromAbstract>()).GroupBy(x => x.Name).Select(x => x.First())
                        .OrderBy(x => x.ContainAttribute<PrimaryKey>() ? null : x.Name))
                {
                    if (!prop.IsInternalType && !prop.ContainAttribute<JsonDocument>() && !prop.ContainAttribute<XmlDocument>())
                    {
                        if (!str.Any(x => x.Object_Type == prop.PropertyType.GetActualType()) && createdTables.All(x => x != prop.PropertyType.GetActualType()))
                            GetDatabase_Diff(prop.PropertyType, str, createdTables);
                        continue;
                    }

                    isPrimaryKey = prop.ContainAttribute<PrimaryKey>() ? prop.GetPropertyName() : isPrimaryKey;
                    var foreignKey = prop.GetCustomAttribute<ForeignKey>();
                    var dbType = prop.GetDbTypeByType(_repository.DataBaseTypes);
                    var propName = string.Format("[{0}]", prop.GetPropertyName());
                    codeToDataBaseMerge.Sql.Append(propName + " ");
                    if (!IsValidName(prop.GetPropertyName()))
                        throw new Exception(prop.GetPropertyName() + " is not a valid Name for the current provider " + _repository.DataBaseTypes);



                    if (!prop.ContainAttribute<PrimaryKey>() || _repository.DataBaseTypes == DataBaseTypes.Mssql)
                        codeToDataBaseMerge.Sql.Append(dbType + " ");


                    if (foreignKey != null && createdTables.All(x => x != foreignKey.Type))
                        GetDatabase_Diff(foreignKey.Type, str, createdTables);

                    if (prop.ContainAttribute<PrimaryKey>())
                    {
                        if (prop.PropertyType.IsNumeric() && prop.GetCustomAttribute<PrimaryKey>().AutoGenerate)
                            codeToDataBaseMerge.Sql.Append(_repository.DataBaseTypes == DataBaseTypes.Mssql ? "IDENTITY(1,1) NOT NULL," : (_repository.DataBaseTypes == DataBaseTypes.Sqllight ? " Integer PRIMARY KEY AUTOINCREMENT," : " BIGSERIAL PRIMARY KEY,"));
                        else codeToDataBaseMerge.Sql.Append(_repository.DataBaseTypes == DataBaseTypes.Mssql ? "NOT NULL," : " " + dbType + "  PRIMARY KEY,");
                        continue;
                    }

                    if (foreignKey != null)
                    {
                        var key = propName + "-" + tableName.GetName(_repository.DataBaseTypes);
                        if (!str.Keys.ContainsKey(key))
                            str.Keys.Add(key, new Tuple<string, ForeignKey>(tableName.GetName(_repository.DataBaseTypes), foreignKey));
                    }

                    codeToDataBaseMerge.Sql.Append((Nullable.GetUnderlyingType(prop.PropertyType) != null || prop.PropertyType == typeof(string)) && !prop.ContainAttribute<NotNullable>() ? " NULL," : " NOT NULL,");
                }

                if (str.Keys.Any() && _repository.DataBaseTypes == DataBaseTypes.Sqllight)
                {
                    while (str.Keys.Any(x => x.Value.Item1 == tableName.Name))
                    {

                        var key = str.Keys.FirstOrDefault(x => x.Value.Item1 == tableName.Name);
                        var type = key.Value.Item2.Type.GetActualType();
                        var keyPrimary = type.GetPrimaryKey().GetPropertyName();
                        var tb = type.TableName();
                        codeToDataBaseMerge.Sql.Append("FOREIGN KEY(" + key.Key.Split('-')[0] + ") REFERENCES " + tb.GetName(_repository.DataBaseTypes) + "(" + keyPrimary + "),");
                        str.Keys.Remove(key.Key);
                    }

                }

                if (!string.IsNullOrEmpty(isPrimaryKey) && _repository.DataBaseTypes == DataBaseTypes.Mssql)
                {
                    codeToDataBaseMerge.Sql.Append(" CONSTRAINT [PK_" + tableName.Name + "] PRIMARY KEY CLUSTERED");
                    codeToDataBaseMerge.Sql.Append(" ([" + isPrimaryKey + "] ASC");
                    codeToDataBaseMerge.Sql.Append(")");
                    codeToDataBaseMerge.Sql.Append("WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                    codeToDataBaseMerge.Sql.Append(") ON [PRIMARY]");
                }
                else
                {
                    if (_repository.DataBaseTypes == DataBaseTypes.Sqllight)
                        codeToDataBaseMerge.Sql = new StringBuilder(codeToDataBaseMerge.Sql.ToString().TrimEnd(','));
                    codeToDataBaseMerge.Sql.Append(")");
                }

                str.Add(codeToDataBaseMerge);
            }
            else
            {
                foreach (var prop in props.Where(x => (x.GetDbTypeByType(_repository.DataBaseTypes) != null || !x.IsInternalType) && !x.ContainAttribute<ExcludeFromAbstract>()).GroupBy(x => x.Name).Select(x => x.First())
                .OrderBy(x => x.ContainAttribute<PrimaryKey>() ? null : x.Name))
                {

                    if (prop.ContainAttribute<ForeignKey>())
                        GetDatabase_Diff(prop.GetCustomAttribute<ForeignKey>().Type, str, createdTables);
                    var propType = prop.PropertyType;
                    if (prop.ContainAttribute<Stringify>() ||
                        prop.ContainAttribute<DataEncode>() ||
                        prop.ContainAttribute<ToBase64String>() ||
                        prop.ContainAttribute<JsonDocument>() ||
                        prop.ContainAttribute<XmlDocument>())
                        propType = typeof(string);

                    var modify = prop.IsInternalType || prop.ContainAttribute<JsonDocument>() || prop.ContainAttribute<XmlDocument>() ? (_repository.DataBaseTypes == DataBaseTypes.PostgreSql ? table.Get(prop.GetPropertyName().ToLower()) : table.Get(prop.GetPropertyName())) : null;
                    if (modify != null)
                    {

                        if (_repository.DataBaseTypes != DataBaseTypes.Sqllight && !(prop.GetDbTypeListByType(_repository.DataBaseTypes).Any(x => x.ToLower().Contains(modify.DataType.ToLower()))) && _repository.DataBaseTypes != DataBaseTypes.PostgreSql)
                        {
                            var constraine = Properties.Resources.DropContraine
                                .Replace("@tb", $"'{tableName.Name}'").Replace("@col", $"'{prop.GetPropertyName()}'")
                                .Replace("@schema", $"'{tableName.Schema ?? ""}'")
                                .Replace("@TableName", "@" + counter++)
                                .Replace("@ColumnName", "@" + counter++)
                                .Replace("@fullName", "@" + counter++)
                                .Replace("@DROP_COMMAND", "@" + counter++)
                                .Replace("@FOREIGN_KEY_NAME", "@" + counter++);
                            codeToDataBaseMerge.Sql.Append(constraine);
                            codeToDataBaseMerge.Sql.Append($"\nALTER TABLE {tableName.GetName(_repository.DataBaseTypes)} ALTER COLUMN [{prop.GetPropertyName()}] {prop.GetDbTypeByType(_repository.DataBaseTypes)} {((Nullable.GetUnderlyingType(propType) != null || propType == typeof(string)) && !prop.ContainAttribute<NotNullable>() ? " NULL" : " NOT NULL")}");
                        }
                        else
                        {
                            if (!(prop.GetDbTypeListByType(_repository.DataBaseTypes).Any(x => x.ToLower().Contains(modify.DataType.ToLower()))) && _repository.DataBaseTypes == DataBaseTypes.PostgreSql)
                                codeToDataBaseMerge.Sql.Append($"\nALTER TABLE {tableName.GetName(_repository.DataBaseTypes)} ALTER COLUMN [{prop.GetPropertyName()}] TYPE {prop.GetDbTypeByType(_repository.DataBaseTypes)}, ALTER COLUMN [{prop.GetPropertyName()}] SET DEFAULT {Querys.GetValueByTypeSTRING(MethodHelper.ConvertValue(null, propType), _repository.DataBaseTypes)};");


                        }
                    }
                    else if (!prop.IsInternalType && !prop.ContainAttribute<JsonDocument>() && !prop.ContainAttribute<XmlDocument>())
                        GetDatabase_Diff(prop.PropertyType, str, createdTables);
                    else
                    {
                        codeToDataBaseMerge.Sql.Append(string.Format("\nALTER TABLE {0} ADD " + (_repository.DataBaseTypes == DataBaseTypes.PostgreSql ? "COLUMN" : "") + " [{1}] {2} {3} DEFAULT {4};", tableName.GetName(_repository.DataBaseTypes), prop.GetPropertyName(), prop.GetDbTypeByType(_repository.DataBaseTypes),
                              (Nullable.GetUnderlyingType(propType) != null || propType == typeof(string)) && !prop.ContainAttribute<NotNullable>() ? " NULL" : " NOT NULL", Querys.GetValueByTypeSTRING(MethodHelper.ConvertValue(null, propType), _repository.DataBaseTypes)));
                    }
                }
            }

            var colRemove = new CodeToDataBaseMerge() { Object_Type = tableType };
            // Now lets clean the table and remove unused columns
            foreach (var col in table.Values.Where(x =>
             !props.Any(a => string.Equals(x.ColumnName, a.GetPropertyName(), StringComparison.CurrentCultureIgnoreCase) &&
             (a.GetDbTypeByType(_repository.DataBaseTypes) != null || (!a.IsInternalType || a.ContainAttribute<JsonDocument>() || a.ContainAttribute<XmlDocument>())) &&
             !a.ContainAttribute<ExcludeFromAbstract>())))
            {
                if (_repository.DataBaseTypes != DataBaseTypes.Sqllight)
                {
                    if (_repository.DataBaseTypes == DataBaseTypes.Mssql)
                    {
                        var constraine = Properties.Resources.DropContraine
                            .Replace("@tb", $"'{tableName.Name}'")
                            .Replace("@col", $"'{col.ColumnName}'")
                            .Replace("@schema", $"'{tableName.Schema ?? ""}'")
                            .Replace("@TableName", "@" + counter++)
                            .Replace("@ColumnName", "@" + counter++)
                            .Replace("@fullName", "@" + counter++)
                            .Replace("@DROP_COMMAND", "@" + counter++)
                            .Replace("@FOREIGN_KEY_NAME", "@" + counter++);

                        colRemove.Sql.Append(constraine);

                    }
                    colRemove.Sql.Append(string.Format("\nALTER TABLE {0} DROP COLUMN IF EXISTS [{1}];", tableName.GetName(_repository.DataBaseTypes), col.ColumnName));

                }
                else
                {
                    colRemove.Sql.Append(string.Format("DROP TABLE IF exists [{0}_temp];\nCREATE TABLE [{0}_temp] AS SELECT {1} FROM [{0}];", tableName.Name, string.Join(",", table.Values.ToList().FindAll(x =>
                    props.Any(a => string.Equals(x.ColumnName, a.GetPropertyName(), StringComparison.CurrentCultureIgnoreCase) &&
                    (a.GetDbTypeByType(_repository.DataBaseTypes) != null || !a.IsInternalType) &&
                    !a.ContainAttribute<ExcludeFromAbstract>())).Select(x => x.ColumnName))));
                    colRemove.Sql.Append(string.Format("DROP TABLE [{0}];\n", tableName.Name));
                    colRemove.Sql.Append(string.Format("ALTER TABLE [{0}_temp] RENAME TO [{0}]; ", tableName.Name));
                }
                colRemove.DataLoss = true;
            }
            str.Add(colRemove);

            foreach (var prop in props.Where(x => !x.IsInternalType && !x.ContainAttribute<JsonDocument>() && !x.ContainAttribute<XmlDocument>() && !x.ContainAttribute<XmlDocument>() && !x.ContainAttribute<ExcludeFromAbstract>()).GroupBy(x => x.Name).Select(x => x.First()))
            {
                var type = prop.PropertyType.GetActualType();
                if (type.GetPrimaryKey() != null)
                    GetDatabase_Diff(type, str, createdTables);
            }

            str.Add(codeToDataBaseMerge);
            return str;
        }


        public void CreateTable(Type tableType, List<Type> createdTables = null, bool force = false)
        {
            tableType = tableType.GetActualType();
            var tableData = _repository.GetColumnSchema(tableType);
            if (!force && tableData.Values.Any())
                return;
            RemoveTable(tableType);
            var data = GetDatabase_Diff(tableType);

            data.Execute(true);
        }

        public void RemoveTable(Type tableType, List<Type> tableRemoved = null, bool remove = true)
        {


            if (tableRemoved == null)
                tableRemoved = new List<Type>();
            if (tableRemoved.Any(x => x == tableType))
                return;
            GlobalConfiguration.Log?.Info("Removig", tableType);
            tableRemoved.Insert(0, tableType);
            var props = DeepCloner.GetFastDeepClonerProperties(tableType);

            foreach (var prop in props.Where(x => (!x.IsInternalType || x.ContainAttribute<ForeignKey>()) && !x.ContainAttribute<ExcludeFromAbstract>() && !x.ContainAttribute<JsonDocument>() && !x.ContainAttribute<XmlDocument>()))
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

            var tableData = _repository.GetColumnSchema(tableType);
            if (!tableData.Values.Any())
                return;
            var c = tableRemoved.Count;
            _repository.CreateTransaction();
            while (c > 0)
            {
                for (var i = tableRemoved.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var tType = tableRemoved[i];
                        if (_repository.GetColumnSchema(tType).Values.Any())
                        {
                            Database.CachedColumnSchema.Remove(tType.FullName + _repository.DataBaseTypes.ToString());
                            var tableName = tType.TableName();
                            _repository.ExecuteNonQuery(_repository.GetSqlCommand("DELETE FROM " + tableName.GetName(_repository.DataBaseTypes) + ";"));
                            var cmd = _repository.GetSqlCommand("DROP TABLE " + tableName.GetName(_repository.DataBaseTypes) + ";");
                            _repository.ExecuteNonQuery(cmd);
                            Database.CachedColumnSchema.Remove(tType.FullName + _repository.DataBaseTypes.ToString());
                        }


                        c--;
                    }
                    catch (Exception e)
                    {
                        if (_repository.DataBaseTypes == DataBaseTypes.PostgreSql)
                        {
                            _repository.Renew();
                        }
                    }
                }
            }
        }
        #endregion
    }
}
