using System.Collections.Generic;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;

namespace LightData.CMS.Modules.Library
{
    public class Person
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SureName { get; set; }

        public List<Address> Addresses { get; set; }
    }
}
