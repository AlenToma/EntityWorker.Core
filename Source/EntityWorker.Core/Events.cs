using EntityWorker.Core.FastDeepCloner;
using System.Data;

namespace EntityWorker.Core
{
    public abstract class Events
    {
        public delegate void IdChanged(long id);

        internal delegate void PropetySetter(IDataReader reader, int col, IFastDeepClonerProperty prop, object item);
    }
}
