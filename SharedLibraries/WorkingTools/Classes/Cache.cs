using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkingTools.Extensions;
using WorkingTools.Extensions.Json;

namespace WorkingTools.Classes
{
    public class Cache<TKey, TValue>
    {
        protected readonly SemaphoreSlim Lock = new SemaphoreSlim(1);

        protected readonly Dictionary<TKey, TValue> DictionaryKesh = new Dictionary<TKey, TValue>();

        protected TKey _key;
        protected TValue _value;
        public virtual TValue Get(TKey key, Func<TKey, TValue> getter)
        {
            if (TryCache(key, out TValue value))
                return value;

            Lock.Wait();
            try
            {
                value = getter(key);
                DictionaryKeshAddNoLock(key, value);
                return value;
            }
            finally { Lock.Release(); }
        }

        public virtual async Task<TValue> GetAsync(TKey key, Func<TKey, Task<TValue>> getter)
        {
            if (TryCache(key, out TValue value))
                return value;

            await Lock.WaitAsync();
            try
            {
                value = await getter(key);
                DictionaryKeshAddNoLock(key, value);
                return value;
            }
            finally { Lock.Release(); }
        }

        public virtual bool Contains(TKey key)
        {
            Lock.Wait();
            try
            {
                if (!object.Equals(key, default(TKey)) && object.Equals(key, _key)) return true;
                return DictionaryKesh.ContainsKey(key);
            }
            finally { Lock.Release(); }
        }

        public virtual void Set(TKey key, TValue value)
        {
            Lock.Wait();
            try { DictionaryKeshAddNoLock(key, value); }
            finally { Lock.Release(); }
        }

        protected virtual void DictionaryKeshAddNoLock(TKey key, TValue value)
        {
            _key = default(TKey); _value = default(TValue);
            DictionaryKesh.AddOrUpdate(key, value);
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

        public virtual void Clear()
        {
            Lock.Wait();
            try { ClearNoLock(); }
            finally { Lock.Release(); }
        }

        protected virtual void ClearNoLock()
        {
            DictionaryKesh.Clear();
            _key = default(TKey);
            _value = default(TValue);
        }

        protected virtual bool TryCache(TKey key, out TValue value)
        {
            TKey k;
            TValue v;

            Lock.Wait();
            try { k = _key; v = _value; }
            finally { Lock.Release(); }

            if (!object.Equals(key, default(TKey)) && object.Equals(key, k))
            {
                value = v;
                return true;
            }

            if (DictionaryKesh.TryGetValue(key, out v))
            {
                Lock.Wait();
                try { _key = key; _value = v; }
                finally { Lock.Release(); }

                value = v;
                return true;
            }

            value = default(TValue);
            return false;
        }
    }

    public class JsonKey
    {
        public static string Create(params object[] args)
        {
            var key = args.ToJson(hiddenNull: false, indented: false, useapostrophe: false);
            return key;
        }

        public JsonKey(params object[] args)
        {
            var key = Create(args);
            Key = key;

            Args = args;
        }

        public JsonKey ToLowerCase()
        {
            Key = Key.Lc();
            return this;
        }

        public string Key { get; protected set; }
        public object[] Args { get; protected set; }

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((JsonKey)obj));
        protected bool Equals(JsonKey other) => string.Equals(Key, other.Key);
        public override int GetHashCode() => (Key != null ? Key.GetHashCode() : 0);
        public static bool operator ==(JsonKey left, JsonKey right) => Equals(left, right);
        public static bool operator !=(JsonKey left, JsonKey right) => !Equals(left, right);
    }
}