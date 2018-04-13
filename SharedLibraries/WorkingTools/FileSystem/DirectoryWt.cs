using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorkingTools.FileSystem
{
    //смысл класса быть более устойчивым к исключениям возникающим в System.IO.Directory
    public static class DirectoryWt
    {
        public static string[] GetDirectories(string path, bool recursive)
        {
            if (!recursive) return GetDirectories(path);

            var items = new List<string>();

            var root = GetDirectories(path);
            items.AddRange(root);
            foreach (var directory in root)
            {
                var child = GetDirectories(directory, true);
                items.AddRange(child);
            }

            return items.ToArray();
        }

        public static string[] GetDirectories(string path, string searchPattern = null)
        {
            if (!Directory.Exists(path))
                return new string[] { };


            if (string.IsNullOrWhiteSpace(searchPattern)) return Directory.GetDirectories(path);
            else return Directory.GetDirectories(path, searchPattern);
        }

        public static string[] GetFiles(string path, bool recursive = false)
        { return GetFiles(path, null, recursive); }

        public static IEnumerable<string> GetFiles(string path, params string[] searchPatterns)
        { return searchPatterns.SelectMany(searchPattern => GetFiles(path, searchPattern)); }

        public static string[] GetFiles(string path, string searchPattern, bool recursive = false)
        {
            if (!Directory.Exists(path))
                return new string[] { };

            if (recursive)
            {
                var files = new List<string>();
                var dirs = Directory.GetDirectories(path);

                foreach (var dir in dirs)
                    files.AddRange(GetFiles(dir, searchPattern, true));

                files.AddRange(!string.IsNullOrWhiteSpace(searchPattern)
                    ? Directory.GetFiles(path, searchPattern)
                    : Directory.GetFiles(path));

                return files.ToArray();
            }

            return searchPattern != null ? Directory.GetFiles(path, searchPattern) : Directory.GetFiles(path);
        }

        public static DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Проверяет каталог на наличие вложенных элементов
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fast">true - метод вернет true если каталог не содержит ни одного элемента; 
        /// false - метод вернет true если каталог не содержит ни одного файла</param>
        /// <returns>true если каталог пуст</returns>
        public static bool IsEmpty(string path, bool fast = true)
        {
            if (!Directory.Exists(path)) return true;

            if (fast)
            {
                if (Directory.EnumerateFileSystemEntries(path).Any())
                    return false;

                return true;
            }
            else
            {
                if (Directory.EnumerateFiles(path).Any()) return false;

                var existsNotEmptyFolder = Directory.EnumerateDirectories(path).Any(p => !IsEmpty(p));
                if (existsNotEmptyFolder) return false;

                return true;
            }
        }

        /// <summary>
        /// Удалить директорию
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        /// <returns>true если удалось удалить директорию</returns>
        public static bool Delete(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                if (!recursive)
                {
                    if (IsEmpty(path))
                    {
                        Directory.Delete(path);
                        return true;
                    }
                }
                else
                {
                    Directory.Delete(path, recursive);
                    return true;
                }
            }

            return false;
        }

        public static void Delete(string path)
        {
            
            Delete(path, false);
        }

        public static IEnumerable<String> EnumerateDirectories(string path)
        {
            

            if (!Directory.Exists(path))
                return Enumerable.Empty<String>();

            return Directory.EnumerateDirectories(path);
        }

        public static void Clear(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var directory in Directory.GetDirectories(path))
                    Directory.Delete(directory, true);

                foreach (var file in Directory.GetFiles(path))
                    File.Delete(file);
            }
        }

        public static bool Exists(string path)
        {
            return Directory.Exists(path);
        }
    }

    public static class PathWt
    {
        public static string Combine(params string[] values)
        {
            if (values == null || values.Length <= 0) return null;
            return Path.Combine(values.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray());
        }
    }

}