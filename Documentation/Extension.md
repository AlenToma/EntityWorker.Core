## EntityWorker.Core Extension
EntityWorker Containe many usefull method that will help you in your developing
```csharp
using EntityWorker.Core.Helper; 
class name (Extension)

        /// <summary>
        /// Convert To Json
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToJson(this object o)
        
        /// <summary>
        /// generic Json to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string json)
        
        /// <summary>
        /// Json to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FromJson(this string json, Type type)
        
        /// <summary>
        /// Json to object
        /// json must containe the $type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object FromJson(this string json)
        
        /// <summary>
        /// Xml to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="repository"> Assign repository to load XmlIgnored properties</param>
        /// <returns></returns>
        public static T FromXml<T>(this string xml, IRepository repository = null) where T : class
        
        /// <summary>
        /// Xml to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="repository"> Assign repository to load XmlIgnored properties</param>
        /// <returns></returns>
        public static object FromXml(this string xml, IRepository repository = null)
        
        /// <summary>
        /// Object to xml
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToXml(this object o)
        
        /// <summary>
        /// Get PropertyName of the expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string GetMemberName<T, TP>(this Expression<Func<T, TP>> action)
        
        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static T ClearAllIdsHierarchy<T>(this T item, bool includeIndependedData = false)
        
        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static List<T> ClearAllIdsHierarchy<T>(List<T> item, bool includeIndependedData = false)
        
        /// <summary>
        /// Clear all PrimaryId and ForeignKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static object ClearAllIdsHierarchy(object item, bool includeIndependedData = false)
        
        /// <summary>
        /// TrimEnd with string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimEnd(this string source, string value)
        
        /// <summary>
        /// Try and insert Last
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="text"> string to insert</param>
        /// <param name="identifier">after last identified string </param>
        /// <param name="insertLastIfNotFound">insert after a specific string, count is from last, even if the identifier text not found</param>
        /// <returns></returns>
        public static string InsertAfter(this string str, string text, string identifier, bool insertLastIfNotFound = true)
        
        /// <summary>
        /// Try to insert 
        /// </summary>
        /// <param name="str">Source</param>
        /// <param name="text">string to insert</param>
        /// <param name="identifier"></param>
        /// <param name="insertLastIfNotFound"></param>
        /// <returns></returns>
        public static string InsertBefore(this string str, string text, string identifier, bool insertLastIfNotFound = true)
        
        /// <summary>
        /// Search and insert before identifier
        /// </summary>
        /// <param name="str"></param>
        /// <param name="text"></param>
        /// <param name="identifier"></param>
        /// <param name="insertLastIfNotFound"></param>
        /// <returns></returns>
        public static StringBuilder InsertBefore(this StringBuilder str, string text, string identifier, bool insertLastIfNotFound = true)
        
        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static string GetDbTypeByType(this Type type, DataBaseTypes dbType)
        
        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static string GetDbTypeByType(this IFastDeepClonerProperty prop, DataBaseTypes dbType)
        
        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static List<string> GetDbTypeListByType(this IFastDeepClonerProperty prop, DataBaseTypes dbType)
        
        /// <summary>
        /// if date is between two dates
        /// </summary>
        /// <param name="input"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        
        /// <summary>
        /// Get PropertyName from the cashed Properties
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string GetPropertyName(this IFastDeepClonerProperty prop)
        
        /// <summary>
        /// Type is numeric eg long, decimal or float
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumeric(this Type type)
        
        /// <summary>
        /// Get the Primary key from type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IFastDeepClonerProperty GetPrimaryKey(this Type type)
        
        /// <summary>
        /// The value of attribute Table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Table TableName<T>()
        
        /// <summary>
        /// The value of attribute Table
        /// </summary>
        /// <typeparam name="type"></typeparam>
        /// <returns></returns>
        public static Table TableName(this Type type)
        
        /// <summary>
        /// Get the Primary key from type
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IFastDeepClonerProperty GetPrimaryKey(this object item)
        
        /// <summary>
        /// Get the Primary key Value
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static object GetPrimaryKeyValue(this object item)
        
        /// <summary>
        /// Validate string and guid and long 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ObjectIsNew(this object value)
        
        /// <summary>
        /// Set the Primary key Value
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetPrimaryKeyValue(this object item, object value = null)
        
        /// <summary>
        /// Create Instance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uninitializedObject"> true for FormatterServices.GetUninitializedObject and false for Activator.CreateInstance </param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, bool uninitializedObject = false)
        
        /// <summary>
        /// Get Internal type of IList
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetActualType(this Type type)
        
        /// <summary>
        /// Check if string is base64
        /// this is only a simple validation by an regxp 
        /// @"^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBase64String(this string str)
        
        /// <summary>
        /// Convert From type to another type,
        /// make sure to have the same propertyNames in both or you could also map them by PropertyName Attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T ToType<T>(this object o)
        
        /// <summary>
        /// Clone Object, se FastDeepCloner fo more information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> items, FieldType fieldType = FieldType.PropertyInfo)
```
```csharp
using EntityWorker.Core.Helper; 
class name (MethodHelper)

        ///// <summary>
        /////  Get All types that containe Property with PrimaryKey Attribute
        ///// </summary>
        ///// <param name="assembly"></param>
        ///// <returns></returns>
        public static List<Type> GetDbEntitys(Assembly assembly)
        
        /// <summary>
        /// Convert Value from Type to Type
        /// when fail a default value will be loaded.
        /// can handle all known types like datetime, time span, string, long etc
        /// ex
        ///  "1115rd" to int? will return null
        ///  "152" to int? 152
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        public static T ConvertValue<T>(this object value)
        
        /// <summary>
        /// Convert Value from Type to Type
        /// when fail a default value will be loaded.
        /// can handle all known types like datetime, time span, string, long etc
        /// ex
        ///  "1115rd" to int? will return null
        ///  "152" to int? 152
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        public static object ConvertValue(this object value, Type toType)
        
        /// <summary>
        /// Convert String ToBase64String
        /// </summary>
        /// <param name="stringToEncode"></param>
        /// <returns></returns>
        public static string EncodeStringToBase64(string stringToEncode)
        
        /// <summary>
        /// Convert Base64String To String 
        /// </summary>
        /// <param name="stringToDecode"></param>
        /// <returns></returns>
        public static string DecodeStringFromBase64(string stringToDecode)
```

