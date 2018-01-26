using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LightData.CMS.Modules.Helper
{
    public static class Methods
    {

        public static string Encode(string password)
        {
            var hash = System.Security.Cryptography.SHA1.Create();
            var encoder = new System.Text.ASCIIEncoding();
            var combined = encoder.GetBytes(password ?? "");
            return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
        }

        public static List<RegionInfo> GetCountriesByIso3166()
        {
            var countries = new List<RegionInfo>();
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var country = new RegionInfo(culture.LCID);
                if (countries.All(p => p.Name != country.Name))
                    countries.Add(country);
            }
            return countries.OrderBy(p => p.EnglishName).ToList();
        }

        //public static List<string> GetTheme(string exp = null)
        //{
        //    var currentTheme = ConfigurationManager.AppSettings["Theme"];
        //    var bin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
        //    var directory = Directory.GetDirectories(bin);
        //    var themePath = directory.First(x => x.Contains("Theme"));

        //    var files = Directory.GetFiles(Path.Combine(themePath, currentTheme), "*", SearchOption.AllDirectories).ToList();
        //    if (exp != null)
        //        return files.FindAll(x => x.ToLower().EndsWith(exp));
        //    return files;
        //}
    }
}