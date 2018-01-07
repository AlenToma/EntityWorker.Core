using System;
using System.Collections.Generic;
using System.Text;

namespace EntityWorker.Core.Object.Library
{
    public class EntityChanges
    {
        public string PropertyName { get; internal set; }

        public object NewValue { get; internal set; }

        public object OldValue { get; internal set; }


    }
}
