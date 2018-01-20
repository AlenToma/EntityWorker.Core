using DataAccess.EntityWorker;
using DataAccess.EntityWorker.Entities.Archive;
using EntityWorker.Core.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class SqlTest
    {
        [TestMethod]
        public void AddEntityWorker()
        {
            using (var repo = new Repository())
            {
                // FileName is not nullable, make sure to specify it 
                // Primary Id cant be set for Image, This will be set automaticly. you cant specify the Id. EntityWorker will consider this as an update otherwise.
                var tag = new Tag { Filename = "test", Interpret = "TestInterpret", Title = "Title", Image = new Image { Data = new byte[10] } };
                repo.Save(tag);
                repo.SaveChanges(); // Commit to the database 
            }

        }



        [TestMethod]
        public void GetAllEntityWorker()
        {
            using (var repo = new Repository())
            {
                var data = repo.GetAll<Tag>();
                Assert.AreNotEqual(0, data.Count());
            }
        }

        [TestMethod]
        public void GetAllEntityWorkerWithChildren()
        {
            using (var repo = new Repository())
            {
                var data = repo.Get<Tag>().LoadChildren(x => x.Image).Execute();
                Assert.AreNotEqual(0, data.Count());
            }
        }

        [TestMethod]
        public void AddEFramework()
        {
            using (var repo = new ERepository())
            {
                // FileName is not nullable, make sure to specify it 
                // Primary Id cant be set for Image, This will be set automaticly. you cant specify the Id. EntityWorker will consider this as an update otherwise.
                var tag = new Tag { Filename = "tesdst", Interpret = "TestInterpret", Title = "Title", Image = new Image { Data = new byte[10] } };
                repo.Tag.Add(tag);
                repo.SaveChanges(); // Comit to the database 
            }
        }

     
  
        [TestMethod]
        public void GetAllEntityFrameWork()
        {
            using (var repo = new ERepository())
            {
                var data = repo.Tag.ToList();
                Assert.AreNotEqual(0, data.Count());
            }
        }


        [TestMethod]
        public void GetAllEntityFrameWorkWithChildren()
        {
            using (var repo = new ERepository())
            {
                var data = repo.Tag.Include(x=> x.Image).ToList();
                Assert.AreNotEqual(0, data.Count());
            }
        }


        [TestMethod]

        public void Delete()
        {
            using (var repo = new Repository())
            {
                repo.Get<Tag>().Execute().ForEach(x => repo.Delete(x));
                repo.SaveChanges();
                var count = repo.Get<Tag>().ExecuteCount();
                Assert.AreEqual(count, 0);
            }
        }
    }
}
