using System.Collections.Generic;
namespace EntityWorker.Core.Object.Library
{
    public class Custom_ValueType<T, P> : Dictionary<T, P>
    {

        public bool TryAdd(T key, P item, bool overwrite = false)
        {
            if (base.ContainsKey(key) && !overwrite)
                return true;
            lock (this)
            {
                if (overwrite && this.ContainsKey(key))
                    this.Remove(key);
                if (!base.ContainsKey(key))
                    base.Add(key, item);

            }
            return true;
        }
        public P GetOrAdd(T key, P item, bool overwrite = false)
        {
            if (base.ContainsKey(key) && !overwrite)
                return base[key];
            lock (this)
            {
                if (overwrite && this.ContainsKey(key))
                    this.Remove(key);
                if (!base.ContainsKey(key))
                    base.Add(key, item);

            }
            return base[key];
        }
    }
}
