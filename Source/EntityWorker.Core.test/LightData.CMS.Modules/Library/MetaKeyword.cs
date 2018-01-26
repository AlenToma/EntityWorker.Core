using EntityWorker.Core.Attributes;
using EntityWorker.Core.Object.Library;

namespace LightData.CMS.Modules.Library
{
    [Table("MetaKeywords")]
    public class MetaKeyword
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public string Key { get; set; }

        public string Description { get; set; }

        [ForeignKey(typeof(Article))]
        public System.Guid ArticleId { get; set; }

    }
}
