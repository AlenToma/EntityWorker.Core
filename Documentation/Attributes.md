## Attributes 
There are many attributes you could use to improve the code
```csharp
/// <summary>
/// Use this when you have types that are unknown like interface wich it can takes more than one type
/// <example>
/// [KnownType(objectType: typeof(Person))]
/// public List<IPerson> Person { get; set; }
/// </example>
/// </summary>
[KnownType]

// For Specify different name for json serialize
[System.Runtime.Serialization.DataMemberAttribute]

/// <summary>
/// Save the property as xml object in the database
/// For the moment those values cant be searched by linq.
/// </summary>
[XmlDocument]

/// <summary>
/// Save the property as Json object in the database
/// For the moment those values cant be searched by linq.
/// you will have to use row sql(JSON_VALUE) to seach them
/// </summary>
[JsonDocument]

/// <summary>
/// Assign a diffrent database type for the property. 
/// Attibutes Stringify, DataEncode ,XmlDocument ,JsonDocument and ToBase64String will override this attribute. 
/// </summary>
/// <param name="dataType">The database type ex nvarchar(4000)</param>
/// <param name="dataBaseTypes">(Optional)null for all providers</param>
[ColumnType]

/// <summary>
/// Ignore serlizing and deserializing property
/// when deserializing using entityWorker.Xml all Xml ignored property will be loaded back
/// from the database as long as primary key exist withing the xml string.
/// </summary>
[XmlIgnore]

/// <summary>
/// Ignore serlizing and deserializing property
/// when deserializing using entityWorker.Json all Json ignored property will be loaded back
/// from the database as long as primary key exist withing the json string.
/// </summary>
[JsonIgnore]

/// <summary>
/// This indicates that the prop will not be saved to the database.
/// </summary>
[ExcludeFromAbstract]

/// <summary>
/// Will be saved to the database as base64string 
/// and converted back to its original string when its read
/// </summary>
[ToBase64String]

/// <summary>
/// Property is a ForeignKey in the database.
/// </summary>
[ForeignKey]

/// <summary>
/// This attr will tell EntityWorker.Core abstract to not auto Delete this object when deleting parent,
/// it will however try to create new or update  
/// </summary>
[IndependentData]

/// This attribute is most used on properties with type string
/// in-case we don't want them to be nullable
/// </summary>
[NotNullable]

/// <summary>
/// Property is a primary key
/// PrimaryId could be System.String,  System.Guid or number eg long and int
/// </summary>
[PrimaryKey]

/// <summary>
/// Have diffrent Name for the property in the database
/// </summary>
[PropertyName]

/// <summary>
/// Define class rule by adding this attribute
/// ruleType must inherit from IDbRuleTrigger
/// ex UserRule : IDbRuleTrigger<User/>
/// </summary>
/// <param name="ruleType"></param>
[Rule]

/// <summary>
/// Save the property as string in the database
/// mostly used when we don't want an enum to be saved as integer in the database
/// </summary>
[StringFy]

/// <summary>
/// Define diffrent name for the table
/// </summary>
[Table]

/// <summary>
/// Assign Default Value when Property is null
/// </summary>
[DefaultOnEmpty]

 /// <summary>
 /// Choose to protect a property in the database so no one could read or decript it without knowing the key
 /// LinqToSql will also Encode the value when you select a Search
 /// <Example>
 /// [DataEncode]
 /// public string Password { get; set;}
 /// now when we search
 /// .Where(x=> x.Password == "test") Will be equal to .Where(x=> x.Password == Encode("test"))
 /// so no need to worry when you search those column in the dataBase 
 /// you could Encode Adress, bankAccount information and so on with ease
 /// </Example>
 /// </summary>
[DataEncode]

```
