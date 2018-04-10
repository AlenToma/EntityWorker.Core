using EntityWorker.Core.Helper;
using EntityWorker.Core.Interface;
using System.Globalization;

namespace EntityWorker.Core
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/GlobalConfiguration.md
    /// Set this in Application_Start
    /// </summary>
    public static class GlobalConfiguration
    {
        /// <summary>
        /// Default Value for DataEncode
        /// </summary>
        public static DataCipherKeySize DataEncode_Key_Size { get; set; } = DataCipherKeySize.Default;


        /// <summary>
        /// Default Value for DataEncode
        /// </summary>
        public static string DataEncode_Key { get; set; } = "EntityWorker.Default.Key.Pass";


        /// <summary>
        /// The Default Value for package encryption
        /// </summary>
        public static string PackageDataEncode_Key { get; set; } = "packageSecurityKey";

        /// <summary>
        /// EntityWorker will Use this CultureInfo to convert the data eg decimal, datetime and so on, from the database 
        /// Default is EN;
        /// </summary>
        public static CultureInfo CultureInfo { get; set; } = new CultureInfo("en");

        /// <summary>
        /// Assign the Logger class.
        /// the class must inherit from ILogg
        /// </summary>
        public static ILog Logg { get; set; }

    }
}
