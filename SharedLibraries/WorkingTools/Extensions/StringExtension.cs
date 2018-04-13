using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WorkingTools.Extensions
{
    public static class StringExtension
    {
        public static bool EqualsIgnoreCase(this string a, string b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        public static bool IsNotNullAndWhiteSpace(this string v) => !IsNullOrWhiteSpace(v);
        public static bool IsNullOrWhiteSpace(this string v) => string.IsNullOrWhiteSpace(v);
        public static bool IsNotNullAndEmpty(this string v) => !IsNullOrEmpty(v);
        public static bool IsNullOrEmpty(this string v) => string.IsNullOrEmpty(v);

        public static string UpFrLet(this string v) => ToUpperFirstLetter(v);
        public static string ToUpperFirstLetter(this string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return v;

            if (v.Length <= 1)
                return v.ToUpperInvariant();

            var word = char.ToUpperInvariant(v[0]) + v.Substring(1);
            return word;
        }

        public static string ToLowerFirstLetter(this string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return v;

            if (v.Length <= 1)
                return v.ToLowerInvariant();

            var word = char.ToLowerInvariant(v[0]) + v.Substring(1);
            return word;
        }

        public static string Short(this string value, int maxlenght, string addshortending)
        {
            if (maxlenght < 0) throw new ArgumentOutOfRangeException(nameof(maxlenght), "значение не может быть отрицательным");

            if (value == null) return null;
            if (maxlenght == 0) return "";
            if (value.Length <= maxlenght) return value;

            string newvalue;
            var addshortendingLenght = addshortending?.Length;

            if (addshortending != null && maxlenght > addshortendingLenght)
                newvalue = value.Substring(0, maxlenght - addshortendingLenght.Value) + addshortending;
            else
                newvalue = value.Substring(0, maxlenght);

            return newvalue;
        }


        public static string ToOneLine(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value.Replace(Environment.NewLine, " ").Replace("\n", " ");
        }

        public static string Short(this string value, int maxlenght, bool addshortending = true)
        { return Short(value, maxlenght, addshortending ? "..." : null); }


        //public static bool AsBool(this string value, bool defValue) => AsBool(value) ?? defValue;

        public static bool? AsBool(this string value)
        {
            bool boolResult;
            if (bool.TryParse(value, out boolResult))
                return boolResult;

            int intResult;
            if (int.TryParse(value, out intResult))
                return Convert.ToBoolean(intResult);

            if (value != null)
            {
                value = value.ToLower();
                if (value == "да")
                    return true;
                else if (value == "нет")
                    return false;
            }

            if (value == null) return null;

            if (!string.IsNullOrWhiteSpace(value)) return true;
            else return false;
        }

        /// <summary>
        /// Привести строку к Guid?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid? AsGuid(this string value)
        {
            if (value == null)
                return null;

            try
            {
                return new Guid(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Привести строку к Guid?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string value)
        {
            if (value == null)
                return Guid.Empty;

            try
            {
                return new Guid(value);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Привести строку к int?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            int res;
            if (!Int32.TryParse(value, out res))
                throw new ArgumentOutOfRangeException(nameof(value), "не удалось привести строку к числу");

            return res;
        }

        /// <summary>
        /// Привести строку к int?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? AsInt(this string value)
        {
            if (value == null)
                return null;

            if (!int.TryParse(value, out int res))
                return null;

            return res;
        }

        private static readonly Regex RegexIntParse = new Regex(@"\d+", RegexOptions.Compiled);
        public static int? IntParse(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var sb = new StringBuilder(value);
            var v = sb.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "").ToString();
            v = RegexIntParse.Match(v).Value;

            return v.AsInt();
        }

        /// <summary>
        /// Привести строку к long?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long? AsLong(this string value)
        {
            if (value == null)
                return null;

            if (!long.TryParse(value, out long res))
                return null;

            return res;
        }

        /// <summary>
        /// Привести строку к long
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ToLong(this string value)
        {
            return long.Parse(value);
        }

        /// <summary>
        /// Привести строку к int, возвращает defaultValue если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int AsInt(this string value, int defaultValue)
        {
            if (value == null)
                return defaultValue;

            if (!int.TryParse(value, out int res))
                return defaultValue;

            return res;
        }

        /// <summary>
        /// Привести строку к decimal?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return default(decimal);

            try
            { return decimal.Parse(value, NumberFormatInfo.InvariantInfo); }
            catch (Exception ex)
            { throw new Exception(string.Format("не удалось преобразовать значение {0} к {1}", value, typeof(decimal)), ex); }
        }

        public static decimal AsDecimal(this string value, decimal defVal, bool adaptiveSeparator = true) => value.AsDecimal(adaptiveSeparator) ?? defVal;

        /// <summary>
        /// Привести строку к decimal?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <param name="adaptiveSeparator">автоматически определять точку и запятую</param>
        /// <returns></returns>
        public static decimal? AsDecimal(this string value, bool adaptiveSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            try
            {
                if (adaptiveSeparator) value = value.Replace(',', '.');

                return decimal.Parse(value, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception ex)
            { throw new Exception(string.Format("не удалось преобразовать значение {0} к {1}", value, typeof(decimal)), ex); }
        }

        public static float ToFloat(this string value, bool adaptiveSeparator = true)
        {
            if (adaptiveSeparator) value = value.Replace(',', '.').Replace(" ", "");
            return float.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        public static double ToDouble(this string value, bool adaptiveSeparator = true)
        {
            if (adaptiveSeparator) value = value.Replace(',', '.').Replace(" ", "");
            return double.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        private static readonly Regex RegexDoubleOnly = new Regex(@"[0-9]+\.?([0-9]+)?", RegexOptions.Compiled);
        /// <summary>
        /// Привести строку к double?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <param name="adaptiveSeparator">автоматически определять точку и запятую</param>
        /// <param name="throwException">true если нужно бросать исключения при не возможности преобразовать значение</param>
        /// <param name="parseValue">true если нужно пройтись regex и взять из строки только числа</param>
        /// <returns></returns>
        public static double? AsDouble(this string value, bool throwException = false, bool adaptiveSeparator = true, bool parseValue = true)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            try
            {
                if (adaptiveSeparator) value = value.Replace(',', '.').Replace(" ", "");
                if (parseValue) value = RegexDoubleOnly.Match(value).Value;
                if (string.IsNullOrWhiteSpace(value)) return null;

                return double.Parse(value, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception ex)
            {
                if (throwException) throw new Exception(string.Format("не удалось преобразовать значение {0} к {1}", value, typeof (decimal)), ex);
                else return null;
            }
        }

        /// <summary>
        /// Привести строку к DateTime?, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? AsDateTime(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            try { return Convert.ToDateTime(value); }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Привести строку к XDocument, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XDocument AsXDocument(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            try { return XDocument.Parse(value); }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Привести строку к XElement, возвращает null если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XElement AsXElement(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            try { return XElement.Parse(value); }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Кодирует строку как массив байтов, возврящает null выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding">по умолчанию UTF8</param>
        /// <returns></returns>
        public static byte[] AsBytes(this string value, Encoding encoding = null)
        {
            if (value == null)
                return null;

            try { return (encoding ?? Encoding.UTF8).GetBytes(value); }
            catch (Exception) { return null; }
        }

        public static string Lc(this string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return v;
            return v.ToLower();
        }

        #region Comparer

        private class EqualityStringComparer : IEqualityComparer<string>
        {
            private readonly bool _ignoreCase;

            public EqualityStringComparer(bool ignoreCase)
            { _ignoreCase = ignoreCase; }

            public bool Equals(string x, string y)
            { return string.Compare(x, y, _ignoreCase) == 0; }

            public int GetHashCode(string s)
            { return s.ToLower().GetHashCode(); }
        }

        #endregion

        public static IEnumerable<string> Distinct(this IEnumerable<string> enumerable, bool ignoreCase)
        { return enumerable.Distinct(new EqualityStringComparer(ignoreCase)); }

        /// <summary>
        /// Возвращает значение, указывающее, содержит ли указанная строка значение подстроки переданной в качестве параметра (игнорирует регистр и региональные параметры)
        /// </summary>
        /// <param name="v"></param>
        /// <param name="value"></param>
        /// <returns>true, если параметр value встречается в строке или value является пустой строкой (""); в противном случае — false.</returns>
        public static bool ContainsIgnrCase(this string v, string value)
        {
            if (v == null) return false;
            if (string.IsNullOrEmpty(v)) return false;
            if (string.IsNullOrEmpty(value)) return true;

            if (v.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) != -1) return true;
            return false;
        }

        public static IEnumerable<string> NotNullAndWhiteSpace(this IEnumerable<string> items)
        { return items.NotNull().Where(a => !string.IsNullOrWhiteSpace(a)); }
    }
}
