using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SlideshowCreator
{
    public class ImageConverter
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
        public static Bitmap ResizeImage(System.Drawing.Image image, int imgWidth, int imgHeight, int canvasWidth, int canvasHeight, bool highQuality)
        {
            var destRect = new Rectangle((canvasWidth - imgWidth) / 2, (canvasHeight - imgHeight) / 2, imgWidth, imgHeight);
            var destImage = new Bitmap(canvasWidth, canvasHeight);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                if (highQuality)
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                }
                else
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.Low;
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                }

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ScaleImage(System.Drawing.Image sourceImage, int maxWidth, int maxHeight, bool highQuality)
        {
            double scaleFactor;
            if (sourceImage.Width > sourceImage.Height)
                scaleFactor = (double)sourceImage.Width / (double)maxWidth;
            else
                scaleFactor = (double)sourceImage.Height / (double)maxHeight;

            double width = (double)sourceImage.Width / scaleFactor;
            double height = (double)sourceImage.Height / scaleFactor;
            return ResizeImage(sourceImage, (int)Math.Floor(width), (int)Math.Floor(height), maxWidth, maxHeight, highQuality);
        }

        public static BitmapImage ScaleToBitmapImage(Uri source, int width, int height)
        {
            Bitmap bitmap = ScaleImage(new Bitmap(source.LocalPath), width, height, false);

            return ToBitmapImage(bitmap, source.LocalPath);
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap, string origSource)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
