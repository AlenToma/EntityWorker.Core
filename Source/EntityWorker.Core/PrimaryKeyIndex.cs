using System.Collections.Generic;
using System.Linq;

namespace EntityWorker.Core
{
    internal class PrimaryKeyIndex
    {
        private Dictionary<object, LightDataTableRow> _savedIndexes = new Dictionary<object, LightDataTableRow>();

        internal LightDataTableRow this[object key] => _savedIndexes[key];

        internal bool ContainValue(object value)
        {
            return _savedIndexes.ContainsKey(value);
        }

        internal void AddValue(object oldValue, object newValue, LightDataTableRow index)
        {
            if (oldValue != null && ContainValue(oldValue))
            {
                if (ContainValue(newValue)) return;
                _savedIndexes.Remove(oldValue);
                _savedIndexes.Add(newValue, index);
            }
            else if (newValue != null && !ContainValue(newValue))
                _savedIndexes.Add(newValue, index);
        }

        /// <summary>
        /// For faster search we index all values for primaryKey.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="key"></param>
        internal void ClearAndRenderValues(List<LightDataTableRow> rows, string key)
        {
            _savedIndexes.Clear();
            if (string.IsNullOrEmpty(key)) return;
            if (rows.Any())
                _savedIndexes = rows.GroupBy(x => x[key]).Select(x => x.First()).ToList().FindAll(x => x[key] != null).ToDictionary(x => x[key], x => x);
        }
    }
}
