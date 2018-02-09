using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Save the property as string in the database
    /// mostly used when we don't want an enum to be saved as integer in the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Stringify : Attribute
    {
    }
}
