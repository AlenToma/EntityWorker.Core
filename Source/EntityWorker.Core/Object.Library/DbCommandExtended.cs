using EntityWorker.Core.Helper;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace EntityWorker.Core.Object.Library
{
    public class DbCommandExtended
    {
        private DbCommand _cmd;

        /// <summary>
        /// Set This for faster reading of sql
        /// </summary>
        public Type TableType { get; internal set; }

        public DbCommandExtended(DbCommand cmd, Type type = null)
        {
            _cmd = cmd;
            TableType = type?.GetActualType();
        }


        public DbCommand Command { get => _cmd; }
    }
}
