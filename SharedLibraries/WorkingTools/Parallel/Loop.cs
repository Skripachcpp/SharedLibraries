using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkingTools.Parallel
{
    /// <summary>
    /// Вызов делегата с заданным интервалом
    /// </summary>
    /// <remarks>отсчет веремени до повторного вызова ведется с момента окончания работы делегата</remarks>
    public class Loop : IDisposable
    {
        protected readonly object Lock = new object();

        protected readonly ManualResetEvent EventStop = new ManualResetEvent(true);//событие завершения работы

        protected CancellationTokenSource SoftStop;//токен остановки

        protected readonly Action Callback;//исполняемый делегат

        protected bool Started { set; get; }


        private Loop(int seconds)
        {
            Interval = seconds <= 0 ? 60 * 10 : seconds;

            Started = false;
        }

        public Loop(int intervalSeconds, Action сallback)
            : this(intervalSeconds)
        {
            Callback = сallback;
        }


        public int Interval { get; }


        private class BeginArgs
        {
            public BeginArgs(int firstStart, CancellationToken softStopToken)
            {
                if (softStopToken == null) throw new ArgumentNullException(nameof(softStopToken));

                FirstStart = firstStart < 0 ? 0 : firstStart;
                SoftStopToken = softStopToken;
            }

            public int FirstStart { get; private set; }

            public CancellationToken SoftStopToken { get; private set; }
        }

        protected virtual void Begin(object beginArgs)
        {
            if (!(beginArgs is BeginArgs))
                throw new ArgumentOutOfRangeException(nameof(beginArgs), $"ожидается {typeof(BeginArgs)}");

            var args = (BeginArgs)beginArgs;

            var token = args.SoftStopToken;

            var tsFirstStart = new TimeSpan(0, 0, 0, args.FirstStart);
            var tsInterval = new TimeSpan(0, 0, 0, Interval);

            if (!token.IsCancellationRequested && !token.WaitHandle.WaitOne(tsFirstStart))
            {
                Callback.Invoke();
                while (!token.IsCancellationRequested && !token.WaitHandle.WaitOne(tsInterval))
                    Callback.Invoke();
            }
        }

        protected virtual void End()
        {
            lock (Lock)
            {
                Started = false;
            }

            EventStop.Set();
        }

        protected virtual void End(Task task)
        {
            End();
            task.Dispose();
        }

        public virtual void Start(int firstStart = 0, bool async = true)
        {
            CancellationTokenSource softStop;
            lock (Lock)//не запускать если уже запущен
            {
                if (Started) return;
                else Started = true;


                if (SoftStop == null || SoftStop.IsCancellationRequested)
                {
                    if (SoftStop != null)
                        SoftStop.Dispose();

                    SoftStop = new CancellationTokenSource();
                }

                softStop = SoftStop;
            }

            EventStop.Reset();

            var args = new BeginArgs(firstStart, softStop.Token);
            if (async)
            {
                Task.Factory.StartNew(Begin, args).ContinueWith(End);
            }
            else
            {
                Begin(args);
                End();
            }
        }

        public virtual void Stop(bool wait = false)
        {
            CancellationTokenSource softStop;
            lock (Lock)
            {
                if (!Started) return;
                softStop = SoftStop;
            }

            if (softStop != null)
                softStop.Cancel();

            if (wait) Wait();
        }

        public void Wait()
        { EventStop.WaitOne(); }

        /// <summary>
        /// Освободить ресурсы используемые объектом
        /// </summary>
        /// <param name="wait">дождаться окончания освобождения ресурсов объектом</param>
        public void Dispose(bool wait)
        {
            Stop(wait);
            if (SoftStop != null) SoftStop.Dispose();
        }

        public void Dispose()
        { Dispose(true); }
    }
}