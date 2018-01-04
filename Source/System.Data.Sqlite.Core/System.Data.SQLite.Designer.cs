using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace System.Data.SQLite
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal sealed class SR
	{
		private static System.Resources.ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return System.Data.SQLite.SR.resourceCulture;
			}
			set
			{
				System.Data.SQLite.SR.resourceCulture = value;
			}
		}

		internal static string DataTypes
		{
			get
			{
				return System.Data.SQLite.SR.ResourceManager.GetString("DataTypes", System.Data.SQLite.SR.resourceCulture);
			}
		}

		internal static string Keywords
		{
			get
			{
				return System.Data.SQLite.SR.ResourceManager.GetString("Keywords", System.Data.SQLite.SR.resourceCulture);
			}
		}

		internal static string MetaDataCollections
		{
			get
			{
				return System.Data.SQLite.SR.ResourceManager.GetString("MetaDataCollections", System.Data.SQLite.SR.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(System.Data.SQLite.SR.resourceMan, null))
				{
					System.Data.SQLite.SR.resourceMan = new System.Resources.ResourceManager("System.Data.SQLite.SR", typeof(System.Data.SQLite.SR).Assembly);
				}
				return System.Data.SQLite.SR.resourceMan;
			}
		}

		internal SR()
		{
		}
	}
}