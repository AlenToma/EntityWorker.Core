using System;

namespace EntityWorker.Core.SQLite
{
	public sealed class SQLiteContext : ISQLiteNativeHandle
	{
		private IntPtr pContext;

		public IntPtr NativeHandle
		{
			get
			{
				return this.pContext;
			}
		}

		internal SQLiteContext(IntPtr pContext)
		{
			this.pContext = pContext;
		}

		public void SetBlob(byte[] value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			UnsafeNativeMethods.sqlite3_result_blob(this.pContext, value, (int)value.Length, (IntPtr)(-1));
		}

		public void SetDouble(double value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_double(this.pContext, value);
		}

		public void SetError(string value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			byte[] utf8BytesFromString = SQLiteString.GetUtf8BytesFromString(value);
			if (utf8BytesFromString == null)
			{
				throw new ArgumentNullException("value");
			}
			UnsafeNativeMethods.sqlite3_result_error(this.pContext, utf8BytesFromString, (int)utf8BytesFromString.Length);
		}

		public void SetErrorCode(SQLiteErrorCode value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_error_code(this.pContext, value);
		}

		public void SetErrorNoMemory()
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_error_nomem(this.pContext);
		}

		public void SetErrorTooBig()
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_error_toobig(this.pContext);
		}

		public void SetInt(int value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_int(this.pContext, value);
		}

		public void SetInt64(long value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_int64(this.pContext, value);
		}

		public void SetNull()
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_null(this.pContext);
		}

		public void SetString(string value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			byte[] utf8BytesFromString = SQLiteString.GetUtf8BytesFromString(value);
			if (utf8BytesFromString == null)
			{
				throw new ArgumentNullException("value");
			}
			UnsafeNativeMethods.sqlite3_result_text(this.pContext, utf8BytesFromString, (int)utf8BytesFromString.Length, (IntPtr)(-1));
		}

		public void SetValue(SQLiteValue value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			UnsafeNativeMethods.sqlite3_result_value(this.pContext, value.NativeHandle);
		}

		public void SetZeroBlob(int value)
		{
			if (this.pContext == IntPtr.Zero)
			{
				throw new InvalidOperationException();
			}
			UnsafeNativeMethods.sqlite3_result_zeroblob(this.pContext, value);
		}
	}
}