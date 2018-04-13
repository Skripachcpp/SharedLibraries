using System;
using System.Linq;

namespace WorkingTools.Extensions
{
    public static class TypeExtension
    {
        public static bool Inherited(this Type type, Type isType)
        {
            var inherited = type.GetInterfaces().Any(x =>
                  x.IsGenericType &&
                  x.GetGenericTypeDefinition() == isType);

            return inherited;
        }


        /// <summary>
        /// Создать экземпляр объекта
        /// </summary>
        /// <param name="type">тип объекта</param>
        /// <param name="param">аргументы конструктора</param>
        /// <returns></returns>
        public static object Create(this Type type, params object[] param)
        {
            return Activator.CreateInstance(type, param);
        }

        /// <summary>
        /// Создать экземпляр объекта
        /// </summary>
        /// <param name="type">тип объекта</param>
        /// <param name="param">аргументы конструктора</param>
        /// <returns></returns>
        public static TRes Create<TRes>(this Type type, params object[] param)
        {
            return (TRes)Activator.CreateInstance(type, param);
        }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        /// <param name="type">тип объекта</param>
        /// <returns>возвращает значение по умолчанию для элементарных типов и структур и null для ссылочных типов</returns>
        public static object Default(this Type type)
        {
            return type.IsValueType ? type.Create() : null;
        }
    }
}
