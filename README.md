# Introduction to [EntityWorker.Core](https://www.nuget.org/packages/EntityWorker.Core/)

## EntityFrameWork vs EntityWorker.Core Performance test
![screenshot](https://github.com/AlenToma/EntityWorker.Core/blob/master/EF_VS_EW.PNG?raw=true)

## CodeProject
[EntityWorker-Core-An-Alternative-to-Entity-Framewo](https://www.codeproject.com/Tips/1222424/EntityWorker-Core-An-Alternative-to-Entity-Framewo)

## EntityWorker.Core in Action
[LightData.CMS](https://github.com/AlenToma/LightData.CMS)

## <h1 id="update">Update in >= 1.2.5</h1>
DbEntity is Removed now, you dont have to inherit from it anymore. This way you can use the objects in other project without refering to EntityWorker.Core.

Also, make sure to read about GlobalConfiguration

SqlQueryable now inherits from IOrderedQueryable so we could cast it IQueryable<T> later on.

## .NET FRAMEWORK SUPPORT 
1- .NETCoreApp 2.0

2- .NETFramework 4.5.1

3- .NETFramework 4.6

4- .NETFramework 4.6.1

5- .NETStandard 2.0
## What is EntityWorker.Core?
EntityWorker.Core is an object-relation mappar that enables .NET developers to work with relational data using objects.
EntityWorker.Core is an alternative to the Entity Framework. It is more flexible and much faster than the Entity Framework.
## Can I use it to an existing database?
Yes, you can easily implement your existing modules and use attributes to map all your Primary Keys and Foreign Key without even
touching the database.
## Expression
EntityWorker.Core has its own provider called ISqlQueryable, which can handle almost every expression like Startwith,
EndWith, Contains and so on
See Code Example for more info.
## Code Example
Configurate GlobalConfiguration. 
```csharp
// In Application_Start
// Those two setting are for DataEnode Attribute
/// Set the key size for dataEncoding 128 or 256 Default is 128
EntityWorker.Core.GlobalConfiguration.DataEncode_Key_Size = DataCipherKeySize.Key_128;

/// Set the secret key for encoding. Default is "EntityWorker.Default.Key.Pass"
EntityWorker.Core.GlobalConfiguration.DataEncode_Key = "the key used to Encode the data";

/// Last set the culture for converting the data
EntityWorker.Core.GlobalConfiguration.CultureInfo = new CultureInfo("en");
```
let's start by creating the dbContext, lets call it Repository
```csharp
    // Here we inherit from Transaction which contains the database logic for handling the transaction.
    // Thats all we need right now.
    public class Repository : Transaction
    {
        // there are three databases types mssql, Sqlite and PostgreSql
        // then true or false for migration
        public Repository(DataBaseTypes dbType = DataBaseTypes.Mssql) : 
        base(GetConnectionString(dbType), true, dbType) 
        { 
            if (!base.DataBaseExist())
            {
                base.CreateDataBase();
            }
        }

        // get the full connection string
        public static string GetConnectionString(DataBaseTypes dbType)
        {
          if (dbType == DataBaseTypes.Mssql)
            return  @"Server=.\SQLEXPRESS; Database=CMS; User Id=root; Password=root;";
          else if (dbType == DataBaseTypes.Sqlite)
            return  @"Data Source=D:\Projects\CMS\source\App_Data\CMS.db";
          else return "Host=localhost;Username=postgres;Password=root;Database=CMS";
        }
    }
```
Let's start building our models, lets build a simple User model
```csharp
    // Table attribute indicates that the object name differs from the database table name
    [Table("Users")]
    [Rule(typeof(UserRule))]
    public class User
    {
        [PrimaryId]
        public Guid? Id { get; set; }
    
        public string UserName { get; set; }
        
       
        public string Password { get; set; }
        
        // Here we indicate that this attribute its a ForeignKey to object Role.
        [ForeignKey(type: typeof(Role))]
        public Guid Role_Id { get; set; }
        
        // when deleting an object the EntityWorker.Core will try and delete all object that are connected too 
        // by adding IndependentData we let the EntityWorker.Core know that this object should not be automaticlly deleted
        // when we delete a User
        [IndependentData]
        public Role Role { get; set; }

        public List<Address> Address { get; set; }
        
        //[ExcludeFromAbstract] means that it should not be included in the DataBase Update or insert.
        // It also means that it does not exist in the Table User.
        // use this attribute to include other properties that you only want to use in the code and it should not be 
        // saved to the database
        [ExcludeFromAbstract]
        public string test { get; set; }
    }
    
    [Table("Roles")]
    public class Role
    {
        [PrimaryId]
        public Guid? Id { get; set; }
        
        public string Name { get; set; }

        public List<User> Users { get; set; }
    }
    
    public class Address
    {
        [PrimaryId]
        public Guid? Id { get; set; }
        
        public string AddressName { get; set; }
        // in the User class we have a list of adresses, EntityWorker.Core will do an inner join and load the address 
        // if its included in the query
        [ForeignKey(typeof(User))]
        public Guid User_Id { get; set; }
    }
    
    // EntityWorker.Core has its own way to validate the data.
    // lets create an object and call it UserRule
    // above the User class we have specified this class to be executed before save and after.
    // by adding [Rule(typeof(UserRule))] to the user class
    public class UserRule : IDbRuleTrigger<User>
    {
        public void BeforeSave(IRepository repository, User itemDbEntity)
        {
            if (string.IsNullOrEmpty(itemDbEntity.Password) || string.IsNullOrEmpty(itemDbEntity.UserName))
            {
                // this will do a transaction rollback and delete all changes that have happened to the database
                throw new Exception("Password or UserName can not be empty");

            }
        }

        public void AfterSave(IRepository repository, User itemDbEntity, Guid objectId)
        {
            // lets do some changes here, when the item have updated..
              itemDbEntity.Password = MethodHelper.EncodeStringToBase64(itemDbEntity.Password);
            // and now we want to save this change to the database 
            // the EntityWorker.Core will now know that it need to update the database agen.
            // it will detect the changes that has been made to the current object
        }
    }

```
## Query and Expression
Lets build a select expression here and see how it works
```csharp
   using (var rep = new Repository())
   {
        // LoadChildren indicates that it will load the children hierarchy.
        // It has no problem handling circular references.
        // The query does not call to the database before we invoke Execute or ExecuteAsync
        var users = rep.Get<User>().Where(x => 
                (x.Role.Name.EndsWith("SuperAdmin") &&
                 x.UserName.Contains("alen")) ||
                 x.Address.Any(a=> a.AddressName.StartsWith("st"))
                ).LoadChildren().Execute(); 
                
        // lets say that we need only to load some children and ignore some other, then our select will be like this instead
          var users = rep.Get<User>().Where(x => 
                (x.Role.Name.EndsWith("SuperAdmin") &&
                 x.UserName.Contains("alen")) ||
                 x.Address.Any(a=> a.AddressName.StartsWith("st"))
                ).LoadChildren(x=> x.Role.Users.Select(a=> a.Address), x=> x.Address)
                .IgnoreChildren(x=> x.Role.Users.Select(a=> a.Role)).OrderBy(x=> x.UserName).Skip(20).Take(100).Execute();        
        Console.WriteLine(users.ToJson());
        Console.ReadLine();
   }

```
## Edit, delete and insert
EntityWorker.Core has only one method for inserts and updates.
Depending on the primarykey it will either execute an update or insert.
```csharp
   using (var rep = new Repository())
   {
        var users = rep.Get<User>().Where(x => 
                (x.Role.Name.EndsWith("SuperAdmin") &&
                 x.UserName.Contains("alen")) ||
                 x.Address.Any(a=> a.AddressName.StartsWith("st"))
                ).LoadChildren();
         foreach (User user in users.Execute())
         {
             user.UserName = "test 1";
             user.Role.Name = "Administrator";
             user.Address.First().AddressName = "Changed";
             // now we could do rep.Save(user); but will choose to save the whole list late
         }
        users.Save();    
        // how to delete 
        // remove all the data offcource Roles will be ignored here 
        users.Remove();
        // Remove with expression
        users.RemoveAll(x=> x.UserName.Contains("test"));
        // we could also clone the whole object and insert it as new to the database like this
        var clonedUser = users.Execute().Clone().ClearAllIdsHierarchy();
        foreach (var user in clonedUser)
        {
            // Now this will clone the object to the database, of course all Foreign Key will be automatically assigned.
            // Role wont be cloned here. cource it has IndependedData attr we could choose to clone it too, by invoking
            // .ClearAllIdsHierarchy(true);
            rep.Save(user);
        }
        Console.WriteLine(users.ToJson());
        Console.ReadLine();
   }

```
## ObjectChanges
lets see how EntityWorker gets the changed objects 
```csharp
        using (var rep = new Repository())
            {
                //var m = rep.Get<User>().
                var user = rep.Get<User>().LoadChildren().ExecuteFirstOrDefault();
                user.UserName = "hahahadfsfddfsdfhaha";
                var changes = rep.GetObjectChanges(person); 
                var oldValue = changes.First().OldValue;
                var newValue = changes.First().NewValue;
                var propertyName = changes.First().PropertyName;
                rep.Save(user);
                var changes2 = rep.GetObjectChanges(person);
                rep.SaveChanges();
                var changes3 = rep.GetObjectChanges(person);
            }

```
## LinqToSql Result Example
lets test and see how EntityWorker.Core LinqToSql generator looks like.
We will execute a complicated query and see how it gets parsed.
```csharp
            using (var rep = new Repository())
            {
               ISqlQueriable<User> users = rep.Get<User>().Where(x =>
               (x.Role.Name.EndsWith("SuperAdmin") &&
                x.UserName.Contains("alen")) ||
                x.Address.Any(a => (a.AddressName.StartsWith("st") || a.AddressName.Contains("mt")) && a.Id > 0).
                Skip(20).Take(100).Execute();  
                );
                
                List<User> userList = users.Execute();
                var sql = users.ParsedLinqToSql;
            }
            // And here is the generated Sql Query
             SELECT distinct Users.* FROM Users 
             left join [Roles] CEjB on CEjB.[Id] = Users.[Role_Id]
             WHERE (([CEjB].[Name] like String[%SuperAdmin] AND [Users].[UserName] like String[%alen%]) 
             OR  EXISTS (SELECT 1 FROM [Address] 
             INNER JOIN [Address] MJRhcYK on Users.[Id] = MJRhcYK.[User_Id]
             WHERE (([Address].[AddressName] like String[st%] OR [Address].[AddressName] like String[%mt%]) AND ([Address].[Id] > 0))))
             ORDER BY Id
             OFFSET 20
             ROWS FETCH NEXT 100 ROWS ONLY;
             // All String[] and Date[] will be translated to Parameters later on.   
```

## Migration
EntityWorker.Core has its own Migration methods, so lets see how it works.
```csharp
   //Create Class and call it IniMigration and inhert from Migration
   public class IniMigration : Migration
        public IniMigration()
        {
           // in the database a migration will be created that contains this Identifier.
           // it's very important that its unique.
            MigrationIdentifier = "SystemFirstStart"; 
        }
        public override void ExecuteMigration(ICustomRepository repository)
        {
            // create the tables User, Role, Address 
            // because we have a foreign keys in user class that refer to address and roles, those will also be
            // created
            repository.CreateTable<User>(true);
            var user = new User()
            {
                Role = new Role() { Name = "Admin" },
                Address = new List<Address>() { new Address() { AddressName = "test" } },
                UserName = "Alen Toma",
                Password = "test"
            };
            repository.Save(user);

            base.ExecuteMigration(repository);
        }
    }
  }

    // now lets create the MigrationConfig Class
    public class MigrationConfig : IMigrationConfig
    {
        /// <summary>
        /// All available Migrations to be executed.
        // when Migration Is enabled in Transaction.
        // this class will be triggered at system start.
        /// </summary>
        public IList<Migration> GetMigrations(ICustomRepository repository)
        {
            // return all migrations that are to be executetd
            // all already executed migrations that do exist in the database will be ignored
            return new List<Migration>(){new IniMigration()};
        }
    }

```
## Entity Mappings
Sometimes we want to use a different model for database and presentation.
And we want to be able to map and convert from one type to another.
EntityWorker.Core has similar functionality that could do just that with few lines.
lets try casting User to UserModule with diffrent property names in each class.
```csharp
using EntityWorker.Core.Helper;
public class UserModule {
/// se here we mapped Name to UserName or we could just call the property UserName,
/// then we won't need the PropertyName Attribute
[PropertyName("UserName")]
public string Name { get; set; }

[PropertyName("Password")]
public string Key { get; set;}

[PropertyName("Role")]
public RoleModule UserGroup { get; set; }
}

var user =new List<User>(){ new User { UserName="test", Password="test", Role= new Role() }};

var userModule = user.ToType<List<UserModule>>()
//or for only first item 
user.ToType<UserModule>() ;

```
## Attributes 
There are many attributes you could use to improve the code
```csharp
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
/// PrimaryId could be System.Guid or number eg long and int
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


## Issues
Please report any bugs or improvement you might find.
