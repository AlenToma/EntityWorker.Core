using System;
using EntityWorker.Core.Attributes;

namespace EntityWorker.Core.Object.Library
{
    [Table("Generic_LightDataTable_DBMigration")]
    internal class DBMigration : DbEntity
    {
        public string Name { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
