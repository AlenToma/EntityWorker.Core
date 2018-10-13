using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;

namespace EntityWorker.Core.FastDeepCloner
{
    internal class FastDeepClonerProperty : IFastDeepClonerProperty
    {
        internal readonly Func<object, object> _propertyGet;

        internal readonly Action<object, object> _propertySet;

        public bool CanRead { get; private set; }

        public bool CanReadWrite { get; private set; }

        public bool CanWrite { get; private set; }

        public bool FastDeepClonerIgnore { get; private set; }

        public string Name { get; private set; }

        public string FullName { get; private set; }

        public bool IsInternalType { get; private set; }

        public Type PropertyType { get; private set; }

        public bool? IsVirtual { get; private set; }

        public AttributesCollections Attributes { get; set; }

        public MethodInfo PropertyGetValue { get; set; }

        public MethodInfo PropertySetValue { get; set; }

        internal FastDeepClonerProperty(FieldInfo field)
        {
            CanRead = !(field.IsInitOnly || field.FieldType == typeof(IntPtr) || field.IsLiteral);
            CanReadWrite = CanRead;
            CanWrite = CanRead;
            FastDeepClonerIgnore = field.GetCustomAttribute<FastDeepClonerIgnore>() != null;
            Attributes = new AttributesCollections(field.GetCustomAttributes().ToList());
            _propertyGet = field.GetValue;
            _propertySet = field.SetValue;
            Name = field.Name;
            FullName = field.FieldType.FullName;
            PropertyType = field.FieldType;
            IsInternalType = field.FieldType.IsInternalType();

            if (ContainAttribute<KnownType>())
            {
                if (PropertyType != PropertyType.GetActualType())
                    PropertyType = typeof(List<>).MakeGenericType(GetCustomAttribute<KnownType>().ObjectType);
                else PropertyType = GetCustomAttribute<KnownType>().ObjectType;
            }
        }

        internal FastDeepClonerProperty(PropertyInfo property)
        {
            CanRead = !(!property.CanRead || property.PropertyType == typeof(IntPtr) || property.GetIndexParameters().Length > 0);
            CanReadWrite = property.CanWrite && CanRead;
            CanWrite = property.CanWrite;
            FastDeepClonerIgnore = property.GetCustomAttribute<FastDeepClonerIgnore>() != null;
            _propertyGet = property.GetValue;
            _propertySet = property.SetValue;
            Attributes = new AttributesCollections(property.GetCustomAttributes().ToList());
            Name = property.Name;
            FullName = property.PropertyType.FullName;
            PropertyType = property.PropertyType;
            IsInternalType = property.PropertyType.IsInternalType();
            IsVirtual = property.GetMethod.IsVirtual;
            PropertyGetValue = property.GetMethod;
            PropertySetValue = property.SetMethod;

            if (ContainAttribute<KnownType>())
            {
                if (PropertyType != PropertyType.GetActualType())
                    PropertyType = typeof(List<>).MakeGenericType(GetCustomAttribute<KnownType>().ObjectType);
                else PropertyType = GetCustomAttribute<KnownType>().ObjectType;
            }
        }

        public bool ContainAttribute<T>() where T : Attribute
        {
            return Attributes?.ContainedAttributestypes.ContainsKey(typeof(T)) ?? false;
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return ContainAttribute<T>() ? (T)Attributes?.ContainedAttributestypes[typeof(T)] : null;
        }

        public Attribute GetCustomAttribute(Type type)
        {
            return ContainAttribute(type) ? Attributes?.ContainedAttributestypes[type] : null;
        }

        public bool ContainAttribute(Type type)
        {
            return Attributes?.ContainedAttributestypes.ContainsKey(type) ?? false;
        }

        public void SetValue(object o, object value)
        {
            _propertySet(o, value);
        }

        public object GetValue(object o)
        {
            return _propertyGet(o);
        }

        public void Add(Attribute attr)
        {
            Attributes.Add(attr);
            if (ContainAttribute<KnownType>())
            {
                if (PropertyType != PropertyType.GetActualType())
                    PropertyType = typeof(List<>).MakeGenericType(GetCustomAttribute<KnownType>().ObjectType);
                else PropertyType = GetCustomAttribute<KnownType>().ObjectType;
            }
        }
    }
}
