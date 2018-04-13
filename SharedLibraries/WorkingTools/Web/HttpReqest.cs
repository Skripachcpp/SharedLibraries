using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WorkingTools.Extensions;
using WorkingTools.Extensions.Json;

namespace WorkingTools.Web
{
    public static class HttpReqest
    {
        private const string CONTENTTYPE_JSON = "application/json";

        private static async Task<string> ReadAsStringAsync(HttpWebRequest req)
        {
            var res = (HttpWebResponse)(await req.GetResponseAsync());
            var responseString = ReadAsString(res);
            return responseString;
        }

        private static string ReadAsString(HttpWebRequest req)
        {
            var res = (HttpWebResponse)req.GetResponse();
            var responseString = ReadAsString(res);
            return responseString;
        }

        private static string ReadAsString(HttpWebResponse res)
        {
            string responseString;
            using (var receiveStream = res.GetResponseStream())
            {
                if (receiveStream != null)
                    using (var streamReader = new StreamReader(receiveStream))
                        responseString = streamReader.ReadToEnd();
                else responseString = null;
            }

            return responseString;
        }

        public static HttpWebRequest CreateHttpWebRequest(
            string method,
            string url,
            string contentType = null,

            Dictionary<string, string> headers = null,

            IWebProxy webProxy = null,
            CookieContainer cookieContainer = null,
            bool webProxyNone = false,
            bool contentTypeAddCharsetUtf8 = false)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);

            if (webProxyNone) req.Proxy = null;
            else if (webProxy == null) req.Proxy.Credentials = CredentialCache.DefaultCredentials;
            else req.Proxy = webProxy;

            req.Method = method;
            req.Timeout = 100000;

            if (contentType != null && contentTypeAddCharsetUtf8) contentType = contentType + ";charset=UTF-8";

            req.ContentType = contentType;

            req.CookieContainer = cookieContainer ?? new CookieContainer();

            if (headers != null) 
                foreach (var header in headers) 
                    req.Headers.Add(header.Key, header.Value);

            return req;
        }

        public static HttpWebRequest CreateHttpWebRequest(
            string method,
            string url,
            Dictionary<string, string> prms,
            string contentType = null,
            bool contentTypeAddCharsetUtf8 = false)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            url = url + str.JoinTitle("?", str.Join("&", prms?.Select(a => $"{a.Key}={a.Value}")));
            var req = CreateHttpWebRequest(method, url, contentType: contentType, contentTypeAddCharsetUtf8: contentTypeAddCharsetUtf8);
            return req;
        }

        public static T Put<T>(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            bool ignoreEmpty = true)
        {
            var response = Put(
                url,
                p0, v0,
                p1, v1,
                p2, v2,
                p3, v3,
                p4, v4,
                p5, v5,
                p6, v6,
                p7, v7,
                p8, v8,
                p9, v9,
                ignoreEmpty);
            if (string.IsNullOrWhiteSpace(response)) return default(T);
            return response.FromJson<T>();
        }

        public static string Put(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            bool ignoreEmpty = true)
        {
            Dictionary<string, string> prms = null;
            if (p0 != null)
            {
                prms = new Dictionary<string, string>();
                AddPrm(prms, p0, v0, ignoreEmpty);
                AddPrm(prms, p1, v1, ignoreEmpty);
                AddPrm(prms, p2, v2, ignoreEmpty);
                AddPrm(prms, p3, v3, ignoreEmpty);
                AddPrm(prms, p4, v4, ignoreEmpty);
                AddPrm(prms, p5, v5, ignoreEmpty);
                AddPrm(prms, p6, v6, ignoreEmpty);
                AddPrm(prms, p7, v7, ignoreEmpty);
                AddPrm(prms, p8, v8, ignoreEmpty);
                AddPrm(prms, p9, v9, ignoreEmpty);
                if (prms.Count <= 0) prms = null;
            }

            return Put(url, prms);
        }

        public static string Put(string url, Dictionary<string, string> prms)
        {
            var responseString = ReadAsString(CreateHttpWebRequest("PUT", url, prms));
            return responseString;
        }

        public static T Post<T>(string url, string data, string contentType = CONTENTTYPE_JSON, bool utf8 = false, Dictionary<string, string> headers = null)
            => Post(url, data, contentType, utf8, headers).FromJson<T>();

        public static string Post(string url, string data, string contentType = CONTENTTYPE_JSON, bool utf8 = false, Dictionary<string, string> headers = null)
        {
            var req = CreateHttpWebRequest("POST", url, headers: headers, contentType: contentType, contentTypeAddCharsetUtf8: utf8);

            byte[] sentData = Encoding.UTF8.GetBytes(data);
            req.ContentLength = sentData.Length;

            var sendStream = req.GetRequestStream();

            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();

            var responseString = ReadAsString(req);
            return responseString;
        }

        public static T Get<T>(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            string contentType = null,
            bool ignoreEmpty = true,
            bool utf8 = false)
        {
            var response = Get(
                url,
                p0, v0,
                p1, v1,
                p2, v2,
                p3, v3,
                p4, v4,
                p5, v5,
                p6, v6,
                p7, v7,
                p8, v8,
                p9, v9,
                contentType,
                ignoreEmpty,
                utf8);
            if (string.IsNullOrWhiteSpace(response)) return default(T);
            return response.FromJson<T>();
        }

        private static void AddPrm(Dictionary<string, string> prms, string p, object v, bool ignoreEmpty)
        {
            if (p != null)
            {
                var sv = v?.ToString();
                if (!ignoreEmpty || !string.IsNullOrEmpty(sv))
                {
                    prms.AddOrUpdate(p, sv);
                }
            }
        }

        public static string Get(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            string contentType = null,
            bool ignoreEmpty = true,
            bool utf8 = false)
        {
            Dictionary<string, string> prms = null;
            if (p0 != null)
            {
                prms = new Dictionary<string, string>();
                AddPrm(prms, p0, v0, ignoreEmpty);
                AddPrm(prms, p1, v1, ignoreEmpty);
                AddPrm(prms, p2, v2, ignoreEmpty);
                AddPrm(prms, p3, v3, ignoreEmpty);
                AddPrm(prms, p4, v4, ignoreEmpty);
                AddPrm(prms, p5, v5, ignoreEmpty);
                AddPrm(prms, p6, v6, ignoreEmpty);
                AddPrm(prms, p7, v7, ignoreEmpty);
                AddPrm(prms, p8, v8, ignoreEmpty);
                AddPrm(prms, p9, v9, ignoreEmpty);
                if (prms.Count <= 0) prms = null;
            }

            return Get(url, prms, null, utf8);
        }

        public static string Get(string url, Dictionary<string, string> prms, string contentType, bool contentTypeAddCharsetUtf8 = false)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            url = url + str.JoinTitle("?", str.Join("&", prms?.Select(a => $"{a.Key}={a.Value}")));

            var req = CreateHttpWebRequest("GET", url, contentType: contentType, contentTypeAddCharsetUtf8: contentTypeAddCharsetUtf8);

            var responseString = ReadAsString(req);
            return responseString;
        }

        #region async

        public static async Task<string> PutAsync(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            bool ignoreEmpty = true)
        {
            Dictionary<string, string> prms = null;
            if (p0 != null)
            {
                prms = new Dictionary<string, string>();
                AddPrm(prms, p0, v0, ignoreEmpty);
                AddPrm(prms, p1, v1, ignoreEmpty);
                AddPrm(prms, p2, v2, ignoreEmpty);
                AddPrm(prms, p3, v3, ignoreEmpty);
                AddPrm(prms, p4, v4, ignoreEmpty);
                AddPrm(prms, p5, v5, ignoreEmpty);
                AddPrm(prms, p6, v6, ignoreEmpty);
                AddPrm(prms, p7, v7, ignoreEmpty);
                AddPrm(prms, p8, v8, ignoreEmpty);
                AddPrm(prms, p9, v9, ignoreEmpty);
                if (prms.Count <= 0) prms = null;
            }

            return await PutAsync(url, prms);
        }

        public static async Task<string> PutAsync(string url, Dictionary<string, string> prms)
        {
            var responseString = await ReadAsStringAsync(CreateHttpWebRequest("PUT", url, prms));
            return responseString;
        }

        public static async Task<T> PostAsync<T>(string url, string data, string contentType = CONTENTTYPE_JSON, bool utf8 = false, Dictionary<string, string> headers = null)
            => (await PostAsync(url, data, contentType, utf8, headers)).FromJson<T>();

        public static async Task<string> PostAsync(string url, string data, string contentType = CONTENTTYPE_JSON, bool utf8 = false, Dictionary<string, string> headers = null)
        {
            var req = CreateHttpWebRequest("POST", url, headers: headers, contentType: contentType, contentTypeAddCharsetUtf8: utf8);

            byte[] sentData = Encoding.UTF8.GetBytes(data);
            req.ContentLength = sentData.Length;

            var sendStream = await req.GetRequestStreamAsync();

            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();

            var responseString = await ReadAsStringAsync(req);
            return responseString;
        }

        public static async Task<T> GetAsync<T>(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            string contentType = null,
            bool ignoreEmpty = true,
            bool utf8 = false)
        {
            var response = await GetAsync(
                url,
                p0, v0,
                p1, v1,
                p2, v2,
                p3, v3,
                p4, v4,
                p5, v5,
                p6, v6,
                p7, v7,
                p8, v8,
                p9, v9,
                contentType,
                ignoreEmpty,
                utf8);
            if (string.IsNullOrWhiteSpace(response)) return default(T);
            return response.FromJson<T>();
        }

        public static async Task<string> GetAsync(string url,
            string p0 = null, object v0 = null,
            string p1 = null, object v1 = null,
            string p2 = null, object v2 = null,
            string p3 = null, object v3 = null,
            string p4 = null, object v4 = null,
            string p5 = null, object v5 = null,
            string p6 = null, object v6 = null,
            string p7 = null, object v7 = null,
            string p8 = null, object v8 = null,
            string p9 = null, object v9 = null,
            string contentType = null,
            bool ignoreEmpty = true,
            bool utf8 = false)
        {
            Dictionary<string, string> prms = null;
            if (p0 != null)
            {
                prms = new Dictionary<string, string>();
                AddPrm(prms, p0, v0, ignoreEmpty);
                AddPrm(prms, p1, v1, ignoreEmpty);
                AddPrm(prms, p2, v2, ignoreEmpty);
                AddPrm(prms, p3, v3, ignoreEmpty);
                AddPrm(prms, p4, v4, ignoreEmpty);
                AddPrm(prms, p5, v5, ignoreEmpty);
                AddPrm(prms, p6, v6, ignoreEmpty);
                AddPrm(prms, p7, v7, ignoreEmpty);
                AddPrm(prms, p8, v8, ignoreEmpty);
                AddPrm(prms, p9, v9, ignoreEmpty);
                if (prms.Count <= 0) prms = null;
            }

            return await GetAsync(url, prms, null, utf8);
        }

        public static async Task<string> GetAsync(string url, Dictionary<string, string> prms, string contentType, bool contentTypeAddCharsetUtf8 = false)
        {   
            if (url == null) throw new ArgumentNullException(nameof(url));
            url = url + str.JoinTitle("?", str.Join("&", prms?.Select(a => $"{a.Key}={a.Value}")));

            var req = CreateHttpWebRequest("GET", url, contentType: contentType, contentTypeAddCharsetUtf8: contentTypeAddCharsetUtf8);

            var responseString = await ReadAsStringAsync(req);
            return responseString;
        }
        
        #endregion
    }
}