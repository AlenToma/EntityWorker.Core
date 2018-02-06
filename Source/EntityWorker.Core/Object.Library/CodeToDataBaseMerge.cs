using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using System;
using System.Text;

namespace EntityWorker.Core.Object.Library
{
    public class CodeToDataBaseMerge
    {
        public Type Object_Type { get; internal set; }

        public StringBuilder Sql { get; internal set; } = new StringBuilder();

        public bool Executed { get; internal set; }

        public Exception Exception { get; internal set; }

        internal int Counter { get; set; }

        /// <summary>
        /// If sql statement containe drop operations, which will loose data
        /// </summary>
        public bool DataLoss { get; set; }

        public DataBaseTypes DataBaseTypes { get; set; }



    }
}
