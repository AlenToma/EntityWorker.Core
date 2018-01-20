using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace EntityWorker.Core.SQLite
{
	internal static class HelperMethods
	{
		private const string DisplayNullObject = "<nullObject>";

		private const string DisplayEmptyString = "<emptyString>";

		private const string DisplayStringFormat = "\"{0}\"";

		private const string DisplayNullArray = "<nullArray>";

		private const string DisplayEmptyArray = "<emptyArray>";

		private const char ArrayOpen = '[';

		private const string ElementSeparator = ", ";

		private const char ArrayClose = ']';

		private readonly static char[] SpaceChars;

		private readonly static object staticSyncRoot;

		private readonly static string MonoRuntimeType;

		private static bool? isMono;

		private static bool? debuggerBreak;

		static HelperMethods()
		{
			HelperMethods.SpaceChars = new char[] { '\t', '\n', '\r', '\v', '\f', ' ' };
			HelperMethods.staticSyncRoot = new object();
			HelperMethods.MonoRuntimeType = "Mono.Runtime";
			HelperMethods.isMono = null;
			HelperMethods.debuggerBreak = null;
		}

		private static int GetProcessId()
		{
			Process currentProcess = Process.GetCurrentProcess();
			if (currentProcess == null)
			{
				return 0;
			}
			return currentProcess.Id;
		}

		private static bool IsMono()
		{
			bool value;
			try
			{
				lock (HelperMethods.staticSyncRoot)
				{
					if (!HelperMethods.isMono.HasValue)
					{
						HelperMethods.isMono = new bool?(Type.GetType(HelperMethods.MonoRuntimeType) != null);
					}
					value = HelperMethods.isMono.Value;
				}
			}
			catch
			{
				return false;
			}
			return value;
		}

		internal static bool IsWindows()
		{
			PlatformID platform = Environment.OSVersion.Platform;
			if (platform != PlatformID.Win32S && platform != PlatformID.Win32Windows && platform != PlatformID.Win32NT && platform != PlatformID.WinCE)
			{
				return false;
			}
			return true;
		}

		internal static bool LogBackup(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogBackup;
			return flags == SQLiteConnectionFlags.LogBackup;
		}

		internal static bool LogBind(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogBind;
			return flags == SQLiteConnectionFlags.LogBind;
		}

		internal static bool LogCallbackExceptions(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogCallbackException;
			return flags == SQLiteConnectionFlags.LogCallbackException;
		}

		internal static bool LogModuleError(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogModuleError;
			return flags == SQLiteConnectionFlags.LogModuleError;
		}

		internal static bool LogModuleException(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogModuleException;
			return flags == SQLiteConnectionFlags.LogModuleException;
		}

		internal static bool LogPreBind(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogPreBind;
			return flags == SQLiteConnectionFlags.LogPreBind;
		}

		internal static bool LogPrepare(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.LogPrepare;
			return flags == SQLiteConnectionFlags.LogPrepare;
		}

		internal static void MaybeBreakIntoDebugger()
		{
			lock (HelperMethods.staticSyncRoot)
			{
				if (HelperMethods.debuggerBreak.HasValue)
				{
					return;
				}
			}
			if (UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_BreakIntoDebugger", null) != null)
			{
				try
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] processId = new object[] { HelperMethods.GetProcessId() };
					Console.WriteLine(HelperMethods.StringFormat(currentCulture, "Attach a debugger to process {0} and press any key to continue.", processId));
					Console.ReadKey();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						CultureInfo cultureInfo = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { typeof(Console), exception };
						Trace.WriteLine(HelperMethods.StringFormat(cultureInfo, "Failed to issue debugger prompt, {0} may be unusable: {1}", objArray));
					}
					catch
					{
					}
				}
				try
				{
					Debugger.Break();
					lock (HelperMethods.staticSyncRoot)
					{
						HelperMethods.debuggerBreak = new bool?(true);
					}
				}
				catch
				{
					lock (HelperMethods.staticSyncRoot)
					{
						HelperMethods.debuggerBreak = new bool?(false);
					}
					throw;
				}
			}
		}

		internal static bool NoLogModule(SQLiteConnectionFlags flags)
		{
			flags &= SQLiteConnectionFlags.NoLogModule;
			return flags == SQLiteConnectionFlags.NoLogModule;
		}

		internal static string StringFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (HelperMethods.IsMono())
			{
				return string.Format(format, args);
			}
			return string.Format(provider, format, args);
		}

		public static string ToDisplayString(object value)
		{
			if (value == null)
			{
				return "<nullObject>";
			}
			string str = value.ToString();
			if (str.Length == 0)
			{
				return "<emptyString>";
			}
			if (str.IndexOfAny(HelperMethods.SpaceChars) < 0)
			{
				return str;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { str };
			return HelperMethods.StringFormat(invariantCulture, "\"{0}\"", objArray);
		}

		public static string ToDisplayString(Array array)
		{
			if (array == null)
			{
				return "<nullArray>";
			}
			if (array.Length == 0)
			{
				return "<emptyArray>";
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (object obj in array)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(HelperMethods.ToDisplayString(obj));
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Insert(0, '[');
				stringBuilder.Append(']');
			}
			return stringBuilder.ToString();
		}
	}
}