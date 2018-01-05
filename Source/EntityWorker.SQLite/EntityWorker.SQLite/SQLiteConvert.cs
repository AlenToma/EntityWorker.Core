using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace EntityWorker.SQLite
{
	public abstract class SQLiteConvert
	{
		private const DbType FallbackDefaultDbType = DbType.Object;

		private const string FullFormat = "yyyy-MM-ddTHH:mm:ss.fffffffK";

		private readonly static string FallbackDefaultTypeName;

		protected readonly static DateTime UnixEpoch;

		private readonly static double OleAutomationEpochAsJulianDay;

		private readonly static long MinimumJd;

		private readonly static long MaximumJd;

		private static string[] _datetimeFormats;

		private readonly static string _datetimeFormatUtc;

		private readonly static string _datetimeFormatLocal;

		private static Encoding _utf8;

		internal SQLiteDateFormats _datetimeFormat;

		internal DateTimeKind _datetimeKind;

		internal string _datetimeFormatString;

		private static Type[] _affinitytotype;

		private static DbType[] _typetodbtype;

		private static int[] _dbtypetocolumnsize;

		private static object[] _dbtypetonumericprecision;

		private static object[] _dbtypetonumericscale;

		private static Type[] _dbtypeToType;

		private static TypeAffinity[] _typecodeAffinities;

		private static object _syncRoot;

		private static SQLiteDbTypeMap _typeNames;

		static SQLiteConvert()
		{
			SQLiteConvert.FallbackDefaultTypeName = string.Empty;
			SQLiteConvert.UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			SQLiteConvert.OleAutomationEpochAsJulianDay = 2415018.5;
			SQLiteConvert.MinimumJd = SQLiteConvert.computeJD(DateTime.MinValue);
			SQLiteConvert.MaximumJd = SQLiteConvert.computeJD(DateTime.MaxValue);
			string[] strArrays = new string[] { "THHmmssK", "THHmmK", "HH:mm:ss.FFFFFFFK", "HH:mm:ssK", "HH:mmK", "yyyy-MM-dd HH:mm:ss.FFFFFFFK", "yyyy-MM-dd HH:mm:ssK", "yyyy-MM-dd HH:mmK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", "yyyy-MM-ddTHH:mmK", "yyyy-MM-ddTHH:mm:ssK", "yyyyMMddHHmmssK", "yyyyMMddHHmmK", "yyyyMMddTHHmmssFFFFFFFK", "THHmmss", "THHmm", "HH:mm:ss.FFFFFFF", "HH:mm:ss", "HH:mm", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyyMMddHHmmss", "yyyyMMddHHmm", "yyyyMMddTHHmmssFFFFFFF", "yyyy-MM-dd", "yyyyMMdd", "yy-MM-dd" };
			SQLiteConvert._datetimeFormats = strArrays;
			SQLiteConvert._datetimeFormatUtc = SQLiteConvert._datetimeFormats[5];
			SQLiteConvert._datetimeFormatLocal = SQLiteConvert._datetimeFormats[19];
			SQLiteConvert._utf8 = new UTF8Encoding();
			Type[] typeArray = new Type[] { typeof(object), typeof(long), typeof(double), typeof(string), typeof(byte[]), typeof(object), typeof(DateTime), typeof(object) };
			SQLiteConvert._affinitytotype = typeArray;
			DbType[] dbTypeArray = new DbType[] { DbType.Object, DbType.Binary, DbType.Object, DbType.Boolean, DbType.SByte, DbType.SByte, DbType.Byte, DbType.Int16, DbType.UInt16, DbType.Int32, DbType.UInt32, DbType.Int64, DbType.UInt64, DbType.Single, DbType.Double, DbType.Decimal, DbType.DateTime, DbType.Object, DbType.String };
			SQLiteConvert._typetodbtype = dbTypeArray;
			SQLiteConvert._dbtypetocolumnsize = new int[] { 2147483647, 2147483647, 1, 1, 8, 8, 8, 8, 8, 16, 2, 4, 8, 2147483647, 1, 4, 2147483647, 8, 2, 4, 8, 8, 2147483647, 2147483647, 2147483647, 2147483647 };
			object[] value = new object[] { DBNull.Value, DBNull.Value, 3, DBNull.Value, 19, DBNull.Value, DBNull.Value, 53, 53, DBNull.Value, 5, 10, 19, DBNull.Value, 3, 24, DBNull.Value, DBNull.Value, 5, 10, 19, 53, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value };
			SQLiteConvert._dbtypetonumericprecision = value;
			object[] objArray = new object[] { DBNull.Value, DBNull.Value, 0, DBNull.Value, 4, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, 0, 0, 0, DBNull.Value, 0, DBNull.Value, DBNull.Value, DBNull.Value, 0, 0, 0, 0, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value };
			SQLiteConvert._dbtypetonumericscale = objArray;
			Type[] typeArray1 = new Type[] { typeof(string), typeof(byte[]), typeof(byte), typeof(bool), typeof(decimal), typeof(DateTime), typeof(DateTime), typeof(decimal), typeof(double), typeof(Guid), typeof(short), typeof(int), typeof(long), typeof(object), typeof(sbyte), typeof(float), typeof(string), typeof(DateTime), typeof(ushort), typeof(uint), typeof(ulong), typeof(double), typeof(string), typeof(string), typeof(string), typeof(string) };
			SQLiteConvert._dbtypeToType = typeArray1;
			TypeAffinity[] typeAffinityArray = new TypeAffinity[] { TypeAffinity.Null, TypeAffinity.Blob, TypeAffinity.Null, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Double, TypeAffinity.Double, TypeAffinity.Double, TypeAffinity.DateTime, TypeAffinity.Null, TypeAffinity.Text };
			SQLiteConvert._typecodeAffinities = typeAffinityArray;
			SQLiteConvert._syncRoot = new object();
			SQLiteConvert._typeNames = null;
		}

		internal SQLiteConvert(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString)
		{
			this._datetimeFormat = fmt;
			this._datetimeKind = kind;
			this._datetimeFormatString = fmtString;
		}

		private static DateTime computeHMS(long jd, DateTime? badValue)
		{
			DateTime dateTime;
			if (!SQLiteConvert.isValidJd(jd))
			{
				if (!badValue.HasValue)
				{
					throw new ArgumentException("Not a supported Julian Day value.");
				}
				return badValue.Value;
			}
			int num = (int)((jd + (long)43200000) % (long)86400000);
			decimal num1 = num / new decimal(10000, 0, 0, false, 1);
			num = (int)num1;
			int num2 = (int)((num1 - num) * new decimal(10000, 0, 0, false, 1));
			num1 -= num;
			int num3 = num / 3600;
			num = num - num3 * 3600;
			int num4 = num / 60;
			num1 = num1 + (num - num4 * 60);
			int num5 = (int)num1;
			try
			{
				DateTime minValue = DateTime.MinValue;
				dateTime = new DateTime(minValue.Year, minValue.Month, minValue.Day, num3, num4, num5, num2);
			}
			catch
			{
				if (!badValue.HasValue)
				{
					throw;
				}
				dateTime = badValue.Value;
			}
			return dateTime;
		}

		private static long computeJD(DateTime dateTime)
		{
			int year = dateTime.Year;
			int month = dateTime.Month;
			int day = dateTime.Day;
			if (month <= 2)
			{
				year--;
				month += 12;
			}
			int num = year / 100;
			int num1 = 2 - num + num / 4;
			int num2 = 36525 * (year + 4716) / 100;
			int num3 = 306001 * (month + 1) / 10000;
			long hour = (long)(((double)(num2 + num3 + day + num1) - 1524.5) * 86400000);
			hour += (long)(dateTime.Hour * 3600000 + dateTime.Minute * 60000 + dateTime.Second * 1000 + dateTime.Millisecond);
			return hour;
		}

		private static DateTime computeYMD(long jd, DateTime? badValue)
		{
			DateTime dateTime;
			if (!SQLiteConvert.isValidJd(jd))
			{
				if (!badValue.HasValue)
				{
					throw new ArgumentException("Not a supported Julian Day value.");
				}
				return badValue.Value;
			}
			int num = (int)((jd + (long)43200000) / (long)86400000);
			int num1 = (int)(((double)num - 1867216.25) / 36524.25);
			num1 = num + 1 + num1 - num1 / 4;
			int num2 = num1 + 1524;
			int num3 = (int)(((double)num2 - 122.1) / 365.25);
			int num4 = 36525 * num3 / 100;
			int num5 = (int)((double)(num2 - num4) / 30.6001);
			int num6 = (int)(30.6001 * (double)num5);
			int num7 = num2 - num4 - num6;
			int num8 = (num5 < 14 ? num5 - 1 : num5 - 13);
			int num9 = (num8 > 2 ? num3 - 4716 : num3 - 4715);
			try
			{
				dateTime = new DateTime(num9, num8, num7);
			}
			catch
			{
				if (!badValue.HasValue)
				{
					throw;
				}
				dateTime = badValue.Value;
			}
			return dateTime;
		}

		internal static int DbTypeToColumnSize(DbType typ)
		{
			return SQLiteConvert._dbtypetocolumnsize[(int)typ];
		}

		internal static object DbTypeToNumericPrecision(DbType typ)
		{
			return SQLiteConvert._dbtypetonumericprecision[(int)typ];
		}

		internal static object DbTypeToNumericScale(DbType typ)
		{
			return SQLiteConvert._dbtypetonumericscale[(int)typ];
		}

		internal static Type DbTypeToType(DbType typ)
		{
			return SQLiteConvert._dbtypeToType[(int)typ];
		}

		internal static string DbTypeToTypeName(SQLiteConnection connection, DbType dbType, SQLiteConnectionFlags flags)
		{
			SQLiteDbTypeMapping sQLiteDbTypeMapping;
			SQLiteDbTypeMapping sQLiteDbTypeMapping1;
			string str;
			string defaultTypeName = null;
			if (connection != null)
			{
				flags |= connection.Flags;
				if ((flags & SQLiteConnectionFlags.UseConnectionTypes) == SQLiteConnectionFlags.UseConnectionTypes)
				{
					SQLiteDbTypeMap sQLiteDbTypeMap = connection._typeNames;
					if (sQLiteDbTypeMap != null && sQLiteDbTypeMap.TryGetValue(dbType, out sQLiteDbTypeMapping))
					{
						return sQLiteDbTypeMapping.typeName;
					}
				}
				defaultTypeName = connection.DefaultTypeName;
			}
			if ((flags & SQLiteConnectionFlags.NoGlobalTypes) == SQLiteConnectionFlags.NoGlobalTypes)
			{
				if (defaultTypeName != null)
				{
					return defaultTypeName;
				}
				defaultTypeName = SQLiteConvert.GetDefaultTypeName(connection);
				SQLiteConvert.DefaultTypeNameWarning(dbType, flags, defaultTypeName);
				return defaultTypeName;
			}
			lock (SQLiteConvert._syncRoot)
			{
				if (SQLiteConvert._typeNames == null)
				{
					SQLiteConvert._typeNames = SQLiteConvert.GetSQLiteDbTypeMap();
				}
				if (!SQLiteConvert._typeNames.TryGetValue(dbType, out sQLiteDbTypeMapping1))
				{
					if (defaultTypeName != null)
					{
						return defaultTypeName;
					}
					defaultTypeName = SQLiteConvert.GetDefaultTypeName(connection);
					SQLiteConvert.DefaultTypeNameWarning(dbType, flags, defaultTypeName);
					return defaultTypeName;
				}
				else
				{
					str = sQLiteDbTypeMapping1.typeName;
				}
			}
			return str;
		}

		private static void DefaultDbTypeWarning(string typeName, SQLiteConnectionFlags flags, DbType? dbType)
		{
			if (!string.IsNullOrEmpty(typeName) && (flags & SQLiteConnectionFlags.TraceWarning) == SQLiteConnectionFlags.TraceWarning)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { dbType, typeName };
				Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "WARNING: Type mapping failed, returning default type {0} for name \"{1}\".", objArray));
			}
		}

		private static void DefaultTypeNameWarning(DbType dbType, SQLiteConnectionFlags flags, string typeName)
		{
			if ((flags & SQLiteConnectionFlags.TraceWarning) == SQLiteConnectionFlags.TraceWarning)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { typeName, dbType };
				Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "WARNING: Type mapping failed, returning default name \"{0}\" for type {1}.", objArray));
			}
		}

		private static long DoubleToJd(double julianDay)
		{
			return (long)Math.Round(julianDay * 86400000);
		}

		private static string GetDateTimeKindFormat(DateTimeKind kind, string formatString)
		{
			if (formatString != null)
			{
				return formatString;
			}
			if (kind != DateTimeKind.Utc)
			{
				return SQLiteConvert._datetimeFormatLocal;
			}
			return SQLiteConvert._datetimeFormatUtc;
		}

		private static DbType GetDefaultDbType(SQLiteConnection connection)
		{
			DbType dbType;
			if (((connection != null ? connection.Flags : SQLiteConnectionFlags.None) & SQLiteConnectionFlags.NoConvertSettings) == SQLiteConnectionFlags.NoConvertSettings)
			{
				return DbType.Object;
			}
			bool flag = false;
			string str = "Use_SQLiteConvert_DefaultDbType";
			object settingValue = null;
			string str1 = null;
			if (connection == null || !connection.TryGetCachedSetting(str, str1, out settingValue))
			{
				settingValue = UnsafeNativeMethods.GetSettingValue(str, str1);
				if (settingValue == null)
				{
					settingValue = DbType.Object;
				}
			}
			else
			{
				flag = true;
			}
			try
			{
				if (settingValue as DbType? == DbType.AnsiString)
				{
					settingValue = SQLiteConnection.TryParseEnum(typeof(DbType), SQLiteConvert.SettingValueToString(settingValue), true);
					if (settingValue as DbType? == DbType.AnsiString)
					{
						settingValue = DbType.Object;
					}
				}
				dbType = (DbType)settingValue;
			}
			finally
			{
				if (!flag && connection != null)
				{
					connection.SetCachedSetting(str, settingValue);
				}
			}
			return dbType;
		}

		private static string GetDefaultTypeName(SQLiteConnection connection)
		{
			if (((connection != null ? connection.Flags : SQLiteConnectionFlags.None) & SQLiteConnectionFlags.NoConvertSettings) == SQLiteConnectionFlags.NoConvertSettings)
			{
				return SQLiteConvert.FallbackDefaultTypeName;
			}
			string str = "Use_SQLiteConvert_DefaultTypeName";
			object settingValue = null;
			string str1 = null;
			if (connection == null || !connection.TryGetCachedSetting(str, str1, out settingValue))
			{
				try
				{
					settingValue = UnsafeNativeMethods.GetSettingValue(str, str1) ?? SQLiteConvert.FallbackDefaultTypeName;
				}
				finally
				{
					if (connection != null)
					{
						connection.SetCachedSetting(str, settingValue);
					}
				}
			}
			return SQLiteConvert.SettingValueToString(settingValue);
		}

		private static SQLiteDbTypeMap GetSQLiteDbTypeMap()
		{
			SQLiteDbTypeMapping[] sQLiteDbTypeMapping = new SQLiteDbTypeMapping[] { new SQLiteDbTypeMapping("BIGINT", DbType.Int64, false), new SQLiteDbTypeMapping("BIGUINT", DbType.UInt64, false), new SQLiteDbTypeMapping("BINARY", DbType.Binary, false), new SQLiteDbTypeMapping("BIT", DbType.Boolean, true), new SQLiteDbTypeMapping("BLOB", DbType.Binary, true), new SQLiteDbTypeMapping("BOOL", DbType.Boolean, false), new SQLiteDbTypeMapping("BOOLEAN", DbType.Boolean, false), new SQLiteDbTypeMapping("CHAR", DbType.AnsiStringFixedLength, true), new SQLiteDbTypeMapping("CLOB", DbType.String, false), new SQLiteDbTypeMapping("COUNTER", DbType.Int64, false), new SQLiteDbTypeMapping("CURRENCY", DbType.Decimal, false), new SQLiteDbTypeMapping("DATE", DbType.DateTime, false), new SQLiteDbTypeMapping("DATETIME", DbType.DateTime, true), new SQLiteDbTypeMapping("DECIMAL", DbType.Decimal, true), new SQLiteDbTypeMapping("DECIMALTEXT", DbType.Decimal, false), new SQLiteDbTypeMapping("DOUBLE", DbType.Double, false), new SQLiteDbTypeMapping("FLOAT", DbType.Double, false), new SQLiteDbTypeMapping("GENERAL", DbType.Binary, false), new SQLiteDbTypeMapping("GUID", DbType.Guid, false), new SQLiteDbTypeMapping("IDENTITY", DbType.Int64, false), new SQLiteDbTypeMapping("IMAGE", DbType.Binary, false), new SQLiteDbTypeMapping("INT", DbType.Int32, true), new SQLiteDbTypeMapping("INT8", DbType.SByte, false), new SQLiteDbTypeMapping("INT16", DbType.Int16, false), new SQLiteDbTypeMapping("INT32", DbType.Int32, false), new SQLiteDbTypeMapping("INT64", DbType.Int64, false), new SQLiteDbTypeMapping("INTEGER", DbType.Int64, true), new SQLiteDbTypeMapping("INTEGER8", DbType.SByte, false), new SQLiteDbTypeMapping("INTEGER16", DbType.Int16, false), new SQLiteDbTypeMapping("INTEGER32", DbType.Int32, false), new SQLiteDbTypeMapping("INTEGER64", DbType.Int64, false), new SQLiteDbTypeMapping("LOGICAL", DbType.Boolean, false), new SQLiteDbTypeMapping("LONG", DbType.Int64, false), new SQLiteDbTypeMapping("LONGCHAR", DbType.String, false), new SQLiteDbTypeMapping("LONGTEXT", DbType.String, false), new SQLiteDbTypeMapping("LONGVARCHAR", DbType.String, false), new SQLiteDbTypeMapping("MEMO", DbType.String, false), new SQLiteDbTypeMapping("MONEY", DbType.Decimal, false), new SQLiteDbTypeMapping("NCHAR", DbType.StringFixedLength, true), new SQLiteDbTypeMapping("NOTE", DbType.String, false), new SQLiteDbTypeMapping("NTEXT", DbType.String, false), new SQLiteDbTypeMapping("NUMBER", DbType.Decimal, false), new SQLiteDbTypeMapping("NUMERIC", DbType.Decimal, false), new SQLiteDbTypeMapping("NUMERICTEXT", DbType.Decimal, false), new SQLiteDbTypeMapping("NVARCHAR", DbType.String, true), new SQLiteDbTypeMapping("OLEOBJECT", DbType.Binary, false), new SQLiteDbTypeMapping("RAW", DbType.Binary, false), new SQLiteDbTypeMapping("REAL", DbType.Double, true), new SQLiteDbTypeMapping("SINGLE", DbType.Single, true), new SQLiteDbTypeMapping("SMALLDATE", DbType.DateTime, false), new SQLiteDbTypeMapping("SMALLINT", DbType.Int16, true), new SQLiteDbTypeMapping("SMALLUINT", DbType.UInt16, true), new SQLiteDbTypeMapping("STRING", DbType.String, false), new SQLiteDbTypeMapping("TEXT", DbType.String, false), new SQLiteDbTypeMapping("TIME", DbType.DateTime, false), new SQLiteDbTypeMapping("TIMESTAMP", DbType.DateTime, false), new SQLiteDbTypeMapping("TINYINT", DbType.Byte, true), new SQLiteDbTypeMapping("TINYSINT", DbType.SByte, true), new SQLiteDbTypeMapping("UINT", DbType.UInt32, true), new SQLiteDbTypeMapping("UINT8", DbType.Byte, false), new SQLiteDbTypeMapping("UINT16", DbType.UInt16, false), new SQLiteDbTypeMapping("UINT32", DbType.UInt32, false), new SQLiteDbTypeMapping("UINT64", DbType.UInt64, false), new SQLiteDbTypeMapping("ULONG", DbType.UInt64, false), new SQLiteDbTypeMapping("UNIQUEIDENTIFIER", DbType.Guid, true), new SQLiteDbTypeMapping("UNSIGNEDINTEGER", DbType.UInt64, true), new SQLiteDbTypeMapping("UNSIGNEDINTEGER8", DbType.Byte, false), new SQLiteDbTypeMapping("UNSIGNEDINTEGER16", DbType.UInt16, false), new SQLiteDbTypeMapping("UNSIGNEDINTEGER32", DbType.UInt32, false), new SQLiteDbTypeMapping("UNSIGNEDINTEGER64", DbType.UInt64, false), new SQLiteDbTypeMapping("VARBINARY", DbType.Binary, false), new SQLiteDbTypeMapping("VARCHAR", DbType.AnsiString, true), new SQLiteDbTypeMapping("VARCHAR2", DbType.AnsiString, false), new SQLiteDbTypeMapping("YESNO", DbType.Boolean, false) };
			return new SQLiteDbTypeMap(sQLiteDbTypeMapping);
		}

		public static string GetStringOrNull(object value)
		{
			if (value == null)
			{
				return null;
			}
			if (value is string)
			{
				return (string)value;
			}
			if (value == DBNull.Value)
			{
				return null;
			}
			return value.ToString();
		}

		internal static bool IsStringDbType(DbType type)
		{
			DbType dbType = type;
			if (dbType != DbType.AnsiString && dbType != DbType.String)
			{
				switch (dbType)
				{
					case DbType.AnsiStringFixedLength:
					case DbType.StringFixedLength:
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		private static bool isValidJd(long jd)
		{
			if (jd < SQLiteConvert.MinimumJd)
			{
				return false;
			}
			return jd <= SQLiteConvert.MaximumJd;
		}

		private static double JdToDouble(long jd)
		{
			return (double)((double)jd / 86400000);
		}

		internal static bool LooksLikeDateTime(SQLiteConvert convert, string text)
		{
			if (convert == null)
			{
				return false;
			}
			try
			{
				if (string.Equals(convert.ToString(convert.ToDateTime(text)), text, StringComparison.Ordinal))
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		internal static bool LooksLikeDouble(string text)
		{
			double num;
			if (!double.TryParse(text, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent | NumberStyles.Integer | NumberStyles.Float, CultureInfo.InvariantCulture, out num))
			{
				return false;
			}
			return string.Equals(num.ToString(CultureInfo.InvariantCulture), text, StringComparison.Ordinal);
		}

		internal static bool LooksLikeInt64(string text)
		{
			long num;
			if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
			{
				return false;
			}
			return string.Equals(num.ToString(CultureInfo.InvariantCulture), text, StringComparison.Ordinal);
		}

		internal static bool LooksLikeNull(string text)
		{
			return text == null;
		}

		internal static string[] NewSplit(string value, char separator, bool keepQuote, ref string error)
		{
			if (separator == '\\' || separator == '\"')
			{
				error = "separator character cannot be the escape or quote characters";
				return null;
			}
			if (value == null)
			{
				error = "string value to split cannot be null";
				return null;
			}
			int length = value.Length;
			if (length == 0)
			{
				return new string[0];
			}
			List<string> strs = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			bool flag = false;
			bool flag1 = false;
			while (num < length)
			{
				int num1 = num;
				num = num1 + 1;
				char chr = value[num1];
				if (flag)
				{
					if (chr != '\\' && chr != '\"' && chr != separator)
					{
						stringBuilder.Append('\\');
					}
					stringBuilder.Append(chr);
					flag = false;
				}
				else if (chr == '\\')
				{
					flag = true;
				}
				else if (chr == '\"')
				{
					if (keepQuote)
					{
						stringBuilder.Append(chr);
					}
					flag1 = !flag1;
				}
				else if (chr != separator)
				{
					stringBuilder.Append(chr);
				}
				else if (!flag1)
				{
					strs.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
				else
				{
					stringBuilder.Append(chr);
				}
			}
			if (flag || flag1)
			{
				error = "unbalanced escape or quote character found";
				return null;
			}
			if (stringBuilder.Length > 0)
			{
				strs.Add(stringBuilder.ToString());
			}
			return strs.ToArray();
		}

		private static string SettingValueToString(object value)
		{
			if (value is string)
			{
				return (string)value;
			}
			if (value == null)
			{
				return null;
			}
			return value.ToString();
		}

		public static string[] Split(string source, char separator)
		{
			string str;
			char[] chrArray = new char[] { '\"', separator };
			char[] chrArray1 = new char[] { '\"' };
			int num = 0;
			List<string> strs = new List<string>();
			while (source.Length > 0)
			{
				num = source.IndexOfAny(chrArray, num);
				if (num == -1)
				{
					break;
				}
				if (source[num] != chrArray[0])
				{
					str = source.Substring(0, num).Trim();
					if (str.Length > 1 && str[0] == chrArray1[0] && str[str.Length - 1] == str[0])
					{
						str = str.Substring(1, str.Length - 2);
					}
					source = source.Substring(num + 1).Trim();
					if (str.Length > 0)
					{
						strs.Add(str);
					}
					num = 0;
				}
				else
				{
					num = source.IndexOfAny(chrArray1, num + 1);
					if (num == -1)
					{
						break;
					}
					num++;
				}
			}
			if (source.Length > 0)
			{
				str = source.Trim();
				if (str.Length > 1 && str[0] == chrArray1[0] && str[str.Length - 1] == str[0])
				{
					str = str.Substring(1, str.Length - 2);
				}
				strs.Add(str);
			}
			string[] strArrays = new string[strs.Count];
			strs.CopyTo(strArrays, 0);
			return strArrays;
		}

		internal static Type SQLiteTypeToType(SQLiteType t)
		{
			if (t.Type != DbType.Object)
			{
				return SQLiteConvert.DbTypeToType(t.Type);
			}
			return SQLiteConvert._affinitytotype[(int)t.Affinity];
		}

		internal static DateTime TicksToDateTime(long ticks, DateTimeKind kind)
		{
			return new DateTime(ticks, kind);
		}

		internal static bool ToBoolean(object obj, IFormatProvider provider, bool viaFramework)
		{
			object[] objArray;
			CultureInfo currentCulture;
			if (obj == null)
			{
				return false;
			}
			TypeCode typeCode = Type.GetTypeCode(obj.GetType());
			switch (typeCode)
			{
				case TypeCode.Empty:
				case TypeCode.DBNull:
				{
					return false;
				}
				case TypeCode.Object:
				case TypeCode.DateTime:
				case TypeCode.Object | TypeCode.DateTime:
				{
					currentCulture = CultureInfo.CurrentCulture;
					objArray = new object[] { typeCode };
					throw new SQLiteException(HelperMethods.StringFormat(currentCulture, "Cannot convert type {0} to boolean", objArray));
				}
				case TypeCode.Boolean:
				{
					return (bool)obj;
				}
				case TypeCode.Char:
				{
					if ((char)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.SByte:
				{
					if ((sbyte)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Byte:
				{
					if ((byte)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Int16:
				{
					if ((short)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.UInt16:
				{
					if ((ushort)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Int32:
				{
					if ((int)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.UInt32:
				{
					if ((uint)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Int64:
				{
					if ((long)obj == (long)0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.UInt64:
				{
					if ((ulong)obj == (long)0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Single:
				{
					if ((float)obj == 0f)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Double:
				{
					if ((double)obj == 0)
					{
						return false;
					}
					return true;
				}
				case TypeCode.Decimal:
				{
					if ((decimal)obj == new decimal(0))
					{
						return false;
					}
					return true;
				}
				case TypeCode.String:
				{
					if (viaFramework)
					{
						return Convert.ToBoolean(obj, provider);
					}
					return SQLiteConvert.ToBoolean(SQLiteConvert.ToStringWithProvider(obj, provider));
				}
				default:
				{
					currentCulture = CultureInfo.CurrentCulture;
					objArray = new object[] { typeCode };
					throw new SQLiteException(HelperMethods.StringFormat(currentCulture, "Cannot convert type {0} to boolean", objArray));
				}
			}
		}

		public static bool ToBoolean(object source)
		{
			if (source is bool)
			{
				return (bool)source;
			}
			return SQLiteConvert.ToBoolean(SQLiteConvert.ToStringWithProvider(source, CultureInfo.InvariantCulture));
		}

		public static bool ToBoolean(string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (string.Compare(source, 0, bool.TrueString, 0, source.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			if (string.Compare(source, 0, bool.FalseString, 0, source.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return false;
			}
			string lower = source.ToLower(CultureInfo.InvariantCulture);
			string str = lower;
			if (lower != null)
			{
				switch (str)
				{
					case "y":
					case "yes":
					case "on":
					case "1":
					{
						return true;
					}
					case "n":
					case "no":
					case "off":
					case "0":
					{
						return false;
					}
				}
			}
			throw new ArgumentException("source");
		}

		public DateTime ToDateTime(string dateText)
		{
			return SQLiteConvert.ToDateTime(dateText, this._datetimeFormat, this._datetimeKind, this._datetimeFormatString);
		}

		public static DateTime ToDateTime(string dateText, SQLiteDateFormats format, DateTimeKind kind, string formatString)
		{
			string str;
			string[] strArrays;
			DateTimeFormatInfo invariantInfo;
			DateTimeStyles dateTimeStyle;
			string str1;
			string str2;
			DateTimeFormatInfo dateTimeFormatInfo;
			DateTimeStyles dateTimeStyle1;
			switch (format)
			{
				case SQLiteDateFormats.Ticks:
				{
					return SQLiteConvert.TicksToDateTime(Convert.ToInt64(dateText, CultureInfo.InvariantCulture), kind);
				}
				case SQLiteDateFormats.ISO8601:
				{
					if (formatString != null)
					{
						str1 = dateText;
						str2 = formatString;
						dateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
						dateTimeStyle1 = (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None);
						return DateTime.SpecifyKind(DateTime.ParseExact(str1, str2, dateTimeFormatInfo, dateTimeStyle1), kind);
					}
					str = dateText;
					strArrays = SQLiteConvert._datetimeFormats;
					invariantInfo = DateTimeFormatInfo.InvariantInfo;
					dateTimeStyle = (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None);
					return DateTime.SpecifyKind(DateTime.ParseExact(str, strArrays, invariantInfo, dateTimeStyle), kind);
				}
				case SQLiteDateFormats.JulianDay:
				{
					return SQLiteConvert.ToDateTime(Convert.ToDouble(dateText, CultureInfo.InvariantCulture), kind);
				}
				case SQLiteDateFormats.UnixEpoch:
				{
					return SQLiteConvert.UnixEpochToDateTime(Convert.ToInt64(dateText, CultureInfo.InvariantCulture), kind);
				}
				case SQLiteDateFormats.InvariantCulture:
				{
					if (formatString == null)
					{
						return DateTime.SpecifyKind(DateTime.Parse(dateText, DateTimeFormatInfo.InvariantInfo, (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None)), kind);
					}
					return DateTime.SpecifyKind(DateTime.ParseExact(dateText, formatString, DateTimeFormatInfo.InvariantInfo, (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None)), kind);
				}
				case SQLiteDateFormats.CurrentCulture:
				{
					if (formatString == null)
					{
						return DateTime.SpecifyKind(DateTime.Parse(dateText, DateTimeFormatInfo.CurrentInfo, (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None)), kind);
					}
					return DateTime.SpecifyKind(DateTime.ParseExact(dateText, formatString, DateTimeFormatInfo.CurrentInfo, (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None)), kind);
				}
				default:
				{
					if (formatString != null)
					{
						str1 = dateText;
						str2 = formatString;
						dateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
						dateTimeStyle1 = (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None);
						return DateTime.SpecifyKind(DateTime.ParseExact(str1, str2, dateTimeFormatInfo, dateTimeStyle1), kind);
					}
					str = dateText;
					strArrays = SQLiteConvert._datetimeFormats;
					invariantInfo = DateTimeFormatInfo.InvariantInfo;
					dateTimeStyle = (kind == DateTimeKind.Utc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None);
					return DateTime.SpecifyKind(DateTime.ParseExact(str, strArrays, invariantInfo, dateTimeStyle), kind);
				}
			}
		}

		public DateTime ToDateTime(double julianDay)
		{
			return SQLiteConvert.ToDateTime(julianDay, this._datetimeKind);
		}

		public static DateTime ToDateTime(double julianDay, DateTimeKind kind)
		{
			long jd = SQLiteConvert.DoubleToJd(julianDay);
			DateTime dateTime = SQLiteConvert.computeYMD(jd, null);
			DateTime dateTime1 = SQLiteConvert.computeHMS(jd, null);
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime1.Hour, dateTime1.Minute, dateTime1.Second, dateTime1.Millisecond, kind);
		}

		internal DateTime ToDateTime(IntPtr ptr, int len)
		{
			return this.ToDateTime(this.ToString(ptr, len));
		}

		public static double ToJulianDay(DateTime value)
		{
			return SQLiteConvert.JdToDouble(SQLiteConvert.computeJD(value));
		}

		public virtual string ToString(IntPtr nativestring, int nativestringlen)
		{
			return SQLiteConvert.UTF8ToString(nativestring, nativestringlen);
		}

		public string ToString(DateTime dateValue)
		{
			return SQLiteConvert.ToString(dateValue, this._datetimeFormat, this._datetimeKind, this._datetimeFormatString);
		}

		public static string ToString(DateTime dateValue, SQLiteDateFormats format, DateTimeKind kind, string formatString)
		{
			DateTime dateTime;
			switch (format)
			{
				case SQLiteDateFormats.Ticks:
				{
					return dateValue.Ticks.ToString(CultureInfo.InvariantCulture);
				}
				case SQLiteDateFormats.ISO8601:
				{
					if (dateValue.Kind != DateTimeKind.Unspecified)
					{
						return dateValue.ToString(SQLiteConvert.GetDateTimeKindFormat(dateValue.Kind, formatString), CultureInfo.InvariantCulture);
					}
					dateTime = DateTime.SpecifyKind(dateValue, kind);
					return dateTime.ToString(SQLiteConvert.GetDateTimeKindFormat(kind, formatString), CultureInfo.InvariantCulture);
				}
				case SQLiteDateFormats.JulianDay:
				{
					return SQLiteConvert.ToJulianDay(dateValue).ToString(CultureInfo.InvariantCulture);
				}
				case SQLiteDateFormats.UnixEpoch:
				{
					TimeSpan timeSpan = dateValue.Subtract(SQLiteConvert.UnixEpoch);
					return (timeSpan.Ticks / (long)10000000).ToString();
				}
				case SQLiteDateFormats.InvariantCulture:
				{
					return dateValue.ToString((formatString != null ? formatString : "yyyy-MM-ddTHH:mm:ss.fffffffK"), CultureInfo.InvariantCulture);
				}
				case SQLiteDateFormats.CurrentCulture:
				{
					return dateValue.ToString((formatString != null ? formatString : "yyyy-MM-ddTHH:mm:ss.fffffffK"), CultureInfo.CurrentCulture);
				}
				default:
				{
					if (dateValue.Kind != DateTimeKind.Unspecified)
					{
						return dateValue.ToString(SQLiteConvert.GetDateTimeKindFormat(dateValue.Kind, formatString), CultureInfo.InvariantCulture);
					}
					dateTime = DateTime.SpecifyKind(dateValue, kind);
					return dateTime.ToString(SQLiteConvert.GetDateTimeKindFormat(kind, formatString), CultureInfo.InvariantCulture);
				}
			}
		}

		internal static string ToString(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToStringWithProvider(object obj, IFormatProvider provider)
		{
			if (obj == null)
			{
				return null;
			}
			if (obj is string)
			{
				return (string)obj;
			}
			IConvertible convertible = obj as IConvertible;
			if (convertible == null)
			{
				return obj.ToString();
			}
			return convertible.ToString(provider);
		}

		public static long ToUnixEpoch(DateTime value)
		{
			TimeSpan timeSpan = value.Subtract(SQLiteConvert.UnixEpoch);
			return timeSpan.Ticks / (long)10000000;
		}

		public static byte[] ToUTF8(string sourceText)
		{
			if (sourceText == null)
			{
				return null;
			}
			int byteCount = SQLiteConvert._utf8.GetByteCount(sourceText) + 1;
			byte[] numArray = new byte[byteCount];
			byteCount = SQLiteConvert._utf8.GetBytes(sourceText, 0, sourceText.Length, numArray, 0);
			numArray[byteCount] = 0;
			return numArray;
		}

		public byte[] ToUTF8(DateTime dateTimeValue)
		{
			return SQLiteConvert.ToUTF8(this.ToString(dateTimeValue));
		}

		internal static DbType TypeNameToDbType(SQLiteConnection connection, string typeName, SQLiteConnectionFlags flags)
		{
			SQLiteDbTypeMapping sQLiteDbTypeMapping;
			SQLiteDbTypeMapping sQLiteDbTypeMapping1;
			DbType dbType;
			DbType? defaultDbType = null;
			if (connection != null)
			{
				flags |= connection.Flags;
				if ((flags & SQLiteConnectionFlags.UseConnectionTypes) == SQLiteConnectionFlags.UseConnectionTypes)
				{
					SQLiteDbTypeMap sQLiteDbTypeMap = connection._typeNames;
					if (sQLiteDbTypeMap != null && typeName != null)
					{
						if (sQLiteDbTypeMap.TryGetValue(typeName, out sQLiteDbTypeMapping))
						{
							return sQLiteDbTypeMapping.dataType;
						}
						int num = typeName.IndexOf('(');
						if (num > 0 && sQLiteDbTypeMap.TryGetValue(typeName.Substring(0, num).TrimEnd(new char[0]), out sQLiteDbTypeMapping))
						{
							return sQLiteDbTypeMapping.dataType;
						}
					}
				}
				defaultDbType = connection.DefaultDbType;
			}
			if ((flags & SQLiteConnectionFlags.NoGlobalTypes) == SQLiteConnectionFlags.NoGlobalTypes)
			{
				if (defaultDbType.HasValue)
				{
					return defaultDbType.Value;
				}
				defaultDbType = new DbType?(SQLiteConvert.GetDefaultDbType(connection));
				SQLiteConvert.DefaultDbTypeWarning(typeName, flags, defaultDbType);
				return defaultDbType.Value;
			}
			lock (SQLiteConvert._syncRoot)
			{
				if (SQLiteConvert._typeNames == null)
				{
					SQLiteConvert._typeNames = SQLiteConvert.GetSQLiteDbTypeMap();
				}
				if (typeName != null)
				{
					if (!SQLiteConvert._typeNames.TryGetValue(typeName, out sQLiteDbTypeMapping1))
					{
						int num1 = typeName.IndexOf('(');
						if (num1 > 0 && SQLiteConvert._typeNames.TryGetValue(typeName.Substring(0, num1).TrimEnd(new char[0]), out sQLiteDbTypeMapping1))
						{
							dbType = sQLiteDbTypeMapping1.dataType;
							return dbType;
						}
					}
					else
					{
						dbType = sQLiteDbTypeMapping1.dataType;
						return dbType;
					}
				}
				if (defaultDbType.HasValue)
				{
					return defaultDbType.Value;
				}
				defaultDbType = new DbType?(SQLiteConvert.GetDefaultDbType(connection));
				SQLiteConvert.DefaultDbTypeWarning(typeName, flags, defaultDbType);
				return defaultDbType.Value;
			}
			return dbType;
		}

		internal static TypeAffinity TypeToAffinity(Type typ, SQLiteConnectionFlags flags)
		{
			TypeCode typeCode = Type.GetTypeCode(typ);
			if (typeCode == TypeCode.Object)
			{
				if (typ != typeof(byte[]) && typ != typeof(Guid))
				{
					return TypeAffinity.Text;
				}
				return TypeAffinity.Blob;
			}
			if (typeCode == TypeCode.Decimal && (flags & SQLiteConnectionFlags.GetDecimalAsText) == SQLiteConnectionFlags.GetDecimalAsText)
			{
				return TypeAffinity.Text;
			}
			return SQLiteConvert._typecodeAffinities[(int)typeCode];
		}

		internal static DbType TypeToDbType(Type typ)
		{
			TypeCode typeCode = Type.GetTypeCode(typ);
			if (typeCode != TypeCode.Object)
			{
				return SQLiteConvert._typetodbtype[(int)typeCode];
			}
			if (typ == typeof(byte[]))
			{
				return DbType.Binary;
			}
			if (typ == typeof(Guid))
			{
				return DbType.Guid;
			}
			return DbType.String;
		}

		internal static DateTime UnixEpochToDateTime(long seconds, DateTimeKind kind)
		{
			DateTime unixEpoch = SQLiteConvert.UnixEpoch;
			return DateTime.SpecifyKind(unixEpoch.AddSeconds((double)seconds), kind);
		}

		public static string UTF8ToString(IntPtr nativestring, int nativestringlen)
		{
			if (nativestring == IntPtr.Zero || nativestringlen == 0)
			{
				return string.Empty;
			}
			if (nativestringlen < 0)
			{
				nativestringlen = 0;
				while (Marshal.ReadByte(nativestring, nativestringlen) != 0)
				{
					nativestringlen++;
				}
				if (nativestringlen == 0)
				{
					return string.Empty;
				}
			}
			byte[] numArray = new byte[nativestringlen];
			Marshal.Copy(nativestring, numArray, 0, nativestringlen);
			return SQLiteConvert._utf8.GetString(numArray, 0, nativestringlen);
		}
	}
}