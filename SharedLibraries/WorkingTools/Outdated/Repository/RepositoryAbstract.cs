using System;
using System.Collections.Generic;
using System.Linq;
using WorkingTools.Extensions;

namespace WorkingTools.Repository
{
    /// <summary>
    /// Репозиторий "один ключ - одно значение" с отложенным сохранением и загрузкой по требованию
    /// </summary>
    /// <typeparam name="TKey">ключ (обязательна перегрузка GetHashCode и Equals)</typeparam>
    /// <typeparam name="TValue">элемент (желательна перегрузка GetHashCode и Equals)</typeparam>
    public abstract class RepositoryAbstract<TKey, TValue> : IRepository<TKey, TValue>, IDisposable
    {
        protected object Lock = new object();

        private bool _loaded = false;
        
        private readonly Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, TValue> _newItems = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, TValue> _delItems = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, TValue> _updItems = new Dictionary<TKey, TValue>();

        private readonly HashSet<TKey> _notExistKeys = new HashSet<TKey>();


        protected virtual bool ContainsInternal(TKey key)
        {
            if (_loaded == false) LoadInternal();

            if (_items.ContainsKey(key))
                return true;

            if (_notExistKeys.Contains(key))
                return false;

            TValue value;
            return LoadInternal(key, out value);
        }

        public bool Contains(TKey key) { lock (Lock) return ContainsInternal(key); }

        protected virtual IEnumerable<KeyValuePair<TKey, TValue>> GetInternal()
        {
            if (_loaded == false) LoadInternal();
            return _items.ToArray();//возможно коллекция будет изменена во время перечисления
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Get() { lock (Lock) return GetInternal(); }

        protected virtual bool GetInternal(TKey key, out TValue value)
        {
            if (_loaded == false) Load();

            if (_items.ContainsKey(key))
            {
                value = _items[key];
                return true;
            }
            else
            {
                return LoadInternal(key, out value);
            }
        }

        public bool Get(TKey key, out TValue value) { lock (Lock) return GetInternal(key, out value); }

        protected virtual TValue GetInternal(TKey key)
        {
            TValue value;
            GetInternal(key, out value);
            return value;
        }

        public TValue Get(TKey key) { return GetInternal(key); }

        protected virtual void SetInternal(TKey key, TValue value, bool autoLoad = true)
        {
            if (_loaded == false && autoLoad) Load();

            /*если элемент существует*/
            if (_items.ContainsKey(key))
            {
                /*если это новый элемент то изменитьего значение в справочнике измененных*/
                if (_newItems.ContainsKey(key))
                    _newItems[key] = value;
                else
                    _updItems.Add(key, value);


                _items[key] = value;
            }
            /*если элемент не существует*/
            else
            {
                /*если элемент есть в списке удаленных*/
                if (_delItems.ContainsKey(key))
                {
                    /*если его занчение не совпадает с добавляемым, то добавить в список измененных*/
                    if (!Equals(_delItems[key], value))
                        _updItems.Add(key, value);

                    _delItems.Remove(key);
                }
                else
                {
                    _newItems.Add(key, value);
                }

                if (_notExistKeys.Contains(key))
                    _notExistKeys.Remove(key);

                _items.Add(key, value);
            }
        }

        public void Set(TKey key, TValue value) { lock (Lock) SetInternal(key, value); }

        protected virtual bool RemoveInternal(TKey key)
        {
            if (_loaded == false) Load();

            if (!_items.ContainsKey(key))
                return false;

            /*если удаляется не новый элемент (сохраненный) то добавить его в список удаленных*/
            if (!_newItems.Remove(key))
                _delItems.Add(key, _items[key]);
            /*иначе удалить его из измененных (если он там есть)*/
            else
                _updItems.Remove(key);

            /*и удалить его из основного списка*/
            return _items.Remove(key);
        }

        public bool Remove(TKey key) { lock (Lock) return RemoveInternal(key); }

        protected virtual void ClearInternal()
        {
            if (_loaded == false) LoadInternal();

            /*очистить список новых элементов*/
            _newItems.ForEach(item => _items.Remove(item.Key));
            _newItems.Clear();

            /*очистить список измененных элементов*/
            _updItems.Clear();

            /*перенести все оставшиеся элементы в удаленные*/
            _items.ForEach(item => _delItems.Add(item.Key, item.Value));
            _items.Clear();
        }

        public void Clear() { lock (Lock) ClearInternal(); }

        protected virtual bool LoadInternal(TKey key, out TValue value)
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

        /// <summary>
        /// Подгрузить элемент по ключу
        /// </summary>
        /// <param name="key">ключ</param>
        /// <param name="value">значение</param>
        /// <returns>true если удалось найти элемент по ключу</returns>
        /// <remarks>если элемент не будет подгружен - то ключь будет помечен как несуществующий и в дальнейшем не будет пытаться подгрузиться</remarks>
        protected bool Load(TKey key, out TValue value) { lock (Lock) return LoadInternal(key, out value); }

        protected virtual void LoadInternal()
        {
            _items.Clear();

            _newItems.Clear();
            _delItems.Clear();
            _updItems.Clear();

            _notExistKeys.Clear();

            /*добавить загруженные элементы в _items*/
            LoadItems().Do(items => items.ForEach(item => SetInternal(item.Key, item.Value, false)));

            _loaded = true;//критично чтобы переменная была true до вызова метода Set
        }

        public void Load() { lock (Lock) LoadInternal(); }

        protected virtual void SaveInternal()
        {
            /*если история не загружена*/
            if (_loaded == false)
                return;

            /*если в истории нет изменений*/
            if (_newItems.Count <= 0 && _delItems.Count <= 0 && _updItems.Count <= 0)
                return;

            SaveItems(_items, _newItems, _updItems, _delItems);

            _newItems.Clear();
            _delItems.Clear();
            _updItems.Clear();
        }

        public virtual void Save() { lock (Lock) SaveInternal(); }

        public void Dispose()
        {
            lock (Lock)
            {
                _items.Clear();
                _newItems.Clear();
                _delItems.Clear();
                _updItems.Clear();
            }
        }

        protected virtual bool LoadItem(TKey key, out TValue value) { value = default(TValue); return false; }

        protected abstract IEnumerable<Pair<TKey, TValue>> LoadItems();

        protected abstract void SaveItems(IEnumerable<KeyValuePair<TKey, TValue>> items, IEnumerable<KeyValuePair<TKey, TValue>> newItems,
            IEnumerable<KeyValuePair<TKey, TValue>> updItems, IEnumerable<KeyValuePair<TKey, TValue>> delItems);
    }
}
