using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace WorkingTools.Extensions
{
    public static class Sugar
    {
        public static IEnumerable<TObj> Distinct<TObj>(this IEnumerable<TObj> objs, params Func<TObj, string>[] fields)
        {
            var keys = new HashSet<string>();
            foreach (var obj in objs)
            {
                var key = string.Join(";", fields.Select(a => a(obj)));
                if (keys.Contains(key)) continue;
                keys.Add(key);

                yield return obj;
            }

            //keys.Clear();
        }

        public static bool WaitOne(this EventWaitHandle evn, CancellationToken token, int intervalCheck = 5 * 1000)
        {
            while (!evn.WaitOne(intervalCheck))
                if (token.IsCancellationRequested)
                    return false;

            return true;
        }

        public static T F<T>(this IEnumerable<T> value, Func<T, bool> where = null) => where == null ? value.FirstOrDefault() : value.FirstOrDefault(where);

        public static IEnumerable<TObj> NotNull<TObj>(this IEnumerable<TObj> items, Func<TObj, bool> where)
        {
            if (where == null) throw new ArgumentNullException(nameof(where));
            return items.NotNull().Where(where);
        }

        public static IEnumerable<TObj> NotNull<TObj>(this IEnumerable<TObj> items)
        { return items?.Where(item => item != null) ?? Enumerable.Empty<TObj>(); }

        public static bool IsNotNullOrEmpty<TObj>(this List<TObj> items)
        { return items != null && items.Count > 0; }

        public static bool IsNotNullOrEmpty<TKey, TVal>(this Dictionary<TKey, TVal> items)
        { return items != null && items.Count > 0; }

        public static bool IsNullOrEmpty<TKey, TVal>(this Dictionary<TKey, TVal> items)
        { return items == null || items.Count <= 0; }

        public static bool IsNotNullOrEmpty<TObj>(this TObj[] items)
        { return items != null && items.Length > 0; }

        public static bool IsNullOrEmpty<T>(this List<T> array)
        {
            if (array == null) return true;
            if (array.Count <= 0) return true;

            return false;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            if (array == null) return true;
            if (array.Length <= 0) return true;

            return false;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func, Action<T, Exception> @catch)
        {
            if (func == null || enumerable == null) return;
            foreach (T item in enumerable)
                try
                {
                    func(item);
                }
                catch (Exception ex)
                {
                    if (@catch == null) throw;
                    @catch(item, ex);
                }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
        {
            if (func == null || enumerable == null) return;
            foreach (T item in enumerable)
                func(item);
        }

        public static T[] Union<T>(this T[] array, params T[] items)
        {
            if (array == null) return items;
            if (items == null || items.Length <= 0) return array;

            var newArray = array;
            Array.Resize(ref newArray, array.Length + items.Length);

            int insertPosition = array.Length;
            int insertionElement = 0;
            while (insertPosition < newArray.Length)
            {
                newArray[insertPosition] = items[insertionElement];

                insertPosition++;
                insertionElement++;
            }

            return newArray;
        }

        /// <summary>
        /// Возвращает строку в указаном формате или null, если выполнить преобразование не удалось
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding">по умолчанию UTF8</param>
        /// <returns></returns>
        public static string AsString(this byte[] value, Encoding encoding)
        {
            if (value == null)
                return null;

            try
            {
                return (encoding ?? Encoding.UTF8).GetString(value);
            }
            catch (Exception) { return null; }
        }

        public static string AsStr(this object obj) => AsString(obj);

        public static string AsString(this object obj)
        {
            if (obj is byte[]) return ((byte[])obj).AsString(null);

            return obj?.ToString();
        }

        public static TObj ThrowIfNull<TObj>(this TObj obj, string paramName, string errorMessage) where TObj : class
        {
            if (obj == null) throw new ArgumentNullException(paramName, errorMessage);
            return obj;
        }

        public static TObj ThrowIfNull<TObj>(this TObj? obj, string errorMessage) where TObj : struct
        {
            if (obj == null) throw new NullReferenceException(errorMessage);
            return obj.Value;
        }


        public static TObj ThrowIfNull<TObj>(this TObj obj, string errorMessage) where TObj : class
        {
            if (obj == null) throw new NullReferenceException(errorMessage);
            return obj;
        }

        public static TRes As<TRes>(this object obj) where TRes : class
        { return obj as TRes; }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> vs, IEqualityComparer<T> comparer = null)
        {
            if (comparer != null) return vs == null ? null : new HashSet<T>(vs, comparer);
            return vs == null ? null : new HashSet<T>(vs);
        }

        public static HashSet<TR> ToHashSet<TS, TR>(this IEnumerable<TS> vs, Func<TS, TR> f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));

            if (vs == null) return null;

            var vr = vs.Select(f).Distinct();
            return new HashSet<TR>(vr);
        }

        public static DataTable ToDataTable<T>(this ICollection<T> iList)
        {
            //TODO: не самый быстрый способ понвертировать перечисление в таблицу
            var dataTable = new DataTable();
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));

            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                var propertyDescriptor = propertyDescriptorCollection[i];
                var type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                if (type == null) throw new Exception("не удалось получить тип");

                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }

            var values = new object[propertyDescriptorCollection.Count];

            foreach (var iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}