using System;
using System.Globalization;
using WorkingTools.Extensions.Parts;

namespace WorkingTools.Extensions
{
    public static class DoubleExtension
    {
        public static double Round(this double value, int decimals = 0)
        { return Math.Round(value, decimals); }

        public static double? Round(this double? value, int decimals = 0)
        {
            if (value == null) return null;
            return Math.Round(value.Value, decimals);
        }

        public static string ToString(this double value, Separator separator)
        { return AsString(value, separator.ToCode()); }

        public static string AsString(this double? value, Separator separator)
        { return AsString(value, separator.ToCode()); }

        private static readonly NumberFormatInfo _numberFormatInfo = new NumberFormatInfo();
        public static string AsString(this double? value, string separator)
        {
            if (value == null) return null;
            if (separator == null) return value.Value.ToString(CultureInfo.InvariantCulture);

            lock (_numberFormatInfo)
            {
                _numberFormatInfo.NumberDecimalSeparator = separator;
                return value.Value.ToString(null, _numberFormatInfo);
            }
        }
    }

    public static class DecimalExtension
    {
        //public static decimal Rnd(this decimal value, int decimals = 0) => value.Round(decimals);
        public static decimal Round(this decimal value, int decimals = 0)
        { return decimal.Round(value, decimals); }


        public static decimal? Rnd(this decimal? value, int decimals = 0) => value.Round(decimals);

        public static decimal? Round(this decimal? value, int decimals = 0)
        {
            if (value == null) return null;
            return decimal.Round(value.Value, decimals);
        }

        public static string ToString(this decimal value, Separator separator)
        { return AsString(value, separator.ToCode()); }

        public static string AsString(this decimal? value, Separator separator)
        { return AsString(value, separator.ToCode()); }

        private static readonly NumberFormatInfo _numberFormatInfo = new NumberFormatInfo();
        public static string AsString(this decimal? value, string separator)
        {
            if (value == null) return null;
            if (separator == null) return value.Value.ToString(CultureInfo.InvariantCulture);

            lock (_numberFormatInfo)
            {
                _numberFormatInfo.NumberDecimalSeparator = separator;
                return value.Value.ToString(null, _numberFormatInfo);
            }
        }
    }
}