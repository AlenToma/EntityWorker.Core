using System;
using EntityWorker.Core.Helper;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// Use this when you have types that are unknown like interface wich it can takes more than one type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KnownType : Attribute
    {
        public Type ObjectType { get; set; }

        public KnownType(Type objectType)
        {
            if (objectType == null)
                throw new Exception("KnownType must have objectType, object type cant be null");
            ObjectType = objectType.GetActualType();

        }

    }
}
