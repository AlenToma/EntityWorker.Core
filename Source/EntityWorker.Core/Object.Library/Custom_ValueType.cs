using System;
using System.Collections.Generic;
namespace EntityWorker.Core.Object.Library
{
    public class Custom_ValueType<T, P> : Dictionary<T, P>
    {
        public Custom_ValueType(Dictionary<T, P> dic = null)
        {
            if (dic != null)
                foreach (var item in dic)
                    TryAdd(item.Key, item.Value);
        }

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

        public P Get(T key)
        {
            object o = null;
            if (this.ContainsKey(key))
                return this[key];
            return (P)o;
        }
    }
}
