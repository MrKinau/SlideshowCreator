using Accord.Video.FFMPEG;
using SlideshowCreator.transitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace SlideshowCreator
{
    public class VideoCreator
    {

        private readonly BackgroundWorker exportWorker = new BackgroundWorker();
        private ExportVideoProgressWindow exportWindow;
        private string fileName;
        private int width;
        private int height;
        private List<TimelinePictureElementControl> timelineElements;

        private int frameRate;
        private int frames;
        private int bitrate;


        /*
         * Bitrate calculation from: https://www.ezs3.com/public/What_bitrate_should_I_use_when_encoding_my_video_How_do_I_optimize_my_video_for_the_web.cfm
         */
        public VideoCreator(string fileName, int width, int height, int bitrate, int fps, List<TimelinePictureElementControl> timelineElements)
        {
            this.fileName = fileName;
            this.width = width;
            this.height = height;
            this.timelineElements = timelineElements;
            this.bitrate = bitrate;
            this.frameRate = fps;

            ///\todo exception, when no content is added (disable create button if no content is in timeline)

            double timeInSeconds = 0;
            
            foreach (TimelinePictureElementControl element in timelineElements)
            {
                timeInSeconds += (element.EndTime - element.StartTime) / 100.0;
                timeInSeconds += element.Transition == null ? 0 : (element.Transition.ExecutionTime / 1000.0);
            }

            frames = (int)Math.Floor(timeInSeconds) * frameRate;
        }

        public void CreateVideo()
        {
            exportWindow = new ExportVideoProgressWindow(this);
            exportWindow.progressBar.Maximum = frames;
            exportWorker.WorkerSupportsCancellation = true;
            exportWorker.WorkerReportsProgress = true;
            exportWorker.DoWork += exportWorker_work;
            exportWorker.ProgressChanged += exportWorker_progressChanged;
            exportWorker.RunWorkerCompleted += exportWorker_completed;
            exportWorker.RunWorkerAsync();
            exportWindow.ShowDialog();
        }

        public void stopWorker()
        {
            if (exportWorker == null)
                return;
            if (!exportWorker.IsBusy)
                return;
            if (exportWorker.CancellationPending)
                return;
            exportWorker.CancelAsync();
        }

        private void exportWorker_work(object sender, DoWorkEventArgs e)
        {
            using (VideoFileWriter writer = new VideoFileWriter())
            {
                writer.Open(fileName, width, height, frameRate, VideoCodec.MPEG4, bitrate);

                int currFrame = 1;
                int counter = 0;
                foreach (TimelinePictureElementControl element in timelineElements)
                {
                    //Current element image
                    Bitmap bitmap;
                    if (element.Thumbnail != null)
                        bitmap = ImageConverter.ScaleImage(new Bitmap(element.Thumbnail), width, height, true);
                    else
                        bitmap = ImageConverter.CreateBlankImage(width, height);

                    //Draw transition to slideshow
                    if (element.Transition != null)
                    {
                        Bitmap previous;
                        if (timelineElements.IndexOf(element) != 0)
                            previous = ImageConverter.ScaleImage(new Bitmap(timelineElements[timelineElements.IndexOf(element) - 1].Thumbnail), width, height, true);
                        else
                            previous = ImageConverter.CreateBlankImage(bitmap.Width, bitmap.Height);

                        List<Bitmap> transitionImages = element.Transition.Render(previous, bitmap, (int)Math.Floor((element.Transition.ExecutionTime / 1000.0) * frameRate));
                        foreach (Bitmap transImg in transitionImages)
                        {
                            if (exportWorker.CancellationPending == true)
                            {
                                e.Cancel = true;
                                return;
                            }
                            writer.WriteVideoFrame(transImg);
                            (sender as BackgroundWorker).ReportProgress(currFrame);
                            currFrame++;
                        }
                    }

                    //Draw image to slideshow
                    int framesThisSlide = (int)((element.EndTime - element.StartTime) * frameRate) / 100;

                    for (int i = 0; i < framesThisSlide; i++)
                    {
                        if (exportWorker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }
                        writer.WriteVideoFrame(bitmap);
                        (sender as BackgroundWorker).ReportProgress(currFrame);
                        currFrame++;
                    }
                    counter++;
                }
                writer.Close();
            }
        }

        private void exportWorker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                exportWindow.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                File.Delete(fileName);
            }
            exportWindow.Close();
        }

        private void exportWorker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            double percent = ((double)e.ProgressPercentage / (double)exportWindow.progressBar.Maximum);
            exportWindow.progressBar.Value = e.ProgressPercentage;
            exportWindow.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            exportWindow.taskbarItemInfo.ProgressValue = percent;
            exportWindow.progressText.Text = "Export in progress (" + Math.Round(percent * 100) + "% completed)";
        }
    }
}
