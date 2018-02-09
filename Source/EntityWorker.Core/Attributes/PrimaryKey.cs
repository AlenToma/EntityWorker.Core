using System;

namespace EntityWorker.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class PrimaryKey : Attribute
    {
        public bool AutoGenerate { get; private set; }
        /// <summary>
        ///  Primary Id Type can be string, Guid or numeric eg int or long
        /// </summary>
        /// <param name="autoGenerate"> AutoGenerate Primary_Id.
        ///  When Property is NullOrEmpty or 0 it will autogenerate it anyway 
        /// </param>
        public PrimaryKey(bool autoGenerate = true)
        {
            AutoGenerate = autoGenerate;
        }
    }
}
