using EntityWorker.Core.Attributes;
using System.Collections.Generic;
using static LightData.CMS.Modules.Helper.EnumHelper;

namespace LightData.CMS.Modules.Library
{
    [Table("Folders")]
    public class Folder 
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(typeof(Folder))]
        public System.Guid? Parent_Id { get; set; }

        public List<Folder> Children { get; set; }

        public bool IsSystem { get; set; }

        [Stringify]
        public FolderTypes FolderType { get; set; }

        public List<FileItem> Files { get; set; }

        public bool IsActive { get; set; }
    }
}
