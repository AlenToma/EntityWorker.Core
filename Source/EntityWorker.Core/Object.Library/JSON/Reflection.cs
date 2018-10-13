using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Text;
using EntityWorker.Core.FastDeepCloner;
using EntityWorker.Core.Attributes;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
#if !SILVERLIGHT
using System.Data;
#endif
using System.Collections.Specialized;

namespace EntityWorker.Core.Object.Library.JSON
{
    public struct Getters
    {
        public string Name;
        public string lcName;
        public string memberName;
        public FastDeepCloner.IFastDeepClonerProperty Property;
    }

    public enum myPropInfoType
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

    internal class myPropInfo
    {
        public Type pt;
        public Type bt;
        public Type changeType;
        public FastDeepCloner.IFastDeepClonerProperty Property;
        public Type[] GenericTypes;
        public string Name;
        public string memberName;
        public myPropInfoType Type;
        public bool CanWrite;

        public bool IsClass;
        public bool IsValueType;
        public bool IsGenericType;
        public bool IsStruct;
        public bool IsInterface;
    }

    public sealed class Reflection
    {
        // Singleton pattern 4 from : http://csharpindepth.com/articles/general/singleton.aspx
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

        public static bool RDBMode = false;

        public delegate string Serialize(object data);
        public delegate object Deserialize(string data);


        private SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>(10);
        private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>(10);
        private SafeDictionary<Type, Getters[]> _getterscache = new SafeDictionary<Type, Getters[]>(10);
        private SafeDictionary<string, Dictionary<string, myPropInfo>> _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        private SafeDictionary<Type, Type[]> _genericTypes = new SafeDictionary<Type, Type[]>(10);
        private SafeDictionary<Type, Type> _genericTypeDef = new SafeDictionary<Type, Type>(10);
        private static SafeDictionary<short, OpCode> _opCodes;

        private static bool TryGetOpCode(short code, out OpCode opCode)
        {
            if (_opCodes != null)
                return _opCodes.TryGetValue(code, out opCode);
            var dict = new SafeDictionary<short, OpCode>();
            foreach (var fi in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!typeof(OpCode).IsAssignableFrom(fi.FieldType)) continue;
                var innerOpCode = (OpCode)fi.GetValue(null);
                if (innerOpCode.OpCodeType != OpCodeType.Nternal)
                    dict.Add(innerOpCode.Value, innerOpCode);
            }
            _opCodes = dict;
            return _opCodes.TryGetValue(code, out opCode);
        }

        #region bjson custom types
        //internal UnicodeEncoding unicode = new UnicodeEncoding();
        private static UTF8Encoding utf8 = new UTF8Encoding();

        // TODO : optimize utf8 
        public static byte[] UTF8GetBytes(string str)
        {
            return utf8.GetBytes(str);
        }

        public static string UTF8GetString(byte[] bytes, int offset, int len)
        {
            return utf8.GetString(bytes, offset, len);
        }

        public unsafe static byte[] UnicodeGetBytes(string str)
        {
            int len = str.Length * 2;
            byte[] b = new byte[len];
            fixed (void* ptr = str)
            {
                System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), b, 0, len);
            }
            return b;
        }

        public static string UnicodeGetString(byte[] b)
        {
            return UnicodeGetString(b, 0, b.Length);
        }

        public unsafe static string UnicodeGetString(byte[] bytes, int offset, int buflen)
        {
            string str = "";
            fixed (byte* bptr = bytes)
            {
                char* cptr = (char*)(bptr + offset);
                str = new string(cptr, 0, buflen / 2);
            }
            return str;
        }
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

        internal Dictionary<string, myPropInfo> Getproperties(Type type, string typename)
        {
            Dictionary<string, myPropInfo> sd = null;
            if (_propertycache.TryGetValue(typename, out sd))
            {
                return sd;
            }
            else
            {
                sd = new Custom_ValueType<string, myPropInfo>();
                var pr = DeepCloner.GetFastDeepClonerProperties(type);
                foreach (var p in pr)
                {
                    if (!p.CanRead)// Property is an indexer
                        continue;

                    myPropInfo d = CreateMyProp(p.PropertyType, p.Name);
                    d.Property = p;
                    d.CanWrite = p.CanWrite;

                    foreach (var at in p.Attributes)
                    {
                        if (at is DataMemberAttribute)
                        {
                            var dm = (DataMemberAttribute)at;
                            if (dm.Name != "")
                                d.memberName = dm.Name;
                        }
                    }
                    if (d.memberName != null)
                        sd.Add(d.memberName, d);
                    else
                        sd.Add(p.Name.ToLowerInvariant(), d);
                }

                _propertycache.Add(typename, sd);

            }
            return sd;
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
                d.bt = Reflection.Instance.GetGenericArguments(t)[0];
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

        public string GetTypeAssemblyName(Type t)
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

                if (RDBMode)
                {
                    if (t == null) // RaptorDB : loading runtime assemblies
                    {
                        t = Type.GetType(typename, (name) =>
                        {
                            return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
                        }, null, true);
                    }
                }

                _typecache.Add(typename, t);
                return t;
            }
        }


        internal static FieldInfo GetGetterBackingField(PropertyInfo autoProperty)
        {
            var getMethod = autoProperty.GetGetMethod();
            // Restrict operation to auto properties to avoid risking errors if a getter does not contain exactly one field read instruction (such as with calculated properties).
            if (!getMethod.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)) return null;

            var byteCode = getMethod.GetMethodBody()?.GetILAsByteArray() ?? new byte[0];
            //var byteCode = getMethod.GetMethodBody().GetILAsByteArray();
            int pos = 0;
            // Find the first LdFld instruction and parse its operand to a FieldInfo object.
            while (pos < byteCode.Length)
            {
                // Read and parse the OpCode (it can be 1 or 2 bytes in size).
                byte code = byteCode[pos++];
                if (!(TryGetOpCode(code, out var opCode) || pos < byteCode.Length && TryGetOpCode((short)(code * 0x100 + byteCode[pos++]), out opCode)))
                    throw new NotSupportedException("Unknown IL code detected.");
                // If it is a LdFld, read its operand, parse it to a FieldInfo and return it.
                if (opCode == OpCodes.Ldfld && opCode.OperandType == OperandType.InlineField && pos + sizeof(int) <= byteCode.Length)
                {
                    return getMethod.Module.ResolveMember(BitConverter.ToInt32(byteCode, pos), getMethod.DeclaringType?.GetGenericArguments(), null) as FieldInfo;
                }
                // Otherwise, set the current position to the start of the next instruction, if any (we need to know how much bytes are used by operands).
                pos += opCode.OperandType == OperandType.InlineNone
                            ? 0
                            : opCode.OperandType == OperandType.ShortInlineBrTarget ||
                              opCode.OperandType == OperandType.ShortInlineI ||
                              opCode.OperandType == OperandType.ShortInlineVar
                                ? 1
                                : opCode.OperandType == OperandType.InlineVar
                                    ? 2
                                    : opCode.OperandType == OperandType.InlineI8 ||
                                      opCode.OperandType == OperandType.InlineR
                                        ? 8
                                        : opCode.OperandType == OperandType.InlineSwitch
                                            ? 4 * (BitConverter.ToInt32(byteCode, pos) + 1)
                                            : 4;
            }
            return null;
        }






        public Getters[] GetGetters(Type type, List<Type> IgnoreAttributes)
        {
            Getters[] val = null;
            if (_getterscache.TryGetValue(type, out val))
                return val;

            var props = DeepCloner.GetFastDeepClonerProperties(type);
            List<Getters> getters = new List<Getters>();
            foreach (var p in props)
            {
                if (!p.CanRead)//|| isAnonymous == false))
                    continue;

                if (IgnoreAttributes != null)
                {
                    bool found = false;
                    foreach (var ignoreAttr in IgnoreAttributes)
                    {
                        if (p.ContainAttribute(ignoreAttr))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }
                string mName = null;

                foreach (var at in p.Attributes)
                {
                    if (at is DataMemberAttribute)
                    {
                        var dm = (DataMemberAttribute)at;
                        if (dm.Name != "")
                        {
                            mName = dm.Name;
                        }
                    }
                }

                getters.Add(new Getters { Property = p, Name = p.Name, lcName = p.Name.ToLowerInvariant(), memberName = mName });
            }

            val = getters.ToArray();
            _getterscache.Add(type, val);
            return val;
        }

        internal static bool IsAnonymousType(Type type)
        {
            // may break in the future if compiler defined names change...
            const string CS_ANONYMOUS_PREFIX = "<>f__AnonymousType";
            const string VB_ANONYMOUS_PREFIX = "VB$AnonymousType";
            const string Auto_ANONYMOUS_PREFIX = "AnonymousType";

            if (type == null)
                throw new ArgumentNullException("type");

            if (type.Namespace == null && (type.Name.StartsWith(CS_ANONYMOUS_PREFIX, StringComparison.Ordinal)
                || type.Name.StartsWith(VB_ANONYMOUS_PREFIX, StringComparison.Ordinal) ||
                type.Name.StartsWith(Auto_ANONYMOUS_PREFIX, StringComparison.Ordinal)))
            {
                return type.IsDefined(typeof(CompilerGeneratedAttribute), false);
            }

            return false;
        }
        #endregion

        internal void ResetPropertyCache()
        {
            _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>();
        }

        internal void ClearReflectionCache()
        {
            _tyname = new SafeDictionary<Type, string>(10);
            _typecache = new SafeDictionary<string, Type>(10);
            _getterscache = new SafeDictionary<Type, Getters[]>(10);
            _propertycache = new SafeDictionary<string, Dictionary<string, myPropInfo>>(10);
            _genericTypes = new SafeDictionary<Type, Type[]>(10);
            _genericTypeDef = new SafeDictionary<Type, Type>(10);
        }
    }
}