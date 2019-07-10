using EntityWorker.Core.Helper;
using LightData.CMS.Modules.Library;
using LightData.CMS.Modules.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp.Core
{
    class Program
    {
        private static Stopwatch sw = new Stopwatch();
        static void Main(string[] args)
        {

            ExpressionTest();
            Console.ReadLine();
            Main(null);
        }

        private static void ExpressionTest()
        {
            using (var rep = new Repository(DataBaseTypes.Sqllight))
            {

                ////execute(rep.Get<User>().Where(x => string.IsNullOrEmpty(x.UserName)), "IsNullOrEmpty");
                //execute(rep.Get<User>().Where(x => string.IsNullOrEmpty(x.Person.FirstName)), "IsNullOrEmpty");
                Guid? id = Guid.NewGuid();

                //execute(rep.Get<Person>().Where(x => id == null || id == x.Id), "test");
                execute(rep.Get<Person>().Where(x => x.FirstName.Contains("Admin") || string.IsNullOrEmpty(x.FirstName) || string.IsNullOrEmpty(x.FirstName) == false && x.Id != id), "!IsNullOrEmpty");
            }

        }

        public static void start()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        public static void stop()
        {
            sw.Stop();
            Console.WriteLine("Time taken: {0}s", sw.Elapsed.TotalSeconds);
            sw = new Stopwatch();
        }
        private static List<object> usResult = null;
        public static void execute(dynamic q, string identifier)
        {
            try
            {
                Console.WriteLine("----------------" + identifier + "------------------");
                start();
                var r = q.Execute();
                Console.WriteLine("Success");
                Console.WriteLine(" ");
                stop();
            }
            catch (Exception ex)
            {
                stop();
                Console.WriteLine("Failed ={0}\n\n{1}", q.ParsedLinqToSql, ex.Message);
                Console.WriteLine(" ");
                Console.WriteLine(" ");
            }
        }

    }
}
