using EntityWorker.Core.Object.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityWorker.Core.FastDeepCloner
{
    internal static class FastDeepClonerCachedItems
    {
        public delegate object ObjectActivator();
        private static readonly Custom_ValueType<Type, Custom_ValueType<string, IFastDeepClonerProperty>> CachedFields = new Custom_ValueType<Type, Custom_ValueType<string, IFastDeepClonerProperty>>();
        private static readonly Custom_ValueType<Type, Custom_ValueType<string, IFastDeepClonerProperty>> CachedPropertyInfo = new Custom_ValueType<Type, Custom_ValueType<string, IFastDeepClonerProperty>>();
        private static readonly Custom_ValueType<Type, Type> CachedTypes = new Custom_ValueType<Type, Type>();
        private static readonly Custom_ValueType<Type, Func<object>> CachedConstructor = new Custom_ValueType<Type, Func<object>>();
        private static readonly Custom_ValueType<Type, ObjectActivator> CachedDynamicMethod = new Custom_ValueType<Type, ObjectActivator>();


        public static void CleanCachedItems()
        {
            CachedFields.Clear();
            CachedTypes.Clear();
            CachedConstructor.Clear();
            CachedPropertyInfo.Clear();
            CachedDynamicMethod.Clear();
        }

        internal static Type GetIListType(this Type type)
        {
            if (CachedTypes.ContainsKey(type))
                return CachedTypes[type];
            if (type.IsArray)
                return CachedTypes.GetOrAdd(type, type.GetElementType());
            else
            {
                if (type.GenericTypeArguments.Any())
                    return CachedTypes.GetOrAdd(type, typeof(List<>).MakeGenericType(type.GenericTypeArguments.First()));
                else if (type.FullName.Contains("List`1"))
                    return CachedTypes.GetOrAdd(type, typeof(List<>).MakeGenericType(type.GetRuntimeProperty("Item").PropertyType));
                else return CachedTypes.GetOrAdd(type, type);
            }
        }



        internal static object Creator(this Type type)
        {
#if NETSTANDARD2_0 || NETSTANDARD1_3 || NETSTANDARD1_5
            if (CachedConstructor.ContainsKey(type))
                return CachedConstructor[type].Invoke();
            return CachedConstructor.GetOrAdd(type, Expression.Lambda<Func<object>>(Expression.New(type)).Compile()).Invoke();
#else
            if (CachedDynamicMethod.ContainsKey(type))
                return CachedDynamicMethod[type]();

            var emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new System.Reflection.Emit.DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);
            System.Reflection.Emit.ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(System.Reflection.Emit.OpCodes.Nop);
            ilGenerator.Emit(System.Reflection.Emit.OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
            return CachedDynamicMethod.GetOrAdd(type, (ObjectActivator)dynamicMethod.CreateDelegate(typeof(ObjectActivator)))();
#endif
        }


        internal static bool GetField(this FieldInfo field, Custom_ValueType<string, IFastDeepClonerProperty> properties)
        {
            if (!properties.ContainsKey(field.Name))
                return properties.TryAdd(field.Name, new FastDeepClonerProperty(field));
            return true;
        }

        internal static bool GetField(this PropertyInfo field, Custom_ValueType<string, IFastDeepClonerProperty> properties)
        {
            if (!properties.ContainsKey(field.Name))
                return properties.TryAdd(field.Name, new FastDeepClonerProperty(field));
            return true;
        }

        internal static Dictionary<string, IFastDeepClonerProperty> GetFastDeepClonerFields(this Type primaryType)
        {
            if (!CachedFields.ContainsKey(primaryType))
            {
                var properties = new Custom_ValueType<string, IFastDeepClonerProperty>();
                if (primaryType.GetTypeInfo().BaseType != null && primaryType.GetTypeInfo().BaseType.Name != "Object")
                {
                    primaryType.GetRuntimeFields().Where(x => x.GetField(properties)).ToList();
                    primaryType.GetTypeInfo().BaseType.GetRuntimeFields().Where(x => x.GetField(properties)).ToList();

                }
                else primaryType.GetRuntimeFields().Where(x => x.GetField(properties)).ToList();
                CachedFields.Add(primaryType, properties);
            }
            return CachedFields[primaryType];
        }


        internal static Dictionary<string, IFastDeepClonerProperty> GetFastDeepClonerProperties(this Type primaryType)
        {
            if (!CachedPropertyInfo.ContainsKey(primaryType))
            {
                var properties = new Custom_ValueType<string, IFastDeepClonerProperty>();
                if (primaryType.GetTypeInfo().BaseType != null && primaryType.GetTypeInfo().BaseType.Name != "Object")
                {
                    primaryType.GetRuntimeProperties().Where(x => x.GetField(properties)).ToList();
                    primaryType.GetTypeInfo().BaseType.GetRuntimeProperties().Where(x => x.GetField(properties)).ToList();

                }
                else primaryType.GetRuntimeProperties().Where(x => x.GetField(properties)).ToList();
                CachedPropertyInfo.Add(primaryType, properties);
            }
            return CachedPropertyInfo[primaryType];
        }
    }
}
