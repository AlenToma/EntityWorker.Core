using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using EntityWorker.Core.Object.Library;
using EntityWorker.Core.Object.Library.JSON;

namespace EntityWorker.Core.InterFace
{
    /// <summary>
    /// EntityWorker.Core Repository
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// </summary>
    public interface IRepository : IQueryProvider, IDisposable
    {
        /// <summary>
        /// Clone Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="level"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        T Clone<T>(T o, FastDeepCloner.CloneLevel level, FastDeepCloner.FieldType fieldType = FastDeepCloner.FieldType.PropertyInfo) where T : class;

        /// <summary>
        /// Database type
        /// </summary>
        DataBaseTypes DataBaseTypes { get; }

        /// <summary>
        /// Get LightDataTable by SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="primaryKey"> set the primary key for faster browsing the datatable </param>
        /// <returns></returns>

        ILightDataTable GetLightDataTable(ISqlCommand cmd, string primaryKey = null);

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type">Set for faster Converting of dbreader to object</param>
        /// <returns></returns>
        ISqlCommand GetSqlCommand(string sql);

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="type">Set for faster Converting of dbreader to object</param>
        /// <returns></returns>
        ISqlCommand GetStoredProcedure(string storedProcedure);

        /// <summary>
        /// Add Parameter to sqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>

        IRepository AddInnerParameter(ISqlCommand cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar);

        /// <summary>
        /// Add parameters to SqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        IRepository AddInnerParameter(ISqlCommand cmd, string attrName, object value, DbType dbType);

        /// <summary>
        /// Get SqlDbType By system Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        SqlDbType GetSqlType(Type type);


        /// <summary>
        /// Get DbType By System.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        DbType GetDbType(Type type);

        /// <summary>
        /// ExecuteScale with return value
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>

        object ExecuteScalar(ISqlCommand cmd);

        /// <summary>
        /// Execute Quary
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>

        int ExecuteNonQuery(ISqlCommand cmd);

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
        /// SaveChanges is needed after
        /// </summary>
        /// <param name="entity"></param>

        IRepository Delete(object entity);
        /// <summary>
        /// Delete entity.
        /// SaveChanges is needed after
        /// </summary>
        /// <param name="entity"></param>
       
        Task<IRepository> DeleteAsync(object entity);

        /// <summary>
        /// Save Entity 
        /// ignore/execlude updateing some properties to the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="ignoredProperties"></param>
        /// <returns></returns>
        IRepository Save<T>(T entity, params Expression<Func<T, object>>[] ignoredProperties);

        /// <summary>
        /// Save Entity 
        /// ignore/execlude updateing some properties to the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="ignoredProperties"></param>
        /// <returns></returns>
        Task<IRepository> SaveAsync<T>(T entity, params Expression<Func<T, object>>[] ignoredProperties);

        /// <summary>
        /// Get Entity 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ISqlQueryable<T> Get<T>();

        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param">The Default is GlobalConfigration.JSONParameters</param>
        /// <returns></returns>
        ISqlQueryable<T> FromJson<T>(string json, JSONParameters param = null) where T : class;

        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param">The Default is GlobalConfigration.JSONParameters</param>
        /// <returns></returns>
        Task<ISqlQueryable<T>> FromJsonAsync<T>(string json, JSONParameters param = null) where T : class;

        /// <summary>
        /// Get ISqlQueryable from Xml.
        /// All XmlIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ISqlQueryable<T> FromXml<T>(string xmlString) where T : class;

        /// <summary>
        /// Get ISqlQueryable from Xml.
        /// All XmlIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<ISqlQueryable<T>> FromXmlAsync<T>(string xmlString) where T : class;

        /// <summary>
        /// Get all 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IList<T> GetAll<T>();

        /// <summary>
        /// Get all 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IList<T>> GetAllAsync<T>();

        /// <summary>
        /// load children 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>
        void LoadChildren<T>(T item, bool onlyFirstLevel = false, List<string> actions = null, List<string> ignoreList = null);

        /// <summary>
        /// load children 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="classes"></param>
        /// <param name="ignoreList"></param>
        Task LoadChildrenAsync<T>(T item, bool onlyFirstLevel, List<string> classes, List<string> ignoreList);

        /// <summary>
        /// load children 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>

        Task LoadChildrenAsync<T, TP>(T item, bool onlyFirstLevel = false, List<string> ignoreList = null, params Expression<Func<T, TP>>[] actions);

        /// <summary>
        /// load children 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="item"></param>
        /// <param name="onlyFirstLevel"></param>
        /// <param name="ignoreList"></param>
        /// <param name="actions"></param>
        void LoadChildren<T, TP>(T item, bool onlyFirstLevel = false, List<string> ignoreList = null, params Expression<Func<T, TP>>[] actions);

        /// <summary>
        /// Get IList by sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlString"></param>
        /// <returns></returns>

        IList<T> Select<T>(string sqlString);

        /// <summary>
        /// Get IList by sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlString"></param>
        /// <returns></returns>

        Task<IList<T>> SelectAsync<T>(string sqlString);

        /// <summary>
        /// Get EntityChanges. entity has to be attachet 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        List<EntityChanges> GetObjectChanges(object entity);

        /// <summary>
        /// Get object changes
        /// </summary>
        /// <param name="entityA"></param>
        /// <param name="entityB"></param>
        /// <returns></returns>
        List<EntityChanges> GetObjectChanges(object entityA, object entityB);

        /// <summary>
        /// If enetiy is attached
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>

        bool IsAttached(object entity);

        /// <summary>
        /// Create Table by System.Type
        /// all under tables will be created
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="force">force recreation if table eg if exist delete then create agen</param>

        IRepository CreateTable<T>(bool force = false);


        /// <summary>
        /// Create Table by System.Type
        /// all under tables will be created
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="force">force recreation if table eg if exist delete then create agen</param>

        IRepository CreateTable(Type type, bool force = false);

        /// <summary>
        /// Remove Table
        /// all under tables will also be removed
        /// </summary>
        /// <typeparam name="T"></typeparam>

        IRepository RemoveTable<T>();

        /// <summary>
        /// Remove Table
        /// all under tables will also be removed
        /// </summary>
        /// <typeparam name="T"></typeparam>

        IRepository RemoveTable(Type type);

        /// <summary>
        /// Convert DbCommandExtended to List of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        ISqlQueryable<T> DataReaderConverter<T>(ISqlCommand command);

        /// <summary>
        /// Convert DbCommandExtended to list of System.Type
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IList DataReaderConverter(ISqlCommand command, Type type);

        /// <summary>
        /// Create Protected package that contain files or data for backup purpose or moving data from one location to another.
        /// Note that this package can only be readed by EntityWorker.Core
        /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        byte[] CreatePackage<T>(T package) where T : PackageEntity;

        /// <summary>
        /// Read the package and get its content
        /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        T GetPackage<T>(byte[] package) where T : PackageEntity;
    }
}
