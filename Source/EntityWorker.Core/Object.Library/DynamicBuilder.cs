using EntityWorker.Core.Attributes;
using EntityWorker.Core.Helper;
using FastDeepCloner;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace EntityWorker.Core.Object.Library
{
    internal class DynamicBuilder
    {
#if !(NETSTANDARD2_0 || NETSTANDARD1_3 || NETSTANDARD1_5)
        private static readonly MethodInfo getValueMethod = typeof(DataRecordExtended).GetMethod("get_Item", new Type[] { typeof(int), typeof(string), typeof(string), typeof(int), typeof(string), typeof(string), typeof(string) });
        private delegate object Load(DataRecordExtended dataRecord);
        private Load handler;

        private DynamicBuilder()
        {

        }

        public object Build(DataRecordExtended dataRecord)
        {
            return handler(dataRecord);
        }

        public static DynamicBuilder CreateBuilder(DataRecordExtended dataRecord, Type type)
        {
            var props = DeepCloner.GetFastDeepClonerProperties(type);
            DynamicBuilder dynamicBuilder = new DynamicBuilder();

            DynamicMethod method = new DynamicMethod("DynamicCreate", type, new Type[] { typeof(DataRecordExtended) }, type, true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(type);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                var columnName = dataRecord.GetName(i);
                var prop = DeepCloner.GetProperty(type, columnName);

                if (prop == null)
                    prop = props.FirstOrDefault(x => x.GetPropertyName() == columnName);

                if (prop != null && prop.PropertySetValue != null)
                {
                    var propType = prop.PropertyType;

                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        propType = propType.GetGenericArguments()?.FirstOrDefault() ?? propType;

                    var dataEncode = prop.GetCustomAttribute<DataEncode>();
                    var toBase64String = prop.GetCustomAttribute<ToBase64String>();

                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);

                    generator.Emit(OpCodes.Ldstr, dataEncode?.Key ?? "");

                    generator.Emit(OpCodes.Ldstr, dataEncode?.KeySize.ToString() ?? "");

                    generator.Emit(OpCodes.Ldc_I4, toBase64String != null ? 1 : 0);

                    generator.Emit(OpCodes.Ldstr, propType.ToString());

                    generator.Emit(OpCodes.Ldstr, dataRecord.GetFieldType(i).ToString());

                    generator.Emit(OpCodes.Ldstr, prop.PropertyType.AssemblyQualifiedName);

                    generator.Emit(OpCodes.Callvirt, getValueMethod);

                    generator.Emit(OpCodes.Unbox_Any, prop.PropertyType);
                    generator.Emit(OpCodes.Callvirt, prop.PropertySetValue);
                }
            }

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
#endif
    }
}