using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace EntityWorker.Core.Object.Library.Modules
{
    /// <summary>
    /// Object Configration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ObjectMapps<T> : IObjectMapps<T> where T : class
    {
        internal ObjectMapps()
        {

        }

        /// <summary>
        /// Assign diffrent name for the object in the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IObjectMapps<T> TableName(string name)
        {
            Extension.CachedTableNames.GetOrAdd(typeof(T), new Regex("[^a-zA-Z0-9,_]").Replace(name, "_"), true);
            return this;
        }

        /// <summary>
        /// Assign a rule to object
        /// </summary>
        /// <typeparam name="Source">Must implement interface IDbRuleTrigger</typeparam>
        /// <returns></returns>
        public IObjectMapps<T> HasRule<Source>()
        {
            if (typeof(Source).GetInterfaces().Length <= 0 || !typeof(Source).GetInterfaces().Any(x => x.ToString().Contains("IDbRuleTrigger")))
                throw new Exception("Source dose not implement interface IDbRuleTrigger");
            var rule = typeof(Source).CreateInstance();
            DbSchema.CachedIDbRuleTrigger.GetOrAdd(typeof(T), rule);
            return this;
        }


        /// <summary>
        /// Add Primary Key to Property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="autoGenerate"></param>
        /// <returns></returns>
        public IObjectMapps<T> HasPrimaryKey<TP>(Expression<Func<T, TP>> action, bool autoGenerate = true)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null || !prop.IsInternalType || (prop.PropertyType != typeof(string) && !prop.PropertyType.IsNumeric() && prop.PropertyType != typeof(Guid)))
                throw new Exception("PrimaryKey for Property " + prop?.FullName + " kan only be of type string or numeric or Guid");
            prop.Attributes.Add(new PrimaryKey(autoGenerate));
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
        public IObjectMapps<T> HasForeignKey<Source, TP>(Expression<Func<T, TP>> action, string propertyName = null) where Source : class
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null || !prop.IsInternalType || (prop.PropertyType != typeof(string) && !prop.PropertyType.IsNumeric() && prop.PropertyType != typeof(Guid)))
                throw new Exception("ForeignKey for Property " + prop?.FullName + " kan only be of type string or numeric or Guid");

            prop.Attributes.Add(new ForeignKey(typeof(Source), propertyName));
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
        public IObjectMapps<T> HasDataEncode<TP>(Expression<Func<T, TP>> action, string key = null, DataCipherKeySize keySize = DataCipherKeySize.Default)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null || prop.PropertyType != typeof(string))
                throw new Exception("DataEncode for Property " + prop?.FullName + " kan only be of type string");

            prop.Attributes.Add(new DataEncode(key, keySize));
            return this;
        }


        /// <summary>
        /// Add IndependentData for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObjectMapps<T> HasIndependentData<TP>(Expression<Func<T, TP>> action) where TP : class
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null || prop.IsInternalType)
                throw new Exception("IndependentData for Property " + prop?.FullName + " kan only be of type class, eg an object");

            prop.Attributes.Add(new IndependentData());
            return this;
        }

        /// <summary>
        /// Add NotNullable for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObjectMapps<T> NotNullable<TP>(Expression<Func<T, TP>> action)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null)
                throw new Exception("Could not find Property " + prop?.FullName + "");

            prop.Attributes.Add(new NotNullable());
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
        public IObjectMapps<T> HasPropertyName<TP>(Expression<Func<T, TP>> action, string name, string displayName = null)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null)
                throw new Exception("Could not find Property " + prop?.FullName + "");

            prop.Attributes.Add(new PropertyName(name, displayName));
            return this;
        }

        /// <summary>
        /// Add Stringify for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObjectMapps<T> HasStringify<TP>(Expression<Func<T, TP>> action)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null)
                throw new Exception("Could not find Property " + prop?.FullName + "");

            prop.Attributes.Add(new Stringify());
            return this;
        }


        /// <summary>
        /// Add ExcludeFromAbstract for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObjectMapps<T> ExcludeFromAbstract<TP>(Expression<Func<T, TP>> action)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null)
                throw new Exception("Could not find Property " + prop?.FullName + "");

            prop.Attributes.Add(new ExcludeFromAbstract());
            return this;
        }

        /// <summary>
        /// Add DefaultOnEmpty for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"> Fefault value when property is null</param>
        /// <returns></returns>
        public IObjectMapps<T> HasDefaultOnEmpty<TP>(Expression<Func<T, TP>> action, object value)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null)
                throw new Exception("Could not find Property " + prop?.FullName + "");

            prop.Attributes.Add(new DefaultOnEmpty(value));
            return this;
        }

        /// <summary>
        /// Add ToBase64String for property
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObjectMapps<T> HasToBase64String<TP>(Expression<Func<T, TP>> action)
        {
            var prop = FastDeepCloner.DeepCloner.GetProperty(typeof(T), Extension.GetMemberName(action));
            if (prop == null || prop.PropertyType != typeof(string))
                throw new Exception("ToBase64String for Property " + prop?.FullName + " kan only be of type string");

            prop.Attributes.Add(new ToBase64String());
            return this;
        }
    }
}
