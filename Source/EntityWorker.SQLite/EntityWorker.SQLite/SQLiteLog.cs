using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace EntityWorker.SQLite
{
	public static class SQLiteLog
	{
		private static object syncRoot;

		private static EventHandler _domainUnload;

		private static SQLiteLogEventHandler _defaultHandler;

		private static SQLiteLogCallback _callback;

		private static SQLiteBase _sql;

		private static bool _enabled;

		public static bool Enabled
		{
			get
			{
				bool flag;
				lock (SQLiteLog.syncRoot)
				{
					flag = SQLiteLog._enabled;
				}
				return flag;
			}
			set
			{
				lock (SQLiteLog.syncRoot)
				{
					SQLiteLog._enabled = value;
				}
			}
		}

		static SQLiteLog()
		{
			SQLiteLog.syncRoot = new object();
		}

		public static void AddDefaultHandler()
		{
			SQLiteLog.InitializeDefaultHandler();
			SQLiteLog.Log += SQLiteLog._defaultHandler;
		}

		private static void DomainUnload(object sender, EventArgs e)
		{
			lock (SQLiteLog.syncRoot)
			{
				SQLiteLog.RemoveDefaultHandler();
				SQLiteLog._enabled = false;
				if (SQLiteLog._sql != null)
				{
					SQLiteErrorCode sQLiteErrorCode = SQLiteLog._sql.Shutdown();
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, "Failed to shutdown interface.");
					}
					sQLiteErrorCode = SQLiteLog._sql.SetLogCallback(null);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, "Failed to shutdown logging.");
					}
				}
				if (SQLiteLog._callback != null)
				{
					SQLiteLog._callback = null;
				}
				if (SQLiteLog._domainUnload != null)
				{
					AppDomain.CurrentDomain.DomainUnload -= SQLiteLog._domainUnload;
					SQLiteLog._domainUnload = null;
				}
			}
		}

		public static void Initialize()
		{
			if (SQLite3.StaticIsInitialized())
			{
				return;
			}
			if (!AppDomain.CurrentDomain.IsDefaultAppDomain() && UnsafeNativeMethods.GetSettingValue("Force_SQLiteLog", null) == null)
			{
				return;
			}
			lock (SQLiteLog.syncRoot)
			{
				if (SQLiteLog._domainUnload == null)
				{
					SQLiteLog._domainUnload = new EventHandler(SQLiteLog.DomainUnload);
					AppDomain.CurrentDomain.DomainUnload += SQLiteLog._domainUnload;
				}
				if (SQLiteLog._sql == null)
				{
					SQLiteLog._sql = new SQLite3(SQLiteDateFormats.ISO8601, DateTimeKind.Unspecified, null, IntPtr.Zero, null, false);
				}
				if (SQLiteLog._callback == null)
				{
					SQLiteLog._callback = new SQLiteLogCallback(SQLiteLog.LogCallback);
					SQLiteErrorCode sQLiteErrorCode = SQLiteLog._sql.SetLogCallback(SQLiteLog._callback);
					if (sQLiteErrorCode != SQLiteErrorCode.Ok)
					{
						throw new SQLiteException(sQLiteErrorCode, "Failed to initialize logging.");
					}
				}
				SQLiteLog._enabled = true;
				SQLiteLog.AddDefaultHandler();
			}
		}

		private static void InitializeDefaultHandler()
		{
			lock (SQLiteLog.syncRoot)
			{
				if (SQLiteLog._defaultHandler == null)
				{
					SQLiteLog._defaultHandler = new SQLiteLogEventHandler(SQLiteLog.LogEventHandler);
				}
			}
		}

		private static void LogCallback(IntPtr pUserData, int errorCode, IntPtr pMessage)
		{
			bool flag;
			SQLiteLogEventHandler sQLiteLogEventHandler;
			lock (SQLiteLog.syncRoot)
			{
				flag = SQLiteLog._enabled;
				if (SQLiteLog._handlers == null)
				{
					sQLiteLogEventHandler = null;
				}
				else
				{
					sQLiteLogEventHandler = SQLiteLog._handlers.Clone() as SQLiteLogEventHandler;
				}
			}
			if (flag && sQLiteLogEventHandler != null)
			{
				sQLiteLogEventHandler(null, new LogEventArgs(pUserData, (object)errorCode, SQLiteConvert.UTF8ToString(pMessage, -1), null));
			}
		}

		private static void LogEventHandler(object sender, LogEventArgs e)
		{
			if (e == null)
			{
				return;
			}
			string message = e.Message;
			if (message != null)
			{
				message = message.Trim();
				if (message.Length == 0)
				{
					message = "<empty>";
				}
			}
			else
			{
				message = "<null>";
			}
			object errorCode = e.ErrorCode;
			string str = "error";
			if (errorCode as SQLiteErrorCode? != SQLiteErrorCode.Ok || errorCode is int)
			{
				SQLiteErrorCode sQLiteErrorCode = (SQLiteErrorCode)((int)(errorCode ?? -1));
				sQLiteErrorCode &= SQLiteErrorCode.NonExtendedMask;
				if (sQLiteErrorCode == SQLiteErrorCode.Ok)
				{
					str = "message";
				}
				else if (sQLiteErrorCode == SQLiteErrorCode.Notice)
				{
					str = "notice";
				}
				else if (sQLiteErrorCode == SQLiteErrorCode.Warning)
				{
					str = "warning";
				}
				else if (sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
				{
					str = "data";
				}
			}
			else if (errorCode == null)
			{
				str = "trace";
			}
			if (errorCode == null || object.ReferenceEquals(errorCode, string.Empty))
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { str, message };
				Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "SQLite {0}: {1}", objArray));
				return;
			}
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			object[] objArray1 = new object[] { str, errorCode, message };
			Trace.WriteLine(HelperMethods.StringFormat(cultureInfo, "SQLite {0} ({1}): {2}", objArray1));
		}

		public static void LogMessage(string message)
		{
			SQLiteLog.LogMessage(null, message);
		}

		public static void LogMessage(SQLiteErrorCode errorCode, string message)
		{
			SQLiteLog.LogMessage(errorCode, message);
		}

		public static void LogMessage(int errorCode, string message)
		{
			SQLiteLog.LogMessage(errorCode, message);
		}

		private static void LogMessage(object errorCode, string message)
		{
			bool flag;
			SQLiteLogEventHandler sQLiteLogEventHandler;
			lock (SQLiteLog.syncRoot)
			{
				flag = SQLiteLog._enabled;
				if (SQLiteLog._handlers == null)
				{
					sQLiteLogEventHandler = null;
				}
				else
				{
					sQLiteLogEventHandler = SQLiteLog._handlers.Clone() as SQLiteLogEventHandler;
				}
			}
			if (flag && sQLiteLogEventHandler != null)
			{
				sQLiteLogEventHandler(null, new LogEventArgs(IntPtr.Zero, errorCode, message, null));
			}
		}

		public static void RemoveDefaultHandler()
		{
			SQLiteLog.InitializeDefaultHandler();
			SQLiteLog.Log -= SQLiteLog._defaultHandler;
		}

		private static event SQLiteLogEventHandler _handlers;

		public static event SQLiteLogEventHandler Log
		{
			add
			{
				lock (SQLiteLog.syncRoot)
				{
					SQLiteLog._handlers -= value;
					SQLiteLog._handlers += value;
				}
			}
			remove
			{
				lock (SQLiteLog.syncRoot)
				{
					SQLiteLog._handlers -= value;
				}
			}
		}
	}
}