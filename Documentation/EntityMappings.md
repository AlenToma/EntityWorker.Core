
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
