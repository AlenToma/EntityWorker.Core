using System;
using EntityWorker.Core.Helper;

namespace EntityWorker.Core
{
    public class RoundingSettings
    {
        public int RowDecimalRoundTotal { get; set; }

        public RoundingConvention? RoundingConventionMethod { get; set; }

        public RoundingSettings()
        {
            RowDecimalRoundTotal = 5;// Default Value
        }

        public decimal Round(decimal value)
        {
            var isNegative = (RoundingConventionMethod != null && RoundingConventionMethod != RoundingConvention.Normal) && value < 0;
            if (RoundingConventionMethod != null && isNegative)
                value = Math.Abs(value); // its very importend to have a positive number when using floor/Ceiling

            switch (RoundingConventionMethod)
            {
                case null:
                case RoundingConvention.Normal:
                    value = Math.Round(value, this.RowDecimalRoundTotal);
                    break;
                case RoundingConvention.RoundUpp:
                {
                    var adjustment = (decimal)Math.Pow(10, RowDecimalRoundTotal);
                    value = (Math.Ceiling(value * adjustment) / adjustment);
                }
                    break;
                case RoundingConvention.RoundDown:
                {
                    var adjustment = (decimal)Math.Pow(10, RowDecimalRoundTotal);
                    value = (Math.Floor(value * adjustment) / adjustment);
                }
                    break;
                case RoundingConvention.None:
                {
                    var adjustment = (decimal)Math.Pow(10, RowDecimalRoundTotal);
                    value = (Math.Truncate(value * adjustment) / adjustment);
                }
                    break;
            }
            if (isNegative)
                value = -value;

            return value;
        }

        public double Round(double value)
        {
            var isNegative = (RoundingConventionMethod != null && RoundingConventionMethod != RoundingConvention.Normal) && value < 0;

            if (RoundingConventionMethod != null && isNegative)
                value = Math.Abs(value); // its very importend to have a positive number when using floor/Ceiling
            switch (RoundingConventionMethod)
            {
                case null:
                case RoundingConvention.Normal:
                    value = Math.Round(value, this.RowDecimalRoundTotal);
                    break;
                case RoundingConvention.RoundUpp:
                {
                    var adjustment = Math.Pow(10, RowDecimalRoundTotal);
                    value = (Math.Ceiling(value * adjustment) / adjustment);
                }
                    break;
                case RoundingConvention.RoundDown:
                {
                    var adjustment = Math.Pow(10, RowDecimalRoundTotal);
                    value = (Math.Floor(value * adjustment) / adjustment);
                }
                    break;
                case RoundingConvention.None:
                {
                    var adjustment = Math.Pow(10, RowDecimalRoundTotal);
                    value = (Math.Truncate(value * adjustment) / adjustment);
                }
                    break;
            }

            if (isNegative)
                value = -value;

            return value;
        }
    }
}
