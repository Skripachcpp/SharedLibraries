using System;
using System.Net;

namespace WorkingTools.Classes.FtpProviderPart
{
    public enum FtpRequestType
    {
        /// <summary>
        /// Представляет метод протокола FTP APPE, который используется для присоединения файла к существующему файлу на FTP-сервере.
        /// </summary>
        DownloadFile,
        /// <summary>
        /// Представляет метод протокола FTP DELE, который используется для удаления файла на FTP-сервере.
        /// </summary>
        ListDirectory,
        /// <summary>
        /// Представляет метод протокола FTP RETR, который используется для загрузки файла с FTP-сервера.
        /// </summary>
        UploadFile,
        /// <summary>
        /// Представляет метод протокола FTP MDTM, который используется для получения штампа даты и времени из файла на FTP-сервере.
        /// </summary>
        DeleteFile,
        /// <summary>
        /// Представляет метод протокола FTP SIZE, который используется для получения размера файла на FTP-сервере.
        /// </summary>
        AppendFile,
        /// <summary>
        /// Представляет метод протокола FTP NLIST, который возвращает краткий список файлов на FTP-сервере.
        /// </summary>
        GetFileSize,
        /// <summary>
        /// Представляет метод протокола FTP LIST, который возвращает подробный список файлов на FTP-сервере.
        /// </summary>
        UploadFileWithUniqueName,
        /// <summary>
        /// Представляет метод протокола FTP MKD, который создает каталог на FTP-сервере.
        /// </summary>
        MakeDirectory,
        /// <summary>
        /// Представляет метод протокола FTP PWD, который отображает имя текущего рабочего каталога.
        /// </summary>
        RemoveDirectory,
        /// <summary>
        /// Представляет метод протокола FTP RMD, который удаляет каталог.
        /// </summary>
        ListDirectoryDetails,
        /// <summary>
        /// Представляет метод протокола FTP RENAME, который переименовывает каталог.
        /// </summary>
        GetDateTimestamp,
        /// <summary>
        /// Представляет метод протокола FTP STOR, который выгружает файл на FTP-сервер.
        /// </summary>
        PrintWorkingDirectory,
        /// <summary>
        /// Представляет метод протокола FTP STOU, который выгружает файл с уникальным именем на FTP-сервер.
        /// </summary>
        Rename,
    }

    public static class FtpRequestTypeExtention
    {
        public static string ToFtpWebRequestMethod(this FtpRequestType ftpRequestType)
        {
            switch (ftpRequestType)
            {
                case FtpRequestType.DownloadFile:
                    return WebRequestMethods.Ftp.DownloadFile;
                case FtpRequestType.ListDirectory:
                    return WebRequestMethods.Ftp.ListDirectory;
                case FtpRequestType.UploadFile:
                    return WebRequestMethods.Ftp.UploadFile;
                case FtpRequestType.DeleteFile:
                    return WebRequestMethods.Ftp.DeleteFile;
                case FtpRequestType.AppendFile:
                    return WebRequestMethods.Ftp.AppendFile;
                case FtpRequestType.GetFileSize:
                    return WebRequestMethods.Ftp.GetFileSize;
                case FtpRequestType.UploadFileWithUniqueName:
                    return WebRequestMethods.Ftp.UploadFileWithUniqueName;
                case FtpRequestType.MakeDirectory:
                    return WebRequestMethods.Ftp.MakeDirectory;
                case FtpRequestType.RemoveDirectory:
                    return WebRequestMethods.Ftp.RemoveDirectory;
                case FtpRequestType.ListDirectoryDetails:
                    return WebRequestMethods.Ftp.ListDirectoryDetails;
                case FtpRequestType.GetDateTimestamp:
                    return WebRequestMethods.Ftp.GetDateTimestamp;
                case FtpRequestType.PrintWorkingDirectory:
                    return WebRequestMethods.Ftp.PrintWorkingDirectory;
                case FtpRequestType.Rename:
                    return WebRequestMethods.Ftp.Rename;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ftpRequestType), "значение не предусмотрено");
            }
        }
    }
}