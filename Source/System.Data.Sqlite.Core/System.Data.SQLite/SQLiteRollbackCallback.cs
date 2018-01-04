using System;
using System.Runtime.InteropServices;

namespace System.Data.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SQLiteRollbackCallback(IntPtr puser);
}