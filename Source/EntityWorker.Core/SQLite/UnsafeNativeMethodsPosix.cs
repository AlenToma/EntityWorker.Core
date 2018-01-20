using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EntityWorker.Core.SQLite
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethodsPosix
	{
		internal const int RTLD_LAZY = 1;

		internal const int RTLD_NOW = 2;

		internal const int RTLD_GLOBAL = 256;

		internal const int RTLD_LOCAL = 0;

		internal const int RTLD_DEFAULT = 258;

		[DllImport("__Internal", BestFitMapping=false, CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi, ExactSpelling=false, SetLastError=true, ThrowOnUnmappableChar=true)]
		internal static extern IntPtr dlopen(string fileName, int mode);
	}
}