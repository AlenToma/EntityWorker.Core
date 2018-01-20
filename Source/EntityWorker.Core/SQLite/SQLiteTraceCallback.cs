using System;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SQLiteTraceCallback(IntPtr puser, IntPtr statement);
}