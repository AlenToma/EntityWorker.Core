using EntityWorker.Core.Helper;
using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// Choose to Encode the data in the database so none could read or decript it without knowing the key
    /// LinqToSql will also Encode the value when you select a Search
    /// <Example>
    /// .Where(x=> x.Password == "test") Will be equal to .Where(x=> x.Password == Encode("test"))
    /// so no need to worry when you search those column in the dataBase 
    /// you could Encode Adress, bankAccount information and so on with ease
    /// </Example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataEncode : Attribute
    {
        internal string Key { get; set; }

        internal DataCipherKeySize KeySize { get; set; }
        /// <summary>
        /// Encode Key or leave it empty to use a EntityWorker.Core Defult Key
        /// </summary>
        /// <param name="key">Default =  GlobalConfiguration.DataEncode_Key</param>
        /// <param name="keySize"> NETCOREAPP2.0 can only handle 128.  128 || 256 Default = GlobalConfiguration.DataEncode_Key_Size  </param>
        /// <param name=""></param>
        public DataEncode(string key = null, DataCipherKeySize keySize = DataCipherKeySize.Default)
        {
            if (!string.IsNullOrEmpty(key))
                Key = key;
            if (keySize != DataCipherKeySize.Default)
                KeySize = keySize;
            else keySize = GlobalConfiguration.DataEncode_Key_Size;
        }
    }
}
