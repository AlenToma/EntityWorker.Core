using System.Collections.Generic;
using System.Linq;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using LightData.CMS.Modules.Library;
using LightData.CMS.Modules.Helper;
namespace LightData.CMS.Modules.Migrations
{
    public class MigrationStartUp : Migration
    {
        public override void ExecuteMigration(IRepository repository)
        {
            var countries = Methods.GetCountriesByIso3166()
                .Select(culture => new Country()
                {
                    CountryCode = culture.Name,
                    Name = culture.DisplayName
                }).ToList();

            foreach (var country in countries)
            {
                repository.Save(country);
            }

            var siteSettingCollection = new SiteSettingCollection()
            {
                Name = "Theme Settings",
                SiteSettings = new List<SiteSetting>()
                {
                new SiteSetting()
                {
                    Name = "Default Theme",
                    Value = "Demo",
                    Key = EnumHelper.Keys.DefaultTheme
                },
                new SiteSetting()
                {
                    Name = "Site local path",
                    Value = @"D:\Projects\LightData.CMS\source\LightDataTable.Site\Views",
                    Key = EnumHelper.Keys.LocalPath
                }
                }
            };

            var role = new Role() { Name = "Admin", RoleDefinition = EnumHelper.RoleDefinition.Developer };
            repository.Save(siteSettingCollection);
            var users = new List<User>();
            var i = 0;
            while (i != 1)
            {
                users.AddRange(new List<User>()
            {
                new User()
                {
                Id= "ADMIN",
                UserName = "Admin",
                Password = "Admin",
                Role = role,
                Person = new Person()
                {
                    FirstName = "Alen",
                    LastName = "Toma",
                    SureName = "Nather",
                    Addresses = new List<Address>()
                    {
                        new Address()
                        {
                            Country = countries.Find(x=> x.CountryCode.Contains("SV")),
                            Name = "test"
                        }
                    }
                }
                }
            });
                i++;
            }

            users.ForEach(x => repository.Save(x));

            var menus = new List<Menus>()
            {
                new Menus()
                {
                    Id= 100,
                    DisplayName = "Menus",
                    Uri = "/Menus/index",
                    Children = new List<Menus>()
                    {
                        new Menus()
                        {
                               DisplayName = "Menus_1",
                               Uri = "/Menus/index",
                        }
                    }
                }
            };

            menus.ForEach(x => repository.Save(x));
            if (!repository.Get<Folder>().Execute().Any())
            {
                var folders = new List<Folder>()
            {
                new Folder()
                {
                    Name= "Root",
                    IsSystem= true,
                    FolderType = EnumHelper.FolderTypes.ROOT
                }
            };

                folders.ForEach(x => repository.Save(x));
            }
            base.ExecuteMigration(repository);
        }
    }
}
