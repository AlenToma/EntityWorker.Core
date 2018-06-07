using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.Transaction;

namespace DataAccess.EntityWorker
{
    public class Repository : Transaction
    {
        public Repository(DataBaseTypes dbType = DataBaseTypes.Mssql) : base(GetConnectionString(dbType), dbType)
        {
     
        }

        // get the full connection string
        public static string GetConnectionString(DataBaseTypes dbType)
        {
            return dbType == DataBaseTypes.Mssql ? @"Server=.\SQLEXPRESS; Database=testDB; User Id=root; Password=root;" : @"Data Source=LightDataTable.db";
        }

        protected override void OnModuleConfiguration(IModuleBuilder moduleBuilder)
        {
       
        }

        protected override void OnModuleStart()
        {
            if (!base.DataBaseExist())
                base.CreateDataBase(); /// Create the database if it dose not exist.

            var code = GetCodeLatestChanges();
            if (code.Any())
                code.Execute();

            InitializeMigration();
        }
    }
}
