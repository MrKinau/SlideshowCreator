using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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

        /// <summary>
        /// Resize the image to fit into a max-width and max-height rectangle without stretching it
        /// </summary>
        /// <param name="sourceImage">The image to resize</param>
        /// <param name="maxWidth">The max width</param>
        /// <param name="maxHeight">The max height</param>
        /// <param name="highQuality">use high quality or render fast</param>
        /// <returns>the resized image as Bitmap</returns>
        public static Bitmap ScaleImage(System.Drawing.Image sourceImage, int maxWidth, int maxHeight, bool highQuality)
        {
            double scaleFactor;
            if (sourceImage.Width > sourceImage.Height)
                scaleFactor = (double)sourceImage.Width / (double)maxWidth;
            else
                scaleFactor = (double)sourceImage.Height / (double)maxHeight;

            double width = (double)sourceImage.Width / scaleFactor;
            double height = (double)sourceImage.Height / scaleFactor;

            if (width > maxWidth)
                scaleFactor = (double)sourceImage.Width / (double)maxWidth;
            else if (height > maxHeight)
                scaleFactor = (double)sourceImage.Height / (double)maxHeight;

            width = (double)sourceImage.Width / scaleFactor;
            height = (double)sourceImage.Height / scaleFactor;

            return ResizeImage(sourceImage, (int)Math.Floor(width), (int)Math.Floor(height), maxWidth, maxHeight, highQuality);
        }

        /// <summary>
        /// Resizes an image from URI to fit into a max-width and max-height rectangle without stretching it
        /// </summary>
        /// <param name="source">source of the image</param>
        /// <param name="width">The max width</param>
        /// <param name="height">The max height</param>
        /// <returns>the resized image as BitmapImage</returns>
        public static BitmapImage ScaleToBitmapImage(Uri source, int width, int height)
        {
            Bitmap bitmap = ScaleImage(new Bitmap(source.LocalPath), width, height, false);

            return ToBitmapImage(bitmap);
        }

        /// <summary>
        /// Converts a Bitmap to a BitmapImage
        /// </summary>
        /// <param name="bitmap">input bitmap</param>
        /// <returns>a BitmapImage of the input</returns>
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
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

        /// <summary>
        /// Creates a black/blank image
        /// </summary>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        /// <returns>the resultant all-black bitmap</returns>
        public static Bitmap CreateBlankImage(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.Low;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, width, height);
                }
            }
            return bitmap;
        }
    }
}
