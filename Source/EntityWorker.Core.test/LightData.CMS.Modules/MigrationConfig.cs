using System.Collections.Generic;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using LightData.CMS.Modules.Migrations;

namespace LightData.CMS.Modules
{
    public class MigrationConfig : IMigrationConfig
    {
        public IList<Migration> GetMigrations(IRepository repository)
        {
            return new List<Migration>()
            {
                new MigrationIni(),
                new MigrationStartUp()
            };
        }
    }
}
