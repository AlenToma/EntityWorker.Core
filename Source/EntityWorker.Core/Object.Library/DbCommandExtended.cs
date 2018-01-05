using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using System;
using System.Data.Common;

namespace EntityWorker.Core.Object.Library
{
    public class DbCommandExtended
    {
        private DbCommand _cmd;

        /// <summary>
        /// This is initialized after TableType
        /// </summary>
        public ILightDataTable DataStructure { get; private set; }
        /// <summary>
        /// Set This for faster reading of sql
        /// </summary>
        public Type TableType { get; private set; }

        public DbCommandExtended(DbCommand cmd, Type tableType = null)
        {
            _cmd = cmd;
            TableType = tableType?.GetActualType();
            if (TableType != null)
                DataStructure = new LightDataTable(TableType.CreateInstance(), true, null, false, true);
            else DataStructure = new LightDataTable();
        }


        public DbCommand Command { get => _cmd; }
    }
}
