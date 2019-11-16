using Accord.Video.FFMPEG;
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
        private ExportVideoProgress exportWindow;
        private string fileName;
        private int width;
        private int height;
        private List<TimelineElementControl> timelineElements;

        private int frameRate = 30;
        private int frames;
        private int bitrate;


        /*
         * Bitrate calculation from: https://www.ezs3.com/public/What_bitrate_should_I_use_when_encoding_my_video_How_do_I_optimize_my_video_for_the_web.cfm
         */
        public VideoCreator(string fileName, int width, int height, List<TimelineElementControl> timelineElements)
        {
            this.fileName = fileName;
            this.width = width;
            this.height = height;
            this.timelineElements = timelineElements;

            int timeInSeconds = (int)(timelineElements[timelineElements.Count - 1].EndTime / 100);
            frames = timeInSeconds * frameRate;
            bitrate = (int)Math.Round(height * width * 5 * 0.07) * timeInSeconds;
        }

        public void CreateVideo()
        {
            exportWindow = new ExportVideoProgress(this);
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
                foreach (TimelineElementControl element in timelineElements)
                {
                    Bitmap bitmap = ImageConverter.ScaleImage(new Bitmap(element.Thumbnail), width, height, true);
                    int framesThisSlide = (int)((element.EndTime - element.StartTime) * frameRate) / 100;

                    for (int i = 0; i < framesThisSlide; i++)
                    {
                        try
                        {
                            if (exportWorker.CancellationPending == true)
                            {
                                e.Cancel = true;
                                return;
                            }
                            Console.WriteLine(currFrame + "/" + frames);
                            writer.WriteVideoFrame(bitmap);
                            (sender as BackgroundWorker).ReportProgress(currFrame);
                            currFrame++;
                        }
                        catch (ArgumentException ex) { }
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
