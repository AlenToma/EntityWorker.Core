using System;

namespace DataAccess.EntityWorker.Entities.Archive
{
    public class PlaylistGroup : GroupBase
    {
        protected PlaylistGroup()
        {
        }

        public PlaylistGroup(string name)
            : base(name)
        {
            Created = DateTime.UtcNow;
        }

        public DateTime? Created { get; set; }
    }
}