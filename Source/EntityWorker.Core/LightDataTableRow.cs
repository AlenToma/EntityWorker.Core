using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Reflection;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Object.Library;

namespace EntityWorker.Core
{
    public class LightDataTableRow : LightDataTableShared
    {
        internal object[] _itemArray;
        public object[] ItemArray
        {
            get => _itemArray;
            set
            {
                if (_itemArray == null)
                    _itemArray = new object[ColumnLength];
                for (var i = 0; i <= value.Length - 1; i++)
                    this[i] = value[i];
            }
        }

        public object this[string columnName, bool loadDefaultOnError = false]
        {
            get
            {
                try
                {
                    if (loadDefaultOnError)
                        TypeValidation(ref _itemArray[Columns[columnName].ColumnIndex], Columns[columnName].DataType, loadDefaultOnError, Columns[columnName].DefaultValue);
                    return _itemArray[Columns[columnName].ColumnIndex];
                }
                catch (Exception ex)
                {
                    throw new Exception("ColumnName:" + columnName + " " + ex.Message);
                }
            }
            set
            {
                try
                {
                    var column = Columns[columnName];
                    if (loadDefaultOnError)
                        TypeValidation(ref value, column.DataType, loadDefaultOnError, column.DefaultValue);
                    _itemArray[column.ColumnIndex] = value;
                }
                catch (Exception ex)
                {
                    throw new Exception("ColumnName:" + columnName + " " + ex.Message);
                }
            }
        }

        public object this[int columnIndex, bool loadDefaultOnError = false]
        {
            get
            {
                try
                {
                    if (loadDefaultOnError)
                        TypeValidation(ref _itemArray[columnIndex], ColumnsWithIndexKey[columnIndex].DataType, loadDefaultOnError, ColumnsWithIndexKey[columnIndex].DefaultValue);
                    return _itemArray[columnIndex];
                }
                catch (Exception ex)
                {
                    throw new Exception("ColumnName:" + ColumnsWithIndexKey[columnIndex].ColumnName + " " + ex.Message);
                }
            }
            set
            {
                try
                {
                    var column = ColumnsWithIndexKey[columnIndex];
                    if (loadDefaultOnError)
                        TypeValidation(ref value, column.DataType, loadDefaultOnError, column.DefaultValue);

                    _itemArray[column.ColumnIndex] = value;
                }
                catch (Exception ex)
                {
                    throw new Exception("ColumnName:" + ColumnsWithIndexKey[columnIndex].ColumnName + " " + ex.Message);
                }
            }
        }

        public object this[LightDataTableColumn column, bool loadDefaultOnError = false]
        {
            get
            {
                if (loadDefaultOnError)
                    TypeValidation(ref _itemArray[column.ColumnIndex], column.DataType, loadDefaultOnError, column.DefaultValue);
                return _itemArray[column.ColumnIndex];
            }
            set
            {
                if (loadDefaultOnError)
                    TypeValidation(ref value, column.DataType, loadDefaultOnError, column.DefaultValue);
                _itemArray[column.ColumnIndex] = value;
            }
        }

        public LightDataTableRow() : base()
        {
        }

        internal LightDataTableRow(object[] itemArray, ColumnsCollections<string> columns, ColumnsCollections<int> columnWithIndex, CultureInfo cultureInfo = null) : base(cultureInfo)
        {
            base.Columns = columns;
            base.ColumnsWithIndexKey = columnWithIndex;
            ItemArray = itemArray;
            ColumnLength = itemArray.Length - 1;
        }

        internal LightDataTableRow(LightDataTableRow row, ColumnsCollections<string> columns, ColumnsCollections<int> columnWithIndex, CultureInfo cultureInfo = null) : base(cultureInfo)
        {
            base.Columns = columns;
            base.ColumnsWithIndexKey = columnWithIndex;
            ItemArray = row.ItemArray;
            ColumnLength = row.ColumnLength;
        }

        internal LightDataTableRow(int columnLength, ColumnsCollections<string> columns, ColumnsCollections<int> columnWithIndex, CultureInfo cultureInfo = null) : base(cultureInfo)
        {
            base.Columns = columns;
            base.ColumnsWithIndexKey = columnWithIndex;
            _itemArray = new object[columnLength];
            ColumnLength = columnLength;
        }

        /// <summary>
        /// Column ContainKey By Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ContainKey<T, TP>(Expression<Func<T, TP>> action)
        {
            var member = (MemberExpression)action.Body;
            var propertyName = member.Member.Name;
            return Columns.ContainsKey(propertyName);

        }

        /// <summary>
        /// Set Value By Expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public TP SetTValue<T, TP>(Expression<Func<T, TP>> action, TP value)
        {
            var key = action.GetMemberName();
            return (TP)(this[key] = value);

        }


        /// <summary>
        /// return already converted value by T eg row<string/>(0) its a lot faster when reading values, but the returned values is wont be a sheared one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Value<T>(int key)
        {
            return (T)this[key];
        }

        /// <summary>
        /// return already converted value by T eg row<string/>(0) its a lot faster when reading values, but the returned values is wont be a sheared one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Value<T>(string key)
        {
            return (T)this[key];
        }

        /// <summary>
        /// Get Property By expression;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public TP Value<T, TP>(Expression<Func<T, TP>> action)
        {
            var propertyName = action.GetMemberName();
            var v = this[propertyName];
            TypeValidation(ref v, typeof(TP), true);
            return (TP)v;
        }

        /// <summary>
        /// return already converted value by T eg row<string/>(0) its a lot faster when reading values, but the returned values is wont be a sheared one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Value<T>(LightDataTableColumn key)
        {
            return (T)this[key];
        }

        public T Value<T>(Enum key)
        {
            return (T)this[key.ToString()];
        }

        public T ValueAndConvert<T>(Enum key)
        {
            return this.ValueAndConvert<T>(key.ToString());
        }

        public T TryValueAndConvert<T>(Enum key, bool loadDefault = false)
        {
            return this.TryValueAndConvert<T>(key.ToString(), loadDefault);
        }


        public T ValueAndConvert<T>(int key, bool loadDefault = false)
        {
            var column = this.ColumnsWithIndexKey[key];
            return this.ValueAndConvert<T>(column.ColumnName, loadDefault);
        }

        public object ValueAndConvert(Type type, int key, bool loadDefault = false)
        {
            var column = this.ColumnsWithIndexKey[key];
            return this.ValueAndConvert(type, column.ColumnName, loadDefault);
        }

        /// <summary>
        /// This will try to load the selected value and convert it to the selected type when it fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="loadDefault"></param>
        /// <returns></returns>
        public T ValueAndConvert<T>(string key, bool loadDefault = false)
        {
            return (T)ValueAndConvert(typeof(T), key, loadDefault);
        }

        /// <summary>
        /// This will try to load the selected value and convert it to the selected type when it fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="loadDefault"></param>
        /// <returns></returns>
        public object ValueAndConvert(Type type, string key, bool loadDefault = false)
        {
            var v = this[key, loadDefault];
            if (v != DBNull.Value && v != null && v.GetType() != Columns[key].DataType && !(type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>)) && v != "")
                return Convert.ChangeType(this[key], type);
            if (v == null || v == DBNull.Value || string.IsNullOrEmpty(v.ToString()))
            {
                TypeValidation(ref v, type, true);
                if (v == null && type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>))
                    return ValueByType(type);
            }
            else
                TypeValidation(ref v, type, false);

            if (v?.GetType() != type && (!type.GetTypeInfo().IsGenericType || type.GetTypeInfo().GetGenericTypeDefinition() != typeof(Nullable<>)))
                return Convert.ChangeType(v, type);

            return v?.GetType() != type ? Convert.ChangeType(v, type.GetGenericArguments()[0]) : v;
        }

        public T TryValueAndConvert<T>(string key, bool loadDefault = false)
        {
            try
            {
                return ValueAndConvert<T>(key, loadDefault);
            }
            catch
            {
                return (T)ValueByType(typeof(T));
            }
        }


        public object TryValueAndConvert(Type type, string key, bool loadDefault = false)
        {
            try
            {
                return ValueAndConvert(type, key, loadDefault);
            }
            catch
            {
                return ValueByType(type);
            }
        }


        public object TryValueAndConvert(Type type, int index, bool loadDefault = false)
        {
            try
            {
                return ValueAndConvert(type, index, loadDefault);
            }
            catch
            {
                return ValueByType(type);
            }
        }

        public T TryValueAndConvert<T>(int index, bool loadDefault = false)
        {
            try
            {
                return ValueAndConvert<T>(index, loadDefault);
            }
            catch
            {
                return (T)ValueByType(typeof(T));
            }
        }

        public TP ValueAndConvert<T, TP>(Expression<Func<T, TP>> action)
        {
            var propertyName = action.GetMemberName();
            return (TP)ValueAndConvert<TP>(propertyName);
        }


        /// <summary>
        /// Convert LightDataRow to DataRow
        /// </summary>
        /// <param name="parentTable"></param>
        /// <returns></returns>
        public DataRow ToDataRow(DataTable parentTable)
        {
            var row = parentTable.NewRow();
            foreach (var item in Columns.Values)
                if (parentTable.Columns.Contains(item.ColumnName))
                {
                    var v = this[item.ColumnName];
                    TypeValidation(ref v, parentTable.Columns[item.ColumnName].DataType, true);
                    if (row != null && row[item.ColumnName] != v)
                        row[item.ColumnName] = v;
                }
            return row;
        }

        /// <summary>
        /// Merge two rows together.
        /// </summary>
        /// <param name="row"></param>
        public LightDataTableRow Merge(LightDataTableRow row)
        {
            foreach (var item in row.Columns)
                if (Columns.ContainsKey(item.Key))
                    if (this[item.Key] != row[item.Key])
                        this[item.Key, true] = row[item.Key, true];
            return this;
        }

        /// <summary>
        /// Merge a class to the selected LightDataRow
        /// </summary>
        /// <param name="objectToBeMerged"></param>
        /// <returns></returns>
        public LightDataTableRow MergeUnKnownObject(object objectToBeMerged)
        {
            if ((objectToBeMerged as IDictionary<string, object>) == null)
            {
                foreach (var property in FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(objectToBeMerged.GetType()))
                {
                    var name = Columns.ContainsKey(property.Name) ? property.Name : property.GetPropertyName();
                    if (!Columns.ContainsKey(name)) continue;
                    try
                    {
                        var v = property.GetValue(objectToBeMerged);
                        if (property.IsInternalType)
                        {
                            TypeValidation(ref v, Columns[name].DataType, true);

                        }
                        else if (v != null)
                        {
                            v = new LightDataTable(v);
                        }

                        if (name != null && this[name] != v)
                            this[name] = v;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
            else
            {
                var dictionary = (IDictionary<string, object>)objectToBeMerged;
                foreach (var key in dictionary.Keys)
                {
                    var name = Columns.ContainsKey(key) ? key : null;
                    if (name == null)
                        continue;
                    var v = dictionary[key];
                    TypeValidation(ref v, Columns[name].DataType, true);
                    if (!string.IsNullOrEmpty(name) && this[name] != v)
                        this[name] = v;
                }
            }
            return this;
        }
        /// <summary>
        /// Merge LightDataRow to an object
        /// </summary>
        /// <param name="selectedObject"></param>
        public void MergeToAnObject(object selectedObject)
        {
            foreach (var prop in FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(selectedObject.GetType()))
            {
                var name = Columns.ContainsKey(prop.Name) ? prop.Name : prop.GetPropertyName();
                if (!Columns.ContainsKey(name) || !prop.CanReadWrite) continue;
                try
                {
                    var v = this[name];
                    if (!(v is LightDataTable))
                    {
                        TypeValidation(ref v, prop.PropertyType, true);

                    }
                    else
                    {
                        var table = v as LightDataTable;
                        if (table == null || !table.Rows.Any())
                            continue;
                        v = table.Rows.ToObject(prop.PropertyType);
                        if (prop.PropertyType != prop.PropertyType.GetActualType())
                            v = (v as IList)?.Cast<object>().FirstOrDefault();
                    }
                    prop.SetValue(selectedObject, v);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Convert the current created row to an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ToObject<T>()
        {
            var o = FormatterServices.GetUninitializedObject(typeof(T)) is IList ? typeof(T).CreateInstance() : FormatterServices.GetUninitializedObject(typeof(T));
            var obj = o is IList
                ?
                    o.GetType().GetActualType().CreateInstance()
                : typeof(T).CreateInstance();
            foreach (var pr in FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(obj.GetType()))
            {
                var name = pr.GetPropertyName();
                if (!Columns.ContainsKey(name) || !pr.CanReadWrite)
                    continue;
                var value = this[name, true];
                if (!(value is LightDataTable))
                {
                    if (value != null && pr.ContainAttribute<ToBase64String>())
                    {
                        if (value.ConvertValue<string>().IsBase64String())
                        {
                            value = MethodHelper.DecodeStringFromBase64(value.ConvertValue<string>());
                        }
                    }
                    else if (value != null && pr.ContainAttribute<DataEncode>())
                    {
                        value = new DataCipher(pr.GetCustomAttribute<DataEncode>().Key, pr.GetCustomAttribute<DataEncode>().KeySize).Decrypt(value.ConvertValue<string>());
                    }
                    else if (pr.ContainAttribute<JsonDocument>())
                        value = value?.ToString().FromJson(pr.PropertyType);
                    else if (pr.ContainAttribute<XmlDocument>())
                        value = value?.ToString().FromXml();
                    TypeValidation(ref value, pr.PropertyType, true);
                }
                else
                {
                    var table = value as LightDataTable;
                    if (table == null || !table.Rows.Any())
                        continue;
                    value = table.Rows.ToObject(pr.PropertyType);
                    if (pr.PropertyType == pr.PropertyType.GetActualType())
                        value = (value as IList)?.Cast<object>().FirstOrDefault();
                }


                try
                {
                    pr.SetValue(obj, value);
                }
                catch
                {
                    // ignored
                }
            }

            if (o is IList)
            {
                ((IList)o).Add(obj);
                return (T)o;
            }
            else
                return (T)obj;

        }

        /// <summary>
        /// Convert the current created row to an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object ToObject(object t)
        {
            var type = t is Type ? (Type)t : t.GetType();
            var o = type.CreateInstance();
            var obj = type.GetActualType().CreateInstance();

            foreach (var pr in FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(obj?.GetType()))
            {
                var name = pr.GetPropertyName();
                if (!Columns.ContainsKey(name) || !pr.CanReadWrite)
                    continue;
                var value = this[name, true];
                if (!(value is LightDataTable))
                {
                    if (pr.ContainAttribute<ToBase64String>())
                    {
                        if (value != null && value.ConvertValue<string>().IsBase64String())
                        {
                            value = MethodHelper.DecodeStringFromBase64(value.ConvertValue<string>());
                        }
                    }
                    else if (value != null && pr.ContainAttribute<DataEncode>())
                        value = new DataCipher(pr.GetCustomAttribute<DataEncode>().Key, pr.GetCustomAttribute<DataEncode>().KeySize).Decrypt(value.ConvertValue<string>());
                    else if (pr.ContainAttribute<JsonDocument>())
                        value = value?.ToString().FromJson(pr.PropertyType);
                    else if (pr.ContainAttribute<XmlDocument>())
                        value = value?.ToString().FromXml();

                    TypeValidation(ref value, pr.PropertyType, true);
                }
                else
                {
                    var table = value as LightDataTable;
                    if (table == null || !table.Rows.Any())
                        continue;
                    value = table.Rows.ToObject(pr.PropertyType);
                    if (pr.PropertyType == pr.PropertyType.GetActualType())
                        value = (value as IList)?.Cast<object>().FirstOrDefault();
                }

                try
                {
                    pr.SetValue(obj, value);
                }
                catch
                {
                    // ignored
                }
            }

            if (o is IList)
            {
                ((IList)o).Add(obj);
                return o;
            }
            else
                return obj;

        }


        /// <summary>
        /// This Method should only be called from the lightdatatable object
        /// </summary>
        /// <param name="col"></param>
        /// <param name="value"></param>
        /// <param name="cols"></param>
        /// <param name="colsIndex"></param>
        internal void AddValue(LightDataTableColumn col, object value, ColumnsCollections<string> cols, ColumnsCollections<int> colsIndex)
        {
            ColumnsWithIndexKey = colsIndex;
            Columns = cols;
            ColumnLength = colsIndex.Count;
            var newList = _itemArray.ToList();
            newList.Add(value ?? ValueByType(col.DataType));
            _itemArray = newList.ToArray();
        }


        /// <summary>
        /// This Method should only be called from the lightdatatable object
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="cols"></param>
        /// <param name="colsIndex"></param>
        internal void Remove(int columnIndex, ColumnsCollections<string> cols, ColumnsCollections<int> colsIndex)
        {
            ColumnsWithIndexKey = colsIndex;
            Columns = cols;
            var newList = _itemArray.ToList();
            newList.RemoveAt(columnIndex);
            _itemArray = newList.ToArray();
        }
    }
}
