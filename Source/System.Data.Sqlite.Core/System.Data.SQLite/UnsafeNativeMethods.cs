using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml;

namespace System.Data.SQLite
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethods
	{
		public const string ExceptionMessageFormat = "Caught exception in \"{0}\" method: {1}";

		internal const string SQLITE_DLL = "SQLite.Interop.dll";

		private readonly static string DllFileExtension;

		private readonly static string ConfigFileExtension;

		private readonly static string XmlConfigFileName;

		private readonly static string XmlConfigDirectoryToken;

		private readonly static string AssemblyDirectoryToken;

		private readonly static string TargetFrameworkToken;

		private readonly static object staticSyncRoot;

		private static Dictionary<string, string> processorArchitecturePlatforms;

		private static string cachedAssemblyDirectory;

		private static bool noAssemblyDirectory;

		private static string cachedXmlConfigFileName;

		private static bool noXmlConfigFileName;

		private readonly static string PROCESSOR_ARCHITECTURE;

		internal static string _SQLiteNativeModuleFileName;

		private static IntPtr _SQLiteNativeModuleHandle;

		static UnsafeNativeMethods()
		{
			UnsafeNativeMethods.DllFileExtension = ".dll";
			UnsafeNativeMethods.ConfigFileExtension = ".config";
			UnsafeNativeMethods.XmlConfigFileName = string.Concat(typeof(UnsafeNativeMethods).Namespace, UnsafeNativeMethods.DllFileExtension, UnsafeNativeMethods.ConfigFileExtension);
			UnsafeNativeMethods.XmlConfigDirectoryToken = "%PreLoadSQLite_XmlConfigDirectory%";
			UnsafeNativeMethods.AssemblyDirectoryToken = "%PreLoadSQLite_AssemblyDirectory%";
			UnsafeNativeMethods.TargetFrameworkToken = "%PreLoadSQLite_TargetFramework%";
			UnsafeNativeMethods.staticSyncRoot = new object();
			UnsafeNativeMethods.PROCESSOR_ARCHITECTURE = "PROCESSOR_ARCHITECTURE";
			UnsafeNativeMethods._SQLiteNativeModuleFileName = null;
			UnsafeNativeMethods._SQLiteNativeModuleHandle = IntPtr.Zero;
			UnsafeNativeMethods.Initialize();
		}

		private static string AbbreviateTargetFramework(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			value = value.Replace(".NETFramework,Version=v", "net");
			value = value.Replace(".", string.Empty);
			int num = value.IndexOf(',');
			if (num != -1)
			{
				value = value.Substring(0, num);
			}
			return value;
		}

		private static bool CheckAssemblyCodeBase(Assembly assembly, ref string fileName)
		{
			bool flag;
			try
			{
				if (assembly != null)
				{
					string codeBase = assembly.CodeBase;
					if (!string.IsNullOrEmpty(codeBase))
					{
						string localPath = (new Uri(codeBase)).LocalPath;
						if (File.Exists(localPath))
						{
							string directoryName = Path.GetDirectoryName(localPath);
							if (!File.Exists(UnsafeNativeMethods.MaybeCombinePath(directoryName, UnsafeNativeMethods.XmlConfigFileName)))
							{
								List<string> strs = null;
								if (UnsafeNativeMethods.CheckForArchitecturesAndPlatforms(directoryName, ref strs) <= 0)
								{
									flag = false;
								}
								else
								{
									fileName = localPath;
									flag = true;
								}
							}
							else
							{
								fileName = localPath;
								flag = true;
							}
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray = new object[] { exception };
					Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "Native library pre-loader failed to check code base for currently executing assembly: {0}", objArray));
				}
				catch
				{
				}
				return false;
			}
			return flag;
		}

		private static int CheckForArchitecturesAndPlatforms(string directory, ref List<string> matches)
		{
			int num = 0;
			if (matches == null)
			{
				matches = new List<string>();
			}
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				if (!string.IsNullOrEmpty(directory) && UnsafeNativeMethods.processorArchitecturePlatforms != null)
				{
					foreach (KeyValuePair<string, string> processorArchitecturePlatform in UnsafeNativeMethods.processorArchitecturePlatforms)
					{
						if (Directory.Exists(UnsafeNativeMethods.MaybeCombinePath(directory, processorArchitecturePlatform.Key)))
						{
							matches.Add(processorArchitecturePlatform.Key);
							num++;
						}
						string value = processorArchitecturePlatform.Value;
						if (value == null || !Directory.Exists(UnsafeNativeMethods.MaybeCombinePath(directory, value)))
						{
							continue;
						}
						matches.Add(value);
						num++;
					}
				}
			}
			return num;
		}

		private static string FixUpDllFileName(string fileName)
		{
			if (string.IsNullOrEmpty(fileName) || !HelperMethods.IsWindows() || fileName.EndsWith(UnsafeNativeMethods.DllFileExtension, StringComparison.OrdinalIgnoreCase))
			{
				return fileName;
			}
			return string.Concat(fileName, UnsafeNativeMethods.DllFileExtension);
		}

		private static string GetAssemblyDirectory()
		{
			string str;
			try
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				if (executingAssembly != null)
				{
					string location = null;
					if (!UnsafeNativeMethods.CheckAssemblyCodeBase(executingAssembly, ref location))
					{
						location = executingAssembly.Location;
					}
					if (!string.IsNullOrEmpty(location))
					{
						string directoryName = Path.GetDirectoryName(location);
						if (!string.IsNullOrEmpty(directoryName))
						{
							lock (UnsafeNativeMethods.staticSyncRoot)
							{
								UnsafeNativeMethods.cachedAssemblyDirectory = directoryName;
							}
							str = directoryName;
						}
						else
						{
							lock (UnsafeNativeMethods.staticSyncRoot)
							{
								UnsafeNativeMethods.noAssemblyDirectory = true;
							}
							str = null;
						}
					}
					else
					{
						lock (UnsafeNativeMethods.staticSyncRoot)
						{
							UnsafeNativeMethods.noAssemblyDirectory = true;
						}
						str = null;
					}
				}
				else
				{
					lock (UnsafeNativeMethods.staticSyncRoot)
					{
						UnsafeNativeMethods.noAssemblyDirectory = true;
					}
					str = null;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray = new object[] { exception };
					Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "Native library pre-loader failed to get directory for currently executing assembly: {0}", objArray));
				}
				catch
				{
				}
				lock (UnsafeNativeMethods.staticSyncRoot)
				{
					UnsafeNativeMethods.noAssemblyDirectory = true;
				}
				return null;
			}
			return str;
		}

		private static string GetAssemblyTargetFramework(Assembly assembly)
		{
			if (assembly != null)
			{
				return ".NETFramework,Version=v3.5";
			}
			return null;
		}

		private static string GetBaseDirectory()
		{
			string settingValue = UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_BaseDirectory", null);
			if (settingValue != null)
			{
				return settingValue;
			}
			if (UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_UseAssemblyDirectory", null) != null)
			{
				settingValue = UnsafeNativeMethods.GetAssemblyDirectory();
				if (settingValue != null)
				{
					return settingValue;
				}
			}
			return AppDomain.CurrentDomain.BaseDirectory;
		}

		private static string GetCachedAssemblyDirectory()
		{
			string str;
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				if (UnsafeNativeMethods.cachedAssemblyDirectory != null)
				{
					str = UnsafeNativeMethods.cachedAssemblyDirectory;
				}
				else if (!UnsafeNativeMethods.noAssemblyDirectory)
				{
					return UnsafeNativeMethods.GetAssemblyDirectory();
				}
				else
				{
					str = null;
				}
			}
			return str;
		}

		private static string GetCachedXmlConfigFileName()
		{
			string str;
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				if (UnsafeNativeMethods.cachedXmlConfigFileName != null)
				{
					str = UnsafeNativeMethods.cachedXmlConfigFileName;
				}
				else if (!UnsafeNativeMethods.noXmlConfigFileName)
				{
					return UnsafeNativeMethods.GetXmlConfigFileName();
				}
				else
				{
					str = null;
				}
			}
			return str;
		}

		internal static string GetNativeLibraryFileNameOnly()
		{
			string settingValue = UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_LibraryFileNameOnly", null);
			if (settingValue != null)
			{
				return settingValue;
			}
			return "SQLite.Interop.dll";
		}

		private static string GetPlatformName(string processorArchitecture)
		{
			string str;
			string str1;
			if (processorArchitecture == null)
			{
				processorArchitecture = UnsafeNativeMethods.GetProcessorArchitecture();
			}
			if (string.IsNullOrEmpty(processorArchitecture))
			{
				return null;
			}
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				if (UnsafeNativeMethods.processorArchitecturePlatforms == null)
				{
					str1 = null;
				}
				else if (!UnsafeNativeMethods.processorArchitecturePlatforms.TryGetValue(processorArchitecture, out str))
				{
					return null;
				}
				else
				{
					str1 = str;
				}
			}
			return str1;
		}

		private static string GetProcessorArchitecture()
		{
			string settingValue = UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_ProcessorArchitecture", null);
			if (settingValue != null)
			{
				return settingValue;
			}
			settingValue = UnsafeNativeMethods.GetSettingValue(UnsafeNativeMethods.PROCESSOR_ARCHITECTURE, null);
			if (IntPtr.Size == 4 && string.Equals(settingValue, "AMD64", StringComparison.OrdinalIgnoreCase))
			{
				settingValue = "x86";
			}
			return settingValue;
		}

		internal static string GetSettingValue(string name, string @default)
		{
			if (Environment.GetEnvironmentVariable("No_SQLiteGetSettingValue") != null)
			{
				return @default;
			}
			if (name == null)
			{
				return @default;
			}
			bool flag = true;
			string environmentVariable = null;
			if (Environment.GetEnvironmentVariable("No_Expand") == null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { name };
				if (Environment.GetEnvironmentVariable(HelperMethods.StringFormat(invariantCulture, "No_Expand_{0}", objArray)) != null)
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			environmentVariable = Environment.GetEnvironmentVariable(name);
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				if (flag)
				{
					environmentVariable = Environment.ExpandEnvironmentVariables(environmentVariable);
				}
				environmentVariable = UnsafeNativeMethods.ReplaceEnvironmentVariableTokens(environmentVariable);
			}
			if (environmentVariable != null)
			{
				return environmentVariable;
			}
			if (Environment.GetEnvironmentVariable("No_SQLiteXmlConfigFile") != null)
			{
				return @default;
			}
			return UnsafeNativeMethods.GetSettingValueViaXmlConfigFile(UnsafeNativeMethods.GetCachedXmlConfigFileName(), name, @default, flag);
		}

		private static string GetSettingValueViaXmlConfigFile(string fileName, string name, string @default, bool expand)
		{
			string str;
			try
			{
				if (fileName == null || name == null)
				{
					str = @default;
				}
				else
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(fileName);
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { name };
					XmlElement xmlElement = xmlDocument.SelectSingleNode(HelperMethods.StringFormat(invariantCulture, "/configuration/appSettings/add[@key='{0}']", objArray)) as XmlElement;
					if (xmlElement != null)
					{
						string attribute = null;
						if (xmlElement.HasAttribute("value"))
						{
							attribute = xmlElement.GetAttribute("value");
						}
						if (!string.IsNullOrEmpty(attribute))
						{
							if (expand)
							{
								attribute = Environment.ExpandEnvironmentVariables(attribute);
							}
							attribute = UnsafeNativeMethods.ReplaceEnvironmentVariableTokens(attribute);
							attribute = UnsafeNativeMethods.ReplaceXmlConfigFileTokens(fileName, attribute);
						}
						if (attribute != null)
						{
							str = attribute;
							return str;
						}
					}
					return @default;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray1 = new object[] { name, fileName, exception };
					Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "Native library pre-loader failed to get setting \"{0}\" value from XML configuration file \"{1}\": {2}", objArray1));
				}
				catch
				{
				}
				return @default;
			}
			return str;
		}

		private static string GetXmlConfigFileName()
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string str = UnsafeNativeMethods.MaybeCombinePath(baseDirectory, UnsafeNativeMethods.XmlConfigFileName);
			if (File.Exists(str))
			{
				lock (UnsafeNativeMethods.staticSyncRoot)
				{
					UnsafeNativeMethods.cachedXmlConfigFileName = str;
				}
				return str;
			}
			str = UnsafeNativeMethods.MaybeCombinePath(UnsafeNativeMethods.GetCachedAssemblyDirectory(), UnsafeNativeMethods.XmlConfigFileName);
			if (File.Exists(str))
			{
				lock (UnsafeNativeMethods.staticSyncRoot)
				{
					UnsafeNativeMethods.cachedXmlConfigFileName = str;
				}
				return str;
			}
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				UnsafeNativeMethods.noXmlConfigFileName = true;
			}
			return null;
		}

		internal static void Initialize()
		{
			HelperMethods.MaybeBreakIntoDebugger();
			if (UnsafeNativeMethods.GetSettingValue("No_PreLoadSQLite", null) != null)
			{
				return;
			}
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				if (UnsafeNativeMethods.processorArchitecturePlatforms == null)
				{
					UnsafeNativeMethods.processorArchitecturePlatforms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
					{
						{ "x86", "Win32" },
						{ "AMD64", "x64" },
						{ "IA64", "Itanium" },
						{ "ARM", "WinCE" }
					};
				}
				if (UnsafeNativeMethods._SQLiteNativeModuleHandle == IntPtr.Zero)
				{
					string str = null;
					string str1 = null;
					UnsafeNativeMethods.SearchForDirectory(ref str, ref str1);
					UnsafeNativeMethods.PreLoadSQLiteDll(str, str1, ref UnsafeNativeMethods._SQLiteNativeModuleFileName, ref UnsafeNativeMethods._SQLiteNativeModuleHandle);
				}
			}
		}

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr interop_compileoption_get(int N);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int interop_compileoption_used(IntPtr zOptName);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr interop_libversion();

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr interop_sourceid();

		private static string ListToString(IList<string> list)
		{
			if (list == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string str in list)
			{
				if (str == null)
				{
					continue;
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(str);
			}
			return stringBuilder.ToString();
		}

		private static string MaybeCombinePath(string path1, string path2)
		{
			if (path1 == null)
			{
				if (path2 != null)
				{
					return path2;
				}
				return null;
			}
			if (path2 == null)
			{
				return path1;
			}
			return Path.Combine(path1, path2);
		}

		private static bool PreLoadSQLiteDll(string baseDirectory, string processorArchitecture, ref string nativeModuleFileName, ref IntPtr nativeModuleHandle)
		{
			bool flag;
			if (baseDirectory == null)
			{
				baseDirectory = UnsafeNativeMethods.GetBaseDirectory();
			}
			if (baseDirectory == null)
			{
				return false;
			}
			string nativeLibraryFileNameOnly = UnsafeNativeMethods.GetNativeLibraryFileNameOnly();
			if (nativeLibraryFileNameOnly == null)
			{
				return false;
			}
			string str = UnsafeNativeMethods.FixUpDllFileName(UnsafeNativeMethods.MaybeCombinePath(baseDirectory, nativeLibraryFileNameOnly));
			if (File.Exists(str))
			{
				return false;
			}
			if (processorArchitecture == null)
			{
				processorArchitecture = UnsafeNativeMethods.GetProcessorArchitecture();
			}
			if (processorArchitecture == null)
			{
				return false;
			}
			str = UnsafeNativeMethods.FixUpDllFileName(UnsafeNativeMethods.MaybeCombinePath(UnsafeNativeMethods.MaybeCombinePath(baseDirectory, processorArchitecture), nativeLibraryFileNameOnly));
			if (!File.Exists(str))
			{
				string platformName = UnsafeNativeMethods.GetPlatformName(processorArchitecture);
				if (platformName == null)
				{
					return false;
				}
				str = UnsafeNativeMethods.FixUpDllFileName(UnsafeNativeMethods.MaybeCombinePath(UnsafeNativeMethods.MaybeCombinePath(baseDirectory, platformName), nativeLibraryFileNameOnly));
				if (!File.Exists(str))
				{
					return false;
				}
			}
			try
			{
				try
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray = new object[] { str };
					Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "Native library pre-loader is trying to load native SQLite library \"{0}\"...", objArray));
				}
				catch
				{
				}
				nativeModuleFileName = str;
				nativeModuleHandle = NativeLibraryHelper.LoadLibrary(str);
				flag = nativeModuleHandle != IntPtr.Zero;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				try
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					CultureInfo cultureInfo = CultureInfo.CurrentCulture;
					object[] objArray1 = new object[] { str, lastWin32Error, exception };
					Trace.WriteLine(HelperMethods.StringFormat(cultureInfo, "Native library pre-loader failed to load native SQLite library \"{0}\" (getLastError = {1}): {2}", objArray1));
				}
				catch
				{
				}
				return false;
			}
			return flag;
		}

		private static string ReplaceEnvironmentVariableTokens(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				string cachedAssemblyDirectory = UnsafeNativeMethods.GetCachedAssemblyDirectory();
				if (!string.IsNullOrEmpty(cachedAssemblyDirectory))
				{
					try
					{
						value = value.Replace(UnsafeNativeMethods.AssemblyDirectoryToken, cachedAssemblyDirectory);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						try
						{
							CultureInfo currentCulture = CultureInfo.CurrentCulture;
							object[] objArray = new object[] { exception };
							Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "Native library pre-loader failed to replace assembly directory token: {0}", objArray));
						}
						catch
						{
						}
					}
				}
				Assembly executingAssembly = null;
				try
				{
					executingAssembly = Assembly.GetExecutingAssembly();
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					try
					{
						CultureInfo cultureInfo = CultureInfo.CurrentCulture;
						object[] objArray1 = new object[] { exception2 };
						Trace.WriteLine(HelperMethods.StringFormat(cultureInfo, "Native library pre-loader failed to obtain executing assembly: {0}", objArray1));
					}
					catch
					{
					}
				}
				string str = UnsafeNativeMethods.AbbreviateTargetFramework(UnsafeNativeMethods.GetAssemblyTargetFramework(executingAssembly));
				if (!string.IsNullOrEmpty(str))
				{
					try
					{
						value = value.Replace(UnsafeNativeMethods.TargetFrameworkToken, str);
					}
					catch (Exception exception5)
					{
						Exception exception4 = exception5;
						try
						{
							CultureInfo currentCulture1 = CultureInfo.CurrentCulture;
							object[] objArray2 = new object[] { exception4 };
							Trace.WriteLine(HelperMethods.StringFormat(currentCulture1, "Native library pre-loader failed to replace target framework token: {0}", objArray2));
						}
						catch
						{
						}
					}
				}
			}
			return value;
		}

		private static string ReplaceXmlConfigFileTokens(string fileName, string value)
		{
			if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fileName))
			{
				try
				{
					string directoryName = Path.GetDirectoryName(fileName);
					if (!string.IsNullOrEmpty(directoryName))
					{
						value = value.Replace(UnsafeNativeMethods.XmlConfigDirectoryToken, directoryName);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					try
					{
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						object[] objArray = new object[] { fileName, exception };
						Trace.WriteLine(HelperMethods.StringFormat(currentCulture, "Native library pre-loader failed to replace XML configuration file \"{0}\" tokens: {1}", objArray));
					}
					catch
					{
					}
				}
			}
			return value;
		}

		private static void ResetCachedAssemblyDirectory()
		{
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				UnsafeNativeMethods.cachedAssemblyDirectory = null;
				UnsafeNativeMethods.noAssemblyDirectory = false;
			}
		}

		private static void ResetCachedXmlConfigFileName()
		{
			lock (UnsafeNativeMethods.staticSyncRoot)
			{
				UnsafeNativeMethods.cachedXmlConfigFileName = null;
				UnsafeNativeMethods.noXmlConfigFileName = false;
			}
		}

		private static bool SearchForDirectory(ref string baseDirectory, ref string processorArchitecture)
		{
			if (UnsafeNativeMethods.GetSettingValue("PreLoadSQLite_NoSearchForDirectory", null) != null)
			{
				return false;
			}
			string nativeLibraryFileNameOnly = UnsafeNativeMethods.GetNativeLibraryFileNameOnly();
			if (nativeLibraryFileNameOnly == null)
			{
				return false;
			}
			string[] assemblyDirectory = new string[] { UnsafeNativeMethods.GetAssemblyDirectory(), AppDomain.CurrentDomain.BaseDirectory };
			string[] strArrays = assemblyDirectory;
			string[] strArrays1 = new string[] { UnsafeNativeMethods.GetProcessorArchitecture(), UnsafeNativeMethods.GetPlatformName(null) };
			string[] strArrays2 = strArrays1;
			string[] strArrays3 = strArrays;
			for (int i = 0; i < (int)strArrays3.Length; i++)
			{
				string str = strArrays3[i];
				if (str != null)
				{
					string[] strArrays4 = strArrays2;
					for (int j = 0; j < (int)strArrays4.Length; j++)
					{
						string str1 = strArrays4[j];
						if (str1 != null && File.Exists(UnsafeNativeMethods.FixUpDllFileName(UnsafeNativeMethods.MaybeCombinePath(UnsafeNativeMethods.MaybeCombinePath(str, str1), nativeLibraryFileNameOnly))))
						{
							baseDirectory = str;
							processorArchitecture = str1;
							return true;
						}
					}
				}
			}
			return false;
		}

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_aggregate_count(IntPtr context);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_backup_finish_interop(IntPtr backup);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_backup_init(IntPtr destDb, byte[] zDestName, IntPtr sourceDb, byte[] zSourceName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_backup_pagecount(IntPtr backup);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_backup_remaining(IntPtr backup);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_backup_step(IntPtr backup, int nPage);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_blob(IntPtr stmt, int index, byte[] value, int nSize, IntPtr nTransient);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_double(IntPtr stmt, int index, double value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_int(IntPtr stmt, int index, int value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_int64(IntPtr stmt, int index, long value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_null(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_bind_parameter_name_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_text16(IntPtr stmt, int index, string value, int nlen, IntPtr pvReserved);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_bind_int", ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_uint(IntPtr stmt, int index, uint value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_bind_int64", ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_bind_uint64(IntPtr stmt, int index, ulong value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_blob_bytes(IntPtr blob);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_blob_close(IntPtr blob);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_blob_close_interop(IntPtr blob);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_blob_open(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, long rowId, int flags, ref IntPtr ptrBlob);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_blob_read(IntPtr blob, byte[] buffer, int count, int offset);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_blob_reopen(IntPtr blob, long rowId);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_blob_write(IntPtr blob, byte[] buffer, int count, int offset);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_busy_timeout(IntPtr db, int ms);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_changes(IntPtr db);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_changes_interop(IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_clear_bindings(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_close_interop(IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_column_bytes16(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_column_count(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_database_name_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_database_name16_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_decltype_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_decltype16_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern double sqlite3_column_double(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_column_int(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern long sqlite3_column_int64(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_name_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_name16_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_origin_name_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_origin_name16_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_table_name_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_table_name16_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_text_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_column_text16_interop(IntPtr stmt, int index, ref int len);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SQLiteCommitCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_compileoption_get(int N);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_compileoption_used(IntPtr zOptName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_config", ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_config_int(SQLiteConfigOpsEnum op, int value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_config", ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_config_log(SQLiteConfigOpsEnum op, SQLiteLogCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_config", ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_config_none(SQLiteConfigOpsEnum op);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_context_collcompare_interop(IntPtr context, byte[] p1, int p1len, byte[] p2, int p2len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_context_collseq_interop(IntPtr context, ref int type, ref int enc, ref int len);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_create_collation(IntPtr db, byte[] strName, int nType, IntPtr pvUser, SQLiteCollation func);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_create_disposable_module(IntPtr db, IntPtr name, ref UnsafeNativeMethods.sqlite3_module module, IntPtr pClientData, UnsafeNativeMethods.xDestroyModule xDestroy);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_create_function_interop(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal, int needCollSeq);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_cursor_rowid_interop(IntPtr stmt, int cursor, ref long rowid);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_db_config", ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_db_config_int_refint(IntPtr db, SQLiteConfigDbOpsEnum op, int value, ref int result);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_db_filename(IntPtr db, IntPtr dbName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, EntryPoint="sqlite3_db_filename", ExactSpelling=false)]
		internal static extern IntPtr sqlite3_db_filename_bytes(IntPtr db, byte[] dbName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_db_handle(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_db_readonly(IntPtr db, IntPtr dbName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_db_release_memory(IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_declare_vtab(IntPtr db, IntPtr zSQL);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_dispose_module(IntPtr pModule);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_enable_load_extension(IntPtr db, int enable);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_enable_shared_cache(int enable);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_errcode(IntPtr db);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_errmsg_interop(IntPtr db, ref int len);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_errstr(SQLiteErrorCode rc);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, ref IntPtr errMsg);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_extended_errcode(IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_extended_result_codes(IntPtr db, int onoff);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_file_control(IntPtr db, byte[] zDbName, int op, IntPtr pArg);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_finalize_interop(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_free(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_get_autocommit(IntPtr db);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_index_column_info_interop(IntPtr db, byte[] catalog, byte[] IndexName, byte[] ColumnName, ref int sortOrder, ref int onError, ref IntPtr Collation, ref int colllen);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_interrupt(IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_key(IntPtr db, byte[] key, int keylen);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern long sqlite3_last_insert_rowid(IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_libversion();

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_libversion_number();

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_load_extension(IntPtr db, byte[] fileName, byte[] procName, ref IntPtr pError);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_log(SQLiteErrorCode iErrCode, byte[] zFormat);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_malloc(int n);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_malloc_size_interop(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern long sqlite3_memory_highwater(int resetFlag);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern long sqlite3_memory_used();

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_mprintf(IntPtr format);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_open_interop(byte[] utf8Filename, byte[] vfsName, SQLiteOpenFlagsEnum flags, int extFuncs, ref IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_open_v2(byte[] utf8Filename, ref IntPtr db, SQLiteOpenFlagsEnum flags, byte[] vfsName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_open16(string fileName, ref IntPtr db);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_open16_interop(byte[] utf8Filename, byte[] vfsName, SQLiteOpenFlagsEnum flags, int extFuncs, ref IntPtr db);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_overload_function(IntPtr db, IntPtr zName, int nArgs);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_prepare_interop(IntPtr db, IntPtr pSql, int nBytes, ref IntPtr stmt, ref IntPtr ptrRemain, ref int nRemain);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_progress_handler(IntPtr db, int ops, SQLiteProgressCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_realloc(IntPtr p, int n);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_rekey(IntPtr db, byte[] key, int keylen);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_release_memory(int nBytes);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_reset_interop(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_double(IntPtr context, double value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_error_code(IntPtr context, SQLiteErrorCode value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_error_nomem(IntPtr context);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_error_toobig(IntPtr context);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern void sqlite3_result_error16(IntPtr context, string strName, int nLen);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_int(IntPtr context, int value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_int64(IntPtr context, long value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_null(IntPtr context);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern void sqlite3_result_text16(IntPtr context, string strName, int nLen, IntPtr pvReserved);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_value(IntPtr context, IntPtr value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3_result_zeroblob(IntPtr context, int nLen);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_set_authorizer(IntPtr db, SQLiteAuthorizerCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_shutdown();

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_sourceid();

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_step(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_stmt_readonly(IntPtr stmt);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_table_column_metadata_interop(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, ref IntPtr ptrDataType, ref IntPtr ptrCollSeq, ref int notNull, ref int primaryKey, ref int autoInc, ref int dtLen, ref int csLen);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_table_cursor_interop(IntPtr stmt, int db, int tableRootPage);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_trace(IntPtr db, SQLiteTraceCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_update_hook(IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_value_blob(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_value_bytes(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_value_bytes16(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern double sqlite3_value_double(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3_value_int(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern long sqlite3_value_int64(IntPtr p);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_value_text_interop(IntPtr p, ref int len);

		[DllImport("SQLite.Interop.dll", CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern IntPtr sqlite3_value_text16_interop(IntPtr p, ref int len);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_win32_compact_heap(ref uint largest);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_win32_reset_heap();

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3_win32_set_directory(uint type, string value);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changegroup_add(IntPtr changeGroup, int nData, IntPtr pData);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changegroup_add_strm(IntPtr changeGroup, UnsafeNativeMethods.xSessionInput xInput, IntPtr pIn);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3changegroup_delete(IntPtr changeGroup);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changegroup_new(ref IntPtr changeGroup);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changegroup_output(IntPtr changeGroup, ref int nData, ref IntPtr pData);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changegroup_output_strm(IntPtr changeGroup, UnsafeNativeMethods.xSessionOutput xOutput, IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_apply(IntPtr db, int nChangeSet, IntPtr pChangeSet, UnsafeNativeMethods.xSessionFilter xFilter, UnsafeNativeMethods.xSessionConflict xConflict, IntPtr context);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_apply_strm(IntPtr db, UnsafeNativeMethods.xSessionInput xInput, IntPtr pIn, UnsafeNativeMethods.xSessionFilter xFilter, UnsafeNativeMethods.xSessionConflict xConflict, IntPtr context);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_concat(int nA, IntPtr pA, int nB, IntPtr pB, ref int nOut, ref IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_concat_strm(UnsafeNativeMethods.xSessionInput xInputA, IntPtr pInA, UnsafeNativeMethods.xSessionInput xInputB, IntPtr pInB, UnsafeNativeMethods.xSessionOutput xOutput, IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_conflict(IntPtr iterator, int columnIndex, ref IntPtr pValue);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_finalize(IntPtr iterator);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_fk_conflicts(IntPtr iterator, ref int conflicts);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_invert(int nIn, IntPtr pIn, ref int nOut, ref IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_invert_strm(UnsafeNativeMethods.xSessionInput xInput, IntPtr pIn, UnsafeNativeMethods.xSessionOutput xOutput, IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_new(IntPtr iterator, int columnIndex, ref IntPtr pValue);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_next(IntPtr iterator);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_old(IntPtr iterator, int columnIndex, ref IntPtr pValue);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_op(IntPtr iterator, ref IntPtr pTblName, ref int nColumns, ref SQLiteAuthorizerActionCode op, ref int bIndirect);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_pk(IntPtr iterator, ref IntPtr pPrimaryKeys, ref int nColumns);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_start(ref IntPtr iterator, int nChangeSet, IntPtr pChangeSet);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3changeset_start_strm(ref IntPtr iterator, UnsafeNativeMethods.xSessionInput xInput, IntPtr pIn);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_attach(IntPtr session, byte[] tblName);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_changeset(IntPtr session, ref int nChangeSet, ref IntPtr pChangeSet);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_changeset_strm(IntPtr session, UnsafeNativeMethods.xSessionOutput xOutput, IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_create(IntPtr db, byte[] dbName, ref IntPtr session);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3session_delete(IntPtr session);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_diff(IntPtr session, byte[] fromDbName, byte[] tblName, ref IntPtr errMsg);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3session_enable(IntPtr session, int enable);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3session_indirect(IntPtr session, int indirect);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern int sqlite3session_isempty(IntPtr session);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_patchset(IntPtr session, ref int nPatchSet, ref IntPtr pPatchSet);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern SQLiteErrorCode sqlite3session_patchset_strm(IntPtr session, UnsafeNativeMethods.xSessionOutput xOutput, IntPtr pOut);

		[DllImport("SQLite.Interop.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.None, ExactSpelling=false)]
		internal static extern void sqlite3session_table_filter(IntPtr session, UnsafeNativeMethods.xSessionFilter xFilter, IntPtr context);

		internal struct sqlite3_index_constraint
		{
			public int iColumn;

			public SQLiteIndexConstraintOp op;

			public byte usable;

			public int iTermOffset;

			public sqlite3_index_constraint(SQLiteIndexConstraint constraint)
			{
				this = new UnsafeNativeMethods.sqlite3_index_constraint();
				if (constraint != null)
				{
					this.iColumn = constraint.iColumn;
					this.op = constraint.op;
					this.usable = constraint.usable;
					this.iTermOffset = constraint.iTermOffset;
				}
			}
		}

		internal struct sqlite3_index_constraint_usage
		{
			public int argvIndex;

			public byte omit;

			public sqlite3_index_constraint_usage(SQLiteIndexConstraintUsage constraintUsage)
			{
				this = new UnsafeNativeMethods.sqlite3_index_constraint_usage();
				if (constraintUsage != null)
				{
					this.argvIndex = constraintUsage.argvIndex;
					this.omit = constraintUsage.omit;
				}
			}
		}

		internal struct sqlite3_index_info
		{
			public int nConstraint;

			public IntPtr aConstraint;

			public int nOrderBy;

			public IntPtr aOrderBy;

			public IntPtr aConstraintUsage;

			public int idxNum;

			public string idxStr;

			public int needToFreeIdxStr;

			public int orderByConsumed;

			public double estimatedCost;

			public long estimatedRows;

			public SQLiteIndexFlags idxFlags;

			public long colUsed;
		}

		internal struct sqlite3_index_orderby
		{
			public int iColumn;

			public byte desc;

			public sqlite3_index_orderby(SQLiteIndexOrderBy orderBy)
			{
				this = new UnsafeNativeMethods.sqlite3_index_orderby();
				if (orderBy != null)
				{
					this.iColumn = orderBy.iColumn;
					this.desc = orderBy.desc;
				}
			}
		}

		internal struct sqlite3_module
		{
			public int iVersion;

			public UnsafeNativeMethods.xCreate xCreate;

			public UnsafeNativeMethods.xConnect xConnect;

			public UnsafeNativeMethods.xBestIndex xBestIndex;

			public UnsafeNativeMethods.xDisconnect xDisconnect;

			public UnsafeNativeMethods.xDestroy xDestroy;

			public UnsafeNativeMethods.xOpen xOpen;

			public UnsafeNativeMethods.xClose xClose;

			public UnsafeNativeMethods.xFilter xFilter;

			public UnsafeNativeMethods.xNext xNext;

			public UnsafeNativeMethods.xEof xEof;

			public UnsafeNativeMethods.xColumn xColumn;

			public UnsafeNativeMethods.xRowId xRowId;

			public UnsafeNativeMethods.xUpdate xUpdate;

			public UnsafeNativeMethods.xBegin xBegin;

			public UnsafeNativeMethods.xSync xSync;

			public UnsafeNativeMethods.xCommit xCommit;

			public UnsafeNativeMethods.xRollback xRollback;

			public UnsafeNativeMethods.xFindFunction xFindFunction;

			public UnsafeNativeMethods.xRename xRename;

			public UnsafeNativeMethods.xSavepoint xSavepoint;

			public UnsafeNativeMethods.xRelease xRelease;

			public UnsafeNativeMethods.xRollbackTo xRollbackTo;
		}

		internal struct sqlite3_vtab
		{
			public IntPtr pModule;

			public int nRef;

			public IntPtr zErrMsg;
		}

		internal struct sqlite3_vtab_cursor
		{
			public IntPtr pVTab;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xBegin(IntPtr pVtab);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xBestIndex(IntPtr pVtab, IntPtr pIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xClose(IntPtr pCursor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xColumn(IntPtr pCursor, IntPtr pContext, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xCommit(IntPtr pVtab);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xConnect(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xCreate(IntPtr pDb, IntPtr pAux, int argc, IntPtr argv, ref IntPtr pVtab, ref IntPtr pError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xDestroy(IntPtr pVtab);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void xDestroyModule(IntPtr pClientData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xDisconnect(IntPtr pVtab);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int xEof(IntPtr pCursor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xFilter(IntPtr pCursor, int idxNum, IntPtr idxStr, int argc, IntPtr argv);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int xFindFunction(IntPtr pVtab, int nArg, IntPtr zName, ref SQLiteCallback callback, ref IntPtr pUserData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xNext(IntPtr pCursor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xOpen(IntPtr pVtab, ref IntPtr pCursor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xRelease(IntPtr pVtab, int iSavepoint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xRename(IntPtr pVtab, IntPtr zNew);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xRollback(IntPtr pVtab);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xRollbackTo(IntPtr pVtab, int iSavepoint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xRowId(IntPtr pCursor, ref long rowId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xSavepoint(IntPtr pVtab, int iSavepoint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SQLiteChangeSetConflictResult xSessionConflict(IntPtr context, SQLiteChangeSetConflictType type, IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int xSessionFilter(IntPtr context, IntPtr pTblName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SQLiteErrorCode xSessionInput(IntPtr context, IntPtr pData, ref int nData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SQLiteErrorCode xSessionOutput(IntPtr context, IntPtr pData, int nData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xSync(IntPtr pVtab);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate SQLiteErrorCode xUpdate(IntPtr pVtab, int argc, IntPtr argv, ref long rowId);
	}
}