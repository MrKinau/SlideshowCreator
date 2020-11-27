using Accord.Video.FFMPEG;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SlideshowCreator.transitions
{
    /// <summary>
    /// A transition which slides the second picture from right to left over the beginning picture
    /// </summary>
    public class SlideTransition : Transition
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
        public override int Render(Bitmap start, Bitmap end, int frames, VideoFileWriter writer, BackgroundWorker progressWorker, int currFrame)
        {
            for (int i = frames; i > 0; i--)
            {
                using (Bitmap currImg = new Bitmap(start))
                {
                    using (var graphics = Graphics.FromImage(currImg))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        int xNewImg = (int)(((double)start.Width / (double)frames) * i);
                        graphics.DrawImage(end, new Rectangle(xNewImg, 0, start.Width, start.Height));
                    }
                    writer.WriteVideoFrame(currImg);
                    progressWorker.ReportProgress(currFrame);
                    currFrame++;
                }
            }
            return currFrame;
        }
    }
}
