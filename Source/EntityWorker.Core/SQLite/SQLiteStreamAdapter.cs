using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	internal sealed class SQLiteStreamAdapter : IDisposable
	{
		private Stream stream;

		private SQLiteConnectionFlags flags;

		private UnsafeNativeMethods.xSessionInput xInput;

		private UnsafeNativeMethods.xSessionOutput xOutput;

		private bool disposed;

		public SQLiteStreamAdapter(Stream stream, SQLiteConnectionFlags flags)
		{
			this.stream = stream;
			this.flags = flags;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(SQLiteStreamAdapter).Name);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && disposing)
				{
					if (this.xInput != null)
					{
						this.xInput = null;
					}
					if (this.xOutput != null)
					{
						this.xOutput = null;
					}
					if (this.stream != null)
					{
						this.stream = null;
					}
				}
			}
			finally
			{
				this.disposed = true;
			}
		}

		~SQLiteStreamAdapter()
		{
			this.Dispose(false);
		}

		private SQLiteConnectionFlags GetFlags()
		{
			return this.flags;
		}

		public UnsafeNativeMethods.xSessionInput GetInputDelegate()
		{
			this.CheckDisposed();
			if (this.xInput == null)
			{
				this.xInput = new UnsafeNativeMethods.xSessionInput(this.Input);
			}
			return this.xInput;
		}

		public UnsafeNativeMethods.xSessionOutput GetOutputDelegate()
		{
			this.CheckDisposed();
			if (this.xOutput == null)
			{
				this.xOutput = new UnsafeNativeMethods.xSessionOutput(this.Output);
			}
			return this.xOutput;
		}

		private SQLiteErrorCode Input(IntPtr context, IntPtr pData, ref int nData)
		{
			SQLiteErrorCode sQLiteErrorCode;
			try
			{
				Stream stream = this.stream;
				if (stream != null)
				{
					if (nData > 0)
					{
						byte[] numArray = new byte[nData];
						int num = stream.Read(numArray, 0, nData);
						if (num > 0 && pData != IntPtr.Zero)
						{
							Marshal.Copy(numArray, 0, pData, num);
						}
						nData = num;
					}
					sQLiteErrorCode = SQLiteErrorCode.Ok;
				}
				else
				{
					sQLiteErrorCode = SQLiteErrorCode.Misuse;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this.GetFlags()))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "xSessionInput", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
				return SQLiteErrorCode.IoErr_Read;
			}
			return sQLiteErrorCode;
		}

		private SQLiteErrorCode Output(IntPtr context, IntPtr pData, int nData)
		{
			SQLiteErrorCode sQLiteErrorCode;
			try
			{
				Stream stream = this.stream;
				if (stream != null)
				{
					if (nData > 0)
					{
						byte[] numArray = new byte[nData];
						if (pData != IntPtr.Zero)
						{
							Marshal.Copy(pData, numArray, 0, nData);
						}
						stream.Write(numArray, 0, nData);
					}
					stream.Flush();
					sQLiteErrorCode = SQLiteErrorCode.Ok;
				}
				else
				{
					sQLiteErrorCode = SQLiteErrorCode.Misuse;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					if (HelperMethods.LogCallbackExceptions(this.GetFlags()))
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { "xSessionOutput", exception };
						SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(currentCulture, "Caught exception in \"{0}\" method: {1}", objArray));
					}
				}
				catch
				{
				}
				return SQLiteErrorCode.IoErr_Write;
			}
			return sQLiteErrorCode;
		}
	}
}