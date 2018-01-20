using System;
using EntityWorker.Core.Attributes;

namespace DataAccess.EntityWorker.Entities.Archive
{
    [Table(nameof(GroupTag))]
    public class GroupTag : EntityBase
    {
        private DateTime? _played;

        [ForeignKey(typeof(GroupBase))]
        public int GroupId { get; set; }
        public GroupBase Group { get; set; }

        [ForeignKey(typeof(Tag))]
        public int TagId { get; set; }
        public Tag Tag { get; set; }
        public int OrderIndex { get; set; }

        public DateTime? Played
        {
            get { return _played; }
            set { _played = value >= System.Data.SqlTypes.SqlDateTime.MinValue.Value ? value : null; }
        }
    }
}