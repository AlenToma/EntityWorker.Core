using System;
using System.Collections.Generic;

namespace System.Data.SQLite
{
	internal static class SQLiteDefineConstants
	{
		public readonly static IList<string> OptionList;

		static SQLiteDefineConstants()
		{
			string[] strArrays = new string[] { "INTEROP_CODEC", "INTEROP_EXTENSION_FUNCTIONS", "INTEROP_FTS5_EXTENSION", "INTEROP_JSON1_EXTENSION", "INTEROP_PERCENTILE_EXTENSION", "INTEROP_REGEXP_EXTENSION", "INTEROP_SESSION_EXTENSION", "INTEROP_SHA1_EXTENSION", "INTEROP_TOTYPE_EXTENSION", "INTEROP_VIRTUAL_TABLE", "NET_35", "PRELOAD_NATIVE_LIBRARY", "THROW_ON_DISPOSED", "TRACE", "TRACE_PRELOAD", "TRACE_SHARED", "TRACE_WARNING", "USE_INTEROP_DLL", "USE_PREPARE_V2", "WINDOWS", null };
			SQLiteDefineConstants.OptionList = new List<string>(strArrays);
		}
	}
}