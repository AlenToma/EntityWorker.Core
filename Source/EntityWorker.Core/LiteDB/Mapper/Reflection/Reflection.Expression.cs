﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityWorker.Core.LiteDB
{
    /// <summary>
    /// Using Expressions is the easy and fast way to create classes, structs, get/set fields/properties. But it not works in NET35
    /// </summary>
    internal partial class Reflection
    {
        public static CreateObject CreateClass(Type type)
        {
            return Expression.Lambda<CreateObject>(Expression.New(type)).Compile();
        }

        public static CreateObject CreateStruct(Type type)
        {
            var newType = Expression.New(type);
            var convert = Expression.Convert(newType, typeof(object));

            return Expression.Lambda<CreateObject>(convert).Compile();
        }

        public static GenericGetter CreateGenericGetter(Type type, MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

            // if has no read
            if (memberInfo is PropertyInfo && (memberInfo as PropertyInfo).CanRead == false) return null;

            var obj = Expression.Parameter(typeof(object), "o");
            var accessor = Expression.MakeMemberAccess(Expression.Convert(obj, memberInfo.DeclaringType), memberInfo);

            return Expression.Lambda<GenericGetter>(Expression.Convert(accessor, typeof(object)), obj).Compile();
        }

        public static GenericSetter CreateGenericSetter(Type type, MemberInfo memberInfo)
        {
#if HAVE_EXPRESSION_ASSIGN
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            
            var fieldInfo = memberInfo as FieldInfo;
            var propertyInfo = memberInfo as PropertyInfo;

            // if is property and has no write
            if (memberInfo is PropertyInfo && propertyInfo.CanWrite == false) return null;

            // if *Structs*, use direct reflection - net35 has no Expression.Unbox to cast target
            if (type.GetTypeInfo().IsValueType)
            {
                return memberInfo is FieldInfo ?
                    (GenericSetter)fieldInfo.SetValue :
                    ((t, v) => propertyInfo.SetValue(t, v, null));
            }

            var dataType = memberInfo is PropertyInfo ?
                propertyInfo.PropertyType :
                fieldInfo.FieldType;

            var target = Expression.Parameter(typeof(object), "obj");
            var value = Expression.Parameter(typeof(object), "val");

            var castTarget = Expression.Convert(target, type);
            var castValue = Expression.ConvertChecked(value, dataType);

            var accessor = memberInfo is PropertyInfo ? 
                Expression.Property(castTarget, propertyInfo) :
                Expression.Field(castTarget, fieldInfo);

            var assign = Expression.Assign(accessor, castValue);
            var conv = Expression.Convert(assign, typeof(object));
            
            return Expression.Lambda<GenericSetter>(conv, target, value).Compile();
#else
            // when member is a field, use simple Reflection
            if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;

                return fieldInfo.SetValue;
            }

            // if is property, use invoke from setMethod
            var propertyInfo = memberInfo as PropertyInfo;
            var setMethod = propertyInfo.GetSetMethod(true);

            if (setMethod == null) return null;

            return (target, value) => setMethod.Invoke(target, new[] { value });
#endif
        }
    }
}