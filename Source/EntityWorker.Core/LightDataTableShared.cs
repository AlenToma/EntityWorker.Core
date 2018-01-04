using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace EntityWorker.Core
{
    public class LightDataTableShared
    {
        internal CultureInfo Culture;
        public RoundingSettings RoundingSettings { get; set; }
        public ColumnsCollections<string> Columns { get; protected set; }
        public ColumnsCollections<int> ColumnsWithIndexKey { get; protected set; }
        public int ColumnLength { get; set; } = 0;

        public bool IgnoreTypeValidation;

        public LightDataTableShared(CultureInfo cultureInfo)
        {
            Columns = new ColumnsCollections<string>();
            ColumnsWithIndexKey = new ColumnsCollections<int>();
            RoundingSettings = new RoundingSettings();
            Culture = cultureInfo ?? new CultureInfo("en");
            ValidateCulture();
        }

        public LightDataTableShared()
        {
            Columns = new ColumnsCollections<string>();
            ColumnsWithIndexKey = new ColumnsCollections<int>();
            RoundingSettings = new RoundingSettings();
            Culture = new CultureInfo("en");
            ValidateCulture();
        }


        protected object ValueByType(Type propertyType, object defaultValue = null)
        {
            if (defaultValue != null)
            {
                var typeOne = propertyType;
                var typeTwo = defaultValue.GetType();
                if (Nullable.GetUnderlyingType(typeOne) != null)
                    typeOne = Nullable.GetUnderlyingType(typeOne);
                if (Nullable.GetUnderlyingType(typeTwo) != null)
                    typeTwo = Nullable.GetUnderlyingType(typeTwo);

                if (typeOne == typeTwo)
                    return defaultValue;
            }
            if (propertyType == typeof(int?))
                return new int?();
            if (propertyType == typeof(int))
                return 0;

            if (propertyType == typeof(long?))
                return new long?();
            if (propertyType == typeof(long))
                return 0;

            if (propertyType == typeof(decimal?))
                return new decimal?();

            if (propertyType == typeof(decimal))
                return new decimal();

            if (propertyType == typeof(double?))
                return new double?();

            if (propertyType == typeof(double))
                return new double();

            if (propertyType == typeof(DateTime?))
                return new DateTime?();

            if (propertyType == typeof(DateTime))
                return SqlDateTime.MinValue.Value;

            if (propertyType == typeof(bool?))
                return new bool?();

            if (propertyType == typeof(bool))
                return false;

            if (propertyType == typeof(TimeSpan?))
                return new TimeSpan?();

            if (propertyType == typeof(TimeSpan))
                return new TimeSpan();

            if (propertyType == typeof(byte[]))
                return new byte[0];

            return propertyType == typeof(string) ? string.Empty : null;
        }

        internal void ValidateCulture()
        {
            try
            {
                if (Culture != null && System.Threading.Thread.CurrentThread.CurrentCulture.Name != Culture.Name) // vi behöver sätta det första gången bara. detta snabbar upp applikationen ta inte bort detta.
                    System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
            }
            catch
            {
                //Ignore
            }
        }

        protected void TypeValidation(ref object value, Type dataType, bool loadDefaultOnError, object defaultValue = null)
        {
            try
            {
                ValidateCulture();
                if (value == null || value is DBNull)
                {
                    value = ValueByType(dataType, defaultValue);
                    return;
                }

                if (IgnoreTypeValidation)
                    return;

                if (dataType == typeof(int?) || dataType == typeof(int))
                {
                    if (double.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = Convert.ToInt32(douTal);
                    else
                        if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);

                    return;
                }

                if (dataType == typeof(long?) || dataType == typeof(long))
                {
                    if (double.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = Convert.ToInt64(douTal);
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);

                    return;
                }

                if (dataType == typeof(decimal?) || dataType == typeof(decimal))
                {
                    if (decimal.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var decTal))
                        value = RoundingSettings.Round(decTal);
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;

                }

                if (dataType == typeof(double?) || dataType == typeof(double))
                {
                    if (double.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = RoundingSettings.Round(douTal);
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;

                }

                if (dataType == typeof(DateTime?) || dataType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value.ToString(), Culture, DateTimeStyles.None, out var dateValue))
                        value = dateValue;
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;

                }

                if (dataType == typeof(bool?) || dataType == typeof(bool))
                {
                    if (bool.TryParse(value.ToString(), out var boolValue))
                        value = boolValue;
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;
                }

                if (dataType == typeof(TimeSpan?) || dataType == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(value.ToString(), Culture, out var timeSpanValue))
                        value = timeSpanValue;
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;


                }

                if (dataType.IsEnum)
                {
                    if (value is int || value is long)
                    {
                        if (Enum.IsDefined(dataType, Convert.ToInt32(value)))
                            value = Enum.ToObject(dataType, Convert.ToInt32(value));
                    }
                    else if (Enum.IsDefined(dataType, value))
                        value = Enum.Parse(dataType, value.ToString(), true);
                }
                else if (dataType == typeof(string))
                {
                    value = value.ToString();

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error: InvalidType. ColumnType is " + dataType.FullName + " and the given value is of type " + value.GetType().FullName + " Original Exception " + ex.Message);

            }
        }


        private object CleanValue(Type valueType, object value)
        {
            if ((valueType != typeof(decimal) && valueType != typeof(double)) && (valueType != typeof(decimal?) && valueType != typeof(double?))) return value;
            value = Culture.NumberFormat.NumberDecimalSeparator == "." ? value.ToString().Replace(",", ".") : value.ToString().Replace(".", ",");
            value = System.Text.RegularExpressions.Regex.Replace(value.ToString(), @"\s", "");
            return value;
        }

        public LightDataTableRow NewRow(CultureInfo cultureInfo = null)
        {
            var row = new LightDataTableRow(ColumnLength, Columns, ColumnsWithIndexKey, cultureInfo ?? Culture)
            {
                RoundingSettings = this.RoundingSettings
            };
            return row;
        }

        public LightDataTableRow NewRow(object[] items)
        {
            var row = new LightDataTableRow(ColumnLength, Columns, ColumnsWithIndexKey, Culture)
            {
                RoundingSettings = this.RoundingSettings,
                _itemArray = items
            };
            return row;
        }

    }
}
