using System;
using System.Threading.Tasks;

namespace WorkingTools.Extensions
{
    /// <summary>
    /// Выполнить при условии
    /// </summary>
    public static class Maybe
    {
        /// <summary>
        /// Выполнить если объект != null
        /// </summary>
        /// <typeparam name="TObj">тип объекта</typeparam>
        /// <typeparam name="TRes">тип результата</typeparam>
        /// <param name="obj">объект</param>
        /// <param name="func">функция, результат которой возвращает метод</param>
        /// <param name="defaultRes">возвращаемое значение если объект == null</param>
        /// <returns>возвращает результат работы ф-ии, если переданный объект != null</returns>
        public static TRes Get<TObj, TRes>(this TObj obj, Func<TObj, TRes> func, TRes defaultRes)
        {
            if (Equals(obj, null) || func == null)
                return defaultRes;

            TRes result;
            try { result = func(obj); }
            catch { result = defaultRes; }

            return result;
        }

        /// <summary>
        /// Выполнить если объект != null
        /// </summary>
        /// <typeparam name="TObj">тип объекта</typeparam>
        /// <typeparam name="TRes">тип результата</typeparam>
        /// <param name="obj">объект</param>
        /// <param name="func">функция, результат которой возвращает метод</param>
        /// <param name="continuation">продолжение, выполняется над переданным оъектом после получения значения</param>
        /// <returns>возвращает результат работы ф-ии, если переданный объект != null</returns>
        public static TRes Get<TObj, TRes>(this TObj obj, Func<TObj, TRes> func, Action<TObj> continuation = null)
        {
            if (Equals(obj, null) || func == null)
                return default(TRes);

            TRes result;
            try { result = func(obj); }
            catch { result = default(TRes); }

            if (!Equals(continuation, null))
            {
                try { continuation(obj); }
                catch { /*ignore*/ }
            }

            return result;
        }

        /// <summary>
        /// Выполнить действие, если объект != null
        /// </summary>
        /// <typeparam name="TObj">тип объекта</typeparam>
        /// <param name="obj">объект</param>
        /// <param name="func">выполняемая функция</param>
        /// <returns>возвращает переданный объект</returns>
        public static TObj Do<TObj>(this TObj obj, Action<TObj> func)
        {
            if (Equals(obj, null) || func == null)
                return obj;

            func(obj);

            return obj;
        }

        public static TObj DoAsync<TObj>(this TObj obj, Action<TObj> func)
        {
            Task.Factory.StartNew(() => Do(obj, func));
            return obj;
        }

        public static TRes If<TObj, TRes>(this TObj obj, Func<TObj, bool> when, Func<TObj, TRes> then, Func<TObj, TRes> els = null)
        {
            if (when == null) throw new ArgumentNullException(nameof(when), "условие не можут быть пустым");

            if (when(obj)) return then != null ? then(obj) : default(TRes);
            else return els != null ? els(obj) : default(TRes);
        }
    }
}
