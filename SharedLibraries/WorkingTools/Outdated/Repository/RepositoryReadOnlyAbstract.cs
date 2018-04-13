using System;
using System.Collections.Generic;
using WorkingTools.Extensions;

namespace WorkingTools.Repository
{
    public interface IRepositoryReadOnly<TKey, TValue> : IDisposable
    {
        bool Contains(TKey key);

        IEnumerable<KeyValuePair<TKey, TValue>> Get();
        bool Get(TKey key, out TValue value);
        TValue Get(TKey key);

        void Load();
    }

    public abstract class RepositoryReadOnlyAbstract<TKey, TValue> : IRepositoryReadOnly<TKey, TValue>, IDisposable
    {
        protected object Lock = new object();

        private bool _loaded = false;

        private readonly Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
        private readonly HashSet<TKey> _notExistKeys = new HashSet<TKey>();


        public virtual bool Contains(TKey key)
        {
            lock (Lock)
            {
                if (_loaded == false) Load();

                if (_items.ContainsKey(key))
                    return true;

                if (_notExistKeys.Contains(key))
                    return false;

                TValue value;
                return Load(key, out value);
            }
        }

        public virtual IEnumerable<KeyValuePair<TKey, TValue>> Get()
        {
            lock (Lock)
            {
                if (_loaded == false) Load();

                return _items;
            }
        }

        public virtual bool Get(TKey key, out TValue value)
        {
            lock (Lock)
            {
                if (_loaded == false) Load();

                if (_items.ContainsKey(key))
                {
                    value = _items[key];
                    return true;
                }
                else
                {
                    return Load(key, out value);
                }
            }
        }

        public virtual TValue Get(TKey key)
        {
            TValue value;
            Get(key, out value);
            return value;
        }

        /// <summary>
        /// Подгрузить элемент по ключу
        /// </summary>
        /// <param name="key">ключ</param>
        /// <param name="value">значение</param>
        /// <returns>true если удалось найти элемент по ключу</returns>
        /// <remarks>если элемент не будет подгружен - то ключь будет помечен как несуществующий и в дальнейшем не будет пытаться подгрузиться</remarks>
        protected virtual bool Load(TKey key, out TValue value)
        {
            //попробовать подгрузить элемент
            if (!_notExistKeys.Contains(key))
            {
                if (LoadItem(key, out value))
                {
                    _items.Add(key, value);
                    return true;
                }
                else
                {
                    _notExistKeys.Add(key);
                }
            }

            value = default(TValue);
            return false;
        }

        public virtual void Load()
        {
            lock (Lock)
            {
                _items.Clear();

                _notExistKeys.Clear();

                /*добавить загруженные элементы в _items*/
                LoadItems().Do(items => items.ForEach(item => _items.Add(item.Key, item.Value)));

                _loaded = true;
            }
        }       

        public void Dispose()
        {
            lock (Lock)
            {
                _items.Clear();
            }
        }

        /// <summary>
        /// Подгрузить элемент по ключу
        /// </summary>
        /// <param name="key">ключ</param>
        /// <param name="value">значение</param>
        /// <returns>true если значение удалось найти</returns>
        protected virtual bool LoadItem(TKey key, out TValue value) { value = default(TValue); return false; }

        /// <summary>
        /// Подгрузить все элементы
        /// </summary>
        /// <returns>пары ключь-значение</returns>
        protected abstract IEnumerable<Pair<TKey, TValue>> LoadItems();
    }

    public class RepositoryReadOnly<TKey, TValue> : RepositoryReadOnlyAbstract<TKey, TValue>
    {
        private readonly Func<IEnumerable<Pair<TKey, TValue>>> _loadItems;
        private readonly Func<TKey, TValue> _loadItem;


        public RepositoryReadOnly(Func<IEnumerable<Pair<TKey, TValue>>> loadItems, Func<TKey, TValue> loadItem = null)
        {
            if (loadItems == null) throw new ArgumentNullException(nameof(loadItems));

            _loadItems = loadItems;
            _loadItem = loadItem;
        }

        protected override bool LoadItem(TKey key, out TValue value)
        {
            if (_loadItem != null)
            {
                value = _loadItem(key);
                return true;
            }

            return base.LoadItem(key, out value);
        }

        protected override IEnumerable<Pair<TKey, TValue>> LoadItems()
        {
            return _loadItems();
        }
    }

    public class RepositoryReadOnly<TValue> : RepositoryReadOnly<int, TValue>
    {
        public RepositoryReadOnly(Func<IEnumerable<Pair<int, TValue>>> loadItems, Func<int, TValue> loadItem = null) : base(loadItems, loadItem)
        {
        }
    }
}