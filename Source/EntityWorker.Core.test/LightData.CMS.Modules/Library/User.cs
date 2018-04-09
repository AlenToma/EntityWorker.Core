using EntityWorker.Core.Attributes;
using LightData.CMS.Modules.Interface;

namespace LightData.CMS.Modules.Library
{
    public class User
    {
        public User(string txt)
        {

        }
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public System.Guid RoleId { get; set; }

        public Role Role { get; set; }

        public System.Guid PersonId { get; set; }

        public IPerson Person { get; set; }

    }
}
