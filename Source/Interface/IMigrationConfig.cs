using System.Collections.Generic;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;


namespace EntityWorker.Core.Interface
{
    public interface IMigrationConfig
    {
        /// <summary>
        /// All available Migrations to be executed
        /// </summary>
        IList<Migration> GetMigrations(IRepository repository);
    }
}
