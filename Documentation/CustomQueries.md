## Create Custom ISqlQueryable
```csharp
using EntityWorker.Core.Helper;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Transaction;
using Test.Modules.Library;
using System.Collections.Generic;
using System.Linq;

namespace Test.Modules.Repository
{
    public class DbContext : Transaction, IRepository
    {
        public DbContext(DataBaseTypes dbType = DataBaseTypes.Mssql) : base(GetConnectionString(dbType), true, dbType)
        {

        }

        protected override void OnModuleStart()
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
            return dbType == DataBaseTypes.Mssql ? @"Server=.\SQLEXPRESS; Database=IRent; User Id=root; Password=root;" :
                  (dbType == DataBaseTypes.Sqllight ? @"Data Source=D:\Projects\LightData.CMS\source\LightData.CMS\App_Data\LightDataTabletest.db" :
                  "Host=localhost;Username=postgres;Password=root;Database=mydatabase");
        }

        /// Create a custom ISqlQueryable, you could have store proc or a row sql query
        public List<User> GetUserByUserNameName(string userName)
        {
            var cmd = GetSqlCommand("SELECT * FROM User WHERE UserName = @userName");
            AddInnerParameter(cmd, "userName", userName, System.Data.SqlDbType.NVarChar);
            return DataReaderConverter<User>(cmd).LoadChildren().Execute(); /// convert the result to Data

        }
    }
}
```
