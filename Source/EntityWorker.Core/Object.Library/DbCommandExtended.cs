using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EntityWorker.Core.Object.Library
{
    public class DbCommandExtended
    {
        private DbCommand _cmd;
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="provider"></param>
        /// <param name="tableType"></param>
        public DbCommandExtended(DbCommand cmd, IRepository provider)
        {
            Provider = provider;
            _cmd = cmd;
        }

        public IRepository Provider { get; set; }

        public DbCommand Command { get => _cmd; }

        public override string ToString()
        {
            return _cmd.CommandText;
        }
    }
}
