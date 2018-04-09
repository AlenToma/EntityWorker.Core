using EntityWorker.Core.Attributes;
using LightData.CMS.Modules.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightData.CMS.Modules.Interface
{
    public interface IPerson
    {
        [PrimaryKey]
        Guid? Id { get; set; }

        string FirstName { get; set; }

        string LastName { get; set; }

        string SureName { get; set; }

        List<Address> Addresses { get; set; }
    }
}
