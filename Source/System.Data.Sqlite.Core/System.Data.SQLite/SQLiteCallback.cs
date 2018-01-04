using System;
using System.Runtime.InteropServices;

namespace System.Data.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void SQLiteCallback(IntPtr context, int argc, IntPtr argv);
}