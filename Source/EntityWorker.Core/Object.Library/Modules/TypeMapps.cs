using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using FastDeepCloner;
using System;
using System.Linq;

namespace EntityWorker.Core.Object.Library.Modules
{
    public sealed class TypeMapps
    {
        private Type _objectType;
        internal TypeMapps(Type objectType)
        {
            _objectType = objectType;
        }

        /// <summary>
        /// Add Rules to property
        /// </summary>
        /// <param name="name">propertyName</param>
        /// <returns></returns>
        public ModuleBuilderProperty<object> Property(string name)
        {
            var prop = DeepCloner.GetProperty(_objectType, name);
            if (prop == null)
                throw new EntityException($"Could not find Property{name} in object {_objectType.FullName}");
            return new ModuleBuilderProperty<object>(prop, _objectType);
        }

        /// <summary>
        /// Assign diffrent name for the object in the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="schema">eg. dbo</param>
        /// <returns></returns>
        public TypeMapps TableName(string name, string schema = null)
        {
            Extension.CachedTableNames.GetOrAdd(_objectType, new Table(name.CleanName(), schema), true);
            return this;
        }

        /// <summary>
        /// Assign a rule to object
        /// </summary>
        /// <typeparam name="ruleType">Must implement interface IDbRuleTrigger</typeparam>
        /// <returns></returns>
        public TypeMapps HasRule(Type ruleType)
        {
            if (ruleType.GetInterfaces().Length <= 0 || !ruleType.GetInterfaces().Any(x => x.ToString().Contains("IDbRuleTrigger") && x.ToString().Contains(ruleType.FullName)))
                throw new EntityException($"ruleType dose not implement interface IDbRuleTrigger<{ruleType.Name }>");
            var rule = ruleType.CreateInstance();
            DbSchema.CachedIDbRuleTrigger.GetOrAdd(ruleType, rule);
            return this;
        }

    }
}
