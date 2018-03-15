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
        /// <param name="primaryKey"></param>
        /// <returns></returns>

        ILightDataTable GetLightDataTable(DbCommandExtended cmd, string primaryKey = null);

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type">Set for faster Converting of dbreader to object</param>
        /// <returns></returns>
        DbCommandExtended GetSqlCommand(string sql, Type type = null);

        /// <summary>
        /// Return SqlCommand that already contain SQLConnection
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="type">Set for faster Converting of dbreader to object</param>
        /// <returns></returns>
        DbCommandExtended GetStoredProcedure(string storedProcedure, Type type = null);

        /// <summary>
        /// Add Parameter to sqlCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="attrName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>

        IRepository AddInnerParameter(DbCommandExtended cmd, string attrName, object value, SqlDbType dbType = SqlDbType.NVarChar);

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

        object ExecuteScalar(DbCommandExtended cmd);

        /// <summary>
        /// Execute Quary
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>

        int ExecuteNonQuery(DbCommandExtended cmd);

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

        void Delete(object entity);
        /// <summary>
        /// Delete entity.
        /// SaveChanges is needed after
        /// </summary>
        /// <param name="entity"></param>
        /// 
        Task DeleteAsync(object entity);

        /// <summary>
        /// Save entity.
        /// SaveChanges is needed after
        /// </summary>
        /// <param name="entity"></param>

        void Save(object entity);

        /// <summary>
        /// Save entity.
        /// SaveChanges is needed after
        /// </summary>
        /// <param name="entity"></param>

        Task SaveAsync(object entity);

        /// <summary>
        /// Attach an object to entityWorker.
        /// this is only to validate two objects to get the changes between two objects.
        /// You dont have to attach the object to save it to the database
        /// </summary>
        /// <param name="objcDbEntity"></param>
        /// <param name="overwrite"> override if exist</param>
        void Attach(object objcDbEntity, bool overwrite = false);

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
        /// <returns></returns>
        ISqlQueryable<T> FromJson<T>(string json) where T : class;

        /// <summary>
        /// Get ISqlQueryable from Json.
        /// All JsonIgnore Values will be loaded from the database if a primary key exist and the value is default()  eg null or empty or even 0 for int and decimal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<ISqlQueryable<T>> FromJsonAsync<T>(string json) where T : class;

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

        void CreateTable<T>(bool force = false);


        /// <summary>
        /// Create Table by System.Type
        /// all under tables will be created
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="force">force recreation if table eg if exist delete then create agen</param>

        void CreateTable(Type type, bool force = false);

        /// <summary>
        /// Remove Table
        /// all under tables will also be removed
        /// </summary>
        /// <typeparam name="T"></typeparam>

        void RemoveTable<T>();

        /// <summary>
        /// Remove Table
        /// all under tables will also be removed
        /// </summary>
        /// <typeparam name="T"></typeparam>

        void RemoveTable(Type type);

        /// <summary>
        /// Convert DbCommandExtended to List of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        ISqlQueryable<T> DataReaderConverter<T>(DbCommandExtended command);

        /// <summary>
        /// Convert DbCommandExtended to list of System.Type
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IList DataReaderConverter(DbCommandExtended command, Type type);

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
