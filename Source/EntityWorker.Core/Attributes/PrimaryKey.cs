using System;

namespace EntityWorker.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class PrimaryKey : Attribute
    {
    }
}
