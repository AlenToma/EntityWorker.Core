using EntityWorker.Core.Helper;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using LightData.CMS.Modules.Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightData.CMS.Modules.Migrations
{
    public class MigrationIni : Migration
    {
        public override void ExecuteMigration(IRepository repository)
        {
            var folders = new List<Folder>();
            try
            {
                folders.AddRange(repository.Get<Folder>().Where(x => !x.Parent_Id.HasValue).LoadChildren()
                    .IgnoreChildren(x => x.Files.Select(a => a.Folder),
                    x => x.Files.Select(a => a.Slider.Select(s => s.File)),
                    x => x.Files.Select(a => a.Slider.Select(s => s.SliderCollection.Sliders))
                    ).Execute());
            }
            catch (Exception ex)
            {
                // Ignore
            }

            MethodHelper.GetDbEntitys(this.GetType().Assembly).ForEach(repository.RemoveTable);
            MethodHelper.GetDbEntitys(this.GetType().Assembly).ForEach(x => repository.CreateTable(x));
            folders.ForEach(x => repository.Save(x.ClearAllIdsHierarchy(true)));
            base.ExecuteMigration(repository);
        }
    }
}
