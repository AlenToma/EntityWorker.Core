using System.Collections.Generic;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using LightData.CMS.Modules.Interface;

namespace LightData.CMS.Modules.Library
{
    public class Person : IPerson 
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string SureName { get; set; }

        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}
