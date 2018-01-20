using DataAccess.EntityWorker.Entities.Archive;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityWorker
{
    public class ERepository : DbContext
    {
        public IDbSet<Tag> Tag { get; set; }

        public IDbSet<Image> Image { get; set; }

        public ERepository(): base(@"Server=.\SQLEXPRESS; Database=EftestDB; User Id=root; Password=root;")
        {

        }
    }
}
