using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkingTools.Extensions;

namespace WorkingTools.Parallel
{
    /// <summary>
    /// Асинхронная очередь задач
    /// </summary>
    /// <typeparam name="TParams">наличие Comparer-а или перегрузка методов Equals и GetHashCode обязательны для объектов</typeparam>
    public class QueueTasks<TParams> : IDisposable
    {
        protected object Lock = new object();

        protected readonly Pool Pool = new Pool();

        protected readonly Action<TParams, CancellationToken> TaskHandler;
        protected Action<TParams, CancellationToken> TaskHandlerContinuation;
        protected Func<IEnumerable<TParams>> TasksGetter;


        protected readonly List<Exception> FailTasksGetter;
        protected readonly Dictionary<TParams, Exception> FailTasks;
        protected readonly Dictionary<TParams, Exception> FailContinuationTasks;
        protected readonly HashSet<TParams> PoolTasks;
        protected readonly HashSet<TParams> PerformCompleted;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="taskHandler">обработчик задачи</param>
        /// <param name="parallel">число паралельно обрабатываемых задач</param>
        /// <param name="comparer">объект хранит историю выполнения задач, 
        /// дабы избежать повторного выполнения задачи возвращаемой getter-ом 
        /// используется comparer, для поиска задачи в истории выполнения</param>
        protected QueueTasks(Action<TParams, CancellationToken> taskHandler, int? parallel = null, IEqualityComparer<TParams> comparer = null)
        {
            TaskHandler = taskHandler;

            if (parallel != null) Pool.Parallel = parallel.Value;

            FailTasksGetter = new List<Exception>();
            FailTasks = new Dictionary<TParams, Exception>(comparer);
            FailContinuationTasks = new Dictionary<TParams, Exception>();
            PoolTasks = new HashSet<TParams>(comparer);
            PerformCompleted = new HashSet<TParams>(comparer);

            AllowReprocessing = false;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="taskHandler">обработчик задачи</param>
        /// <param name="tasksGetter">функция получения задач</param>
        /// <param name="taskHandlerContinuation">продолжение (вызывается в случае успешного завершения задачи)</param>
        /// <param name="parallel">число паралельно обрабатываемых задач</param>
        /// <param name="comparer">объект хранит историю выполнения задач, 
        /// дабы избежать повторного выполнения задачи возвращаемой getter-ом 
        /// используется comparer, для поиска задачи в истории выполнения</param>
        public QueueTasks(Action<TParams, CancellationToken> taskHandler, Func<IEnumerable<TParams>> tasksGetter,
            Action<TParams, CancellationToken> taskHandlerContinuation = null, int? parallel = null,
            IEqualityComparer<TParams> comparer = null)
            : this(taskHandler, parallel, comparer)
        {
            if (taskHandler == null) throw new ArgumentNullException(nameof(taskHandler));
            if (tasksGetter == null) throw new ArgumentNullException(nameof(tasksGetter));

            TasksGetter = tasksGetter;
            TaskHandlerContinuation = taskHandlerContinuation;
        }

        /// <summary>
        /// Кол-во параленьно обрабатываемых операций
        /// </summary>
        public int Parallel { get { return Pool.Parallel; } set { Pool.Parallel = value; } }

        /// <summary>
        /// Повторно обрабатывать задачи завершившие свое выполнение
        /// </summary>
        public bool AllowReprocessing { get; set; }

        protected virtual void TaskCompliteEvent(TParams prms, CancellationToken token) { }

        /// <summary>
        /// Выполнение задачи в очереди
        /// </summary>
        /// <param name="prms">параметры</param>
        /// <param name="token"></param>
        /// <remarks>в случае если операция завершена из за токена отмены - она должна бросить исключения,
        /// дабы продолжение не было вызвано</remarks>
        protected virtual void Perform(TParams prms, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                TaskHandler(prms, token);

                lock (Lock)
                {
                    PoolTasks.Remove(prms);
                    FailTasks.Remove(prms);

                    if (!PerformCompleted.Contains(prms)) PerformCompleted.Add(prms);
                }

                TaskCompliteEvent(prms, token);
            }
            catch (Exception ex)
            {
                lock (Lock)
                {
                    PoolTasks.Remove(prms);
                    FailTasks.Add(prms, ex);
                }

                return;
            }

            //запускается только в случае успешного завершения операции
            if (TaskHandlerContinuation != null)
                try { TaskHandlerContinuation(prms, token); }
                catch (Exception ex) { FailContinuationTasks.Add(prms, ex); }
        }

        /// <summary>
        /// Добрать значения из очереди задач
        /// </summary>
        /// <returns>true если в очередь добавлена хотябы одна задача</returns>
        private bool RunNewTasks(CancellationToken token)
        {
            if (token.IsCancellationRequested || TasksGetter == null)
                return false;

            var startAny = false;

            IEnumerable<TParams> tasks;
            try { tasks = TasksGetter(); }
            catch (Exception ex)
            {
                lock (Lock) FailTasksGetter.Add(new Exception("При получении задач очереди произошла ошибка: ", ex));
                return false;
            }

            foreach (var taskPrms in tasks)
            {
                lock (Lock)
                {
                    //пропустить задачу если она выполяется или завершилась с ошибкой
                    if (FailTasks.ContainsKey(taskPrms)
                        || PoolTasks.Contains(taskPrms)
                        || (PerformCompleted.Contains(taskPrms) && !AllowReprocessing))
                        continue;

                    //добавить задачу в список выполняемых задач
                    PoolTasks.Add(taskPrms);
                }

                //выполнить задачу
                Pool.Invoke(Perform, taskPrms, token);
                startAny = true;
            }

            return startAny;
        }


        //private const int AutoLoadIntervalDefault = 10 * 1000;
        private const int AutoLoadIntervalMin = 250;

        /// <summary>
        /// Запустить обработку очереди (синхронно)
        /// </summary>
        /// <remarks>при первом вызове происходит подгрузка задач из репозитория;</remarks>
        public void Start(CancellationToken? token = null, int? autoLoadInterval = null)
        {
            try
            {
                ClearPerformHistory();

                Pool.Parallel = Parallel;//значение Pool.Parallel меняется в Stop() и может отличатся от Parallel

                if (autoLoadInterval == null)
                {
                    RunNewTasks(token ?? CancellationToken.None);
                    Pool.WaitAll();
                }
                else
                {
                    int interval = autoLoadInterval.Value;
                    if (interval < AutoLoadIntervalMin) interval = AutoLoadIntervalMin;

                    do
                    {
                        do
                        {
                            Pool.WaitAllRunning(); //подождать пока все не будут в работе

                            RunNewTasks(token ?? CancellationToken.None); //докинуть задач на выполнеине
                        } while (!Pool.WaitAllRunning(interval));
                        //подождать пока все задачи не будут в очереди, если не дождалить, то докинуть еще
                    } while (!Pool.WaitAll(interval));
                    //подождать выполнения всех задач, если не дождалить, то докинуть еще
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ВНИМАНИЕ!!! Произошла ошибка внутренней части! Приложение может начать работать непредсказуемо!", ex);
            }

            if (ThrowException) ThrowExceptionInternal();
        }


        private Task _startAsyncTask = null;
        /// <summary>
        /// Запустить обработку очереди (асинхронно)
        /// </summary>
        /// <returns>признак успешного запуска асинхронной операции</returns>
        public bool StartAsync(CancellationToken? token = null, int? autoLoadInterval = null)
        {
            if (_startAsyncTask != null) return false;

            var callback = new Callback<CancellationToken?, int?>(Start, token, autoLoadInterval);
            _startAsyncTask = Task.Factory.StartNew(() => { callback.Invoke(); _startAsyncTask = null; }, token ?? CancellationToken.None);
            return true;
        }

        /// <summary>
        /// Дождаться окончания асинхронных операций
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool StartWait(int millisecondsTimeout = -1, CancellationToken? token = null)
        {
            var task = _startAsyncTask;
            return task == null || task.Wait(millisecondsTimeout, token ?? CancellationToken.None);
        }

        public void Stop()
        {
            Pool.Parallel = 0;
            Pool.WaitAll();
        }

        private void ClearPerformHistory()
        {
            lock (Lock)
            {
                FailTasksGetter.Clear();
                FailTasks.Clear();
                FailContinuationTasks.Clear();
                PoolTasks.Clear();
                PerformCompleted.Clear();
            }
        }

        private IEnumerable<Exception> ConvertExceptionItems(IEnumerable<KeyValuePair<TParams, Exception>> items)
        {
            return from item in items
                   let encoding = SerializeXmlExtension.DefaultWithoutBomEncoding
                   let xmlParams = item.Key.ToXmlString(encoding, omitXmlDeclaration: true, indent: false)
                   let message = $"\nprms: {xmlParams} \nex:"
                   select new Exception(message, item.Value);
        }

        public bool ThrowException { get; set; } = false;

        protected void ThrowExceptionInternal()
        {
            lock (Lock)
            {
                if (FailTasks.Count > 0 || FailContinuationTasks.Count > 0 || FailTasksGetter.Count > 0)
                {
                    var exsTasksGetter = FailTasksGetter;
                    var exsTasks = ConvertExceptionItems(FailTasks);
                    var exsContinuationTasks = ConvertExceptionItems(FailContinuationTasks);

                    var exsAll = exsTasksGetter.Union(exsTasks).Union(exsContinuationTasks);

                    throw new AggregateException(exsAll);
                }
            }
        }

        public void Dispose()
        {
            ClearPerformHistory();
        }
    }
}