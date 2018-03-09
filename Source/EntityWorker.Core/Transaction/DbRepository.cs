using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;

namespace EntityWorker.Core.Transaction
{
    internal class DbRepository : Transaction
    {

        public DbRepository(string connection, DataBaseTypes dbType): base(connection, dbType)
        {

        }

        protected override void OnModuleConfiguration(IModuleBuilder moduleBuilder)
        {
        }

        protected override void OnModuleStart()
        {
            
        }
    }
}
