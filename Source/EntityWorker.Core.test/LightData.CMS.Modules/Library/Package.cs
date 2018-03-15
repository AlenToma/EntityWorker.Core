using System.Collections.Generic;

namespace LightData.CMS.Modules.Library
{
    public class Package : EntityWorker.Core.Object.Library.PackageEntity
    {
        public override List<object> Data { get; set; }
        public override List<byte[]> Files { get; set; }
    }
}
