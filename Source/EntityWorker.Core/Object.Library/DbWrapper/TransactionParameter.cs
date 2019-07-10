using EntityWorker.Core.Helper;
using FastDeepCloner;
using System.Data;
using System.Data.Common;

namespace EntityWorker.Core.Object.Library.DbWrapper
{
    public sealed class TransactionParameter
    {
        private static SafeValueType<DataBaseTypes, IFastDeepClonerProperty> dbTypeProperty = new SafeValueType<DataBaseTypes, IFastDeepClonerProperty>();

        public DbType DbType { get => (DbType)dbTypeProperty[DataBaseTypes].GetValue(Parameter); set => dbTypeProperty[DataBaseTypes].SetValue(Parameter, value); }

        public DataBaseTypes DataBaseTypes { get; private set; }

        public readonly DbParameter Parameter;
        public TransactionParameter(DataBaseTypes dataBaseTypes, string attrName, object value, DbType? dbType = null)
        {
            DataBaseTypes = dataBaseTypes;
            switch (dataBaseTypes)
            {
                case DataBaseTypes.Mssql:
                    Parameter = "System.Data.SqlClient.SqlParameter".GetObjectType("System.Data.SqlClient").CreateInstance(new[] { attrName, value }) as DbParameter;
                    break;
                case DataBaseTypes.PostgreSql:
                    Parameter = "Npgsql.NpgsqlParameter".GetObjectType("Npgsql").CreateInstance(new[] { attrName, value }) as DbParameter;
                    break;
                case DataBaseTypes.Sqllight:
                    Parameter = "System.Data.SQLite.SQLiteParameter".GetObjectType("System.Data.SQLite").CreateInstance(new[] { attrName, value }) as DbParameter;
                    break;
            }

            if (!dbTypeProperty.ContainsKey(dataBaseTypes))
                dbTypeProperty.TryAdd(dataBaseTypes, DeepCloner.GetProperty(Parameter.GetType(), "DbType"));

            if (dbType.HasValue)
                DbType = dbType.Value;
        }
    }
}
