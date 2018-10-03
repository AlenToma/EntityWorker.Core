using EntityWorker.Core.Helper;
using System;

namespace EntityWorker.Core.Interface
{
    /// <summary>
    /// Configrate you class here, add forgenKeys or Primary Keys so on.
    /// </summary>
    public interface IModuleBuilder
    {
        /// <summary>
        /// Provider type
        /// </summary>
        DataBaseTypes DataBaseType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IObjectMapps<T> Entity<T>() where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ITypeMapps EntityType(Type objectType);
    }
}
