using EntityWorker.Core.Attributes;
using System;

namespace LightData.CMS.Modules.Library
{
    public class ArticleNode
    {

        [PrimaryKey]
        public System.Guid? Id { get; set; }

        public long ArticleId { get; set; }

        public string PageHeader { get; set; }

        [ToBase64String]
        public string Content { get; set; }

        public string Tags { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [ForeignKey(typeof(Country))]
        public Guid LanguageId { get; set; }

        public Country Language { get; set; }
    }
}
