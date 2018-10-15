using EntityWorker.Core.Transaction;
using EntityWorker.Core.Helper;
using System.Linq;
using EntityWorker.Core.Interface;
using LightData.CMS.Modules.Library;
using System;
using LightData.CMS.Modules.Rules;

namespace LightData.CMS.Modules.Repository
{
    public class Repository : Transaction
    {
        public Repository(DataBaseTypes dbType = DataBaseTypes.Mssql) : base(GetConnectionString(dbType), dbType)
        {

        }

        protected override void OnModuleStart()
        {
            if (!base.DataBaseExist())
                base.CreateDataBase();

            /// Limited support for sqlite
            // Get the latest change between the code and the database. 
            // Property Rename is not supported. renaming property x will end up removing the x and adding y so there will be dataloss
            // Adding a primary key is not supported either
            var latestChanges = GetCodeLatestChanges();
            if (latestChanges.Any())
                latestChanges.Execute(true);

            // Start the migration
            InitializeMigration();
        }

        protected override void OnModuleConfiguration(IModuleBuilder moduleBuilder)
        {
            moduleBuilder.Entity<User>()
                .TableName("Users", "geto")
                .HasKnownType(x => x.Person, typeof(Person))
                .HasPrimaryKey(x => x.Id, false)
                .NotNullable(x => x.UserName)
                .HasDataEncode(x => x.UserName)
                .HasDataEncode(x => x.Password)
                .HasForeignKey<Role, Guid>(x => x.RoleId)
                .HasIndependentData(x => x.Role)
                .HasForeignKey<Person, Guid>(x => x.PersonId)
                .HasRule<UserRule>()
                .HasJsonIgnore(x => x.Password);

            moduleBuilder.EntityType(typeof(User))
                .TableName("Users", "geto")
                .HasKnownType("Person", typeof(Person))
                .HasPrimaryKey("Id", false)
                .NotNullable("UserName")
                .HasDataEncode("UserName")
                .HasDataEncode("Password")
                .HasForeignKey<Role>("RoleId")
                .HasIndependentData("Role")
                .HasForeignKey<Person>("PersonId")
                .HasRule<UserRule>()
                .HasJsonIgnore("Password");
        }

        // Get the full connection string from the web-config
        public static string GetConnectionString(DataBaseTypes dbType)
        {
            return dbType == DataBaseTypes.Mssql ? @"Server=DESKTOP-4BRMQVE\mssql;Trusted_Connection=false; Database=CMStest; User Id=root; Password=root;" :
                  (dbType == DataBaseTypes.Sqllight ? @"Data Source=D:\Projects\LightData.CMS\source\LightData.CMS\App_Data\LightDataTabletest.db" :
                  "Host=localhost;Username=postgres;Password=root;Database=mydatabase");
        }
    }
}
