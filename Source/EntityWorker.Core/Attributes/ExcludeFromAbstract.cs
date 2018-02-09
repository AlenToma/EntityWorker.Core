using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// This indicates that the prop will not be saved to the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcludeFromAbstract : Attribute
    {
    }
}