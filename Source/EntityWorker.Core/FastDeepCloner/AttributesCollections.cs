using EntityWorker.Core.Object.Library;
using System;
using System.Collections.Generic;

namespace EntityWorker.Core.FastDeepCloner
{
    public class AttributesCollections : List<Attribute>
    {
        internal Custom_ValueType<Attribute, Attribute> ContainedAttributes = new Custom_ValueType<Attribute, Attribute>();
        internal Custom_ValueType<Type, Attribute> ContainedAttributestypes = new Custom_ValueType<Type, Attribute>();

        public AttributesCollections(List<Attribute> attrs)
        {
            if (attrs == null)
                return;
            foreach(Attribute attr in attrs)
            {
                ContainedAttributes.GetOrAdd(attr, attr);
                ContainedAttributestypes.GetOrAdd(attr.GetType(), attr);
                base.Add(attr);
            }
          
        }

        public new void Add(Attribute attr)
        {
            ContainedAttributes.GetOrAdd(attr, attr, true);
            ContainedAttributestypes.GetOrAdd(attr.GetType(), attr, true);
            base.Add(attr);
        }

        public new void Remove(Attribute attr)
        {
            this.Remove(attr);
            ContainedAttributes.Remove(attr);
            ContainedAttributestypes.Remove(attr.GetType());
        }

    }
}
