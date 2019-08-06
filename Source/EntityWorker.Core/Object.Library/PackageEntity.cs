using System.Collections.Generic;

namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// EntityWorker.Core Package handler
    /// create a file that can be executed by entityworker.core only.
    /// this is so you could transfer files and data from one locaion to another location or you could create a backup file 
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md
    /// </summary>
    public abstract class PackageEntity
    {
        /// <summary>
        /// Included items in package
        /// </summary>
        public abstract List<object> Data { get; set; }
    }
}
