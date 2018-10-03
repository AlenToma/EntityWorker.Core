using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using System;

namespace EntityWorker.Core.Object.Library.Modules
{
    /// <summary>
    /// Configrate you class here, add forgenKeys or Primary Keys so on.
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// </summary>
    public sealed class ModuleBuilder : IModuleBuilder
    {
        internal ModuleBuilder(DataBaseTypes dataBaseTypes)
        {
            DataBaseType = dataBaseTypes;
        }

        /// <summary>
        /// Providers type
        /// </summary>
        public DataBaseTypes DataBaseType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IObjectMapps<T> Entity<T>() where T : class
        {
            return new ObjectMapps<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ITypeMapps EntityType(Type objectType)
        {
            return new TypeMapps(objectType);
        }
    }
}
