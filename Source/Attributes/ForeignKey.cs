using System;

namespace EntityWorker.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Property is a ForeignKey in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ForeignKey : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Type Type { get; private set; }

        public string PropertyName { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal ForeignKey() { }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">Map this key to propertyName</param>
        public ForeignKey(Type type, string propertyName = null)
        {
            Type = type;
            PropertyName = propertyName;
        }


    }
}
