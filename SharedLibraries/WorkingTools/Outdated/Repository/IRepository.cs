using System;

namespace WorkingTools.Repository
{
    public interface IRepository<TKey, TValue> : IRepositoryReadOnly<TKey, TValue>, IDisposable
    {
        void Set(TKey key, TValue value);
        bool Remove(TKey key);
        void Clear();
        void Save();
    }

    public interface IRepositoryAutoKey<TKey, TValue> : IRepository<TKey, TValue>
    {
        TKey Add(TValue value);
    }
}
