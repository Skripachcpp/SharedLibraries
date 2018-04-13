using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace WorkingTools.Extensions
{
    public static class DataTableExtension
    {
        public static IEnumerable<string> ToEnumerable(this DataRow row)
        {
            if (row == null) yield break;

            var columnsCount = row.ItemArray.Length;
            for (int i = 0; i < columnsCount; i++)
            {
                yield return row.Get(i);
            }
        }

        public static void Get(this DataTable table, ref string @out, string column, int row = 0) => Get<string>(table, ref @out, column, row);
        public static void Get<TOut>(this DataTable table, ref TOut @out, string column, int row = 0)
        {
            var value = Get<TOut>(table, column, row);
            if (object.Equals(value, null)) return;
            //стоит ли сюда дописать проверку на default (TOut)? вот в чем вопрос!

            @out = value;
        }

        public static string Get(this DataTable table, string column, int row = 0)
        {
            var value = Get<string>(table, column, row);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }

        public static TValue Get<TValue>(this DataTable table, string column, int row = 0)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(column)) throw new ArgumentNullException(nameof(column));
            if (row < 0) throw new ArgumentNullException(nameof(row));
            if (table.Rows.Count <= row) return default(TValue);
            //if (table.Rows.Count <= row) throw new ArgumentOutOfRangeException("row", @"ѕереданное значение больше общего числа строк");

            var tableRow = table.Rows[row];
            var value = Get<TValue>(tableRow, column);
            return value;
        }

        public static TValue Get<TValue>(this DataTable table, int column, int row = 0)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (column < 0) throw new ArgumentNullException(nameof(column));
            if (row < 0) throw new ArgumentNullException(nameof(row));
            if (table.Rows.Count <= row) return default(TValue);
            //if (table.Rows.Count <= row) throw new ArgumentOutOfRangeException("row", @"ѕереданное значение больше общего числа строк");

            var tableRow = table.Rows[row];
            var value = Get<TValue>(tableRow, column);
            return value;
        }
        public static string Gv(this DataRow row, int column) => Get(row, column);
        public static string Get(this DataRow row, int column) => Get<string>(row, column);
        public static string Get(this DataRow row, string column) => Get<string>(row, column);

        public static TValue Get<TValue>(this DataRow row, string column)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (string.IsNullOrWhiteSpace(column)) throw new ArgumentNullException(nameof(column));

            var dbvalue = row[column];
            var value = Convert<TValue>(dbvalue);
            return value;
        }

        public static TValue Get<TValue>(this DataRow row, int column)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (column < 0) throw new ArgumentNullException(nameof(column));
            if (row.Table.Columns.Count <= column) return default(TValue);

            var dbvalue = row[column];
            var value = Convert<TValue>(dbvalue);
            return value;
        }

        private static TValue Convert<TValue>(object value)
        {
            return (TValue)Convert(value, typeof(TValue));
        }

        private static bool IsGenericNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();
        }
        private static Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        private static object Convert(object value, Type to)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var valueType = value.GetType();
            if (valueType == null)
                throw new Exception("не удалось получить тип объекта");

            //если значение можно просто присвоить то возвращаем его же
            if (to.IsAssignableFrom(valueType))
                return value;

            //если nullabel тип то определяем что за nullabel
            var coreTo = IsGenericNullable(to) ? GetUnderlyingType(to) : to;

            // /* разбираем по типам
            if (coreTo.IsAssignableFrom(typeof(int?)))
                if (valueType.IsAssignableFrom(typeof(string)))
                {
                    var valueString = (string)value;
                    if (string.IsNullOrWhiteSpace(valueString)) return null;
                    return System.Convert.ToInt32(value);
                }

            if (coreTo.IsAssignableFrom(typeof(decimal)))
            {
                if (valueType.IsAssignableFrom(typeof (string)))
                    return decimal.Parse(((string)value), NumberFormatInfo.InvariantInfo);

                return System.Convert.ToDecimal(value);
            }

            if (coreTo.IsAssignableFrom(typeof(int)))
                return System.Convert.ToInt32(value);

            if (coreTo.IsAssignableFrom(typeof(string)))
                return value.ToString();

            if (coreTo.IsAssignableFrom(typeof(bool)))
                return System.Convert.ToBoolean(value);

            if (coreTo.IsAssignableFrom(typeof(DateTime)))
            {

                if (value is string)
                    return DateTime.Parse((string)value);

                return value;
            }

            if (coreTo.IsAssignableFrom(typeof(double)))
                return System.Convert.ToDouble(value);
            // */

            throw new Exception(string.Format("ое удалось конвертировать {0} в {1}. ƒанное преобразование не предусмотрено.", value.GetType().FullName, to.FullName));
        }
    }
}