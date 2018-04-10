using System;

namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// DataChanges
    /// </summary>
    public sealed class EntityChanges
    {

        /// <summary>
        /// Entity Type
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// PropertyType
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        /// The new Value
        /// </summary>
        public object NewValue { get; internal set; }

        /// <summary>
        /// Old Value
        /// </summary>
        public object OldValue { get; internal set; }
    }
}
