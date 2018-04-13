using System;
using System.Net;
using WorkingTools.Factories.WebRequestFactoryPart;

namespace WorkingTools.Factories
{
    public static class WebRequestFactory
    {
        public static WebRequest Create(string url, string method)
        { return Create(url, method, (ICredentials)null, (IWebProxy)null); }

        public static WebRequest Create(string url, string method, ILoginPass user)
        { return Create(url, method, user, (IWebProxy)null); }

        public static WebRequest Create(string url, string method, IProxySettings proxy)
        { return Create(url, method, null, proxy); }

        public static WebRequest Create(string url, string method, IWebProxy proxy)
        { return Create(url, method, (ICredentials)null, proxy); }

        public static WebRequest Create(string url, string method, ILoginPass credentials, IProxySettings proxy)
        { return Create(url, method, credentials, proxy.ToWebProxy()); }

        public static WebRequest Create(string url, string method, ILoginPass credentials, IWebProxy proxy)
        { return Create(url, method, credentials.ToCredentials(), proxy); }

        public static WebRequest Create(string url, string method, ICredentials credentials, IWebProxy proxy)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url), "не указан адрес запроса");
            if (string.IsNullOrEmpty(method)) throw new ArgumentNullException(nameof(method), "не указан метод (тип) запроса");
            if (credentials == null) credentials = CredentialCache.DefaultCredentials;

            var request = WebRequest.Create(url);
            request.Method = method;
            request.Credentials = credentials;
            request.Proxy = proxy;

            return request;
        }
    }
}
