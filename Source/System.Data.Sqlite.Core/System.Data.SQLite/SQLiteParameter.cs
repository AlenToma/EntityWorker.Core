using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.SQLite
{
	public sealed class SQLiteParameter : DbParameter, ICloneable
	{
		private const System.Data.DbType UnknownDbType = System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset;

		private IDbCommand _command;

		internal System.Data.DbType _dbType;

		private DataRowVersion _rowVersion;

		private object _objValue;

		private string _sourceColumn;

		private string _parameterName;

		private int _dataSize;

		private bool _nullable;

		private bool _nullMapping;

		private string _typeName;

		public IDbCommand Command
		{
			get
			{
				return this._command;
			}
			set
			{
				this._command = value;
			}
		}

		[DbProviderSpecificTypeProperty(true)]
		[RefreshProperties(RefreshProperties.All)]
		public override System.Data.DbType DbType
		{
			get
			{
				if (this._dbType != (System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset))
				{
					return this._dbType;
				}
				if (this._objValue == null || this._objValue == DBNull.Value)
				{
					return System.Data.DbType.String;
				}
				return SQLiteConvert.TypeToDbType(this._objValue.GetType());
			}
			set
			{
				this._dbType = value;
			}
		}

		public override ParameterDirection Direction
		{
			get
			{
				return ParameterDirection.Input;
			}
			set
			{
				if (value != ParameterDirection.Input)
				{
					throw new NotSupportedException();
				}
			}
		}

		public override bool IsNullable
		{
			get
			{
				return this._nullable;
			}
			set
			{
				this._nullable = value;
			}
		}

		public override string ParameterName
		{
			get
			{
				return this._parameterName;
			}
			set
			{
				this._parameterName = value;
			}
		}

		[DefaultValue(0)]
		public override int Size
		{
			get
			{
				return this._dataSize;
			}
			set
			{
				this._dataSize = value;
			}
		}

		public override string SourceColumn
		{
			get
			{
				return this._sourceColumn;
			}
			set
			{
				this._sourceColumn = value;
			}
		}

		public override bool SourceColumnNullMapping
		{
			get
			{
				return this._nullMapping;
			}
			set
			{
				this._nullMapping = value;
			}
		}

		public override DataRowVersion SourceVersion
		{
			get
			{
				return this._rowVersion;
			}
			set
			{
				this._rowVersion = value;
			}
		}

		public string TypeName
		{
			get
			{
				return this._typeName;
			}
			set
			{
				this._typeName = value;
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(StringConverter))]
		public override object Value
		{
			get
			{
				return this._objValue;
			}
			set
			{
				this._objValue = value;
				if (this._dbType == (System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset) && this._objValue != null && this._objValue != DBNull.Value)
				{
					this._dbType = SQLiteConvert.TypeToDbType(this._objValue.GetType());
				}
			}
		}

		internal SQLiteParameter(IDbCommand command) : this()
		{
			this._command = command;
		}

		public SQLiteParameter() : this(null, System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset, 0, null, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(string parameterName) : this(parameterName, System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset, 0, null, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(string parameterName, object value) : this(parameterName, System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset, 0, null, DataRowVersion.Current)
		{
			this.Value = value;
		}

		public SQLiteParameter(string parameterName, System.Data.DbType dbType) : this(parameterName, dbType, 0, null, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(string parameterName, System.Data.DbType dbType, string sourceColumn) : this(parameterName, dbType, 0, sourceColumn, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(string parameterName, System.Data.DbType dbType, string sourceColumn, DataRowVersion rowVersion) : this(parameterName, dbType, 0, sourceColumn, rowVersion)
		{
		}

		public SQLiteParameter(System.Data.DbType dbType) : this(null, dbType, 0, null, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(System.Data.DbType dbType, object value) : this(null, dbType, 0, null, DataRowVersion.Current)
		{
			this.Value = value;
		}

		public SQLiteParameter(System.Data.DbType dbType, string sourceColumn) : this(null, dbType, 0, sourceColumn, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(System.Data.DbType dbType, string sourceColumn, DataRowVersion rowVersion) : this(null, dbType, 0, sourceColumn, rowVersion)
		{
		}

		public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize) : this(parameterName, parameterType, parameterSize, null, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, string sourceColumn) : this(parameterName, parameterType, parameterSize, sourceColumn, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, string sourceColumn, DataRowVersion rowVersion)
		{
			this._parameterName = parameterName;
			this._dbType = parameterType;
			this._sourceColumn = sourceColumn;
			this._rowVersion = rowVersion;
			this._dataSize = parameterSize;
			this._nullable = true;
		}

		private SQLiteParameter(SQLiteParameter source) : this(source.ParameterName, source._dbType, 0, source.Direction, source.IsNullable, 0, 0, source.SourceColumn, source.SourceVersion, source.Value)
		{
			this._nullMapping = source._nullMapping;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion rowVersion, object value) : this(parameterName, parameterType, parameterSize, sourceColumn, rowVersion)
		{
			this.Direction = direction;
			this.IsNullable = isNullable;
			this.Value = value;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion rowVersion, bool sourceColumnNullMapping, object value) : this(parameterName, parameterType, parameterSize, sourceColumn, rowVersion)
		{
			this.Direction = direction;
			this.SourceColumnNullMapping = sourceColumnNullMapping;
			this.Value = value;
		}

		public SQLiteParameter(System.Data.DbType parameterType, int parameterSize) : this(null, parameterType, parameterSize, null, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(System.Data.DbType parameterType, int parameterSize, string sourceColumn) : this(null, parameterType, parameterSize, sourceColumn, DataRowVersion.Current)
		{
		}

		public SQLiteParameter(System.Data.DbType parameterType, int parameterSize, string sourceColumn, DataRowVersion rowVersion) : this(null, parameterType, parameterSize, sourceColumn, rowVersion)
		{
		}

		public object Clone()
		{
			return new SQLiteParameter(this);
		}

		public override void ResetDbType()
		{
			this._dbType = System.Data.DbType.Binary | System.Data.DbType.Byte | System.Data.DbType.Boolean | System.Data.DbType.Currency | System.Data.DbType.Date | System.Data.DbType.DateTime | System.Data.DbType.Decimal | System.Data.DbType.Double | System.Data.DbType.Guid | System.Data.DbType.Int16 | System.Data.DbType.Int32 | System.Data.DbType.Int64 | System.Data.DbType.Object | System.Data.DbType.SByte | System.Data.DbType.Single | System.Data.DbType.String | System.Data.DbType.Time | System.Data.DbType.UInt16 | System.Data.DbType.UInt32 | System.Data.DbType.UInt64 | System.Data.DbType.VarNumeric | System.Data.DbType.AnsiStringFixedLength | System.Data.DbType.StringFixedLength | System.Data.DbType.Xml | System.Data.DbType.DateTime2 | System.Data.DbType.DateTimeOffset;
		}
	}
}