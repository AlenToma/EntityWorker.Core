using EntityWorker.Core.Attributes;
using EntityWorker.Core.Object.Library;

namespace LightData.CMS.Modules.Library
{
    public class Cities 
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }
            
        [ForeignKey(typeof(Country))]
        public System.Guid CountryId { get; set; }
    }
}
