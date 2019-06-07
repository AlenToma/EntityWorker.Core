using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EntityWorker.Core.Object.Library.Modules
{
    /// <summary>
    /// Entity Configration
    /// Here we could configrate and set all primary keys and Foreign keys for properties
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ObjectMapps<T> where T : class
    {
        internal ObjectMapps()
        {

        }

        /// <summary>
        /// Add Rules to property
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> Property<P>(Expression<Func<T, P>> action)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null)
                throw new EntityException($"Could not find Property{Extension.GetMemberName(action)}");

            return new ModuleBuilderProperty<T>(prop);
        }

        /// <summary>
        /// Assign diffrent name for the object in the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ObjectMapps<T> TableName(string name, string schema = null)
        {
            Extension.CachedTableNames.GetOrAdd(typeof(T), new Table(name.CleanName(), schema), true);
            return this;
        }

        /// <summary>
        /// Assign a rule to object
        /// </summary>
        /// <typeparam name="Source">Must implement interface IDbRuleTrigger</typeparam>
        /// <returns></returns>
        public ObjectMapps<T> HasRule<Source>()
        {
            if (typeof(Source).GetInterfaces().Length <= 0 || !typeof(Source).GetInterfaces().Any(x => x.ToString().Contains("IDbRuleTrigger") && x.ToString().Contains(typeof(T).FullName)))
                throw new EntityException($"Source dose not implement interface IDbRuleTrigger<{typeof(T).Name }>");
            var rule = typeof(Source).CreateInstance();
            DbSchema.CachedIDbRuleTrigger.GetOrAdd(typeof(T), rule);
            return this;
        }

    }
}
