using Accord.Video.FFMPEG;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SlideshowCreator.transitions
{
    /// <summary>
    /// A transition which slightly reduces the alpha of the first image. An alpha blending transition
    /// </summary>
    class FadeTransition : Transition
    {

        /// <summary>
        /// The function which is called by the creating video stream to add the transition images to it
        /// </summary>
        /// <param name="start">Starting bitmap</param>
        /// <param name="end">Ending bitmap</param>
        /// <param name="frames">The framecount for the transition</param>
        /// <param name="writer">The VideoWriterStream</param>
        /// <param name="progressReporter">A background worker which should get progress reports</param>
        /// <param name="currFrame">The currently rendered frame as int (for the progress worker calculation)</param>
        /// <returns>the current frame count</returns>
        public override int Render(Bitmap start, Bitmap end, int frames, VideoFileWriter writer, BackgroundWorker progressReporter, int currFrame)
        {
            for (int i = 0; i < frames; i++)
            {
                using (Bitmap currImg = new Bitmap(start))
                {
                    using (var graphics = Graphics.FromImage(currImg))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;


                        ColorMatrix opacityMatrix = new ColorMatrix();
                        opacityMatrix.Matrix33 = (1.0F / (float)frames) * i;
                        ImageAttributes imgAttributes = new ImageAttributes();
                        imgAttributes.SetColorMatrix(opacityMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        graphics.DrawImage(end, new Rectangle(0, 0, start.Width, start.Height), 0, 0, start.Width, start.Height, GraphicsUnit.Pixel, imgAttributes);
                    }
                    writer.WriteVideoFrame(currImg);
                    progressReporter.ReportProgress(currFrame);
                    currFrame++;
                }
            }
            return currFrame;
        }
    }
}
