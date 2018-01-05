using System;
using System.Runtime.InteropServices;

namespace EntityWorker.SQLite
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int SQLiteCommitCallback(IntPtr puser);
}