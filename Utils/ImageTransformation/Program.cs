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
            const string preview1240X1240 = "preview_1240x1240";
            const string preview1024X1024 = "preview_1024x1024";
            // удаляем старые кадрированные изображения
            foreach (var directory in DirectoryWt.GetDirectories("files", true))
            {
                var directoryName = Path.GetFileName(directory);
                //
                if (directoryName == preview || directoryName == preview1240X1240 || directoryName == preview1024X1024)
                {
                    Report.WriteLine(directory, ConsoleColor.Yellow);
                    DirectoryWt.Delete(directory);
                }
            }

            Parallel.ForEach(DirectoryWt.GetFiles("files", true), filePath =>
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var directory = Path.GetDirectoryName(filePath) ?? "";
                var directoryName = Path.GetFileName(directory);

                // пропускаем папки с кадрированными изображениями
                if (directoryName == preview || directoryName == preview1240X1240 || directoryName == preview1024X1024)
                {
                    // пропустить
                }
                else
                {
                    // полиучить оригинал
                    using (var image = Image.FromFile(filePath))
                        // кадрирование изображения
                    using (var img = ImgTransformation.Resize(image, 150, 150))
                    using (var img1240X1240 = ImgTransformation.Resize(image, 1240, 1240))
                    using (var img1024X1024 = ImgTransformation.Resize(image, 1024, 1024))
                    {
                        ImgSave(img, directory, preview, fileName);
                        ImgSave(img1240X1240, directory, preview1240X1240, fileName);
                        ImgSave(img1024X1024, directory, preview1024X1024, fileName);

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
            });

            Report.WriteLine();
            Report.WriteLine("готово");
            Console.ReadKey();
        }

        public static void ImgSave(Image img, string directory, string preview, string fileName)
        {
            DirectoryWt.CreateDirectory(Path.Combine(directory, preview));

            var imgPath = Path.Combine(directory, preview, fileName + ".png");
            FileWt.Delete(imgPath);
            img.Save(imgPath, ImageFormat.Png);
            Report.WriteLine(imgPath, ConsoleColor.Blue);
        }
    }
}
