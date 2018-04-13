using System;
using System.Net;
using WorkingTools.Classes.FtpProviderPart;
using WorkingTools.Factories.FtpWebRequestFacrotyPart;
using WorkingTools.Factories.WebRequestFactoryPart;

namespace WorkingTools.Factories
{
    public static class FtpWebRequestFacroty
    {
        public static FtpWebRequest Create(string ftpPath, FtpRequestType ftpRequestMethod, IFtpServerConnection ftpServerConnection, IProxySettings proxy)
        { return Create(ftpPath, ftpRequestMethod, ftpServerConnection, proxy.ToWebProxy()); }

        private static string ToUrl(string ftpServerName, string ftpPath)
        {
            if (string.IsNullOrWhiteSpace(ftpServerName)) throw new ArgumentOutOfRangeException(nameof(ftpServerName), "имя ftp сервера отсутствует или является пустым");
            if (string.IsNullOrWhiteSpace(ftpPath)) throw new ArgumentOutOfRangeException(nameof(ftpPath), "путь до файла на на ftp отсутствует или является пустым");

            var url = string.Format(@"ftp://{0}", string.Join(@"/", ftpServerName, ftpPath));
            return url;
        }

        public static FtpWebRequest Create(string ftpPath, FtpRequestType ftpRequestMethod, IFtpServerConnection ftpServerConnection, IWebProxy proxy)
        {
            if (ftpServerConnection == null) throw new ArgumentNullException(nameof(ftpServerConnection), "отсутствуют параметры подключения к ftp");
            if (string.IsNullOrWhiteSpace(ftpServerConnection.Server)) throw new ArgumentOutOfRangeException(nameof(ftpServerConnection), "в параметрах подключения к ftp серверу отсутствует адрес сервера");

            var request = Create(ftpPath, ftpRequestMethod, ftpServerConnection.Server, ftpServerConnection.User.ToCredentials(), ftpServerConnection.UsePassive, proxy);
            return request;
        }

        public static FtpWebRequest Create(string ftpPath, FtpRequestType ftpRequestMethod,
            string ftpServerName, ICredentials ftpServerCredentials, bool ftpServerUsePassive,
            IWebProxy proxy)
        {
            if (ftpServerName == null) throw new ArgumentNullException(nameof(ftpServerName));
            if (string.IsNullOrWhiteSpace(ftpServerName)) throw new ArgumentOutOfRangeException(nameof(ftpServerName), "в параметрах подключения к ftp серверу отсутствует адрес сервера");

            var url = ToUrl(ftpServerName, ftpPath);
            var request = (FtpWebRequest)WebRequestFactory.Create(url, ftpRequestMethod.ToFtpWebRequestMethod(), ftpServerCredentials, proxy);
            request.UsePassive = ftpServerUsePassive;
            return request;
        }
    }
}
