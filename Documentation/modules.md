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
        
        // Adress will be removed when we remove the user
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
