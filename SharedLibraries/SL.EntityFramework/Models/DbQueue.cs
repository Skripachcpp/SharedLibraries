using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using FP.DataLayer.EfDb;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace SL.EntityFramework
{
    public class DbQueue<T>
        where T : class
    {
        private readonly EfContextLite _ef;
        private readonly DbSet<QueueTask> _queueTask;
        private readonly string _queueCode;
        private readonly Action<T[], CancellationToken> _taskHandler;
        private readonly int _packetSize;

        public DbQueue(EfContextLite ef, DbSet<QueueTask> queueTask, string code, Action<T[], CancellationToken> taskHandler, int packetSize = 1000)
        {
            _ef = ef;
            _queueTask = queueTask;
            _queueCode = code;
            _taskHandler = taskHandler;
            _packetSize = packetSize;
        }

        protected static void TaskHandler<TObj>(DbSet<QueueTask> queueTask, string queueCode, Action<TObj[], CancellationToken> taskHandler, QTaskPrms[] tasks, CancellationToken token)
            where TObj : class
        {
            var packet = new TObj[tasks.Length];
            var keys = new List<Guid>();
            var index = 0;
            foreach (var a in tasks)
            {
                packet[index] = a.Prms.FromJson<TObj>();
                keys.Add(a.Key);

                index++;
            }

            taskHandler(packet, token);

            if (!token.IsCancellationRequested)
                queueTask.Where(a => keys.Contains(a.Key) && a.QueueCode == queueCode)
                    .Update(a => new QueueTask() {CompliteDate = DateTime.Now});
        }

        protected static QueueTask[] TasksGetter(DbSet<QueueTask> queueTask, string queueCode, int packetSize)
        {
            var tasks = queueTask.Where(a => a.CompliteDate == null && a.QueueCode == queueCode).Take(packetSize).ToArray();
            return tasks;
        }

        public void Start(CancellationToken? token = null)
        {
            if (token == null) token = CancellationToken.None;

            QueueTask[] dbpacket;
            while ((dbpacket = TasksGetter(_queueTask, _queueCode, _packetSize)).Length > 0 && !token.Value.IsCancellationRequested)
            {
                var packet = dbpacket.Select(a => new QTaskPrms() { Key = a.Key, Prms = a.Prms }).ToArray();
                TaskHandler(_queueTask, _queueCode, _taskHandler, packet, token.Value);
            }
        }

        //private readonly List<string> _queue = new List<string>();
        //public void SetDef(IEnumerable<T> vs)
        //{
        //    lock (_queue) _queue.AddRange(vs.Select(a => a.ToJson()));
        //}
        //public void SetDef(T v)
        //{
        //    lock (_queue) _queue.Add(v.ToJson());
        //}

        //public void Set()
        //{
        //    if (_queue.Count <= 0) return;

        //    lock (_queue)
        //    {
        //        EFBatchOperation
        //            .For(_ef, _queueTask)
        //            .InsertAll(_queue.Select(a => new QueueTask()
        //            {
        //                Key = Guid.NewGuid(),
        //                QueueCode = _queueCode,
        //                Prms = a,
        //                CreateDate = DateTime.Now
        //            }));

        //        _queue.Clear();
        //    }
        //}

        //public void Clear()
        //{
        //    lock (_queue)
        //    {
        //        _queue.Clear();
        //        _queueTask.Where(a => a.CompliteDate == null && a.QueueCode == _queueCode).Delete();
        //    }
        //}
    }


    public class QTaskPrms
    {
        public Guid Key { get; set; }
        public string Prms { get; set; }
        

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((QTaskPrms) obj));
        protected bool Equals(QTaskPrms other) => Key.Equals(other.Key) && string.Equals(Prms, other.Prms);
        public override int GetHashCode() { unchecked { return (Key.GetHashCode() * 397) ^ (Prms?.GetHashCode() ?? 0); } }
        public static bool operator ==(QTaskPrms left, QTaskPrms right) => Equals(left, right);
        public static bool operator !=(QTaskPrms left, QTaskPrms right) => !Equals(left, right);
    }


    internal static class JsonExtension
    {
        private static JsonSerializerSettings _jsonSerializerSettingsHiddenNull;
        public static string ToJson<TObj>(this TObj obj, bool hiddenNull = true, bool indented = false)
        {
            return JsonConvert.SerializeObject(obj,
                settings: (hiddenNull ? _jsonSerializerSettingsHiddenNull ?? (_jsonSerializerSettingsHiddenNull = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) : null),
                formatting: indented ? Formatting.Indented : Formatting.None);
        }

        public static object FromJson(this string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public static TObj FromJson<TObj>(this string json)
        {
            return JsonConvert.DeserializeObject<TObj>(json);
        }
    }
}