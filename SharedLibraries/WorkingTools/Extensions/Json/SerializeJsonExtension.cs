using System;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace WorkingTools.Extensions.Json
{
    public static class JsonExtension
    {
        private static JsonSerializerSettings _jsonSerializerSettingsHiddenNull;
        public static string ToJson<TObj>(this TObj obj, bool hiddenNull = true, bool indented = false, bool useapostrophe = false)
        {
            if (obj == null) return null;

            var json = JsonConvert.SerializeObject(obj,
                settings: (hiddenNull ? _jsonSerializerSettingsHiddenNull ?? (_jsonSerializerSettingsHiddenNull = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) : null),
                formatting: indented ? Formatting.Indented : Formatting.None);

            if (useapostrophe) json = json.Replace("\"", "'");

            return json;
        }

        public static object FromJson(this string json)
        {
            if (json == null) return null;
            return JsonConvert.DeserializeObject(json);
        }

        public static TObj FromJson<TObj>(this string json, bool throwEx = true)
        {
            if (json == null) return default(TObj);

            try { return JsonConvert.DeserializeObject<TObj>(json); }
            catch { if (throwEx) throw; else { return default(TObj); } }
        }

        public static Stream ToJson<TObj>(this TObj obj, Stream stream)
        {
            if (obj == null) return stream;

            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                var ser = new JsonSerializer();
                ser.Serialize(jsonWriter, obj);
                jsonWriter.Flush();
            }

            return stream;
        }

        public static TObj FromJson<TObj>(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var ser = new JsonSerializer();
                return ser.Deserialize<TObj>(jsonReader);
            }
        }
    }
}
