using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Assign Default Value when Property is null
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultOnEmpty : Attribute
    {
        public object Value { get; set; }
        /// <summary>
        /// add Default Value when Property is null
        /// </summary>
        /// <param name="value"></param>
        public DefaultOnEmpty(object value)
        {
            Value = value;
        }
    }
}
