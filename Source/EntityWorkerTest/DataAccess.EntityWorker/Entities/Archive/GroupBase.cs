using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities.Archive
{
    [Table(nameof(GroupBase))]
    public abstract class GroupBase : EntityBase
    {
        protected GroupBase()
        {
        }

        protected GroupBase(string name)
            : this()
        {
            Name = name;
        }

        [NotNullable]
        public string Name { get; set; }
    }
}