using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// This attribute is most used on properties with type string
    /// in-case we don't want them to be null able
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class NotNullable : Attribute
    {
    }
}
