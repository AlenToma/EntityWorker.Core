using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityWorker.Core.Postgres
{
    interface ICancelable : IDisposable
    {
        void Cancel();
    }
}
