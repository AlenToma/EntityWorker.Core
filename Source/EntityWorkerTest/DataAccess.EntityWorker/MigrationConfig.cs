using System.Collections.Generic;
using DataAccess.EntityWorker.Migrations;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;

namespace DataAccess.EntityWorker
{
    public class MigrationConfig : IMigrationConfig
    {
        public IList<Migration> GetMigrations(IRepository repository)
        {
            return new List<Migration>()
            {
                new MigrationIni(),
            };
        }
    }
}
