using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using SL.Elasticsearch.EsContextLiteInside;

namespace SL.Elasticsearch
{
    /// <summary>
    /// Простая реализайия CRUD операция для Elastic
    /// </summary>
    /// <remarks>обертка nest</remarks>
    public class EsContextLite : IDisposable
    {
        public virtual int Top => 10000;
        protected string RootIndex;
        protected string DefaultIndex;

        private readonly Lazy<ElasticClient> _client;
        public ElasticClient Client => _client.Value;

        public EsContextLite(string url, string rootIndex = null, string defaultIndex = null)
        {
            if (string.IsNullOrEmpty(defaultIndex))
                defaultIndex = null;

            RootIndex = rootIndex;
            DefaultIndex = defaultIndex;

            _client = new Lazy<ElasticClient>(() =>
            {
                var cntStr = new ConnectionSettings(new Uri(url))
                //.EnableHttpCompression()
                .DisableDirectStreaming()
                .ThrowExceptions(true)
                .RequestTimeout(new TimeSpan(0, 0, 15, 0))
                ;

                return new ElasticClient(cntStr);
            });
        }

        private Dictionary<Type, List<IBinder>> _binders = null;
        protected Binder<TSource, TTarget> Binder<TSource, TTarget>(Func<TSource, object> value, Expression<Func<TTarget, object>> field)
            where TSource : class
            where TTarget : class
        {
            var binder = new Binder<TSource, TTarget>(this);
            binder.ForeignKey(value, field);

            var typeSource = typeof(TSource);
            if (_binders.ContainsKey(typeSource)) _binders[typeSource].Add(binder);
            else _binders.Add(typeof(TSource), new List<IBinder>() { binder });

            return binder;
        }

        protected virtual void Binders()
        {
            _binders = new Dictionary<Type, List<IBinder>>();
        }

        private static readonly Dictionary<EntityIdKey, int> NewId = new Dictionary<EntityIdKey, int>();
        protected virtual T InitEntity<T>(T entity, string index)
            where T : class
        {
            var cd = entity as ICreateDate;
            if (cd != null && cd.CreateDate == null)
                cd.CreateDate = DateTime.UtcNow;

            var ent = entity as IAutoId;
            if (ent == null) return entity;

            if (ent.Id > 0) return entity;

            lock (NewId)
            {
                // проверяем автоинкримент
                var newidKey = new EntityIdKey(index, typeof(T));
                var newid = NewId.ContainsKey(newidKey) ? NewId[newidKey] : 0;

                if (newid > 0 && ent.Id > 0 && ent.Id >= newid) newid = ent.Id + 1;

                // /* присваиваем новое значение автоинкрименту и сущьности

                if (newid <= 0)
                {
                    var documents = Client.Search<T>(se => se
                        .Index(index).Type<T>()
                        .Sort(s => s.Field(f => f.Field("id").Order(SortOrder.Descending)))
                        .Source(s => s.Includes(i => i.Field("id"))).Size(1))?.Documents;

                    var obj = documents?.FirstOrDefault() as IAutoId;
                    newid = obj?.Id + 1 ?? 1;
                }

                ent.Id = newid;
                newid++;

                // */

                // /* сохраняем значение автоинкримента
                if (NewId.ContainsKey(newidKey)) NewId[newidKey] = newid;
                else NewId.Add(newidKey, newid);
                // */
            }

            return entity;
        }

        public void SetAutoId<T>(T entity, string index = null)
            where T : class, IAutoId
        {
            if (entity == null) return;

            index = BuildIndx(index);
            InitEntity(entity, index);
        }

        protected virtual void SetTriggerBefore<T>(T entity, string index = null) { }
        protected virtual void SetTriggerAfter<T>(T entity, string index = null) { }
        public T Set<T>(T entity, object parentId = null, string index = null, bool init = true, bool refresh = true, bool runSetTrigger = true, bool runBinder = true)
            where T : class
        {
            if (runSetTrigger) SetTriggerBefore(entity, index);

            var indx = BuildIndx(index);

            if (init) InitEntity(entity, indx);

            if (parentId == null && entity is IParent) parentId = ((IParent)entity).getparentid();

            if (parentId == null) Client.Index(entity, i => i.Index(indx).Type<T>().Refresh(refresh ? Refresh.WaitFor : Refresh.False));
            else Client.Index(entity, i => i.Index(indx).Type<T>().Parent(parentId.ToString()).Refresh(refresh ? Refresh.WaitFor : Refresh.False));

            if (runBinder)
            {
                if (_binders == null) Binders();

                //обновить все связанные записи
                var type = typeof(T);
                if (_binders.ContainsKey(type))
                {
                    var binders = _binders[type];
                    foreach (var binder in binders)
                        binder.UpdateTargets(entity);
                }
            }

            if (runSetTrigger) SetTriggerAfter(entity, index);

            return entity;
        }

        private readonly List<Task> _settasks = new List<Task>();


        public void SetAsync<T>(T entity, object parentId = null, string index = null, bool init = true,
            bool refresh = true) where T : class
        {
            var task = Task.Factory.StartNew(() => Set<T>(entity, parentId, index, init, refresh));
            lock (_settasks) _settasks.Add(task);

            if (_settasks.Count > 1000) removecomplited();
        }

        private int _setDefCount = 0;
        private readonly Dictionary<SetKey, HashSet<object>> _setQueue = new Dictionary<SetKey, HashSet<object>>();
        /// <summary>
        /// Сохранить отложенно
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentId"></param>
        /// <param name="index"></param>
        /// <param name="init"></param>
        public void SetDef<T>(T entity, object parentId = null, string index = null, bool init = true)
            where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            index = BuildIndx(index);

            if (init) InitEntity(entity, index);

            var setKey = new SetKey()
            {
                Index = index,
                //ParentId = parentId?.ToString(),
                Type = typeof(T),
            };

            lock (_setQueue)
            {
                if (!_setQueue.TryGetValue(setKey, out HashSet<object> queue))
                    _setQueue.Add(setKey, queue = new HashSet<object>());
                if (queue.Add(entity)) _setDefCount++;
            }
        }

        private void SavePack(string index, Type type, string parentId, List<object> bulkPack)
        {
            IBulkResponse br = null;
            Exception exception = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    br = Client.Bulk(b => b.Index(index).Type(type).IndexMany(bulkPack, (a, o) =>
                    {
                        var prntId = parentId ?? (o as IParent)?.getparentid();
                        if (prntId != null) return a.Parent(new Id(prntId));
                        else return a;
                    }).Refresh(Refresh.False));
                }
                catch (Exception e)
                {
                    //произошла ошибка? попробуй еще раз .. возможно это интернет лагнул
                    exception = e;
                    continue;
                }

                break;
            }

            if (br == null) throw exception ?? new Exception("не удалось получить ответ от сервера");
            if (br.Errors) throw new Exception(br.DebugInformation.Substring(0, 10000));
        }

        /// <summary>
        /// Выполнить отложенное сохранение
        /// </summary>
        public void Set(bool refresh = true, int? packet = 10000, Action<int, int> re = null)
        {
            if (re == null) re = (a, b) => { };
            var refreshThis = new HashSet<string>();
            lock (_setQueue)
            {
                int saved = 0;
                foreach (KeyValuePair<SetKey, HashSet<object>> setItem in _setQueue.Where(a => a.Value.Count > 0))
                {
                    var index = setItem.Key.Index;
                    var parentId = setItem.Key.ParentId;
                    var type = setItem.Key.Type;

                    List<object> bulkPack = packet != null ? new List<object>(packet.Value) : new List<object>();
                    int bulkPackLenght = 0;
                    foreach (var item in setItem.Value)
                    {
                        bulkPack.Add(item);

                        bulkPackLenght++;

                        if (packet != null && bulkPackLenght >= packet)
                        {
                            SavePack(index, type, parentId, bulkPack);

                            saved += bulkPack.Count;
                            re(saved, _setDefCount);

                            bulkPack.Clear();
                            bulkPackLenght = 0;
                        }
                    }

                    if (bulkPack.Count > 0)
                    {
                        SavePack(index, type, parentId, bulkPack);

                        saved += bulkPack.Count;
                        re(saved, _setDefCount);

                        bulkPack.Clear();
                        bulkPackLenght = 0;
                    }

                    refreshThis.Add(setItem.Key.Index);

                    setItem.Value.Clear();
                }

                _setQueue.Clear();
                _setDefCount = 0;
            }

            if (refresh)
            {
                foreach (var refreshIndex in refreshThis)
                {
                    if (!ExistsIndex(refreshIndex)) continue;
                    Client.Refresh(refreshIndex);
                }
            }
        }

        public bool SetWait()
        {
            var tasks = getsettasks().ToArray();
            if (tasks.Length <= 0)
                return true;

            Task.WaitAll(tasks);
            return true;
        }

        #region

        private void removecomplited()
        {
            lock (_settasks)
            {
                for (int index = _settasks.Count - 1; index > 0; index--)
                {
                    var task = _settasks[index];
                    if (task == null || task.IsCanceled || task.IsCompleted || task.IsFaulted)
                        _settasks.RemoveAt(index);
                }
            }
        }

        private IEnumerable<Task> getsettasks()
        {
            lock (_settasks)
            {
                for (int index = _settasks.Count - 1; index > 0; index--)
                {
                    var task = _settasks[index];
                    if (task == null || task.IsCanceled || task.IsCompleted || task.IsFaulted)
                    { _settasks.RemoveAt(index); continue; }

                    yield return task;
                }
            }
        }

        #endregion

        protected virtual void RemoveTriggerBefor<T>(string id, string index = null, bool force = false) { }
        protected virtual void RemoveTriggerAfter<T>(string id, string index = null, bool force = false) { }
        public void Remove<T>(string id, string index = null, bool runTrigger = true, bool force = false)
            where T : class
        {
            if (runTrigger) RemoveTriggerBefor<T>(id, index, force);

            index = BuildIndx(index);

            var path = new DocumentPath<T>(id);
            Client.Delete(path, s => s.Index(index));

            if (runTrigger) RemoveTriggerAfter<T>(id, index, force);
        }

        public T Get<T>(object id, Func<GetDescriptor<T>, IGetRequest> cnt = null, string parentId = null, string index = null)
            where T : class
        {
            if (id == null) return null;
            index = BuildIndx(index);

            if (cnt == null) cnt = c => c;

            var item = parentId == null
                ? Client.Get<T>(DocumentPath<T>.Id(id.ToString()), s => cnt(s.Index(index).Type<T>()))
                : Client.Get<T>(DocumentPath<T>.Id(id.ToString()), s => cnt(s.Index(index).Type<T>().Parent(parentId)));

            return item?.Source;
        }

        public T Get<T>(Expression<Func<T, object>> field, object value, Func<SearchDescriptor<T>, SearchDescriptor<T>> cnt = null, string index = null)
            where T : class
        {
            if (cnt == null) cnt = sede => sede.Size(1);
            else
            {
                var externalSearchDescriptor = cnt;
                cnt = sede => externalSearchDescriptor(sede).Size(1);
            }

            var items = FindByTerm<T>(field, value, cnt, index);
            return items?.FirstOrDefault();
        }

        public IReadOnlyCollection<T> Find<T>(string index = null) where T : class
        {
            index = BuildIndx(index);
            var search = Client.Search<T>(s => s.Index(index).Type<T>().Size(Top));
            return search?.Documents;
        }

        public Itms<IReadOnlyCollection<T0>> MultiSearch<T0>(Func<SearchDescriptor<T0>, ISearchRequest> src0)
            where T0 : class
        {
            var index = BuildIndx();

            if (src0 == null) src0 = se0 => se0;

            var search = Client.MultiSearch(ms => ms
                .Search<T0>("T0", s => src0(s.Index(index).Type<T0>())));

            return new Itms<IReadOnlyCollection<T0>>()
            {
                Itm0 = search.GetResponse<T0>("T0").Documents,
            };
        }

        public Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>> MultiSearch<T0, T1>(
            Func<SearchDescriptor<T0>, ISearchRequest> src0,
            Func<SearchDescriptor<T1>, ISearchRequest> src1)
            where T0 : class
            where T1 : class
        {
            var index = BuildIndx();

            if (src0 == null) src0 = se0 => se0;
            if (src1 == null) src1 = se1 => se1;

            var search = Client.MultiSearch(ms => ms
                .Search<T0>("T0", s => src0(s.Index(index).Type<T0>()))
                .Search<T1>("T1", s => src1(s.Index(index).Type<T1>())));

            return new Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>>()
            {
                Itm0 = search.GetResponse<T0>("T0").Documents,
                Itm1 = search.GetResponse<T1>("T1").Documents,
            };
        }

        public Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>, IReadOnlyCollection<T2>> MultiSearch<T0, T1, T2>(
            Func<SearchDescriptor<T0>, ISearchRequest> src0,
            Func<SearchDescriptor<T1>, ISearchRequest> src1,
            Func<SearchDescriptor<T2>, ISearchRequest> src2)
            where T0 : class
            where T1 : class
            where T2 : class
        {
            var index = BuildIndx();

            if (src0 == null) src0 = se0 => se0;
            if (src1 == null) src1 = se1 => se1;
            if (src2 == null) src2 = se2 => se2;

            var search = Client.MultiSearch(ms => ms
                .Search<T0>("T0", s => src0(s.Index(index).Type<T0>().Size(Top)))
                .Search<T1>("T1", s => src1(s.Index(index).Type<T1>().Size(Top)))
                .Search<T2>("T2", s => src2(s.Index(index).Type<T2>().Size(Top))));

            return new Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>, IReadOnlyCollection<T2>>()
            {
                Itm0 = search.GetResponse<T0>("T0").Documents,
                Itm1 = search.GetResponse<T1>("T1").Documents,
                Itm2 = search.GetResponse<T2>("T2").Documents,
            };
        }

        public Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>, IReadOnlyCollection<T2>, IReadOnlyCollection<T3>> MultiSearch<T0, T1, T2, T3>(
            Func<SearchDescriptor<T0>, ISearchRequest> src0,
            Func<SearchDescriptor<T1>, ISearchRequest> src1,
            Func<SearchDescriptor<T2>, ISearchRequest> src2,
            Func<SearchDescriptor<T3>, ISearchRequest> src3)
            where T0 : class
            where T1 : class
            where T2 : class
            where T3 : class
        {
            var index = BuildIndx();

            if (src0 == null) src0 = se0 => se0;
            if (src1 == null) src1 = se1 => se1;
            if (src2 == null) src2 = se2 => se2;
            if (src3 == null) src3 = se3 => se3;

            var search = Client.MultiSearch(ms => ms
                .Search<T0>("T0", s => src0(s.Index(index).Type<T0>()))
                .Search<T1>("T1", s => src1(s.Index(index).Type<T1>()))
                .Search<T2>("T2", s => src2(s.Index(index).Type<T2>()))
                .Search<T3>("T3", s => src3(s.Index(index).Type<T3>())));

            return new Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>, IReadOnlyCollection<T2>, IReadOnlyCollection<T3>>()
            {
                Itm0 = search.GetResponse<T0>("T0").Documents,
                Itm1 = search.GetResponse<T1>("T1").Documents,
                Itm2 = search.GetResponse<T2>("T2").Documents,
                Itm3 = search.GetResponse<T3>("T3").Documents,
            };
        }

        public Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>, IReadOnlyCollection<T2>, IReadOnlyCollection<T3>, IReadOnlyCollection<T4>> MultiSearch<T0, T1, T2, T3, T4>(
            Func<SearchDescriptor<T0>, ISearchRequest> src0,
            Func<SearchDescriptor<T1>, ISearchRequest> src1,
            Func<SearchDescriptor<T2>, ISearchRequest> src2,
            Func<SearchDescriptor<T3>, ISearchRequest> src3,
            Func<SearchDescriptor<T4>, ISearchRequest> src4)
            where T0 : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            var index = BuildIndx();

            if (src0 == null) src0 = se0 => se0;
            if (src1 == null) src1 = se1 => se1;
            if (src2 == null) src2 = se2 => se2;
            if (src3 == null) src3 = se3 => se3;
            if (src4 == null) src4 = se4 => se4;

            var search = Client.MultiSearch(ms => ms
                .Search<T0>("T0", s => src0(s.Index(index).Type<T0>()))
                .Search<T1>("T1", s => src1(s.Index(index).Type<T1>()))
                .Search<T2>("T2", s => src2(s.Index(index).Type<T2>()))
                .Search<T3>("T3", s => src3(s.Index(index).Type<T3>()))
                .Search<T4>("T4", s => src4(s.Index(index).Type<T4>())));

            return new Itms<IReadOnlyCollection<T0>, IReadOnlyCollection<T1>, IReadOnlyCollection<T2>, IReadOnlyCollection<T3>, IReadOnlyCollection<T4>>()
            {
                Itm0 = search.GetResponse<T0>("T0").Documents,
                Itm1 = search.GetResponse<T1>("T1").Documents,
                Itm2 = search.GetResponse<T2>("T2").Documents,
                Itm3 = search.GetResponse<T3>("T3").Documents,
                Itm4 = search.GetResponse<T4>("T4").Documents,
            };
        }

        public IReadOnlyCollection<T> Find<T>(Func<QueryContainerDescriptor<T>, QueryContainer> query,
            Func<SearchDescriptor<T>, SearchDescriptor<T>> cnt = null,
            string index = null) where T : class
        {
            index = BuildIndx(index);

            if (cnt == null) cnt = sede => sede;
            ISearchResponse<T> search = null;
            for (int i = 0; i < 100; i++)
                try { search = Client.Search<T>(s => cnt(s.Index(index).Type<T>().Size(Top).Query(query))); break; }
                catch (Exception ex) { if (i >= 2) throw ex; }

            return search?.Documents;
        }

        public IReadOnlyCollection<T> FindByTerm<T>(Expression<Func<T, object>> field, object value, Func<SearchDescriptor<T>, SearchDescriptor<T>> cnt = null, string index = null)
            where T : class
        {
            index = BuildIndx(index);

            if (cnt == null) cnt = sede => sede;

            var search = Client.Search<T>(s => cnt(s.Index(index).Type<T>().Query(q => q.Term(t => t.Field(field).Value(value))).Size(Top)));
            return search?.Documents;
        }



        public ISearchResponse<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> srch = null,
            string index = null) where T : class
        {
            index = BuildIndx(index);
            if (srch == null) srch = sede => sede;

            return Client.Search<T>(s => srch(s.Index(index).Type<T>().From(0).Size(Top)));
        }

        public IEnumerable<T> GetPageDocs<T>(out long total, int page, int count, Expression<Func<T, object>> sortBy, bool desc, Func<SearchDescriptor<T>, ISearchRequest> srch = null)
            where T : class
        {
            var search = GetPage(page, count, sortBy, desc, srch);
            total = search.Total;
            return search.Documents;
        }

        public IEnumerable<T> GetPageDocs<T>(out long total, int page, int count, Func<SearchDescriptor<T>, ISearchRequest> srch = null)
            where T : class
        {
            var search = GetPage(page, count, srch);
            total = search.Total;
            return search.Documents;
        }

        public ISearchResponse<T> GetPage<T>(int page, int count, Expression<Func<T, object>> sortBy, bool desc, Func<SearchDescriptor<T>, ISearchRequest> srch = null)
            where T : class
        {
            if (sortBy == null) throw new ArgumentNullException(nameof(sortBy));
            if (srch == null) srch = sl => sl;
            return GetPage<T>(page, count, s => srch(s.Sort(so => so.Field(sortBy, desc ? SortOrder.Descending : SortOrder.Ascending))));
        }

        public ISearchResponse<T> GetPage<T>(int page, int count, Func<SearchDescriptor<T>, ISearchRequest> srch = null)
            where T : class
        {
            if (srch == null) srch = sl => sl;
            var search = Search<T>(s => srch(s.From(page * count).Size(count)));

            return search;
        }

        public void RefreshIndex(string index = null)
        {
            index = BuildIndx(index);
            if (!ExistsIndex(index)) return;

            Client.Refresh(index);
        }

        public void Clear(string index = null)
        {
            index = BuildIndx(index, autoCreate: false);
            //если интекс не существует, и делать тут нечего
            if (!Client.IndexExists(index).Exists) return;

            lock (NewId)
            {
                var keys = NewId.Where(a => a.Key.Index == index).Select(a => a.Key).ToArray();
                foreach (var key in keys) NewId.Remove(key);
            }

            if (_indexCreated.Contains(index))
            {
                if (_lastIndexCreated == index)
                    _lastIndexCreated = null;

                _indexCreated.Remove(index);
            }

            Client.DeleteIndex(index);
        }

        public IUpdateByQueryResponse UpdateByQuery<T>(Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> upd, string index = null)
            where T : class
        {
            index = BuildIndx(index);
            var ur = Client.UpdateByQuery<T>(u => upd(u.Index(index)));
            return ur;
        }

        public void DeleteByQuery<T>(Func<QueryContainerDescriptor<T>, QueryContainer> query = null,
            Func<DeleteByQueryDescriptor<T>, DeleteByQueryDescriptor<T>> cnt = null,
            string index = null)
            where T : class
        {
            index = BuildIndx(index);
            if (!Client.IndexExists(index).Exists) return;

            if (query == null) query = q => q.MatchAll();
            if (cnt == null) cnt = sede => sede;
            Client.DeleteByQuery<T>(s => cnt(s.Index(index).Type<T>().Query(query)));
        }

        private readonly object ExistsIndexLock = new object();
        private readonly HashSet<string> _indexCreated = new HashSet<string>();
        private string _lastIndexCreated;
        protected bool ExistsIndex(string index)
        {
            //если индекс проверяли при предидущем вызове
            if (index == _lastIndexCreated)
                return true;


            //если индекс проверяли
            if (_indexCreated.Contains(index))
            {
                _lastIndexCreated = index;
                return true;
            }

            lock (ExistsIndexLock)
            {
                //если индекс проверяли
                if (_indexCreated.Contains(index))
                {
                    _lastIndexCreated = index;
                    return true;
                }

                //если индекс существует
                if (Client.IndexExists(index).Exists)
                {
                    _lastIndexCreated = index;
                    _indexCreated.Add(_lastIndexCreated = index);
                    return true;
                }
            }

            return false;
        }

        public string BuildIndx(string index = null, bool autoCreate = true)
        {
            if (string.IsNullOrWhiteSpace(index)) index = DefaultIndex;
            index = $"{RootIndex}{(RootIndex != null && index != null ? "-" : "")}{index}".ToLower();

            if (autoCreate) CreateIndexIfNotExists(index);
            return index;
        }

        private readonly object CreateIndexIfNotExistsLock = new object();
        protected virtual void CreateIndexIfNotExists(string index)
        {
            if (ExistsIndex(index)) return;
            lock (CreateIndexIfNotExistsLock)
            {
                if (ExistsIndex(index)) return;
                CreateIndex(index);

                _indexCreated.Add(_lastIndexCreated = index);
            }
        }

        public virtual void CreateIndex(string index) { }

        public virtual void Dispose() { }
    }

    public class SetKey
    {
        public string Index { get; set; }
        public Type Type { get; set; }
        public string ParentId { get; set; }

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((SetKey)obj));
        protected bool Equals(SetKey other) => string.Equals(Index, other.Index) && Type == other.Type && string.Equals(ParentId, other.ParentId);
        public override int GetHashCode() { unchecked { var hashCode = Index?.GetHashCode() ?? 0; hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0); hashCode = (hashCode * 397) ^ (ParentId?.GetHashCode() ?? 0); return hashCode; } }
        public static bool operator ==(SetKey left, SetKey right) => Equals(left, right);
        public static bool operator !=(SetKey left, SetKey right) => !Equals(left, right);
    }

    public static class QueryExtension
    {
        public static QueryContainer Wldcrd<T>(this QueryContainerDescriptor<T> qcd, Expression<Func<T, object>> field, string searchText)
            where T : class
        {
            return qcd.Wildcard(field, searchText, rewrite: (MultiTermQueryRewrite)null);
        }
    }

    public interface IBinder
    {
        void UpdateTargets(object source);
    }

    public class Binder<TSource, TTarget> : IBinder
        where TSource : class
        where TTarget : class
    {
        public Type Source { get; } = typeof(TSource);
        public Type Target { get; } = typeof(TTarget);

        protected readonly EsContextLite Es;
        public Binder(EsContextLite es) => Es = es;

        protected Expression<Func<TTarget, object>> TargetForeignKeyField { get; private set; }
        protected Func<TSource, object> TargetForeignKeyValue { get; private set; }
        public Binder<TSource, TTarget> ForeignKey(Func<TSource, object> targetForeignKeyValue, Expression<Func<TTarget, object>> targetForeignKeyField)
        {
            TargetForeignKeyField = targetForeignKeyField;
            TargetForeignKeyValue = targetForeignKeyValue;

            return this;
        }

        /// <summary>
        /// Set ending script
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public Binder<TSource, TTarget> Script(string script)
        {
            if (string.IsNullOrWhiteSpace(script)) throw new ArgumentOutOfRangeException(nameof(script));
            EndingScript = script;
            return this;
        }

        public Binder<TSource, TTarget> Field(Func<TSource, object> sourceField, Expression<Func<TTarget, object>> targetForeignKeyField)
        {
            var body = targetForeignKeyField.Body as MemberExpression;

            if (body == null)
            {
                var ubody = (UnaryExpression)targetForeignKeyField.Body;
                body = ubody.Operand as MemberExpression;
            }

            string fieldName = body?.Member?.Name;

            if (fieldName == null) throw new Exception("не удалось получить имя поля");

            //первый символ к нижнему регистру, хотя может это и не важно
            if (fieldName.Length <= 1) fieldName = fieldName.ToLowerInvariant();
            else fieldName = char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);

            return Field(fieldName, sourceField);
        }

        protected Dictionary<string, Func<TSource, object>> Fields { get; } = new Dictionary<string, Func<TSource, object>>();
        public Binder<TSource, TTarget> Field(string targetField, Func<TSource, object> sourceField)
        {
            Fields.Add(targetField, sourceField);
            return this;
        }


        public void UpdateTargets(object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var tsource = source as TSource;
            if (tsource == null) throw new ArgumentOutOfRangeException(nameof(source), $@"не удалось получить источник данных, ожидался {typeof(TSource).Name}");
            UpdateTargets(tsource);
        }

        protected string EndingScript { get; private set; }

        protected string RunScript { get; private set; }
        
        public void UpdateTargets(TSource source)
        {
            if (RunScript == null)
            {
                var sbScript = new StringBuilder();
                foreach (var field in Fields)
                {
                    sbScript.Append("ctx._source.");
                    sbScript.Append(field.Key);
                    sbScript.Append(" = params.");
                    sbScript.Append(field.Key);
                    sbScript.Append(";");
                }

                if (!string.IsNullOrWhiteSpace(EndingScript))
                {
                    sbScript.Append(EndingScript);
                }

                RunScript = sbScript.ToString();
            }


            Dictionary<string, object> prms = null;
            if (Fields != null && Fields.Count > 0)
                prms = Fields.ToDictionary(a => a.Key, a => a.Value(source));

            var foreignKeyValue = TargetForeignKeyValue(source);

            Es.UpdateByQuery<TTarget>(s => s
                .Query(q => q.Term(TargetForeignKeyField, foreignKeyValue))
                .Script(sc => sc.Inline(RunScript)
                    .Params(prms)));

        }
    }
}
