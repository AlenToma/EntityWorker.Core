using EntityWorker.Core.Helper;
using System.Globalization;

namespace EntityWorker.Core
{
    /// <summary>
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
        /// EntityWorker will Use this CultureInfo to convert the data eg decimal, datetime and so on, from the database 
        /// Default is EN;
        /// </summary>
        public static CultureInfo cultureInfo { get; set; } = new CultureInfo("en");

    }
}
