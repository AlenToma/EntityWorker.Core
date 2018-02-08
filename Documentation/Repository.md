## Transaction
```csharp
    // Here we inherit from Transaction which contains the database logic for handling the transaction.
    // Thats all we need right now.
    public class Repository : Transaction
    {
        // there are three databases types mssql, Sqlite and PostgreSql
        public Repository(DataBaseTypes dbType = DataBaseTypes.Mssql) : 
        base(GetConnectionString(dbType), dbType) 
        { 
        }
        
        protected override void OnModuleStart()
        {
            if (!base.DataBaseExist())
                base.CreateDataBase();
                
                
            // You could choose to use this to apply you changes to the database or create your own migration
            // that will update the database, like alter drop or create.
            // Limited support for sqlite
            // Get the latest change between the code and the database. 
            // Property Rename is not supported. renaming property x will end up removing the x and adding y so there will be dataloss
            // Adding a primary key is not supported either
            var latestChanges = GetCodeLatestChanges();
            if (latestChanges.Any())
                latestChanges.Execute(true);
                
             // Start the migration
            InitiolizeMigration();
            base.OnModuleStart();
        }
        
        // We could configrate our modules here instead of adding attributes in the class, offcource you could choose.
        protected override void OnModuleConfiguration(IModuleBuilder moduleBuilder)
        {
            moduleBuilder.Entity<User>()
                .TableName("Users")
                .HasPrimaryKey(x => x.Id, false)
                .NotNullable(x => x.UserName)
                .HasDataEncode(x => x.UserName)
                .HasForeignKey<Role, Guid>(x => x.RoleId)
                .HasIndependentData(x => x.Role)
                .HasForeignKey<Person, Guid>(x => x.PersonId);
                
            base.OnModuleConfiguration(moduleBuilder);
         }


        // get the full connection string
        // for postgresql make sure to have the database name lower case
        public static string GetConnectionString(DataBaseTypes dbType)
        {
          if (dbType == DataBaseTypes.Mssql)
            return  @"Server=.\SQLEXPRESS; Database=CMS; User Id=root; Password=root;";
          else if (dbType == DataBaseTypes.Sqlite)
            return  @"Data Source=D:\Projects\CMS\source\App_Data\CMS.db";
          else return "Host=localhost;Username=postgres;Password=root;Database=cms"; 
        }
    }
```
