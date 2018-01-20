using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EntityWorker.Core.SQLite
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethodsWin32
	{
		[DllImport("kernel32", BestFitMapping=false, CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true, ThrowOnUnmappableChar=true)]
		internal static extern IntPtr LoadLibrary(string fileName);
	}
}