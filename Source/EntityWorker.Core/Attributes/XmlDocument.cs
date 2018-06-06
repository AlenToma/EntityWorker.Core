using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Save the property as Xml object in the database
    /// For the moment those values cant be searched by linq.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
   public sealed class XmlDocument : Attribute
    {
    }
}
