using System;

namespace WorkingTools.Extensions
{
    public static class guid
    {
        public static string NewString() => Guid.NewGuid().ToString("N");
        public static Guid NewGuid() => Guid.NewGuid();
    }
}