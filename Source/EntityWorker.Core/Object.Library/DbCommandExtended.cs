using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// EntityWorker.Core SqlCommand
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// </summary>
    public class DbCommandExtended : ISqlCommand
    {
        private DbCommand _cmd;

        public IRepository _provider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="provider"></param>
        /// <param name="tableType"></param>
        internal DbCommandExtended(DbCommand cmd, IRepository provider)
        {
            _provider = provider;
            _cmd = cmd;
        }

        /// <summary>
        /// DbCommand
        /// </summary>
        public DbCommand Command { get => _cmd; }

        /// <summary>
        /// Add Parameter to sqlCommand
        /// </summary>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>

        public ISqlCommand AddInnerParameter(string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar)
        {
            _provider.AddInnerParameter(this, attrName, value, dbType);
            return this;
        }

        /// <summary>
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        public ISqlCommand AddInnerParameter(string attrName, object value, DbType dbType)
        {
            _provider.AddInnerParameter(this, attrName, value, dbType);
            return this;
        }

        /// <summary>
        /// Convert SqlCommand to List of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISqlQueryable<T> DataReaderConverter<T>()
        {
            return _provider.DataReaderConverter<T>(this);
        }

        /// <summary>
        /// Convert SqlCommand to list of System.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList DataReaderConverter(Type type)
        {
            return _provider.DataReaderConverter(this, type);
        }

        /// <summary>
        /// Get LightDataTable by SqlCommand
        /// </summary>
        /// <param name="primaryKey"> set the primary key for faster browsing the datatable </param>
        /// <returns></returns>

        public ILightDataTable GetLightDataTable(string primaryKey = null)
        {
            return _provider.GetLightDataTable(this, primaryKey);
        }

        /// <summary>
        /// ExecuteScale with return value
        /// </summary>
        /// <returns></returns>

        public object ExecuteScalar()
        {
            return _provider.ExecuteScalar(this);
        }

        /// <summary>
        /// Execute Quary
        /// </summary>
        /// <returns></returns>
  
        public int ExecuteNonQuery()
        {
            return _provider.ExecuteNonQuery(this);
        }

        /// <summary>
        /// get the sql command
        /// </summary>
        /// <returns></returns>
 
        public override string ToString()
        {
            return _cmd.CommandText;
        }
    }
}
