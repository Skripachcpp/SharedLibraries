using System.Collections.Generic;
using System.Linq;

namespace WorkingTools.Extensions
{
    public static class enm
    {
        public static IEnumerable<T> Union<T>(params IEnumerable<T>[] vs)
        {
            if (vs == null) return null;
            return vs.Where(v => v != null).SelectMany(v => v);
        }
    }
}