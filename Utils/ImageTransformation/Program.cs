using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SL.ConsoleWriter;
using WorkingTools.FileSystem;
using WorkingTools.Img;

namespace ImageTransformation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!DirectoryWt.Exists("files"))
                DirectoryWt.CreateDirectory("files");

            const string preview = "preview_150x150";
            // удаляем старые кадрированные изображения
            foreach (var directory in DirectoryWt.GetDirectories("files", true))
            {
                var directoryName = Path.GetFileName(directory);
                if (directoryName == preview)
                {
                    Report.WriteLine(directory, ConsoleColor.Yellow);
                    DirectoryWt.Delete(directory);
                }
            }

            foreach (var filePath in DirectoryWt.GetFiles("files", true))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var directory = Path.GetDirectoryName(filePath) ?? "";
                var directoryName = Path.GetFileName(directory);

                // пропускаем папки с кадрированными изображениями
                if (directoryName == preview)
                    continue;

                // полиучить оригинал
                using (var image = Image.FromFile(filePath))
                // кадрирование изображения
                using (var img = ImgTransformation.Resize(image, 150, 150))
                {
                    DirectoryWt.CreateDirectory(Path.Combine(directory, preview));

                    var imgPath = Path.Combine(directory, preview, fileName + ".png");
                    FileWt.Delete(imgPath);
                    img.Save(imgPath, ImageFormat.Png);
                    Report.WriteLine(imgPath, ConsoleColor.Blue);

                    var fileExt = Path.GetExtension(filePath);
                    // если формат картинки не png
                    if (fileExt != ".png")
                    {   
                        // сохранить картинку в формате png
                        image.Save(Path.Combine(directory, fileName + ".png"), ImageFormat.Png);
                        // удалить оригинал
                        image.Dispose();
                        FileWt.Delete(filePath);
                        Report.WriteLine(filePath, ConsoleColor.Yellow);
                    }
                }
            }

            Report.WriteLine();
            Report.WriteLine("готово");
            Console.ReadKey();
        }
    }
}
