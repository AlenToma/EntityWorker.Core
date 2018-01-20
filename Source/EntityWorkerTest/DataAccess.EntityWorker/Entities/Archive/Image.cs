using EntityWorker.Core.Attributes;
using System;

namespace DataAccess.EntityWorker.Entities.Archive
{
    [Table(nameof(Image))]
    public class Image : EntityBase
    {

        public byte[] Data { get; set; }
    }
}