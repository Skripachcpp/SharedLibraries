using System;
using System.Threading.Tasks;

namespace WorkingTools.Parallel
{
    public static class Async
    {
        public static void Go(ICallback callback, ICallback continueWith = null)
        {
            if (callback != null)
            {
                var task = new Task(callback.Invoke);

                task.ContinueWith(t =>
                {
                    if (continueWith != null) continueWith.Invoke();
                    if (t != null) t.Dispose();
                });

                task.Start();
            }
        }

        public static void Go(Action action)
        { Go(new Callback(action)); }

        #region p

        public static void Go<T>(Action<T> action, T p1)
        { Go(new Callback<T>(action, p1)); }

        public static void Go<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2)
        { Go(new Callback<T1, T2>(action, p1, p2)); }

        public static void Go<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3)
        { Go(new Callback<T1, T2, T3>(action, p1, p2, p3)); }

        //public static void Go<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4)
        //{ Go(new Callback<T1, T2, T3, T4>(action, p1, p2, p3, p4)); }

        //public static void Go<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        //{ Go(new Callback<T1, T2, T3, T4, T5>(action, p1, p2, p3, p4, p5)); }

        #endregion p

        #region ICallback continueWith

        public static void Go(Action action, ICallback continueWith)
        { Go(new Callback(action), continueWith); }

        public static void Go<T>(Action<T> action, T p1, ICallback continueWith)
        { Go(new Callback<T>(action, p1), continueWith); }

        public static void Go<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2, ICallback continueWith)
        { Go(new Callback<T1, T2>(action, p1, p2), continueWith); }

        public static void Go<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3, ICallback continueWith)
        { Go(new Callback<T1, T2, T3>(action, p1, p2, p3), continueWith); }

        //public static void Go<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4, ICallback continueWith)
        //{ Go(new Callback<T1, T2, T3, T4>(action, p1, p2, p3, p4), continueWith); }

        //public static void Go<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, ICallback continueWith)
        //{ Go(new Callback<T1, T2, T3, T4, T5>(action, p1, p2, p3, p4, p5), continueWith); }

        #endregion ICallback continueWith

        #region Action continueWith

        public static void Go(Action action, Action continueWith)
        { Go(new Callback(action), new Callback(continueWith)); }

        public static void Go<T>(Action<T> action, T p1, Action continueWith)
        { Go(new Callback<T>(action, p1), new Callback(continueWith)); }

        public static void Go<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2, Action continueWith)
        { Go(new Callback<T1, T2>(action, p1, p2), new Callback(continueWith)); }

        public static void Go<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3, Action continueWith)
        { Go(new Callback<T1, T2, T3>(action, p1, p2, p3), new Callback(continueWith)); }

        //public static void Go<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4, Action continueWith)
        //{ Go(new Callback<T1, T2, T3, T4>(action, p1, p2, p3, p4), new Callback(continueWith)); }

        //public static void Go<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, Action continueWith)
        //{ Go(new Callback<T1, T2, T3, T4, T5>(action, p1, p2, p3, p4, p5), new Callback(continueWith)); }

        #endregion Action continueWith
    }
}
