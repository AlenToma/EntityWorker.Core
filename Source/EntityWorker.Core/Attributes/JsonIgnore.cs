using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// Ignore serialize to json
    /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonIgnore : Attribute
    {
    }
}
