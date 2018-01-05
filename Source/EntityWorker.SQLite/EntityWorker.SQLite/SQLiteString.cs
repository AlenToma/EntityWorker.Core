using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EntityWorker.SQLite
{
	internal static class SQLiteString
	{
		private static int ThirtyBits;

		private readonly static Encoding Utf8Encoding;

		static SQLiteString()
		{
			SQLiteString.ThirtyBits = 1073741823;
			SQLiteString.Utf8Encoding = Encoding.UTF8;
		}

		public static string GetStringFromUtf8Bytes(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}
			return SQLiteString.Utf8Encoding.GetString(bytes);
		}

		public static byte[] GetUtf8BytesFromString(string value)
		{
			if (value == null)
			{
				return null;
			}
			return SQLiteString.Utf8Encoding.GetBytes(value);
		}

		public static int ProbeForUtf8ByteLength(IntPtr pValue, int limit)
		{
			int num = 0;
			if (pValue != IntPtr.Zero && limit > 0)
			{
				while (Marshal.ReadByte(pValue, num) != 0 && num < limit)
				{
					num++;
				}
			}
			return num;
		}

		public static string[] StringArrayFromUtf8SizeAndIntPtr(int argc, IntPtr argv)
		{
			string str;
			if (argc < 0)
			{
				return null;
			}
			if (argv == IntPtr.Zero)
			{
				return null;
			}
			string[] strArrays = new string[argc];
			int num = 0;
			int size = 0;
			while (num < (int)strArrays.Length)
			{
				IntPtr intPtr = SQLiteMarshal.ReadIntPtr(argv, size);
				string[] strArrays1 = strArrays;
				int num1 = num;
				if (intPtr != IntPtr.Zero)
				{
					str = SQLiteString.StringFromUtf8IntPtr(intPtr);
				}
				else
				{
					str = null;
				}
				strArrays1[num1] = str;
				num++;
				size += IntPtr.Size;
			}
			return strArrays;
		}

		public static string StringFromUtf8IntPtr(IntPtr pValue)
		{
			return SQLiteString.StringFromUtf8IntPtr(pValue, SQLiteString.ProbeForUtf8ByteLength(pValue, SQLiteString.ThirtyBits));
		}

		public static string StringFromUtf8IntPtr(IntPtr pValue, int length)
		{
			if (pValue == IntPtr.Zero)
			{
				return null;
			}
			if (length <= 0)
			{
				return string.Empty;
			}
			byte[] numArray = new byte[length];
			Marshal.Copy(pValue, numArray, 0, length);
			return SQLiteString.GetStringFromUtf8Bytes(numArray);
		}

		public static IntPtr[] Utf8IntPtrArrayFromStringArray(string[] values)
		{
			if (values == null)
			{
				return null;
			}
			IntPtr[] intPtrArray = new IntPtr[(int)values.Length];
			for (int i = 0; i < (int)intPtrArray.Length; i++)
			{
				intPtrArray[i] = SQLiteString.Utf8IntPtrFromString(values[i]);
			}
			return intPtrArray;
		}

		public static IntPtr Utf8IntPtrFromString(string value)
		{
			int num = 0;
			return SQLiteString.Utf8IntPtrFromString(value, ref num);
		}

		public static IntPtr Utf8IntPtrFromString(string value, ref int length)
		{
			if (value == null)
			{
				return IntPtr.Zero;
			}
			IntPtr zero = IntPtr.Zero;
			byte[] utf8BytesFromString = SQLiteString.GetUtf8BytesFromString(value);
			if (utf8BytesFromString == null)
			{
				return IntPtr.Zero;
			}
			length = (int)utf8BytesFromString.Length;
			zero = SQLiteMemory.Allocate(length + 1);
			if (zero == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			Marshal.Copy(utf8BytesFromString, 0, zero, length);
			Marshal.WriteByte(zero, length, 0);
			return zero;
		}
	}
}