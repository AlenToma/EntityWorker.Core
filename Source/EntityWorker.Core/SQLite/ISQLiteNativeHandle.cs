using System;

namespace EntityWorker.Core.SQLite
{
	public interface ISQLiteNativeHandle
	{
		IntPtr NativeHandle
		{
			get;
		}
	}
}