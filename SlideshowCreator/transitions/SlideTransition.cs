﻿using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator.transitions
{
    public class SlideTransition : Transition
{
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
