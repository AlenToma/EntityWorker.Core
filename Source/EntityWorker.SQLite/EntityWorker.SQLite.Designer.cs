using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace EntityWorker.SQLite
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
				return EntityWorker.SQLite.SR.resourceCulture;
			}
			set
			{
				EntityWorker.SQLite.SR.resourceCulture = value;
			}
		}

		internal static string DataTypes
		{
			get
			{
				return EntityWorker.SQLite.SR.ResourceManager.GetString("DataTypes", EntityWorker.SQLite.SR.resourceCulture);
			}
		}

		internal static string Keywords
		{
			get
			{
				return EntityWorker.SQLite.SR.ResourceManager.GetString("Keywords", EntityWorker.SQLite.SR.resourceCulture);
			}
		}

		internal static string MetaDataCollections
		{
			get
			{
				return EntityWorker.SQLite.SR.ResourceManager.GetString("MetaDataCollections", EntityWorker.SQLite.SR.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(EntityWorker.SQLite.SR.resourceMan, null))
				{
					EntityWorker.SQLite.SR.resourceMan = new System.Resources.ResourceManager("EntityWorker.SQLite.SR", typeof(EntityWorker.SQLite.SR).Assembly);
				}
				return EntityWorker.SQLite.SR.resourceMan;
			}
		}

		internal SR()
		{
		}
	}
}