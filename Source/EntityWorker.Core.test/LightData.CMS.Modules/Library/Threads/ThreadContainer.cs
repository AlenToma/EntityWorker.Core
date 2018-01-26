using System.Collections.Generic;
using System.Linq;

namespace LightData.CMS.Modules.Library.Threads
{
    public class ThreadContainer : List<ThreadItem>
    {

        public void Start()
        {
            this.ForEach(x => x.Start());
        }

        public void Wait()
        {
            while (this.Any(x => x.IsRunning()))
            {

            }
        }
    }
}
