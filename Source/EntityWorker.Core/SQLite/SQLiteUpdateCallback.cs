using System;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SQLiteUpdateCallback(IntPtr puser, int type, IntPtr database, IntPtr table, long rowid);
}