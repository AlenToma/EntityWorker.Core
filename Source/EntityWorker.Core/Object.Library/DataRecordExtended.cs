using EntityWorker.Core.Helper;
using System;
using System.Data;

namespace EntityWorker.Core.Object.Library
{
    internal class DataRecordExtended
    {
        private IDataRecord _record;

        public int FieldCount => _record.FieldCount;
        public object this[int index, string dataEnoderKey, string dataEncodeSize, int toBase64, string propertyType, string dbType, string propetyTypeFullName]
        {
            get
            {
                var value = _record[index];
                if (value != null)
                {
                    if (value as byte[] != null && dbType.Contains("Guid"))
                        value = new Guid(value as byte[]);                        
                    if (toBase64 == 1)
                    {
                        if (value.ToString().IsBase64String())
                            value = MethodHelper.DecodeStringFromBase64(value.ToString());
                    }
                    else if (dataEncodeSize != "")
                        value = new DataCipher(dataEnoderKey, dataEncodeSize.ConvertValue<DataCipherKeySize>()).Decrypt(value.ToString());
                    else if (value is DBNull || propertyType != dbType)
                        value = value.ConvertValue(Type.GetType(propetyTypeFullName) ?? GetFieldType(index));
                }
                else if (value is DBNull || propertyType != dbType) value = value.ConvertValue(Type.GetType(propetyTypeFullName) ?? GetFieldType(index));

                return value;
            }
        }

        public DataRecordExtended(IDataRecord record)
        {
            _record = record;
        }

        public Type GetFieldType(int index)
        {
            return _record.GetFieldType(index);
        }

        public string GetName(int index)
        {
            return _record.GetName(index);
        }
    }
}
