using System;

namespace EntityWorker.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class PropertyName : Attribute
    {
        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public PropertyName() { }

        public PropertyName(string name, string displayName = null)
        {
            Name = name;
            DisplayName = displayName ?? Name;
        }
    }
}
