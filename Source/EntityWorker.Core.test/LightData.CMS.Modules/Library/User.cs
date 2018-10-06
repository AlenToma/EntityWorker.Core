using EntityWorker.Core.Attributes;
using LightData.CMS.Modules.Interface;
using System;

namespace LightData.CMS.Modules.Library
{
    [Table("Users", "geto")]
    public class User
    {
        public User(string txt)
        {

        }
        public Guid? Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        //public string test { get; set; }

        public System.Guid RoleId { get; set; }

        public Role Role { get; set; }

        public System.Guid PersonId { get; set; }

        public IPerson Person { get; set; }

        public bool IsActive { get; set; }

    }
}
