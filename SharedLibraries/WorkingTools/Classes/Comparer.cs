using System;
using System.Collections.Generic;

namespace WorkingTools.Classes
{
    public class ComparerWt<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public ComparerWt(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            if (getHashCode == null) throw new ArgumentNullException(nameof(getHashCode));

            _equals = equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            else if (x == null || y == null) return false;       

            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (obj == null) return 0;

            return _getHashCode(obj);
        }
    }
}