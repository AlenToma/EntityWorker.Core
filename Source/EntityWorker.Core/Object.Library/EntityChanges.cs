namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// DataChanges
    /// </summary>
    public class EntityChanges
    {
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
