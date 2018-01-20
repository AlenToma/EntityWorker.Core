using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace EntityWorker.Core.Properties
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
                return EntityWorker.Core.Properties.SR.resourceCulture;
            }
            set
            {
                Properties.SR.resourceCulture = value;
            }
        }

        internal static string DataTypes
        {
            get
            {
                return Properties.SR.ResourceManager.GetString("DataTypes", Properties.SR.resourceCulture);
            }
        }

        internal static string Keywords
        {
            get
            {
                return Properties.SR.ResourceManager.GetString("Keywords", Properties.SR.resourceCulture);
            }
        }

        internal static string MetaDataCollections
        {
            get
            {
                return Properties.SR.ResourceManager.GetString("MetaDataCollections", Properties.SR.resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(Properties.SR.resourceMan, null))
                {
                    Properties.SR.resourceMan = new System.Resources.ResourceManager("EntityWorker.SQLite.SR", typeof(Properties.SR).Assembly);
                }
                return Properties.SR.resourceMan;
            }
        }

        internal SR()
        {
        }

        internal static string GetString(string format, params object[] args)
        {
            return string.Format(format, string.Join("\n", args));
        }

        internal static string GetString(string value)
        {
            return value;
        }
    }
}