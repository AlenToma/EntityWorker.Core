using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.Object.Library;

namespace EntityWorker.Core.InterFace
{
    public interface IRepository : IDisposable
    {
        DataBaseTypes DataBaseTypes { get; }

        ILightDataTable GetLightDataTable(DbCommand cmd, string primaryKey = null);

        DbCommand GetSqlCommand(string sql);

        void AddInnerParameter(DbCommand cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar);

        SqlDbType GetSqlType(Type type);

        object ExecuteScalar(DbCommand cmd);

        int ExecuteNonQuery(DbCommand cmd);

        DbTransaction CreateTransaction();

        void Rollback();

        void SaveChanges();

        void Delete(IDbEntity entity);

        Task DeleteAsync(IDbEntity entity);

        long Save(IDbEntity entity);

        Task<long> SaveAsync(IDbEntity entity);

        void Attach(DbEntity objcDbEntity);

        ISqlQueryable<T> Get<T>() where T : class, IDbEntity;

        IList<T> GetAll<T>() where T : class, IDbEntity;

        Task<IList<T>> GetAllAsync<T>() where T : class, IDbEntity;

        /// <summary>
        /// load children 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="repository"></param>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>
        void LoadChildren<T>(T item, bool onlyFirstLevel = false, List<string> actions = null, List<string> ignoreList = null) where T : class, IDbEntity;


        Task LoadChildrenAsync<T>(T item, bool onlyFirstLevel, List<string> classes, List<string> ignoreList) where T : class, IDbEntity;

        Task LoadChildrenAsync<T, TP>(T item, bool onlyFirstLevel = false, List<string> ignoreList = null, params Expression<Func<T, TP>>[] actions) where T : class, IDbEntity;

        /// <summary>
        /// load children 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="repository"></param>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>
        void LoadChildren<T, TP>(T item, bool onlyFirstLevel = false, List<string> ignoreList = null, params Expression<Func<T, TP>>[] actions) where T : class, IDbEntity;

        IList<T> Select<T>(string sqlString) where T : class, IDbEntity;

        Task<IList<T>> SelectAsync<T>(string sqlString) where T : class, IDbEntity;


        Dictionary<string, object> GetObjectChanges(DbEntity entity);

        bool IsAttached(DbEntity entity);


        void CreateTable<T>(bool force = false) where T : class, IDbEntity;

        void CreateTable(Type type, bool force = false);

        void RemoveTable<T>() where T : class, IDbEntity;

        void RemoveTable(Type type);

    }
}
