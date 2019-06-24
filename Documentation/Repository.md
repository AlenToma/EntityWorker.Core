## Transaction
```csharp
    // Here we inherit from Transaction which contains the database logic for handling the transaction.
    // Thats all we need right now.
    // You will have to install the correct provider package.
    // Depending on which provider you use, you will have to install System.Data.SqlClient for mssql , Npgsql for pgsql and
    //System.Data.SQLite for SQLite. You will be noticed when the providers package is missing
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
            InitializeMigration();
        }
        
        // We could configrate our modules here instead of adding attributes in the class,
        // offcource its upp to you too choose.
        // note: this will override the attributes that exist in the class
        protected override void OnModuleConfiguration(IModuleBuilder moduleBuilder)
        {
           moduleBuilder.Entity<User>()
                .HasRule<UserRule>()
                .TableName("Users", "geto")
                .Property(x => x.Person)
                .HasKnownType(typeof(Person))
                .Property(x => x.Id)
                .HasPrimaryKey(true)
                .Property(x => x.UserName)
                .NotNullable()
                .HasDataEncode()
                .Property(x => x.Password)
                .HasJsonIgnore()
                .HasDataEncode()
                .Property(x => x.RoleId)
                .HasForeignKey<Role>()
                .Property(x => x.Role)
                .HasIndependentData()
                .Property(x => x.PersonId)
                .HasForeignKey<Person>();
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
