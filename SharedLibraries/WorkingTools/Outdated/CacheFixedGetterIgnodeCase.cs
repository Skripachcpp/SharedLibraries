using System;
using System.Collections.Generic;
using System.Linq;
using WorkingTools.Extensions;

namespace WorkingTools.Classes
{
    public class CacheFixedGetterIgnodeCase<TValue> : CacheFixedGetter<string, TValue>
    {
        public CacheFixedGetterIgnodeCase(Func<string, TValue> get) : base(get) { }

        public override TValue Get(string key)
        {
            var lckey = key?.ToLower();
            if (lckey == null) return default(TValue);
            if (object.Equals(lckey, _key)) return _value;

            TValue v;
            if (DictionaryKesh.TryGetValue(lckey, out v))
            {
                _key = lckey; _value = v;
                return v;
            }

            lock (Lock)
            {
                if (DictionaryKesh.TryGetValue(lckey, out v))
                {
                    _key = lckey; _value = v;
                    return v;
                }

                var value = GetValueByKey(key);
                DictionaryKeshAddNoLock(lckey, value);

                _key = key; _value = value;
                return value;
            }
        }
        public override bool Contains(string key) => base.Contains(key?.ToLower());
        public override void Set(string key, TValue value) => base.Set(key?.ToLower(), value);
        public override void Set(IEnumerable<KeyValuePair<string, TValue>> items) => base.Set(items.Select(a => new KeyValuePair<string, TValue>(a.Key?.ToLower(), a.Value)));
    }
}