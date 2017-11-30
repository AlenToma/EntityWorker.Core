using System;
namespace EntityWorker.Core
{
    public class LightDataTableColumn
    {
        public string ColumnName { get; private set; }

        public Type DataType { get; private set; }

        public object DefaultValue { get; private set; }

        public int ColumnIndex { get; private set; }

        public string DisplayName { get; private set; }

        internal LightDataTableColumn(string columnName, string displayName, Type dataType, object defaultValue = null, int columnIndex = 0)
        {
            ColumnName = columnName;
            DataType = dataType ?? typeof(string);
            DefaultValue = defaultValue;
            ColumnIndex = columnIndex;
            DisplayName = displayName;
        }

    }
}
