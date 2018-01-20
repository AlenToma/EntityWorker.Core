using EntityWorker.Core.Helper;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;

namespace DataAccess.EntityWorker.Migrations
{
    public class MigrationIni : Migration
    {
        public override void ExecuteMigration(IRepository repository)
        {
            // Remove all tables from the database.
            // GetDbEntitys will return all type that containes a property with PrimaryKey attribute
            MethodHelper.GetDbEntitys(this.GetType().Assembly)
                .ForEach(repository.RemoveTable);

            // Now create all the tables structures for our modules
            MethodHelper.GetDbEntitys(this.GetType().Assembly)
                .ForEach(x => repository.CreateTable(x));
            base.ExecuteMigration(repository);
            repository.SaveChanges();
        }
    }
}
