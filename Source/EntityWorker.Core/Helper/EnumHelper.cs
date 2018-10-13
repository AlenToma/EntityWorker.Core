namespace EntityWorker.Core.Helper
{

    public enum JsonFormatting
    {
        Auto, // Default the name wont change eg as PropertyName
        LowerCase, // The name will be lowerCase
        CamelCase // CamelCase firstchar is LowerCase
    }

    public enum DataBaseTypes
    {
        Mssql,
        Sqllight,
        PostgreSql,
    }

    public enum RoundingConvention
    {
        None,
        Normal,
        RoundUpp,
        RoundDown
    }

    /// <summary>
    /// Default = GlobalConfiguration.DataEncode_Key_Size
    /// </summary>
    public enum DataCipherKeySize
    {
        Default,
        Key_128 = 128,
        Key_256 = 256
    }
}
