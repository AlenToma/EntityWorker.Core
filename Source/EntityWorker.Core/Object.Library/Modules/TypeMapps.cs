using EntityWorker.Core.Attributes;
using EntityWorker.Core.FastDeepCloner;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using System;
using System.Linq;

namespace EntityWorker.Core.Object.Library.Modules
{
    public class TypeMapps : ITypeMapps
    {
        private Type _objectType;
        internal TypeMapps(Type objectType)
        {
            _objectType = objectType;
        }

        private IFastDeepClonerProperty GetProperty(string action)
        {
            var prop = DeepCloner.GetProperty(_objectType, action);
            if (prop == null)
                throw new EntityException($"Could not find Property{action} in object {_objectType.FullName}");
            return prop;
        }

        public ITypeMapps ExcludeFromAbstract(string action)
        {
            GetProperty(action).Add(new ExcludeFromAbstract());
            return this;
        }

        public ITypeMapps HasColumnType(string action, string dataType, DataBaseTypes? dataBaseTypes = null)
        {
            GetProperty(action).Add(new ColumnType(dataType, dataBaseTypes));
            return this;
        }

        public ITypeMapps HasDataEncode(string action, string key = null, DataCipherKeySize keySize = DataCipherKeySize.Default)
        {
            GetProperty(action).Add(new DataEncode(key, keySize));
            return this;
        }

        public ITypeMapps HasDefaultOnEmpty(string action, object value)
        {
            GetProperty(action).Add(new DefaultOnEmpty(value));
            return this;
        }

        public ITypeMapps HasForeignKey<Source>(string action, string propertyName = null) where Source : class
        {
            GetProperty(action).Add(new ForeignKey(typeof(Source), propertyName));
            return this;
        }

        public ITypeMapps HasIndependentData(string action)
        {
            GetProperty(action).Add(new IndependentData());
            return this;
        }

        public ITypeMapps HasJsonDocument(string action)
        {
            GetProperty(action).Add(new JsonDocument());
            return this;
        }

        public ITypeMapps HasJsonIgnore(string action)
        {
            GetProperty(action).Add(new JsonIgnore());
            return this;
        }

        public ITypeMapps HasKnownType(string action, Type objectType)
        {
            GetProperty(action).Add(new KnownType(objectType));
            return this;
        }

        public ITypeMapps HasPrimaryKey(string action, bool autoGenerate = true)
        {
            GetProperty(action).Add(new PrimaryKey(autoGenerate));
            return this;
        }

        public ITypeMapps HasPropertyName(string action, string name, string displayName = null)
        {
            GetProperty(action).Add(new PropertyName(name, displayName));
            return this;
        }

        public ITypeMapps HasRule<Source>()
        {
            if (typeof(Source).GetInterfaces().Length <= 0 || !typeof(Source).GetInterfaces().Any(x => x.ToString().Contains("IDbRuleTrigger") && x.ToString().Contains(_objectType.FullName)))
                throw new EntityException($"Source dose not implement interface IDbRuleTrigger<{_objectType.FullName }>");
            var rule = typeof(Source).CreateInstance();
            DbSchema.CachedIDbRuleTrigger.GetOrAdd(_objectType, rule);
            return this;
        }

        public ITypeMapps HasStringify(string action)
        {
            GetProperty(action).Add(new Stringify());
            return this;
        }

        public ITypeMapps HasToBase64String(string action)
        {
            GetProperty(action).Add(new ToBase64String());
            return this;
        }

        public ITypeMapps HasXmlDocument(string action)
        {
            GetProperty(action).Add(new XmlDocument());
            return this;
        }

        public ITypeMapps HasXmlIgnore(string action)
        {
            GetProperty(action).Add(new XmlIgnore());
            return this;
        }

        public ITypeMapps NotNullable(string action)
        {
            GetProperty(action).Add(new NotNullable());
            return this;
        }

        public ITypeMapps TableName(string name, string schema = null)
        {
            Extension.CachedTableNames.GetOrAdd(_objectType, new Table(name.CleanName(), schema), true);
            return this;
        }
    }
}
