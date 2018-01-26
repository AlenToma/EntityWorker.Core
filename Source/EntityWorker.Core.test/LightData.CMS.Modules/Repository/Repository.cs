using EntityWorker.Core.Transaction;
using EntityWorker.Core.Helper;
using System.Linq;

namespace LightData.CMS.Modules.Repository
{
    public class Repository : Transaction
    {
        public Repository(DataBaseTypes dbType = DataBaseTypes.Mssql) : base(GetConnectionString(dbType), true, dbType)
        {
            if (!base.DataBaseExist())
                base.CreateDataBase();

            /// Limited support for sqlite
            // Get the latest change between the code and the database. 
            // Property Rename is not supported. renaming property x will end up removing the x and adding y so there will be dataloss
            // Adding a primary key is not supported either
            var latestChanges = GetCodeLatestChanges();
            if (latestChanges.Any())
                latestChanges.Execute(true);
        }

        // Get the full connection string from the web-config
        public static string GetConnectionString(DataBaseTypes dbType)
        {
            return dbType == DataBaseTypes.Mssql ? @"Server=.\SQLEXPRESS; Database=CMStest; User Id=root; Password=root;" :
                  (dbType == DataBaseTypes.Sqllight ? @"Data Source=D:\Projects\LightData.CMS\source\LightData.CMS\App_Data\LightDataTabletest.db" :
                  "Host=localhost;Username=postgres;Password=root;Database=mydatabase");
        }
    }
}
