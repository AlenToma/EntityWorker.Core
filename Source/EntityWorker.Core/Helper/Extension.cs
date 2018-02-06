using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Interface;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.FastDeepCloner;
using System.Text.RegularExpressions;

namespace EntityWorker.Core.Helper
{
    /// <summary>
    /// UseFull Extension, that work with EntityWorker.Core
    /// </summary>
    public static class Extension
    {
        private static readonly Custom_ValueType<IFastDeepClonerProperty, string> CachedPropertyNames = new Custom_ValueType<IFastDeepClonerProperty, string>();
        private static readonly Custom_ValueType<Type, IFastDeepClonerProperty> CachedPrimaryKeys = new Custom_ValueType<Type, IFastDeepClonerProperty>();
        private static readonly Custom_ValueType<Type, string> CachedTableNames = new Custom_ValueType<Type, string>();

        private static readonly Custom_ValueType<Type, List<string>> DbMsSqlMapper = new Custom_ValueType<Type, List<string>>()
        {
            {typeof(int), new List<string>(){ "BIGINT" , "int", "single", "smallint", "tinyint" } },
            {typeof(long), new List<string>(){ "BIGINT" } },
            {typeof(string), new List<string>(){ "NVARCHAR(4000)" , "text", "varchar", "xml" } },
            {typeof(bool), new List<string>(){ "BIT"} },
            {typeof(DateTime), new List<string>(){ "DATETIME" , "date", "datetime2", "datetimeoffset", "smalldatetime" } },
            {typeof(TimeSpan), new List<string>(){ "DATETIME" , "time" } },
            {typeof(float), new List<string>(){ "FLOAT"} },
            {typeof(decimal), new List<string>(){ "DECIMAL(18,5)", "money" , "numeric", "smallmoney" } },
            {typeof(Guid), new List<string>(){ "UNIQUEIDENTIFIER"} },
            {typeof(byte[]), new List<string>(){ "varbinary(MAX)" , "image" , "rowversion", "timestamp" } },
            {typeof(char), new List<string>(){ "NVARCHAR(10)", "char" , "nchar" , "ntext" } },
            {typeof(double), new List<string>(){ "DECIMAL(18,5)", "money" , "numeric", "smallmoney" } },

        };

        private static readonly Custom_ValueType<Type, List<string>> DbSQLiteMapper = new Custom_ValueType<Type, List<string>>()
        {
            {typeof(int), new List<string>(){ "BIGINT" , "SMALLINT", "TINYINT", "MEDIUMINT", "UNSIGNED BIG INT", "INT2", "INT8" } },
            {typeof(long), new List<string>(){ "BIGINT", "INT", "INTEGER"}},
            {typeof(string),  new List<string>(){"NVARCHAR(4000)", "CHARACTER","VARYING CHARACTER", "NCHAR", "NATIVE CHARACTER" , "CLOB" }},
            {typeof(bool), new List<string>(){ "BIT"}},
            {typeof(DateTime), new List<string>(){ "DATETIME", "date"}},
            {typeof(TimeSpan), new List<string>(){ "DATETIME"}},
            {typeof(float),  new List<string>(){"FLOAT"}},
            {typeof(decimal), new List<string>(){ "DECIMAL(18,5)", "real" , "NUMERIC", "DOUBLE PRECISION"}},
            {typeof(Guid), new List<string>(){ "UNIQUEIDENTIFIER"}},
            {typeof(byte[]), new List<string>(){ "BLOB"}},
            {typeof(char), new List<string>(){ "NVARCHAR(10)"}},
            {typeof(double), new List<string>(){ "DECIMAL(18,5)", "real" , "NUMERIC", "DOUBLE PRECISION"}},
        };

        private static readonly Custom_ValueType<Type, List<string>> DbPostGresqlMapper = new Custom_ValueType<Type, List<string>>()
        {
            {typeof(int), new List<string>(){ "BIGINT", "smallint", "integer", "smallserial", "serial"}},
            {typeof(long), new List<string>(){ "BIGINT", "bigserial" } },
            {typeof(string), new List<string>(){ "TEXT", "varchar" , "character varying", "character", "json", "enum" } },
            {typeof(bool), new List<string> { "BOOLEAN" } },
            {typeof(DateTime), new List<string>(){ "TIMESTAMP" , "timestamp", "date", "interval" } },
            {typeof(TimeSpan), new List<string>(){ "TIME" } },
            {typeof(float),new List<string>(){ "FLOAT"} },
            {typeof(decimal), new List<string>(){ "DECIMAL(18,5)", "numeric" , "real" , "money"  } },
            {typeof(Guid), new List<string>(){ "uuid"} },
            {typeof(byte[]), new List<string>(){ "bytea"} },
            {typeof(char), new List<string>(){ "VARCHAR(10)", "char"} },
            {typeof(double), new List<string>(){ "DECIMAL(18,5)", "numeric" , "real" , "money"  } },
        };



        internal static string GetValidSqlName(this DataBaseTypes dbtype, string col)
        {
            return dbtype == DataBaseTypes.PostgreSql ? col : "[" + col + "]";
        }

        internal static string CleanValidSqlName(this string col, DataBaseTypes dbtype)
        {
            if (dbtype == DataBaseTypes.PostgreSql)
                return col.Replace("[", "").Replace("]", "");
            return col;
        }

        public static string GetMemberName<T, TP>(this Expression<Func<T, TP>> action)
        {
            var member = action.Body is UnaryExpression ? ((MemberExpression)((UnaryExpression)action.Body).Operand) : (action.Body is MethodCallExpression ? ((MemberExpression)((MethodCallExpression)action.Body).Object) : (MemberExpression)action.Body);
            return member?.Member.Name;
        }

        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static T ClearAllIdsHierarchy<T>(this T item, bool includeIndependedData = false)
        {
            return (T)ClearAllIdsHierarchy(item as object, includeIndependedData);
        }

        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static List<T> ClearAllIdsHierarchy<T>(List<T> item, bool includeIndependedData = false)
        {
            return (List<T>)ClearAllIdsHierarchy(item as object, includeIndependedData);
        }

        /// <summary>
        /// Clear all PrimaryId and ForeignKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static object ClearAllIdsHierarchy(object item, bool includeIndependedData = false)
        {
            if (item == null)
                return item;
            if (item is IList)
            {
                foreach (var tm in item as IList)
                {
                    if (tm != null)
                        ClearAllIdsHierarchy(tm, includeIndependedData);
                }
            }
            else
            {
                if (item == null)
                    return item;

                var props = DeepCloner.GetFastDeepClonerProperties(item.GetType());
                foreach (var p in props)
                {
                    if (p.ContainAttribute<ExcludeFromAbstract>() || (p.ContainAttribute<IndependentData>() && !includeIndependedData))
                        continue;
                    if (!p.IsInternalType)
                    {
                        ClearAllIdsHierarchy(p.GetValue(item), includeIndependedData);
                        continue;
                    }
                    else if (p.ContainAttribute<PrimaryKey>() || p.ContainAttribute<ForeignKey>())
                        p.SetValue(item, MethodHelper.ConvertValue(null, p.PropertyType)); // Reset to default

                }
            }

            return item;
        }

        /// <summary>
        /// TrimEnd with string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimEnd(this string source, string value)
        {
            return !source.EndsWith(value) ? source : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
        }

        /// <summary>
        /// Try and insert Last
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="text"> string to insert</param>
        /// <param name="identifier"></param>
        /// <param name="insertLastIfNotFound">insert after a specific string, count is from last</param>
        /// <returns></returns>
        public static string InsertAfter(this string str, string text, string identifier, bool insertLastIfNotFound = true)
        {
            var txt = "";
            var found = false;
            for (var j = str.Length - 1; j >= 0; j--)
            {
                txt = str[j] + txt;
                if (txt == identifier)
                {
                    str = str.Insert(j, text);
                    found = true;
                    break;
                }
                else if (txt.Length >= identifier.Length || txt.Last() != identifier.Last() || (txt.Length >= 2 && identifier.Length >= 2 && txt[txt.Length - 2] != identifier[identifier.Length - 2]))
                    txt = "";
            }

            if (!found && insertLastIfNotFound)
                str += text;
            return str;
        }

        public static string InsertBefore(this string str, string text, string identifier, bool insertLastIfNotFound = true)
        {
            var txt = "";
            var found = false;
            for (var j = str.Length - 1; j >= 0; j--)
            {
                txt = str[j] + txt;
                if (txt == identifier)
                {
                    str = str.Insert(j, text);
                    found = true;
                    break;
                }
                else if (txt.Length >= identifier.Length || txt.Last() != identifier.Last() || (txt.Length >= 2 && identifier.Length >= 2 && txt[txt.Length - 2] != identifier[identifier.Length - 2]))
                    txt = "";
            }
            if (!found && insertLastIfNotFound)
                str += text;

            return str;
        }


        /// <summary>
        /// Generate an entityKey Primary Id cant be null or empty
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string EntityKey(this object entity) => entity.GetType().FullName + entity.GetPrimaryKeyValue()?.ToString();


        public static StringBuilder InsertBefore(this StringBuilder str, string text, string identifier, bool insertLastIfNotFound = true)
        {
            str = new StringBuilder(str.ToString().InsertBefore(text, identifier, insertLastIfNotFound));
            return str;
        }

        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static string GetDbTypeByType(this Type type, DataBaseTypes dbType)
        {
            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);

            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (dbType == DataBaseTypes.Mssql)
                return DbMsSqlMapper.ContainsKey(type) ? DbMsSqlMapper[type].First() : null;
            else if (dbType == DataBaseTypes.Sqllight)
                return DbSQLiteMapper.ContainsKey(type) ? DbSQLiteMapper[type].First() : null;
            else return DbPostGresqlMapper.ContainsKey(type) ? DbPostGresqlMapper[type].First() : null;
        }

        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static string GetDbTypeByType(this IFastDeepClonerProperty prop, DataBaseTypes dbType)
        {
            var type = prop.PropertyType;

            if (prop.ContainAttribute<StringFy>() || prop.ContainAttribute<DataEncode>() || prop.ContainAttribute<ToBase64String>())
                return typeof(string).GetDbTypeByType(dbType);


            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);

            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (dbType == DataBaseTypes.Mssql)
                return DbMsSqlMapper.ContainsKey(type) ? DbMsSqlMapper[type].First() : null;
            else if (dbType == DataBaseTypes.Sqllight)
                return DbSQLiteMapper.ContainsKey(type) ? DbSQLiteMapper[type].First() : null;
            else return DbPostGresqlMapper.ContainsKey(type) ? DbPostGresqlMapper[type].First() : null;
        }


        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static List<string> GetDbTypeListByType(this IFastDeepClonerProperty prop, DataBaseTypes dbType)
        {
            var type = prop.PropertyType;

            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);

            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (dbType == DataBaseTypes.Mssql)
                return DbMsSqlMapper.ContainsKey(type) ? DbMsSqlMapper[type] : null;
            else if (dbType == DataBaseTypes.Sqllight)
                return DbSQLiteMapper.ContainsKey(type) ? DbSQLiteMapper[type] : null;
            else return DbPostGresqlMapper.ContainsKey(type) ? DbPostGresqlMapper[type] : null;
        }

        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static List<string> GetDbTypeListByType(this Type type, DataBaseTypes dbType)
        {
            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);

            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (dbType == DataBaseTypes.Mssql)
                return DbMsSqlMapper.ContainsKey(type) ? DbMsSqlMapper[type] : null;
            else if (dbType == DataBaseTypes.Sqllight)
                return DbSQLiteMapper.ContainsKey(type) ? DbSQLiteMapper[type] : null;
            else return DbPostGresqlMapper.ContainsKey(type) ? DbPostGresqlMapper[type] : null;
        }



        /// <summary>
        /// if date is between two dates
        /// </summary>
        /// <param name="input"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input > date1 && input < date2);
        }

        /// <summary>
        /// Get PropertyName from the cashed Properties
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string GetPropertyName(this IFastDeepClonerProperty prop)
        {
            if (CachedPropertyNames.ContainsKey(prop))
                return CachedPropertyNames[prop];

            return CachedPropertyNames.GetOrAdd(prop, (new Regex("[^a-zA-Z0-9,_]").Replace(prop.GetCustomAttribute<PropertyName>()?.Name ?? prop.Name, "_")));
        }




        public static bool IsNumeric(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments().FirstOrDefault() ?? type;
            return type.IsPrimitive && type != typeof(char) && type != typeof(bool);
        }

        /// <summary>
        /// Get the Primary key from type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IFastDeepClonerProperty GetPrimaryKey(this Type type)
        {
            if (CachedPrimaryKeys.ContainsKey(type))
                return CachedPrimaryKeys[type];
            return CachedPrimaryKeys.GetOrAdd(type, DeepCloner.GetFastDeepClonerProperties(type).FirstOrDefault(x => x.ContainAttribute<PrimaryKey>()));
        }



        public static String TableName<T>()
        {
            return typeof(T).TableName();
        }

        public static String TableName(this Type type)
        {
            if (CachedTableNames.ContainsKey(type))
                return CachedTableNames[type];
            return CachedTableNames.GetOrAdd(type, (new Regex("[^a-zA-Z0-9,_]").Replace(type.GetCustomAttribute<Table>()?.Name ?? type.Name, "_")));
        }

        /// <summary>
        /// Get the Primary key from type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IFastDeepClonerProperty GetPrimaryKey(this object item)
        {
            return item.GetType().GetPrimaryKey();
        }

        /// <summary>
        /// Get the Primary key Value
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static object GetPrimaryKeyValue(this object item)
        {
            return item.GetPrimaryKey().GetValue(item);
        }

        /// <summary>
        /// Validate string and guid and long 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public static bool ObjectIsNew(this object value)
        {
            if (value == null || (value.GetType().IsNumeric() && value.ConvertValue<long>() <= 0))
                return true;
            else if (string.IsNullOrEmpty(value.ToString()) || value.ConvertValue<string>() == Guid.Empty.ToString())
                return true;
            return false;
        }

        /// <summary>
        /// Set the Primary key Value
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetPrimaryKeyValue(this object item, object value = null)
        {
            var prop = item.GetPrimaryKey();
            prop.SetValue(item, MethodHelper.ConvertValue(value, prop.PropertyType));
        }

        /// <summary>
        /// Create Instance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uninitializedObject"> true for FormatterServices.GetUninitializedObject and false for Activator.CreateInstance </param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, bool uninitializedObject = false)
        {
            return uninitializedObject ? FormatterServices.GetUninitializedObject(type) : DeepCloner.CreateInstance(type);
        }


        /// <summary>
        /// Get IList Actual Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static readonly Dictionary<Type, Type> CachedActualType = new Dictionary<Type, Type>();

        /// <summary>
        /// Get Internal type of IList
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetActualType(this Type type)
        {
            if (CachedActualType.ContainsKey(type))
                return CachedActualType[type];
            lock (CachedActualType)
            {
                if (type.GetTypeInfo().IsArray)
                    CachedActualType.Add(type, type.GetElementType());
                else if (type.GenericTypeArguments.Any())
                    CachedActualType.Add(type, type.GenericTypeArguments.First());
                else if (type.FullName?.Contains("List`1") ?? false)
                    CachedActualType.Add(type, type.GetRuntimeProperty("Item").PropertyType);
                else
                    CachedActualType.Add(type, type);
            }

            return CachedActualType[type];
        }

        /// <summary>
        /// Check if string is base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBase64String(this string str)
        {
            if ((str.Length % 4) != 0)
            {
                return false;
            }
            // Check that the string matches the base64 layout
            var regex = new System.Text.RegularExpressions.Regex(@"^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$");
            return regex.Match(str).Success;
        }

        /// <summary>
        /// Convert From type to another type,
        /// make sure to have the same propertyNames in both or you could also map them by PropertyName Attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T ToType<T>(this object o)
        {
            object item;
            if (typeof(T) == typeof(IList))
                return (T)(item = new LightDataTable(o).Rows.Select(x => ((IList)x.ToObject<T>())[0]).ToList());
            else return new LightDataTable(o).Rows.First().ToObject<T>();
        }

        /// <summary>
        /// Clone Object, se FastDeepCloner fo more information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> items, FieldType fieldType = FieldType.PropertyInfo)
        {
            return DeepCloner.Clone(items, new FastDeepClonerSettings()
            {
                FieldType = fieldType,
                OnCreateInstance = new Extensions.CreateInstance(FormatterServices.GetUninitializedObject)
            });
        }


        internal static List<string> ConvertExpressionToIncludeList(this Expression[] actions, bool onlyLast = false)
        {
            var result = new List<string>();
            if (actions == null) return result;
            foreach (var exp in actions)
            {
                var tempList = new List<string>();
                var expString = exp.ToString().Split('.');
                var propTree = "";
                foreach (var item in expString)
                {
                    var x = item.Trim().Replace("(", "").Replace(")", "").Replace("&&", "").Replace("||", "").Trim();
                    if (x.Any(char.IsWhiteSpace) || x.Contains("="))
                        continue;
                    propTree += ("." + x);
                    if (propTree.Split('.').Length == 4)
                        propTree = string.Join(".", propTree.Split('.').Skip(2));
                    tempList.Add(propTree.TrimStart('.'));
                }


                if (!onlyLast)
                    result.AddRange(tempList);
                else if (tempList.Any())
                {
                    var str = tempList.Last();
                    str = str?.Split('.').Length >= 2 ? string.Join(".", str.Split('.').Reverse().Take(2).Reverse()) : str;
                    result.Add(str);
                }
            }
            return result;
        }



        internal static Type TypeByTypeAndDbIsNull(this Type propertyType, bool allowDbNull)
        {
            if (propertyType == typeof(int))
                return allowDbNull ? typeof(int?) : typeof(int);

            if (propertyType == typeof(float))
                return allowDbNull ? typeof(int?) : typeof(int);

            if (propertyType == typeof(byte))
                return allowDbNull ? typeof(byte?) : typeof(byte);

            if (propertyType == typeof(decimal))
                return allowDbNull ? typeof(decimal?) : typeof(decimal);

            if (propertyType == typeof(double))
                return allowDbNull ? typeof(double?) : typeof(double);

            if (propertyType == typeof(float))
                return allowDbNull ? typeof(float?) : typeof(float);

            if (propertyType == typeof(DateTime))
                return allowDbNull ? typeof(DateTime?) : typeof(DateTime);

            if (propertyType == typeof(long))
                return allowDbNull ? typeof(long?) : typeof(long);

            if (propertyType == typeof(TimeSpan))
                return allowDbNull ? typeof(TimeSpan?) : typeof(TimeSpan);

            if (propertyType == typeof(bool))
                return allowDbNull ? typeof(bool?) : typeof(bool);

            if (propertyType == typeof(Guid))
                return allowDbNull ? typeof(Guid?) : typeof(Guid);

            return propertyType == typeof(byte[]) ? typeof(byte[]) : typeof(string);
        }


        internal static List<T> DataReaderConverter<T>(Transaction.Transaction repository, IDataReader reader, DbCommandExtended command)
        {
            return ((List<T>)DataReaderConverter(repository, reader, command, typeof(T)));
        }

        internal static Custom_ValueType<Type, DynamicBuilder> CachedDataRecord = new Custom_ValueType<Type, DynamicBuilder>();
        internal static IList DataReaderConverter(Transaction.Transaction repository, IDataReader reader, DbCommandExtended command, Type type)
        {
            var tType = type.GetActualType();
            var baseListType = typeof(List<>);
            var listType = baseListType.MakeGenericType(tType);
            var iList = DeepCloner.CreateInstance(listType) as IList;
#if (NETSTANDARD2_0 || NETSTANDARD1_3 || NETSTANDARD1_5)
            var props = DeepCloner.GetFastDeepClonerProperties(tType);
#endif
            try
            {
                while (reader.Read())
                {
                    object item = null;
                    object clItem = null;
#if (NETSTANDARD2_0 || NETSTANDARD1_3 || NETSTANDARD1_5)

                    item = DeepCloner.CreateInstance(tType);
                    clItem = DeepCloner.CreateInstance(tType);
                    var col = 0;

                    while (col < reader.FieldCount)
                    {

                        var columnName = reader.GetName(col);
                        var value = reader[columnName];

                        var prop = DeepCloner.GetProperty(tType, columnName);

                        if (prop == null)
                              prop = props.FirstOrDefault(x => x.GetPropertyName() == columnName || x.Name.ToLower() == columnName || x.GetPropertyName().ToLower() == columnName);

                        if (value != null && prop != null && prop.CanRead)
                        {
                            if (value as byte[] != null && prop.PropertyType.FullName.Contains("Guid"))
                                value = new Guid(value as byte[]);

                            var dataEncode = prop.GetCustomAttribute<DataEncode>();
                            var toBase64String = prop.GetCustomAttribute<ToBase64String>();

                            if (toBase64String != null)
                            {
                                if (value.ConvertValue<string>().IsBase64String())
                                    value = MethodHelper.DecodeStringFromBase64(value.ConvertValue<string>());
                            }
                            else if (dataEncode != null)
                                value = new DataCipher(dataEncode.Key, dataEncode.KeySize).Decrypt(value.ConvertValue<string>());
                            else value = MethodHelper.ConvertValue(value, prop.PropertyType);

                            prop.SetValue(item, value);
                            prop.SetValue(clItem, value);
                        }
                        col++;
                    }
#else
                    var cmReader = new DataRecordExtended(reader);
                    if (!CachedDataRecord.ContainsKey(tType))
                        CachedDataRecord.GetOrAdd(tType, DynamicBuilder.CreateBuilder(cmReader, tType));
                    var x = CachedDataRecord[tType];
                    item = x.Build(cmReader);
                    clItem = !(repository?.IsAttached(item) ?? true) ? x.Build(cmReader) : null;
#endif

                    if (clItem != null && !(repository?.IsAttached(clItem) ?? true))
                        repository?.AttachNew(clItem);
                    iList.Add(item);

                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                if (repository.OpenedDataReaders.ContainsKey(reader))
                    repository.OpenedDataReaders.Remove(reader);
            }

            return iList;
        }

        private static readonly Dictionary<string, Exception> CachedSqlException = new Dictionary<string, Exception>();
        private static readonly Dictionary<string, LightDataTable> CachedGetSchemaTable = new Dictionary<string, LightDataTable>();
        internal static ILightDataTable ReadData(this ILightDataTable data, DataBaseTypes dbType, IDataReader reader, DbCommandExtended command, string primaryKey = null, bool closeReader = true)
        {
            var i = 0;
            if (reader.FieldCount <= 0)
                return data;
            data.TablePrimaryKey = primaryKey;

            if (reader.FieldCount <= 0)
            {
                if (closeReader)
                {
                    reader.Close();
                    reader.Dispose();
                }
                return data;
            }
            if (command.TableType == null)
            {
                try
                {

                    var key = command?.TableType != null ? command.TableType.FullName : command.Command.CommandText;
                    if (!CachedSqlException.ContainsKey(command.Command.CommandText))
                    {
                        if (!CachedGetSchemaTable.ContainsKey(key))
                            CachedGetSchemaTable.Add(key, new LightDataTable(reader.GetSchemaTable()));

                        foreach (var item in CachedGetSchemaTable[key].Rows)
                        {
                            var columnName = item.Value<string>("ColumnName");
                            data.TablePrimaryKey = data.TablePrimaryKey == null && item.Columns.ContainsKey("IsKey") && item.TryValueAndConvert<bool>("IsKey", false) ? columnName : data.TablePrimaryKey;
                            var dataType = TypeByTypeAndDbIsNull(item["DataType"] as Type,
                                item.TryValueAndConvert<bool>("AllowDBNull", true));
                            if (data.Columns.ContainsKey(columnName))
                                columnName = columnName + i;
                            data.AddColumn(columnName, dataType);

                            i++;
                        }
                    }
                    else
                    {
                        for (var col = 0; col < reader.FieldCount; col++)
                        {
                            var columnName = reader.GetName(col);
                            var dataType = TypeByTypeAndDbIsNull(reader.GetFieldType(col) as Type, true);
                            if (data.Columns.ContainsKey(columnName))
                                columnName = columnName + i;
                            data.AddColumn(columnName, dataType);
                            i++;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!string.IsNullOrEmpty(command.Command.CommandText))
                        CachedSqlException.Add(command.Command.CommandText, e);
                    return ReadData(data, dbType, reader, command, primaryKey);
                }

            }
            else
            {
                foreach (var c in command.DataStructure.Columns.Values)
                    data.AddColumn(c.ColumnName, c.DataType, c.DefaultValue);
            }

            while (reader.Read())
            {
                var row = data.NewRow();
                reader.GetValues(row._itemArray);
                data.AddRow(row);
            }

            if (closeReader)
            {
                reader.Close();
                reader.Dispose();
            }

            return data;
        }
    }
}
