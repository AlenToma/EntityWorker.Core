using System;

namespace EntityWorker.SQLite
{
	public interface ISQLiteNativeHandle
	{
		IntPtr NativeHandle
		{
			get;
		}
	}
}