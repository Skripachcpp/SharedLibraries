using System;
using System.Collections.Generic;
using System.Linq;

namespace SL.Elasticsearch.Extensions
{
    public static class DictMl
    {
        public static string[] Lgs { get; set; }

        public static Dictionary<string, string> Uac(string key, string val)
        {
            return Uac(new Dictionary<string, string>(), key, val);
        }

        public static Dictionary<string, string> Uac(this Dictionary<string, string> dict, string key, string val)
        {
            if (Lgs == null) throw new Exception("не инициализирован набор языков для локализиции значения");
            if (dict == null) dict = new Dictionary<string, string>();

            dict.UpdateAndCopy(key, val, Lgs);
            return dict;
        }

        public static Dictionary<string, HashSet<string>> Uac(this Dictionary<string, HashSet<string>> dict, string key, HashSet<string> val)
        {
            if (Lgs == null) throw new Exception("не инициализирован набор языков для локализиции значения");
            if (dict == null) dict = new Dictionary<string, HashSet<string>>();

            dict.UpdateAndCopy(key, val, Lgs);
            return dict;
        }
        

        internal static Dictionary<TKey, TVal> UpdateAndCopy<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key, TVal val, TKey[] keysForInsertIfHimValueNotExists)
        {
            if (dict == null) dict = new Dictionary<TKey, TVal>();

            if (dict.ContainsKey(key)) dict[key] = val;
            else dict.Add(key, val);

            if (keysForInsertIfHimValueNotExists != null)
            foreach (var k in keysForInsertIfHimValueNotExists.Where(a => a != null))
                if (!dict.ContainsKey(k))
                    dict.Add(k, val);

            return dict;
        }

        internal static Dictionary<TKey, HashSet<TVal>> UpddateAndCopy<TKey, TVal>(this Dictionary<TKey, HashSet<TVal>> dict, TKey key, TVal val, TKey[] keysForInsertIfHimValueNotExists)
        {
            if (dict == null) dict = new Dictionary<TKey, HashSet<TVal>>();

            if (dict.ContainsKey(key)) dict[key].Add(val);
            else dict.Add(key, new HashSet<TVal>() { val });

            if (keysForInsertIfHimValueNotExists != null)
            foreach (var k in keysForInsertIfHimValueNotExists.Where(a => a != null))
                if (!dict.ContainsKey(k))
                    dict.Add(k, new HashSet<TVal>() { val });

            return dict;
        }
    }
}