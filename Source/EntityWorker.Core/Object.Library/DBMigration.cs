using System;
using EntityWorker.Core.Attributes;

namespace EntityWorker.Core.Object.Library
{
    [Table("Generic_LightDataTable_DBMigration")]
    internal class DBMigration
    {
        [PrimaryKey]
        internal int? Id { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
