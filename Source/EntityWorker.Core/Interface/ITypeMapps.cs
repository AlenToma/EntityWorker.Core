using EntityWorker.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityWorker.Core.Interface
{
    /// <summary>
    /// Entity Configration
    /// Here we could configrate and set all primary keys and Foreign keys for properties
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypeMapps
    {
        /// <summary>
        /// Assign diffrent name for the object in the database.
        /// schema works only for MSSQl and postGreSql
        /// Database should allow Create Schema for this to work or the Schema should already be created
        /// </summary>
        /// <param name="name"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        ITypeMapps TableName(string name, string schema = null);

        /// <summary>
        /// Assign a rule to object
        /// </summary>
        /// <typeparam name="Source">Must implement interface IDbRuleTrigger</typeparam>
        /// <returns></returns>
        ITypeMapps HasRule<Source>();

        /// <summary>
        /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasJsonIgnore(string action);

        /// <summary>
        /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasXmlIgnore(string action);

        /// <summary>
        /// Use this when you have types that are unknown like interface wich it can takes more than one type 
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasKnownType(string action, Type objectType);

        /// <summary>
        /// Assign a diffrent database type for the property
        /// Attibutes Stringify, DataEncode, JsonDocument , XmlDocument and ToBase64String will override this attribute.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dataType">The database type ex nvarchar(4000)</param>
        /// <param name="dataBaseTypes">null for all providers</param>
        /// <returns></returns>
        ITypeMapps HasColumnType(string action, string dataType, DataBaseTypes? dataBaseTypes = null);

        /// <summary>
        /// Add Primary Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="autoGenerate"></param>
        /// <returns></returns>
        ITypeMapps HasPrimaryKey(string action, bool autoGenerate = true);

        /// <summary>
        /// Add Foreign Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="Source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        ITypeMapps HasForeignKey<Source>(string action, string propertyName = null) where Source : class;

        /// <summary>
        /// Add DataEncode for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="key"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
        ITypeMapps HasDataEncode(string action, string key = null, DataCipherKeySize keySize = DataCipherKeySize.Default);

        /// <summary>
        /// Add IndependentData for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasIndependentData(string action);

        /// <summary>
        /// Add NotNullable for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps NotNullable(string action);

        /// <summary>
        /// Add PropertyName for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        ITypeMapps HasPropertyName(string action, string name, string displayName = null);

        /// <summary>
        /// Add Stringify for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasStringify(string action);

        /// <summary>
        /// Add ExcludeFromAbstract for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps ExcludeFromAbstract(string action);

        /// <summary>
        /// Add DefaultOnEmpty for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"> Fefault value when property is null</param>
        /// <returns></returns>
        ITypeMapps HasDefaultOnEmpty(string action, object value);

        /// <summary>
        /// Add ToBase64String for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasToBase64String(string action);

        /// <summary>
        /// Add JsonDocument for property
        /// Save the property as Json object in the database 
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasJsonDocument(string action);

        /// <summary>
        /// Add XmlDocument for property
        /// Save the property as xml object in the database 
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        ITypeMapps HasXmlDocument(string action);
    }
}
