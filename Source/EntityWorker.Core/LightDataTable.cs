using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using FastDeepCloner;

namespace EntityWorker.Core
{
    public class LightDataTable : LightDataTableShared, ILightDataTable
    {
        internal PrimaryKeyIndex PrimaryKeyIndex = new PrimaryKeyIndex();
        private string _tablePrimaryKey;
        private int _pageNumber = 1;
        private int _setsPerPage = 20;
        private LightDataRowCollection _rows = new LightDataRowCollection();
        public string TableName { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)_rows.Count / (_setsPerPage > 0 ? _setsPerPage : 1));

        public string TablePrimaryKey
        {
            get => _tablePrimaryKey;
            set
            {
                if (_tablePrimaryKey != value)
                    PrimaryKeyIndex.ClearAndRenderValues(_rows, value);
                _tablePrimaryKey = value;

            }
        }



        public LightDataRowCollection Rows => _rows;


        /// <summary>
        /// ReOrders the _columns
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="position"></param>
        public ILightDataTable OrderBy(string columnName, bool position)
        {
            if (string.IsNullOrEmpty(columnName) || !Columns.ContainsKey(columnName))
                return this;
            _rows = !position ? new LightDataRowCollection(Rows.OrderBy(x => x[columnName, true])) : new LightDataRowCollection(Rows.OrderByDescending(x => x[columnName, true]));

            return this;
        }

        /// <summary>
        /// Orderby expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="action"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public ILightDataTable OrderBy<T, TP>(Expression<Func<T, TP>> action, bool position)
        {
            var member = (MemberExpression)action.Body;
            var columnName = member.Member.Name;
            if (!Columns.ContainsKey(columnName))
                return this;
            _rows = !position ? new LightDataRowCollection(Rows.OrderBy(x => x[columnName, true])) : new LightDataRowCollection(Rows.OrderByDescending(x => x[columnName, true]));
            return this;
        }


        /// <summary>
        /// return true if pageNumber exist
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public bool ValidatePagerByPageNumber(int pageNumber)
        {
            return !(pageNumber > TotalPages);

        }

        /// <summary>
        /// Merge two dataTable togather. The Two tables dose not have to have the same number of columns!
        /// </summary>
        /// <param name="data2"></param>
        /// <param name="mergeColumnAndRow"></param>
        public ILightDataTable Merge(LightDataTable data2, bool mergeColumnAndRow = true)
        {
            if (string.IsNullOrEmpty(_tablePrimaryKey))
                throw new Exception("Data cant be merged. There is no primary key specified");
            if (mergeColumnAndRow)
            {
                var columnsNotIncluded = data2.Columns.ToList().FindAll(x => !Columns.ContainsKey(x.Key));
                foreach (var col in columnsNotIncluded)
                    AddColumn(col.Value.ColumnName, col.Value.DataType, col.Value.DefaultValue ?? ValueByType(col.Value.DataType));

            }

            foreach (var row in data2.Rows)
            {

                if (data2.Columns.ContainsKey(_tablePrimaryKey) && FindByPrimaryKey<bool>(row[_tablePrimaryKey]))
                    FindByPrimaryKey<LightDataTableRow>(row[_tablePrimaryKey]).Merge(row);
                else if (mergeColumnAndRow)
                    AddRow(NewRow().Merge(row));
            }

            return this;

        }

        /// <summary>
        /// Merge two dataTables _columns togather. The Two tables dose not have to have the same number of columns!
        /// </summary>
        /// <param name="data2"></param>
        public ILightDataTable MergeColumns(LightDataTable data2)
        {
            var columnsNotIncluded = data2.Columns.ToList().FindAll(x => !Columns.ContainsKey(x.Key));
            foreach (var col in columnsNotIncluded)
                AddColumn(col.Value.ColumnName, col.Value.DataType, col.Value.DefaultValue ?? ValueByType(col.Value.DataType));
            return this;
        }

        /// <summary>
        /// Assign Value to column. 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ILightDataTable AssignValueToColumn(string columnName, object value)
        {
            var dataType = Columns[columnName].DataType;
            this.RemoveColumn(columnName);
            this.AddColumn(columnName, dataType, value);
            return this;
        }


        public void RemoveColumn(int columnIndex)
        {
            RemoveColumn(ColumnsWithIndexKey[columnIndex].ColumnName);
        }

        public void RemoveColumn(string columnName)
        {
            var colIndex = Columns[columnName].ColumnIndex;
            var columns = ColumnsWithIndexKey.Values.ToList();
            columns.RemoveAt(columns.FindIndex(x => x.ColumnName == columnName));
            Columns.Clear();
            ColumnsWithIndexKey.Clear();
            for (var i = 0; i < columns.Count; i++)
            {
                var col = new LightDataTableColumn(columns[i].ColumnName, columns[i].DisplayName, columns[i].DataType, columns[i].DefaultValue, i);
                ColumnsWithIndexKey.Add(i, col);
                Columns.Add(col.ColumnName, col);
            }
            ColumnLength--;
            foreach (var row in _rows)
                row.Remove(colIndex, Columns, ColumnsWithIndexKey);
        }



        /// <summary>
        /// Remove Column By Expression eg this.RemoveColumn((Employee x)=> x.EmployeeID);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ILightDataTable RemoveColumn<T, TP>(Expression<Func<T, TP>> action)
        {
            var member = (MemberExpression)action.Body;
            var propertyName = member.Member.Name;
            RemoveColumn(propertyName);
            return this;
        }

        /// <summary>
        ///  Merge two existing rows by its primaryKey
        /// </summary>
        /// <param name="rowToBeMerged"></param>
        public void MergeByPrimaryKey(LightDataTableRow rowToBeMerged)
        {
            if (string.IsNullOrEmpty(_tablePrimaryKey))
                throw new Exception("There is no primary key specified");

            FindByPrimaryKey<LightDataTableRow>(rowToBeMerged.ValueAndConvert<string>(_tablePrimaryKey)).Merge(rowToBeMerged);
        }


        /// <summary>
        /// T can only be Bool or LightDataRow.
        /// bool for is found or lightDatarow for data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKeyValue"></param>
        /// <returns></returns>
        public T FindByPrimaryKey<T>(object primaryKeyValue)
        {
            object o = null;
            if (typeof(T) == typeof(bool))
                return (T)(o = PrimaryKeyIndex.ContainValue(primaryKeyValue));

            if (PrimaryKeyIndex.ContainValue(primaryKeyValue))
                return (T)(o = PrimaryKeyIndex[primaryKeyValue]);
            return default(T);
        }

        /// <summary>
        /// Add Column to LightDataTable, its alot faster to add column before adding any rows.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="dataType"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public ILightDataTable AddColumn(string columnName, Type dataType = null, object defaultValue = null)
        {
            ColumnLength = Columns.Count + 1;
            var col = new LightDataTableColumn(columnName, columnName, dataType ?? typeof(string), defaultValue, ColumnLength - 1);
            Columns.Add(columnName, col);
            ColumnsWithIndexKey.Add(col.ColumnIndex, col);
            if (_rows.Count <= 0) return this;
            foreach (var row in _rows)
                row.AddValue(col, col.DefaultValue, Columns, ColumnsWithIndexKey);
            return this;
        }


        /// <summary>
        /// Add Column to LightDataTable, its alot faster to add column before adding any rows.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="displayName"></param>
        /// <param name="dataType"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public ILightDataTable AddColumn(string columnName, string displayName, Type dataType = null, object defaultValue = null)
        {
            ColumnLength = Columns.Count + 1;
            var col = new LightDataTableColumn(columnName, displayName, dataType ?? typeof(string), defaultValue, ColumnLength - 1);
            Columns.Add(columnName, col);
            ColumnsWithIndexKey.Add(col.ColumnIndex, col);
            if (_rows.Count <= 0) return this;
            foreach (var row in _rows)
                row.AddValue(col, col.DefaultValue, Columns, ColumnsWithIndexKey);
            return this;
        }

        /// <summary>
        /// Add new row to lightDataTable
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public LightDataTableRow AddRow(object[] values)
        {
            var row = NewRow();
            row.ItemArray = values;
            if (!string.IsNullOrEmpty(_tablePrimaryKey) && Columns.ContainsKey(_tablePrimaryKey))
                PrimaryKeyIndex.AddValue(null, row[_tablePrimaryKey], row);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Add new Row to LightDataTable
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public LightDataTableRow AddRow(LightDataTableRow row)
        {
            if (!string.IsNullOrEmpty(_tablePrimaryKey) && Columns.ContainsKey(_tablePrimaryKey))
                PrimaryKeyIndex.AddValue(null, row[_tablePrimaryKey], row);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Find by T. This method will be able to return bool, LightDataRow, List<LightDataRow>,LightDataRowCollection or Json string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public T SelectMany<T>(Predicate<LightDataTableRow> func)
        {
            object o = new object();

            if (typeof(T) == typeof(bool))
                return (T)(o = _rows.Exists(func));
            if (typeof(T) == typeof(LightDataTableRow))
            {
                return (T)(o = _rows.Find(func));

            }
            if (typeof(T) == typeof(List<LightDataTableRow>) || typeof(T) == typeof(LightDataRowCollection))
            {
                o = typeof(T) == typeof(List<LightDataTableRow>) ? _rows.FindAll(func) : new LightDataRowCollection(_rows.FindAll(func));
            }

            if (typeof(T) == typeof(LightDataTable))
            {
                var res = _rows.FindAll(func);

                o = res.Any() ? new LightDataTable(new LightDataRowCollection(res)) { TablePrimaryKey = this._tablePrimaryKey, TableName = this.TableName, ColumnLength = ColumnLength } : new LightDataTable() { TablePrimaryKey = this._tablePrimaryKey, TableName = this.TableName, Columns = Columns, ColumnsWithIndexKey = ColumnsWithIndexKey, ColumnLength = ColumnLength };
            }
            return (T)o;
        }


        /// <summary>
        /// Convert ObjectList To LightDataTable
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="execludeClasses"></param>
        /// <param name="overdedDataType"></param>
        /// <param name="ignoreTypeValidation"></param>
        public LightDataTable(object obj, bool execludeClasses = false, Type overdedDataType = null, bool ignoreTypeValidation = false, bool structureOnly = false) : base()
        {
            this.IgnoreTypeValidation = ignoreTypeValidation;
            if (obj == null)
                return;
            if (obj is LightDataRowCollection)
            {
                _rows = obj as LightDataRowCollection;
                Culture = _rows.First().Culture;
                base.ColumnLength = _rows.First().ColumnLength;
                base.Columns = _rows.First().Columns;
                base.ColumnsWithIndexKey = _rows.First().ColumnsWithIndexKey;
                base.RoundingSettings = _rows.First().RoundingSettings;
            }
            else if (obj is IList)
            {
                var oList = obj as IList;
                var T = oList.GetType().GetActualType().CreateInstance();
                foreach (var item in FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(T.GetType()))
                    if (!execludeClasses || item.IsInternalType)
                        AddColumn(item.Name, item.PropertyType, null);
                if (!structureOnly)
                    foreach (var o in oList)
                    {
                        AddRow(NewRow().MergeUnKnownObject(o));
                    }
            }
            else if (obj is DataTable)
            {
                var tb = obj as DataTable;
                if (tb.PrimaryKey?.Any() ?? false)
                    TablePrimaryKey = tb.PrimaryKey[0].ColumnName;
                TableName = tb.TableName;

                foreach (DataColumn col in tb.Columns)
                    AddColumn(col.ColumnName, overdedDataType ?? col.DataType, col.DefaultValue);

                if (!structureOnly)
                    foreach (DataRow row in tb.Rows)
                        AddRow(NewRow(row.ItemArray));

            }
            else if ((obj as IDictionary<string, object>) != null)
            {
                var dictionary = (IDictionary<string, object>)obj;
                foreach (string key in dictionary.Keys)
                    AddColumn(key, dictionary[key].GetType(), null);

                if (!structureOnly)
                    AddRow(NewRow().MergeUnKnownObject(obj));
            }
            else
            {
                foreach (var item in FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(obj.GetType()))
                    if (!execludeClasses || item.IsInternalType)
                    {
                        if (item.ContainAttribute<PrimaryKey>() && string.IsNullOrEmpty(TablePrimaryKey))
                            TablePrimaryKey = item.Name;
                        AddColumn(item.Name, item.PropertyType, null);

                    }
                if (!structureOnly)
                    AddRow(NewRow().MergeUnKnownObject(obj));
            }
        }

        public LightDataTable() : base()
        {
        }
    }
}
