using System;
using System.Runtime.InteropServices;

namespace System.Data.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SQLiteUpdateCallback(IntPtr puser, int type, IntPtr database, IntPtr table, long rowid);
}