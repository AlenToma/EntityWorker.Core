using System.Collections.Generic;
using EntityWorker.Core.Attributes;

namespace LightData.CMS.Modules.Library
{
    public class Country
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Name { get; set; }

        public string CountryCode { get; set; }

        public List<Cities> Cities { get; set; }

        public bool Visible { get; set; }
    }
}
