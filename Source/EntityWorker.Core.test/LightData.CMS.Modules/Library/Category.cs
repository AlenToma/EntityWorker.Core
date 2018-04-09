using EntityWorker.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightData.CMS.Modules.Library
{
   public class Category
    {
        [PrimaryKey]
        public Guid? Id { get; set; }

        [ForeignKey(typeof(Category))]
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Indicate 
        /// this is a list so the where sats will be 
        /// Select * from Category where ParentId = Id
        /// </summary>
        public List<Category> Chlidren { get; set; }
    }
}
