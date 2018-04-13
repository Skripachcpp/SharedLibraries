using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WorkingTools
{
    public static class XmlCacheFactory
    {
        #region Variables

        private static readonly Dictionary<Type, XmlSerializer> CacheXmlSerializers = new Dictionary<Type, XmlSerializer>();

        #endregion Variables


        #region

        /// <summary>
        /// Возвращает XmlSerializer из кеша или создает и сохраняет в кеш
        /// </summary>
        /// <param name="typeObj"></param>
        /// <returns></returns>
        public static XmlSerializer XmlSerializer(Type typeObj)
        {
            lock (CacheXmlSerializers)
            {
                if (CacheXmlSerializers.ContainsKey(typeObj))
                    return CacheXmlSerializers[typeObj];

                //ошибка создания XmlSerializer чаще всего 
                //происходит из-за отсутствия конструктора без 
                //параметров у класса, тип которого был передан
                //конструктору XmlSerializer

                //но возможны и другие причины

                var newXmlSerializer = new XmlSerializer(typeObj);
                CacheXmlSerializers.Add(typeObj, newXmlSerializer);

                if (CreateXmlSerializer != null)//отчитаться о создании, если это кому то нужно
                    OnCreateXmlSerializer(new EventCreateXmlSerializerArgs(typeObj));

                return newXmlSerializer;
            }
        }

        #endregion


        #region Events

        public delegate void EventCreateXmlSerializer(EventCreateXmlSerializerArgs e);

        private static void OnCreateXmlSerializer(EventCreateXmlSerializerArgs e)
        { CreateXmlSerializer?.Invoke(e); }

        /// <summary>
        /// При создании нового XmlSerializer
        /// </summary>
        public static event EventCreateXmlSerializer CreateXmlSerializer;

        #endregion Events
    }

    public class EventCreateXmlSerializerArgs
    {
        public EventCreateXmlSerializerArgs(Type type)
        {
            Type = type;
        }

        public Type Type { get; protected set; }

        public string ToMessage()
        {
            return string.Format("XmlSerializer(type: [{0}])", Type);
        }
    }
}
