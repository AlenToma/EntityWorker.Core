using LightData.CMS.Modules.Repository;
using LightData.CMS.Modules.Library;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using EntityWorker.Core.Helper;
using EntityWorker.Core;
using LightData.CMS.Modules.Library.Internal;
using EntityWorker.Core.Interface;

namespace ConsoleApp1
{
    class Program
    {
        private static Stopwatch sw = new Stopwatch();
        static void Main(string[] args)
        {
            GlobalConfiguration.Log = new Logger((object sender, EventArgs arg) =>
            {
                //Console.WriteLine((arg as Args).Data);
            });
            //test();
            DynamicLinq();
            SaveJson();
            PackageTest();
            TestSave();
            ExpressionTest();
            Console.ReadLine();
            Console.Clear();
            Main(null);

        }

        public static void test()
        {
            using (var rep = new Repository())
            {
                var strList = new List<string>() { "Alen", "Toma" };
                var data =rep.Get<User>().Where(x =>  !x.IsActive2.HasValue && x.IsActive2.HasValue == true);
                var users = data.ExecuteFirstOrDefault();
                var sql = data.ParsedLinqToSql;
            }
        }

        public static void DynamicLinq()
        {
            using (var rep = new Repository())
            {
                var id = Guid.NewGuid();

                execute(rep.Get<User>().Where("x.Person.FirstName.EndsWith(\"n\") And (x.Person.FirstName.Contains(\"a\") OR x.Person.FirstName.StartsWith(\"a\"))").LoadChildren(), "Dynamic exp");

            }
        }

        public static void start()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        public static void SaveJson()
        {
            Console.WriteLine("test jsonValue Attributes");

            Console.WriteLine("Mssql");
            using (var rep = new Repository(DataBaseTypes.Mssql))
            {
                var user = rep.Get<User>().LoadChildren().ExecuteFirstOrDefault();
                var jsonUser = new UserTemp()
                {
                    User = user,
                    UserXml = user,
                };
                rep.Save(jsonUser);

                var userJson = rep.Get<UserTemp>().ExecuteFirstOrDefault();

                jsonUser.User.UserName = "test";
                rep.Save(jsonUser);
                rep.SaveChanges();
                userJson = rep.Get<UserTemp>().ExecuteFirstOrDefault();
                Console.WriteLine((jsonUser.User.UserName == "test" ? "Success" : "Failed"));
            }

            Console.WriteLine("PostgreSql");
            using (var rep = new Repository(DataBaseTypes.PostgreSql))
            {
                var user = rep.Get<User>().LoadChildren().ExecuteFirstOrDefault();
                var jsonUser = new UserTemp()
                {
                    User = user,
                    UserXml = user,
                };
                rep.Save(jsonUser);

                var userJson = rep.Get<UserTemp>().ExecuteFirstOrDefault();

                jsonUser.User.UserName = "test";
                rep.Save(jsonUser);
                rep.SaveChanges();
                userJson = rep.Get<UserTemp>().ExecuteFirstOrDefault();
                Console.WriteLine((jsonUser.User.UserName == "test" ? "Success" : "Failed"));
            }

            Console.WriteLine("Sqlite");
            using (var rep = new Repository(DataBaseTypes.Sqllight))
            {
                var user = rep.Get<User>().LoadChildren().ExecuteFirstOrDefault();
                var jsonUser = new UserTemp()
                {
                    User = user,
                    UserXml = user,
                };
                rep.Save(jsonUser);

                var userJson = rep.Get<UserTemp>().ExecuteFirstOrDefault();

                jsonUser.User.UserName = "test";
                rep.Save(jsonUser);
                rep.SaveChanges();
                userJson = rep.Get<UserTemp>().ExecuteFirstOrDefault();
                Console.WriteLine((jsonUser.User.UserName == "test" ? "Success" : "Failed"));
            }
        }



        public static void stop()
        {
            sw.Stop();
            Console.WriteLine("Time taken: {0}ms", sw.Elapsed.TotalMilliseconds);
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
                Console.WriteLine("Success Count:" + r.Count);
                Console.WriteLine(" ");
                var sql = q.ParsedLinqToSql;
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


        private static void PackageTest()
        {
            Console.WriteLine("----------------Package Handler Create Package------------------");
            using (var rep = new Repository(DataBaseTypes.PostgreSql))
            {
                var users = rep.Get<User>().LoadChildren().Execute();
                var package = rep.CreatePackage(new Package() { Data = users.Cast<object>().ToList() });
                var readerPackage = rep.GetPackage<Package>(package);
                Console.WriteLine((readerPackage.Data.Count <= 0 ? "Failed" : "Success"));
            }
        }

        public static void TestSave()
        {


            Console.WriteLine("----------------Postgresql------------------");
            using (var rep = new Repository(DataBaseTypes.PostgreSql))
            {

                var role = rep.Get<Role>().ExecuteFirstOrDefault();

                var user = new User("") { UserName = "test", Password = "test", Role = role, Person = new Person() { FirstName = "asd" } };
                Console.WriteLine("----------------Roleback Test------------------");
                rep.Save(user);
                rep.Rollback();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() > 0 ? "Failed" : "Success"));
                Console.WriteLine(" ");

                user = new User("") { UserName = "test", Password = "test", Role = role, Person = new Person() { FirstName = "asd" } };
                Console.WriteLine("----------------SaveChanges Test------------------");
                rep.Save(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() > 0 ? "Success" : "Failed"));
                Console.WriteLine(" ");


                user = rep.Get<User>().Where(x => x.Id == user.Id).ExecuteFirstOrDefault();

                user.UserName = "aalen";
                Console.WriteLine("----------------UpdateTest Test------------------");
                rep.Save(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteFirstOrDefault().UserName == "aalen" ? "Success" : "Failed"));
                Console.WriteLine(" ");

                user = rep.Get<User>().Where(x => x.Id == user.Id).LoadChildren().ExecuteFirstOrDefault();
                //var test = rep.Get<User>().LoadChildren().Execute().ToType<List<User>>();
                Console.WriteLine("----------------Remove Test------------------");
                rep.Delete(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() <= 0 ? "Success" : "Failed"));
            }

            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");


            Console.WriteLine("----------------MSSQL------------------");
            using (var rep = new Repository())
            {
                var role = rep.Get<Role>().ExecuteFirstOrDefault();

                var user = new User("") { UserName = "test", Password = "test", Role = role, Person = new Person() { FirstName = "asd" } };
                Console.WriteLine("----------------Roleback Test------------------");
                rep.Save(user);
                rep.Rollback();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() > 0 ? "Failed" : "Success"));
                Console.WriteLine(" ");

                user = new User("") { UserName = "test", Password = "test", Role = role, Person = new Person() { FirstName = "asd" } };
                Console.WriteLine("----------------SaveChanges Test------------------");
                rep.Save(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() > 0 ? "Success" : "Failed"));
                Console.WriteLine(" ");


                user = rep.Get<User>().Where(x => x.Id == user.Id).ExecuteFirstOrDefault();

                user.UserName = "alen";
                Console.WriteLine("----------------UpdateTest Test------------------");
                rep.Save(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteFirstOrDefault().UserName == "alen" ? "Success" : "Failed"));
                Console.WriteLine(" ");

                user = rep.Get<User>().Where(x => x.Id == user.Id).LoadChildren().ExecuteFirstOrDefault();
                //var test = rep.Get<User>().LoadChildren().Execute().ToType<List<User>>();
                Console.WriteLine("----------------Remove Test------------------");
                rep.Delete(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() <= 0 ? "Success" : "Failed"));
            }

            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");

            Console.WriteLine("----------------SQLITE------------------");
            using (var rep = new Repository(EntityWorker.Core.Helper.DataBaseTypes.Sqllight))
            {
                var role = rep.Get<Role>().ExecuteFirstOrDefault();
                var user = new User("") { UserName = "test", Password = "test", Role = role, Person = new Person() { FirstName = "asd" } };
                Console.WriteLine("----------------Roleback Test------------------");
                rep.Save(user);
                rep.Rollback();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() > 0 ? "Failed" : "Success"));
                Console.WriteLine(" ");

                user = new User("") { UserName = "test", Password = "test", Role = role, Person = new Person() { FirstName = "asd" } };
                Console.WriteLine("----------------SaveChanges Test------------------");
                rep.Save(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() > 0 ? "Success" : "Failed"));
                Console.WriteLine(" ");


                user = rep.Get<User>().Where(x => x.Id == user.Id).ExecuteFirstOrDefault();
                user.UserName = "alen";
                Console.WriteLine("----------------UpdateTest Test------------------");
                rep.Save(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteFirstOrDefault().UserName == "alen" ? "Success" : "Failed"));
                Console.WriteLine(" ");

                user = rep.Get<User>().Where(x => x.Id == user.Id).LoadChildren().ExecuteFirstOrDefault();
                Console.WriteLine("----------------Remove Test------------------");
                rep.Delete(user);
                rep.SaveChanges();
                Console.WriteLine((rep.Get<User>().Where(x => x.Id == user.Id).ExecuteCount() <= 0 ? "Success" : "Failed"));
            }
            Console.WriteLine(" ");
            Console.WriteLine(" ");
        }

        public static void ExpressionTest()
        {
            Console.WriteLine("POSTGRESQL");
            using (var rep = new Repository(DataBaseTypes.PostgreSql))
            {
                var id = Guid.NewGuid();

                var json = rep.Get<User>().LoadChildren().Json();
                var test = rep.FromJson<User>(json);

                //var xml = rep.Get<User>().LoadChildren().Xml();
                //var test = rep.FromXml<User>(xml);

                execute(rep.Get<User>().LoadChildren(), "Get All");
                execute(rep.Get<Person>().Where(x => x.FirstName.Contains("Admin") || !string.IsNullOrEmpty(x.FirstName) || string.IsNullOrEmpty(x.FirstName) == false && x.Id != id), "IsNullOrEmpty");

                execute(rep.DataReaderConverter<User>(rep.GetSqlCommand("select * from geto.Users")).LoadChildren(), "CustomSelect");

                execute(rep.Get<Person>().Where(x => x.Addresses.Any(a => a.Country.Name.Contains("US") && !string.IsNullOrEmpty(a.PostalCode))), "Any");

                execute(rep.Get<Person>().Where(x => x.FirstName.EndsWith("a")), "EndsWith");
                var strList = new List<string>() { "Alen", "Toma" };
                execute(rep.Get<Person>().Where(x => strList.Contains(x.FirstName) || !strList.Contains(x.FirstName)), "EndsWith");

                execute(rep.Get<Person>().Where(x => x.FirstName.StartsWith("a") || !x.FirstName.StartsWith("b") && x.FirstName.StartsWith("b") == false), "StartsWith");

            }

            Console.WriteLine(" ");
            Console.WriteLine(" ");

            Console.WriteLine("MSQLtEST");
            using (var rep = new Repository())
            {
                var id = Guid.NewGuid();
                execute(rep.Get<User>().LoadChildren(), "Get All");
                execute(rep.Get<Person>().Where(x => x.FirstName.Contains("Admin") || !string.IsNullOrEmpty(x.FirstName) || string.IsNullOrEmpty(x.FirstName) == false && x.Id != id), "IsNullOrEmpty");


                execute(rep.DataReaderConverter<User>(rep.GetSqlCommand("select * from geto.Users")).LoadChildren(), "CustomSelect");


                execute(rep.Get<Person>().Where(x => x.Addresses.Any(a => a.Country.Name.Contains("US") && !string.IsNullOrEmpty(a.PostalCode))), "Any");

                execute(rep.Get<Person>().Where(x => x.FirstName.EndsWith("a")), "EndsWith");
                var strList = new List<string>() { "Alen", "Toma" };
                execute(rep.Get<Person>().Where(x => strList.Contains(x.FirstName) || !strList.Contains(x.FirstName)), "EndsWith");

                execute(rep.Get<Person>().Where(x => x.FirstName.StartsWith("a") || !x.FirstName.StartsWith("b") && x.FirstName.StartsWith("b") == false), "StartsWith");

            }

            Console.WriteLine(" ");
            Console.WriteLine(" ");

            Console.WriteLine("SQLLITETEST");
            using (var rep = new Repository(EntityWorker.Core.Helper.DataBaseTypes.Sqllight))
            {
                execute(rep.Get<User>().LoadChildren(), "Get All");

                execute(rep.Get<Person>().Where(x => x.FirstName.Contains("Admin") || !string.IsNullOrEmpty(x.FirstName) || string.IsNullOrEmpty(x.FirstName) == false), "IsNullOrEmpty");

                execute(rep.DataReaderConverter<User>(rep.GetSqlCommand("select * from Users")).LoadChildren(), "CustomSelect");

                execute(rep.Get<Person>().Where(x => x.Addresses.Any(a => a.Country.Name.Contains("US") && !string.IsNullOrEmpty(a.PostalCode))), "Any");

                execute(rep.Get<Person>().Where(x => x.FirstName.EndsWith("a")), "EndsWith");
                var strList = new List<string>() { "Alen", "Toma" };
                execute(rep.Get<Person>().Where(x => strList.Contains(x.FirstName) || !strList.Contains(x.FirstName)), "EndsWith");

                execute(rep.Get<Person>().Where(x => x.FirstName.StartsWith("a") || !x.FirstName.StartsWith("b") && x.FirstName.StartsWith("b") == false), "StartsWith");
            }
        }

    }
}
