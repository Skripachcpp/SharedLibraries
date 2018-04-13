using System;
using System.Collections.Generic;
using System.Threading;
using WorkingTools.Repository;

namespace WorkingTools.Parallel
{
    public class QueueTasksLite<TKey, TActionParams>: IDisposable
    {
        protected readonly object Lock = new object();

        protected readonly Pool Pool;

        protected readonly Action<TActionParams> Action;
        protected readonly Action<TActionParams> Continuation;
        protected readonly Action<TActionParams, Exception> WasError;//если произошла ошибка в action

        protected readonly HashSet<TKey> FailTasks = new HashSet<TKey>();
        protected readonly HashSet<TKey> ProcessingTasks = new HashSet<TKey>();
        protected readonly HashSet<TKey> PoolTasks = new HashSet<TKey>();

        protected readonly IRepositoryReadOnly<TKey, TActionParams> Repository;

        public QueueTasksLite(Action<TActionParams> action,
            IRepositoryReadOnly<TKey, TActionParams> repositoryReadOnly,
            Action<TActionParams> continuation = null,
            Action<TActionParams, Exception> wasError = null,
            bool allowReprocessing = false,
            int? parallel = null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (repositoryReadOnly == null) throw new ArgumentNullException(nameof(repositoryReadOnly));

            Repository = repositoryReadOnly;

            Pool = new Pool();
            _parallel = Pool.Parallel;
            Action = action;
            WasError = wasError;

            Continuation = continuation;
            AllowReprocessing = allowReprocessing;

            if (parallel != null) Parallel = (int)parallel;
        }

        private int _parallel;
        /// <summary>
        /// Допустимое число задач выполняемых паралельно
        /// </summary>
        /// <remarks>вы можете выставить кол-во паралельно выполняемых задач в 0,
        /// активные задачи будут завершены, а ожидающие своей очереди задачи не будут запущены,
        /// обработка задач будет считаться завершенной и не нечнется до вызова метода Invoke();
        /// если вновь изменить свойство на отличное от 0 до того того как все активные задачи были завершены,
        /// кол-во паралельно выполняемых задач будут стремиться к указаному</remarks>
        public int Parallel { get { return _parallel; } set { lock (Lock) _parallel = Pool.Parallel = value; } }

        public bool AllowReprocessing { get; set; }

        protected virtual void PerformCompleted(TKey key)
        {
        }

        /// <summary>
        /// Выполнение задачи в очереди
        /// </summary>
        /// <param name="key">идентификатор задачи</param>
        /// <param name="prms">параметры</param>
        /// <param name="token"></param>
        protected virtual void Perform(TKey key, TActionParams prms, CancellationToken? token)
        {
            PoolTasks.Remove(key);

            if (token != null && ((CancellationToken)token).IsCancellationRequested)
                return;

            try
            {
                Action(prms);

                ProcessingTasks.Add(key);
                FailTasks.Remove(key);

                PerformCompleted(key);
            }
            catch (Exception ex)
            {
                FailTasks.Add(key);

                if (WasError != null)
                    try { WasError(prms, ex); }
                    catch (Exception) {/*ignored*/}

                return;
            }

            //запускается только в случае успешного завершения операции
            if (Continuation != null)
                try { Continuation(prms); }
                catch (Exception) {/*ingnore*/}
        }

        /// <summary>
        /// Запустить повторную обработку задач завершившихся ошибкой
        /// </summary>
        /// <returns>true если запущена обработка; false если задач завершившихся ошибкой не найдено</returns>
        public bool ReRunFailTasks(CancellationToken? token = null)
        {
            var startAny = false;
            foreach (var key in FailTasks)
            {
                TActionParams value;
                if (Repository.Get(key, out value))
                {
                    PoolTasks.Add(key);
                    Pool.Invoke(Perform, key, value, token);
                    startAny = true;
                }
            }

            FailTasks.Clear();

            return startAny;
        }


        /// <summary>
        /// Очистить историю выполнения
        /// </summary>
        public void ClearProcessHistory()
        {
            ProcessingTasks.Clear();
            FailTasks.Clear();
        }

        protected virtual void LoadAndInvokeTasks(CancellationToken? token, bool autoLoad)
        {
            if (autoLoad) lock (Lock) Repository.Load();

            lock (Lock)
            {
                foreach (var pair in Repository.Get())
                {
                    //начать обработку если эту задачу еще не выполняли
                    if ((AllowReprocessing || !ProcessingTasks.Contains(pair.Key) && !FailTasks.Contains(pair.Key)) && !PoolTasks.Contains(pair.Key))
                    {
                        PoolTasks.Add(pair.Key);
                        Pool.Add(Perform, pair.Key, pair.Value, token);
                    }
                }
            }

            Pool.Invoke();
        }

        /// <summary>
        /// Запустить обработку очереди
        /// </summary>
        /// <remarks>при первом вызове происходит подгрузка задач из репозитория;
        /// автоматическая подгрузка задач провоцирует синхронное выполнение метода</remarks>
        public void Start(CancellationToken? token = null, bool autoLoad = false, int autoLoadInterval = 10 * 1000)
        {
            Pool.Parallel = Parallel;//значение Pool.Parallel меняется в Stop() и может отличатся от Parallel

            if (!autoLoad)
            {
                LoadAndInvokeTasks(token, autoLoad);
            }
            else
            {
                if (autoLoadInterval <= 0) autoLoadInterval = 500;

                do
                {
                    do
                    {
                        Pool.WaitAllRunning(); //подождать пока все не будут в работе

                        LoadAndInvokeTasks(token, autoLoad); //докинуть задач на выполнеине
                    } while (!Pool.WaitAllRunning(autoLoadInterval));//подождать пока все задачи не будут в очереди, если не дождалить, то докинуть еще
                } while (!Pool.WaitAll(autoLoadInterval));//подождать выполнения всех задач, если не дождалить, то докинуть еще
            }
        }

        /// <summary>
        /// Преостановить выполнение задач
        /// </summary>
        public void Stop()
        {
            lock (Lock)
            {
                Pool.Parallel = 0;

                Pool.WaitAll();
            }
        }

        /// <summary>
        /// Ждать завершения всех задач
        /// </summary>
        public void WaitAll()
        {
            Pool.WaitAll();
        }

        /// <summary>
        /// Ждать завершения всех задач
        /// </summary>
        /// <param name="millisecondsTimeout">максимальный период ожидания</param>
        /// <returns>true если задачи завершились до окончиния периода ожидания</returns>
        public bool WaitAll(int millisecondsTimeout)
        {
            return Pool.WaitAll(millisecondsTimeout);
        }

        #region events

        public event EventHandler Disposed;

        protected virtual void OnDisposed()
        {
            var handler = Disposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion events

        public virtual void Dispose()
        {
            OnDisposed();

            lock (Lock)
            {
                if (Pool != null) Pool.Dispose();//критично уничтожить pool до repository
                if (Repository != null) Repository.Dispose();
            }
        }
    }
}