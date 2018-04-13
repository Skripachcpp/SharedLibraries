using System;

namespace WorkingTools.Parallel
{
    public class Pool : PoolLite
    {
        public Pool()
            : base()
        {
        }

        public Pool(int parallel)
            : base(parallel)
        {
        }

        public void Add<T>(Action<T> action, T p1, Action<T> continuation = null)
        {
            if (action == null) return;
            Add(new Callback<T>(action, p1));
        }

        public void Add<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2, Action<T1, T2> continuation = null)
        {
            if (action == null) return;
            Add(new Callback<T1, T2>(action, p1, p2));
        }

        public void Add<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3, Action<T1, T2, T3> continuation = null)
        {
            if (action == null) return;
            Add(new Callback<T1, T2, T3>(action, p1, p2, p3));
        }

        public void Add<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4, Action<T1, T2, T3, T4> continuation = null)
        {
            if (action == null) return;
            Add(new Callback<T1, T2, T3, T4>(action, p1, p2, p3, p4));
        }

        public void Add<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, Action<T1, T2, T3, T4, T5> continuation = null)
        {
            if (action == null) return;
            Add(new Callback<T1, T2, T3, T4, T5>(action, p1, p2, p3, p4, p5));
        }

        public void Invoke(Action action, Action continuation = null)
        {
            Add(action);
            Invoke();
        }

        public void Invoke<T>(Action<T> action, T p1, Action<T> continuation = null)
        {
            Add(action, p1);
            Invoke();
        }

        public void Invoke<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2, Action<T1, T2> continuation = null)
        {
            Add(action, p1, p2);
            Invoke();
        }

        public void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3, Action<T1, T2, T3> continuation = null)
        {
            Add(action, p1, p2, p3);
            Invoke();
        }

        public void Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4, Action<T1, T2, T3, T4> continuation = null)
        {
            Add(action, p1, p2, p3, p4);
            Invoke();
        }

        public void Invoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, Action<T1, T2, T3, T4, T5> continuation = null)
        {
            Add(action, p1, p2, p3, p4, p5);
            Invoke();
        }


        /// <summary>
        /// Ждать завершения всех задач
        /// </summary>
        /// <param name="millisecondsTimeout">максимальный период ожидания</param>
        /// <returns>true если задачи завершились до окончиния периода ожидания</returns>
        public bool WaitAll(int millisecondsTimeout)
        {
            return IsComplit.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        /// Ждать пока все задачи не будут запущены
        /// </summary>
        /// <param name="millisecondsTimeout">максимальный период ожидания</param>
        /// <returns>true если задачи завершились до окончиния периода ожидания</returns>
        public bool WaitAllRunning(int millisecondsTimeout)
        {
            return QueueIsEmptyOrNotProcess.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        /// Ждать завершения любой задачи
        /// </summary>
        /// <param name="millisecondsTimeout">максимальный период ожидания</param>
        /// <returns>true если задачи завершились до окончиния периода ожидания</returns>
        public bool WaitAny(int millisecondsTimeout)
        {
            ResetAnyComplete();
            return AnyComplete.WaitOne(millisecondsTimeout);
        }

        public virtual bool Dispose(bool wait, int millisecondsTimeout)
        {
            Clear();
            return !wait || WaitAll(millisecondsTimeout);
        }
    }
}
