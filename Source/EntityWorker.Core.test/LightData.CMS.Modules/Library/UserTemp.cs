using EntityWorker.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightData.CMS.Modules.Library
{
    [Table("UserJsonTest")]
    public class UserTemp
    {
        [PrimaryKey]
        public Guid? Id { get; set; }

        [JsonDocument]
        [PropertyName("UserJson")]
        public User User { get; set; }
    }
}
