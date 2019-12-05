﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowCreator.transitions
{
    class FadeTransition : ITransition
    {

        public List<Bitmap> Render(Bitmap start, Bitmap end, int frames)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();
            for (int i = 0; i < frames; i++)
            {
                Bitmap currImg = new Bitmap(start);
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
                    //graphics.FillRectangle(new SolidBrush(Color.FromArgb(, 255, 255, 255)), 0, 0, currImg.Width, currImg.Height);
                    
                }
                bitmaps.Add(currImg);
            }
            return bitmaps;
        }
    }
}