using System.IO;
using WorkingTools.FileSystem;

namespace WorkingTools.Extensions
{
    public static class MemoryStreamExtension
    {
        public static void Save(this MemoryStream ms, string path)
        {
            using (var fileStream = FileWt.Create(path))
            {
                ms.WriteTo(fileStream);
                fileStream.Close();
            }
        }
    }
}
