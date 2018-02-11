## Migration
EntityWorker.Core has its own Migration methods, so lets see how it works.
```csharp
   //Create Class and call it IniMigration and inherit from Migration
   public class IniMigration : Migration
   {
        public override void ExecuteMigration(IRepository repository)
        {
            var user = new User()
            {
                Role = new Role() { Name = "Admin" },
                Address = new List<Address>() { new Address() { AddressName = "test" } },
                UserName = "Alen Toma",
                Password = "test"
            };
            repository.Save(user);
        }
    }

    // now lets create the MigrationConfig Class
    public class MigrationConfig : IMigrationConfig
    {
        /// <summary>
        /// All available Migrations to be executed.
        /// trigger this class by InitializeMigration() in OnModuleStart
        /// </summary>
        public IList<Migration> GetMigrations(IRepository repository)
        {
            // return all migrations that are to be executetd
            // all already executed migrations that do exist in the database will be ignored
            return new List<Migration>(){new IniMigration()};
        }
     }

```
