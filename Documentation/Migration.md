## Migration
EntityWorker.Core has its own Migration methods, so lets see how it works.
```csharp
   //Create Class and call it IniMigration and inhert from Migration
   public class IniMigration : Migration
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
