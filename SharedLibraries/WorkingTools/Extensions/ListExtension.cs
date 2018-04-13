using System.Collections.Generic;

namespace WorkingTools.Extensions
{
    public static class ListExtension
    {
        public static T Gv<T>(this List<T> list, int index) => GetValue<T>(list, index);
        public static T GetValue<T>(this List<T> list, int index)
        {
            if (list == null) return default(T);
            if (index >= list.Count) return default(T);

            return list[index];
        }
    }
}