using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator.transitions
{
    public class SlideTransition : Transition
    {
        public override List<Bitmap> Render(Bitmap start, Bitmap end, int frames)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();
            for (int i = frames; i > 0; i--)
            {
                Bitmap currImg = new Bitmap(start);
                using (var graphics = Graphics.FromImage(currImg))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    int xNewImg = (int)(((double)start.Width / (double)frames) * i);
                    graphics.DrawImage(end, new Rectangle(xNewImg, 0, start.Width, start.Height));
                }
                bitmaps.Add(currImg);
            }
            return bitmaps;
        }
    }
}
