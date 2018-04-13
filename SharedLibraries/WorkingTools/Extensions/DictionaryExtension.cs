using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkingTools.Extensions
{
    public static class DictionaryExtension
    {
        public static TVal Gv<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dict, TKey key, TVal defVal = default(TVal)) => GetValue(dict, key, defVal);
        public static TVal GetValue<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dict, TKey key, TVal defVal = default(TVal))
        {
            if (key == null) return default(TVal);
            if (dict == null) return default(TVal);
            if (!dict.ContainsKey(key)) return defVal;
            var val = dict[key];
            return val;
        }

        public static Dictionary<TKey, TVal> Upd<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val) => Update(dict, key, val);
        public static Dictionary<TKey, TVal> Update<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            if (dict == null) dict = new Dictionary<TKey, TVal>();

            if (dict.ContainsKey(key)) dict[key] = val;
            else dict.Add(key, val);

            return dict;
        }

        public static Dictionary<TKey, TVal> UpdateAndCopy<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val, TKey[] keysForInsertIfHimValueNotExists)
        {
            if (dict == null) dict = new Dictionary<TKey, TVal>();

            if (dict.ContainsKey(key)) dict[key] = val;
            else dict.Add(key, val);

            foreach (var k in keysForInsertIfHimValueNotExists.NotNull())
                if (!dict.ContainsKey(k))
                    dict.Add(k, val);

            return dict;
        }

        public static Dictionary<TKey, HashSet<TVal>> UpdateAndCopy<TKey, TVal>(this Dictionary<TKey, HashSet<TVal>> dict, TKey key, TVal val, TKey[] keysForInsertIfHimValueNotExists)
        {
            if (dict == null) dict = new Dictionary<TKey, HashSet<TVal>>();

            if (dict.ContainsKey(key)) dict[key].Add(val);
            else dict.Add(key, new HashSet<TVal>() { val });

            foreach (var k in keysForInsertIfHimValueNotExists.NotNull())
                if (!dict.ContainsKey(k))
                    dict.Add(k, new HashSet<TVal>() { val });

            return dict;
        }

        public static bool Cross<TKey, TVal>(this Dictionary<TKey, TVal> a, Dictionary<TKey, TVal> b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            return a.Keys.Any(key => b.ContainsKey(key) && object.Equals(a[key], b[key]));
        }

        public static bool Cross<TKey, TVal>(this Dictionary<TKey, TVal> a, TKey key, TVal value)
        {
            if (a == null) return false;
            return a.Keys.Any(k => object.Equals(key, k) && object.Equals(a[k], value));
        }

        public static bool AddIfNotExists<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            if (dict == null) return false;
            if (dict.ContainsKey(key)) return false;

            dict.Add(key, val);

            return true;
        }

        public static bool AddOrUpdate<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            if (dict == null) return false;
            if (dict.ContainsKey(key)) dict[key] = val;
            else dict.Add(key, val);

            return true;
        }

        public static bool TryAdd<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            if (dict == null) return false;
            if (dict.ContainsKey(key)) return false;
            
            dict.Add(key, val);

            return true;
        }
    }
}