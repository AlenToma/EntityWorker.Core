namespace EntityWorker.Core.Helper
{
    public enum ItemState
    {
        Added, // is the default
        Changed // this is mostly used in IDbRuleTrigger AfterSave in-case we want to re-update the record
    }

    public enum DataBaseTypes
    {
        Mssql,
        Sqllight
    }

    public enum RoundingConvention
    {
        None,
        Normal,
        RoundUpp,
        RoundDown
    }

    public enum ControlTypes
    {
        TextArea,
        Text,
        Password,
        CheckBox
    }
}
