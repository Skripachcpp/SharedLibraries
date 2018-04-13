using System;
using System.Collections.Generic;
using System.Threading;

namespace WorkingTools.Classes
{

    public class CacheFixedGetter<TKey, TValue> : CacheFixedGetterLite<TKey, TValue>
    {
        public CacheFixedGetter(Func<TKey, TValue> get) : base(get) { }

        public virtual bool Contains(TKey key)
        { lock (Lock) return DictionaryKesh.ContainsKey(key); }

        public virtual void Set(TKey key, TValue value)
        {
            lock (Lock)
            {
                _key = default(TKey); _value = default(TValue);

                if (DictionaryKesh.ContainsKey(key))
                    DictionaryKesh[key] = value;
                else
                    DictionaryKesh.Add(key, value);
            }
        }

        public virtual void Set(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (Lock)
            {
                _key = default(TKey); _value = default(TValue);

                if (items != null)
                    foreach (var item in items)
                        DictionaryKesh.Add(item.Key, item.Value);
            }
        }

        public virtual void Clear() { lock (Lock) DictionaryKesh.Clear(); }

        public virtual ICollection<KeyValuePair<TKey, TValue>> Items => DictionaryKesh;
    }


    public class CacheFixedGetterLite<TKey, TValue>
    {
        protected readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        protected readonly Dictionary<TKey, TValue> DictionaryKesh = new Dictionary<TKey, TValue>();
        protected Func<TKey, TValue> GetValueByKey;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="get">функция получения значения по ключу</param>
        public CacheFixedGetterLite(Func<TKey, TValue> get)
        {
            GetValueByKey = get;
        }

        protected TKey _key;
        protected TValue _value;
        public virtual TValue Get(TKey key)
        {
            TKey k;
            TValue v;

            try
            {
                Lock.EnterReadLock();

                k = _key;
                v = _value;
            }
            finally { Lock.ExitReadLock(); }

            if (!object.Equals(key, default(TKey)) && object.Equals(key, k)) return v;

            if (DictionaryKesh.TryGetValue(key, out v))
            {
                try { Lock.EnterWriteLock(); _key = key; _value = v; }
                finally { Lock.ExitWriteLock(); }

                return v;
            }

            try
            {
                Lock.EnterWriteLock();
                return DictionaryKeshAddNoLock(key);
            }
            finally { Lock.ExitWriteLock(); }
        }

        protected virtual TValue DictionaryKeshAddNoLock(TKey key)
        {
            var value = GetValueByKey(key);
            DictionaryKeshAddNoLock(key, value);
            return value;
        }

        protected virtual void DictionaryKeshAddNoLock(TKey key, TValue value)
        {
            _key = default(TKey); _value = default(TValue);
            DictionaryKesh.Add(key, value);
        }


        protected virtual void RemoveNoLock(TKey key)
        {
            DictionaryKesh.Remove(key);

            if (object.Equals(key, _key))
            {
                _key = default(TKey);
                _value = default(TValue);
            }
        }
    }
}
