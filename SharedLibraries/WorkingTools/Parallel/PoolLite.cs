using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WorkingTools.Parallel
{
    public class PoolLite : IDisposable
    {
        protected readonly object Access = new object();//синхронизация доступа к переменным состояния
        protected readonly Dictionary<int?, Task> Tasks = new Dictionary<int?, Task>();//активные обработчики задач
        protected readonly Queue<ICallback> Queue = new Queue<ICallback>();//очередь задач
        protected readonly ManualResetEvent QueueIsEmptyOrNotProcess = new ManualResetEvent(true);//очередь задач пуста или обработка задач еще не началась
        protected readonly ManualResetEvent IsComplit = new ManualResetEvent(true);//нет активных задач
        protected readonly ManualResetEvent AnyComplete = new ManualResetEvent(true);//какаето задача завершилась

        public PoolLite()
        {
        }

        public PoolLite(int parallel)
        {
            Parallel = parallel;
        }

        public bool IsRunning { get; private set; }

        private int _parallelRun = 4;
        /// <summary>
        /// Допустимое число задач выполняемых паралельно
        /// </summary>
        /// <remarks>вы можете выставить кол-во паралельно выполняемых задач в 0,
        /// активные задачи будут завершены, а ожидающие своей очереди задачи не будут запущены,
        /// обработка задач будет считаться завершенной и не нечнется до вызова метода Invoke();
        /// если вновь изменить свойство на отличное от 0 до того того как все активные задачи были завершены,
        /// кол-во паралельно выполняемых задач будут стремиться к указаному;
        /// для того чтобы экстренно прервать выполнение операций - рекомендуется передавать
        /// token отмены и проверять его в обработчике операции</remarks>
        public int Parallel
        {
            get { return _parallelRun; }
            set
            {
                _parallelRun = value;
                if (IsRunning) //если обработка задач уже идет
                    Invoke(); //то может можно добавить еще обработчиков
            }
        }

        //действия при завершении обработчика; запускать только в lock (Access)
        private void StopQueueProcess()
        {
            var taskCurrentId = Task.CurrentId;
            if (taskCurrentId == null)
                throw new Exception("не удалось получить идентификатор текущей задачи");

            Tasks.Remove(taskCurrentId);

            if (Tasks.Count <= 0) //если это был последний активный обработчик
            { IsRunning = false; IsComplit.Set(); }//установить признак очончиния обработки задач и сказать всем что обработка задач закончина
        }

        private void QueueProcess(object first)
        {
            var callback = first as ICallback;
            do
            {
                if (callback != null)
                {
                    try
                    {
                        callback.Invoke(); //начать обработку задачи
                        callback.Continue(); //выполнить продолжение задачи
                    }
                    catch (Exception)
                    {
                         /*ignore*/
                    }
                    

                    lock (Access)
                    {
                        AnyComplete.Set();//оповестить о завершении задачи

                        if (Tasks.Count > Parallel)//если обработчиков задач больше допустипого
                        { StopQueueProcess(); return; }//завершить свою работу
                    }
                }

                lock (Access)
                {
                    if (Queue.Count > 0)//если очередь не пуста
                        callback = Queue.Dequeue();//получить задачу из очереди
                    else //если очередь пуста
                    { StopQueueProcess(); return; }//завершить свою работу

                    if (Queue.Count <= 0)//если забрали последнюю задачу
                        QueueIsEmptyOrNotProcess.Set();//то сообщить о том что все задачи очереди в процеесе выполнения
                }

            } while (callback != null);
        }

        protected void Add(ICallback callback)
        {
            if (callback == null) return;
            lock (Access) Queue.Enqueue(callback);
            if (IsRunning) Invoke();
        }

        public void Add(Action action)
        {
            if (action == null) return;
            Add(new Callback(action));
        }

        public void Invoke()
        {
            lock (Access)
            {
                while ((Tasks.Count < Parallel || Parallel < 0) && Queue.Count > 0)
                {
                    //выставить статусы состояния
                    IsComplit.Reset();
                    IsRunning = true;

                    var task = new Task(QueueProcess, Queue.Dequeue());
                    Tasks.Add(task.Id, task);
                    task.Start();
                }

                //выставить статус очереди
                if (Queue.Count > 0) QueueIsEmptyOrNotProcess.Reset();
                else QueueIsEmptyOrNotProcess.Set();
            }
        }

        /// <summary>
        /// Очистить очередь задач ожидающий выполнения
        /// </summary>
        /// <remarks>активные задачи не будут остановлены</remarks>
        public void Clear()
        {
            lock (Access)
            {
                Queue.Clear();
                QueueIsEmptyOrNotProcess.Set();
            }
        }

        /// <summary>
        /// Ждать завершения всех задач
        /// </summary>
        public void WaitAll()
        {
            IsComplit.WaitOne();
        }

        /// <summary>
        /// Ждать пока все задачи не будут запущены
        /// </summary>
        public void WaitAllRunning()
        {
            QueueIsEmptyOrNotProcess.WaitOne();
        }

        protected void ResetAnyComplete()
        { lock (Access) if (IsRunning) AnyComplete.Reset(); }

        /// <summary>
        /// Ждать завершения любой задачи
        /// </summary>
        public void WaitAny()
        {
            ResetAnyComplete();
            AnyComplete.WaitOne();
        }


        public virtual void Dispose()
        {
            Clear();

            WaitAll();
        }

        public virtual void Dispose(bool wait)
        {
            Clear(); 
            
            if (wait) WaitAll();
        }
    }
}
