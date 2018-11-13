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
using WorkingTools.Extensions;
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

            const string targetFormatExtension = ".jpg";
            var targetFormat = ImageFormat.Jpeg;

            const string directoryPreview150X150 = "preview_150x150";
            const string directoryPreview1240X1240 = "preview_1240x1240";
            const string directoryPreview1024X1024 = "preview_1024x1024";
            const string directoryOrigin = "origin";

            // удаляем старые кадрированные изображения
            foreach (var directory in DirectoryWt.GetDirectories("files", true))
            {
                var directoryName = Path.GetFileName(directory);
                //
                if (directoryName == directoryPreview150X150 
                    || directoryName == directoryPreview1240X1240 
                    || directoryName == directoryPreview1024X1024 
                    || directoryName == directoryOrigin)
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
                if (directoryName == directoryPreview150X150 || directoryName == directoryPreview1240X1240 || directoryName == directoryPreview1024X1024)
                {
                    // пропустить
                }
                else
                {
                    // полиучить оригинал
                    using (var image = Image.FromFile(filePath))

                    // кадрирование изображения
                    using (var imgOrigin = ImgTransformation.Resize(image, image.Width, image.Height))
                    using (var img150X150 = ImgTransformation.Resize(image, 150, 150))
                    using (var img1240X1240 = ImgTransformation.Resize(image, 1240, 1240))
                    using (var img1024X1024 = ImgTransformation.Resize(image, 1024, 1024))
                    {
                        ImgSave(imgOrigin, directory, directoryOrigin, fileName + targetFormatExtension, targetFormat);
                        ImgSave(img150X150, directory, directoryPreview150X150, fileName + targetFormatExtension, targetFormat);
                        ImgSave(img1240X1240, directory, directoryPreview1240X1240, fileName + targetFormatExtension, targetFormat);
                        ImgSave(img1024X1024, directory, directoryPreview1024X1024, fileName + targetFormatExtension, targetFormat);

                        var fileExt = Path.GetExtension(filePath);
                        // если формат картинки не png
                        if (fileExt != targetFormatExtension)
                        {
                            // сохранить картинку
                            image.Save(Path.Combine(directory, fileName + targetFormatExtension), targetFormat);
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

        public static void ImgSave(Image img, string directory, string preview, string fileName, ImageFormat imageFormat)
        {
            DirectoryWt.CreateDirectory(Path.Combine(directory, preview));

            var imgPath = Path.Combine(directory, preview, fileName);
            FileWt.Delete(imgPath);
            img.Save(imgPath, imageFormat);
            Report.WriteLine(imgPath, ConsoleColor.Blue);
        }
    }
}
