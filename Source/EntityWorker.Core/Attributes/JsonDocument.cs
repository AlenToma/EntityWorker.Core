using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Save the property as Json object in the database
    /// For the moment those values cant be searched by linq.
    /// you will have to use row sql to seach them
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsonDocument : Attribute
    {

    }
}
