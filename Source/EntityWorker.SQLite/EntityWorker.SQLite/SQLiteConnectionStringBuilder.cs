using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace EntityWorker.SQLite
{
    [DefaultProperty("DataSource")]
    public sealed class SQLiteConnectionStringBuilder : DbConnectionStringBuilder
    {
        private Hashtable _properties;

        [Browsable(true)]
        [DefaultValue("sqlite_default_schema")]
        [DisplayName("Base Schema Name")]
        public string BaseSchemaName
        {
            get
            {
                object obj;
                if (this.TryGetValue("baseschemaname", out obj))
                {
                    if (obj is string)
                    {
                        return (string)obj;
                    }
                    if (obj != null)
                    {
                        return obj.ToString();
                    }
                }
                return null;
            }
            set
            {
                this["baseschemaname"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [DisplayName("Binary GUID")]
        public bool BinaryGUID
        {
            get
            {
                object obj;
                this.TryGetValue("binaryguid", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["binaryguid"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(0)]
        [DisplayName("Busy Timeout")]
        public int BusyTimeout
        {
            get
            {
                object obj;
                this.TryGetValue("busytimeout", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["busytimeout"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(-2000)]
        [DisplayName("Cache Size")]
        public int CacheSize
        {
            get
            {
                object obj;
                this.TryGetValue("cache size", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["cache size"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue("")]
        [DisplayName("Data Source")]
        public string DataSource
        {
            get
            {
                object obj;
                this.TryGetValue("data source", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["data source"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(SQLiteDateFormats.ISO8601)]
        [DisplayName("DateTime Format")]
        public SQLiteDateFormats DateTimeFormat
        {
            get
            {
                object obj;
                if (this.TryGetValue("datetimeformat", out obj))
                {
                    if (obj as SQLiteDateFormats? != null && obj as SQLiteDateFormats? != SQLiteDateFormats.Ticks)
                    {
                        return (SQLiteDateFormats)obj;
                    }
                    if (obj != null)
                    {
                        return (SQLiteDateFormats)TypeDescriptor.GetConverter(typeof(SQLiteDateFormats)).ConvertFrom(obj);
                    }
                }
                return SQLiteDateFormats.ISO8601;
            }
            set
            {
                this["datetimeformat"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("DateTime Format String")]
        public string DateTimeFormatString
        {
            get
            {
                object obj;
                if (this.TryGetValue("datetimeformatstring", out obj))
                {
                    if (obj is string)
                    {
                        return (string)obj;
                    }
                    if (obj != null)
                    {
                        return obj.ToString();
                    }
                }
                return null;
            }
            set
            {
                this["datetimeformatstring"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(System.DateTimeKind.Unspecified)]
        [DisplayName("DateTime Kind")]
        public System.DateTimeKind DateTimeKind
        {
            get
            {
                object obj;
                if (this.TryGetValue("datetimekind", out obj))
                {
                    if (obj as System.DateTimeKind? != null && obj as System.DateTimeKind? != System.DateTimeKind.Unspecified)
                    {
                        return (System.DateTimeKind)obj;
                    }
                    if (obj != null)
                    {
                        return (System.DateTimeKind)TypeDescriptor.GetConverter(typeof(System.DateTimeKind)).ConvertFrom(obj);
                    }
                }
                return System.DateTimeKind.Unspecified;
            }
            set
            {
                this["datetimekind"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(-1)]
        [DisplayName("Default Database Type")]
        public DbType DefaultDbType
        {
            get
            {
                object obj;
                if (this.TryGetValue("defaultdbtype", out obj))
                {
                    if (obj is string)
                    {
                        return (DbType)TypeDescriptor.GetConverter(typeof(DbType)).ConvertFrom(obj);
                    }
                    if (obj != null)
                    {
                        return (DbType)obj;
                    }
                }
                return DbType.Binary | DbType.Byte | DbType.Boolean | DbType.Currency | DbType.Date | DbType.DateTime | DbType.Decimal | DbType.Double | DbType.Guid | DbType.Int16 | DbType.Int32 | DbType.Int64 | DbType.Object | DbType.SByte | DbType.Single | DbType.String | DbType.Time | DbType.UInt16 | DbType.UInt32 | DbType.UInt64 | DbType.VarNumeric | DbType.AnsiStringFixedLength | DbType.StringFixedLength | DbType.Xml | DbType.DateTime2 | DbType.DateTimeOffset;
            }
            set
            {
                this["defaultdbtype"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(IsolationLevel.Serializable)]
        [DisplayName("Default Isolation Level")]
        public IsolationLevel DefaultIsolationLevel
        {
            get
            {
                object obj;
                this.TryGetValue("default isolationlevel", out obj);
                if (!(obj is string))
                {
                    return (IsolationLevel)obj;
                }
                return (IsolationLevel)TypeDescriptor.GetConverter(typeof(IsolationLevel)).ConvertFrom(obj);
            }
            set
            {
                this["default isolationlevel"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(30)]
        [DisplayName("Default Timeout")]
        public int DefaultTimeout
        {
            get
            {
                object obj;
                this.TryGetValue("default timeout", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["default timeout"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("Default Type Name")]
        public string DefaultTypeName
        {
            get
            {
                object obj;
                this.TryGetValue("defaulttypename", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["defaulttypename"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        public bool Enlist
        {
            get
            {
                object obj;
                this.TryGetValue("enlist", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["enlist"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Fail If Missing")]
        public bool FailIfMissing
        {
            get
            {
                object obj;
                this.TryGetValue("failifmissing", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["failifmissing"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(SQLiteConnectionFlags.Default)]
        public SQLiteConnectionFlags Flags
        {
            get
            {
                object obj;
                if (this.TryGetValue("flags", out obj))
                {
                    if (obj is SQLiteConnectionFlags)
                    {
                        return (SQLiteConnectionFlags)obj;
                    }
                    if (obj != null)
                    {
                        return (SQLiteConnectionFlags)TypeDescriptor.GetConverter(typeof(SQLiteConnectionFlags)).ConvertFrom(obj);
                    }
                }
                return SQLiteConnectionFlags.Default;
            }
            set
            {
                this["flags"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Foreign Keys")]
        public bool ForeignKeys
        {
            get
            {
                object obj;
                this.TryGetValue("foreign keys", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["foreign keys"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("Full URI")]
        public string FullUri
        {
            get
            {
                object obj;
                this.TryGetValue("fulluri", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["fulluri"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("Hexadecimal Password")]
        [PasswordPropertyText(true)]
        public byte[] HexPassword
        {
            get
            {
                object obj;
                if (this.TryGetValue("hexpassword", out obj))
                {
                    if (obj is string)
                    {
                        return SQLiteConnection.FromHexString((string)obj);
                    }
                    if (obj != null)
                    {
                        return (byte[])obj;
                    }
                }
                return null;
            }
            set
            {
                this["hexpassword"] = SQLiteConnection.ToHexString(value);
            }
        }

        [Browsable(true)]
        [DefaultValue(SQLiteJournalModeEnum.Default)]
        [DisplayName("Journal Mode")]
        public SQLiteJournalModeEnum JournalMode
        {
            get
            {
                object obj;
                this.TryGetValue("journal mode", out obj);
                if (!(obj is string))
                {
                    return (SQLiteJournalModeEnum)obj;
                }
                return (SQLiteJournalModeEnum)TypeDescriptor.GetConverter(typeof(SQLiteJournalModeEnum)).ConvertFrom(obj);
            }
            set
            {
                this["journal mode"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Legacy Format")]
        public bool LegacyFormat
        {
            get
            {
                object obj;
                this.TryGetValue("legacy format", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["legacy format"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(0)]
        [DisplayName("Maximum Page Count")]
        public int MaxPageCount
        {
            get
            {
                object obj;
                this.TryGetValue("max page count", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["max page count"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("No Default Flags")]
        public bool NoDefaultFlags
        {
            get
            {
                object obj;
                this.TryGetValue("nodefaultflags", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["nodefaultflags"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("No Shared Flags")]
        public bool NoSharedFlags
        {
            get
            {
                object obj;
                this.TryGetValue("nosharedflags", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["nosharedflags"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(4096)]
        [DisplayName("Page Size")]
        public int PageSize
        {
            get
            {
                object obj;
                this.TryGetValue("page size", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["page size"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue("")]
        [PasswordPropertyText(true)]
        public string Password
        {
            get
            {
                object obj;
                this.TryGetValue("password", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["password"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        public bool Pooling
        {
            get
            {
                object obj;
                this.TryGetValue("pooling", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["pooling"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(3)]
        [DisplayName("Prepare Retries")]
        public int PrepareRetries
        {
            get
            {
                object obj;
                this.TryGetValue("prepareretries", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["prepareretries"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(0)]
        [DisplayName("Progress Ops")]
        public int ProgressOps
        {
            get
            {
                object obj;
                this.TryGetValue("progressops", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                this["progressops"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Read Only")]
        public bool ReadOnly
        {
            get
            {
                object obj;
                this.TryGetValue("read only", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["read only"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Recursive Triggers")]
        public bool RecursiveTriggers
        {
            get
            {
                object obj;
                this.TryGetValue("recursive triggers", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["recursive triggers"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [DisplayName("Set Defaults")]
        public bool SetDefaults
        {
            get
            {
                object obj;
                this.TryGetValue("setdefaults", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["setdefaults"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(SynchronizationModes.Normal)]
        [DisplayName("Synchronous")]
        public SynchronizationModes SyncMode
        {
            get
            {
                object obj;
                this.TryGetValue("synchronous", out obj);
                if (!(obj is string))
                {
                    return (SynchronizationModes)obj;
                }
                return (SynchronizationModes)TypeDescriptor.GetConverter(typeof(SynchronizationModes)).ConvertFrom(obj);
            }
            set
            {
                this["synchronous"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [DisplayName("To Full Path")]
        public bool ToFullPath
        {
            get
            {
                object obj;
                this.TryGetValue("tofullpath", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["tofullpath"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("URI")]
        public string Uri
        {
            get
            {
                object obj;
                this.TryGetValue("uri", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["uri"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Use UTF-16 Encoding")]
        public bool UseUTF16Encoding
        {
            get
            {
                object obj;
                this.TryGetValue("useutf16encoding", out obj);
                return SQLiteConvert.ToBoolean(obj);
            }
            set
            {
                this["useutf16encoding"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(3)]
        public int Version
        {
            get
            {
                object obj;
                this.TryGetValue("version", out obj);
                return Convert.ToInt32(obj, CultureInfo.CurrentCulture);
            }
            set
            {
                if (value != 3)
                {
                    throw new NotSupportedException();
                }
                this["version"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("VFS Name")]
        public string VfsName
        {
            get
            {
                object obj;
                this.TryGetValue("vfsname", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["vfsname"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [DisplayName("ZipVFS Version")]
        public string ZipVfsVersion
        {
            get
            {
                object obj;
                this.TryGetValue("zipvfsversion", out obj);
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
            set
            {
                this["zipvfsversion"] = value;
            }
        }

        public SQLiteConnectionStringBuilder()
        {
            this.Initialize(null);
        }

        public SQLiteConnectionStringBuilder(string connectionString)
        {
            this.Initialize(connectionString);
        }

        private void FallbackGetProperties(Hashtable propertyList)
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, true))
            {
                if (!(property.Name != "ConnectionString") || propertyList.ContainsKey(property.DisplayName))
                {
                    continue;
                }
                propertyList.Add(property.DisplayName, property);
            }
        }

        private void Initialize(string cnnString)
        {
            this._properties = new Hashtable(StringComparer.OrdinalIgnoreCase);
            try
            {
                base.GetProperties(this._properties);
            }
            catch (NotImplementedException notImplementedException)
            {
                this.FallbackGetProperties(this._properties);
            }
            if (!string.IsNullOrEmpty(cnnString))
            {
                base.ConnectionString = cnnString;
            }
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            bool flag = base.TryGetValue(keyword, out value);
            if (!this._properties.ContainsKey(keyword))
            {
                return flag;
            }
            PropertyDescriptor item = this._properties[keyword] as PropertyDescriptor;
            if (item == null)
            {
                return flag;
            }
            if (!flag)
            {
                DefaultValueAttribute defaultValueAttribute = item.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
                if (defaultValueAttribute != null)
                {
                    value = defaultValueAttribute.Value;
                    flag = true;
                }
            }
            else if (item.PropertyType == typeof(bool))
            {
                value = SQLiteConvert.ToBoolean(value);
            }
            else if (item.PropertyType != typeof(byte[]))
            {
                value = TypeDescriptor.GetConverter(item.PropertyType).ConvertFrom(value);
            }
            return flag;
        }
    }
}