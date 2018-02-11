using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Text;
using EntityWorker.Core.FastDeepCloner;
using EntityWorker.Core.Attributes;
#if !SILVERLIGHT
using System.Data;
#endif
using System.Collections.Specialized;

namespace EntityWorker.Core.Object.Library.JSON
{
    internal struct Getters
    {
        public string Name;
        public string lcName;
        public string memberName;
        public Func<object, object> Getter;
    }

    internal enum myPropInfoType
    {
        Int,
        Long,
        String,
        Bool,
        DateTime,
        Enum,
        Guid,

        Array,
        ByteArray,
        Dictionary,
        StringKeyDictionary,
        NameValue,
        StringDictionary,
#if !SILVERLIGHT
        Hashtable,
        DataSet,
        DataTable,
#endif
        Custom,
        Unknown,
    }

    internal struct myPropInfo
    {
        public Type pt;
        public Type bt;
        public Type changeType;
        public IFastDeepClonerProperty Property;
        public Type[] GenericTypes;
        public string Name;
#if net4
        public string memberName;
#endif
        public myPropInfoType Type;
        public bool IsClass;
        public bool IsValueType;
        public bool IsGenericType;
        public bool IsStruct;
        public bool IsInterface;
    }

    internal sealed class Reflection
    {
        // Sinlgeton pattern 4 from : http://csharpindepth.com/articles/general/singleton.aspx
        private static readonly Reflection instance = new Reflection();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Reflection()
        {
        }
        private Reflection()
        {
        }
        public static Reflection Instance { get { return instance; } }

        private delegate object CreateObject();

        private SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>();
        private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>();
        private SafeDictionary<Type, CreateObject> _constrcache = new SafeDictionary<Type, CreateObject>();
        private SafeDictionary<Type, Getters[]> _getterscache = new SafeDictionary<Type, Getters[]>();
        private SafeDictionary<string, Dictionary<string, myPropInfo>> _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        private SafeDictionary<Type, Type[]> _genericTypes = new SafeDictionary<Type, Type[]>();
        private SafeDictionary<Type, Type> _genericTypeDef = new SafeDictionary<Type, Type>();

        #region bjson custom types
        internal UnicodeEncoding unicode = new UnicodeEncoding();
        internal UTF8Encoding utf8 = new UTF8Encoding();
        #endregion

        #region json custom types
        // JSON custom
        internal SafeDictionary<Type, Serialize> _customSerializer = new SafeDictionary<Type, Serialize>();
        internal SafeDictionary<Type, Deserialize> _customDeserializer = new SafeDictionary<Type, Deserialize>();

        internal object CreateCustom(string v, Type type)
        {
            Deserialize d;
            _customDeserializer.TryGetValue(type, out d);
            return d(v);
        }

        internal void RegisterCustomType(Type type, Serialize serializer, Deserialize deserializer)
        {
            if (type != null && serializer != null && deserializer != null)
            {
                _customSerializer.Add(type, serializer);
                _customDeserializer.Add(type, deserializer);
                // reset property cache
                Instance.ResetPropertyCache();
            }
        }

        internal bool IsTypeRegistered(Type t)
        {
            if (_customSerializer.Count == 0)
                return false;
            Serialize s;
            return _customSerializer.TryGetValue(t, out s);
        }
        #endregion

        public Type GetGenericTypeDefinition(Type t)
        {
            Type tt = null;
            if (_genericTypeDef.TryGetValue(t, out tt))
                return tt;
            else
            {
                tt = t.GetGenericTypeDefinition();
                _genericTypeDef.Add(t, tt);
                return tt;
            }
        }

        public Type[] GetGenericArguments(Type t)
        {
            Type[] tt = null;
            if (_genericTypes.TryGetValue(t, out tt))
                return tt;
            else
            {
                tt = t.GetGenericArguments();
                _genericTypes.Add(t, tt);
                return tt;
            }
        }

        public Dictionary<string, myPropInfo> Getproperties(Type type, string typename)
        {
            Dictionary<string, myPropInfo> sd = null;
            if (_propertycache.TryGetValue(typename, out sd))
            {
                return sd;
            }
            else
            {
                sd = new Dictionary<string, myPropInfo>();
                var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                var pr = FastDeepCloner.DeepCloner.GetFastDeepClonerProperties(type);
                //type.GetProperties(bf);
                foreach (var p in pr)
                {
                    if (!p.CanRead)// Property is an indexer
                        continue;

                    myPropInfo d = CreateMyProp(p.PropertyType, p.Name);
                    d.Property = p;

                    sd.Add(p.Name.ToLower(), d);
                }
                var fi = FastDeepCloner.DeepCloner.GetFastDeepClonerFields(type);
                //FieldInfo[] fi = type.GetFields(bf);
                foreach (var f in fi)
                {
                    if (!f.CanRead)// Property is an indexer
                        continue;

                    myPropInfo d = CreateMyProp(f.PropertyType, f.Name);

                    d.Property = f;

                    sd.Add(f.Name.ToLower(), d);



                    _propertycache.Add(typename, sd);

                }

                return sd;
            }
        }

        private myPropInfo CreateMyProp(Type t, string name)
        {
            myPropInfo d = new myPropInfo();
            myPropInfoType d_type = myPropInfoType.Unknown;

            if (t == typeof(int) || t == typeof(int?)) d_type = myPropInfoType.Int;
            else if (t == typeof(long) || t == typeof(long?)) d_type = myPropInfoType.Long;
            else if (t == typeof(string)) d_type = myPropInfoType.String;
            else if (t == typeof(bool) || t == typeof(bool?)) d_type = myPropInfoType.Bool;
            else if (t == typeof(DateTime) || t == typeof(DateTime?)) d_type = myPropInfoType.DateTime;
            else if (t.IsEnum) d_type = myPropInfoType.Enum;
            else if (t == typeof(Guid) || t == typeof(Guid?)) d_type = myPropInfoType.Guid;
            else if (t == typeof(StringDictionary)) d_type = myPropInfoType.StringDictionary;
            else if (t == typeof(NameValueCollection)) d_type = myPropInfoType.NameValue;
            else if (t.IsArray)
            {
                d.bt = t.GetElementType();
                if (t == typeof(byte[]))
                    d_type = myPropInfoType.ByteArray;
                else
                    d_type = myPropInfoType.Array;
            }
            else if (t.Name.Contains("Dictionary"))
            {
                d.GenericTypes = Reflection.Instance.GetGenericArguments(t);
                if (d.GenericTypes.Length > 0 && d.GenericTypes[0] == typeof(string))
                    d_type = myPropInfoType.StringKeyDictionary;
                else
                    d_type = myPropInfoType.Dictionary;
            }
#if !SILVERLIGHT
            else if (t == typeof(Hashtable)) d_type = myPropInfoType.Hashtable;
            else if (t == typeof(DataSet)) d_type = myPropInfoType.DataSet;
            else if (t == typeof(DataTable)) d_type = myPropInfoType.DataTable;
#endif
            else if (IsTypeRegistered(t))
                d_type = myPropInfoType.Custom;

            if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof(decimal))
                d.IsStruct = true;

            d.IsInterface = t.IsInterface;
            d.IsClass = t.IsClass;
            d.IsValueType = t.IsValueType;
            if (t.IsGenericType)
            {
                d.IsGenericType = true;
                d.bt = t.GetGenericArguments()[0];
            }

            d.pt = t;
            d.Name = name;
            d.changeType = GetChangeType(t);
            d.Type = d_type;

            return d;
        }

        private Type GetChangeType(Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                return Reflection.Instance.GetGenericArguments(conversionType)[0];

            return conversionType;
        }

        #region [   PROPERTY GET SET   ]

        internal string GetTypeAssemblyName(Type t)
        {
            string val = "";
            if (_tyname.TryGetValue(t, out val))
                return val;
            else
            {
                string s = t.AssemblyQualifiedName;
                _tyname.Add(t, s);
                return s;
            }
        }

        internal Type GetTypeFromCache(string typename)
        {
            Type val = null;
            if (_typecache.TryGetValue(typename, out val))
                return val;
            else
            {
                Type t = Type.GetType(typename);
                _typecache.Add(typename, t);
                return t;
            }
        }

        internal object FastCreateInstance(Type objtype)
        {
            return objtype.Creator();
        }

        internal Getters[] GetGetters(Type type, bool ShowReadOnlyProperties)
        {
            Getters[] val = null;
            if (_getterscache.TryGetValue(type, out val))
                return val;
            //bool isAnonymous = IsAnonymousType(type);

            var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            //if (ShowReadOnlyProperties)
            //    bf |= BindingFlags.NonPublic;
            var props = DeepCloner.GetFastDeepClonerProperties(type);
            List<Getters> getters = new List<Getters>();
            foreach (FastDeepClonerProperty p in props)
            {
                if (!p.CanRead || p.ContainAttribute<JsonIgnore>())
                {// Property is an indexer
                    continue;
                }

                string mName = null;
                getters.Add(new Getters { Getter = p._propertyGet, Name = p.Name, lcName = p.Name.ToLower(), memberName = mName });
            }

            ////FieldInfo[] fi = type.GetFields(bf);
            //var fi = DeepCloner.GetFastDeepClonerFields(type);
            //foreach (FastDeepClonerProperty f in fi)
            //{
            //    if (!f.CanRead || f.ContainAttribute<JsonIgnore>())
            //    {// Property is an indexer
            //        continue;
            //    }
            //    string mName = null;

            //    getters.Add(new Getters { Getter = f._propertyGet, Name = f.Name, lcName = f.Name.ToLower(), memberName = mName });

            //}
            val = getters.ToArray();
            _getterscache.Add(type, val);
            return val;
        }

        #endregion

        internal void ResetPropertyCache()
        {
            _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        }

        internal void ClearReflectionCache()
        {
            _tyname = new SafeDictionary<Type, string>();
            _typecache = new SafeDictionary<string, Type>();
            _constrcache = new SafeDictionary<Type, CreateObject>();
            _getterscache = new SafeDictionary<Type, Getters[]>();
            _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
            _genericTypes = new SafeDictionary<Type, Type[]>();
            _genericTypeDef = new SafeDictionary<Type, Type>();
        }
    }
}
