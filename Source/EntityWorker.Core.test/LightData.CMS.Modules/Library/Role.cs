using EntityWorker.Core.Attributes;
using EntityWorker.Core.Object.Library;
using LightData.CMS.Modules.Helper;

namespace LightData.CMS.Modules.Library
{
    [Table("Roles")]
   public class Role
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        [NotNullable]
        public string Name { get; set; }

        [StringFy]
        public EnumHelper.RoleDefinition RoleDefinition { get; set; }
    }
}
