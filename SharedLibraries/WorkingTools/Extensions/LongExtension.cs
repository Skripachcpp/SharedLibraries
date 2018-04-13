using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingTools.Extensions
{
    public static class NumberExtension
    {
        public static long Divide(this long numerator, long denominator)
        {
            if (numerator == 0 || denominator == 0) return 0;
            return numerator / denominator;
        }

        public static int Divide(this int numerator, int denominator)
        {
            if (numerator == 0 || denominator == 0) return 0;
            return numerator / denominator;
        }

        public static decimal Divide(this decimal numerator, decimal denominator)
        {
            if (numerator == 0 || denominator == 0) return 0;
            return numerator / denominator;
        }

        public static double Divide(this double numerator, double denominator)
        {
            if (numerator == 0 || denominator == 0) return 0;
            return numerator / denominator;
        }
    }
}
