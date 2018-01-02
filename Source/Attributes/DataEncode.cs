using System;

namespace EntityWorker.Core.Attributes
{
    /// <summary>
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
        internal string Key { get; set; } = "EntityWorker.Default.Key.Pass";
        /// <summary>
        /// Encode Key or leave it empty to use a EntityWorker.Core Defult Key
        /// </summary>
        /// <param name="key"></param>
        public DataEncode(string key = null)
        {
            if (!string.IsNullOrEmpty(key))
                Key = key;
        }
    }
}
