using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WorkingTools.Img
{
    public static class ImgTransformation
    {
        public static Image Resize(Image image, int width, int height) => Resize((image as Bitmap) ?? new Bitmap(image), new Size(width, height));
        public static Bitmap Resize(Bitmap bitmap, int width, int height) => Resize(bitmap, new Size(width, height));
        public static Bitmap Resize(Bitmap bitmap, Size target)
        {
            double ratio;

            var trimHeight = false;
            var trimWidth = false;

            var trgBitmap = new Bitmap(target.Width, target.Height);

            if ((bitmap.Width / Convert.ToDouble(target.Width)) > (bitmap.Height / Convert.ToDouble(target.Height)))
            {
                ratio = Convert.ToDouble(bitmap.Width) / Convert.ToDouble(target.Width);
                trimHeight = true;
            }
            else
            {
                ratio = Convert.ToDouble(bitmap.Height) / Convert.ToDouble(target.Height);
                trimWidth = true;
            }
            var thumbHeight = Math.Ceiling(bitmap.Height / ratio);
            var thumbWidth = Math.Ceiling(bitmap.Width / ratio);

            var thumbSize = new Size((int)thumbWidth, (int)thumbHeight);
            if (trimHeight)
                trgBitmap = new Bitmap(target.Width, thumbSize.Height);

            if (trimWidth)
                trgBitmap = new Bitmap(thumbSize.Width, target.Height);

            using (var grp = Graphics.FromImage(trgBitmap))
            {
                grp.SmoothingMode = SmoothingMode.AntiAlias;

                grp.InterpolationMode = InterpolationMode.HighQualityBicubic;

                grp.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var rct = new Rectangle(0, 0, thumbSize.Width, thumbSize.Height);

                grp.DrawImage(bitmap, rct, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
            }

            return trgBitmap;
        }

        public static Image Miniature(Image image, int width, int height) => Miniature(new Bitmap(image), new Size(width, height));
        public static Bitmap Miniature(Bitmap bitmap, int width, int height) => Miniature(bitmap, new Size(width, height));
        public static Bitmap Miniature(Bitmap bitmap, Size size)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
            Size bitmapSize;
            lock (bitmap) bitmapSize = bitmap.Size;

            var trgPrpSize = GetSizeByProportion(bitmapSize, size);

            var pointOffset = CalcPointOffset(bitmapSize, trgPrpSize);
            var rectangle = new Rectangle(pointOffset, trgPrpSize);

            var trgBitmap = new Bitmap(rectangle.Width, rectangle.Height);
            lock (bitmap)
            {
                using (var graphics = Graphics.FromImage(trgBitmap))
                    graphics.DrawImage(bitmap, new Rectangle(Point.Empty, trgBitmap.Size), rectangle, GraphicsUnit.Pixel);
            }

            trgBitmap = Resize(trgBitmap, size);

            return trgBitmap;
        }




        private static Size GetSizeByProportion(Size source, Size target)
        {
            double srcWidth = source.Width;
            double srcHeight = source.Height;

            double trgHeight = target.Height;
            double trgWidth = target.Width;

            if (trgHeight / srcHeight > trgWidth / srcWidth)
                return new Size((int)(srcHeight / trgHeight * trgWidth), (int)srcHeight);
            else
                return new Size((int)srcWidth, (int)(srcWidth / trgWidth * trgHeight));
        }

        private static Point CalcPointOffset(Size source, Size target)
        {
            var pointOffset = new Point();
            if (source.Width > target.Width)
                pointOffset.X = (source.Width - target.Width) / 2;

            if (source.Height > target.Height)
                pointOffset.Y = (source.Height - target.Height) / 2;

            return pointOffset;
        }
    }
}
