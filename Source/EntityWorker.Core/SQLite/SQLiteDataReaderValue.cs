using System;

namespace EntityWorker.Core.SQLite
{
	public sealed class SQLiteDataReaderValue
	{
		public SQLiteBlob BlobValue;

		public bool? BooleanValue;

		public byte? ByteValue;

		public byte[] BytesValue;

		public char? CharValue;

		public char[] CharsValue;

		public DateTime? DateTimeValue;

		public decimal? DecimalValue;

		public double? DoubleValue;

		public float? FloatValue;

		public Guid? GuidValue;

		public short? Int16Value;

		public int? Int32Value;

		public long? Int64Value;

		public string StringValue;

		public object Value;

		public SQLiteDataReaderValue()
		{
		}
	}
}