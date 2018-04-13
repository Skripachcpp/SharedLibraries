using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingTools.Extensions
{
    public static class HashSetExtension
    {
        public static T F<T>(this HashSet<T> value) => value.FirstOrDefault();

        public static void Add<T>(this HashSet<T> value, IEnumerable<T> items)
        { if (value != null) foreach (var item in items.NotNull()) value.Add(item); }
    }
}
