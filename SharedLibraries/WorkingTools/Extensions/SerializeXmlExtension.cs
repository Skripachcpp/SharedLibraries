using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WorkingTools.Extensions
{
    public enum SerializeTo
    {
        MemoryStream, String
    }

    public static class SerializeXmlExtension
    {
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
        public static readonly Encoding DefaultWithoutBomEncoding = new UTF8Encoding(false);

        public static MemoryStream ToXmlMemoryStream<TObj>(this TObj obj, Encoding encoding = null, bool indent = true, bool omitXmlDeclaration = false) //where TObj : class
        {
            var type = typeof(TObj);

            var stream = new MemoryStream();
            var startingPosition = stream.Position;


            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = indent,//следует ли использовать отступ для элементов
                Encoding = (encoding ?? DefaultEncoding),
                OmitXmlDeclaration = omitXmlDeclaration//следует ли опустить XML-объявление
            };

            using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
            {
                //пустое пространство имен при сериализации
                var xmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });

                XmlCacheFactory.XmlSerializer(type).Serialize(xmlWriter, obj, xmlSerializerNamespaces);
                xmlWriter.Close();
            }

            stream.Position = startingPosition;
            return stream;
        }

        public static object ToXml<TObj>(this TObj obj, SerializeTo serializeTo, Encoding encoding = null, bool? indent = true, bool? omitXmlDeclaration = null)
        {
            switch (serializeTo)
            {
                case SerializeTo.MemoryStream:
                    return ToXmlMemoryStream(obj, encoding, indent ?? true, omitXmlDeclaration ?? false);
                case SerializeTo.String:
                    using (var memoryStream = ToXmlMemoryStream(obj, encoding, indent ?? true, omitXmlDeclaration ?? true))
                        return (encoding ?? DefaultEncoding).GetString(memoryStream.ToArray());
                default: throw new NotImplementedException(string.Format("отсутствует реализация сериализации в {0}", serializeTo));
            }
        }

        public static string ToXml<TObj>(this TObj obj, Encoding encoding = null, bool? indent = true, bool? omitXmlDeclaration = null)
        {
            return (string)ToXml(obj, SerializeTo.String, encoding, indent, omitXmlDeclaration);
        }

        public static string ToXmlString<TObj>(this TObj obj, Encoding encoding = null, bool? indent = true, bool? omitXmlDeclaration = null)
        {
            return (string)ToXml(obj, SerializeTo.String, encoding, indent, omitXmlDeclaration);
        }

        public static object FromXml(this XmlReader source, Type type)
        { return XmlCacheFactory.XmlSerializer(type).Deserialize(source); }

        public static TRes FromXml<TRes>(this XmlReader source) //where TRes : class
        { return (TRes)FromXml(source, typeof(TRes)); }


        public static object FromXml(this string source, Type type, Encoding encoding = null)
        {
            using (Stream stream = new MemoryStream((encoding ?? DefaultEncoding).GetBytes(source)))
                return FromXml(stream, type);
        }

        public static TRes FromXml<TRes>(this string source, Encoding encoding = null) //where TRes : class
        { return (TRes)FromXml(source, typeof(TRes), encoding); }



        public static object FromXml(this Stream source, Type type)
        { return XmlCacheFactory.XmlSerializer(type).Deserialize(source); }

        public static TRes FromXml<TRes>(this Stream source) //where TRes : class
        { return (TRes)FromXml(source, typeof(TRes)); }
    }
}
