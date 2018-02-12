## Attributes 
There are many attributes you could use to improve the code
```csharp
/// <summary>
/// Assign a diffrent database type for the property. 
/// Attibutes Stringify, DataEncode and ToBase64String will override this attribute. 
/// </summary>
/// <param name="dataType">The database type ex nvarchar(4000)</param>
/// <param name="dataBaseTypes">(Optional)null for all providers</param>
[ColumnType]

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
