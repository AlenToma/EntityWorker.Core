using System;
using System.Globalization;
using System.Linq.Expressions;

namespace EntityWorker.Core.Interface
{
    public interface ILightDataTable
    {
        /// <summary>
        /// Table Primary Key Name 
        /// to use FindByPrimaryKey
        /// </summary>
        string TablePrimaryKey { get; set; }

        /// <summary>
        /// Total Pages
        /// </summary>
        int TotalPages { get; }
        /// <summary>
        /// Columns string
        /// </summary>
        ColumnsCollections<string> Columns { get; }
        /// <summary>
        /// Column with int
        /// </summary>
        ColumnsCollections<int> ColumnsWithIndexKey { get; }
        /// <summary>
        /// Add column to lightDataTable
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="displayName"></param>
        /// <param name="dataType"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        ILightDataTable AddColumn(string columnName, string displayName, Type dataType = null, object defaultValue = null);
        /// <summary>
        /// Add column to lightDataTable
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="dataType"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        ILightDataTable AddColumn(string columnName, Type dataType = null, object defaultValue = null);
        /// <summary>
        /// Generic find by primary Key Value.
        /// T can be bool or LightDataTableRow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKeyValue"></param>
        /// <returns></returns>
        T FindByPrimaryKey<T>(object primaryKeyValue);
        /// <summary>
        /// Add row
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        LightDataTableRow AddRow(object[] values);
        /// <summary>
        /// Add Row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        LightDataTableRow AddRow(LightDataTableRow row);
        /// <summary>
        /// Remove column by expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ILightDataTable RemoveColumn<T, TP>(Expression<Func<T, TP>> action) where T : class;
        /// <summary>
        /// merge two rows, will merge both column and values
        /// </summary>
        /// <param name="rowToBeMerged"></param>
        void MergeByPrimaryKey(LightDataTableRow rowToBeMerged);
        /// <summary>
        /// return new Row
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        LightDataTableRow NewRow(CultureInfo cultureInfo = null);
        /// <summary>
        /// Remove column by string
        /// </summary>
        /// <param name="columnName"></param>
        void RemoveColumn(string columnName);
        /// <summary>
        /// remove column by its index
        /// </summary>
        /// <param name="columnIndex"></param>
        void RemoveColumn(int columnIndex);
        /// <summary>
        /// change value of the column
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        ILightDataTable AssignValueToColumn(string columnName, object value);
        /// <summary>
        /// merge Column, will add if not exist
        /// </summary>
        /// <param name="data2"></param>
        /// <returns></returns>
        ILightDataTable MergeColumns(LightDataTable data2);
        /// <summary>
        /// Merge LightDataTable 
        /// </summary>
        /// <param name="data2"></param>
        /// <param name="mergeColumnAndRow"></param>
        /// <returns></returns>
        ILightDataTable Merge(LightDataTable data2, bool mergeColumnAndRow = true);
        /// <summary>
        /// Reorder rows by expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="action"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        ILightDataTable OrderBy<T, TP>(Expression<Func<T, TP>> action, bool position) where T : class;
        /// <summary>
        /// Reorder by string
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="position">desc or asc</param>
        /// <returns></returns>
        ILightDataTable OrderBy(string columnName, bool position);
        /// <summary>
        /// Rows
        /// </summary>
        LightDataRowCollection Rows { get; }

        /// <summary>
        /// T could be bool For Any
        /// T could be LightDataTableRow for the first found Item
        /// T could be List of LightDataTableRow found items
        /// T could be LightDataTable a Table result 
        /// T could also be LightDataRowCollection of found items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        T SelectMany<T>(Predicate<LightDataTableRow> func);
    }
}
