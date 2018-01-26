using EntityWorker.Core.Object.Library;
using System.Collections.Generic;
using EntityWorker.Core.Attributes;

namespace LightData.CMS.Modules.Library
{
    public class SiteSettingCollection
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Name { get; set; }

        public List<SiteSetting> SiteSettings { get; set; }
    }
}
