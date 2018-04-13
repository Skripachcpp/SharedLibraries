using System;
using System.IO;
using System.Net;
using WorkingTools.Extensions;
using WorkingTools.Factories;

namespace WorkingTools.Classes.FtpProviderPart
{
    public class FtpProviderLite
    {
        private const int BufferSize = 1024;

        private readonly string _ftpServerName;
        private readonly ICredentials _ftpServerCredentials;
        private readonly IWebProxy _proxy;
        private readonly bool _ftpServerUsePassive;

        public FtpProviderLite(string ftpServerName, ICredentials ftpServerCredentials = null, IWebProxy proxy = null, bool ftpServerUsePassive = false)
        {
            if (string.IsNullOrWhiteSpace(ftpServerName)) throw new ArgumentOutOfRangeException(nameof(ftpServerName), "имя ftp сервера отсутствует или является пустым");

            _ftpServerName = ftpServerName;
            _ftpServerCredentials = ftpServerCredentials;
            _proxy = proxy;
            _ftpServerUsePassive = ftpServerUsePassive;
        }

        #region Validate

        protected void CheckFtpPath(string ftpFilePath)
        { if (string.IsNullOrWhiteSpace(ftpFilePath)) throw new ArgumentOutOfRangeException(nameof(ftpFilePath), "путь до файла на ftp отсутствует или является пустым"); }

        protected void CheckLocalPath(string localFilePath)
        { if (string.IsNullOrWhiteSpace(localFilePath)) throw new ArgumentOutOfRangeException(nameof(localFilePath), "путь до файла отсутствует или является пустым"); }

        #endregion Validate

        public void UploadFile(Stream fileStream, string ftpFilePath)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream), "отсутствует поток выгружаемого файла");
            CheckFtpPath(ftpFilePath);

            var request = FtpWebRequestFacroty.Create(ftpFilePath, FtpRequestType.UploadFile, _ftpServerName, _ftpServerCredentials, _ftpServerUsePassive, _proxy);
            Stream requestStream = request.GetRequestStream();

            request.ContentLength = fileStream.Length;

            var buffer = new byte[BufferSize];
            int bytesread;
            do
            {
                bytesread = fileStream.Read(buffer, 0, buffer.Length);
                requestStream.Write(buffer, 0, bytesread);
            } while (bytesread > 0);

            fileStream.Close();

            requestStream.Close();

            var response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }


        /// <summary>
        /// Загрузить файл
        /// </summary>
        /// <param name="ftpFilePath">путь до файла на ftp</param>
        /// <param name="outStream">поток в которой будет идти запись; после с завершения загрузки поток не закрывается</param>
        public void DownloadFile(string ftpFilePath, Stream outStream)
        {
            CheckFtpPath(ftpFilePath);
            if (outStream == null) throw new ArgumentNullException(nameof(outStream), "поток для записи загружаемого файла отсутствует");

            var request = FtpWebRequestFacroty.Create(ftpFilePath, FtpRequestType.DownloadFile, _ftpServerName, _ftpServerCredentials, _ftpServerUsePassive, _proxy);

            using (var response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream().ThrowIfNull("не удалось получить поток для передачи файла"))
            {
                var buffer = new byte[BufferSize];
                int bytesread;
                do
                {
                    bytesread = responseStream.Read(buffer, 0, buffer.Length);
                    outStream.Write(buffer, 0, bytesread);
                } while (bytesread > 0);
            }
        }

        /// <summary>
        /// Загрузить файл
        /// </summary>
        /// <param name="ftpFilePath">путь до файла на ftp</param>
        /// <param name="processingStreamFunc">функция обработки потока</param>
        /// <returns>возвращает результат работы функции обработки потока скачивания</returns>
        public TRes DownloadFile<TRes>(string ftpFilePath, Func<Stream, TRes> processingStreamFunc)
        {
            CheckFtpPath(ftpFilePath);
            if (processingStreamFunc == null) throw new ArgumentNullException(nameof(processingStreamFunc), "функция обработки загружаемого файла отсутствует");

            var request = FtpWebRequestFacroty.Create(ftpFilePath, FtpRequestType.DownloadFile, _ftpServerName, _ftpServerCredentials, _ftpServerUsePassive, _proxy);

            using (var response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream().ThrowIfNull("не удалось получить поток для передачи файла"))
                return processingStreamFunc(responseStream);
        }
    }
}
