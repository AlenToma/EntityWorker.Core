using System;
using System.Runtime.InteropServices;

namespace EntityWorker.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SQLiteLogCallback(IntPtr pUserData, int errorCode, IntPtr pMessage);
}