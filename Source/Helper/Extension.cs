using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using FastDeepCloner;

namespace EntityWorker.Core.Helper
{
    public static class Extension
    {
        private static readonly Dictionary<IFastDeepClonerProperty, string> CachedPropertyNames = new Dictionary<IFastDeepClonerProperty, string>();

        private static readonly Dictionary<Type, string> DbMapper = new Dictionary<Type, string>()
        {
            {typeof(int), "BIGINT"},
            {typeof(long), "BIGINT"},
            {typeof(string), "NVARCHAR(4000)"},
            {typeof(bool), "BIT"},
            {typeof(DateTime), "DATETIME"},
            {typeof(float), "FLOAT"},
            {typeof(decimal), "DECIMAL(18,0)"},
            {typeof(Guid), "UNIQUEIDENTIFIER"},
            {typeof(byte[]), "varbinary(MAX)"},
        };

        public static string TrimEnd(this string source, string value)
        {
            return !source.EndsWith(value) ? source : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
        }

        public static string InsertLast(this string str, string text, char ch)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(ch.ToString()))
                str = str.Insert(str.Length - 1, text);
            else
            {
                var i = str.Length - 1;
                for (var j = str.Length - 1; j >= 0; j--)
                {
                    if (str[j] == ch) continue;
                    i = j + 1;
                    break;
                }
                str = str.Insert(i, text);
            }
            return str;
        }

        public static string GetEntityKey(this IDbEntity entity)
        {
            return string.Format("{0}&{1}", entity.GetType().FullName, entity.Id);
        }

        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDbTypeByType(this Type type)
        {
            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);

            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            return DbMapper.ContainsKey(type) ? DbMapper[type] : null;
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
            CachedPropertyNames.Add(prop, prop.GetCustomAttribute<PropertyName>()?.Name ?? prop.Name);
            return CachedPropertyNames[prop];
        }

        /// <summary>
        /// Get the Primary key from type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IFastDeepClonerProperty GetPrimaryKey(this Type type)
        {
            return DeepCloner.GetFastDeepClonerProperties(type).FirstOrDefault(x => x.ContainAttribute<PrimaryKey>());
        }

        public static object CreateInstance(this Type type, bool uninitializedObject = true)
        {
            return uninitializedObject ? FormatterServices.GetUninitializedObject(type) : Activator.CreateInstance(type);
        }

        /// <summary>
        /// Get IList Actual Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static readonly Dictionary<Type, Type> CachedActualType = new Dictionary<Type, Type>();
        public static Type GetActualType(this Type type)
        {
            if (CachedActualType.ContainsKey(type))
                return CachedActualType[type];
            else if (type.GetTypeInfo().IsArray)
                CachedActualType.Add(type, type.GetElementType());
            else if (type.GenericTypeArguments.Any())
                CachedActualType.Add(type, type.GenericTypeArguments.First());
            else if (type.FullName?.Contains("List`1") ?? false)
                CachedActualType.Add(type, type.GetRuntimeProperty("Item").PropertyType);
            else
                CachedActualType.Add(type, type);

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
            var regex = new System.Text.RegularExpressions.Regex(@"[^-A-Za-z0-9+/=]|=[^=]|={3,}$");
            if (regex.Match(str).Success)
            {
                return false;
            }

            try
            {
                // If no exception is caught, then it is possibly a base64 encoded string
                var stream = new MemoryStream(Convert.FromBase64String(str));
                return true;
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }

        /// <summary>
        /// Convert From type to another type,
        /// make sure to have the same propertyNames in both or you could also map them by PropertyName Attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T ToType<T>(this object o) where T : class
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
        public static List<T> Clone<T>(this List<T> items, FieldType fieldType = FieldType.PropertyInfo) where T : class, IDbEntity
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
                    result.Add(tempList.Last());
            }
            return result;
        }

        internal static Type TypeByTypeAndDbIsNull(this Type propertyType, bool allowDbNull)
        {
            if (propertyType == typeof(int))
                return allowDbNull ? typeof(int?) : typeof(int);

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

            return propertyType == typeof(byte[]) ? typeof(byte[]) : typeof(string);
        }

        private static readonly Dictionary<string, Exception> CachedSqlException = new Dictionary<string, Exception>();
        private static readonly Dictionary<string, LightDataTable> CachedGetSchemaTable = new Dictionary<string, LightDataTable>();
        internal static ILightDataTable ReadData(this ILightDataTable data, IDataReader reader, string primaryKey = null, string command = null)
        {
            var i = 0;
            if (reader.FieldCount <= 0)
                return data;
            data.TablePrimaryKey = primaryKey;

            if (reader.FieldCount <= 0)
            {
                reader.Close();
                reader.Dispose();
                return data;
            }
            try
            {

                if (!CachedSqlException.ContainsKey(command))
                {
                    if (!CachedGetSchemaTable.ContainsKey(command))
                        CachedGetSchemaTable.Add(command, new LightDataTable(reader.GetSchemaTable()));
                    foreach (var item in CachedGetSchemaTable[command].Rows)
                    {
                        //var isKey = Converter<bool>.Parse(item["IsKey"]);
                        var columnName = item["ColumnName"].ToString();
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

                    }
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(command))
                    CachedSqlException.Add(command, e);
                return ReadData(data, reader, primaryKey, command);
            }

            while (reader.Read())
            {
                var row = data.NewRow();
                reader.GetValues(row.ItemArray);
                data.AddRow(row);
            }

            reader.Close();
            reader.Dispose();
            return data;
        }
    }
}
