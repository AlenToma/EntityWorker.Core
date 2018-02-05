using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;

namespace LightData.CMS.Modules.Library
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey(autoGenerate: false)]
        public string Id { get; set; }

        
        [NotNullable]
        [DataEncode]
        public string UserName { get; set; }

        [NotNullable]
        [DataEncode]
        public string Password { get; set; }

        [ForeignKey(typeof(Role))]
        public System.Guid RoleId { get; set; }

        [IndependentData]
        public Role Role { get; set; }

        [ForeignKey(typeof(Person))]
        public System.Guid PersonId { get; set; }

        public Person Person { get; set; }

    }
}
