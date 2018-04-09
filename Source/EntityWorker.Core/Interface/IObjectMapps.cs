using EntityWorker.Core.Helper;
using System;
using System.Linq.Expressions;

namespace EntityWorker.Core.Interface
{
    /// <summary>
    /// Entity Configration
    /// Here we could configrate and set all primary keys and Foreign keys for properties
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectMapps<T> where T : class
    {
        /// <summary>
        /// Assign diffrent name for the object in the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IObjectMapps<T> TableName(string name);

        /// <summary>
        /// Assign a rule to object
        /// </summary>
        /// <typeparam name="Source">Must implement interface IDbRuleTrigger</typeparam>
        /// <returns></returns>
        IObjectMapps<T> HasRule<Source>();

        /// <summary>
        /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> HasJsonIgnore<TP>(Expression<Func<T, TP>> action);

        /// <summary>
        /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> HasXmlIgnore<TP>(Expression<Func<T, TP>> action);



        /// <summary>
        /// Use this when you have types that are unknown like interface wich it can takes more than one type 
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> HasKnownType<TP>(Expression<Func<T, TP>> action, Type objectType);

        /// <summary>
        /// Assign a diffrent database type for the property
        /// Attibutes Stringify, DataEncode and ToBase64String will override this attribute.
        /// </summary>
        /// <param name="dataType">The database type ex nvarchar(4000)</param>
        /// <param name="dataBaseTypes">null for all providers</param>
        /// <returns></returns>
        IObjectMapps<T> HasColumnType<TP>(Expression<Func<T, TP>> action, string dataType, DataBaseTypes? dataBaseTypes = null);

        /// <summary>
        /// Add Primary Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="autoGenerate"></param>
        /// <returns></returns>
        IObjectMapps<T> HasPrimaryKey<TP>(Expression<Func<T, TP>> action, bool autoGenerate = true);

        /// <summary>
        /// Add Foreign Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="Source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        IObjectMapps<T> HasForeignKey<Source, TP>(Expression<Func<T, TP>> action, string propertyName = null) where Source : class;

        /// <summary>
        /// Add DataEncode for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="key"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
        IObjectMapps<T> HasDataEncode<TP>(Expression<Func<T, TP>> action, string key = null, DataCipherKeySize keySize = DataCipherKeySize.Default);

        /// <summary>
        /// Add IndependentData for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> HasIndependentData<TP>(Expression<Func<T, TP>> action) where TP : class;

        /// <summary>
        /// Add NotNullable for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> NotNullable<TP>(Expression<Func<T, TP>> action);

        /// <summary>
        /// Add PropertyName for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        IObjectMapps<T> HasPropertyName<TP>(Expression<Func<T, TP>> action, string name, string displayName = null);

        /// <summary>
        /// Add Stringify for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> HasStringify<TP>(Expression<Func<T, TP>> action);

        /// <summary>
        /// Add ExcludeFromAbstract for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> ExcludeFromAbstract<TP>(Expression<Func<T, TP>> action);

        /// <summary>
        /// Add DefaultOnEmpty for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"> Fefault value when property is null</param>
        /// <returns></returns>
        IObjectMapps<T> HasDefaultOnEmpty<TP>(Expression<Func<T, TP>> action, object value);

        /// <summary>
        /// Add ToBase64String for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        IObjectMapps<T> HasToBase64String<TP>(Expression<Func<T, TP>> action);
    }
}
