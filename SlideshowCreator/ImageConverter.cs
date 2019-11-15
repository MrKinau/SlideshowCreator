using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SlideshowCreator
{
    class ImageConverter
    {

        /*
         * src: https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
         */
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(System.Drawing.Image image, int imgWidth, int imgHeight, int canvasWidth, int canvasHeight)
        {
            var destRect = new Rectangle((canvasWidth - imgWidth) / 2, (canvasHeight - imgHeight) / 2, imgWidth, imgHeight);
            var destImage = new Bitmap(canvasWidth, canvasHeight);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ScaleImage(System.Drawing.Image sourceImage, int maxWidth, int maxHeight)
        {
            double scaleFactor;
            Console.WriteLine(sourceImage.Width);
            if (sourceImage.Width > sourceImage.Height)
                scaleFactor = (double)sourceImage.Width / (double)maxWidth;
            else
                scaleFactor = (double)sourceImage.Height / (double)maxHeight;

            double width = (double)sourceImage.Width / scaleFactor;
            double height = (double)sourceImage.Height / scaleFactor;
            return ResizeImage(sourceImage, (int)Math.Floor(width), (int)Math.Floor(height), maxWidth, maxHeight);
        }

        public static BitmapImage ScaleToBitmapImage(Uri source, int width, int height)
        {
            BitmapImage result = new BitmapImage();

            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.UriSource = source;
            if (width > height)
                result.DecodePixelWidth = width;
            else
                result.DecodePixelHeight = height;
            result.EndInit();

            return result;
        }
    }
}
