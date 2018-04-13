using System;
using System.Drawing;
using System.IO;

namespace WorkingTools.Img
{
    public static class ImgExtension
    {
        public static string ToBase64(this Image image)
        {
            using (var m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}