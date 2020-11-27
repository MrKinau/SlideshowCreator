using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Shell;

namespace SlideshowCreator
{
    /// <summary>
    /// Utility class for creating videos and audios
    /// </summary>
    public class VideoCreator
    {

        private readonly BackgroundWorker _exportWorker = new BackgroundWorker();
        private ExportVideoProgressWindow _exportWindow;
        private string _fileName;
        private int _width;
        private int _height;
        private List<TimelinePictureElementControl> _timelineElements;
        private List<TimelineMusicElementControl> _timelineMusicElements;

        private int _frameRate;
        private int _frames;
        private int _bitrate;

        /*
         * Bitrate calculation from: https://www.ezs3.com/public/What_bitrate_should_I_use_when_encoding_my_video_How_do_I_optimize_my_video_for_the_web.cfm
         */
        /// <summary>
        /// Creating new Video Creator
        /// </summary>
        /// <param name="fileName">path of the resultant video file</param>
        /// <param name="width">video resolution width</param>
        /// <param name="height">video resolution height</param>
        /// <param name="bitrate">video bitrate</param>
        /// <param name="fps">video fps</param>
        /// <param name="timelineElements">list of all timeline elements</param>
        /// <param name="timelineMusicElements">list of all music elements</param>
        public VideoCreator(string fileName, int width, int height, int bitrate, int fps, List<TimelinePictureElementControl> timelineElements, List<TimelineMusicElementControl> timelineMusicElements)
        {
            this._fileName = fileName;
            this._width = width;
            this._height = height;
            this._timelineElements = timelineElements;
            this._timelineMusicElements = timelineMusicElements;
            this._bitrate = bitrate;
            this._frameRate = fps;

            double timeInSeconds = 0;
            
            foreach (TimelinePictureElementControl element in timelineElements)
            {
                timeInSeconds += (element.EndTime - element.StartTime) / 100.0;
                timeInSeconds += element.Transition == null ? 0 : (element.Transition.ExecutionTime / 1000.0);
            }

            _frames = (int)Math.Floor(timeInSeconds) * _frameRate;
        }

        /// <summary>
        /// Start video export and show export dialog
        /// </summary>
        public void CreateVideo()
        {
            _exportWindow = new ExportVideoProgressWindow(this);
            _exportWindow.Progress.Maximum = _frames;
            _exportWorker.WorkerSupportsCancellation = true;
            _exportWorker.WorkerReportsProgress = true;
            _exportWorker.DoWork += exportWorker_work;
            _exportWorker.ProgressChanged += exportWorker_progressChanged;
            _exportWorker.RunWorkerCompleted += exportWorker_completed;
            _exportWorker.RunWorkerAsync();
            _exportWindow.ShowDialog();
        }

        /// <summary>
        /// Shows an error box and resets to use the video only without audio
        /// </summary>
        private void musicError()
        {
            Console.WriteLine("Error creating slideshow");
            MessageBox.Show("Audio could not be found!", "Audio not found", MessageBoxButton.OK, MessageBoxImage.Warning);
            File.Copy("output.mp4", _fileName, true);
            File.Delete("output.mp4");
        }

        //https://stackoverflow.com/a/16072577
        //https://superuser.com/a/590210
        /// <summary>
        /// Adds audio to the generated video
        /// </summary>
        public void AddAudio()
        {
            //Convert all audios to mp3, because mp3 is needed for concat
            if (_timelineMusicElements.Count != 0)
            {
                if (!File.Exists(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\ffmpeg.exe"))
                {
                    MessageBox.Show("You need to download ffmpeg.exe and put it into the root application directory to append audio to the slideshow.", "Audio not appended", MessageBoxButton.OK, MessageBoxImage.Warning);
                    File.Copy("output.mp4", _fileName, true);
                    File.Delete("output.mp4");
                    return;
                }
                foreach(TimelineMusicElementControl track in _timelineMusicElements)
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.FileName = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) +
                                                @"\ffmpeg.exe";

                        process.StartInfo.Arguments = "-y -i \"" + track.MusicPath + "\" \"" + track.MusicPath + ".mp3\"";

                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();
                        string output = process.StandardError.ReadToEnd();
                        Console.WriteLine(output);

                        if (output.Contains("No such file or directory"))
                        {
                            musicError();
                            return;
                        }
                    }
                }

                //Create one audio file out of all audio files
                using (Process process = new Process())
                {
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) +
                                            @"\ffmpeg.exe";

                    string allMusic = "";
                    foreach (TimelineMusicElementControl music in _timelineMusicElements)
                    {
                        allMusic += music.MusicPath + ".mp3" + "|";
                    }
                    allMusic = allMusic.Substring(0, allMusic.Length - 1); //remove last "|"

                    process.StartInfo.Arguments = "-y -i \"concat:" + allMusic + "\" -acodec copy input.mp3";

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    Console.WriteLine(output);

                    if (output.Contains("No such file or directory"))
                    {
                        musicError();
                        return;
                    }
                }

                //Add audio file to created slideshow video
                using (Process process = new Process())
                {
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) +
                                            @"\ffmpeg.exe";

                    process.StartInfo.Arguments = "-y -i output.mp4 -i input.mp3 -c copy -map 0:v:0 -map 1:a:0 " + _fileName;

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    Console.WriteLine(output);

                    if (output.Contains("No such file or directory"))
                    {
                        musicError();
                        return;
                    }
                }

                //Cleaning up the mp3 files
                foreach(TimelineMusicElementControl track in _timelineMusicElements)
                {
                    File.Delete(track.MusicPath + ".mp3");
                }
            }
            else
            {
                File.Copy("output.mp4", _fileName, true);
            }
            File.Delete("output.mp4");
        }


        /// <summary>
        /// Cancel video creation
        /// </summary>
        public void stopWorker()
        {
            if (_exportWorker == null)
                return;
            if (!_exportWorker.IsBusy)
                return;
            if (_exportWorker.CancellationPending)
                return;
            _exportWorker.CancelAsync();
        }

        /// <summary>
        /// Working video creation. Adding slides and transitions
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void exportWorker_work(object sender, DoWorkEventArgs e)
        {
            using (VideoFileWriter writer = new VideoFileWriter())
            {
                writer.Open("output.mp4", _width, _height, _frameRate, VideoCodec.MPEG4, _bitrate);

                int currFrame = 1;
                int counter = 0;
                foreach (TimelinePictureElementControl element in _timelineElements)
                {
                    //Current element image
                    Bitmap bitmap;
                    if (element.Thumbnail != null)
                        bitmap = ImageConverter.ScaleImage(new Bitmap(element.Thumbnail), _width, _height, true);
                    else
                        bitmap = ImageConverter.CreateBlankImage(_width, _height);

                    //Draw transition to slideshow
                    if (element.Transition != null)
                    {
                        Bitmap previous;
                        if (_timelineElements.IndexOf(element) != 0 && _timelineElements[_timelineElements.IndexOf(element) - 1].Thumbnail != null)
                            previous = ImageConverter.ScaleImage(new Bitmap(_timelineElements[_timelineElements.IndexOf(element) - 1].Thumbnail), _width, _height, true);
                        else
                            previous = ImageConverter.CreateBlankImage(bitmap.Width, bitmap.Height);

                        currFrame = element.Transition.Render(previous, bitmap, (int)Math.Floor((element.Transition.ExecutionTime / 1000.0) * _frameRate), writer, (sender as BackgroundWorker), currFrame);
                    }

                    //Draw image to slideshow
                    int framesThisSlide = (int)((element.EndTime - element.StartTime) * _frameRate) / 100;

                    for (int i = 0; i < framesThisSlide; i++)
                    {
                        if (_exportWorker.CancellationPending == true)
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

        /// <summary>
        /// Successful end video creation
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void exportWorker_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _exportWindow.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                File.Delete(_fileName);
            }
            else
            {
                AddAudio();
            }
            _exportWindow.Close();
        }

        /// <summary>
        /// Update video export progress
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void exportWorker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            double percent = ((double)e.ProgressPercentage / (double)_exportWindow.Progress.Maximum);
            _exportWindow.Progress.Value = e.ProgressPercentage;
            _exportWindow.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            _exportWindow.taskbarItemInfo.ProgressValue = percent;
            _exportWindow.ProgressText.Text = "Export in progress (" + Math.Round(percent * 100) + "% completed)";
        }
    }
}
