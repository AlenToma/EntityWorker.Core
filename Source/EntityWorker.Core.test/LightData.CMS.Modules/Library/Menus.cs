using System.Collections.Generic;
using EntityWorker.Core.Attributes;

namespace LightData.CMS.Modules.Library
{
    public class Menus
    {
        [PrimaryKey(autoGenerate: false)]
        public long? Id { get; set; }

        [NotNullable]

        public string DisplayName { get; set; }

        [ForeignKey(typeof(Menus))]
        public long? ParentId { get; set; }


        /// <summary>
        /// Indicate 
        /// this is a list so the where sats will be 
        /// Select * from Menus where ParentId = Id
        /// the parentId in this list will be set to Id automaticly and this list will be children to the current Menus
        /// </summary>
        public List<Menus> Children { get; set; }

        [NotNullable]
        public string Uri { get; set; }

        [ForeignKey(typeof(FileItem))]
        public System.Guid? IconId { get; set; }

        public FileItem Icon { get; set; }

        public bool Publish { get; set; }

        public string Description { get; set; }

        [ForeignKey(typeof(User))]
        public string CreatedBy { get; set; }

        public User CreatedByUser { get; set; }
    }
}
