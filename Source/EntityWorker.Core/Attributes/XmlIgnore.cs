using System;
using System.Collections.Generic;
using System.Text;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// Ignore serialize to xml
    /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlIgnore : Attribute
    {
    }
}
