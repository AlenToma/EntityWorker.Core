using System;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace EntityWorker.SQLite
{
	[Serializable]
	public sealed class SQLiteException : DbException, ISerializable
	{
		private SQLiteErrorCode _errorCode;

		public override int ErrorCode
		{
			get
			{
				return (int)this._errorCode;
			}
		}

		public SQLiteErrorCode ResultCode
		{
			get
			{
				return this._errorCode;
			}
		}

		private SQLiteException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this._errorCode = (SQLiteErrorCode)info.GetInt32("errorCode");
		}

		public SQLiteException(SQLiteErrorCode errorCode, string message) : base(SQLiteException.GetStockErrorMessage(errorCode, message))
		{
			this._errorCode = errorCode;
		}

		public SQLiteException(string message) : this(SQLiteErrorCode.Unknown, message)
		{
		}

		public SQLiteException()
		{
		}

		public SQLiteException(string message, Exception innerException) : base(message, innerException)
		{
		}

		private static string GetErrorString(SQLiteErrorCode errorCode)
		{
			BindingFlags bindingFlag = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
			Type type = typeof(SQLite3);
			object[] objArray = new object[] { errorCode };
			return type.InvokeMember("GetErrorString", bindingFlag, null, null, objArray) as string;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				info.AddValue("errorCode", this._errorCode);
			}
			base.GetObjectData(info, context);
		}

		private static string GetStockErrorMessage(SQLiteErrorCode errorCode, string message)
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] errorString = new object[] { SQLiteException.GetErrorString(errorCode), Environment.NewLine, message };
			return HelperMethods.StringFormat(currentCulture, "{0}{1}{2}", errorString).Trim();
		}
	}
}