using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WorkingTools.Extensions
{
    public static class str
    {
        public static string Join<T>(string separator, params T[] values) => Join(separator, (IEnumerable<string>)values?.Select(a => a?.ToString()));
        public static string Join(string separator, params object[] values) => Join(separator, (IEnumerable<string>)values?.Select(a => a?.ToString()));
        public static string Join(string separator, params string[] values) => Join(separator, (IEnumerable<string>)values);

        /// <summary>
        /// Соединяет строки через разделитель пропуская пустые элементы
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Join(string separator, IEnumerable<string> values)
        {
            if (values == null) return null;

            var vs = values.Where(a => !string.IsNullOrEmpty(a));
            return string.Join(separator, vs);
        }

        
        public static string JoinTitle(object title, string value, string ending = null, string seporate = null) => JoinTitle(title?.ToString(), value, ending, seporate);

        /// <summary>
        /// Соединяет начало, текст и конец; возвращает null в случае если текст отсутствует
        /// </summary>
        /// <param name="title">начало строки</param>
        /// <param name="value">текст</param>
        /// <param name="ending">конец строки</param>
        /// <param name="seporate">разделитель</param>
        /// <returns></returns>
        public static string JoinTitle(string title, string value, string ending = null, string seporate = null)
        {
            if (string.IsNullOrEmpty(value)) return null;

            if (string.IsNullOrEmpty(title))
            {
                title = null;
                seporate = null;
            }

            return string.Concat(title, seporate, value, ending);
        }

        /// <summary>
        /// Возвращает первую не пустую строку
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string FirstNotEmpty(params string[] values) 
            => values.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a));

        /// <summary>
        /// Возвращает форматированную строку или null, в случае если значения отсутствуют
        /// </summary>
        /// <param name="format">формат</param>
        /// <param name="values">значения</param>
        /// <returns></returns>
        public static string Format(string format, params object[] values)
        {
            if (values == null) return null;
            if (values.Length <= 0) return null;
            if (values.All(a => a == null)) return null;

            return string.Format(format, values);
        }


        
        public static string GetCorrect(int number, string ending1, string ending2, string ending5) => GetCorrect((long)number, ending1, ending2, ending5);
        public static string GetCorrect(int? number, string ending1, string ending2, string ending5) => GetCorrect((long)(number ?? 0), ending1, ending2, ending5);
        public static string GetCorrect(long? number, string ending1, string ending2, string ending5) => GetCorrect(number ?? 0, ending1, ending2, ending5);

        /// <summary>
        /// Возвращает правильное существительного в зависимости от числа 
        /// </summary>
        /// <param name="number">число перед существительнм</param>
        /// <param name="ending1">например "яблоко"</param>
        /// <param name="ending2">например "яблока"</param>
        /// <param name="ending5">например "яблок"</param>
        /// <returns>вернет 2 "яблока", 1 "яблоко", 5 "яблок"</returns>
        public static string GetCorrect(long number, string ending1, string ending2, string ending5)
        {
            if (ending1 == null) throw new ArgumentNullException(nameof(ending1));
            if (ending2 == null) throw new ArgumentNullException(nameof(ending2));
            if (ending5 == null) throw new ArgumentNullException(nameof(ending5));

            if (number == 0)
                return ending5;

            number = number % 100;
            if (number >= 11 && number <= 19)
                return ending5;

            var i = number % 10;
            switch (i)
            {
                case 1:
                    return ending1;
                case 2:
                case 3:
                case 4:
                    return ending2;
                default:
                    return ending5;
            }
        }


        /// <summary>
        /// Сравнивает две строки без учета регистра
        /// </summary>
        /// <param name="a">строка 1</param>
        /// <param name="b">строка 2</param>
        /// <returns></returns>
        public static bool Equals(string a, string b) 
            => string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
    }
}
