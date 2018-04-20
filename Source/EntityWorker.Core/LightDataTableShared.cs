using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;

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
            Culture = cultureInfo ?? GlobalConfiguration.CultureInfo;
            ValidateCulture();
        }

        public LightDataTableShared()
        {
            Columns = new ColumnsCollections<string>();
            ColumnsWithIndexKey = new ColumnsCollections<int>();
            RoundingSettings = new RoundingSettings();
            Culture = GlobalConfiguration.CultureInfo;
            ValidateCulture();
        }


        internal static object ValueByType(Type propertyType, object defaultValue = null)
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

            if (propertyType.IsEnum)
                return Activator.CreateInstance(propertyType);
            

            if (propertyType == typeof(int?))
                return new int?();
            if (propertyType == typeof(int))
                return 0;

            if (propertyType == typeof(long?))
                return new long?();
            if (propertyType == typeof(long))
                return 0;

            if (propertyType == typeof(float?))
                return new long?();
            if (propertyType == typeof(float))
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

            if (propertyType == typeof(Guid?))
                return new Guid?();

            if (propertyType == typeof(Guid))
                return new Guid();

            if (propertyType == typeof(byte))
                return new byte();


            if (propertyType == typeof(byte?))
                return new byte?();

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

                if (dataType == typeof(byte[]) && value.GetType() == typeof(string))
                {
                    if (value.ToString().Length % 4 == 0) // its a valid base64string
                        value = Convert.FromBase64String(value.ToString());
                    return;
                }

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

                if (dataType == typeof(float?) || dataType == typeof(float))
                {
                    if (float.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = RoundingSettings.Round(douTal);
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
                    {
                        if (dateValue < SqlDateTime.MinValue)
                            dateValue = SqlDateTime.MinValue.Value;
                        value = dateValue;

                    }
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

                if (dataType.IsEnum || (dataType.GenericTypeArguments?.FirstOrDefault()?.IsEnum ?? false))
                {

                    var type = dataType;
                    if ((dataType.GenericTypeArguments?.FirstOrDefault()?.IsEnum ?? false))
                        type = (dataType.GenericTypeArguments?.FirstOrDefault());
                    if (value is int || value is long)
                    {
                        if (Enum.IsDefined(type, Convert.ToInt32(value)))
                            value = Enum.ToObject(type, Convert.ToInt32(value));
                    }
                    else if (Enum.IsDefined(type, value))
                        value = Enum.Parse(type, value.ToString(), true);
                    else if (loadDefaultOnError)
                        value = Activator.CreateInstance(dataType);
                }
                else if (dataType == typeof(Guid) || dataType == typeof(Guid?))
                {
                    if (Guid.TryParse(value.ToString(), out Guid v))
                        value = v;
                    else if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                }
                else if (dataType == typeof(string))
                {
                    value = value.ToString();

                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error: InvalidType. ColumnType is {dataType.FullName} and the given value is of type {value.GetType().FullName} Original Exception {ex.Message}");

            }
        }


        private object CleanValue(Type valueType, object value)
        {
            if ((valueType != typeof(decimal) && valueType != typeof(double)) && (valueType != typeof(decimal?) && valueType != typeof(float?) && valueType != typeof(float)) ) return value;
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
