using System;

namespace System.Data.SQLite
{
	internal static class NativeLibraryHelper
	{
		public static IntPtr LoadLibrary(string fileName)
		{
			NativeLibraryHelper.LoadLibraryCallback loadLibraryCallback = new NativeLibraryHelper.LoadLibraryCallback(NativeLibraryHelper.LoadLibraryWin32);
			if (!HelperMethods.IsWindows())
			{
				loadLibraryCallback = new NativeLibraryHelper.LoadLibraryCallback(NativeLibraryHelper.LoadLibraryPosix);
			}
			return loadLibraryCallback(fileName);
		}

		private static IntPtr LoadLibraryPosix(string fileName)
		{
			return UnsafeNativeMethodsPosix.dlopen(fileName, 258);
		}

		private static IntPtr LoadLibraryWin32(string fileName)
		{
			return UnsafeNativeMethodsWin32.LoadLibrary(fileName);
		}

		private delegate IntPtr LoadLibraryCallback(string fileName);
	}
}