using EntityWorker.Core.Attributes;
using System;

namespace LightData.CMS.Modules.Library
{
   public class ArticleTemp
    {
        [PrimaryKey]
        public System.Guid? Id { get; set; }

        [ForeignKey(typeof(Article))]
        public System.Guid ArticleId { get; set; }

        public Article ParentArticle { get; set; }

    }
}
