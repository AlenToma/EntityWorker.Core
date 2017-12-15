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

        /// <summary>
        /// Clone Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        T Clone<T>(T o, FastDeepCloner.FieldType fieldType = FastDeepCloner.FieldType.PropertyInfo) where T : class;

        /// <summary>
        /// Database type
        /// </summary>
        DataBaseTypes DataBaseTypes { get; }

        /// <summary>
        /// Get LightDataTable by SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>

        ILightDataTable GetLightDataTable(DbCommand cmd, string primaryKey = null);

        /// <summary>
        /// Get SqlCommand by sql string
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DbCommand GetSqlCommand(string sql);

        /// <summary>
        /// Add Parameter to sqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>

        void AddInnerParameter(DbCommand cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar);

        /// <summary>
        /// Get SqlDbType By system Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        SqlDbType GetSqlType(Type type);

        /// <summary>
        /// ExecuteScale with return value
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>

        object ExecuteScalar(DbCommand cmd);

        /// <summary>
        /// Execute Quary
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>

        int ExecuteNonQuery(DbCommand cmd);

        /// <summary>
        /// Create Transaction
        /// </summary>
        /// <returns></returns>

        DbTransaction CreateTransaction();

        /// <summary>
        /// RoleBack trnsaction
        /// </summary>

        void Rollback();

        /// <summary>
        /// Commit Changes
        /// </summary>

        void SaveChanges();

        /// <summary>
        /// Delete entity.
        /// SaveChanges is needet after
        /// </summary>
        /// <param name="entity"></param>

        void Delete(IDbEntity entity);
        /// <summary>
        /// Delete entity.
        /// SaveChanges is needet after
        /// </summary>
        /// <param name="entity"></param>
        /// 
        Task DeleteAsync(IDbEntity entity);

        /// <summary>
        /// Save entity.
        /// SaveChanges is needet after
        /// </summary>
        /// <param name="entity"></param>

        long Save(IDbEntity entity);

        /// <summary>
        /// Save entity.
        /// SaveChanges is needet after
        /// </summary>
        /// <param name="entity"></param>

        Task<long> SaveAsync(IDbEntity entity);

        /// <summary>
        /// Attach an object to entityWorker.
        /// </summary>
        /// <param name="objcDbEntity"></param>

        void Attach(DbEntity objcDbEntity);

        /// <summary>
        /// Get Entity 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        ISqlQueryable<T> Get<T>() where T : class, IDbEntity;

        /// <summary>
        /// Get all 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

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
        Task LoadChildrenAsync<T>(T item, bool onlyFirstLevel, List<string> classes, List<string> ignoreList) where T : class, IDbEntity;

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

        /// <summary>
        /// Get IList by sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlString"></param>
        /// <returns></returns>

        IList<T> Select<T>(string sqlString) where T : class, IDbEntity;

        /// <summary>
        /// Get IList by sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlString"></param>
        /// <returns></returns>

        Task<IList<T>> SelectAsync<T>(string sqlString) where T : class, IDbEntity;

        /// <summary>
        /// Get EntityChanges. entity has to be attachet 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Dictionary<string, object> GetObjectChanges(DbEntity entity);

        /// <summary>
        /// If enetiy is attached
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>

        bool IsAttached(DbEntity entity);

        /// <summary>
        /// Create Table by System.Type
        /// all under tables will be created
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="force">force recreation if table eg if exist delete then create agen</param>

        void CreateTable<T>(bool force = false) where T : class, IDbEntity;


        /// <summary>
        /// Create Table by System.Type
        /// all under tables will be created
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="force">force recreation if table eg if exist delete then create agen</param>

        void CreateTable(Type type, bool force = false);

        /// <summary>
        /// Remove Table
        /// all under tables will also be removed
        /// </summary>
        /// <typeparam name="T"></typeparam>

        void RemoveTable<T>() where T : class, IDbEntity;

        /// <summary>
        /// Remove Table
        /// all under tables will also be removed
        /// </summary>
        /// <typeparam name="T"></typeparam>

        void RemoveTable(Type type);

    }
}
