using System;

namespace EntityWorker.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Will be saved to the database as base64string 
    /// and converted back to its original string when its read
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ToBase64String : Attribute
    {
    }
}
