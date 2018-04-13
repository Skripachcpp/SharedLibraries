using System;
using System.Net;
using WorkingTools.Factories.FtpWebRequestFacrotyPart;

namespace WorkingTools.Factories.WebRequestFactoryPart
{
    public interface IProxySettings : IServerConnection
    {
        int Port { get; }
    }

    public static class ProxySettingsExtension
    {
        public static IWebProxy ToWebProxy(this IProxySettings proxy)
        {
            if (proxy != null)
            {
                if (string.IsNullOrEmpty(proxy.Server))
                    throw new ArgumentOutOfRangeException(nameof(proxy),
                        "в параметрах подключения к proxy не указан адрес сервера");

                var webProxy = proxy.Port < 0
                    ? new WebProxy(proxy.Server)
                    : new WebProxy(proxy.Server, proxy.Port);

                if (proxy.User != null)
                    webProxy.Credentials = new NetworkCredential(proxy.User.Login, proxy.User.Password);

                return webProxy;
            }

            return null;
        }
    }

    public class ProxySettings : ServerConnection, IProxySettings
    {
        public ProxySettings()
        {
            Port = -1;
        }

        public ProxySettings(int port)
        {
            Port = port;
        }

        public ProxySettings(string server, int port, LoginPass user)
            : base(server, user)
        {
            Port = port;
        }

        public int Port { get; set; }
    }


}