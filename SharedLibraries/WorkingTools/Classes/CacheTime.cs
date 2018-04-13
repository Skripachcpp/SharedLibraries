using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkingTools.Parallel;

namespace WorkingTools.Classes
{
    public class CacheTime<TKey, TValue> : Cache<TKey, TValue>, IDisposable
    {
        protected Loop Loop;

        protected readonly TimeSpan MaxTimeSpan;
        protected readonly Dictionary<TKey, DateTime> DictionaryAddKeyTime = new Dictionary<TKey, DateTime>();

        public CacheTime() : this(10) { }

        public CacheTime(int min)
        {
            MaxTimeSpan = new TimeSpan(0, 0, min, 0);

            Loop = new Loop(min * 60, () =>
            {
                var now = DateTime.Now;

                TKey[] removes;
                Lock.Wait();
                try
                {
                    removes = (from keyTime in DictionaryAddKeyTime
                               where now - keyTime.Value > MaxTimeSpan
                               select keyTime.Key).ToArray();
                }
                finally { Lock.Release(); }

                Lock.Wait();
                try { foreach (var key in removes) RemoveNoLock(key); }
                finally { Lock.Release(); }
            });
        }

        private bool _loopStarted = false;

        protected virtual bool Gc(TKey key, DateTime now)
        {
            if (!_loopStarted)
            {
                Loop.Start(Loop.Interval);
                _loopStarted = true;
            }

            bool removed = false;
            Lock.Wait();
            try
            {
                if (DictionaryAddKeyTime.ContainsKey(key))
                {
                    var addKeyTime = DictionaryAddKeyTime[key];

                    var timeSpan = now - addKeyTime;

                    if (timeSpan > MaxTimeSpan) RemoveNoLock(key);
                    else removed = true;
                }
            }
            finally { Lock.Release(); }

            return removed;
        }

        protected virtual void Track(TKey key, DateTime now)
        {
            Lock.Wait();
            try
            {
                if (!DictionaryAddKeyTime.ContainsKey(key))
                    DictionaryAddKeyTime.Add(key, now);
            }
            finally { Lock.Release(); }
        }

        public TValue Get(TKey key, Func<TValue> getter) => Get(key, (k) => getter());
        public override TValue Get(TKey key, Func<TKey, TValue> getter)
        {
            var now = DateTime.UtcNow;
            var removed = Gc(key, now);
            var val = base.Get(key, getter);
            if (removed) Track(key, now);
            return val;
        }

        public async Task<TValue> GetAsync(TKey key, Func<Task<TValue>> getter) => await GetAsync(key, (k) => getter());
        public override async Task<TValue> GetAsync(TKey key, Func<TKey, Task<TValue>> getter)
        {
            var now = DateTime.UtcNow;
            var removed = Gc(key, now);
            var val = await base.GetAsync(key, getter);
            if (removed) Track(key, now);
            return val;
        }

        protected override void RemoveNoLock(TKey key)
        {
            DictionaryAddKeyTime.Remove(key);
            base.RemoveNoLock(key);
        }

        public override void Clear()
        {
            Lock.Wait();
            try
            {
                base.ClearNoLock();

                DictionaryAddKeyTime.Clear();
            }
            finally { Lock.Release(); }
        }

        public void Dispose()
        {
            Loop.Dispose(false);
        }
    }


    public class CacheTime<TValue> : CacheTime<JsonKey, TValue>
    {
        public CacheTime() { }
        public CacheTime(int min) : base(min) { }

        public TValue Get(Func<TValue> getter, params object[] key) => base.Get(new JsonKey(key).ToLowerCase(), getter);
        public TValue Get(Func<JsonKey, TValue> getter, params object[] key) => base.Get(new JsonKey(key).ToLowerCase(), getter);

        public async Task<TValue> GetAsync(Func<Task<TValue>> getter, params object[] key) => await base.GetAsync(new JsonKey(key).ToLowerCase(), getter);
        public async Task<TValue> GetAsync(Func<JsonKey, Task<TValue>> getter, params object[] key) => await base.GetAsync(new JsonKey(key).ToLowerCase(), getter);
    }
}