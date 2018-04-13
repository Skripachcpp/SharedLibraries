using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkingTools.Extensions;

namespace WorkingTools.Repository
{
    public interface IRepository<TItem>
    {
        IEnumerable<TItem> Get();

        bool Add(TItem item);

        bool Remove(TItem item);

        void Clear();

        void Load();
        void Save();
    }

    public class FileRepository<TItem> : IRepository<TItem>
    {
        private readonly HashSet<TItem> _items;
        private bool _itemsChanged = false;
        private bool _loaded = false;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="file"></param>
        /// <param name="primaryKey"></param>
        public FileRepository(string file, IEqualityComparer<TItem> primaryKey = null)
        {
            if (string.IsNullOrWhiteSpace(file)) throw new ArgumentOutOfRangeException(@"не указанно нименование файла репозитория");

            File = file;
            _items = new HashSet<TItem>(primaryKey);
        }

        public string File { get; private set; }

        public bool Contains(TItem item)
        {
            if (!_loaded) Load();

            return _items.Contains(item);
        }

        public IEnumerable<TItem> Get()
        {
            if (!_loaded) Load();

            return _items;
        }

        public bool Add(TItem item)
        {
            if (!_loaded) Load();

            var addFl = _items.Add(item);
            if (addFl) _itemsChanged = true;
            return addFl;
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            if (!_loaded) Load();

            var addFl = false;
            items.ForEach(item => { if (_items.Add(item)) addFl = true; });
            if (addFl) _itemsChanged = true;
        }

        public bool Remove(TItem item)
        {
            if (!_loaded) Load();

            var removeFl = _items.Remove(item);
            if (removeFl) _itemsChanged = true;
            return removeFl;
        }

        public void Clear()
        {
            if (_items.Count > 0) _itemsChanged = true;
            _items.Clear();
        }

        public void Load()
        {
            _loaded = true;
            _itemsChanged = false;
            _items.Clear();

            var items = LoadInternal();

            if (items != null)
                foreach (var item in items)
                    _items.Add(item);
        }

        public void Save()
        {
            if (_itemsChanged)
                SaveInternal(_items);
        }

        public void SetItemsChanged()
        {
            _itemsChanged = true;
        }

        protected virtual IEnumerable<TItem> LoadInternal()
        {
            var file = File;

            if (!System.IO.File.Exists(file))
                return null;

            using (var fileStream = System.IO.File.OpenRead(file))
            {
                if (fileStream.Length <= 0)
                    return null;

                TItem[] items = null;
                try { items = fileStream.FromXml<TItem[]>(); }
                catch (Exception) { /*ignore*/ }

                return items;
            }
        }

        protected virtual void SaveInternal(IEnumerable<TItem> items)
        {
            var file = File;

            var listItems = items.ToList();

            if (listItems.Count <= 0)
                System.IO.File.Delete(file);
            else
                using (var memoryStream = listItems.ToXmlMemoryStream())
                    memoryStream.Save(file);
        }
    }

    public class FileRepository<TKey, TValue> : RepositoryAbstract<TKey, TValue>
    {
        public FileRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentOutOfRangeException(nameof(filePath), "не указан путь до файла");

            FilePath = filePath;
        }

        public string FilePath { get; private set; }

        protected override IEnumerable<Pair<TKey, TValue>> LoadItems()
        {
            string filePath = FilePath;

            if (!File.Exists(filePath))
                return null;

            using (var fileStream = File.OpenRead(filePath))
            {
                Pair<TKey, TValue>[] repositoryItems = null;

                try { repositoryItems = fileStream.FromXml<Pair<TKey, TValue>[]>(); }
                catch (Exception) { /*ignore*/ }

                return repositoryItems;
            }
        }

        protected override void SaveItems(IEnumerable<KeyValuePair<TKey, TValue>> items, IEnumerable<KeyValuePair<TKey, TValue>> newItems, IEnumerable<KeyValuePair<TKey, TValue>> updItems, IEnumerable<KeyValuePair<TKey, TValue>> delItems)
        {
            string filePath = FilePath;

            var repositoryItems = new List<Pair<TKey, TValue>>();
            items.ForEach(item => repositoryItems.Add(new Pair<TKey, TValue>() { Key = item.Key, Value = item.Value }));

            if (repositoryItems.Count <= 0)
                File.Delete(filePath);
            else
                using (var memoryStream = repositoryItems.ToXmlMemoryStream())
                    memoryStream.Save(filePath);
        }
    }

    public class Pair<TKey, TValue>
    {
        public Pair() { }
        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}
