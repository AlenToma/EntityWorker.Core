using System;
using System.Runtime.InteropServices;

namespace EntityWorker.SQLite
{
	internal static class SQLiteBytes
	{
		public static byte[] FromIntPtr(IntPtr pValue, int length)
		{
			if (pValue == IntPtr.Zero)
			{
				return null;
			}
			if (length == 0)
			{
				return new byte[0];
			}
			byte[] numArray = new byte[length];
			Marshal.Copy(pValue, numArray, 0, length);
			return numArray;
		}

		public static IntPtr ToIntPtr(byte[] value)
		{
			int num = 0;
			return SQLiteBytes.ToIntPtr(value, ref num);
		}

		public static IntPtr ToIntPtr(byte[] value, ref int length)
		{
			if (value == null)
			{
				return IntPtr.Zero;
			}
			length = (int)value.Length;
			if (length == 0)
			{
				return IntPtr.Zero;
			}
			IntPtr intPtr = SQLiteMemory.Allocate(length);
			if (intPtr == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			Marshal.Copy(value, 0, intPtr, length);
			return intPtr;
		}
	}
}