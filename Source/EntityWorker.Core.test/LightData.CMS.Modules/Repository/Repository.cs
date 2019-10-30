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

            //moduleBuilder.EntityType(typeof(User))
            //    .HasRule(typeof(UserRule))
            //    .TableName("Users", "geto")
            //    .Property("Person")
            //    .HasKnownType(typeof(Person))
            //    .Property("Id")
            //    .HasPrimaryKey(false)
            //    .Property("UserName")
            //    .NotNullable()
            //    .HasDataEncode()
            //    .Property("Password")
            //    .HasJsonIgnore()
            //    .HasDataEncode()
            //    .Property("RoleId")
            //    .HasForeignKey<Role>("Role_Id")
            //    .Property("Role")
            //    .HasIndependentData()
            //    .Property("PersonId")
            //    .HasForeignKey<Person>("Person_Id");
        }

        // Get the full connection string from the web-config
        public static string GetConnectionString(DataBaseTypes dbType)
        {
            return dbType == DataBaseTypes.Mssql ?
                @"Server=DESKTOP-Q2EP00O\SQLEXPRESS;Trusted_Connection=false; Database=CMStest; User Id=root; Password=root;" :
                  (dbType == DataBaseTypes.Sqllight ? @"Data Source=D:\Projects\EntityWorker.Core\Source\EntityWorker.Core.test\LightData.CMS.Modules\app_data\LightDataTabletest.db" :
                  "Host=localhost;Username=postgres;Password=root;Database=mydatabase");
        }
    }
}
