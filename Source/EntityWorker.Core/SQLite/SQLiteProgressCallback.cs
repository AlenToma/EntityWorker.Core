using System;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate SQLiteProgressReturnCode SQLiteProgressCallback(IntPtr pUserData);
}