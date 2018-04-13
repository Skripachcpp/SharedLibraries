using System;

namespace WorkingTools.Extensions.Parts
{
    public enum Separator { Point, Virgule }

    public static class SeparatorExtension
    {
        public static string ToCode(this Separator value)
        {
            switch (value)
            {
                case Separator.Point: return ".";
                case Separator.Virgule: return ",";
                default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}