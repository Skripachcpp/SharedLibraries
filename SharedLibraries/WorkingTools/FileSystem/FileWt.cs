using System;
using System.IO;
using System.Threading;

namespace WorkingTools.FileSystem
{
    public static class FileWt
    {
        public static DateTimeOffset? GetLastWriteTime(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            if (!File.Exists(filePath)) return null;

            var writeTime = new DateTimeOffset(File.GetLastWriteTimeUtc(filePath));
            return writeTime;
        }


        public static void Delete(string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                return;

            File.Delete(filePath);
        }

        public static Stream Create(params string[] filePath)
        {
            var fullPath = Path.Combine(filePath);

            var dirPath = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            return File.Create(fullPath);
        }

        public static Stream OpenRead(params string[] filePath)
        {
            string fileP = Path.Combine(filePath);

            if (!File.Exists(fileP))
                return Stream.Null;

            return File.OpenRead(fileP);
        }

        public static string ReadAllText(params string[] filePath)
        {
            string fileP = Path.Combine(filePath);

            if (!File.Exists(fileP))
                return null;

            return File.ReadAllText(fileP);
        }

        public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            if (!File.Exists(sourceFileName)) return;

            var destDir = Path.GetDirectoryName(destFileName);
            if (destDir != null && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            File.Copy(sourceFileName, destFileName, overwrite);
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            if (!File.Exists(sourceFileName)) return;

            var destDir = Path.GetDirectoryName(destFileName);
            if (destDir != null && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            File.Move(sourceFileName, destFileName);
        }

        public static void WriteAllText(string path, string contents)
        {
            var destDir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            File.WriteAllText(path, contents);
        }

        public static bool Exists(string file)
        {
            return File.Exists(file);
        }


        private const int RetryTimeout = 250;
        public static FileStream OpenRead(string path, int attempts = 250)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
            }

            if (!File.Exists(path))
            {
            }

            FileStream fileStream = null;
            while (fileStream == null && attempts >= 0)
            {
                try
                {
                    fileStream = File.OpenRead(path);
                }
                catch (Exception)
                {
                    attempts--;
                    Thread.Sleep(RetryTimeout);
                }
            }

            return fileStream;
        }
    }
}