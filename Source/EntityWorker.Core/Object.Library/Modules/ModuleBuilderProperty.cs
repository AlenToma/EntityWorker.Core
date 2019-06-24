using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using FastDeepCloner;
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
    public sealed class ModuleBuilderProperty<T> where T : class
    {
        private IFastDeepClonerProperty _property;

        private Type _objectType;

        internal ModuleBuilderProperty(IFastDeepClonerProperty property)
        {
            _property = property;
            _objectType = typeof(T);
        }

        internal ModuleBuilderProperty(IFastDeepClonerProperty property, Type objectType)
        {
            _property = property;
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
        /// Add Rules to property
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> Property<P>(Expression<Func<T, P>> action)
        {
            var prop = DeepCloner.GetProperty(_objectType, Extension.GetMemberName(action));
            if (prop == null)
                throw new EntityException($"Could not find Property{Extension.GetMemberName(action)}");

            return new ModuleBuilderProperty<T>(prop);
        }


        /// <summary>
        /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasJsonIgnore()
        {
            _property.Add(new JsonIgnore());
            return this;
        }


        /// <summary>
        /// Add JsonDocument for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasJsonDocument()
        {
            //var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            //if (prop == null)
            //    throw new EntityException($"Could not find Property{Extension.GetMemberName(action)}");
            _property.Add(new JsonDocument());
            return this;
        }



        /// <summary>
        /// Add XmlDocument for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasXmlDocument()
        {
            _property.Add(new XmlDocument());
            return this;
        }



        /// <summary>
        /// EntityWorker will ignore serializing or derializing all properties that contain this attribute
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasXmlIgnore()
        {
            _property.Add(new XmlIgnore());
            return this;
        }

        /// <summary>
        /// Use this when you have types that are unknown like interface wich it can takes more than one type 
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasKnownType(Type objectType)
        {
            _property.Add(new KnownType(objectType));
            return this;
        }


        /// <summary>
        /// Add Primary Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="autoGenerate"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasPrimaryKey(bool autoGenerate = true)
        {
            _property.Add(new PrimaryKey(autoGenerate));
            return this;
        }


        /// <summary>
        /// Add Foreign Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="Source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasForeignKey<Source>(string propertyName = null) where Source : class
        {
            if (!_property.IsInternalType || (_property.PropertyType != typeof(string) && !_property.PropertyType.IsNumeric() && _property.PropertyType != typeof(Guid) && _property.PropertyType != typeof(Guid?)))
                throw new EntityException($"ForeignKey for Property { _property.Name } kan only be of type string or numeric or Guid");
            _property.Add(new ForeignKey(typeof(Source), propertyName));
            return this;
        }


        /// <summary>
        /// Assign a diffrent database type fot the property
        /// Attibutes Stringify, DataEncode and ToBase64String will override this attribute. 
        /// </summary>
        /// <param name="dataType">The database type ex nvarchar(4000)</param>
        /// <param name="dataBaseTypes">null for all providers</param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasColumnType(string dataType, DataBaseTypes? dataBaseTypes = null)
        {
            if (!_property.Attributes.Any(x => x is ColumnType && ((ColumnType)x).DataBaseTypes == dataBaseTypes))
                _property.Add(new ColumnType(dataType, dataBaseTypes));
            return this;
        }

        /// <summary>
        /// Add DataEncode for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="key"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasDataEncode(string key = null, DataCipherKeySize keySize = DataCipherKeySize.Default)
        {
            _property.Add(new DataEncode(key, keySize));
            return this;
        }

        /// <summary>
        /// Add IndependentData for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasIndependentData()
        {
            _property.Add(new IndependentData());
            return this;
        }

        /// <summary>
        /// Add NotNullable for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> NotNullable()
        {
            _property.Add(new NotNullable());
            return this;
        }

        /// <summary>
        /// Add PropertyName for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasPropertyName(string name, string displayName = null)
        {
            _property.Add(new PropertyName(name, displayName));
            return this;
        }

        /// <summary>
        /// Add Stringify for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasStringify()
        {
            _property.Add(new Stringify());
            return this;
        }

        /// <summary>
        /// Add ExcludeFromAbstract for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> ExcludeFromAbstract()
        {
            _property.Add(new ExcludeFromAbstract());
            return this;
        }

        /// <summary>
        /// Add DefaultOnEmpty for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"> Fefault value when property is null</param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasDefaultOnEmpty(object value)
        {
            _property.Add(new DefaultOnEmpty(value));
            return this;
        }


        /// <summary>
        /// Add ToBase64String for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public ModuleBuilderProperty<T> HasToBase64String()
        {
            _property.Attributes.Add(new ToBase64String());
            return this;
        }

    }
}
