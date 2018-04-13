using System.IO;
using System.Net;
using WorkingTools.Classes.FtpProviderPart;
using WorkingTools.Extensions;
using WorkingTools.Factories.FtpWebRequestFacrotyPart;
using WorkingTools.Factories.WebRequestFactoryPart;

namespace WorkingTools.Classes
{
    public partial class FtpProvider : FtpProviderLite
    {
        public FtpProvider(string ftpServerName, ICredentials ftpServerCredentials = null, IWebProxy proxy = null, bool ftpServerUsePassive = false)
            : base(ftpServerName, ftpServerCredentials, proxy, ftpServerUsePassive)
        {
        }

        private const string ErrorMessage = "отсутствуют параметры подключения к ftp серверу";
        public FtpProvider(IFtpServerConnection server, IWebProxy proxy = null)
            : this(server.ThrowIfNull(ErrorMessage).Server, server.ThrowIfNull(ErrorMessage).User.ToCredentials(),
            proxy, server.ThrowIfNull(ErrorMessage).UsePassive)
        {
        }

        public FtpProvider(IFtpServerConnection server, IProxySettings proxy = null)
            : this(server, proxy.ToWebProxy())
        {
        }

        public void UploadFile(string localFilePath, string ftpFilePath)
        {
            CheckLocalPath(localFilePath);
            CheckFtpPath(ftpFilePath);

            using (var fileStream = File.Open(localFilePath, FileMode.Open))
                UploadFile(fileStream, ftpFilePath);
        }

        /// <summary>
        /// Загрузить файл
        /// </summary>
        /// <remarks>если файл существует, он будет перезаписан</remarks>
        public void DownloadFile(string ftpFilePath, string localFilePath)
        {
            CheckLocalPath(localFilePath);
            CheckFtpPath(ftpFilePath);

            using (FileStream fileStream = File.Create(localFilePath))
                DownloadFile(ftpFilePath, fileStream);
        }
    }
}
