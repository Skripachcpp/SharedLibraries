using System;

namespace WorkingTools.Static
{
    /// <summary>
    /// —татичный класс дл€ вызова методов api в зависимости от версии
    /// </summary>
    public static class caller
    {
        /// <summary>
        /// ¬ызов метода api в зависимости от версии
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="version">верси€</param>
        /// <param name="funcs">набор методов</param>
        /// <returns></returns>
        public static T Call<T>(int version, params Func<T>[] funcs)
        {
            if (funcs == null) throw new ArgumentNullException(nameof(funcs));
            if (funcs.Length <= 0) throw new ArgumentOutOfRangeException(nameof(funcs), @"отсутствуют обработчики");
            if (version <= 0) throw new Exception("не указана верси€ метода");
            version--;
            if (funcs.Length < version) throw new ArgumentOutOfRangeException(nameof(funcs), $@"отсутствуют отсутствует обработчик дл€ версии {version}");

            return funcs[version]();
        }
    }
}