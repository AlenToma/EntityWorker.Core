using EntityWorker.Core.Attributes;

namespace EntityWorker.Core.Object.Library.DataBase
{
    public class ColumnSchema
    {
        [PropertyName("db")]
        public string Database { get; set; }

        [PropertyName("tb")]
        public string Table { get; set; }

        [PropertyName("columnname")]
        public string ColumnName { get; set; }

        [PropertyName("datatype")]
        public string DataType { get; set; }

        [PropertyName("isnullable")]
        public string IsNullable { get; set; }

    }
}
