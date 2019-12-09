using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator.transitions
{
    class FadeTransition : Transition
    {

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
