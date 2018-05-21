using EntityWorker.Core.Helper;
using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Assign a diffrent database type fot ht property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ColumnType : Attribute
    {
        public DataBaseTypes? DataBaseTypes { get; private set; }

        public string DataType { get; private set; }

        /// <summary>
        /// Assign a diffrent database type for the property
        /// Attibutes Stringify, DataEncode, JsonDocument and ToBase64String will override this attribute. 
        /// </summary>
        /// <param name="dataType">The database type ex nvarchar(4000)</param>
        /// <param name="dataBaseTypes">null for all providers</param>
        public ColumnType(string dataType, DataBaseTypes? dataBaseTypes = null)
        {
            try
            {
                if (string.Equals(dataType, "text", StringComparison.CurrentCulture) && (!dataBaseTypes.HasValue || dataBaseTypes == Helper.DataBaseTypes.Mssql))
                    throw new Exception("EntityWorker.Core cant handle text as a columnDataType for mssql, Microssoft will remove this DataType in the future so avoid using it.\n https://docs.microsoft.com/en-us/sql/t-sql/data-types/ntext-text-and-image-transact-sql");
                DataBaseTypes = dataBaseTypes;
                DataType = dataType;
            }
            catch (Exception e)
            {
                GlobalConfiguration.Log?.Error(e);
                throw;
            }
        }

    }
}
