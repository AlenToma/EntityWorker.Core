using EntityWorker.Core.Object.Library;
using EntityWorker.Core.Attributes;

namespace LightData.CMS.Modules.Library
{
    public class SiteSetting 
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        [StringFy]
        public Helper.EnumHelper.Keys Key { get; set; }

        [ForeignKey(typeof(SiteSettingCollection))]
        public System.Guid SiteSettingCollection_Id { get; set; }
    }
}
