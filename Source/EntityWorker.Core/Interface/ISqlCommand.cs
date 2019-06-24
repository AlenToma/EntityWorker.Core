using EntityWorker.Core.Helper;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace EntityWorker.Core.Interface
{
    public interface ISqlCommand
    {
        DataBaseTypes DataBaseTypes { get; }

        /// <summary>
        /// DbCommand
        /// </summary>
        DbCommand Command { get; }

        /// <summary>
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        ISqlCommand AddInnerParameter(string attrName, object value, DbType? dbType = null);

        /// <summary>
        /// Convert SqlCommand to List of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ISqlQueryable<T> DataReaderConverter<T>();

        /// <summary>
        /// Convert SqlCommand to list of System.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IList DataReaderConverter(Type type);

        /// <summary>
        /// Get LightDataTable by SqlCommand
        /// </summary>
        /// <param name="primaryKey"> set the primary key for faster browsing the datatable </param>
        /// <returns></returns>

        ILightDataTable GetLightDataTable(string primaryKey = null);

        /// <summary>
        /// ExecuteScale with return value
        /// </summary>
        /// <returns></returns>

        object ExecuteScalar();

        /// <summary>
        /// Execute Quary
        /// </summary>
        /// <returns></returns>

        int ExecuteNonQuery();
    }
}
