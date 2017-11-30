using System;
using System.Collections.Generic;
using System.Text;

namespace EntityWorker.Core
{
    public abstract class Events
    {
        public delegate void IdChanged(long id);
    }
}
