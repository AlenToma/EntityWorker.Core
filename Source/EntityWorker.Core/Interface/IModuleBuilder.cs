using EntityWorker.Core.Helper;
using EntityWorker.Core.Object.Library.Modules;
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
        ObjectMapps<T> Entity<T>() where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TypeMapps EntityType(Type objectType);
    }
}
