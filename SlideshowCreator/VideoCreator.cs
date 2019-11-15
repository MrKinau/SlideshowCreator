using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SlideshowCreator
{
    class VideoCreator
    {

        /*
         * Bitrate calculation from: https://www.ezs3.com/public/What_bitrate_should_I_use_when_encoding_my_video_How_do_I_optimize_my_video_for_the_web.cfm
         */
        public void CreateVideo(List<TimelineElementControl> timelineElements, string fileName, int width, int height)
        {

            int frameRate = 30;
            int timeInSeconds = (int)(timelineElements[timelineElements.Count - 1].EndTime / 100);
            int frames = timeInSeconds * frameRate;
            int bitrate = (int)Math.Round(height * width * 5 * 0.07) * timeInSeconds;

            VideoFileWriter writer = new VideoFileWriter();

            writer.Open(fileName, width, height, frameRate, VideoCodec.MPEG4, bitrate);

            foreach (TimelineElementControl element in timelineElements)
            {
                BitmapImage img = (BitmapImage) element.DisplayedImage.Source;

                Bitmap bitmap = ImageConverter.ScaleImage(new Bitmap(img.UriSource.LocalPath), width, height);
                int framesThisSlide = (int)((element.EndTime - element.StartTime) * frameRate) / 100;

                Console.WriteLine(framesThisSlide + " <- " + img.UriSource.LocalPath);

                for (int i = 0; i < framesThisSlide; i++)
                {
                    try
                    {
                        writer.WriteVideoFrame(bitmap);
                    } catch(ArgumentException ex) { }
                }
            }
            writer.Close();
        }
    }
}
